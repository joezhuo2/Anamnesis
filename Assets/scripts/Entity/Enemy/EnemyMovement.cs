using System.Collections.Generic;
using CrystalFlux.Core;
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

        private static readonly List<EnemyMovement> active = new();
        public static IReadOnlyList<EnemyMovement> Active => active;

        private void OnEnable() => active.Add(this);
        private void OnDisable() => active.Remove(this);

        private void Start()
        {
            esm = GetComponent<IStatProvider>();
            a = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();

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

            if (target != null && esm.GetStat(StatType.CanMove) > 0f && esm.GetStat(StatType.isAlive) > 0f)
                velocity += GetMovementVelocity();

            rb.linearVelocity = velocity;
            SetAnimator(velocity != Vector2.zero);
        }
        private Vector2 GetKnockbackVelocity() => KnockbackHandler.UpdateForces(currentForces, Time.deltaTime);

        public void ApplyKnockback(Vector2 d, float f, float t)
            => KnockbackHandler.ApplyKnockback(currentForces, d, f, t, esm.GetStat(StatType.kbRes));

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

            if (dir.x != 0)
            {
                float directionSign = Mathf.Sign(dir.x);
                bool shouldMirror = flipRotation ? directionSign > 0 : directionSign < 0;
                float targetScaleX = Mathf.Abs(cScale.x) * (shouldMirror ? -1f : 1f);

                if (!Mathf.Approximately(cTransform.localScale.x, targetScaleX))
                {
                    cScale.x = targetScaleX;
                    cTransform.localScale = cScale;
                }
            }

            return dir * esm.GetStat(StatType.EffSpd);
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
