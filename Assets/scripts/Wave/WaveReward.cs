using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RarityData
{
    public string rarityName;
    public Color displayColor;
    public float mult = 1f;
    public float weight;
}

[System.Serializable]
public class BaseReward
{
    public Sprite icon;
    public StatBuff baseBuff;
    public float weight;
}

[System.Serializable]
public class GeneratedReward
{
    public BaseReward br;
    public RarityData rd;

    public float finalVal => br.baseBuff.value * rd.mult * mult;
    public float mult = 1f;
    public string GetDescription() => $"{(finalVal >= 0 ? "+" : "")}{finalVal:F2} {br.baseBuff.ToString()} ({rd.rarityName} x{rd.mult})";
}

[System.Serializable]
public class AttackReward
{
    public AttackData newAttack;
    public AttackType type;
    public Sprite icon;
    public string attackName;
    [TextArea] public string desc;
}

[System.Serializable]
public class PlayerUpgradeReward
{
    public PlayerUpgrade upgrade;
    public Sprite icon;
    public string upgradeName;
    [TextArea] public string desc;
}

[System.Serializable]
public class MilestoneRewardData
{
    public string rewardName;
    public List<StatBuff> generatedBuffs = new();

    public string GetDescription()
    {
        var lines = new List<string>();
        foreach (var buff in generatedBuffs)
        {
            string sign = buff.value >= 0 ? "+" : "";
            lines.Add($"{sign}{buff.value:F0} {buff.ToString()}");
        }
        return string.Join("\n", lines);
    }
}