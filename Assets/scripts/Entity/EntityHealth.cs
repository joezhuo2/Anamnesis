using System;
using System.Collections;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SettingsSystem;
using CrystalFlux.StatusEffectSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public partial class EntityHealth : MonoBehaviour, IDamageable
    {
        public float deathAnimTime = 1f;
        public Slider healthBarPrefab;
        public Vector3 healthBarOffset = new(0, 0, 0);
        public TextMeshProUGUI healthBarTextPrefab;

        public event Action<GameObject> OnDeath;
        public static Func<EntityHealth, int, bool> DamageRedirect;
        [HideInInspector] public float DamageTakenMult = 1f;
        private static readonly int IsDeadHash = Animator.StringToHash("isDead");
        private static readonly int IsHurtHash = Animator.StringToHash("isHurt");
        private bool _isTriggeringOnDealDamage;
        private GameObject killSrc;
        private bool _suppressHurtIFrames;
        private bool _pendingHurtIFrames;
        private float regenTimer;
        private const float regenInterval = 0.5f;
        private const float fullRegenFrequency = 5f;
        private const float hurtIFrameDuration = 0.4f;
        private float accumulatedRegen;
        private float overhealthConvPct;
        private float overhealthDecayPct;
        private float overhealthDecayInterval;
        private float overhealthDecayTimer;
        private bool regenOverHealth;
        private float immunityEndTime;
        private bool iFramesActive;
        private float hurtResetTime;
        private bool hurtPending;
        private Animator animator;
        private Slider healthBarInstance;
        private TextMeshProUGUI healthBarTextInstance;
        private Camera mainCamera;
        private PlayerUpgradeManager cpum;
        private IStatProvider esm;
        private ITeamMember ownTeam;
        private ICastHandler castHandler;
        private IStatusEffectReceiver ownSem;
        private EnemyPhase phase;
        private bool isMirage;
        private Canvas cachedCanvas;
        public bool IsAlive => esm != null && esm.GetStat(StatType.isAlive) > 0f;
        private bool Immune => esm != null && esm.GetStat(StatType.isImmune) > 0f;
        public float Overhealth => esm != null ? esm.GetStat(StatType.overhealth) : 0f;
        private int CurHp => esm != null ? Mathf.RoundToInt(esm.GetStat(StatType.currentHp)) : 0;
        private int MaxHp => esm != null ? Mathf.RoundToInt(esm.GetStat(StatType.EffMaxHp)) : 0;

        private void Start()
        {
            esm = GetComponent<IStatProvider>();
            animator = GetComponent<Animator>();
            mainCamera = Camera.main;
            isPlayerEntity = CompareTag("Player");

            regenTimer = 0f;
            accumulatedRegen = 0f;
            overhealthDecayTimer = 0f;

            if (esm == null)
            {
                Debug.LogError($"EntityHealth on '{name}' found no IStatProvider.", this);
                return;
            }

            esm.AddStat(new StatBuff(StatType.isAlive, 1f));
            esm.AddStat(new StatBuff(StatType.CanGainHp, 1f));

            isMirage = TryGetComponent<MirageClone>(out _);
            if (!isMirage && TryGetComponent<PlayerUpgradeManager>(out var pum)) cpum = pum;
            TryGetComponent(out ownTeam);
            TryGetComponent(out castHandler);
            TryGetComponent(out ownSem);
            TryGetComponent(out phase);
        }

        public const string HealthBarCanvasName = "HealthBarCanvas";
        private const int healthBarSortingOrder = -1;
        private static Canvas sharedCanvas;
        private bool barRetired;
        private bool isPlayerEntity;
        private bool BarsAllowed => isPlayerEntity || GameSettings.Current.showEnemyHealthBars;
        private int barCurHp = int.MinValue;
        private int barMaxHp = int.MinValue;
        private int barOverhealth = int.MinValue;
        private Vector3 lastBarWorldPos = new(float.NaN, float.NaN, float.NaN);
        private Vector3 lastBarCamPos = new(float.NaN, float.NaN, float.NaN);
        private const float barMoveEpsilonSqr = 1e-6f;
        private bool barHidden;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            sharedCanvas = null;
            DamageRedirect = null;
        }

        private void Update()
        {
            TickTimers();
            RegenHp();
            DecayOverhealth();
            MoveHealthBar();
        }

        private void OnDestroy()
        {
            barRetired = true;
            PrefabPool.Release(ref healthBarInstance);
            PrefabPool.Release(ref healthBarTextInstance);
        }
        public float FinalPhaseHpPct(float fallback)
        {
            if (phase == null) TryGetComponent(out phase);
            return phase != null && phase.phaseThresholds != null && phase.phaseThresholds.Length > 0 ? phase.phaseThresholds[phase.phaseThresholds.Length - 1] : fallback;
        }

        public void SetHpPct(float pct)
        {
            if (esm == null || !IsAlive) return;

            int target = Mathf.Clamp(Mathf.FloorToInt(MaxHp * pct * 0.01f), 1, MaxHp);
            esm.AddStat(new StatBuff(StatType.currentHp, target - CurHp));
            UpdatePhase();
        }

        private void UpdatePhase()
        {
            if (phase == null) return;

            float hpPct = (float)CurHp / MaxHp * 100f;
            int newPhase = 0;
            for (int i = 0; i < phase.phaseThresholds.Length; i++)
            {
                if (hpPct <= phase.phaseThresholds[i]) newPhase = i + 1;
                else break;
            }
            phase.UpdatePhase(newPhase);
        }

        private void StartDeathSequence()
        {
            AddOverhealth(-Overhealth);
            esm.AddStat(new StatBuff(StatType.isAlive, -1));

            if (cpum != null) cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnDeath);

            OnDeath?.Invoke(gameObject);

            TrySplit();

            if (killSrc != null && killSrc.TryGetComponent<PlayerUpgradeManager>(out var killPum))
                killPum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnKill, gameObject);

            if (ownSem != null)
                ownSem.ClearAllEffects();

            barRetired = true;
            PrefabPool.Release(ref healthBarInstance);
            PrefabPool.Release(ref healthBarTextInstance);

            if (animator != null && !IsAlive && deathAnimTime > 0f)
            {
                animator.SetBool(IsDeadHash, true);
                StartCoroutine(DeathDelay(deathAnimTime));
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void DropGold(GameObject target)
        {
            if (target == null) return;
            if (!GameSettings.Current.goldDropsEnabled) return;
            if (esm.GetStat(StatType.goldDrop) <= 0) return;

            float stealing = target.TryGetComponent<IStatProvider>(out var tsm) ? tsm.GetStat(StatType.Stealing) : 0f;

            int gold = Mathf.RoundToInt(esm.GetStat(StatType.goldDrop) * UnityEngine.Random.Range(0.7f, 1.3f) * (1f + (stealing * 0.01f)));

            if (gold > 0 && target.TryGetComponent<ICurrencyHolder>(out var ich))
            {
                ich.AddCurrency(gold);

                TextIndicatorSpawner tis = TextIndicatorSpawner.Instance;
                if (tis != null)
                {
                    Color goldColor = new(1f, 0.843f, 0f);
                    tis.SpawnTextIndicator(
                        gold,
                        transform.position,
                        goldColor,
                        0.7f + UnityEngine.Random.Range(0f, 0.15f),
                        UnityEngine.Random.Range(0.5f, 0.7f),
                        UnityEngine.Random.Range(0.8f, 1.2f),
                        UnityEngine.Random.Range(0f, 0.2f),
                        TextType.Gold
                    );
                }
            }
        }

        private void TrySplit()
        {
            if (TryGetComponent<EntitySplitting>(out var splitting))
                splitting.Split();
        }

        private void TickTimers()
        {
            float now = Time.time;

            if (hurtPending && now >= hurtResetTime)
            {
                hurtPending = false;
                if (animator != null) animator.SetBool(IsHurtHash, false);
            }

            if (iFramesActive && now >= immunityEndTime)
            {
                iFramesActive = false;
                if (esm != null) esm.AddStat(new StatBuff(StatType.isImmune, -1f));
            }
        }

        public void TriggerIFrames(float duration)
        {
            if (esm == null) return;

            float end = Time.time + duration;
            if (iFramesActive)
            {
                if (end > immunityEndTime) immunityEndTime = end;
                return;
            }

            iFramesActive = true;
            immunityEndTime = end;
            esm.AddStat(new StatBuff(StatType.isImmune, 1f));
        }

        private IEnumerator DeathDelay(float delay)
        {
            yield return null;
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}
