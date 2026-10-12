using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

public class LastStandInstance : AnomalyInstance
{
    public int count;
    public float sizePct;
    public float reductionPct;
    public float damagePct;
    public float moveSpeedPct;

    private readonly List<EntityHealth> spawned = new();
    private bool empowered;

    public LastStandInstance(AnomalyData data) : base(data)
    {
        sizePct = RollPct(data.lastStandMinSize, data.lastStandMaxSize);
        reductionPct = RollPct(data.lastStandMinReduction, data.lastStandMaxReduction);
        damagePct = RollPct(data.lastStandMinDamage, data.lastStandMaxDamage);
        moveSpeedPct = RollPct(data.lastStandMinMoveSpeed, data.lastStandMaxMoveSpeed);
        RollCount();
    }

    public override string Description
    {
        get
        {
            string who = count == 1 ? "The final enemy" : $"The final {count} enemies";
            return $"{who} of the wave are marked. Once every other enemy falls, they gain +{sizePct}% Size, +{reductionPct}% Damage Reduction, +{damagePct}% Damage and +{moveSpeedPct}% Move Speed";
        }
    }

    private static float RollPct(float min, float max) => Mathf.Round(Random.Range(min, Mathf.Max(min, max)));

    private void RollCount()
    {
        if (amd == null) { count = 1; return; }
        count = Mathf.Max(1, Mathf.RoundToInt(Random.Range((float)amd.lastStandMinCount, Mathf.Max(amd.lastStandMinCount, amd.lastStandMaxCount))));
    }

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        spawned.Clear();
        empowered = false;
    }

    public override void OnEnemySpawned(GameObject enemy, GameObject prefab, int level)
    {
        if (!isActive || empowered || enemy == null) return;
        if (enemy.TryGetComponent<EntityHealth>(out var eh)) spawned.Add(eh);
    }

    // The marked enemies are always the last `count` entries of the spawn order, so ambush and split spawns
    // that join mid-wave simply shift the window. Nothing is committed until spawning is finished.
    public override void UpdateCheck(float dt)
    {
        if (empowered || spawned.Count == 0 || WaveManager.SpawnsPending) return;

        int first = Mathf.Max(0, spawned.Count - count);
        int markedAlive = 0;
        for (int i = first; i < spawned.Count; i++)
            if (Alive(spawned[i])) markedAlive++;

        if (WaveManager.LivingEnemyCount() > markedAlive) return;

        empowered = true;
        if (markedAlive == 0) return;

        for (int i = first; i < spawned.Count; i++)
            if (Alive(spawned[i])) Empower(spawned[i]);

        IAnnouncer.Current?.SetTitleForDuration("Last Stand", 0.6f, 0.2f, 0.2f);
    }

    private void Empower(EntityHealth eh)
    {
        if (eh.TryGetComponent<IStatProvider>(out var esm))
        {
            esm.AddStat(new StatBuff(StatType.damageRes, reductionPct));
            esm.AddStat(new StatBuff(StatType.damagePct, damagePct));
            esm.AddStat(new StatBuff(StatType.moveSpeedPct, moveSpeedPct));
        }

        float mult = 1f + (sizePct * 0.01f);
        if (eh.TryGetComponent<EnemyMovement>(out var em)) em.ScaleSize(mult);
        else eh.transform.localScale *= mult;
    }

    private static bool Alive(EntityHealth eh) => eh != null && eh.IsAlive;

    public override void ResetForWave()
    {
        spawned.Clear();
        empowered = false;
        RollCount();
        base.ResetForWave();
    }
}
