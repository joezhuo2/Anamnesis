using CrystalFlux.Core;
using UnityEngine;

public class StampedeInstance : AnomalyInstance
{
    public float penaltyAmount;

    private readonly string description;

    public StampedeInstance(AnomalyData data) : base(data)
    {
        penaltyAmount = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal));
        description = $"Every enemy in the wave spawns at once, but with -{penaltyAmount}% Health";
    }

    public override string Description => description;

    public float SpawnRadius => amd != null ? amd.anomalyValue : 0f;

    public override void ApplyEnemyBuffs(IStatProvider esm)
    {
        if (esm == null) return;
        esm.AddStat(new StatBuff(StatType.hpPct, -penaltyAmount));
    }
}
