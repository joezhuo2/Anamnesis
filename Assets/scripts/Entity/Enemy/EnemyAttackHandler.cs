using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EntityStatManager))]
public class EnemyAttackHandler : MonoBehaviour
{
    public Vector2 projSpawnOffset;
    public List<AttackData> attacks;
    public float globalCooldown;

    private static readonly int AttackIndexHash = Animator.StringToHash("attackIndex");
    private EnemyStats es;
    private float[] cooldowns;
    private Animator a;
    private bool isAttackingCoroutineRunning = false;
    private List<int> availableIndexes = new();
    private float lastAttackEndTime;

    private void Awake()
    {
        a = GetComponent<Animator>();

        cooldowns = new float[attacks.Count];
        for (int i = 0; i < attacks.Count; i++) cooldowns[i] = attacks[i].cooldown;
    }

    private void Start() => es = GetComponent<EntityStatManager>().s as EnemyStats;
    private void Update()
    {
        if (!es.isAlive) return;
        UpdateCooldowns();
        if (!es.isAttacking) TryAttack();
    }
    private void UpdateCooldowns()
    {
        for (int i = 0; i < attacks.Count; i++) if (cooldowns[i] > 0f) cooldowns[i] -= Time.deltaTime;
    }
    private void TryAttack()
    {
        if (attacks.Count == 0 || es.target == null) return;
        if (globalCooldown > 0 && Time.time - lastAttackEndTime < globalCooldown) return;

        int chosen = ChooseAttackIndex();

        if (chosen == -1) return;

        if (!isAttackingCoroutineRunning) StartCoroutine(PerformAttack(attacks[chosen], chosen));
    }

    private int ChooseAttackIndex()
    {
        float dist = (es.target.transform.position - transform.position).sqrMagnitude;

        availableIndexes.Clear();

        for (int i = 0; i < attacks.Count; i++)
        {
            AttackData a = attacks[i];

            if (cooldowns[i] > 0f || dist > (a.maxRange * a.maxRange)) continue;

            float hpPct = (float)es.currentHp / es.EffMaxHp;

            if (a.minHpPct > 0 && hpPct < a.minHpPct) continue;
            if (a.maxHpPct < 100f && hpPct > a.maxHpPct) continue;
            if (a.phaseReq >= 0 && (es.phase != a.phaseReq)) continue;

            availableIndexes.Add(i);
        }

        if (availableIndexes.Count == 0) return -1;

        return availableIndexes[Random.Range(0, availableIndexes.Count)];
    }
    private System.Collections.IEnumerator PerformAttack(AttackData attack, int index)
    {
        isAttackingCoroutineRunning = true;
        es.isAttacking = true;
        es.canMove = attack.canMoveDuringAttack;

        if (a != null) a.SetInteger(AttackIndexHash, index);

        if (attack.projectilePrefab != null)
        {
            if (attack.spawnDelay > 0) yield return new WaitForSeconds(attack.spawnDelay);

            if (es.target != null)
            {
                Vector2 dir = (es.target.transform.position - transform.position).normalized;

                StartCoroutine(ProjectileSpawner.Instance.SpawnFromPattern(
                    attack.projectilePrefab,
                    gameObject,
                    transform.position,
                    dir,
                    attack.spawnDistance
                ));
            }
        }

        if (attack.summonChance > 0f && attack.summonCondition == SummonCondition.OnCast && Random.value <= attack.summonChance)
        {
            if (TryGetComponent<EntitySummonHandler>(out var summonHandler))
                summonHandler.Summon();
        }

        if (index >= 0) cooldowns[index] = attack.cooldown;

        if (attack.animationLength > 0) yield return new WaitForSeconds(attack.animationLength);

        if (attack.nextAttack != null)
        {
            yield return PerformAttack(attack.nextAttack, -1);
        }
        else
        {
            isAttackingCoroutineRunning = false;
            es.isAttacking = false;
            es.canMove = true;
            lastAttackEndTime = Time.time;
            if (a != null) a.SetInteger(AttackIndexHash, -1);
        }
    }
}