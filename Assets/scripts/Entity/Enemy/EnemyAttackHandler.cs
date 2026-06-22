using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EntityStatManager))]
public class EnemyAttackHandler : MonoBehaviour
{
    private static readonly int IsAttackingHash = Animator.StringToHash("isAttacking");
    private EnemyStats es;
    public List<AttackData> attacks;
    private float[] cooldowns;
    public Vector2 projSpawnOffset;
    private Animator a;
    private bool isAttackingCoroutineRunning = false;
    private List<int> availableIndexes = new();

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
        for (int i = 0; i < attacks.Count; i++) if (cooldowns[i] > 0f)cooldowns[i] -= Time.deltaTime;
    }
    private void TryAttack()
    {
        if (attacks.Count == 0 || es.target == null) return;

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

        if (a != null) a.SetBool(IsAttackingHash, true);

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

        cooldowns[index] = attack.cooldown;

        if (attack.animationLength > 0) yield return new WaitForSeconds(attack.animationLength);

        isAttackingCoroutineRunning = false;
        es.isAttacking = false;
        es.canMove = true;

        if (a != null) a.SetBool(IsAttackingHash, false);
    }
}