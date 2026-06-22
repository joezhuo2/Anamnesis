using UnityEngine;
using UnityEngine.Rendering;

public enum RewardRarity { Common, Uncommon, Rare, Epic, Legendary }

[System.Serializable]
public class RarityData
{
    public RewardRarity rarity;
    public string rarityName;
    public Color displayColor;
    public float mult = 1f;
    public float weight;
}

public class BaseReward : ScriptableObject
{
    public Sprite icon;
    public StatBuff baseBuff;
}

public class GeneratedReward
{
    public BaseReward br;
    public RarityData rd;

    public float finalVal => br.baseBuff.value * rd.mult;

    public string GetDisplayName() => $"[{rd.rarityName}] {br.baseBuff}";
    public string GetDescription() => $"+{finalVal} {br.baseBuff} (x{rd.mult})";
}

