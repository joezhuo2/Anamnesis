using CrystalFlux.Core;
using UnityEngine;

public class StatModifierInstance : AnomalyInstance
{
    public StatType selectedStat;
    public float boostAmount;

    private static readonly StatType[] CommonStats = new StatType[]
    {
        StatType.atkPct,
        StatType.hpPct,
        StatType.armorPct,
        StatType.damagePct,
        StatType.moveSpeedPct
    };

    private string description;

    public StatModifierInstance(AnomalyData data) : base(data)
    {
        selectedStat = CommonStats[Random.Range(0, CommonStats.Length)];
        boostAmount = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal));

        description = $"All enemies in the wave gain +{boostAmount}% {GetStatDisplayName(selectedStat)}";
    }

    public override string Description => description;

    private string GetStatDisplayName(StatType type)
    {
        return type switch
        {
            StatType.atkPct => "Attack",
            StatType.hpPct => "Health",
            StatType.armorPct => "Armor",
            StatType.damagePct => "Damage",
            StatType.moveSpeedPct => "Move Speed",
            _ => type.ToString()
        };
    }

    public StatBuff GetBuff() => new(selectedStat, boostAmount);
}
