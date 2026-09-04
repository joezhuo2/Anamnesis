using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.WaveSystem
{
    public class WaveManager : MonoBehaviour
    {
        protected static WaveManager ActiveManager;

        [Header("Difficulty")]
        public DifficultyData difficulty;

        [Header("Reroll Settings")]
        public int rerollGoldCost = 200;

        [Header("Basic Settings")]
        public WaveSequence currentSequence;
        public float spawnRadius = 2f;
        [Range(0f, 0.9f)] public float killSpawnSpeedup = 0.15f;
        public bool enableExtraSpawns = true;

        protected bool showCompletionMessage => GameSettings.Current.showWaveCompletionMessage;

        [Header("Wave Info Settings")]
        public GameObject waveInfoPanel;
        public TextMeshProUGUI anomalyInfoText;
        public TextMeshProUGUI waveText;
        public Transform bossBarContainer;
        public Transform statusEffectDisplayContainer;

        [Header("Action Buttons")]
        public Transform buttonContainer;
        public Transform actionButtonContainer;
        public Button rerollButton;
        public Button skipButton;
        public int rerolls;
        public TextMeshProUGUI rerollText;
        public Button nextWaveButton;
        public Button corruptButton;

        [Header("Corruption Settings")]
        public float corruptChance = 40f;
        public float corruptPositiveChance = 40f;
        public float maxCorruptBoost = 80f;
        public float corruptionSpecialChance = 8f;
        public List<AttackReward> corruptionSpecialPool = new();

        [Header("Reward Panel Settings")]
        public GameObject rewardPanel;
        public GameObject rewardButtonPrefab;
        public TextMeshProUGUI rewardTitleText;
        public GameObject rewardTitleWrapper;
        public string rewardTitle = "Choose your reward";
        public string anomalyTitle = "Select anomaly reward";

        [Header("Reward Pools")]
        public List<BaseReward> baseBuffPool;
        public List<AttackReward> rarePool;
        public List<PlayerUpgradeReward> treasurePool;
        public List<BaseReward> mixedPool;
        public List<RarityData> rarityData;

        [Header("Milestone Reward Settings")]
        public List<MilestoneReward> milestoneRewards = new();
        public int milestoneInterval = 25;
        public int milestoneRewardChoices = 3;

        [Header("Anomaly Settings")]
        public List<AnomalyData> availableAnomalies = new();
        public GameObject anomalyPrefab = null;
        public AnomalyInstance currentAnomaly = null;
        public int minAnomalyCount = 2;
        public int maxAnomalyCount = 5;
        public float anomalyChance = 15;
        public float anomalyGlobalMinWave = 10;

        protected IAnnouncer GameController => IAnnouncer.Current ?? null;
        protected DifficultyData D => difficulty != null ? difficulty : DifficultyData.Neutral;
        protected float Quality => additionalQuality + D.qualityBonusAdd;
        protected int RerollGoldCost => Mathf.Max(0, rerollGoldCost + D.rerollGoldCostAdd);
        protected RewardType type = RewardType.Basic;
        protected GameObject activeBossBar;
        protected IStatProvider cpsm;
        protected ICurrencyHolder cich;
        protected IAttackHandler cpah;
        protected IUpgradeHolder cpum;
        protected ISkillPointHolder cpst;
        protected readonly List<AttackReward> availableRarePool = new();
        protected readonly List<AttackReward> availableCorruptionSpecialPool = new();
        protected readonly List<AttackReward> corruptionSpecialsThisRoll = new();
        protected readonly List<PlayerUpgradeReward> availableTreasurePool = new();
        protected int currentWaveIndex = 0;
        protected int totalSpawned = 0;
        protected int enemiesKilled = 0;
        protected int waveMaxTotalEnemies = 0;
        protected readonly List<GameObject> currentEnemies = new();
        protected bool isWaveActive = false;
        protected Coroutine spawnCoroutine;
        protected bool pendingStandardRewards = false;
        protected float additionalQuality = 0f;
        protected int pendingOccasionalRerolls = -1;
        protected int pendingOccasionalSkillPoints = 0;
        protected int pendingAnomalyRerolls = -1;
        protected int pendingAnomalySkillPoints = -1;
        protected readonly List<GameObject> activeRewardButtons = new();
        protected static readonly WaitForSeconds _waitForSeconds1_5 = new(1.5f);
        protected static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);

        protected void Awake()
        {
            availableRarePool.AddRange(rarePool);
            availableCorruptionSpecialPool.AddRange(corruptionSpecialPool);
            availableTreasurePool.AddRange(treasurePool);
        }

        protected virtual void Start()
        {
            waveInfoPanel.SetActive(false);

            if (rewardTitleWrapper != null) rewardTitleWrapper.SetActive(false);

            SortRarityData();
            CloseRewardButtons();
            UpdateRerollUI();
            SetupActionButtonTooltips();
        }

        public virtual void ApplyDifficulty(DifficultyData d)
        {
            if (d == null) return;

            difficulty = d;

            rerolls = Mathf.Max(0, rerolls + d.startingRerollsAdd);

            if (d.startingSkillPointsAdd != 0)
            {
                CachePlayerSkillTree();
                if (cpst != null)
                {
                    if (d.startingSkillPointsAdd > 0) cpst.AddSkillPoints(d.startingSkillPointsAdd);
                    else cpst.TrySpend(Mathf.Min(-d.startingSkillPointsAdd, cpst.SkillPoints));
                }
            }

            UpdateRerollUI();
            SetupActionButtonTooltips();
        }

        protected int EnemyLevel(int baseLevel)
            => Mathf.Max(1, baseLevel + D.enemyLevelAdd + Mathf.FloorToInt(D.enemyLevelPerWaveAdd * (GetCurrentWave() - 1)));

        protected int RollAnomalyRerolls()
        {
            int min = Mathf.Max(0, 1 + D.anomalyRerollMinAdd);
            int max = Mathf.Max(min, 3 + D.anomalyRerollMaxAdd);
            return Random.Range(min, max + 1);
        }

        protected int AnomalySkillPointGain() => Mathf.Max(0, 1 + D.anomalySkillPointAdd);

        private void SetupActionButtonTooltips()
        {
            if (rerollButton != null && rerollButton.TryGetComponent<ITooltipDisplay>(out var td))
                td.ShowTooltip("Reroll", $"Rerolls all reward choices.\nCost: 1 reroll token or {RerollGoldCost} gold if none are available.");
            if (corruptButton != null && corruptButton.TryGetComponent<ITooltipDisplay>(out var td2))
                td2.ShowTooltip("Corrupt", "Chance to corrupt any rewards massively increase or decrease their values.\nCan only be used once per wave and removes all other options.");
            if (nextWaveButton != null && nextWaveButton.TryGetComponent<ITooltipDisplay>(out var td3))
                td3.ShowTooltip("Skip", "Skip rewards and start the next wave immediately.");
        }

        private void OnDestroy()
        {
            if (ActiveManager == this) ActiveManager = null;

            if (currentAnomaly != null)
            {
                currentAnomaly.Cleanup();
                currentAnomaly = null;
            }

            ClearRewardButtons();

            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        }
        private void Update()
        {
            if (currentAnomaly != null && currentAnomaly.isActive)
            {
                currentAnomaly.UpdateCheck(Time.deltaTime);

                if (anomalyInfoText != null)
                {
                    switch (currentAnomaly.amd.anomalyType)
                    {
                        case AnomalyType.TimeTrial: UpdateAnomalyTimeInfo(); break;
                        case AnomalyType.NoDamage: anomalyInfoText.text = "No Damage Anomaly Active"; break;
                        case AnomalyType.StatModifier: anomalyInfoText.text = currentAnomaly.Description; break;
                        default: break;
                    }
                }
            }
            else
            {
                if (anomalyInfoText != null && anomalyInfoText.text != "") anomalyInfoText.text = "";
            }
        }

        private void UpdateAnomalyTimeInfo()
        {
            if (currentAnomaly is TimeTrialInstance tt)
            {
                if (tt.timeRemaining <= 0f)
                {
                    anomalyInfoText.text = "Time's Up! Anomaly Failed";
                    GameController?.SetTitleForDuration("Anomaly Failed", 2f, 0.5f, 0.5f);
                    GameController?.SetSubtitleForDuration("Time's Up!", 2f, 0.5f, 0.5f);
                    return;
                }
                anomalyInfoText.text = $"Time Remaining: {tt.timeRemaining:F1}s";
            }
        }

        protected GameObject GetOrCreateRewardButton()
        {
            Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
            GameObject btnObj = PrefabPool.Acquire(rewardButtonPrefab, targetParent);

            if (btnObj != null) activeRewardButtons.Add(btnObj);
            return btnObj;
        }

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
            currentEnemies.Clear();

            if (!RollAndGenerateAnomaly()) BeginWave();
        }

        protected virtual void BeginWave()
        {
            WaveData currentWave = currentSequence.waves[currentWaveIndex];

            isWaveActive = true;
            currentWaveIndex++;

            enemiesKilled = 0;
            waveMaxTotalEnemies = IsBossWave(currentWave) ? 1 : Mathf.Max(1, currentWave.maxTotalEnemies + D.maxTotalEnemiesAdd);

            waveInfoPanel.SetActive(true);
            UpdateWaveText();

            HandleWave(currentWave);
        }

        protected void HandleWave(WaveData c)
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(WaveSpawnRoutine(c));
        }

        protected static bool IsBossWave(WaveData c) => c != null && c.bossBarPrefab != null;

        protected IEnumerator WaveSpawnRoutine(WaveData c)
        {
            int maxCurrent = IsBossWave(c) ? 1 : Mathf.Max(1, c.maxCurrentEnemies + D.maxCurrentEnemiesAdd);

            while (totalSpawned < waveMaxTotalEnemies)
            {
                CleanEnemyList();
                if (currentEnemies.Count >= maxCurrent)
                {
                    yield return null;
                    continue;
                }

                SpawnEnemies(c);
                yield return WaitForNextSpawn(Random.Range(c.minSpawnFrequency, c.maxSpawnFrequency));
            }
            while (currentEnemies.Count > 0)
            {
                CleanEnemyList();
                yield return _waitForSeconds0_5;
            }

            if (showCompletionMessage)
            {
                if (activeBossBar != null) GameController?.SetTitleForDuration("Boss Defeated", 0.5f, 0.25f, 0.25f);
                else if (currentAnomaly != null && currentAnomaly.isActive) GameController?.SetTitleForDuration("Anomaly Complete", 0.5f, 0.25f, 0.25f);
                else GameController?.SetTitleForDuration($"Wave {GetCurrentWave()} Complete", 0.5f, 0.25f, 0.25f);
            }

            RollAndAnnounceWaveRewards();

            if (showCompletionMessage) yield return _waitForSeconds1_5;

            EndWave();
        }

        protected void SpawnEnemies(WaveData c)
        {
            if (IsBossWave(c) || c.maxTotalEnemies == 1 || c.maxCurrentEnemies == 1)
            {
                SpawnEnemy(c);
                return;
            }

            int spawnCount = enableExtraSpawns ? Mathf.Min(Mathf.RoundToInt(GetCurrentWave() / 10) + 1, waveMaxTotalEnemies - totalSpawned) : 1;
            for (int i = 0; i < spawnCount; i++) SpawnEnemy(c);
        }

        protected void SpawnEnemy(WaveData c)
        {
            var enemy = EnemySpawning.SpawnEnemy(c.enemyPrefab, currentSequence.spawnLocation, spawnRadius, EnemyLevel(c.enemyLevel));
            if (enemy == null) return;

            bool hasStats = enemy.TryGetComponent<IStatProvider>(out var esm);

            if (hasStats && currentAnomaly is StatModifierInstance statMod)
                esm.AddStat(statMod.GetBuff());

            if (hasStats && c.bossBarPrefab != null && activeBossBar == null)
            {
                Transform spawnParent = bossBarContainer != null ? bossBarContainer : waveInfoPanel.transform.parent;
                activeBossBar = Instantiate(c.bossBarPrefab, spawnParent);

                if (activeBossBar.TryGetComponent<IBossBar>(out var bossBarScript))
                    bossBarScript.Setup(c.bossBarName, esm);
            }

            if (c.statusEffectDisplayPrefab != null && enemy.TryGetComponent<IStatusEffectReceiver>(out var sem))
            {
                Transform spawnParent = statusEffectDisplayContainer != null ? statusEffectDisplayContainer : waveInfoPanel.transform.parent;
                sem.DisplayPrefab = c.statusEffectDisplayPrefab;
                sem.DisplayContainer = spawnParent;
            }

            totalSpawned++;
            currentEnemies.Add(enemy);
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

        protected void CleanEnemyList()
        {
            int removed = currentEnemies.RemoveAll(enemy => enemy == null);
            if (removed <= 0) return;

            enemiesKilled += removed;
            UpdateWaveText();
        }

        protected void UpdateWaveText()
        {
            if (waveText != null) waveText.text = $"Wave {GetCurrentWave()} ({enemiesKilled}/{waveMaxTotalEnemies})";
        }
        protected void EndWave()
        {
            WaveCleanup();

            OpenRewardButtons();
            UpdateOccasionalWaveRewards(GetCurrentWave());
            UpdateRerollUI();

            if (currentAnomaly != null) CleanupAnomaly();
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

        protected void UpdateOccasionalWaveRewards(int wave)
        {
            if (pendingOccasionalRerolls < 0) RollOccasionalWaveRewards(wave);

            if (pendingOccasionalSkillPoints > 0)
            {
                CachePlayerSkillTree();
                if (cpst != null) cpst.AddSkillPoints(pendingOccasionalSkillPoints);
            }
            rerolls += pendingOccasionalRerolls;

            pendingOccasionalRerolls = -1;
            pendingOccasionalSkillPoints = 0;
        }

        private void RollOccasionalWaveRewards(int wave)
        {
            if (wave % 5 == 0)
            {
                pendingOccasionalRerolls = 1;
                pendingOccasionalSkillPoints = 1;
            }
            else
            {
                pendingOccasionalRerolls = Random.value < 0.5f + D.occasionalRerollChanceAdd ? 1 : 0;
                pendingOccasionalSkillPoints = Random.value < 0.5f + D.occasionalSkillPointChanceAdd ? 1 : 0;
            }
        }

        protected void RollAndAnnounceWaveRewards()
        {
            int wave = GetCurrentWave();
            RollOccasionalWaveRewards(wave);

            bool anomalyCompleted = currentAnomaly != null && currentAnomaly.isActive;
            pendingAnomalyRerolls = anomalyCompleted ? RollAnomalyRerolls() : 0;
            pendingAnomalySkillPoints = anomalyCompleted ? AnomalySkillPointGain() : 0;

            int rerollGain = pendingOccasionalRerolls + pendingAnomalyRerolls;
            int skillPointGain = pendingOccasionalSkillPoints + pendingAnomalySkillPoints;
            if (rerollGain <= 0 && skillPointGain <= 0 || !showCompletionMessage) return;

            string msg = "";
            if (rerollGain > 0) msg = $"+{rerollGain} Reroll{(rerollGain > 1 ? "s" : "")}";
            if (skillPointGain > 0)
            {
                if (msg.Length > 0) msg += ", ";
                msg += $"+{skillPointGain} Skill Point{(skillPointGain > 1 ? "s" : "")}";
            }

            GameController?.SetSubtitleForDuration(msg, 0.5f, 0.25f, 0.25f);
        }

        protected void CleanupAnomaly()
        {
            if (currentAnomaly.isActive)
            {
                HandleAnomalyRewards();
            }
            else
            {
                currentAnomaly.Cleanup();
                currentAnomaly = null;
                ResumeGameLoop();
            }
        }

        protected void HandleAnomalyRewards()
        {
            currentAnomaly.CompleteAnomaly();
            currentAnomaly.Cleanup();
            currentAnomaly = null;

            pendingStandardRewards = true;

            int c = pendingAnomalyRerolls >= 0 ? pendingAnomalyRerolls : RollAnomalyRerolls();
            int sp = pendingAnomalySkillPoints >= 0 ? pendingAnomalySkillPoints : AnomalySkillPointGain();
            pendingAnomalyRerolls = -1;
            pendingAnomalySkillPoints = -1;

            rerolls += c;
            UpdateRerollUI();

            additionalQuality += Random.Range(0.1f, 0.3f) + D.anomalyQualityAdd;

            CachePlayerSkillTree();
            if (cpst != null && sp > 0) cpst.AddSkillPoints(sp);

            type = RewardType.Mixed;
            PanelSetup();
            GenerateMixedPool();
        }
        protected virtual void TriggerStandardRewards(int w)
        {
            pendingStandardRewards = false;
            if (currentAnomaly != null) currentAnomaly.Cleanup();

            currentAnomaly = null;

            if (w % milestoneInterval == 0) GenerateMilestoneRewards();
            else if (w % 10 == 0 && w <= 20) GenerateTreasurePool();
            else if (w % 5 == 0 && w <= 15) GenerateRarePool();
            else if (w % 5 == 0) GenerateMixedPool();
            else GenerateRewards();
        }
        protected bool RollAndGenerateAnomaly()
        {
            if (currentAnomaly != null && currentAnomaly.isActive) return false;
            if (availableAnomalies == null || availableAnomalies.Count == 0) return false;
            if (anomalyPrefab == null || GetCurrentWave() <= anomalyGlobalMinWave) return false;

            float chance = anomalyChance + D.anomalyChanceAdd;
            if (minAnomalyCount <= 0 || maxAnomalyCount <= 0 || chance <= 0f) return false;

            float roll = Random.Range(0f, 100f);
            if (roll > chance) return false;

            return GenerateAnomalyChoices();
        }
        protected bool HasAnomalyChoices()
        {
            if (availableAnomalies == null || availableAnomalies.Count == 0) return false;
            if (anomalyPrefab == null) return false;
            if (minAnomalyCount <= 0 || maxAnomalyCount <= 0) return false;

            int w = GetCurrentWave();
            return availableAnomalies.Exists(a => a != null && w >= a.minWave && w <= a.maxWave);
        }
        protected bool GenerateAnomalyChoices()
        {
            if (availableAnomalies == null || availableAnomalies.Count == 0) return false;
            if (anomalyPrefab == null) return false;
            if (minAnomalyCount <= 0 || maxAnomalyCount <= 0) return false;

            var available = availableAnomalies.FindAll(a => a != null && GetCurrentWave() >= a.minWave && GetCurrentWave() <= a.maxWave);
            if (available.Count == 0) return false;

            type = RewardType.Anomaly;
            OpenAnomalyButtons();
            PanelSetup();

            int minChoices = Mathf.Max(1, minAnomalyCount + D.minAnomalyCountAdd);
            int maxChoices = Mathf.Max(minChoices, maxAnomalyCount + D.maxAnomalyCountAdd);
            int choices = Random.Range(minChoices, maxChoices + 1);

            for (int i = 0; i < choices; i++)
            {
                AnomalyData amd = available[Random.Range(0, available.Count)];
                AnomalyInstance instance = amd.CreateInstance();

                Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
                GameObject btnObj = PrefabPool.Acquire(anomalyPrefab, targetParent);
                if (btnObj == null) continue;

                activeRewardButtons.Add(btnObj);

                if (btnObj.TryGetComponent<AnomalyButtonUI>(out var anomalyButton))
                    anomalyButton.Setup(instance, OnAnomalyButtonClicked);
            }

            return true;
        }
        protected void GenerateRewards()
        {
            type = RewardType.Basic;
            int rewardChoices = PoolPreSetup();

            for (int i = 0; i < rewardChoices; i++)
            {
                if (baseBuffPool.Count == 0 || rarityData.Count == 0) break;

                BaseReward randomBuff = GetWeightedRandomBuff();

                RarityData chosenRarity = WaveQuality.GetWeightedRandomRarity(GetCurrentWave(), rarityData, Quality);

                GeneratedReward generated = new() { br = randomBuff, rd = chosenRarity };

                GameObject btnObj = GetOrCreateRewardButton();

                CachePlayerStatManager();
                string changeLine = BuildChangeLine(randomBuff.baseBuff.type, generated.finalVal);

                if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(generated, OnRewardClaimed, changeLine);
            }
            additionalQuality = 0f;
        }
        protected void GenerateMixedPool()
        {
            type = RewardType.Mixed;
            int rewardChoices = PoolPreSetup();

            for (int i = 0; i < rewardChoices; i++)
            {
                float poolRoll = Random.Range(0f, 100f);

                if (poolRoll < 75f)
                {
                    if (mixedPool.Count == 0 || rarityData.Count == 0) continue;

                    BaseReward randomBuff = GetWeightedRandomMixedBuff();
                    RarityData chosenRarity = WaveQuality.GetWeightedRandomRarity(GetCurrentWave(), rarityData, Quality);
                    GeneratedReward generated = new() { br = randomBuff, rd = chosenRarity };

                    GameObject btnObj = GetOrCreateRewardButton();

                    CachePlayerStatManager();
                    string changeLine = BuildChangeLine(randomBuff.baseBuff.type, generated.finalVal);

                    if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(generated, OnRewardClaimed, changeLine);
                }
                else if (poolRoll < 90f)
                {
                    AttackReward buff = PickRareReward();
                    if (buff == null) continue;
                    GameObject btnObj = GetOrCreateRewardButton();
                    if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnAttackRewardClaimed);
                }
                else
                {
                    PlayerUpgradeReward buff = PickTreasureReward();
                    if (buff == null) continue;
                    GameObject btnObj = GetOrCreateRewardButton();
                    if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnPlayerUpgradeRewardClaimed);
                }
            }

            additionalQuality = 0f;
        }
        protected void GenerateRarePool()
        {
            type = RewardType.Rare;
            int rewardChoices = PoolPreSetup();

            for (int i = 0; i < rewardChoices; i++)
            {
                AttackReward buff = PickRareReward();

                if (buff == null) break;

                GameObject btnObj = GetOrCreateRewardButton();

                if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                    rewardButton.Setup(buff, OnAttackRewardClaimed);
            }
        }
        protected void GenerateTreasurePool()
        {
            type = RewardType.Treasure;
            int rewardChoices = PoolPreSetup();

            for (int i = 0; i < rewardChoices; i++)
            {
                PlayerUpgradeReward buff = PickTreasureReward();

                if (buff == null) break;

                GameObject btnObj = GetOrCreateRewardButton();

                if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                    rewardButton.Setup(buff, OnPlayerUpgradeRewardClaimed);
            }
        }

        public bool TryStartPreRunPicks()
        {
            if (D.preRunPickCount <= 0) return false;
            if (!HasPreRunChoices()) return false;

            ActiveManager = this;
            type = RewardType.PreRun;

            OpenRewardButtons();
            GeneratePreRunPicks();
            UpdateRerollUI();

            return true;
        }

        protected bool HasPreRunChoices()
        {
            int wave = GetCurrentWave();
            return availableRarePool.Exists(a => a != null && a.minWave <= wave)
                || availableTreasurePool.Exists(t => t != null && t.minWave <= wave);
        }

        protected void GeneratePreRunPicks()
        {
            type = RewardType.PreRun;
            PanelSetup();

            int picks = Mathf.Max(1, D.preRunPickCount);

            for (int i = 0; i < picks; i++)
            {
                if (Random.Range(0f, 100f) < D.preRunTreasureChance && TryAddPreRunTreasure()) continue;
                if (TryAddPreRunRare()) continue;
                TryAddPreRunTreasure();
            }
        }

        private bool TryAddPreRunRare()
        {
            AttackReward buff = PickRareReward();
            if (buff == null) return false;

            GameObject btnObj = GetOrCreateRewardButton();
            if (btnObj == null) return false;

            if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnAttackRewardClaimed);
            return true;
        }
        private bool TryAddPreRunTreasure()
        {
            PlayerUpgradeReward buff = PickTreasureReward();
            if (buff == null) return false;

            GameObject btnObj = GetOrCreateRewardButton();
            if (btnObj == null) return false;

            if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnPlayerUpgradeRewardClaimed);
            return true;
        }
        protected AttackReward PickRareReward()
        {
            int wave = GetCurrentWave();
            AttackReward chosen = null;
            int eligible = 0;

            for (int i = 0; i < availableRarePool.Count; i++)
            {
                AttackReward candidate = availableRarePool[i];
                if (candidate == null || candidate.minWave > wave) continue;

                eligible++;
                if (Random.Range(0, eligible) == 0) chosen = candidate;
            }

            return chosen;
        }
        protected AttackReward PickCorruptionSpecialReward()
        {
            int wave = GetCurrentWave();
            AttackReward chosen = null;
            int eligible = 0;

            for (int i = 0; i < availableCorruptionSpecialPool.Count; i++)
            {
                AttackReward candidate = availableCorruptionSpecialPool[i];
                if (candidate == null || candidate.minWave > wave) continue;
                if (corruptionSpecialsThisRoll.Contains(candidate)) continue;

                eligible++;
                if (Random.Range(0, eligible) == 0) chosen = candidate;
            }

            return chosen;
        }
        protected PlayerUpgradeReward PickTreasureReward()
        {
            int wave = GetCurrentWave();
            PlayerUpgradeReward chosen = null;
            int eligible = 0;

            for (int i = 0; i < availableTreasurePool.Count; i++)
            {
                PlayerUpgradeReward candidate = availableTreasurePool[i];
                if (candidate == null || candidate.minWave > wave) continue;

                eligible++;
                if (Random.Range(0, eligible) == 0) chosen = candidate;
            }

            return chosen;
        }
        protected void GenerateMilestoneRewards()
        {
            type = RewardType.Milestone;
            int rewardChoices = Mathf.Min(Mathf.Max(1, milestoneRewardChoices + D.milestoneRewardChoicesAdd), milestoneRewards.Count);

            PanelSetup();

            var selectedRewards = GetWeightedRandomMilestoneRewards(rewardChoices);

            for (int i = 0; i < selectedRewards.Count; i++)
            {
                MilestoneReward sourceReward = selectedRewards[i];
                MilestoneRewardData generated = GenerateMilestoneRewardData(sourceReward);

                GameObject btnObj = GetOrCreateRewardButton();

                if (btnObj.TryGetComponent<RewardButton>(out var rb))
                    rb.Setup(generated, OnMilestoneRewardClaimed);
            }
        }
        protected List<MilestoneReward> GetWeightedRandomMilestoneRewards(int count)
        {
            var result = new List<MilestoneReward>();
            var available = new List<MilestoneReward>(milestoneRewards);

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                float totalWeight = 0;
                foreach (var r in available) totalWeight += r.weight;

                float roll = Random.Range(0f, totalWeight);
                float weightSum = 0;

                for (int j = 0; j < available.Count; j++)
                {
                    weightSum += available[j].weight;
                    if (roll <= weightSum)
                    {
                        result.Add(available[j]);
                        available.RemoveAt(j);
                        break;
                    }
                }
            }

            return result;
        }
        protected MilestoneRewardData GenerateMilestoneRewardData(MilestoneReward source)
        {
            var data = new MilestoneRewardData
            {
                rewardName = source.rewardName,
                generatedBuffs = new List<StatBuff>()
            };

            foreach (var baseBuff in source.baseStatBuffs)
            {
                float varianceMultiplier = 1f + Random.Range(-source.variance, source.variance);
                float finalValue = baseBuff.value * varianceMultiplier;
                StatBuff generatedBuff = new(baseBuff.type, finalValue);
                data.generatedBuffs.Add(generatedBuff);
            }

            return data;
        }
        protected void OnMilestoneRewardClaimed(MilestoneRewardData chosenReward)
        {
            CloseRewardUI();

            CachePlayerStatManager();
            if (cpsm != null)
            {
                foreach (var buff in chosenReward.generatedBuffs)
                    cpsm.AddStat(buff);
            }

            ResumeGameLoop();
        }

        protected int PoolPreSetup()
        {
            int rewardChoices = 0;
            if (!gameObject.TryGetComponent<UnlimitedWaveManager>(out var uwm) && currentSequence != null && currentSequence.waves != null && currentWaveIndex > 0)
            {
                WaveData completedWave = currentSequence.waves[currentWaveIndex - 1];
                rewardChoices = Random.Range(completedWave.minRewardChoices, completedWave.maxRewardChoices + 1);
            }
            else if (uwm != null)
            {
                rewardChoices = Random.Range(uwm.minRewardChoices, uwm.maxRewardChoices + 1);
            }
            else
            {
                rewardChoices = 1;
            }

            rewardChoices = Mathf.Max(1, rewardChoices + D.rewardChoicesAdd);

            PanelSetup();

            return rewardChoices;
        }

        protected void PanelSetup()
        {
            if (rewardPanel != null) rewardPanel.SetActive(true);
            ClearRewardButtons();

            UpdateRewardTitle();
            UpdateCorruptButton();

            Time.timeScale = 0f;
        }

        protected void UpdateRewardTitle()
        {
            if (rewardTitleText == null || rewardTitleWrapper == null) return;
            if (rewardTitleWrapper != null) rewardTitleWrapper.SetActive(true);
            rewardTitleText.text = type == RewardType.Anomaly ? anomalyTitle : rewardTitle;
        }

        private void UpdateCorruptButton()
        {
            if (corruptButton == null) return;

            bool allowed = type != RewardType.Anomaly && type != RewardType.Milestone && type != RewardType.PreRun && GetCurrentWave() % 5 != 0;
            corruptButton.gameObject.SetActive(allowed);
        }

        protected void UpdateRerollUI()
        {
            CachePlayerStatManager();

            var canGoldReroll = cich != null && cich.CurrentAmount >= RerollGoldCost;

            if (rerollText != null)
            {
                if (rerolls > 0)
                {
                    rerollText.text = rerolls.ToString();
                }
                else
                {
                    if (canGoldReroll) rerollText.text = $"{RerollGoldCost}g";
                    else rerollText.text = "0";
                }
            }

            if (rerollButton != null) rerollButton.interactable = rerolls > 0 || canGoldReroll;
        }

        protected void OnSkipButtonClicked()
        {
            if (ActiveManager != null && ActiveManager != this)
            {
                ActiveManager.OnSkipButtonClicked();
                return;
            }

            if (type == RewardType.Anomaly)
            {
                CloseRewardUI();
                Time.timeScale = 1f;
                if (currentAnomaly != null) currentAnomaly.Cleanup();
                currentAnomaly = null;
                BeginWave();
                return;
            }

            CloseRewardUI();
            ResumeGameLoop();
        }

        protected void OnRerollButtonClicked()
        {
            if (ActiveManager != null && ActiveManager != this)
            {
                ActiveManager.OnRerollButtonClicked();
                return;
            }

            CachePlayerStatManager();

            if (type == RewardType.Anomaly && !HasAnomalyChoices()) return;
            if (type == RewardType.PreRun && !HasPreRunChoices()) return;

            if (rerolls > 0)
                rerolls--;
            else if (cich == null || !cich.TrySpend(RerollGoldCost)) return;

            UpdateRerollUI();

            ClearRewardButtons();

            switch (type)
            {
                case RewardType.Anomaly: GenerateAnomalyChoices(); break;
                case RewardType.PreRun: GeneratePreRunPicks(); break;
                case RewardType.Basic: GenerateRewards(); break;
                case RewardType.Rare: GenerateRarePool(); break;
                case RewardType.Treasure: GenerateTreasurePool(); break;
                case RewardType.Mixed: GenerateMixedPool(); break;
                case RewardType.Milestone: GenerateMilestoneRewards(); break;
                default: break;
            }
        }

        public void OnCorruptButtonClicked()
        {
            if (ActiveManager != null && ActiveManager != this)
            {
                ActiveManager.OnCorruptButtonClicked();
                return;
            }

            CachePlayerStatManager();
            if (cpsm == null) return;

            corruptionSpecialsThisRoll.Clear();

            float cChance = corruptChance + D.corruptChanceAdd;
            float cPosChance = corruptPositiveChance + D.corruptPositiveChanceAdd;
            float cMaxBoost = Mathf.Max(2f, maxCorruptBoost + D.maxCorruptBoostAdd);

            foreach (GameObject rb in activeRewardButtons)
            {
                if (Random.value > (cChance * 0.01f)) continue;

                if (!rb.TryGetComponent<RewardButton>(out var grb)) continue;

                GeneratedReward gr = grb.gr;
                if (gr == null) continue;

                if (Random.value < (corruptionSpecialChance * 0.01f))
                {
                    AttackReward special = PickCorruptionSpecialReward();
                    if (special != null)
                    {
                        corruptionSpecialsThisRoll.Add(special);
                        grb.Setup(special, OnAttackRewardClaimed, true);
                        continue;
                    }
                }

                float corruptMult = (Random.value < (cPosChance * 0.01f) ? 1f : -1f) * (1f + (Random.Range(1, cMaxBoost) * 0.01f));

                gr.mult = corruptMult;

                string changeLine = BuildChangeLine(gr.br.baseBuff.type, gr.finalVal);

                grb.CorruptButton(changeLine, corruptMult);
            }

            if (rerollButton != null) rerollButton.gameObject.SetActive(false);
            if (corruptButton != null) corruptButton.gameObject.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);
        }

        protected virtual void OnAnomalyButtonClicked(AnomalyInstance instance)
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

        protected BaseReward GetWeightedRandomBuff()
        {
            float totalWeight = 0;
            foreach (var b in baseBuffPool) totalWeight += b.weight;

            float roll = Random.Range(0f, totalWeight);
            float weightSum = 0;

            foreach (var b in baseBuffPool)
            {
                weightSum += b.weight;
                if (roll <= weightSum) return b;
            }

            return baseBuffPool[0];
        }

        protected BaseReward GetWeightedRandomMixedBuff()
        {
            float totalWeight = 0;
            foreach (var b in mixedPool) totalWeight += b.weight;

            float roll = Random.Range(0f, totalWeight);
            float weightSum = 0;

            foreach (var b in mixedPool)
            {
                weightSum += b.weight;
                if (roll <= weightSum) return b;
            }

            return mixedPool[0];
        }
        protected void OnRewardClaimed(GeneratedReward chosenReward)
        {
            CloseRewardUI();

            StatBuff finalBuff = new(chosenReward.br.baseBuff.type, chosenReward.finalVal);
            CachePlayerStatManager();
            if (cpsm != null) cpsm.AddStat(finalBuff);

            ResumeGameLoop();
        }
        protected void OnAttackRewardClaimed(AttackReward chosenAttack)
        {
            CloseRewardUI();

            if (cpah == null) cpah = GameObject.FindWithTag("Player")?.GetComponent<IAttackHandler>();
            if (cpah != null) cpah.UpdateAttack(chosenAttack.type, chosenAttack.newAttack);
            if (availableRarePool.Contains(chosenAttack)) availableRarePool.Remove(chosenAttack);
            if (availableCorruptionSpecialPool.Contains(chosenAttack)) availableCorruptionSpecialPool.Remove(chosenAttack);

            ResumeGameLoop();
        }
        protected void OnPlayerUpgradeRewardClaimed(PlayerUpgradeReward chosenUpgrade)
        {
            CloseRewardUI();

            if (cpum == null) cpum = GameObject.FindWithTag("Player")?.GetComponent<IUpgradeHolder>();
            if (cpum != null) cpum.AddUpgrade(chosenUpgrade.upgrade);
            if (availableTreasurePool.Contains(chosenUpgrade)) availableTreasurePool.Remove(chosenUpgrade);

            ResumeGameLoop();
        }

        protected void CloseRewardUI()
        {
            ClearRewardButtons();
            if (rewardPanel != null) rewardPanel.SetActive(false);
            if (rewardTitleWrapper != null) rewardTitleWrapper.SetActive(false);
        }

        protected void CloseRewardButtons()
        {
            ClearRewardButtons();

            if (rerollButton != null) rerollButton.gameObject.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);
            if (corruptButton != null) corruptButton.gameObject.SetActive(false);
        }

        public void OpenAnomalyButtons()
        {
            if (rerollButton != null) rerollButton.gameObject.SetActive(true);
            if (skipButton != null) skipButton.gameObject.SetActive(true);
        }

        public void OpenRewardButtons() => OpenAnomalyButtons();

        protected void ResumeGameLoop()
        {
            if (pendingStandardRewards)
            {
                TriggerStandardRewards(GetCurrentWave());
            }
            else
            {
                Time.timeScale = 1f;
                pendingStandardRewards = false;
                StartNextWave();
            }
        }
        protected void ClearRewardButtons()
        {
            for (int i = 0; i < activeRewardButtons.Count; i++)
            {
                GameObject btn = activeRewardButtons[i];
                if (btn == null) continue;

                if (btn.TryGetComponent<RewardButton>(out var rewardButton)) rewardButton.ResetForPooling();
                else if (btn.TryGetComponent<AnomalyButtonUI>(out var anomalyButton)) anomalyButton.ResetForPooling();
                else if (btn.TryGetComponent<Button>(out var button)) button.onClick.RemoveAllListeners();

                PrefabPool.Release(ref btn);
            }
            activeRewardButtons.Clear();
        }
        public virtual int GetCurrentWave() => currentWaveIndex + currentSequence.waveOffset;
        protected void CachePlayerStatManager()
        {
            cpsm ??= GameObject.FindWithTag("Player")?.GetComponent<IStatProvider>();
            cich ??= GameObject.FindWithTag("Player")?.GetComponent<ICurrencyHolder>();
        }
        protected void CachePlayerSkillTree()
        {
            if (cpst == null)
                cpst = GameObject.FindWithTag("Player")?.GetComponent<ISkillPointHolder>();
        }
        protected string BuildChangeLine(StatType type, float finalVal)
        {
            StatType effType = type switch
            {
                StatType.attack => StatType.EffAtk,
                StatType.atkPct => StatType.EffAtk,
                StatType.maxHp => StatType.EffMaxHp,
                StatType.hpPct => StatType.EffMaxHp,
                StatType.hpRegen => StatType.EffHpReg,
                StatType.hpRegPct => StatType.EffHpReg,
                StatType.armor => StatType.EffArmor,
                StatType.armorPct => StatType.EffArmor,
                StatType.moveSpeed => StatType.EffSpd,
                StatType.moveSpeedPct => StatType.EffSpd,
                StatType.Intelligence => StatType.EffInt,
                StatType.IntPct => StatType.EffInt,
                StatType.staminaRegen => StatType.EffStReg,
                StatType.stRegPct => StatType.EffStReg,
                StatType.maxStamina => StatType.EffMaxStamina,
                StatType.maxStaminaPct => StatType.EffMaxStamina,
                StatType.maxManaPct => StatType.EffMaxMana,
                StatType.maxMana => StatType.EffMaxMana,
                _ => type,
            };

            if (cpsm == null) return "";

            float before = cpsm.GetStat(effType);
            cpsm.AddStat(new StatBuff(type, finalVal));

            float after = cpsm.GetStat(effType);
            cpsm.AddStat(new StatBuff(type, finalVal), false);

            return $"{before:F2} → {after:F2}";
        }
        protected void SortRarityData() => rarityData.Sort((a, b) => a.mult.CompareTo(b.mult));
        public void CloseAllButtons()
        {
            ClearRewardButtons();

            foreach (var b in actionButtonContainer.GetComponentsInChildren<Button>())
                b.gameObject.SetActive(false);
        }
    }
}
