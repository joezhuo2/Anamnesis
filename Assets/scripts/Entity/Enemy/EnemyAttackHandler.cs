using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class EnemyAttackHandler : MonoBehaviour
    {
        public Vector2 projSpawnOffset;
        public List<AttackData> attacks;
        public float globalCooldown;

        private static readonly int AttackIndexHash = Animator.StringToHash("attackIndex");
        private float[] cooldowns;
        private Animator a;
        private bool isAttackingCoroutineRunning = false;
        private readonly List<int> availableIndexes = new();
        private float lastAttackEndTime;
        private IStatProvider esm;
        private GameObject Target => TryGetComponent<EnemyMovement>(out var em) ? em.target : null;

        private void Awake()
        {
            a = GetComponent<Animator>();

            var runtimeAttacks = new List<AttackData>();
            if (attacks != null)
            {
                foreach (var attack in attacks)
                {
                    if (attack != null)
                    {
                        var runtimeCopy = Instantiate(attack);
                        runtimeCopy.InitializeRuntimeCopy();
                        runtimeAttacks.Add(runtimeCopy);
                    }
                }
            }
            attacks = runtimeAttacks;

            cooldowns = new float[attacks.Count];
            for (int i = 0; i < attacks.Count; i++) cooldowns[i] = attacks[i].cooldown;
        }

        private void Start() => esm = GetComponent<IStatProvider>();

        private void OnDestroy()
        {
            if (attacks != null)
            {
                foreach (var attack in attacks)
                    if (attack != null && attack.IsRuntimeCopy) DestroyImmediate(attack, true);
                attacks.Clear();
            }
        }
        private void Update()
        {
            if (esm.GetStat(StatType.isAlive) != 1f|| Time.timeScale == 0f) return;
            UpdateCooldowns();
            if (esm.GetStat(StatType.IsAttacking) != 1f) TryAttack();
        }
        private void UpdateCooldowns()
        {
            for (int i = 0; i < attacks.Count; i++) if (cooldowns[i] > 0f) cooldowns[i] -= Time.deltaTime;
        }
        private void TryAttack()
        {
            if (attacks.Count == 0 || Target == null) return;
            if (globalCooldown > 0 && Time.time - lastAttackEndTime < globalCooldown) return;

            int chosen = ChooseAttackIndex();

            if (chosen == -1) return;

            if (!isAttackingCoroutineRunning) StartCoroutine(PerformAttack(attacks[chosen], chosen));
        }

        private int ChooseAttackIndex()
        {
            float dist = (Target.transform.position - transform.position).sqrMagnitude;

            availableIndexes.Clear();

            for (int i = 0; i < attacks.Count; i++)
            {
                AttackData a = attacks[i];

                if (cooldowns[i] > 0f || dist > (a.maxRange * a.maxRange)) continue;

                float hpPct = (float)esm.GetStat(StatType.currentHp) / esm.GetStat(StatType.EffMaxHp);

                if (a.minHpPct > 0 && hpPct < a.minHpPct) continue;
                if (a.maxHpPct < 100f && hpPct > a.maxHpPct) continue;
                if (a.phaseReq >= 0 && (TryGetComponent<EnemyPhase>(out var ep) && ep.phase < a.phaseReq)) continue;

                availableIndexes.Add(i);
            }

            if (availableIndexes.Count == 0) return -1;

            return availableIndexes[Random.Range(0, availableIndexes.Count)];
        }

        private System.Collections.IEnumerator PerformAttack(AttackData attack, int index)
        {
            if (esm.GetStat(StatType.CanAttack) <= 0f || esm.GetStat(StatType.isAlive) <= 0f) yield break;

            float attackStartTime = Time.time;

            isAttackingCoroutineRunning = true;
            esm.AddStat(new(StatType.IsAttacking, 1));
            esm.AddStat(new(StatType.CanMove, attack.canMoveDuringAttack ? 1 : -1));

            if (a != null) a.SetInteger(AttackIndexHash, index);

            HandleOrbitInteractions(attack);

            if (attack.projectilePrefab != null)
            {
                if (attack.spawnDelay > 0) yield return new WaitForSeconds(attack.spawnDelay);

                if (Target != null)
                {
                    Vector2 dir = (Target.transform.position - transform.position).normalized;
                    float dist = Vector2.Distance(Target.transform.position, transform.position);

                    StartCoroutine(ProjectileSpawner.Instance.SpawnFromPattern(
                        attack.projectilePrefab,
                        gameObject,
                        transform.position,
                        dir,
                        dist > attack.spawnDistance ? attack.spawnDistance : dist
                    ));
                }
            }

            if (attack.summonChance > 0f && attack.summonCondition == SummonCondition.OnCast && Random.value <= attack.summonChance)
            {
                if (TryGetComponent<EntitySummonHandler>(out var summonHandler))
                    summonHandler.Summon();
            }

            if (index >= 0) cooldowns[index] = attack.cooldown;

            if (attack.animationLength > 0)
            {
                float remaining = attack.animationLength - (Time.time - attackStartTime);
                if (remaining > 0) yield return new WaitForSeconds(remaining);
            }

            isAttackingCoroutineRunning = false;
            esm.AddStat(new(StatType.IsAttacking, -1));
            esm.AddStat(new(StatType.CanMove, 1));
            lastAttackEndTime = Time.time;
            if (a != null) a.SetInteger(AttackIndexHash, -1);

            if (attack.nextAttack != null) yield return PerformAttack(attack.nextAttack, -1);
        }

        private void HandleOrbitInteractions(AttackData attack)
        {
            if (attack == null) return;
            if (!TryGetComponent<EntityProjectileHandler>(out var handler)) return;

            if (attack.fireOrbits)
            {
                Vector2 dir = Target != null
                    ? ((Vector2)Target.transform.position - (Vector2)transform.position).normalized
                    : Vector2.right;
                handler.ReleaseOrbits(dir, attack.redirectCount);
            }
            else if (attack.absorbOrbitPct > 0f)
            {
                handler.AbsorbOrbits(attack.redirectCount, attack.absorbOrbitPct);
            }
            else if (attack.redirectOrbits)
            {
                handler.RedirectOrbits(attack.redirectCount);
            }
            else if (attack.explodeOrbits)
            {
                handler.ExplodeOrbits(attack.redirectCount);
            }
        }
    }
}
