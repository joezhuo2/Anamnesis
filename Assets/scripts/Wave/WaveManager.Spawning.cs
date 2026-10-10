using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SettingsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.WaveSystem
{
    public partial class WaveManager
    {
        protected int EnemyLevel(int baseLevel)
            => Mathf.Max(1, baseLevel + D.enemyLevelAdd + Mathf.FloorToInt(D.enemyLevelPerWaveAdd * (GetCurrentWave() - 1)));

        public virtual void StartNextWave()
        {
            if (isWaveActive) return;

            ActiveManager = this;

            if (currentWaveIndex >= currentSequence.waves.Count)
            {
                currentWaveIndex = 0;
                if (currentSequence.nextSequence != null) currentSequence = currentSequence.nextSequence;
                else return;
            }

            totalSpawned = 0;
            reservedEnemies = 0;
            currentEnemies.Clear();

            if (!RollAndGenerateAnomaly()) BeginWave();
        }

        protected virtual void BeginWave()
        {
            WaveData currentWave = currentSequence.waves[currentWaveIndex];

            ArmContract(IsBossWave(currentWave));
            isWaveActive = true;
            waveResolving = false;
            currentWaveIndex++;

            enemiesKilled = 0;
            waveMaxTotalEnemies = IsBossWave(currentWave) ? BossCount : IsDuel ? 1 : ScaleEnemyCount(currentWave.maxTotalEnemies + D.maxTotalEnemiesAdd);

            waveInfoPanel.SetActive(true);
            UpdateWaveText();
            RefillPlayerResources();

            HandleWave(currentWave);
        }

        protected void RefillPlayerResources()
        {
            cprp ??= GameObject.FindWithTag("Player")?.GetComponent<PlayerResourcePool>();
            if (cprp != null) cprp.RefillAll();
        }

        protected void HandleWave(WaveData c)
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(WaveSpawnRoutine(c));
        }

        protected static bool IsBossWave(WaveData c) => c != null && c.bossBarPrefab != null;

        protected virtual bool NextWaveIsBoss()
        {
            if (currentSequence == null || currentSequence.waves == null) return false;
            if (currentWaveIndex < 0 || currentWaveIndex >= currentSequence.waves.Count) return false;
            return IsBossWave(currentSequence.waves[currentWaveIndex]);
        }

        protected int ScaleEnemyCount(int baseCount) => Mathf.Max(1, Mathf.RoundToInt(baseCount * EnemyCountMult));

        protected IEnumerator WaveSpawnRoutine(WaveData c)
        {
            int maxCurrent = IsBossWave(c) ? BossCount : IsDuel ? 1 : ScaleEnemyCount(c.maxCurrentEnemies + D.maxCurrentEnemiesAdd);

            while (RemainingToSpawn > 0)
            {
                CleanEnemyList();
                if (currentEnemies.Count >= maxCurrent)
                {
                    yield return _waitForSeconds0_25;
                    continue;
                }

                SpawnEnemies(c);
                yield return WaitForNextSpawn(Random.Range(c.minSpawnFrequency, c.maxSpawnFrequency));
            }
            while (currentEnemies.Count > 0 || reservedEnemies > 0)
            {
                CleanEnemyList();
                yield return _waitForSeconds0_5;
            }

            waveResolving = true;

            EvaluateWaveEnd();

            if (showCompletionMessage)
            {
                if (activeBossBar != null && Hivemind == null) GameController?.SetTitleForDuration("Boss Defeated", 0.5f, 0.25f, 0.25f);
                else if (currentAnomaly != null && currentAnomaly.isActive) GameController?.SetTitleForDuration("Anomaly Complete", 0.5f, 0.25f, 0.25f);
                else GameController?.SetTitleForDuration($"Wave {GetCurrentWave()} Complete", 0.5f, 0.25f, 0.25f);
            }

            RollAndAnnounceWaveRewards();

            if (showCompletionMessage) yield return _waitForSeconds1_5;

            EndWave();
        }

        protected void SpawnEnemies(WaveData c)
        {
            if (IsBossWave(c) || IsDuel)
            {
                int bosses = Twin != null ? RemainingToSpawn : 1;
                for (int i = 0; i < bosses; i++) SpawnEnemy(c);
                return;
            }

            if (Stampede != null)
            {
                int all = RemainingToSpawn;
                for (int i = 0; i < all; i++) SpawnEnemy(c);
                return;
            }

            if (c.maxTotalEnemies == 1 || c.maxCurrentEnemies == 1)
            {
                SpawnEnemy(c);
                return;
            }

            int spawnCount = enableExtraSpawns ? Mathf.Min(Mathf.RoundToInt(GetCurrentWave() / 10) + 1, RemainingToSpawn) : 1;
            for (int i = 0; i < spawnCount; i++) SpawnEnemy(c);
        }

        protected void SpawnEnemy(WaveData c)
        {
            int level = EnemyLevel(c.enemyLevel);
            var enemy = EnemySpawning.SpawnEnemy(c.enemyPrefab, currentSequence.spawnLocation, SpawnRadius, level);
            if (enemy == null) return;

            bool hasStats = enemy.TryGetComponent<IStatProvider>(out var esm);

            ApplySpawnHooks(enemy, c.enemyPrefab, level, hasStats ? esm : null);

            GameObject bossBarSource = IsDuel ? DuelBossBarPrefab(c.bossBarPrefab) : c.bossBarPrefab;

            if (hasStats && !TrySpawnHivemindBar(c.bossBarPrefab) && !TrySpawnTwinBar(bossBarSource, c.bossBarName) && bossBarSource != null && activeBossBar == null)
            {
                Transform spawnParent = bossBarContainer != null ? bossBarContainer : waveInfoPanel.transform.parent;
                activeBossBar = Instantiate(bossBarSource, spawnParent);

                if (activeBossBar.TryGetComponent<IBossBar>(out var bossBarScript))
                    bossBarScript.Setup(IsDuel ? DuelTitle(enemy, c.enemyPrefab, level) : c.bossBarName, esm);
            }

            GameObject statusSource = IsDuel ? DuelStatusEffectPrefab(c.statusEffectDisplayPrefab) : c.statusEffectDisplayPrefab;

            if (statusSource != null && enemy.TryGetComponent<IStatusEffectReceiver>(out var sem))
            {
                Transform spawnParent = statusEffectDisplayContainer != null ? statusEffectDisplayContainer : waveInfoPanel.transform.parent;
                sem.DisplayPrefab = statusSource;
                sem.DisplayContainer = spawnParent;
            }

            totalSpawned++;
            currentEnemies.Add(enemy);
        }

        protected bool TrySpawnHivemindBar(GameObject fallback)
        {
            HivemindInstance h = Hivemind;
            if (h == null) return false;
            if (activeBossBar != null) return true;

            GameObject src = DuelBossBarPrefab(fallback);
            if (src == null) return true;

            Transform spawnParent = bossBarContainer != null ? bossBarContainer : waveInfoPanel.transform.parent;
            activeBossBar = Instantiate(src, spawnParent);

            if (activeBossBar.TryGetComponent<BossBarUI>(out var bb)) bb.SetupPool(h.amd != null ? h.amd.anomalyName : "Hivemind", h.PoolCur, h.PoolMax);
            return true;
        }

        protected bool TrySpawnTwinBar(GameObject src, string title)
        {
            TwinCrownsInstance t = Twin;
            if (t == null) return false;
            if (activeBossBar != null || src == null) return true;

            Transform spawnParent = bossBarContainer != null ? bossBarContainer : waveInfoPanel.transform.parent;
            activeBossBar = Instantiate(src, spawnParent);

            if (activeBossBar.TryGetComponent<BossBarUI>(out var bb)) bb.SetupPool(title, t.PoolCur, t.PoolMax);
            return true;
        }

        public static void StopSpawning()
        {
            WaveManager wm = ActiveManager;
            if (wm == null || !wm.isWaveActive) return;

            wm.waveMaxTotalEnemies = wm.totalSpawned + wm.reservedEnemies;
            wm.UpdateWaveText();
        }

        protected GameObject DuelBossBarPrefab(GameObject fallback) => duelBossBarPrefab != null ? duelBossBarPrefab : fallback;

        protected GameObject DuelStatusEffectPrefab(GameObject fallback) => duelStatusEffectPrefab != null ? duelStatusEffectPrefab : fallback;

        protected static string DuelTitle(GameObject enemy, GameObject prefab, int level)
        {
            string name = null;

            if (enemy != null && enemy.TryGetComponent<EnemyStatManager>(out var esm) && !string.IsNullOrWhiteSpace(esm.displayName))
                name = esm.displayName;
            else if (prefab != null) name = prefab.name;

            return $"[Lv. {level}] {name}";
        }

        protected IEnumerator WaitForNextSpawn(float delay)
        {
            float remaining = delay;
            int lastAlive = currentEnemies.Count;

            while (remaining > 0f)
            {
                yield return null;
                remaining -= Time.deltaTime;

                CleanEnemyList();
                int alive = currentEnemies.Count;

                if (alive < lastAlive)
                    remaining *= Mathf.Pow(1f - killSpawnSpeedup, lastAlive - alive);

                lastAlive = alive;
            }
        }

        public static void RegisterSplitEnemy(GameObject enemy)
        {
            if (enemy == null || ActiveManager == null || !ActiveManager.isWaveActive) return;

            ActiveManager.currentEnemies.Add(enemy);
            ActiveManager.totalSpawned++;
            ActiveManager.waveMaxTotalEnemies++;
            ActiveManager.UpdateWaveText();
        }

        protected int RemainingToSpawn => waveMaxTotalEnemies - totalSpawned - reservedEnemies;

        public static bool ReserveEnemies(int count)
        {
            WaveManager wm = ActiveManager;
            if (count <= 0 || wm == null || !wm.isWaveActive || wm.waveResolving) return false;

            wm.reservedEnemies += count;
            wm.waveMaxTotalEnemies += count;
            wm.UpdateWaveText();
            return true;
        }

        public static void ConsumeReservation()
        {
            WaveManager wm = ActiveManager;
            if (wm == null || wm.reservedEnemies <= 0) return;

            wm.reservedEnemies--;
            wm.waveMaxTotalEnemies--;
            wm.UpdateWaveText();
        }

        public static GameObject SpawnAmbushEnemy(GameObject prefab, Vector2 pos, float radius, int levelBonus)
        {
            WaveManager wm = ActiveManager;
            if (prefab == null || wm == null || !wm.isWaveActive || wm.waveResolving) return null;

            int level = Mathf.Max(1, wm.CurrentEnemyLevel() + levelBonus);
            GameObject enemy = EnemySpawning.SpawnEnemy(prefab, pos, radius, level);
            if (enemy == null) return null;

            wm.ApplySpawnHooks(enemy, prefab, level, enemy.TryGetComponent<IStatProvider>(out var esm) ? esm : null);

            RegisterSplitEnemy(enemy);
            return enemy;
        }

        public static int EnemyLevelFor(int levelBonus)
        {
            WaveManager wm = ActiveManager;
            return Mathf.Max(1, (wm != null ? wm.CurrentEnemyLevel() : 1) + levelBonus);
        }

        protected void ApplySpawnHooks(GameObject enemy, GameObject prefab, int level, IStatProvider esm)
        {
            if (currentAnomaly != null)
            {
                if (esm != null) currentAnomaly.ApplyEnemyBuffs(esm);
                currentAnomaly.OnEnemySpawned(enemy, prefab, level);
            }

            if (contractArmed && currentContract != null)
            {
                if (esm != null) currentContract.ApplyEnemyBuffs(esm);
                currentContract.OnEnemySpawned(enemy, prefab, level);
            }
        }

        protected virtual int CurrentEnemyLevel()
        {
            if (currentSequence == null || currentSequence.waves == null || currentSequence.waves.Count == 0) return EnemyLevel(1);

            int i = Mathf.Clamp(currentWaveIndex - 1, 0, currentSequence.waves.Count - 1);
            WaveData c = currentSequence.waves[i];
            return EnemyLevel(c != null ? c.enemyLevel : 1);
        }

        protected void CleanEnemyList()
        {
            int removed = currentEnemies.RemoveAll(enemy => enemy == null);
            if (removed <= 0) return;

            enemiesKilled += removed;
            UpdateWaveText();
        }

        protected void UpdateWaveText()
        {
            if (waveText != null) waveText.text = $"Wave {GetCurrentWave()}/{totalWaves}\nEnemies: {enemiesKilled}/{waveMaxTotalEnemies}";
        }
        
        protected void EndWave()
        {
            WaveCleanup();
            RollSynergyOffer(GetCurrentWave());

            OpenRewardButtons();
            UpdateOccasionalWaveRewards(GetCurrentWave());
            ApplyContractRewards();
            DisarmContract();
            UpdateRerollUI();

            if (pendingContractPool) OpenContractPool();
            else if (currentAnomaly != null) CleanupAnomaly();
            else TriggerStandardRewards(GetCurrentWave());
        }
        private void WaveCleanup()
        {
            isWaveActive = false;
            waveInfoPanel.SetActive(false);
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

            if (activeBossBar != null)
            {
                Destroy(activeBossBar);
                activeBossBar = null;
            }
        }
    }
}
