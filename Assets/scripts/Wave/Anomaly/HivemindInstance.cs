using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

public class HivemindInstance : AnomalyInstance
{
    private readonly HashSet<EntityHealth> members = new();
    private readonly List<EntityHealth> drainBuffer = new();
    private readonly System.Func<EntityHealth, int, bool> absorb;
    private float poolCur;
    private float poolMax;
    private float hpSum;
    private int joined;
    private bool depleted;
    private bool pendingDrain;
    private bool draining;

    public HivemindInstance(AnomalyData data) : base(data) => absorb = Absorb;

    public override string Description => "All enemies share one pooled health bar. Damage dealt to any enemy damages the pool, and every enemy falls when it empties";

    public int PoolCur() => Mathf.Max(0, Mathf.CeilToInt(poolCur));

    public int PoolMax() => Mathf.RoundToInt(poolMax);

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        Clear();
        EntityHealth.DamageRedirect = absorb;
    }

    public override void OnEnemySpawned(GameObject enemy, GameObject prefab, int level)
    {
        if (enemy == null || !isActive || depleted) return;
        if (!enemy.TryGetComponent<EntityHealth>(out var eh) || !enemy.TryGetComponent<IStatProvider>(out var esm)) return;
        if (!members.Add(eh)) return;

        hpSum += Mathf.Max(1f, esm.GetStat(StatType.EffMaxHp));
        joined++;

        float newMax = hpSum / joined * Mathf.Max(joined, WaveManager.WaveEnemyTotal);
        poolCur += newMax - poolMax;
        poolMax = newMax;

        eh.SetBarHidden(true);
    }

    private bool Absorb(EntityHealth eh, int dmg)
    {
        if (!isActive || draining || depleted || eh == null || !members.Contains(eh)) return false;

        poolCur -= dmg;
        if (poolCur <= 0f)
        {
            poolCur = 0f;
            depleted = true;
            pendingDrain = true;
        }
        return true;
    }

    public override void UpdateCheck(float dt)
    {
        if (!pendingDrain) return;
        pendingDrain = false;
        Drain();
    }

    private void Drain()
    {
        WaveManager.StopSpawning();

        GameObject p = GameObject.FindWithTag("Player");
        drainBuffer.Clear();
        drainBuffer.AddRange(members);
        members.Clear();

        draining = true;
        for (int i = 0; i < drainBuffer.Count; i++)
        {
            EntityHealth eh = drainBuffer[i];
            if (eh == null || !eh.IsAlive || !eh.TryGetComponent<IStatProvider>(out var esm)) continue;

            float hp = esm.GetStat(StatType.currentHp) + eh.Overhealth + 1f;
            eh.TakeDamage(DamagePacketBuilder.BuildDamagePacket(hp, DamageType.True, false, default, p, true, 1f));
        }
        draining = false;
        drainBuffer.Clear();
    }

    private void Clear()
    {
        members.Clear();
        drainBuffer.Clear();
        poolCur = 0f;
        poolMax = 0f;
        hpSum = 0f;
        joined = 0;
        depleted = false;
        pendingDrain = false;
        draining = false;
    }

    public override void ResetForWave()
    {
        if (EntityHealth.DamageRedirect == absorb) EntityHealth.DamageRedirect = null;

        foreach (EntityHealth eh in members)
            if (eh != null) eh.SetBarHidden(false);

        Clear();
        base.ResetForWave();
    }
}
