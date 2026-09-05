using CrystalFlux.Core;
using UnityEngine;

public class DuelInstance : AnomalyInstance
{
    public float boostAmount;

    private readonly string description;

    public DuelInstance(AnomalyData data) : base(data)
    {
        boostAmount = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal));
        description = $"Only a single enemy spawns this wave, and it gains +{boostAmount}% Attack and Health";
    }

    public override string Description => description;

    public override void ApplyEnemyBuffs(IStatProvider esm)
    {
        if (esm == null) return;
        esm.AddStat(new StatBuff(StatType.hpPct, boostAmount));
        esm.AddStat(new StatBuff(StatType.atkPct, boostAmount));
    }
}
