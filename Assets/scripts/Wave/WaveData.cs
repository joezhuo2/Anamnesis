using System.Collections.Generic;
using UnityEngine;

public enum WaveType { Normal, Boss, Endless }

[CreateAssetMenu(fileName = "NewWaveSequence", menuName = "Wave System/Wave Sequence")]
public class WaveSequence : ScriptableObject
{
    public Vector2 spawnLocation;
    public WaveSequence nextSequence;
    public string sequenceName;
    [SerializeReference]
    public List<WaveData> waves = new();
}

[System.Serializable]
public class WaveData
{
    public GameObject enemyPrefab;
    public int maxTotalEnemies;
    public int maxCurrentEnemies;
    public int enemyLevel;
    public float minSpawnFrequency;
    public float maxSpawnFrequency;
    public int minRewardChoices;
    public int maxRewardChoices;
    public float rewardQualityBoost;
    public virtual WaveType Type => WaveType.Normal;
}

[System.Serializable]
public class BossWaveData : WaveData
{
    public override WaveType Type => WaveType.Boss;

    public GameObject bossBarPrefab;
    public string bossName;
}

[System.Serializable]
public class EndlessWaveData : WaveData
{
    public override WaveType Type => WaveType.Endless;

    [Header("Endless Scaling Per Minute")]
    [Tooltip("How many extra enemies to add to the active pool per minute")]
    public int spawnRateIncreasePerMinute = 2;

    [Tooltip("How much enemy levels increase per minute")]
    public float levelScalingPerMinute = 0.5f;

    [Tooltip("Time in seconds before the endless wave forces a win condition (0 for infinite)")]
    public float timeLimit = 0f; 
}