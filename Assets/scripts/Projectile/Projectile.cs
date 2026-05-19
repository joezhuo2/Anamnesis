using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(RectTransform))]
public class Projectile : MonoBehaviour {
    public ProjectileData pd;
    [HideInInspector] public Vector2 dir;
    [HideInInspector] public int pierced;
    private readonly List<GameObject> hit = new();
    private Transform followTarget;
    private Rigidbody2D rb;
    void Start()
    {
        pierced = 0;
        HandleSize();
        HandleDirection();
        rb = GetComponent<Rigidbody2D>();
        HandleMovement(true);

        Destroy(gameObject, pd.lifetime);
    }

    void Update() => HandleMovement(false);
    void OnTriggerEnter2D(Collider2D other)
    {
        if (pierced > pd.numPierce) return;
        if (other.TryGetComponent<EntityStats>(out var stats))
        {
            if (pd.owner != stats)
            {
                HandleHitEntity(stats);
            }
        }
    }
    private void HandleHitEntity(EntityStats other)
    {
        if (!other.GameObject().TryGetComponent<EntityHealth>(out var eh)) return;

        var packet = DamageCalculator.BuildDamagePacket(pd);

        eh.TakeDamage(packet);
        pierced++;

        if (pd.additionalChance > 0f) HandleAdditionalSpawns();
    }

    private void HandleAdditionalSpawns()
    {

    }
    private void HandleSize()
    {
        float sizeMult = 1f + (pd.owner.aoePct / 100f);
        transform.localScale = Vector2.Max(new Vector2(sizeMult, sizeMult), new Vector2(0, 0));
    }

    private void HandleDirection()
    {
        if (pd.owner.GameObject().CompareTag("Enemy") || dir != Vector2.zero) return;

        // finish this later
    }

    private void HandleMovement(bool start)
    {
        if (rb == null || pd.speed <= 0) return;

        if (start) rb.linearVelocity = dir.normalized * pd.speed;

        if (pd.followDistance > 0)
        {
            bool isEnemy = pd.owner.GameObject().CompareTag("Enemy");
            followTarget = FindClosestTargetInRange(pd.followDistance, isEnemy);

            FollowTarget();
        }
    }
    private void FollowTarget()
    {
        float dist = Vector2.Distance(transform.position, followTarget.position);
        if (dist <= pd.followDistance)
        {
            Vector2 newDir = (followTarget.position - transform.position).normalized;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, newDir * pd.speed, 0.1f);
        }
    }

    private void SetTarget(Transform target) => followTarget = target;

    private Transform FindClosestTargetInRange(float range, bool searchForPlayer)
    {
        Transform closest = null;
        float minDist = range;

        if (searchForPlayer)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject e in enemies)
            {
                if (hit.Contains(e) && !pd.canHitSameEntity) continue;

                float dist = Vector2.Distance(transform.position, e.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = e.transform;
                }
            }
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                if (hit.Contains(p) && !pd.canHitSameEntity) continue;

                float dist = Vector2.Distance(transform.position, p.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = p.transform;
                }
            }
        }

        return closest;
    }

    private void TargetApplyEffect(Collider2D target)
    {
        
    }

    private void SelfApplyEffect()
    {

    }
}