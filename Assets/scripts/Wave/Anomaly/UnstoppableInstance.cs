using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

public class UnstoppableInstance : AnomalyInstance
{
    public float vulnAmount;

    private readonly string description;
    private bool applied;

    public UnstoppableInstance(AnomalyData data) : base(data)
    {
        vulnAmount = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal));
        description = $"The boss is immune to knockback, stun, freeze and pull, but takes +{vulnAmount}% more damage from all sources";
    }

    public override string Description => description;

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        applied = false;
    }

    public override void OnEnemySpawned(GameObject enemy, GameObject prefab, int level)
    {
        if (!isActive || applied || enemy == null) return;
        if (!enemy.TryGetComponent<EntityHealth>(out var eh)) return;

        applied = true;
        eh.DamageTakenMult *= 1f + (vulnAmount * 0.01f);
        if (enemy.TryGetComponent<StatusEffectManager>(out var sem)) sem.CcImmune = true;
    }

    public override void ResetForWave()
    {
        applied = false;
        base.ResetForWave();
    }
}
