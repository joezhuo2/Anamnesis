using System.Collections.Generic;
using UnityEngine;

public enum WaveType { Normal, Boss, Endless }

[CreateAssetMenu(fileName = "ws", menuName = "Data/Wave Sequence")]
public class WaveSequence : ScriptableObject
{
    public Vector2 spawnLocation;
    public WaveSequence nextSequence;
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
    public WaveType Type;
    public GameObject bossBarPrefab;
    public string bossBarName;
}