using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MilestoneReward
{
    [Header("Identity")]
    public string rewardName;
    public Color displayColor;
    [Header("Base Stat Buffs (variance applied at runtime)")]
    public List<StatBuff> baseStatBuffs;
    [Header("Settings")]
    [Tooltip("Weight for random selection. All milestone rewards should have equal weight.")]
    public float weight;
    [Tooltip("Variance applied to each stat value (±percentage). 0.15 = ±15%")]
    [Range(0f, 0.5f)] public float variance;
}