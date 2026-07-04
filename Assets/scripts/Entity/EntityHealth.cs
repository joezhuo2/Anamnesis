using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EntityStatManager))]
[RequireComponent(typeof(Animator))]
public class EntityHealth : MonoBehaviour
{
    private static readonly int IsDeadHash = Animator.StringToHash("isDead");
    private static readonly int IsHurtHash = Animator.StringToHash("isHurt");
    private float regenTimer;
    private const float regenInterval = 0.5f; // how often hp regens, in seconds
    private const float fullRegenFrequency = 5f; // time to heal full amount of hpRegen stat
    private const float hurtIFrameDuration = 0.2f; // time after taking damage where you cannot take more damage
    private const float dodgeCooldown = 0.6f; // minimum time before dodges
    private float accumulatedRegen;
    private float lastDodgeTime;
    private Animator animator;
    private EntityStats es;
    public Slider healthBarPrefab;
    private Slider healthBarInstance;
    public Vector3 healthBarOffset = new(0, 0, 0);
    public TextMeshProUGUI healthBarTextPrefab;
    private TextMeshProUGUI healthBarTextInstance;
    private Camera mainCamera;
    private PlayerUpgradeManager cpum;
    private EntityStatManager esm;
    private int lastAppliedPhase = 0;

    private void Start()
    {
        esm = GetComponent<EntityStatManager>();
        if (esm != null) es = esm.s;
        animator = GetComponent<Animator>();

        mainCamera = Camera.main;

        regenTimer = 0f;
        accumulatedRegen = 0f;
        lastDodgeTime = -Mathf.Infinity;

        if (TryGetComponent<PlayerUpgradeManager>(out var pum)) cpum = pum;

        InitializeHealthBar();
    }
    private void InitializeHealthBar()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);

            healthBarInstance.maxValue = es.EffMaxHp;
            healthBarInstance.value = es.EffMaxHp;

            healthBarTextInstance = Instantiate(healthBarTextPrefab, canvas.transform);
            healthBarTextInstance.text = $"{es.currentHp}/{es.EffMaxHp}";
        }
    }
    private void Update()
    {
        if (es != null && es.canGainHp) RegenHp();
        MoveHealthBar();
    }
    private void MoveHealthBar()
    {
        if (healthBarInstance != null && mainCamera != null && es.isAlive && healthBarTextInstance != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position + healthBarOffset);
            healthBarInstance.transform.position = screenPos;

            healthBarTextInstance.transform.position = screenPos + healthBarOffset;
        }
    }

    public void TakeDamage(DamagePacket dp, bool bypassIFrames, GameObject source, float sourceResPen = float.NaN, int sourceDefShred = int.MinValue, float sizeOverride = 0f)
    {
        if (dp == null) return;

        float resPen = sourceResPen;
        int defShred = sourceDefShred;
        if ((float.IsNaN(resPen) || defShred == int.MinValue) && source != null && source.TryGetComponent<EntityStatManager>(out var sourceStats) && sourceStats.s != null)
        {
            if (float.IsNaN(resPen)) resPen = sourceStats.s.resPen;
            if (defShred == int.MinValue) defShred = sourceStats.s.defShred;
        }
        if (float.IsNaN(resPen)) resPen = 0f;
        if (defShred == int.MinValue) defShred = 0;

        foreach (var i in dp.instances)
        {
            var (dmg, sizeMult)= i.type == DamageType.True || i.type == DamageType.DoT ? (i.amount, 1f) : CalculateDamageTaken(i.type, i.amount, resPen, defShred);

            Color color = i.indicatorColor != default ? i.indicatorColor : i.type switch
            {
                DamageType.Physical => Color.gray,
                DamageType.Spell => Color.purple,
                DamageType.True => Color.lightBlue,
                _ => Color.white
            };

            if (i.isCrit) sizeMult *= 1.5f;
            if (sizeOverride > 0f) sizeMult = sizeOverride;

            if (dmg > 0)
                ChangeHealth(-Mathf.RoundToInt(dmg), 0, true, sizeMult, color, bypassIFrames, source);
        }
    }
    public void ChangeHealth(float amount, float pctAmt, bool showIndicator = true, float sizeMult = 1f, Color colorOverride = default, bool bypassIFrames = false, GameObject source = null)
    {
        if ((amount < 0 || pctAmt < 0) && es.isImmune && es.isDashing && cpum != null)
        {
            cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnCounterDodge);
            return;
        }
        if (((amount < 0 || pctAmt < 0) && es.isImmune) || ((amount > 0 || pctAmt > 0) && !es.canGainHp) || (amount == 0 && pctAmt == 0)) return;

        int finalAmount = Mathf.RoundToInt(amount + (pctAmt * es.EffMaxHp));
        if (finalAmount == 0) return;

        if (finalAmount < 0 && es.currentHp > 0 && Mathf.Abs(finalAmount) >= es.currentHp * 3f)
        {
            if (source != null && source.TryGetComponent<PlayerUpgradeManager>(out var pum))
                pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnOverkill);
        }

        int newHp = Math.Min(es.currentHp + finalAmount, (int)es.EffMaxHp);
        es.currentHp = Mathf.Max(0, newHp);

        UpdatePhase();

        DamageIndicatorSpawner dis = DamageIndicatorSpawner.Instance;
        Vector3 pos = transform.position;

        if (dis != null && showIndicator)
        {
            Color indicatorColor = colorOverride != default ? colorOverride : (finalAmount < 0 ? Color.red : Color.green);

            dis.SpawnDamageIndicator(
                Mathf.Abs(finalAmount),
                pos,
                indicatorColor,
                sizeMult,
                0.6f,
                1f
            );
        }

        if (finalAmount < 0 && animator != null && es.currentHp > 0)
        {
            animator.SetBool(IsHurtHash, true);
            StartCoroutine(HurtDelay(es.hurtTime));
            if (!bypassIFrames)
                StartCoroutine(TriggerIFrames(hurtIFrameDuration));
        }

        if (es.currentHp <= 0 && es.isAlive)
        {
            StartDeathSequence();
        }
        else
        {
            if (healthBarInstance != null && healthBarTextInstance != null)
            {
                healthBarInstance.value = es.currentHp;
                healthBarTextInstance.text = $"{es.currentHp}/{es.EffMaxHp}";
            }
        }
    }
    private void UpdatePhase()
    {
        if (es is not EnemyStats enemyStats || enemyStats.phaseThresholds == null) return;

        float hpPct = (float)es.currentHp / es.EffMaxHp * 100f;
        int newPhase = 0;
        for (int i = 0; i < enemyStats.phaseThresholds.Length; i++)
        {
            if (hpPct <= enemyStats.phaseThresholds[i]) newPhase = i + 1;
            else break;
        }
        if (enemyStats.phase == newPhase) return;

        int previousPhase = enemyStats.phase;
        enemyStats.phase = newPhase;

        if (enemyStats.phaseBuffs == null || esm == null) return;

        if (newPhase > previousPhase)
        {
            foreach (var pb in enemyStats.phaseBuffs)
            {
                if (pb.phaseReq > previousPhase && pb.phaseReq <= newPhase)
                    esm.AddStat(pb.buff);
            }
        }
        else if (newPhase < previousPhase)
        {
            foreach (var pb in enemyStats.phaseBuffs)
            {
                if (pb.phaseReq > newPhase && pb.phaseReq <= previousPhase)
                    esm.AddStat(pb.buff, false);
            }
        }
    }
    private void RegenHp()
    {
        if (es?.isAlive != true || es.EffHpReg <= 0f) return;
        if (es.currentHp >= (int)es.EffMaxHp) return;

        regenTimer += Time.deltaTime;

        if (regenTimer >= regenInterval)
        {
            regenTimer -= regenInterval;

            float hpPerSecond = es.EffHpReg / fullRegenFrequency;
            float hpPerTick = hpPerSecond * regenInterval;

            accumulatedRegen += hpPerTick;

            if (accumulatedRegen >= 1f)
            {
                int intRegen = Mathf.FloorToInt(accumulatedRegen);
                accumulatedRegen -= intRegen;
                ChangeHealth(intRegen, 0, false);

                if (cpum != null) cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnHealthRegen);
            }
        }
    }

    private void StartDeathSequence()
    {
        es.isAlive = false;

        if (TryGetComponent<StatusEffectManager>(out var sem))
            sem.ClearAllEffects();

        if (healthBarInstance != null && healthBarTextInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
            Destroy(healthBarTextInstance.gameObject);
        }

        if (animator != null && !es.isAlive)
        {
            animator.SetBool(IsDeadHash, true);
            StartCoroutine(DeathDelay(1f));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private (float dmg, float size) CalculateDamageTaken(DamageType type, float rawDamage, float sourceResPen, int sourceDefShred)
    {
        float effRes = Mathf.Max(-100f, es.damageRes - sourceResPen);
        float resMult = 1f - (effRes * 0.01f);
        float effArmor = Mathf.Max(0, es.EffArmor - sourceDefShred);
        float armorMult = type == DamageType.Physical ? 1f - ((float)effArmor / (effArmor + 100f)) : 1f;

        float typeMult = type switch
        {
            DamageType.Physical => 1f - (es.physicalRes * 0.01f),
            DamageType.Spell => 1f - (es.spellRes * 0.01f),
            _ => 1f
        };

        float finalDamage = rawDamage * resMult * armorMult * typeMult;
        float size = 1f;

        if (Time.time >= lastDodgeTime + dodgeCooldown)
        {
            float dc = es.dodgeChance * 0.01f;
            float dodgeMult = 1f - (es.dodgeResPct * 0.01f);

            if (UnityEngine.Random.Range(0f, 1f) < dc)
            {
                finalDamage *= dodgeMult;
                size = 0.5f;
                lastDodgeTime = Time.time;
            }
        }

        return (finalDamage, size);
    }

    private IEnumerator HurtDelay(float time)
    {
        yield return new WaitForSeconds(time);
        animator?.SetBool(IsHurtHash, false);
    }

    public IEnumerator TriggerIFrames(float duration)
    {
        es.isImmune = true;
        yield return new WaitForSeconds(duration);
        es.isImmune = false;
    }

    private IEnumerator DeathDelay(float delay)
    {
        yield return null;
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}