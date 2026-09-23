using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour, IKnockbackable
    {
        public bool cardinalOnly = true;
        public bool canDeaggro = true;
        public float stoppingDistance = 0;
        public bool flipRotation = false;

        private readonly List<KnockbackHandler.AppliedForce> currentForces = new();
        private static readonly int SpeedHash = Animator.StringToHash("speed");
        private IStatProvider esm;
        private Rigidbody2D rb;
        private Animator a;
        private bool wasMoving = false;
        private readonly float targetCheckInterval = 0.25f;
        private float nextTargetCheckTime = 0f;
        private Transform cTransform;
        private Vector3 cScale;
        private GameObject cachedPlayer;
        public GameObject target;
        private RushState rush;

        private static readonly List<EnemyMovement> active = new();
        public static IReadOnlyList<EnemyMovement> Active => active;
        public bool Rushing => rush != null && rush.Active;

        private void OnEnable() => active.Add(this);
        private void OnDisable()
        {
            active.Remove(this);
            rush?.End();
        }

        private void OnCollisionEnter2D(Collision2D c) => rush?.OnCollision(c);

        private void Start()
        {
            esm = GetComponent<IStatProvider>();
            a = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            rush = new RushState(gameObject, esm);

            esm.AddStat(new StatBuff(StatType.CanMove, 1f));

            cTransform = transform;
            cScale = cTransform.localScale;

            cachedPlayer = GameObject.FindGameObjectWithTag("Player");

            UpdateTargeting();
        }
        private void Update()
        {
            if (Time.timeScale == 0f) return;

            UpdateTargeting();

            Vector2 velocity = GetKnockbackVelocity();

            bool alive = esm.GetStat(StatType.isAlive) > 0f;
            bool canMove = alive && esm.GetStat(StatType.CanMove) > 0f;

            if (!alive) rush.End();
            if (!canMove) rush.OnImmobilized();

            if (rush.Active)
            {
                bool moving = canMove || rush.Unstoppable;
                if (moving)
                {
                    velocity += rush.GetVelocity(GetAimPoint(rush.Ad) - rb.position, Time.deltaTime);
                    Face(rush.Dir.x);
                }
                rush.Tick(Time.deltaTime, moving);
            }
            else if (target != null && canMove)
                velocity += GetMovementVelocity();

            rb.linearVelocity = velocity;
            SetAnimator(velocity != Vector2.zero);
        }
        private Vector2 GetKnockbackVelocity() => KnockbackHandler.UpdateForces(currentForces, Time.deltaTime);

        public void ApplyKnockback(Vector2 d, float f, float t)
        {
            if (rush != null && rush.OnKnockback()) return;
            KnockbackHandler.ApplyKnockback(currentForces, d, f, t, esm.GetStat(StatType.kbRes));
        }

        public void SetTarget(GameObject target) => this.target = target;

        private Vector2 GetMovementVelocity()
        {
            if (target == null) return Vector2.zero;

            Vector2 dist = target.transform.position - cTransform.position;
            float distMag = dist.magnitude;

            if (distMag > 0 && distMag <= stoppingDistance) return Vector2.zero;

            float detectionRange = esm.GetStat(StatType.DetectionRange);
            if (canDeaggro && distMag > detectionRange)
            {
                target = null;
                return Vector2.zero;
            }

            Vector2 dir = dist.normalized;
            if (cardinalOnly)
            {
                dir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ?
                    new Vector2(Mathf.Sign(dir.x), 0) :
                    new Vector2(0, Mathf.Sign(dir.y));
            }

            Face(dir.x);

            return dir * esm.GetStat(StatType.EffSpd);
        }

        private void Face(float x)
        {
            if (x == 0) return;

            float directionSign = Mathf.Sign(x);
            bool shouldMirror = flipRotation ? directionSign > 0 : directionSign < 0;
            float targetScaleX = Mathf.Abs(cScale.x) * (shouldMirror ? -1f : 1f);

            if (!Mathf.Approximately(cTransform.localScale.x, targetScaleX))
            {
                cScale.x = targetScaleX;
                cTransform.localScale = cScale;
            }
        }

        private Vector2 GetFacingDir()
            => new(((cTransform.localScale.x < 0f) == flipRotation) ? 1f : -1f, 0f);

        private Vector2 GetAimPoint(AttackData ad)
        {
            if (target == null) return rb.position;

            Vector2 tp = target.transform.position;
            if (ad == null || !ad.PredictTarget || !target.TryGetComponent<Rigidbody2D>(out var trb)) return tp;

            float spd = RushState.GetSpeed(ad, esm);
            if (spd <= 0f) return tp;

            float maxT = ad.RushType == RushType.Duration ? ad.RushTypeVal : ad.RushTypeVal / spd;
            float t = Mathf.Min(Vector2.Distance(rb.position, tp) / spd, maxT);
            return tp + (trb.linearVelocity * t);
        }

        public void StartRush(AttackData ad)
        {
            if (rush == null || ad == null || !ad.Rushes) return;

            rush.Begin(ad, rb.position, GetAimPoint(ad), GetFacingDir());
        }

        private void SetAnimator(bool moving)
        {
            if (a != null && moving != wasMoving)
            {
                a.SetFloat(SpeedHash, moving ? 1f : 0f);
                wasMoving = moving;
            }
        }
        private void UpdateTargeting()
        {
            if (target != null) return;

            if (Time.time < nextTargetCheckTime) return;
            nextTargetCheckTime = Time.time + targetCheckInterval;

            if (cachedPlayer != null)
            {
                float dist = Vector2.Distance(transform.position, cachedPlayer.transform.position);
                float detectionRange = esm.GetStat(StatType.DetectionRange);
                if (dist <= detectionRange) target = cachedPlayer;
            }
            else
            {
                cachedPlayer = GameObject.FindGameObjectWithTag("Player");
                if (cachedPlayer != null)
                {
                    float dist = Vector2.Distance(transform.position, cachedPlayer.transform.position);
                    float detectionRange = esm.GetStat(StatType.DetectionRange);
                    if (dist <= detectionRange) target = cachedPlayer;
                }
            }
        }
    }
}
