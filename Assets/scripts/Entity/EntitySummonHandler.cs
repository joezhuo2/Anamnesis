using System.Collections.Generic;
using UnityEngine;

public class EntitySummonHandler : MonoBehaviour
{
    [Header("Summon Settings")]
    [Tooltip("Prefab to summon. Should have an Entity component (Player/Enemy) with its own handlers.")]
    public GameObject summonPrefab;
    [Tooltip("Maximum simultaneous summons alive at once. 0 = unlimited.")]
    public int maxSummons = 3;
    [Tooltip("Lifetime in seconds. 0 = permanent (dies naturally or when the summoner dies).")]
    public float lifetime = 0f;
    [Tooltip("Distance from summoner at which summons appear.")]
    public float spawnOffset = 1.5f;
    [Tooltip("Whether to destroy all active summons when this entity is destroyed.")]
    public bool cleanupOnDestroy = true;

    [Header("On-Summon-Death Rewards")]
    [Tooltip("Stat buffs applied to the summoner when a summoned entity dies.")]
    public List<StatBuff> onDeathBuffs = new();
    [Tooltip("Status effects applied to the summoner when a summoned entity dies.")]
    public List<StatusEffect> onDeathEffects = new();

    [Header("Per-Summon Buffs")]
    [Tooltip("Stat buffs granted to the summoner for each active summon. Stack multiplicatively per summon alive.")]
    public List<StatBuff> perSummonBuffs = new();

    private readonly List<GameObject> activeSummons = new();

    private void OnDestroy()
    {
        if (cleanupOnDestroy)
            CleanupAllSummons();
    }
    public GameObject Summon() => SummonAtPosition((Vector2)transform.position + (Random.insideUnitCircle.normalized * spawnOffset));
    public GameObject SummonAtPosition(Vector2 position) =>  SummonAtPosition(position, Quaternion.identity);
    public GameObject SummonAtPosition(Vector2 position, Quaternion rotation)
    {
        if (summonPrefab == null) return null;

        CleanupNullSummons();

        if (maxSummons > 0 && activeSummons.Count >= maxSummons)
        {
            GameObject oldest = activeSummons[0];
            activeSummons.RemoveAt(0);
            if (oldest != null)
            {
                ApplyPerSummonBuffs(false);
                Destroy(oldest);
            }
        }

        GameObject summon = Instantiate(summonPrefab, position, rotation);

        InstantiateRuntimeScriptableObjects(summon);

        if (summon.TryGetComponent<EntityHealth>(out var summonHealth))
            summonHealth.OnDeath += OnSummonDeath;

        activeSummons.Add(summon);

        ApplyPerSummonBuffs(true);

        if (lifetime > 0f) Destroy(summon, lifetime);

        return summon;
    }

    public GameObject SummonWithTarget(GameObject target)
        => SummonWithTarget(target, (Vector2)transform.position + Random.insideUnitCircle.normalized * spawnOffset);

    public bool TrySummon(out GameObject result, Vector2 position)
    {
        result = null;
        if (summonPrefab == null) return false;

        CleanupNullSummons();
        if (maxSummons > 0 && activeSummons.Count >= maxSummons) return false;

        result = SummonAtPosition(position);
        return result != null;
    }

    private void ApplyPerSummonBuffs(bool adding)
    {
        if (perSummonBuffs.Count == 0) return;
        if (!TryGetComponent<EntityStatManager>(out var esm)) return;

        for (int i = 0; i < perSummonBuffs.Count; i++)
            esm.AddStat(perSummonBuffs[i], adding);
    }

    private void OnSummonDeath(GameObject summon)
    {
        activeSummons.Remove(summon);

        if (summon != null && summon.TryGetComponent<EntityHealth>(out var health))
            health.OnDeath -= OnSummonDeath;

        ApplyPerSummonBuffs(false);

        if (onDeathBuffs.Count > 0 && TryGetComponent<EntityStatManager>(out var esm))
        {
            for (int i = 0; i < onDeathBuffs.Count; i++)
                esm.AddStat(onDeathBuffs[i]);
        }

        if (onDeathEffects.Count > 0 && TryGetComponent<StatusEffectManager>(out var sem))
        {
            for (int i = 0; i < onDeathEffects.Count; i++)
            {
                if (onDeathEffects[i] != null)
                    sem.AddEffect(onDeathEffects[i], gameObject);
            }
        }
    }

    public GameObject SummonWithTarget(GameObject target, Vector2 position)
    {
        GameObject summon = SummonAtPosition(position);
        if (summon != null && target != null && summon.TryGetComponent<EnemyMovement>(out var em))
        em.SetTarget(target);
        return summon;
    }

    private static void InstantiateRuntimeScriptableObjects(GameObject obj)
    {
        if (obj.TryGetComponent<EnemyAttackHandler>(out var eah))
        {
            for (int i = 0; i < eah.attacks.Count; i++)
            {
                if (eah.attacks[i] != null)
                {
                    eah.attacks[i] = Object.Instantiate(eah.attacks[i]);
                    eah.attacks[i].InitializeRuntimeCopy();
                }
            }
        }

        if (obj.TryGetComponent<PlayerAttackHandler>(out var pah))
        {
            for (int i = 0; i < pah.attacks.Count; i++)
            {
                if (pah.attacks[i] != null)
                {
                    AttackData runtime = Object.Instantiate(pah.attacks[i]);
                    runtime.type = pah.attacks[i].type;
                    runtime.InitializeRuntimeCopy();
                    pah.attacks[i] = runtime;
                }
            }
        }

        if (obj.TryGetComponent<PlayerUpgradeManager>(out var pum))
        {
            for (int i = 0; i < pum.activeUpgrades.Count; i++)
            {
                if (pum.activeUpgrades[i] != null)
                    pum.activeUpgrades[i] = Object.Instantiate(pum.activeUpgrades[i]);
            }
        }
    }
    public void CleanupAllSummons()
    {
        for (int i = activeSummons.Count - 1; i >= 0; i--)
        {
            if (activeSummons[i] != null)
            {
                if (activeSummons[i].TryGetComponent<EntityHealth>(out var health))
                    health.OnDeath -= OnSummonDeath;
                ApplyPerSummonBuffs(false);
                Destroy(activeSummons[i]);
            }
        }
        activeSummons.Clear();
    }

    private void CleanupNullSummons()
    {
        int before = activeSummons.Count;
        activeSummons.RemoveAll(s => s == null);
        int removed = before - activeSummons.Count;
        for (int i = 0; i < removed; i++)
            ApplyPerSummonBuffs(false);
    }
    public int ActiveSummonCount()
    {
        CleanupNullSummons();
        return activeSummons.Count;
    }
}