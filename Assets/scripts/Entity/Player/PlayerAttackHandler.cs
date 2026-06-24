using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum AttackType { Basic, Skill, Ultimate, Technique, Additional }
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(EntityStatManager))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(EntityHealth))]
[RequireComponent(typeof(PlayerMana))]
public class PlayerAttackHandler : MonoBehaviour
{
    private static readonly int IsAttackingHash = Animator.StringToHash("isAttacking");
    private PlayerStats p;
    private Animator a;
    private PlayerStamina ps;
    private EntityHealth ph;
    private PlayerMana pm;
    private readonly Dictionary<AttackType, float> lastAttackTimes = new();
    public List<AttackData> attacks = new();

    private void Start()
    {
        a = GetComponent<Animator>();
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;
        ps = GetComponent<PlayerStamina>();
        ph = GetComponent<EntityHealth>();
        pm = GetComponent<PlayerMana>();
    }
    public void PerformAttack(AttackType type)
    {
        if (p == null || !p.isAlive || !p.canAttack || Time.timeScale == 0f) return;

        AttackData selected = attacks.Find(atk => atk.type == type);
        if (selected == null) return;

        float lastTime = lastAttackTimes.ContainsKey(type) ? lastAttackTimes[type] : -Mathf.Infinity;
        float cooldown = selected.cooldown * Mathf.Clamp(1f - (p.attackSpeedPct * 0.01f), 0.1f, 10f);
        if (Time.time - lastTime < cooldown) return;

        if (!HandleStatChanges(selected)) return;

        lastAttackTimes[type] = Time.time;

        StartCoroutine(ProjectileSpawner.Instance.SpawnFromPattern(selected.projectilePrefab, gameObject, transform.position));

        a.SetBool(IsAttackingHash, true);
        a.speed = Mathf.Max(0.1f, 1f + (p.attackSpeedPct * 0.01f));
        StartCoroutine(ResetAttackType(selected.animationLength));
    }
    public IEnumerator ResetAttackType(float delay)
    {
        yield return new WaitForSeconds(delay);
        a.SetBool(IsAttackingHash, false);
        a.speed = 1f;
    }
    public bool HandleStatChanges(AttackData attack)
    {
        if (attack == null) return false;

        float totalStaminaCost = Mathf.Abs(attack.staminaCost + (p.maxStamina * (attack.staminaCostPct * 0.01f)));
        float totalHealthCost = Mathf.Abs(attack.healthCost + (p.EffMaxHp * (attack.healthCostPct * 0.01f)));
        float totalManaCost = Mathf.Abs(attack.manaCost + (p.maxMana * (attack.manaCostPct * 0.01f)));

        Debug.Log($"{totalStaminaCost} {totalHealthCost} {totalManaCost}");
        Debug.Log($"{p.currentStamina} {p.currentHp} {p.currentMana}");

        if (totalStaminaCost > p.currentStamina || totalHealthCost > p.currentHp || totalManaCost > p.currentMana)
            return false;

        Debug.Log("Cast");

        if (ps != null) ps.ChangeStamina(-totalStaminaCost);
        if (ph != null) ph.ChangeHealth(-totalHealthCost, 0f, true, false);
        if (pm != null) pm.ChangeMana(-totalManaCost, 0f);

        return true;
    }
    public void UpdateAttack(AttackType type, AttackData newAttack)
    {
        if (newAttack == null) return;
        AttackData current = attacks.Find(atk => atk.type == type);

        if (current != null) attacks.Remove(current);

        AttackData runtimeAttackCopy = Instantiate(newAttack);
        runtimeAttackCopy.type = type;

        attacks.Add(runtimeAttackCopy);
    }
}