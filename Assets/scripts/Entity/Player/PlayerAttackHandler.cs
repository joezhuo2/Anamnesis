using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType { Basic, Skill, Ultimate, Technique, Additional }
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(EntityStatManager))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(EntityHealth))]
public class PlayerAttackHandler : MonoBehaviour
{
    private static readonly int IsAttackingHash = Animator.StringToHash("isAttacking");
    private PlayerStats p;
    private Animator a;
    private PlayerStamina ps;
    private EntityHealth ph;
    private Dictionary<AttackType, float> lastAttackTimes = new();
    public List<AttackData> attacks = new();

    private void Awake()
    {
        a = GetComponent<Animator>();
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;
        ps = GetComponent<PlayerStamina>();
        ph = GetComponent<EntityHealth>();
    }
    public void PerformAttack(AttackType type)
    {
        if (p == null || !p.isAlive || !p.canAttack) return;

        AttackData selected = attacks.Find(atk => atk.type == type);
        if (selected == null) return;

        float lastTime = lastAttackTimes.ContainsKey(type) ? lastAttackTimes[type] : -Mathf.Infinity;
        float cooldown = selected.cooldown * (1f - (p.attackSpeedPct * 0.01f));
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

        float totalStaminaCost = attack.staminaCost + (p.maxStamina * (attack.staminaCostPct * 0.01f));

        float totalHealthCost = attack.healthCost + (p.EffMaxHp * (attack.healthCostPct * 0.01f));

        if (totalStaminaCost > p.currentStamina || totalHealthCost > p.currentHp) return false;

        if (ps != null) ps.ChangeStamina(totalStaminaCost);
        if (ph != null) ph.ChangeHealth(totalHealthCost, 0f, true, false);

        return true;
    }
    public void UpdateAttack(AttackType type, AttackData newAttack)
    {
        if (newAttack == null) return;
        AttackData current = attacks.Find(atk => atk.type == type);

        attacks.Remove(current);
        attacks.Add(newAttack);
    }
}