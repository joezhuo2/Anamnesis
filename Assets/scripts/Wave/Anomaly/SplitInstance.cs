using UnityEngine;

public class SplitInstance : AnomalyInstance
{
    public const int MinSplits = 2;

    public float splitChance;
    public int levelReduction;
    public int maxSplits;

    private readonly string description;

    public SplitInstance(AnomalyData data) : base(data)
    {
        splitChance = Mathf.Round(data.anomalyValue);
        levelReduction = Mathf.Max(0, Mathf.RoundToInt(data.anomalyMinVal));
        maxSplits = Mathf.Max(MinSplits, Mathf.RoundToInt(data.anomalyMaxVal));

        string count = maxSplits > MinSplits ? $"{MinSplits}-{maxSplits}" : MinSplits.ToString();
        string levels = levelReduction == 1 ? "level" : "levels";
        description = $"Enemies have a {splitChance}% chance to split into {count} copies of themselves on death, each {levelReduction} {levels} lower. Split enemies cannot split again, but all of them must be killed to clear the wave";
    }

    public override string Description => description;

    public override void OnEnemySpawned(GameObject enemy, GameObject prefab, int level)
    {
        if (enemy == null || prefab == null) return;

        AnomalySplitter splitter = enemy.AddComponent<AnomalySplitter>();
        splitter.Setup(prefab, level, splitChance, MinSplits, maxSplits, levelReduction);
    }
}
