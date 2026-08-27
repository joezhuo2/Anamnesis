using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(RectTransform))]
public class Projectile : MonoBehaviour, IOnHitEffect
{
    public ProjectileData pd;

    [HideInInspector] public GameObject ownerObj;
    [HideInInspector] public Vector2 dir;
    [HideInInspector] public int pierced;
    private float effSpd;
    private List<GameObject> hit;
    private ProjectileDamageSnapshot damageSnapshot;
    private Transform followTarget;
    private Transform orbitTarget;
    private float orbitDirectionSign;
    private float effectiveOrbitRadius;
    private float orbitAngleOffset;
    private bool orbitInitialized;
    private Rigidbody2D rb;
    private bool boomerangActive;
    private bool boomerangReturning;
    private float boomerangDecel;
    private float boomerangSpeed;
    private bool orbitCancelled;
    private bool canTriggerAdd;
    private Rigidbody2D sourceRb;

    private void Awake()
    {
        hit = new();
        canTriggerAdd = true;
    }

    private void OnDestroy()
    {
        if (pd != null && pd.orbitRadius > 0 && pd.orbitSelf && ownerObj != null &&
            ownerObj.TryGetComponent<IOrbitRegister>(out var ior))
            ior.UnregisterOrbitingProjectile(this);
    }

    private void Start()
    {
        effSpd = ownerObj != null ?
            ownerObj.TryGetComponent<IStatProvider>(out var esm) ?
            pd.speed * (1f + (esm.GetStat(StatType.ProjSpd) * 0.01f)) :
            pd.speed : pd.speed;

        pierced = 0;
        damageSnapshot = ProjectileSnapshot.CaptureSnapshot(pd, ownerObj);
        HandleSize();
        HandleDirection();
        rb = GetComponent<Rigidbody2D>();
        InitBoomerang();
        HandleMovement(true);

        if (pd.effects != null && pd.effects.Count > 0)
        {
            foreach (var ed in pd.effects)
            {
                if (ed.effect != null && ed.applyCondition == ApplyCondition.OnCast && ed.selfApply)
                    ApplyEffect(null, ed);
            }
        }

        if (pd.orbitRadius > 0 && pd.orbitSelf && ownerObj != null &&
            ownerObj.TryGetComponent<IOrbitRegister>(out var iog))
        {
            iog.RegisterOrbitingProjectile(this);
        }

        StartCoroutine(DestroyProjectileAfterDelay(pd.lifetime));
    }

