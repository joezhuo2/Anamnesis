using System;
using System.Collections;
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
    private const float hurtIFrameDuration = 0.4f; // time after taking damage where you cannot take more damage
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
    private void Start()
    {
        es = GetComponent<EntityStatManager>()?.s;
        animator = GetComponent<Animator>();

        mainCamera = Camera.main;

        regenTimer = 0f;
        accumulatedRegen = 0f;
        lastDodgeTime = -Mathf.Infinity;

        InitializeHealthBar();
    }
    private void InitializeHealthBar()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);
            healthBarInstance.maxValue = es.maxHp;
            healthBarInstance.value = es.maxHp;

            healthBarTextInstance = Instantiate(healthBarTextPrefab, canvas.transform);
            healthBarTextInstance.text = $"{es.currentHp}/{es.maxHp}";
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

    public void TakeDamage(DamagePacket dp, bool defOverride = false)
    {
        if (dp == null) return;
        foreach (var i in dp.instances)
        {
            float finalDamage = defOverride ? i.amount : CalculateDamageTaken(i.type, i.amount);

            if (finalDamage > 0)
                ChangeHealth(-Mathf.RoundToInt(finalDamage), 0, true, i.isCrit);
        }
    }
    public void ChangeHealth(float amount, float pctAmt, bool showIndicator, bool isCrit)
    {
        if (((amount < 0 || pctAmt < 0) && es.isImmune) || ((amount > 0 || pctAmt > 0) && !es.canGainHp) || (amount == 0 && pctAmt == 0)) return;

        int finalAmount = Mathf.RoundToInt(amount + (pctAmt * es.maxHp));
        if (finalAmount == 0) return;

        int newHp = Math.Min(es.currentHp + finalAmount, es.maxHp);
        es.currentHp = Mathf.Max(0, newHp);

        var dis = DamageIndicatorSpawner.Instance;
        var pos = transform.position;

        if (dis != null && showIndicator)
        {
            dis.SpawnDamageIndicator(
                finalAmount < 0 ? -finalAmount : finalAmount, pos,
                finalAmount < 0 ? Color.red : Color.green,
                isCrit ? 1.5f : 1f,
                0.6f,
                1f
            );
        }

        if (finalAmount < 0 && animator != null && es.currentHp > 0)
        {
            animator.SetBool(IsHurtHash, true);
            StartCoroutine(HurtDelay(0.3f));
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
                healthBarTextInstance.text = $"{es.currentHp}/{es.maxHp}";
            }
        }
    }
    private void RegenHp()
    {
        if (es?.isAlive != true || es.hpRegen <= 0f) return;
        if (es.currentHp >= es.maxHp) return;

        regenTimer += Time.deltaTime;

        if (regenTimer >= regenInterval)
        {
            regenTimer -= regenInterval;

            float hpPerSecond = es.hpRegen / fullRegenFrequency;
            float hpPerTick = hpPerSecond * regenInterval;

            accumulatedRegen += hpPerTick;

            if (accumulatedRegen >= 1f)
            {
                int intRegen = Mathf.FloorToInt(accumulatedRegen);
                accumulatedRegen -= intRegen;
                ChangeHealth(intRegen, 0, false, false);
            }
        }
    }

    private void StartDeathSequence()
    {
        es.isAlive = false;

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

    private float CalculateDamageTaken(DamageType type, float rawDamage)
    {
        float resMult = 1f - (es.damageRes * 0.01f);
        float armorMult = 1f - ((float)es.armor / (es.armor + 100f));

        float typeMult = type switch
        {
            DamageType.Physical => 1f - (es.physicalRes * 0.01f),
            DamageType.Spell => 1f - (es.spellRes * 0.01f),
            _ => 1f
        };

        float finalDamage = rawDamage * resMult * armorMult * typeMult;

        if (Time.time >= lastDodgeTime + dodgeCooldown)
        {
            float dc = es.dodgeChance * 0.01f;
            float dodgeMult = 1f - (es.dodgeResPct * 0.01f);

            if (UnityEngine.Random.Range(0f, 1f) < dc)
            {
                finalDamage *= dodgeMult;
                lastDodgeTime = Time.time;
            }
        }

        return finalDamage;
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