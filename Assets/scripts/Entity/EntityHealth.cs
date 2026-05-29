using System;
using System.Collections;
using UnityEngine;

public class EntityHealth : MonoBehaviour
{
    private static readonly int IsHurtHash = Animator.StringToHash("isHurt");
    private float regenTimer;
    private const float regenInterval = 0.5f; // how often hp regens, in seconds
    private const float fullRegenFrequency = 5f; // time to heal full amount of hpRegen stat
    private const float hurtIFrameDuration = 0.4f; // time after taking damage where you cannot take more damage
    private const float dodgeCooldown = 0.6f; // minimum time before dodges
    private float accumulatedRegen;
    private float lastDodgeTime;
    public Animator animator;
    public EntityStats es;
    private void Start()
    {
        regenTimer = 0f;
        accumulatedRegen = 0f;
        lastDodgeTime = -Mathf.Infinity;
    }
    private void Update()
    {
        if (es.canGainHp) RegenHp();
    }

    public void TakeDamage(DamagePacket dp, bool defOverride = false)
    {
        if (dp == null) return;
        foreach (var i in dp.instances)
        {
            float finalDamage = defOverride ? i.amount : CalculateDamageTaken(i.type, i.amount);

            if (finalDamage > 0)
                ChangeHealth(-Mathf.RoundToInt(finalDamage), true, i.isCrit);
        }
    }
    public void ChangeHealth(int amount, bool showIndicator, bool isCrit)
    {
        if ((amount < 0 && es.isImmune) || (amount > 0 && !es.canGainHp) || amount == 0) return;

        int newHp = Math.Min(es.currentHp + amount, es.maxHp);
        es.currentHp = Mathf.Max(0, newHp);

        var dis = DamageIndicatorSpawner.Instance;
        var pos = transform.position;
        if (dis != null && showIndicator)
        {
            float scale = isCrit ? 1.5f : 1f;
            if (amount < 0) dis.SpawnDamageIndicator(-amount, pos, Color.red, scale, 0.6f, 1f);
            else dis.SpawnDamageIndicator(amount, pos, Color.green, scale, 0.6f, 1f);
        }

        if (amount < 0 && animator != null && es.currentHp > 0)
        {
            animator.SetBool(IsHurtHash, true);
            StartCoroutine(HurtDelay(0.3f));
            StartCoroutine(TriggerIFrames(hurtIFrameDuration));
        }

        if (es.currentHp <= 0 && es.isAlive)
            StartDeathSequence();
    }

    private void RegenHp()
    {
        if (es?.isAlive != true || es.hpRegen <= 0f) return;
        if (es.currentHp >= es.maxHp) return;

        regenTimer += Time.deltaTime;

        if (regenTimer >= regenInterval)
        {
            regenTimer = 0f;

            float hpPerSecond = es.hpRegen / fullRegenFrequency;
            float hpPerTick = hpPerSecond * regenInterval;

            accumulatedRegen += hpPerTick;

            if (accumulatedRegen >= 1f)
            {
                int intRegen = Mathf.FloorToInt(accumulatedRegen);
                accumulatedRegen -= intRegen;
                ChangeHealth(intRegen, false, false);
            }
        }
    }

    private void StartDeathSequence()
    {
        es.isAlive = false;

        if (animator != null && !es.isAlive)
        {
            animator.SetBool("isDead", true);
            StartCoroutine(DeathDelay(1.25f));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private float CalculateDamageTaken(DamageType type, float rawDamage)
    {
        float resMult = 1f - es.damageRes;
        float armorMult = 1f - ((float)es.armor / (es.armor + 100f));

        float typeMult = type switch
        {
            DamageType.Physical => 1f - es.physicalRes,
            DamageType.Spell => 1f - es.spellRes,
            _ => 1f
        };

        float finalDamage = rawDamage * resMult * armorMult * typeMult;

        if (Time.time >= lastDodgeTime + dodgeCooldown)
        {
            float dc = es.dodgeChance / 100f;
            float dodgeMult = 1f - es.dodgeResPct;

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
        yield return null;
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