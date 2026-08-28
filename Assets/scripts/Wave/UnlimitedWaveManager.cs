using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

namespace CrystalFlux.WaveSystem
{
    public class UnlimitedWaveManager : WaveManager
    {
        [Header("Wave Scaling")]
        public Vector2 spawnLocation;
        public int maxCurrentEnemies = 10;
        public int baseMaxTotalEnemies = 20;
        public int baseEnemyLevel = 1;
        public float minSpawnFrequency = 1f;
        public float maxSpawnFrequency = 3f;
        public float spawnSpeedIncreasePerWave = 0.05f;
        public int minRewardChoices = 2;
        public int maxRewardChoices = 4;

        [Header("Boss Settings")]
        public GameObject bossBarPrefab;
        public GameObject statusEffectDisplayPrefab;
        public float bossWaveChance = 5f;
        public float bossWaveChanceIfPreviousNotBoss = 10f;
        public int minWavesBetweenBossWaves = 5;

        [Header("Enemy Pools")]
        public List<GameObject> enemyPrefabs = new();
        public List<GameObject> bossPrefabs = new();

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

            if (activeBossBar != null) GameController?.SetTitleForDuration("Boss Defeated", 0.5f, 0.25f, 0.25f);
            else if (currentAnomaly != null && currentAnomaly.isActive) GameController?.SetTitleForDuration("Anomaly Complete", 0.5f, 0.25f, 0.25f);
            else GameController?.SetTitleForDuration($"Wave {wave} Complete", 0.5f, 0.25f, 0.25f);

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

            if (enemy.TryGetComponent<IStatProvider>(out var esm) && currentAnomaly is StatModifierInstance statMod)
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
}