    private void FixedUpdate() => HandleMovement(false);
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pierced >= pd.numPierce) return;
        if (hit.Contains(other.gameObject)) return;

        if (other.TryGetComponent<IStatProvider>(out var statManager) && ownerObj != other.gameObject)
            HandleHitEntity(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (pierced >= pd.numPierce) return;
        if (hit.Contains(other.gameObject)) return;

        if (other.TryGetComponent<IStatProvider>(out var statManager) && ownerObj != other.gameObject)
            HandleHitEntity(other.gameObject);
    }

    private void HandleHitEntity(GameObject target)
    {
        if (target == null || ownerObj == null || target == ownerObj) return;
        if (!target.TryGetComponent<IDamageable>(out var eh)) return;

        var tid = target.TryGetComponent<ITeamMember>(out var itm) ? itm.TeamID : 0;
        var sid = ownerObj.TryGetComponent<ITeamMember>(out var sitm) ? sitm.TeamID : 0;

        if (sid == tid) return;

        DamagePacket dp = DamagePacket.BuildDamagePacket(pd, damageSnapshot, true, ownerObj, false, 1f);

        eh.TakeDamage(dp);

        if (pd.kbForce > 0f && (eh as Component).TryGetComponent<Rigidbody2D>(out var rb2d))
        {
            Vector2 kbDir = (rb2d.transform.position - transform.position).normalized;

            float kbf = ownerObj.TryGetComponent<IStatProvider>(out var esm) ?
                pd.kbForce * (1f + (esm.GetStat(StatType.kbPct) * 0.01f)) : pd.kbForce;

            if (target.TryGetComponent<IKnockbackable>(out var kb))
                kb.ApplyKnockback(kbDir, kbf, pd.knockbackTime);
            else if (rb2d.bodyType == RigidbodyType2D.Dynamic)
                rb2d.AddForce(kbDir * kbf, ForceMode2D.Impulse);
        }

        var (hp, stamina, mana) = CalculateStatGains(ownerObj, pd.mainAttack, dp.GetTotalDamage());
        TriggerStatGains(hp, stamina, mana, ownerObj);

        pierced++;
        hit.Add(target);

        OnHit(ownerObj, target, transform.position);

        if (pd.timeBeforeSameEnemy > 0f) StartCoroutine(RemoveFromHitHistory(target, pd.timeBeforeSameEnemy));

        if (pd.additionalChance > 0f && pd.additionalAttack != null && Random.value <= pd.additionalChance)
            HandleAdditionalSpawns();

        canTriggerAdd = false;

        if (pd.effects != null && pd.effects.Count > 0)
        {
            foreach (var ed in pd.effects)
            {
                if (ed.effect != null && ed.applyCondition == ApplyCondition.OnHit)
                {
                    if (ed.selfApply) ApplyEffect(null, ed);
                    else if (ownerObj != target) ApplyEffect(target, ed);
                }
            }
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
        if (!canTriggerAdd) return;
        if (pd.additionalAttack == null || pd.additionalAttack.projectilePrefab == null) return;
        if (ProjectileSpawner.Instance == null) return;

        ProjectileSpawner spawner = ProjectileSpawner.Instance;

        Vector2? addDir = pd.additionalFollowsMouse ? null : dir;

        spawner.StartCoroutine(spawner.SpawnFromPattern(pd.additionalAttack, ownerObj, transform.position, addDir, pd.additionalAttack.spawnDistance));
    }

    private void HandleSize()
    {
        if (!ownerObj.TryGetComponent<IStatProvider>(out var esm) && esm.GetStat(StatType.aoePct) == 0) return;

        float sizeMult = pd.size + (esm.GetStat(StatType.aoePct) * 0.01f);
        transform.localScale = Vector2.Max(new Vector2(sizeMult, sizeMult), new Vector2(0, 0));
    }

    private void HandleDirection()
    {
        if (ownerObj == null || pd == null) return;

        if (ownerObj.TryGetComponent<ITeamMember>(out var itm) && itm.TeamID == 1 && dir == Vector2.zero)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(PlayerInputHandler.mousePos);
            mouseWorldPos.z = 0f;

            dir = (mouseWorldPos - transform.position).normalized;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector2 trueAngle = new(Mathf.Cos(pd.angleOverride * Mathf.Deg2Rad), Mathf.Sin(pd.angleOverride * Mathf.Deg2Rad));
        float finalAngle = pd.randomDir ? Random.Range(0f, 360f) : pd.useTrueAngle ? Mathf.Atan2(trueAngle.y, trueAngle.x) * Mathf.Rad2Deg : angle + pd.rotationOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);
    }

    private void InitBoomerang()
    {
        if (pd.maxBoomerangDist > 0f)
        {
            boomerangActive = true;
            boomerangReturning = false;
            boomerangSpeed = effSpd;
            boomerangDecel = (effSpd * effSpd) / (2f * pd.maxBoomerangDist);
        }
    }

    private void HandleMovement(bool start)
    {
        if (rb == null || ownerObj == null) return;

        if (pd.followSource)
        {
            HandleFollowSourceMovement();
            return;
        }

        if (effSpd <= 0) return;

        if (pd.orbitRadius > 0 && !orbitCancelled)
        {
            HandleOrbitMovement();
            return;
        }

        if (start) rb.linearVelocity = dir.normalized * effSpd;

        if (pd.followDistance > 0)
        {
            if (followTarget == null || !followTarget.gameObject.activeInHierarchy)
            {
            bool searchForPlayer = ownerObj.TryGetComponent<ITeamMember>(out var itm) && itm.TeamID == 0;
                followTarget = FindClosestTargetInRange(pd.followDistance, searchForPlayer);
            }

            if (followTarget != null)
            {
                boomerangActive = false;
                FollowTarget();
                return;
            }
        }

        if (boomerangActive) UpdateBoomerang();
    }

    private void HandleFollowSourceMovement()
    {
        if (sourceRb == null)
        {
            if (ownerObj == null) return;
            sourceRb = ownerObj.GetComponent<Rigidbody2D>();
            if (sourceRb == null) return;
        }

        rb.linearVelocity = sourceRb.linearVelocity;
    }

    private void UpdateBoomerang()
    {
        float dt = Time.fixedDeltaTime;

        if (!boomerangReturning)
        {
            boomerangSpeed -= boomerangDecel * dt;

            if (boomerangSpeed <= 0f)
            {
                boomerangSpeed = 0f;
                boomerangReturning = true;
            }

            rb.linearVelocity = dir.normalized * boomerangSpeed;
        }
        else
        {
            boomerangSpeed += boomerangDecel * dt;
            boomerangSpeed = Mathf.Min(boomerangSpeed, effSpd);

            rb.linearVelocity = -dir.normalized * boomerangSpeed;
        }
    }

    private void FollowTarget()
    {
        if (followTarget == null) return;

        float dist = Vector2.Distance(transform.position, followTarget.position);
        if (dist <= pd.followDistance)
        {
            Vector2 newDir = (followTarget.position - transform.position).normalized;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, newDir * effSpd, 0.1f);
        }
    }

    private void HandleOrbitMovement()
    {
        if (orbitTarget == null || !orbitTarget.gameObject.activeInHierarchy)
        {
            if (pd != null && pd.orbitSelf && ownerObj != null) orbitTarget = ownerObj.transform;
            else orbitTarget = FindClosestEnemyInDirection();
            orbitDirectionSign = 0f;
            orbitInitialized = false;
        }

        if (orbitTarget == null) return;

        Vector2 center = orbitTarget.position;
        Vector2 offset = (Vector2)transform.position - center;
        float dist = offset.magnitude;

        if (dist < 0.01f)
        {
            rb.linearVelocity = dir.normalized * effSpd;
            return;
        }

        if (!orbitInitialized)
        {
            effectiveOrbitRadius = pd.orbitRadius + Random.Range(0f, pd.randOrbRadOffset);
            orbitDirectionSign = pd.rotateClockwise ? -1f : 1f;

            if (dist < effectiveOrbitRadius * 0.5f && dir != Vector2.zero)
                orbitAngleOffset = Mathf.Atan2(dir.y, dir.x);
            else
                orbitAngleOffset = Mathf.Atan2(offset.y, offset.x);

            orbitInitialized = true;
        }

        float currentAngle = Mathf.Atan2(offset.y, offset.x);
        float targetAngle = orbitAngleOffset + (orbitDirectionSign * effSpd * Time.fixedDeltaTime / effectiveOrbitRadius);

        orbitAngleOffset = targetAngle;

        Vector2 desiredPos = center + (new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle)) * effectiveOrbitRadius);
        Vector2 toDesired = desiredPos - (Vector2)transform.position;

        Vector2 tangent = Vector2.Perpendicular(desiredPos - center).normalized;
        Vector2 orbitalVelocity = orbitDirectionSign * effSpd * tangent;

        float radiusError = Vector2.Distance(transform.position, center) - effectiveOrbitRadius;
        Vector2 radialCorrection = -5f * radiusError * (desiredPos - center).normalized;

        rb.linearVelocity = orbitalVelocity + radialCorrection + (toDesired * 5f);
    }

    private Transform FindClosestEnemyInDirection()
    {
        Transform closest = null;
        float closestDist = float.MaxValue;

        float searchRadius = effSpd * pd.lifetime;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        foreach (Collider2D col in colliders)
        {
            if (!col.gameObject.TryGetComponent<ITeamMember>(out var itm) || itm.TeamID != 0) continue;
            if (hit.Contains(col.gameObject)) continue;
            if (col.gameObject == ownerObj) continue;

            Vector2 toEnemy = (col.transform.position - transform.position).normalized;
            float dot = Vector2.Dot(dir.normalized, toEnemy);
            if (dot <= 0) continue;

            if (col.gameObject.TryGetComponent<IStatProvider>(out var esm) && esm.GetStat(StatType.isAlive) <= 0f && esm.GetStat(StatType.currentHp) <= 0) continue;

            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = col.transform;
            }
        }
        return closest;
    }

    private Transform FindClosestTargetInRange(float range, bool searchForPlayer)
    {
        Transform closest = null;
        float minDist = range;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);
        int targetTeam = searchForPlayer ? 1 : 0;

        foreach (Collider2D col in colliders)
        {
            if (!col.gameObject.TryGetComponent<ITeamMember>(out var itm) || itm.TeamID != targetTeam) continue;

            if (hit.Contains(col.gameObject)) continue;

            if (col.gameObject.TryGetComponent<IStatProvider>(out var esm) && esm.GetStat(StatType.isAlive) <= 0f && esm.GetStat(StatType.currentHp) <= 0) continue;

            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = col.transform;
            }
        }
        return closest;
    }

    public void Launch(Vector2 direction)
    {
        orbitCancelled = true;
        orbitTarget = null;
        dir = direction.normalized;
        if (rb != null) rb.linearVelocity = dir * effSpd;
    }

    public void Explode()
    {
        if (pd.additionalAttack != null && pd.additionalAttack.projectilePrefab != null && ProjectileSpawner.Instance != null)
        {
            ProjectileSpawner spawner = ProjectileSpawner.Instance;
            Vector2? addDir = pd.additionalFollowsMouse ? null : dir;
            spawner.StartCoroutine(spawner.SpawnFromPattern(pd.additionalAttack, ownerObj, transform.position, addDir, pd.additionalAttack.spawnDistance));
        }
        Destroy(gameObject);
    }

    private void ApplyEffect(GameObject target, EffectData ed)
    {
        if (ed.effect == null) return;

        if (target == null) target = ownerObj;

        if (target.TryGetComponent<IStatusEffectReceiver>(out var sem))
        {
            if (ed.chance <= 0f) return;

            if (Random.value <= ed.chance)
                sem.Apply(ed.effect, ownerObj);
        }
    }

    private System.Collections.IEnumerator RemoveFromHitHistory(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hit != null && target != null) hit.Remove(target);
        canTriggerAdd = true;
    }

    public static (float hp, float stamina, float mana) CalculateStatGains(GameObject target, AttackData a, float totalDmg = 0f)
    {
        if (target == null || !target.TryGetComponent<IStatProvider>(out var esm)) return (0f, 0f, 0f);

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
            totalStamina += a.staminaPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxStamina);
            totalHp += a.healthPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxHp);
            totalMana += a.manaPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxMana);
        }
        return (totalHp, totalStamina, totalMana);
    }

    private void TriggerStatGains(float hp, float stamina, float mana, GameObject target)
    {
        if (target == null) return;
        if (target.TryGetComponent<IResourcePool>(out var rp))
        {
            rp.TryGain(ResourceType.Stamina, stamina);
            rp.TryGain(ResourceType.Mana, mana);
        }

        if (target.TryGetComponent<IDamageable>(out var eh))
        {
            var dp = DamagePacket.BuildDamagePacket(hp, DamageType.Heal, false, Color.green, target, true, 1f);
            eh.TakeDamage(dp);
        }
    }

    public void OnHit(GameObject projectileOwner, GameObject target, Vector3 hitPosition)
    {
        if (projectileOwner.TryGetComponent<PlayerUpgradeManager>(out var pum))
            pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnProjectileHit, hitPosition);

        if (pd.mainAttack != null && pd.mainAttack.summonChance > 0f && pd.mainAttack.summonCondition == SummonCondition.OnHit && Random.value <= pd.mainAttack.summonChance)
        {
            if (ownerObj.TryGetComponent<EntitySummonHandler>(out var summonHandler))
                summonHandler.TrySummon(out _, target.transform.position);
        }
    }
}
