using System.Collections;
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
    private List<GameObject> hit;
    private Transform followTarget;
    private Rigidbody2D rb;
    private void Awake()
    {
        hit = new();
    }
    private void Start()
    {
        pierced = 0;
        HandleSize();
        HandleDirection();
        rb = GetComponent<Rigidbody2D>();
        HandleMovement(true);

        if (pd.effect != null && pd.applyCondition == ApplyCondition.OnCast && pd.selfApply)
            ApplyEffect();

        StartCoroutine(DestroyProjectileAfterDelay(pd.lifetime));
    }

    private void FixedUpdate() => HandleMovement(false);
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pierced >= pd.numPierce) return;
        if (hit.Contains(other.gameObject)) return;

        if (other.TryGetComponent<EntityStatManager>(out var statManager) && ownerObj != other.gameObject)
            HandleHitEntity(other.gameObject);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (pierced >= pd.numPierce) return;
        if (hit.Contains(other.gameObject)) return;

        if (other.TryGetComponent<EntityStatManager>(out var statManager) && ownerObj != other.gameObject)
            HandleHitEntity(other.gameObject);
    }
    private void HandleHitEntity(GameObject target)
    {
        if (target == null || ownerObj == null || target == ownerObj) return;
        if (!target.TryGetComponent<EntityHealth>(out var eh)) return;

        bool targetIsPlayer = target?.CompareTag("Player") ?? true;
        bool isPlayer = ownerObj?.CompareTag("Player") ?? false;
        bool targetIsEnemy = target?.CompareTag("Enemy") ?? false;
        bool isEnemy = ownerObj?.CompareTag("Enemy") ?? true;

        if (targetIsPlayer == isPlayer || targetIsEnemy == isEnemy) return;

        DamagePacket packet = DamageCalculator.BuildDamagePacket(pd, ownerObj);

        eh.TakeDamage(packet, pd.bypassIFrames || isPlayer, ownerObj);
        var (hp, stamina, mana) = CalculateStatGains(ownerObj, pd.mainAttack, packet.GetTotalDamage());
        TriggerStatGains(hp, stamina, mana, ownerObj);

        pierced++;
        hit.Add(target);

        if (pd.timeBeforeSameEnemy > 0f) StartCoroutine(RemoveFromHitHistory(target, pd.timeBeforeSameEnemy));

        if (pd.additionalChance > 0f && pd.additionalAttack != null)
            HandleAdditionalSpawns();

        if (pd.effect != null && pd.applyCondition == ApplyCondition.OnHit)
        {
            if (pd.selfApply) ApplyEffect();
            else if (ownerObj != target) ApplyEffect(target);
        }
    }
    private IEnumerator DestroyProjectileAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!pd.addAttackRequiresHit) HandleAdditionalSpawns();

        Destroy(gameObject);
    }

    private void HandleAdditionalSpawns()
    {
        if (Random.value > pd.additionalChance) return;
        if (pd.additionalAttack == null || pd.additionalAttack.projectilePrefab == null) return;
        if (ProjectileSpawner.Instance == null) return;

        ProjectileSpawner spawner = ProjectileSpawner.Instance;

        Vector2? addDir = pd.additionalFollowsMouse ? null : dir;

        spawner.StartCoroutine(spawner.SpawnFromPattern(pd.additionalAttack, ownerObj, transform.position, addDir, pd.additionalAttack.spawnDistance));
    }
    private void HandleSize()
    {
        if (!ownerObj.TryGetComponent<EntityStatManager>(out var esm) && esm.s.aoePct == 0) return;

        float sizeMult = pd.size + (esm.s.aoePct * 0.01f);
        transform.localScale = Vector2.Max(new Vector2(sizeMult, sizeMult), new Vector2(0, 0));
    }
    private void HandleDirection()
    {
        // if (ownerObj.CompareTag("Enemy") || dir != Vector2.zero) return;

        if (ownerObj.CompareTag("Player"))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(PlayerInputHandler.mousePos);
            mouseWorldPos.z = 0f;

            dir = (mouseWorldPos - transform.position).normalized;
        }

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
                bool searchForPlayer = ownerObj.GameObject().CompareTag("Enemy");
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

            if (hit.Contains(col.gameObject)) continue;

            if (col.gameObject.TryGetComponent<EntityStatManager>(out var esm) && !esm.s.isAlive && esm.s.currentHp <= 0) continue;

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

        if (target == null && ownerObj.TryGetComponent<StatusEffectManager>(out var ssem))
            ssem.AddEffect(pd.effect, ownerObj);
        else if (target.TryGetComponent<StatusEffectManager>(out var tsem))
            tsem.AddEffect(pd.effect, ownerObj);
    }
    private System.Collections.IEnumerator RemoveFromHitHistory(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hit != null && target != null) hit.Remove(target);
    }
    public static (float hp, float stamina, float mana) CalculateStatGains(GameObject target, AttackData a, float totalDmg = 0f)
    {
        if (target == null || !target.TryGetComponent<EntityStatManager>(out var esm)) return (0f, 0f, 0f);

        float totalStamina = a.staminaGainOnHit;
        float totalHp = a.healthGainOnHit;
        float totalMana = a.manaGainOnHit;

        if (a.basedOnDmgDealt)
        {
            totalStamina += totalDmg * 0.01f * a.staminaPctGainOnHit;
            totalHp += totalDmg * 0.01f * a.healthPctGainOnHit;
            totalMana += totalDmg * 0.01f * a.manaPctGainOnHit;
        }
        else if (totalDmg > 0f)
        {
            totalStamina += a.staminaPctGainOnHit * 0.01f * esm.s.maxStamina;
            totalHp += a.healthPctGainOnHit * 0.01f * esm.s.maxHp;
            totalMana += a.manaPctGainOnHit * 0.01f * esm.s.maxMana;
        }
        return (totalHp, totalStamina, totalMana);
    }
    private void TriggerStatGains(float hp, float stamina, float mana, GameObject target)
    {
        if (target == null) return;
        if (target.TryGetComponent<PlayerStamina>(out var ps)) ps.ChangeStamina(stamina);
        if (target.TryGetComponent<EntityHealth>(out var eh)) eh.ChangeHealth(hp, 0f);
        if (target.TryGetComponent<PlayerMana>(out var pm)) pm.ChangeMana(mana, 0f);
    }
}