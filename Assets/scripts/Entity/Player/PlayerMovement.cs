using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class PlayerMovement : MonoBehaviour, IKnockbackable
    {
        public GameObject dashCooldownUI;

        private readonly List<KnockbackHandler.AppliedForce> currentForces = new();
        [HideInInspector] public Vector2 moveInput;
        private static readonly int SpeedHash = Animator.StringToHash("speed");
        private Rigidbody2D rb;
        private Animator animator;
        public static readonly float baseAnimSpeed = 1f;
        [HideInInspector] public float lastDashTime;
        private float dashTravelled;
        private Vector2 dashDir;
        [HideInInspector] public static int playerDir = 1;
        private PlayerUpgradeManager pum;
        private IStatProvider esm;
        private RushState rush;
        private StatusEffectManager sem;
        private bool Frozen => sem != null && sem.Frozen;
        private float lastAnimSpd = -1f;
        private static Camera cachedMainCam;
        private static Camera MainCam => cachedMainCam != null ? cachedMainCam : cachedMainCam = Camera.main;
        private bool Dashing => esm.GetStat(StatType.IsDashing) > 0f;
        private float Spd => esm.GetStat(StatType.EffSpd);
        public bool Rushing => rush != null && rush.Active;
        public bool RushBlocksAttacks => rush != null && rush.BlocksAttacks;

        private void Awake() => animator = GetComponent<Animator>();

        private void OnDisable() => rush?.End();

        private void OnCollisionEnter2D(Collision2D c) => rush?.OnCollision(c);

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            esm = GetComponent<IStatProvider>();
            pum = GetComponent<PlayerUpgradeManager>();
            sem = GetComponent<StatusEffectManager>();

            rush = new RushState(gameObject, esm,
                () => { if (pum != null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnRushStart); },
                () => { if (pum != null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnRushEnd); });

            esm.AddStat(new StatBuff(StatType.CanMove, 1f));
            esm.AddStat(new StatBuff(StatType.CanDash, 1f));
            esm.AddStat(new StatBuff(StatType.IsDashing, 0f));

            lastDashTime = -Mathf.Infinity;

            playerDir = transform.localScale.x < 0f ? -1 : 1;

            if (dashCooldownUI != null && dashCooldownUI.TryGetComponent<PlayerDashCooldownUI>(out var pdcui))
                pdcui.Setup(this, esm);
        }

        private void FixedUpdate()
        {
            if (Time.timeScale == 0f) return;

            if (Frozen)
            {
                currentForces.Clear();
                rush.End();
                rb.linearVelocity = Vector2.zero;
                animator.speed = baseAnimSpeed;
                return;
            }

            bool alive = esm.GetStat(StatType.isAlive) > 0f;
            bool canMove = alive && esm.GetStat(StatType.CanMove) > 0f;
            if (!alive) rush.End();
            if (!canMove) rush.OnImmobilized();

            if (!canMove && !rush.Unstoppable)
            {
                rush.Tick(Time.fixedDeltaTime, false);
                rb.linearVelocity = Vector2.zero;
                animator.speed = baseAnimSpeed;
                return;
            }

            Vector2 velocity;
            float faceX;
            if (rush.Active)
            {
                velocity = rush.GetVelocity(moveInput, Time.fixedDeltaTime);
                faceX = rush.Dir.x;
            }
            else if (Dashing)
            {
                velocity = dashDir * (Spd * esm.GetStat(StatType.DashSpdMult));
                faceX = 0f;
            }
            else
            {
                velocity = Vector2.ClampMagnitude(moveInput, 1f) * Spd;
                faceX = moveInput.x;
            }

            rb.linearVelocity = velocity + GetKnockbackVelocity();
            rush.Tick(Time.fixedDeltaTime, true);

            float inputMag = moveInput.magnitude;

            if ((faceX > 0 && transform.localScale.x < 0) || (faceX < 0 && transform.localScale.x > 0))
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                playerDir *= -1;
            }

            if (inputMag > 0.1) animator.speed = Mathf.Max(inputMag * baseAnimSpeed, 0.01f);
            if (inputMag != lastAnimSpd)
            {
                animator.SetFloat(SpeedHash, inputMag);
                lastAnimSpd = inputMag;
            }
        }

        private Vector2 GetKnockbackVelocity() => KnockbackHandler.UpdateForces(currentForces, Time.fixedDeltaTime);

        public void ApplyKnockback(Vector2 d, float f, float t)
        {
            if (Frozen) return;
            if (rush != null && rush.OnKnockback()) return;
            KnockbackHandler.ApplyKnockback(currentForces, d, f, t, esm.GetStat(StatType.kbRes));
        }

        public void TryStartDash()
        {
            if (Dashing || esm.GetStat(StatType.CanDash) <= 0f || Time.timeScale == 0f) return;
            if (Time.time < lastDashTime + esm.GetStat(StatType.EffDashCooldown)) return;
            if (esm.GetStat(StatType.CurrentStamina) < esm.GetStat(StatType.EffDashStaminaCost)) return;

            rush.End();

            if (pum!= null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnStartDash);

            esm.AddStat(new StatBuff(StatType.CurrentStamina, Mathf.RoundToInt(esm.GetStat(StatType.EffDashStaminaCost))), false);
            lastDashTime = Time.time;
            dashTravelled = 0f;

            dashDir = moveInput.magnitude > 0.01f ? moveInput.normalized : new Vector2(playerDir, 0f);
            if (dashDir == Vector2.zero) dashDir = new Vector2(playerDir, 0f);

            esm.AddStat(new StatBuff(StatType.IsDashing, 1f));

            float dashSpeed = Spd * esm.GetStat(StatType.DashSpdMult);
            float dashDuration = dashSpeed > 0 ? esm.GetStat(StatType.EffDashDistance) / dashSpeed : 0.2f;

            if (esm.GetStat(StatType.DashShouldApplyIFrame) > 0f && gameObject.TryGetComponent<IDamageable>(out var eh))
                eh.TriggerIFrames(dashDuration + 0.2f);

            StartCoroutine(DashRoutine(dashSpeed));
        }
        private System.Collections.IEnumerator DashRoutine(float dashSpeed)
        {
            float distance = esm.GetStat(StatType.EffDashDistance);

            if (dashSpeed <= 0f || distance <= 0f)
            {
                EndDash();
                yield break;
            }

            while (dashTravelled < distance)
            {
                dashTravelled += dashSpeed * Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            EndDash();
        }

        public void EndDash()
        {
            esm.AddStat(new StatBuff(StatType.IsDashing, 0f));
            dashTravelled = 0f;

            if (pum!= null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnEndDash);
        }

        public void StartRush(AttackData ad)
        {
            if (rush == null || ad == null || !ad.Rushes || Time.timeScale == 0f) return;

            Vector2 aim = MainCam != null ? (Vector2)MainCam.ScreenToWorldPoint(InputState.mousePos) : rb.position;
            rush.Begin(ad, rb.position, aim, new Vector2(playerDir, 0f));
        }

        public void AdvanceDash(float pctAmt)
        {
            var edc = esm.GetStat(StatType.EffDashCooldown);
            if (edc <= 0f) return;

            float timeElapsed = Time.time - lastDashTime;
            float cooldownRemainingPct = 1f - (timeElapsed / edc);
            float newCooldownRemainingPct = Mathf.Clamp01(cooldownRemainingPct - (pctAmt * 0.01f));
            float newLastTime = Time.time - ((1f - newCooldownRemainingPct) * edc);

            lastDashTime = newLastTime;
        }
    }
}
