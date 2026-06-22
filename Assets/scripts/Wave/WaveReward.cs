using UnityEngine;
using UnityEngine.Rendering;

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
}

[System.Serializable]
public class GeneratedReward
{
    public BaseReward br;
    public RarityData rd;

    public float finalVal => br.baseBuff.value * rd.mult;

    public string GetDisplayName() => $"[{rd.rarityName}] {br.baseBuff}";
    public string GetDescription() => $"+{finalVal} {br.baseBuff} (x{rd.mult})";
}

