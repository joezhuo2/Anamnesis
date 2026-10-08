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

        public static bool WaveActive => ActiveManager != null && ActiveManager.isWaveActive;
        public static bool DroughtActive => ActiveManager != null && ActiveManager.isWaveActive && ActiveManager.IsDrought;
        public static int WaveEnemyTotal => ActiveManager != null ? ActiveManager.waveMaxTotalEnemies : 0;
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
    }
}
