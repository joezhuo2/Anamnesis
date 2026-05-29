using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(RectTransform))]
public class Projectile : MonoBehaviour {
    public ProjectileData pd;
    [HideInInspector] public GameObject ownerObj;
    [HideInInspector] public Vector2 dir;
    [HideInInspector] public int pierced;
    private readonly List<GameObject> hit = new();
    private Transform followTarget;
    private Rigidbody2D rb;
    private void Awake()
    {
        if (pd != null) pd = Instantiate(pd);
    }

    private void Start()
    {
        pierced = 0;
        HandleSize();
        HandleDirection();
        rb = GetComponent<Rigidbody2D>();
        HandleMovement(true);

        Destroy(gameObject, pd.lifetime);
    }

    private void FixedUpdate() => HandleMovement(false);
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pierced >= pd.numPierce)
        {
            Destroy(gameObject);
            return;
        }
        if (!pd.canHitSameEntity && hit.Contains(other.gameObject)) return;

        if (other.TryGetComponent<EntityStatManager>(out var statManager) && pd.owner != statManager.s)
            HandleHitEntity(other.gameObject, statManager.s);
    }
    private void HandleHitEntity(GameObject target, EntityStats otherStats)
    {
        if (!target.TryGetComponent<EntityHealth>(out var eh)) return;

        var packet = DamageCalculator.BuildDamagePacket(pd);

        eh.TakeDamage(packet);
        pierced++;
        hit.Add(target);

        if (pierced >= pd.numPierce) Destroy(gameObject);

        if (pd.additionalChance > 0f && pd.additionalAttack != null)
            HandleAdditionalSpawns();

        if (pd.effect == null) return;
        if (ownerObj != target) ApplyEffect(target);
        else if (pd.selfApply) ApplyEffect();
    }

    private void HandleAdditionalSpawns()
    {
        if (Random.value > pd.additionalChance) return;
        if (pd.additionalAttack == null || pd.additionalAttack.projectilePrefab == null) return;
        if (ProjectileSpawner.Instance == null) return;

        ProjectileSpawner spawner = ProjectileSpawner.Instance;
        GameObject prefab = pd.additionalAttack.projectilePrefab;
        GameObject s = ownerObj;
        Vector2? addDir = pd.additionalFollowsMouse ? null : dir;

        spawner.StartCoroutine(spawner.SpawnFromPattern(prefab, s, transform.position, addDir, pd.distFromCenter));
    }
    private void HandleSize()
    {
        float sizeMult = 1f + (pd.owner.aoePct / 100f);
        transform.localScale = Vector2.Max(new Vector2(sizeMult, sizeMult), new Vector2(0, 0));
    }

    private void HandleDirection()
    {
        if (pd.owner.GameObject().CompareTag("Enemy") || dir != Vector2.zero) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(PlayerInputHandler.mousePos);
        mouseWorldPos.z = 0f;

        dir = (mouseWorldPos - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + pd.rotationOffset);
    }

    private void HandleMovement(bool start)
    {
        if (rb == null || pd.speed <= 0) return;

        if (start) rb.linearVelocity = dir.normalized * pd.speed;

        if (pd.followDistance > 0)
        {
            if (followTarget == null || !followTarget.gameObject.activeInHierarchy)
            {
                bool searchForPlayer = pd.owner.GameObject().CompareTag("Enemy");
                followTarget = FindClosestTargetInRange(pd.followDistance, searchForPlayer);
            }

            FollowTarget();
        }
    }
    private void FollowTarget()
    {
        if (followTarget == null) return;

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

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);
        string targetTag = searchForPlayer ? "Player" : "Enemy";

        foreach (Collider2D col in colliders)
        {
            if (!col.CompareTag(targetTag)) continue;

            if (!pd.canHitSameEntity && hit.Contains(col.gameObject)) continue;

            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = col.transform;
            }
        }
        return closest;
    }
    private void ApplyEffect(GameObject target = null)
    {
        if (!(Random.value <= pd.effectChance)) return;

        if (target == null && pd.owner.GameObject().TryGetComponent<StatusEffectManager>(out var ssem))
            ssem.AddEffect(pd.effect, pd.owner.GameObject());
        else if (target.TryGetComponent<StatusEffectManager>(out var tsem))
            tsem.AddEffect(pd.effect, pd.owner.GameObject());
    }
}