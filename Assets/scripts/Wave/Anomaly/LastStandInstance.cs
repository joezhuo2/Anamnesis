using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

public class LastStandInstance : AnomalyInstance
{
    public float countPct;
    public float sizePct;
    public float reductionPct;
    public float damagePct;
    public float moveSpeedPct;

    private readonly List<EntityHealth> spawned = new();
    private bool empowered;

    public LastStandInstance(AnomalyData data) : base(data) => Roll();

    public override string Description =>
        $"When only the final {countPct}% of the wave's enemies remain, they gain +{sizePct}% Size, +{reductionPct}% Damage Reduction, +{damagePct}% Damage and +{moveSpeedPct}% Move Speed";

    private float RollPct()
    {
        if (amd == null) return 0f;
        return Mathf.Round(Random.Range(amd.anomalyMinVal, Mathf.Max(amd.anomalyMinVal, amd.anomalyMaxVal)));
    }

    private void Roll()
    {
        countPct = RollPct();
        sizePct = RollPct();
        reductionPct = RollPct();
        damagePct = RollPct();
        moveSpeedPct = RollPct();
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

    // Empowers whoever is still alive once spawning is finished and no more than countPct% of the wave's enemies
    // remain. Which enemies those are is not decided up front, so the order they die in cannot dodge it.
    public override void UpdateCheck(float dt)
    {
        if (empowered || spawned.Count == 0 || WaveManager.SpawnsPending) return;

        int count = Mathf.Clamp(Mathf.RoundToInt(spawned.Count * countPct * 0.01f), 1, spawned.Count);
        int living = WaveManager.LivingEnemyCount();
        if (living == 0 || living > count) return;

        empowered = true;

        for (int i = 0; i < spawned.Count; i++)
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
        Roll();
        base.ResetForWave();
    }
}
