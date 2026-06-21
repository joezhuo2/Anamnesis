using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType { Basic, Skill, Ultimate, Technique, Additional }
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(EntityStatManager))]
[RequireComponent(typeof(PlayerStamina))]
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
        if (p == null || !p.isAlive) return;

        AttackData selected = attacks.Find(atk => atk.type == type);
        if (selected == null) return;

        float lastTime = lastAttackTimes.ContainsKey(type) ? lastAttackTimes[type] : -Mathf.Infinity;
        float cooldown = selected.cooldown * (1f - (p.attackSpeedPct * 0.01f));
        if (Time.time - lastTime < cooldown) return;

        lastAttackTimes[type] = Time.time;

        StartCoroutine(ProjectileSpawner.Instance.SpawnFromPattern(selected.projectilePrefab, gameObject, transform.position, PlayerInputHandler.mousePos.normalized));

        a.SetBool(IsAttackingHash, true);
        a.speed = Mathf.Max(0.1f, 1f + (p.attackSpeedPct * 0.01f));
        StartCoroutine(ResetAttackType(0.5f / a.speed));
    }
    public IEnumerator ResetAttackType(float delay)
    {
        yield return new WaitForSeconds(delay);
        a.SetBool(IsAttackingHash, false);
        a.speed = 1f;
    }
    public void HandleStatChanges(AttackData attack)
    {
        if (attack == null) return;

        if (attack.staminaCost == 0 && attack.healthCost == 0 &&
            attack.staminaCostPct == 0 && attack.healthCostPct == 0) return;

        if (ps != null) ps.ChangeStamina(attack.staminaCost, attack.staminaCostPct);
        if (ph != null) ph.ChangeHealth(attack.healthCost, attack.healthCostPct, true, false);
    }
    public void UpdateAttack(AttackType type, AttackData newAttack)
    {
        if (newAttack == null) return;
        AttackData current = attacks.Find(atk => atk.type == type);

        attacks.Remove(current);
        attacks.Add(newAttack);
    }
}