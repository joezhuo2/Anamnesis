using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AttackType { Basic, Skill, Ultimate, Technique, Additional }

public class PlayerAttackHandler : MonoBehaviour
{
    private static readonly int AttackIndexHash = Animator.StringToHash("attackIndex");
    public List<AttackData> starting = new();
    public GameObject cooldownPrefab;
    public Transform objContainer;

    private Animator a;
    private PlayerStamina ps;
    private IDamageable ph;
    private PlayerMana pm;
    private IStatProvider esm;
    private PlayerUpgradeManager pum;
    private readonly Dictionary<AttackType, GameObject> spawnedUIElements = new();
    [HideInInspector] public List<AttackData> attacks = new();
    [HideInInspector] public readonly Dictionary<AttackType, float> lastAttackTimes = new();

    private void Start()
    {
        a = GetComponent<Animator>();
        esm = GetComponent<IStatProvider>();
        ph = GetComponent<IDamageable>();
        ps = GetComponent<PlayerStamina>();
        pm = GetComponent<PlayerMana>();
        pum = GetComponent<PlayerUpgradeManager>();

        for (int i = 0; i < starting.Count; i++) UpdateAttack(starting[i].type, starting[i]);
    }
    private void OnDestroy()
    {
        if (attacks != null)
        {
            foreach (var attack in attacks)
                if (attack != null) DestroyImmediate(attack, true);
            attacks.Clear();
        }

        foreach (var kvp in spawnedUIElements)
        {
            if (kvp.Value != null)
            {
                if (kvp.Value.TryGetComponent<Button>(out var btn))
                    btn.onClick.RemoveAllListeners();
                Destroy(kvp.Value);
            }
        }
        spawnedUIElements.Clear();
    }

