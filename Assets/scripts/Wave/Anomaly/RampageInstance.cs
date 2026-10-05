using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

public class RampageInstance : AnomalyInstance
{
    private readonly string description;
    private EntityHealth boss;
    private bool applied;

    public RampageInstance(AnomalyData data) : base(data)
    {
        string eff = data.anomalyEffect is StatusEffect se && !string.IsNullOrWhiteSpace(se.effName) ? se.effName : "Enraged";
        description = $"The boss starts the fight in its final phase at reduced Health, and gains {eff}";
    }

    public override string Description => description;

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        boss = null;
        applied = false;
    }

    public override void OnEnemySpawned(GameObject enemy, GameObject prefab, int level)
    {
        if (!isActive || boss != null || enemy == null) return;
        if (enemy.TryGetComponent<EntityHealth>(out var eh)) boss = eh;
    }

    public override void UpdateCheck(float dt)
    {
        if (applied || boss == null || !boss.IsAlive) return;
        applied = true;

        boss.SetHpPct(boss.FinalPhaseHpPct(amd != null ? amd.anomalyValue : 50f));

        if (amd != null && amd.anomalyEffect != null && boss.TryGetComponent<StatusEffectManager>(out var sem))
            sem.ApplyPermanent(amd.anomalyEffect, null);
    }

    public override void ResetForWave()
    {
        boss = null;
        applied = false;
        base.ResetForWave();
    }
}
