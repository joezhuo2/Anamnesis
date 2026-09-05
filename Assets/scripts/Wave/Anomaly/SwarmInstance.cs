using CrystalFlux.Core;
using UnityEngine;

public class SwarmInstance : AnomalyInstance
{
    public float penaltyAmount;

    private readonly string description;

    public SwarmInstance(AnomalyData data) : base(data)
    {
        penaltyAmount = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal));
        description = $"All enemies in the wave gain -{penaltyAmount}% Health and Damage, but {penaltyAmount}% more of them spawn";
    }

    public override string Description => description;

    public float CountMultiplier => 1f + (penaltyAmount / 50f);

    public override void ApplyEnemyBuffs(IStatProvider esm)
    {
        if (esm == null) return;
        esm.AddStat(new StatBuff(StatType.hpPct, -penaltyAmount));
        esm.AddStat(new StatBuff(StatType.damagePct, -penaltyAmount));
    }
}