    public bool HasAttack(AttackData a)
    {
        if (a == null || a.name == null) return false;
        string n = a.name.Trim();

        for (int i = 0; i < attacks.Count; i++)
            if (attacks[i].name.Trim().Equals(n, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public AttackData FindAttackOfType(AttackType type)
    {
        for (int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i].type == type)
                return attacks[i];
        }
        return null;
    }

    private void CreateButtonUI(AttackData attack)
    {
        GameObject uiObj = Instantiate(cooldownPrefab, objContainer);
        spawnedUIElements[attack.type] = uiObj;

        if (uiObj.TryGetComponent<PlayerAttackCooldownUI>(out var pacui))
            pacui.Setup(this, attack.type, esm);

        if (uiObj.TryGetComponent<Button>(out var b))
        {
            AttackType attackType = attack.type;
            b.onClick.AddListener(() => PerformAttack(attackType));
        }
    }

    public void PerformAttack(AttackType type, bool bypassCooldown = false, bool noCost = false, bool triggerUpgrades = true)
    {
        if (esm == null || esm.GetStat(StatType.isAlive) <= 0f || esm.GetStat(StatType.CanAttack) <= 0f || Time.timeScale == 0f) return;

        AttackData selected = attacks.Find(atk => atk.type == type);
        if (selected == null) return;

        if (!bypassCooldown)
        {
            float lastTime = lastAttackTimes.ContainsKey(type) ? lastAttackTimes[type] : -Mathf.Infinity;
            if (Time.time - lastTime < GetEffCd(selected, esm)) return;
        }

        if (!noCost && !HandleStatChanges(selected)) return;

        if (!bypassCooldown) lastAttackTimes[type] = Time.time;

        HandleOrbitInteractions(selected);

        ProjectileSpawner ps = ProjectileSpawner.Instance;
        if (ps != null)
            StartCoroutine(ps.SpawnFromPattern(selected, gameObject, transform.position));

        if (selected.summonChance > 0f && selected.summonCondition == SummonCondition.OnCast && UnityEngine.Random.value <= selected.summonChance)
        {
            if (TryGetComponent<EntitySummonHandler>(out var summonHandler))
                summonHandler.Summon();
        }

        if (triggerUpgrades)
            TriggerUpgradesOnAttack(type);

        int attackIndex = type switch
        {
            AttackType.Basic => 0,
            AttackType.Skill => 1,
            AttackType.Ultimate => 2,
            _ => -1
        };

        a.SetInteger(AttackIndexHash, attackIndex);
        a.speed = Mathf.Max(0.1f, 1f + (esm.GetStat(StatType.attackSpeedPct) * 0.01f));
        StartCoroutine(ResetAttackType(selected.animationLength));
    }

    private void HandleOrbitInteractions(AttackData attack)
    {
        if (attack == null) return;
        if (!TryGetComponent<EntityProjectileHandler>(out var handler)) return;

        if (attack.fireOrbits) handler.ReleaseOrbits(attack.redirectCount);
        else if (attack.absorbOrbitPct > 0f) handler.AbsorbOrbits(attack.redirectCount, attack.absorbOrbitPct);
        else if (attack.redirectOrbits) handler.RedirectOrbits(attack.redirectCount);
        else if (attack.explodeOrbits) handler.ExplodeOrbits(attack.redirectCount);
    }

    private void TriggerUpgradesOnAttack(AttackType type)
    {
        pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnAttack);

        switch (type)
        {
            case AttackType.Basic: pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnBasicAttack); break;
            case AttackType.Skill: pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnSkillAttack); break;
            case AttackType.Ultimate: pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnUltAttack); break;
            default: break;
        }
    }

    public IEnumerator ResetAttackType(float delay)
    {
        yield return new WaitForSeconds(delay);
        a.SetInteger(AttackIndexHash, -1);
        a.speed = 1f;
    }

    public bool HandleStatChanges(AttackData attack)
    {
        if (attack == null) return false;

        pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnCalculateAttackCost);

        var (hp, sp, mp) = GetCosts(attack, esm);
        (hp, sp) = HandleHexCast(hp, sp);

        if (sp > esm.GetStat(StatType.CurrentStamina) || hp > esm.GetStat(StatType.currentHp) || mp > esm.GetStat(StatType.CurrentMana)) return false;

        var dp = DamagePacket.BuildDamagePacket(hp, DamageType.Consume, false, Color.red, gameObject, false, 1f);
        if (ph != null) ph.TakeDamage(dp);

        if (ps != null) ps.ChangeStamina(-sp);
        if (pm != null) pm.ChangeMana(-mp);

        return true;
    }

    public static (int hp, int sp, int mp) GetCosts(AttackData attack, IStatProvider esm)
    {
        if (attack == null || esm == null) return (0, 0, 0);

        float totalStaminaCost = Mathf.Abs(attack.staminaCost + (esm.GetStat(StatType.EffMaxStamina) * (attack.staminaCostPct * 0.01f))) * (1f + (esm.GetStat(StatType.stCostPct) * 0.01f));
        float totalHealthCost = Mathf.Abs(attack.healthCost + (esm.GetStat(StatType.EffMaxHp) * (attack.healthCostPct * 0.01f)));
        float totalManaCost = Mathf.Abs(attack.manaCost + (esm.GetStat(StatType.EffMaxMana) * (attack.manaCostPct * 0.01f)));

        return (Mathf.RoundToInt(totalHealthCost), Mathf.RoundToInt(totalStaminaCost), Mathf.RoundToInt(totalManaCost));
    }

    public void UpdateAttack(AttackType type, AttackData newAttack)
    {
        if (newAttack == null) return;
        AttackData current = attacks.Find(atk => atk.type == type);

        if (current != null)
        {
            attacks.Remove(current);
            Destroy(current);
        }

        AttackData runtimeAttackCopy = Instantiate(newAttack);
        runtimeAttackCopy.type = type;
        runtimeAttackCopy.InitializeRuntimeCopy();

        attacks.Add(runtimeAttackCopy);

        if (pum.HasUpgradeOfType<SoulRendPU>() && (type == AttackType.Basic || type == AttackType.Skill))
            pum.GetPlayerUpgradeOfType<SoulRendPU>().OnUnlock(gameObject);

        if (spawnedUIElements.ContainsKey(type))
        {
            Destroy(spawnedUIElements[type]);
            spawnedUIElements.Remove(type);
        }
        CreateButtonUI(runtimeAttackCopy);
    }

    public void RemoveAttack(AttackType type)
    {
        AttackData current = attacks.Find(atk => atk.type == type);
        if (current != null)
        {
            attacks.Remove(current);
            Destroy(current);
        }

        if (spawnedUIElements.ContainsKey(type))
        {
            Destroy(spawnedUIElements[type]);
            spawnedUIElements.Remove(type);
        }
    }

    private (int finalHpCost, int finalStaminaCost) HandleHexCast(float hpCost, float staminaCost)
    {
        if (!pum.HasUpgradeOfType<HexCast>() || esm.GetStat(StatType.CurrentStamina) >= staminaCost)
            return (Mathf.RoundToInt(hpCost), Mathf.RoundToInt(staminaCost));

        float missingStamina = staminaCost - esm.GetStat(StatType.CurrentStamina);

        if (missingStamina >= esm.GetStat(StatType.currentHp))
            return (Mathf.RoundToInt(hpCost), Mathf.RoundToInt(staminaCost));

        float newStaminaCost = esm.GetStat(StatType.CurrentStamina);
        float newHpCost = hpCost + missingStamina;

        return (Mathf.RoundToInt(newHpCost), Mathf.RoundToInt(newStaminaCost));
    }

    public void AdvanceAllCooldowns(float pctAmt)
    {
        var keys = new List<AttackType>(lastAttackTimes.Keys);
        foreach (var type in keys) AdvanceCooldown(type, pctAmt);
    }

    public void AdvanceCooldown(AttackType type, float pctAmt)
    {
        if (!lastAttackTimes.ContainsKey(type)) return;

        float lastTime = lastAttackTimes[type];

        var effCd = GetEffCd(attacks.Find(a => a.type == type), esm);

        if (effCd <= 0f) return;

        float timeElapsed = Time.time - lastTime;
        float cooldownRemainingPct = 1f - (timeElapsed / effCd);
        float newCooldownRemainingPct = Mathf.Clamp01(cooldownRemainingPct - (pctAmt * 0.01f));
        float newLastTime = Time.time - ((1f - newCooldownRemainingPct) * effCd);

        lastAttackTimes[type] = newLastTime;
    }

    public static float GetEffCd(AttackData attack, IStatProvider esm)
    {
        float cdrPct = attack.type switch
        {
            AttackType.Basic => esm.GetStat(StatType.basicCdRedPct),
            AttackType.Skill => esm.GetStat(StatType.skillCdRedPct),
            AttackType.Ultimate => esm.GetStat(StatType.ultCdRedPct),
            _ => 0f
        };
        return attack.cooldown *
            Mathf.Clamp(1f - (esm.GetStat(StatType.attackSpeedPct) * 0.01f), 0.3f, 10f) *
            Mathf.Clamp(1f - (cdrPct * 0.01f), 0.1f, 0.9f);
    }
}