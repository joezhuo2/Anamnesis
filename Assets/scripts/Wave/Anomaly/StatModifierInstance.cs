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

    public StatModifierInstance(AnomalyData data) : base(data)
    {
        selectedStat = CommonStats[Random.Range(0, CommonStats.Length)];
        boostAmount = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal));

        UpdateDescription();
    }

    private void UpdateDescription()
    {
        if (amd == null) return;

        string statName = GetStatDisplayName(selectedStat);
        amd.desc = $"All enemies in the wave gain +{boostAmount}% {statName}";
    }

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
