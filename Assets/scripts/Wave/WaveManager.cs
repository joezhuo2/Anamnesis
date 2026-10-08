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
    public partial class WaveManager : MonoBehaviour
    {
        protected static WaveManager ActiveManager;

        [Header("Difficulty")]
        public DifficultyData difficulty;
        public ModeData mode;

        [Header("Reroll Settings")]
        public int rerollGoldCost = 200;

        [Header("Basic Settings")]
        public WaveSequence currentSequence;
        public float spawnRadius = 2f;
        [Range(0f, 0.9f)] public float killSpawnSpeedup = 0.15f;
        public bool enableExtraSpawns = true;
        public int totalWaves;

        protected bool showCompletionMessage => GameSettings.Current.showWaveCompletionMessage;

        [Header("Wave Info Settings")]
        public GameObject waveInfoPanel;
        public TextMeshProUGUI anomalyInfoText;
        public GameObject anomalyInfoBackground;
        public TextMeshProUGUI contractInfoText;
        public GameObject contractInfoBackground;
        private string lastInfo;
        private int lastInfoTick = int.MinValue;
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
        public string contractTitle = "Select contract";

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

        [Header("Synergy Settings")]
        public List<SynergyStatOption> synergyStatPool = new();
        public float minSynergyConversion = 8f;
        public float maxSynergyConversion = 20f;
        public int synergyChoices = 3;
        public int synergyMinWave = 10;
        public float synergyBaseChance = 4f;
        public float synergyChanceGrowth = 4f;
        public string synergyTitle = "Choose a synergy";

        [Header("Anomaly Settings")]
        public List<AnomalyData> availableAnomalies = new();
        public GameObject anomalyPrefab = null;
        public AnomalyInstance currentAnomaly = null;
        public int minAnomalyCount = 2;
        public int maxAnomalyCount = 5;
        public float anomalyChance = 15;
        public float anomalyGlobalMinWave = 10;
        public GameObject duelBossBarPrefab = null;
        public GameObject duelStatusEffectPrefab = null;

        [Header("Contract Settings")]
        public GameObject contractPrefab = null;
        public AnomalyInstance currentContract = null;
        public int minContractCount = 2;
        public int maxContractCount = 3;

        protected bool IsDuel => currentAnomaly != null && currentAnomaly.isActive && currentAnomaly is DuelInstance
            || contractArmed && currentContract is DuelInstance;
        protected float EnemyCountMult => (currentAnomaly != null && currentAnomaly.isActive && currentAnomaly is SwarmInstance sw ? sw.CountMultiplier : 1f)
            * (contractArmed && currentContract is SwarmInstance csw ? csw.CountMultiplier : 1f);
        protected HivemindInstance Hivemind => currentAnomaly is HivemindInstance h && h.isActive ? h
            : contractArmed && currentContract is HivemindInstance ch && ch.isActive ? ch : null;
        protected StampedeInstance Stampede => currentAnomaly is StampedeInstance s && s.isActive ? s
            : contractArmed && currentContract is StampedeInstance cs && cs.isActive ? cs : null;
        protected TwinCrownsInstance Twin => currentAnomaly is TwinCrownsInstance t && t.isActive ? t : null;
        protected int BossCount => Twin != null ? 2 : 1;
        protected float SpawnRadius => Stampede is { } st && st.SpawnRadius > 0f ? st.SpawnRadius : spawnRadius;
        protected bool IsDrought => currentAnomaly is DroughtInstance && currentAnomaly.isActive
            || contractArmed && currentContract is DroughtInstance && currentContract.isActive;
        protected bool ContractHeld => currentContract != null && currentContract.amd != null && (contractPaused || contractArmed && currentContract.isActive);
        protected bool ContractBroken => contractArmed && currentContract != null && !currentContract.isActive;

        protected IAnnouncer GameController => IAnnouncer.Current ?? null;
        protected DifficultyData D => difficulty != null ? difficulty : DifficultyData.Neutral;
        protected ModeData M => mode != null ? mode : ModeData.Neutral;
        protected bool CorruptionAllowed => !IronmanSelector.Enabled && M.allowCorruption;
        protected bool anomalyButtonsOpen;
        protected bool contractButtonsOpen;
        protected bool RerollLocked => IronmanSelector.Enabled || anomalyButtonsOpen && D.lockAnomalyChoice;
        protected bool SkipLocked => anomalyButtonsOpen && (D.lockAnomalyChoice || M.lockAnomalySkip) || contractButtonsOpen && M.lockAnomalySkip;
        protected float Quality => additionalQuality + D.qualityBonusAdd;
        protected int RerollGoldCost => Mathf.Max(0, rerollGoldCost + D.rerollGoldCostAdd);
        protected RewardType type = RewardType.Basic;
        protected GameObject activeBossBar;
        protected IStatProvider cpsm;
        protected ICurrencyHolder cich;
        protected IAttackHandler cpah;
        protected IUpgradeHolder cpum;
        protected ISkillPointHolder cpst;
        protected PlayerResourcePool cprp;
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
        protected AnomalyType? contractType;
        protected bool contractArmed;
        protected bool contractPaused;
        protected int pendingContractRerolls;
        protected int pendingContractSkillPoints;
        protected bool pendingContractPool;
        protected bool pendingAnomalyRewards;
        private int lastContractKey = int.MinValue;
        protected readonly List<GameObject> activeRewardButtons = new();
        protected readonly List<StatSynergy> synergiesThisRoll = new();
        protected StatSynergyManager cssm;
        protected float synergyChanceBonus = 0f;
        protected bool pendingSynergy = false;
        protected RewardButton keptReward;
        protected int LockedSlots => keptReward != null ? 1 : 0;
        protected static readonly WaitForSeconds _waitForSeconds1_5 = new(1.5f);
        protected static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
        protected static readonly WaitForSeconds _waitForSeconds0_25 = new(0.25f);

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

            if (!IronmanSelector.Enabled) rerolls = Mathf.Max(0, rerolls + d.startingRerollsAdd);
            else rerolls = 0;

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

        public virtual void ApplyMode(ModeData m)
        {
            if (m == null) return;

            mode = m;
            RunMode.Tier = m.tier;
            RunMode.UltimatesUnlocked = m.unlockUltimates;

            CachePlayerAttackHandler();
            if (cpah != null)
            {
                if (!m.unlockUltimates)
                {
                    cpah.RemoveAttack(AttackType.Ultimate);
                    if (cpah is PlayerAttackHandler pah) pah.PermaLockSlot(AttackType.Ultimate);
                }
                else if (m.startingUlt != null) cpah.UpdateAttack(AttackType.Ultimate, m.startingUlt);
            }

            SetupActionButtonTooltips();
        }

        private void SetupActionButtonTooltips()
        {
            if (!IronmanSelector.Enabled)
            {
                if (rerollButton != null && rerollButton.TryGetComponent<ITooltipDisplay>(out var td))
                    td.ShowTooltip("Reroll", $"Rerolls all unlocked reward choices.\nCost: 1 reroll token or {RerollGoldCost} gold if none are available.");
                if (CorruptionAllowed && corruptButton != null && corruptButton.TryGetComponent<ITooltipDisplay>(out var td2))
                    td2.ShowTooltip("Corrupt", "Chance to corrupt any rewards massively increase or decrease their values.\nCan only be used once per wave and removes all other options.");
            }
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

            if (currentContract != null)
            {
                currentContract.Cleanup();
                currentContract = null;
            }

            ClearRewardButtons();

            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            
            if (contractInfoBackground != null) contractInfoBackground.SetActive(false);
            if (anomalyInfoBackground != null) anomalyInfoBackground.SetActive(false);
        }
        
        private void Update()
        {
            if (currentAnomaly != null && currentAnomaly.isActive)
            {
                currentAnomaly.UpdateCheck(Time.deltaTime);

                if (anomalyInfoText != null)
                {
                    if (anomalyInfoBackground != null) anomalyInfoBackground.SetActive(true);
                    switch (currentAnomaly.amd.anomalyType)
                    {
                        case AnomalyType.TimeTrial: UpdateAnomalyTimeInfo(); break;
                        case AnomalyType.NoDamage: SetAnomalyInfo("No Damage Anomaly Active"); break;
                        case AnomalyType.StatModifier:
                        case AnomalyType.Swarm:
                        case AnomalyType.Duel:
                        case AnomalyType.Split:
                        case AnomalyType.Sealed:
                        case AnomalyType.Blackout:
                        case AnomalyType.Hivemind:
                        case AnomalyType.Drought:
                        case AnomalyType.Precision:
                        case AnomalyType.Overcharged:
                        case AnomalyType.Stampede:
                        case AnomalyType.TwinCrowns:
                        case AnomalyType.Rampage:
                        case AnomalyType.Unstoppable: SetAnomalyInfo(currentAnomaly.Description); break;
                        default: break;
                    }
                }
            }
            else
            {
                anomalyInfoBackground.SetActive(false);
                lastInfoTick = int.MinValue;
                if (anomalyInfoText != null) SetAnomalyInfo("");
            }

            if (contractArmed && currentContract != null && currentContract.isActive) currentContract.UpdateCheck(Time.deltaTime);
            UpdateContractInfo();
        }

        protected GameObject GetOrCreateRewardButton()
        {
            Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
            GameObject btnObj = PrefabPool.Acquire(rewardButtonPrefab, targetParent);

            if (btnObj != null) activeRewardButtons.Add(btnObj);
            RefreshLockButtons();
            return btnObj;
        }

        protected RewardButton GetLockedReward()
        {
            for (int i = 0; i < activeRewardButtons.Count; i++)
            {
                GameObject go = activeRewardButtons[i];
                if (go != null && go.TryGetComponent<RewardButton>(out var rb) && rb.IsLocked) return rb;
            }
            return null;
        }

        protected void RefreshLockButtons()
        {
            bool show = !IronmanSelector.Enabled && type != RewardType.Anomaly && type != RewardType.Contract && activeRewardButtons.Count > 1;
            RewardButton lk = show ? GetLockedReward() : null;

            for (int i = 0; i < activeRewardButtons.Count; i++)
            {
                GameObject go = activeRewardButtons[i];
                if (go == null || !go.TryGetComponent<RewardButton>(out var rb)) continue;

                rb.SetupLock(show, OnRewardLockToggled);
                rb.SetLockInteractable(lk == null || lk == rb);
            }
        }

        protected void HideLockButtons()
        {
            for (int i = 0; i < activeRewardButtons.Count; i++)
            {
                GameObject go = activeRewardButtons[i];
                if (go != null && go.TryGetComponent<RewardButton>(out var rb)) rb.SetupLock(false, null);
            }
        }

        protected void OnRewardLockToggled(RewardButton rb)
        {
            if (IronmanSelector.Enabled || rb == null) return;

            RewardButton lk = GetLockedReward();
            if (lk != null && lk != rb) return;

            rb.SetLocked(!rb.IsLocked);
            RefreshLockButtons();
        }
        protected void GenerateMilestoneRewards()
        {
            type = RewardType.Milestone;
            int rewardChoices = Mathf.Min(Mathf.Max(1, milestoneRewardChoices + D.milestoneRewardChoicesAdd), milestoneRewards.Count) - LockedSlots;

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
            if (keptReward != null && keptReward.Milestone != null)
            {
                string kn = keptReward.Milestone.rewardName;
                available.RemoveAll(r => r.rewardName == kn);
            }

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
        protected void RollSynergyOffer(int w)
        {
            if (pendingSynergy || w < synergyMinWave || !HasSynergyChoices()) return;

            if (Random.Range(0f, 100f) < synergyBaseChance + synergyChanceBonus)
            {
                pendingSynergy = true;
                synergyChanceBonus = 0f;
            }
            else synergyChanceBonus += synergyChanceGrowth;
        }

        protected bool CanOfferSynergy(StatType src, StatType tgt)
        {
            if (StatSynergy.Effective(src) == StatSynergy.Effective(tgt)) return false;
            if (cssm != null && cssm.HasSynergy(StatSynergy.Effective(src), tgt)) return false;

            for (int i = 0; i < synergiesThisRoll.Count; i++)
                if (synergiesThisRoll[i].source == StatSynergy.Effective(src) && synergiesThisRoll[i].target == tgt) return false;

            return true;
        }

        protected bool HasSynergyChoices()
        {
            if (synergyStatPool == null || synergyStatPool.Count < 2) return false;
            CachePlayerSynergy();
            synergiesThisRoll.Clear();

            foreach (var a in synergyStatPool)
            {
                if (a == null || !a.canBeSource || a.weight <= 0f) continue;
                foreach (var b in synergyStatPool)
                    if (b != null && b.canBeTarget && b.weight > 0f && CanOfferSynergy(a.stat, b.stat)) return true;
            }
            return false;
        }

        protected SynergyStatOption PickSynergyOption(bool asSource, StatType? src = null)
        {
            SynergyStatOption chosen = null;
            float total = 0f;

            foreach (var o in synergyStatPool)
            {
                if (o == null || o.weight <= 0f) continue;
                if (asSource ? !o.canBeSource : !o.canBeTarget) continue;
                if (src.HasValue && !CanOfferSynergy(src.Value, o.stat)) continue;

                total += o.weight;
                if (Random.Range(0f, total) < o.weight) chosen = o;
            }

            return chosen;
        }

        protected SynergyRewardData GenerateSynergyRewardData()
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                SynergyStatOption src = PickSynergyOption(true);
                if (src == null) return null;

                SynergyStatOption tgt = PickSynergyOption(false, src.stat);
                if (tgt == null) continue;

                float lo = Mathf.Min(minSynergyConversion, maxSynergyConversion);
                float hi = Mathf.Max(minSynergyConversion, maxSynergyConversion);
                float pct = Mathf.Round(Random.Range(lo, hi) * 10f) * 0.1f;

                StatSynergy sy = new(StatSynergy.Effective(src.stat), tgt.stat, pct);
                synergiesThisRoll.Add(sy);
                return new SynergyRewardData { sy = sy };
            }

            return null;
        }

        protected void GenerateSynergyRewards()
        {
            type = RewardType.Synergy;
            PanelSetup();
            CachePlayerStatManager();
            CachePlayerSynergy();
            synergiesThisRoll.Clear();
            if (keptReward != null && keptReward.Synergy?.sy != null) synergiesThisRoll.Add(keptReward.Synergy.sy);

            int choices = Mathf.Max(1, synergyChoices) - LockedSlots;

            for (int i = 0; i < choices; i++)
            {
                SynergyRewardData data = GenerateSynergyRewardData();
                if (data == null) break;

                GameObject btnObj = GetOrCreateRewardButton();
                if (btnObj == null) continue;

                string changeLine = "";
                if (cpsm != null)
                {
                    float gain = cpsm.GetStat(data.sy.source) * data.sy.pct * 0.01f;
                    if (StatSynergy.IsWholeStat(data.sy.target)) gain = Mathf.Floor(gain);
                    changeLine = $"Now: +{gain:0.##} {StatSynergy.StatName(data.sy.target)}";
                }

                if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(data, OnSynergyRewardClaimed, changeLine);
            }
        }

        protected void OnSynergyRewardClaimed(SynergyRewardData chosen)
        {
            CloseRewardUI();

            CachePlayerSynergy(true);
            if (cssm != null && chosen?.sy != null) cssm.AddSynergy(chosen.sy);

            ResumeGameLoop();
        }

        protected void CachePlayerSynergy(bool create = false)
        {
            if (cssm != null) return;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            if (!player.TryGetComponent(out cssm) && create) cssm = player.AddComponent<StatSynergyManager>();
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

            rewardChoices = Mathf.Max(1, rewardChoices + D.rewardChoicesAdd) - LockedSlots;

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
            rewardTitleText.text = type switch
            {
                RewardType.Anomaly => anomalyTitle,
                RewardType.Contract => contractTitle,
                RewardType.Synergy => synergyTitle,
                _ => rewardTitle
            };
        }

        private void UpdateCorruptButton()
        {
            if (corruptButton == null) return;

            bool allowed = CorruptionAllowed && type != RewardType.Anomaly && type != RewardType.Milestone && type != RewardType.PreRun && type != RewardType.Synergy && type != RewardType.Contract && GetCurrentWave() % 5 != 0;
            corruptButton.gameObject.SetActive(allowed);
        }

        public static bool WaveActive => ActiveManager != null && ActiveManager.isWaveActive;
        public static bool DroughtActive => ActiveManager != null && ActiveManager.isWaveActive && ActiveManager.IsDrought;
        public static int WaveEnemyTotal => ActiveManager != null ? ActiveManager.waveMaxTotalEnemies : 0;

        public static bool GrantRerolls(int amount)
        {
            if (amount <= 0 || ActiveManager == null || IronmanSelector.Enabled) return false;

            ActiveManager.rerolls += amount;
            ActiveManager.UpdateRerollUI();
            return true;
        }

        protected void UpdateRerollUI()
        {
            if (rerollText != null) rerollText.gameObject.SetActive(!RerollLocked);

            if (RerollLocked)
            {
                if (rerollButton != null) rerollButton.gameObject.SetActive(false);
                return;
            }

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

            if (SkipLocked) return;

            if (type == RewardType.Contract)
            {
                ContinueAfterContract();
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

            if (RerollLocked) return;

            CachePlayerStatManager();

            if (type == RewardType.Anomaly && !HasAnomalyChoices()) return;
            if (type == RewardType.Contract && !HasContractChoices()) return;
            if (type == RewardType.PreRun && !HasPreRunChoices()) return;
            if (type == RewardType.Synergy && !HasSynergyChoices()) return;

            if (rerolls > 0)
                rerolls--;
            else if (cich == null || !cich.TrySpend(RerollGoldCost)) return;

            UpdateRerollUI();

            keptReward = GetLockedReward();
            int ki = keptReward != null ? activeRewardButtons.IndexOf(keptReward.gameObject) : -1;
            if (ki >= 0) activeRewardButtons.RemoveAt(ki);
            else keptReward = null;

            ClearRewardButtons();

            switch (type)
            {
                case RewardType.Anomaly: GenerateAnomalyChoices(); break;
                case RewardType.Contract: GenerateContractChoices(); break;
                case RewardType.PreRun: GeneratePreRunPicks(); break;
                case RewardType.Basic: GenerateRewards(); break;
                case RewardType.Rare: GenerateRarePool(); break;
                case RewardType.Treasure: GenerateTreasurePool(); break;
                case RewardType.Mixed: GenerateMixedPool(); break;
                case RewardType.Milestone: GenerateMilestoneRewards(); break;
                case RewardType.Synergy: GenerateSynergyRewards(); break;
                default: break;
            }

            RestoreKeptReward(ki);
        }

        protected void RestoreKeptReward(int ki)
        {
            if (keptReward == null) return;

            RewardButton kr = keptReward;
            keptReward = null;
            kr.SetLocked(false);

            int idx = Mathf.Clamp(ki, 0, activeRewardButtons.Count);
            if (idx < activeRewardButtons.Count && activeRewardButtons[idx] != null)
                kr.transform.SetSiblingIndex(activeRewardButtons[idx].transform.GetSiblingIndex());
            else
                kr.transform.SetAsLastSibling();

            activeRewardButtons.Insert(idx, kr.gameObject);
            RefreshLockButtons();
        }

        public void OnCorruptButtonClicked()
        {
            if (!CorruptionAllowed) return;

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
                if (!rb.TryGetComponent<RewardButton>(out var grb) || grb.IsLocked) continue;

                if (Random.value > (cChance * 0.01f)) continue;

                GeneratedReward gr = grb.gr;
                if (gr == null) continue;

                if (M.allowCorruptionSpecials && Random.value < (corruptionSpecialChance * 0.01f))
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

            HideLockButtons();

            if (rerollButton != null) rerollButton.gameObject.SetActive(false);
            if (corruptButton != null) corruptButton.gameObject.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);
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

        public void OpenAnomalyButtons() => OpenActionButtons(true);

        public void OpenRewardButtons() => OpenActionButtons(false);

        public void OpenContractButtons() => OpenActionButtons(false, true);

        protected void OpenActionButtons(bool anomaly, bool contract = false)
        {
            anomalyButtonsOpen = anomaly;
            contractButtonsOpen = contract;
            if (rerollButton != null) rerollButton.gameObject.SetActive(!RerollLocked);
            if (skipButton != null) skipButton.gameObject.SetActive(!SkipLocked);
            UpdateRerollUI();
        }

        protected void ResumeGameLoop()
        {
            if (pendingAnomalyRewards)
            {
                pendingAnomalyRewards = false;
                CleanupAnomaly();
            }
            else if (pendingStandardRewards)
            {
                TriggerStandardRewards(GetCurrentWave());
            }
            else if (pendingSynergy)
            {
                pendingSynergy = false;
                OpenRewardButtons();
                UpdateRerollUI();
                GenerateSynergyRewards();
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
                else if (btn.TryGetComponent<ContractButtonUI>(out var contractButton)) contractButton.ResetForPooling();
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
        protected void CachePlayerAttackHandler()
        {
            cpah ??= GameObject.FindWithTag("Player")?.GetComponent<IAttackHandler>();
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
                StatType.defense => StatType.EffDefense,
                StatType.defensePct => StatType.EffDefense,
                StatType.arcaneShield => StatType.EffArcaneShield,
                StatType.arcaneShieldPct => StatType.EffArcaneShield,
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
