using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlimitedWaveManager : WaveManager
{
    [Header("Wave Scaling")]
    [Tooltip("Center point enemies spawn around.")]
    public Vector2 spawnLocation;
    [Tooltip("Max enemies alive at once.")]
    public int maxCurrentEnemies = 10;
    [Tooltip("Base max total enemies for wave 1. Increases by 1-2 randomly every wave.")]
    public int baseMaxTotalEnemies = 20;
    [Tooltip("Base enemy level for wave 1. Increases by 1 every wave.")]
    public int baseEnemyLevel = 1;
    [Tooltip("Base min spawn frequency (seconds between spawns).")]
    public float minSpawnFrequency = 1f;
    [Tooltip("Base max spawn frequency (seconds between spawns).")]
    public float maxSpawnFrequency = 3f;
    [Tooltip("Spawn frequency reduction per wave (spawns get faster).")]
    public float spawnSpeedIncreasePerWave = 0.05f;
    [Tooltip("Min reward choices offered after a wave.")]
    public int minRewardChoices = 2;
    [Tooltip("Max reward choices offered after a wave.")]
    public int maxRewardChoices = 4;

    [Header("Boss Settings")]
    [Tooltip("Boss bar prefab used for ALL bosses.")]
    public GameObject bossBarPrefab;
    [Tooltip("Status effect display prefab used for ALL bosses.")]
    public GameObject statusEffectDisplayPrefab;
    [Tooltip("All bosses that CAN spawn during any boss wave (randomly selected).")]
    public List<GameObject> bossPrefabs = new();
    [Tooltip("Chance (%) any regular wave turns into a boss wave.")]
    public float bossWaveChance = 10f;
    [Tooltip("Additional chance (%) a regular wave turns into a boss wave if the previous wave was NOT a boss wave.")]
    public float bossWaveChanceIfPreviousNotBoss = 20f;
    [Tooltip("Minimum number of waves between boss waves.")]
    public int minWavesBetweenBossWaves = 5;

    [Header("Enemy Pool")]
    [Tooltip("All enemies that CAN spawn during any wave (randomly selected).")]
    public List<GameObject> enemyPrefabs = new();

    private int lastBossWave;
    private bool isBossWave = false;
    private int maxTotalEnemies;

    protected override void Start()
    {
        base.Start();
        lastBossWave = -minWavesBetweenBossWaves;
        maxTotalEnemies = baseMaxTotalEnemies;
    }

    public override void StartNextWave()
    {
        if (isWaveActive) return;

        ActiveManager = this;

        totalSpawned = 0;
        currentEnemies.Clear();

        if (!RollAndGenerateAnomaly()) BeginWave();
    }

    protected override void BeginWave()
    {
        isWaveActive = true;
        currentWaveIndex++;

        int wave = GetCurrentWave();
        isBossWave = ShouldBeBossWave(wave);
        if (isBossWave) lastBossWave = wave;

        if (wave > 1) maxTotalEnemies += Random.Range(1, 3);

        waveInfoPanel.SetActive(true);
        waveText.text = $"Wave {wave}";

        HandleWave();
    }

    private void HandleWave()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(WaveSpawnRoutine());
    }

    private IEnumerator WaveSpawnRoutine()
    {
        int wave = GetCurrentWave();
        int maxCurrent = maxCurrentEnemies;

        while (totalSpawned < maxTotalEnemies)
        {
            if (currentEnemies.Count >= maxCurrent)
            {
                CleanEnemyList();
                yield return null;
                continue;
            }

            SpawnEnemies();

            float spawnDelay = Random.Range(GetMinSpawnFrequency(wave), GetMaxSpawnFrequency(wave));
            yield return new WaitForSeconds(spawnDelay);
        }
        while (currentEnemies.Count > 0)
        {
            CleanEnemyList();
            yield return _waitForSeconds0_5;
        }

        if (activeBossBar != null) GameController.SetTitleForDuration("Boss Defeated", 0.5f, 0.25f, 0.25f);
        else if (currentAnomaly != null && currentAnomaly.isActive) GameController.SetTitleForDuration("Anomaly Complete", 0.5f, 0.25f, 0.25f);
        else GameController.SetTitleForDuration($"Wave {wave} Complete", 0.5f, 0.25f, 0.25f);

        yield return _waitForSeconds1_5;

        EndWave();
    }

    private void SpawnEnemies()
    {
        int wave = GetCurrentWave();
        int spawnCount = Mathf.RoundToInt(wave / 10) + 1;
        for (int i = 0; i < spawnCount; i++) SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        int wave = GetCurrentWave();
        int level = GetEnemyLevel(wave);

        GameObject prefab = isBossWave ? GetRandomBoss() : GetRandomEnemy();
        if (prefab == null) return;

        var enemy = EnemySpawner.SpawnEnemy(prefab, spawnLocation, spawnRadius, level);

        if (enemy.TryGetComponent<EntityStatManager>(out var esm) && currentAnomaly is StatModifierInstance statMod)
            esm.AddStat(statMod.GetBuff());

        if (isBossWave && bossBarPrefab != null && activeBossBar == null)
        {
            Transform spawnParent = bossBarContainer != null ? bossBarContainer : waveInfoPanel.transform.parent;
            activeBossBar = Instantiate(bossBarPrefab, spawnParent);

            string bossName = prefab.name;
            if (activeBossBar.TryGetComponent<BossBarUI>(out var bossBarScript))
                bossBarScript.Setup($"[Lv. {level}] {bossName}", esm);
        }

        if (statusEffectDisplayPrefab != null && enemy.TryGetComponent<StatusEffectManager>(out var sem) && lastBossWave == wave)
        {
            Transform spawnParent = statusEffectDisplayContainer != null ? statusEffectDisplayContainer : waveInfoPanel.transform.parent;
            sem.displayPrefab = statusEffectDisplayPrefab;
            sem.displayContainer = spawnParent;
        }

        totalSpawned++;
        currentEnemies.Add(enemy);
    }

    protected override void TriggerStandardRewards(int w)
    {
        pendingStandardRewards = false;
        if (currentAnomaly != null) currentAnomaly.Cleanup();

        currentAnomaly = null;

        if (w % milestoneInterval == 0) GenerateMilestoneRewards();
        else if (w % 5 == 0) GenerateMixedPool();
        else if (Random.Range(0f, 100f) < 15f) GenerateMixedPool();
        else GenerateRewards();
    }

    protected override void OnAnomalyButtonClicked(AnomalyInstance instance)
    {
        CloseRewardUI();
        Time.timeScale = 1f;

        if (instance != null)
        {
            currentAnomaly = instance;
            currentAnomaly.StartAnomaly();
        }

        BeginWave();
    }

    public override int GetCurrentWave() => currentWaveIndex;

    private bool ShouldBeBossWave(int wave)
    {
        if (wave <= minWavesBetweenBossWaves) return false;
        if (bossPrefabs == null || bossPrefabs.Count == 0) return false;
        if (wave - lastBossWave < minWavesBetweenBossWaves) return false;

        float chance = bossWaveChance;
        if (wave - lastBossWave > 1) chance += bossWaveChanceIfPreviousNotBoss;

        return Random.Range(0f, 100f) < chance;
    }

    private GameObject GetRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;
        return enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
    }

    private GameObject GetRandomBoss()
    {
        if (bossPrefabs == null || bossPrefabs.Count == 0) return null;
        return bossPrefabs[Random.Range(0, bossPrefabs.Count)];
    }

    private int GetEnemyLevel(int wave) => baseEnemyLevel + (wave - 1);
    private float GetMinSpawnFrequency(int wave) => Mathf.Max(0.1f, minSpawnFrequency - (spawnSpeedIncreasePerWave * (wave - 1)));
    private float GetMaxSpawnFrequency(int wave) => Mathf.Max(0.1f, maxSpawnFrequency - (spawnSpeedIncreasePerWave * (wave - 1)));
}