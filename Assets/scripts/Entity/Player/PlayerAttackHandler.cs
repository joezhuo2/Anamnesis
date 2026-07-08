using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AttackType { Basic, Skill, Ultimate, Technique, Additional }
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(EntityStatManager))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(EntityHealth))]
[RequireComponent(typeof(PlayerMana))]
[RequireComponent(typeof(PlayerUpgradeManager))]
public class PlayerAttackHandler : MonoBehaviour
{
    private static readonly int AttackIndexHash = Animator.StringToHash("attackIndex");
    public List<AttackData> starting = new();
    public GameObject cooldownPrefab;
    public Transform objContainer;

    private PlayerStats p;
    private Animator a;
    private PlayerStamina ps;
    private EntityHealth ph;
    private PlayerMana pm;
    private EntityStatManager esm;
    private PlayerUpgradeManager pum;
    private readonly Dictionary<AttackType, GameObject> spawnedUIElements = new();
    [HideInInspector] public List<AttackData> attacks = new();
    [HideInInspector] public readonly Dictionary<AttackType, float> lastAttackTimes = new();

    private void Start()
    {
        a = GetComponent<Animator>();
        esm = GetComponent<EntityStatManager>();
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;
        ps = GetComponent<PlayerStamina>();
        ph = GetComponent<EntityHealth>();
        pm = GetComponent<PlayerMana>();
        pum = GetComponent<PlayerUpgradeManager>();

        for (int i = 0; i < starting.Count; i++)
            UpdateAttack(starting[i].type, starting[i]);
    }
    private void OnDestroy()
    {
        if (attacks != null)
        {
            foreach (var attack in attacks)
            {
                if (attack != null) Destroy(attack);
            }
            attacks.Clear();
        }
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
            pacui.Setup(this, p, attack.type, esm);

        if (uiObj.TryGetComponent<Button>(out var b))
        {
            AttackType attackType = attack.type;
            b.onClick.AddListener(() => PerformAttack(attackType));
        }
    }
    public void PerformAttack(AttackType type, bool bypassCooldown = false, bool noCost = false, bool triggerUpgrades = true)
    {
        if (p == null || !p.isAlive || !p.canAttack || Time.timeScale == 0f) return;

        AttackData selected = attacks.Find(atk => atk.type == type);
        if (selected == null) return;

        if (!bypassCooldown)
        {
            float lastTime = lastAttackTimes.ContainsKey(type) ? lastAttackTimes[type] : -Mathf.Infinity;
            float cooldown = selected.cooldown * Mathf.Clamp(1f - (p.attackSpeedPct * 0.01f), 0.1f, 10f);
            if (Time.time - lastTime < cooldown) return;
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
        a.speed = Mathf.Max(0.1f, 1f + (p.attackSpeedPct * 0.01f));
        StartCoroutine(ResetAttackType(selected.animationLength));
    }
    private void HandleOrbitInteractions(AttackData attack)
    {
        if (attack == null) return;
        if (!TryGetComponent<EntityProjectileHandler>(out var handler)) return;

        if (attack.fireOrbits)
        {
            handler.ReleaseAll();
        }
        else if (attack.absorbOrbits)
        {
            handler.AbsorbAll();
        }
        else if (attack.redirectOrbits)
        {
            handler.RedirectAll();
        }
        else if (attack.explodeOrbits)
        {
            handler.ExplodeAll();
        }
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

        float totalStaminaCost = Mathf.Abs(attack.staminaCost + (p.maxStamina * (attack.staminaCostPct * 0.01f)));
        float totalHealthCost = Mathf.Abs(attack.healthCost + (p.EffMaxHp * (attack.healthCostPct * 0.01f)));
        float totalManaCost = Mathf.Abs(attack.manaCost + (p.maxMana * (attack.manaCostPct * 0.01f)));

        (totalHealthCost, totalStaminaCost) = HandleHexCast(totalHealthCost, totalStaminaCost);

        if (totalStaminaCost > p.currentStamina || totalHealthCost > p.currentHp || totalManaCost > p.currentMana)
            return false;

        if (ps != null) ps.ChangeStamina(-totalStaminaCost);
        if (ph != null) ph.ChangeHealth(-totalHealthCost, 0f, true);
        if (pm != null) pm.ChangeMana(-totalManaCost, 0f);

        return true;
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
    private (float finalHpCost, float finalStaminaCost) HandleHexCast(float hpCost, float staminaCost)
    {
        if (!pum.HasUpgradeOfType<HexCast>() || p.currentStamina >= staminaCost)
            return (hpCost, staminaCost);

        float missingStamina = staminaCost - p.currentStamina;

        if (missingStamina >= p.currentHp) return (hpCost, staminaCost);

        float newStaminaCost = p.currentStamina;
        float newHpCost = hpCost + missingStamina;

        return (newHpCost, newStaminaCost);
    }
}