using UnityEngine;

public enum RewardRarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }

[System.Serializable]
public class RarityData
{
    public RewardRarity rarity;
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

    public float finalVal => br.baseBuff.value * rd.mult;
    public string GetDescription() => $"+{finalVal} {br.baseBuff.type.ToString()} (x{rd.mult})";
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