using System;
using System.Collections;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public class EntityHealth : MonoBehaviour, IDamageable
    {

        public event Action<GameObject> OnDeath;
        private static readonly int IsDeadHash = Animator.StringToHash("isDead");
        private static readonly int IsHurtHash = Animator.StringToHash("isHurt");
        private bool _isTriggeringOnDealDamage;
        private float regenTimer;
        private const float regenInterval = 0.5f;
        private const float fullRegenFrequency = 5f;
        private const float hurtIFrameDuration = 0.2f;
        private float accumulatedRegen;
        private Animator animator;
        public Slider healthBarPrefab;
        private Slider healthBarInstance;
        public Vector3 healthBarOffset = new(0, 0, 0);
        public TextMeshProUGUI healthBarTextPrefab;
        private TextMeshProUGUI healthBarTextInstance;
        private Camera mainCamera;
        private PlayerUpgradeManager cpum;
        private IStatProvider esm;
        private Canvas cachedCanvas;
        public bool IsAlive => esm != null && esm.GetStat(StatType.isAlive) > 0f;
        private bool Immune => esm != null && esm.GetStat(StatType.isImmune) > 0f;
        private int CurHp => esm != null ? Mathf.RoundToInt(esm.GetStat(StatType.currentHp)) : 0;
        private int MaxHp => esm != null ? Mathf.RoundToInt(esm.GetStat(StatType.EffMaxHp)) : 0;

        private void Start()
        {
            esm = GetComponent<IStatProvider>();
            animator = GetComponent<Animator>();
            mainCamera = Camera.main;

            regenTimer = 0f;
            accumulatedRegen = 0f;

            esm.AddStat(new StatBuff(StatType.isAlive, 1f));
            esm.AddStat(new StatBuff(StatType.CanGainHp, 1f));

            if (TryGetComponent<PlayerUpgradeManager>(out var pum)) cpum = pum;

            InitializeHealthBar();
        }

        private static Canvas sharedCanvas;

        private void InitializeHealthBar()
        {
            if (healthBarPrefab == null) return;

            if (sharedCanvas == null) sharedCanvas = FindAnyObjectByType<Canvas>();
            cachedCanvas = sharedCanvas;

            if (cachedCanvas == null) return;

            healthBarInstance = Instantiate(healthBarPrefab, cachedCanvas.transform);

            healthBarInstance.maxValue = MaxHp;
            healthBarInstance.value = CurHp;

            if (healthBarTextPrefab == null) return;

            healthBarTextInstance = Instantiate(healthBarTextPrefab, cachedCanvas.transform);
            healthBarTextInstance.text = $"{CurHp}/{MaxHp}";
        }

        private void Update()
        {
            RegenHp();
            MoveHealthBar();
        }
        private void MoveHealthBar()
        {
            if (healthBarInstance == null || mainCamera == null || !IsAlive) return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position + healthBarOffset);
            healthBarInstance.transform.position = screenPos;

            if (healthBarTextInstance != null)
                healthBarTextInstance.transform.position = screenPos + healthBarOffset;
        }

        public void TakeDamage(DamagePacket dp)
        {
            if (dp == null) return;

            foreach (var i in dp.instances)
            {
                IStatProvider atk = i.owner != null && i.owner.TryGetComponent<IStatProvider>(out var osm) ? osm : null;

                var (dmg, sizeMult) = i.type switch
                {
                    DamageType.True => (i.amount, 1f),
                    DamageType.Physical => DamageCalculator.CalculateDamageTaken(i.type, i.amount, esm, atk),
                    DamageType.Spell => DamageCalculator.CalculateDamageTaken(i.type, i.amount, esm, atk),
                    DamageType.DoT => (i.amount * (1f - (esm.GetStat(StatType.EffectRes) * 0.01f)), 1f),
                    DamageType.Heal => (-i.amount, 1f),
                    DamageType.Consume => (i.amount, 1f),
                    _ => (0f, 1f)
                };

                if (Immune && !dp.bypassIFrames && dmg > 0) continue;

                Color color = i.indicatorColor != default ? i.indicatorColor : i.type switch
                {
                    DamageType.Physical => Color.gray,
                    DamageType.Spell => Color.purple,
                    DamageType.True => Color.lightBlue,
                    _ => Color.white
                };

                PlayerUpgradeManager pum = null;
                if (i.owner != null) i.owner.TryGetComponent(out pum);

                if (pum != null)
                    pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnTargetRecievedHit);

                if (cpum != null && dmg > 0)
                    cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnTakeDamage);

                if (cpum != null && dmg > 0 && IsEnemyHit(dp, i))
                    cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnTakeHit);

                if (i.isCrit)
                {
                    if (pum!= null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnCrit);

                    sizeMult *= 1.5f;
                }

                if (dp.sizeOverride != 1f) sizeMult = dp.sizeOverride;

                if (ChangeHealth(-dmg, true, sizeMult, color, dp.bypassIFrames, dp.source))
                {
                    if (dp.source != null && dp.source.TryGetComponent<PlayerLevel>(out var pl))
                        pl.GainExp(esm.GetStat(StatType.XpDrop) * (Mathf.Pow(1.05f, esm.GetStat(StatType.Level) - 1)) * UnityEngine.Random.Range(0.8f, 1.2f));
                    DropGold(dp.source);

                    if (dp.source != null && dp.source != gameObject && dp.source.TryGetComponent<PlayerUpgradeManager>(out var killPum))
                        killPum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnKill);
                }

                if (!_isTriggeringOnDealDamage && pum != null)
                {
                    _isTriggeringOnDealDamage = true;
                    pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnDealDamage, gameObject, dmg);
                    _isTriggeringOnDealDamage = false;
                }
            }

            if (cpum != null) PlayerEvents.RaisePlayerTakeDamage(this);
        }

        private bool IsEnemyHit(DamagePacket dp, DamageInstance i)
        {
            if (dp.bypassIFrames) return false;
            if (i.type != DamageType.Physical && i.type != DamageType.Spell && i.type != DamageType.True) return false;
            if (i.owner == null || i.owner == gameObject) return false;

            int ownTeam = TryGetComponent<ITeamMember>(out var itm) ? itm.TeamID : 0;
            int atkTeam = i.owner.TryGetComponent<ITeamMember>(out var oitm) ? oitm.TeamID : 0;
            return ownTeam != atkTeam;
        }

        public bool ChangeHealth(float amount, bool showIndicator = true, float sizeMult = 1f, Color colorOverride = default, bool bypassIFrames = false, GameObject source = null)
        {
            int finalAmount = Mathf.RoundToInt(amount);
            if (finalAmount == 0) return false;

            if (cpum != null && finalAmount < 0 && Immune && esm.GetStat(StatType.IsDashing) > 0f)
            {
                cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnCounterDodge);
                return false;
            }
            if (finalAmount < 0 && Immune) return false;
            if (finalAmount > 0 && (esm.GetStat(StatType.CanGainHp)) <= 0f) return false;

            if (finalAmount < 0 && CurHp > 0 && Mathf.Abs(finalAmount) >= CurHp * 3f)
            {
                if (source != null && source.TryGetComponent<PlayerUpgradeManager>(out var pum))
                    pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnOverkill);
            }

            int targetChange = finalAmount;
            if (targetChange > 0) targetChange = Mathf.Min(targetChange, MaxHp - CurHp);
            esm.AddStat(new StatBuff(StatType.currentHp, targetChange));

            UpdatePhase();

            TextIndicatorSpawner tis = TextIndicatorSpawner.Instance;
            Vector3 pos = transform.position;

            if (tis != null && showIndicator)
            {
                Color indicatorColor = colorOverride != default ? colorOverride : (finalAmount < 0 ? Color.red : Color.green);

                tis.SpawnTextIndicator(
                    Mathf.Abs(finalAmount),
                    pos,
                    indicatorColor,
                    sizeMult + UnityEngine.Random.Range(0f, 0.15f),
                    UnityEngine.Random.Range(0.5f, 0.7f),
                    UnityEngine.Random.Range(0.8f, 1.2f),
                    UnityEngine.Random.Range(0f, 0.2f)
                );
            }

            if (finalAmount < 0 && animator != null && CurHp > 0)
            {
                animator.SetBool(IsHurtHash, true);
                StartCoroutine(HurtDelay(esm.GetStat(StatType.HurtTime)));
                if (!bypassIFrames) TriggerIFramesCoroutine(hurtIFrameDuration);
            }

            if (CurHp <= 0 && IsAlive)
            {
                StartDeathSequence();
                return true;
            }
            else
            {
                if (healthBarInstance != null) healthBarInstance.value = CurHp;
                if (healthBarTextInstance != null) healthBarTextInstance.text = $"{CurHp}/{MaxHp}";
            }
            return false;
        }
        private void UpdatePhase()
        {
            if (!TryGetComponent<EnemyPhase>(out var ep)) return;

            float hpPct = (float)CurHp / MaxHp * 100f;
            int newPhase = 0;
            for (int i = 0; i < ep.phaseThresholds.Length; i++)
            {
                if (hpPct <= ep.phaseThresholds[i]) newPhase = i + 1;
                else break;
            }
            ep.UpdatePhase(newPhase);
        }

        private void RegenHp()
        {
            if (Time.timeScale == 0f) return;
            if (esm == null || !IsAlive || esm.GetStat(StatType.CanGainHp) != 1) return;
            if (CurHp >= (int)MaxHp) return;

            regenTimer += Time.deltaTime;

            if (regenTimer >= regenInterval)
            {
                regenTimer -= regenInterval;

                float hpPerSecond = esm.GetStat(StatType.EffHpReg) / fullRegenFrequency;
                float hpPerTick = hpPerSecond * regenInterval;

                accumulatedRegen += hpPerTick;

                if (accumulatedRegen >= 1f)
                {
                    int intRegen = Mathf.FloorToInt(accumulatedRegen);
                    accumulatedRegen -= intRegen;
                    ChangeHealth(intRegen, false);

                    if (cpum != null) cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnHealthRegen);
                }
            }
        }

        private void StartDeathSequence()
        {
            esm.AddStat(new StatBuff(StatType.isAlive, -1));

            if (cpum != null) cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnDeath);

            OnDeath?.Invoke(gameObject);

            TrySplit();

            if (TryGetComponent<IStatusEffectReceiver>(out var sem))
                sem.ClearAllEffects();

            if (healthBarInstance != null) Destroy(healthBarInstance.gameObject);
            if (healthBarTextInstance != null) Destroy(healthBarTextInstance.gameObject);

            if (animator != null && !IsAlive)
            {
                animator.SetBool(IsDeadHash, true);
                StartCoroutine(DeathDelay(1f));
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void DropGold(GameObject target)
        {
            if (target == null) return;
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
                        false,
                        true
                    );
                }
            }
        }

        private void TrySplit()
        {
            if (TryGetComponent<EntitySplitting>(out var splitting))
                splitting.Split();
        }

        private IEnumerator HurtDelay(float time)
        {
            yield return new WaitForSeconds(time);
            animator?.SetBool(IsHurtHash, false);
        }

        public void TriggerIFrames(float duration) => TriggerIFramesCoroutine(duration);
        public Coroutine TriggerIFramesCoroutine(float duration) => StartCoroutine(TriggerIFramesInternal(duration));

        private IEnumerator TriggerIFramesInternal(float duration)
        {
            esm.AddStat(new StatBuff(StatType.isImmune, 1f));
            yield return new WaitForSeconds(duration);
            esm.AddStat(new StatBuff(StatType.isImmune, -1f));
        }

        private IEnumerator DeathDelay(float delay)
        {
            yield return null;
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}
