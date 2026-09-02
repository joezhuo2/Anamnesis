using System;
using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.StatusEffectSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public class PlayerAttackHandler : MonoBehaviour, IAttackHandler, ICastHandler
    {
        bool IAttackHandler.HasAttack(AttackAsset a) => HasAttack(a as AttackData);
        AttackAsset IAttackHandler.FindAttackOfType(AttackType type) => FindAttackOfType(type);
        void IAttackHandler.UpdateAttack(AttackType type, AttackAsset newAttack) => UpdateAttack(type, newAttack as AttackData);
        void IAttackHandler.RemoveAttack(AttackType type) => RemoveAttack(type);

        private static readonly int AttackIndexHash = Animator.StringToHash("attackIndex");
        public List<AttackData> starting = new();
        public GameObject cooldownPrefab;
        public Transform objContainer;

        [Header("Cast Bar")]
        public Slider castBarPrefab;
        public TextMeshProUGUI castBarTextPrefab;
        public Vector3 castBarOffset;

        private Animator a;
        private IResourcePool pr;
        private IDamageable ph;
        private IStatProvider esm;
        private PlayerUpgradeManager pum;
        private readonly Dictionary<AttackType, GameObject> spawnedUIElements = new();
        [HideInInspector] public List<AttackData> attacks = new();
        [HideInInspector] public readonly Dictionary<AttackType, float> lastAttackTimes = new();

        private bool isCasting;
        private bool castCancelled;
        private bool castMovementHeld;
        private bool castStateHeld;
        private Slider castBarInstance;
        private TextMeshProUGUI castBarTextInstance;
        private bool isCharging;
        private bool chargeReleaseRequested;
        private AttackType chargingType;
        private AttackData chargingAttack;
        private readonly List<AttackData> pendingDestroy = new();
        private static readonly HashSet<AttackData> inUseVisited = new();

        public bool IsCasting => isCasting || isCharging;
        public bool IsCharging => isCharging;

        private void Start()
        {
            a = GetComponent<Animator>();
            esm = GetComponent<IStatProvider>();
            ph = GetComponent<IDamageable>();
            pr = GetComponent<IResourcePool>();
            pum = GetComponent<PlayerUpgradeManager>();

            for (int i = 0; i < starting.Count; i++) UpdateAttack(starting[i].type, starting[i]);
        }
        private void OnDisable() => EndAllAttackStates();

        private void OnDestroy()
        {
            EndAllAttackStates();

            foreach (var attack in pendingDestroy)
                if (attack != null) Destroy(attack);
            pendingDestroy.Clear();

            if (attacks != null)
            {
                foreach (var attack in attacks)
                    if (attack != null && attack.IsRuntimeCopy) Destroy(attack);
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

        public static string NormalizeAttackName(string n)
        {
            if (string.IsNullOrEmpty(n)) return string.Empty;
            n = n.Trim();
            while (n.EndsWith("(Clone)", StringComparison.Ordinal))
                n = n.Substring(0, n.Length - 7).TrimEnd();
            return n;
        }

        public bool HasAttack(AttackData a)
        {
            if (a == null) return false;
            string n = NormalizeAttackName(a.name);
            if (n.Length == 0) return false;

            for (int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i] == null) continue;
                if (NormalizeAttackName(attacks[i].name).Equals(n, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private void DestroyAttackDeferred(AttackData attack)
        {
            if (attack == null || !attack.IsRuntimeCopy) return;

            if (!isActiveAndEnabled)
            {
                Destroy(attack);
                return;
            }

            pendingDestroy.Add(attack);
            StartCoroutine(DestroyWhenUnused(attack));
        }

        private IEnumerator DestroyWhenUnused(AttackData attack)
        {
            yield return null;

            while (attack != null && IsAttackDataInUse(attack)) yield return null;

            pendingDestroy.Remove(attack);
            if (attack != null) Destroy(attack);
        }

        private static bool IsAttackDataInUse(AttackData attack)
        {
            inUseVisited.Clear();
            return IsAttackDataInUseInternal(attack);
        }

        private static bool IsAttackDataInUseInternal(AttackData attack)
        {
            if (attack == null || !inUseVisited.Add(attack)) return false;

            if (Projectile.IsDataLive(attack.pd)) return true;
            if (attack.pd != null && IsAttackDataInUseInternal(attack.pd.additionalAttack)) return true;
            if (IsAttackDataInUseInternal(attack.chargeAttack)) return true;
            if (IsAttackDataInUseInternal(attack.nextAttack)) return true;

            return false;
        }

        public AttackData FindAttackOfType(AttackType type)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i] != null && attacks[i].type == type)
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
            if (isCasting || isCharging) return;
            if (esm == null || esm.GetStat(StatType.isAlive) <= 0f || esm.GetStat(StatType.CanAttack) <= 0f || Time.timeScale == 0f) return;

            AttackData selected = attacks.Find(atk => atk.type == type);
            if (selected == null) return;

            if (!bypassCooldown)
            {
                float lastTime = lastAttackTimes.ContainsKey(type) ? lastAttackTimes[type] : -Mathf.Infinity;
                if (Time.time - lastTime < GetEffCd(selected, esm)) return;
            }

            float castTime = selected.GetEffCastTime(esm);
            bool stampCooldownNow = !bypassCooldown && (!selected.canCharge || selected.cooldownOnAttackStart);

            if (castTime > 0f)
            {
                if (stampCooldownNow) lastAttackTimes[type] = Time.time;

                StartCoroutine(CastRoutine(selected, type, castTime, noCost, triggerUpgrades, bypassCooldown));
                return;
            }

            if (!noCost && !selected.canCharge && !HandleStatChanges(selected)) return;

            if (stampCooldownNow) lastAttackTimes[type] = Time.time;

            ExecuteAttack(selected, type, triggerUpgrades, noCost, bypassCooldown);
        }

        private IEnumerator CastRoutine(AttackData selected, AttackType type, float castTime, bool noCost, bool triggerUpgrades, bool bypassCooldown)
        {
            isCasting = true;
            castCancelled = false;

            castStateHeld = true;
            esm.AddStat(new StatBuff(StatType.IsAttacking, 1f));

            if (!selected.canMoveWhileCasting)
            {
                castMovementHeld = true;
                esm.AddStat(new StatBuff(StatType.CanMove, -1f));
            }

            ApplyAttackAnimator(type);
            CastBar.Acquire(castBarPrefab, castBarTextPrefab, out castBarInstance, out castBarTextInstance);

            float elapsed = 0f;

            while (elapsed < castTime)
            {
                if (esm.GetStat(StatType.isAlive) <= 0f) castCancelled = true;
                else if (esm.GetStat(StatType.interruptResist) < 2f && esm.GetStat(StatType.CanAttack) <= 0f) castCancelled = true;

                if (castCancelled) break;

                CastBar.Tick(castBarInstance, castBarTextInstance, transform, castBarOffset, elapsed, castTime);

                yield return null;
                elapsed += Time.deltaTime;
            }

            bool completed = !castCancelled;
            EndCast();

            if (completed && (noCost || selected.canCharge || HandleStatChanges(selected)))
            {
                ExecuteAttack(selected, type, triggerUpgrades, noCost, bypassCooldown);
                yield break;
            }

            if (a != null)
            {
                a.SetInteger(AttackIndexHash, -1);
                a.speed = 1f;
            }
        }

        private void EndCast()
        {
            CastBar.Release(ref castBarInstance, ref castBarTextInstance);

            if (castMovementHeld)
            {
                castMovementHeld = false;
                if (esm != null) esm.AddStat(new StatBuff(StatType.CanMove, 1f));
            }

            if (castStateHeld)
            {
                castStateHeld = false;
                if (esm != null) esm.AddStat(new StatBuff(StatType.IsAttacking, -1f));
            }

            isCasting = false;
            castCancelled = false;
        }

        public void CancelCast()
        {
            if (!isCasting && !isCharging) return;
            if (esm != null && esm.GetStat(StatType.interruptResist) >= 1f) return;

            castCancelled = true;
        }

        private void ExecuteAttack(AttackData selected, AttackType type, bool triggerUpgrades, bool noCost = false, bool bypassCooldown = false)
        {
            HandleOrbitInteractions(selected);
            HandleCleanse(selected);

            if (!selected.canCharge) SpawnAttack(selected);

            if (selected.summonChance > 0f && selected.summonCondition == SummonCondition.OnCast && UnityEngine.Random.value <= selected.summonChance)
            {
                if (TryGetComponent<EntitySummonHandler>(out var summonHandler))
                    summonHandler.Summon();
            }

            if (triggerUpgrades)
                TriggerUpgradesOnAttack(type);

            ApplyAttackAnimator(type);
            StartCoroutine(ResetAttackType(selected.animationLength));

            if (selected.canCharge) StartCoroutine(ChargeRoutine(selected, type, noCost, bypassCooldown));
        }

        private void HandleCleanse(AttackData ad)
        {
            if (ad.cleanseDebuffs <= 0) return;
            if (TryGetComponent<StatusEffectManager>(out var sem)) sem.RemoveDebuffs(ad.cleanseDebuffs);
        }

        private void SpawnAttack(AttackData ad)
        {
            ProjectileSpawner ps = ProjectileSpawner.Instance;
            if (ps != null) StartCoroutine(ps.SpawnFromPattern(ad, gameObject, transform.position));
        }

        public void ReleaseAttack(AttackType type)
        {
            if (isCharging && chargingType == type) chargeReleaseRequested = true;
        }

        private IEnumerator ChargeRoutine(AttackData selected, AttackType type, bool noCost, bool bypassCooldown)
        {
            isCharging = true;
            chargeReleaseRequested = false;
            chargingType = type;
            chargingAttack = selected;
            castCancelled = false;

            float maxTime = Mathf.Max(selected.maxChargeTime, selected.minChargeTime);
            float interval = Mathf.Max(selected.chargeTickInterval, 0.05f);
            float elapsed = 0f;

            while (elapsed < selected.chargeThreshold)
            {
                if (esm.GetStat(StatType.isAlive) <= 0f) castCancelled = true;
                else if (esm.GetStat(StatType.interruptResist) < 2f && esm.GetStat(StatType.CanAttack) <= 0f) castCancelled = true;

                if (castCancelled || chargeReleaseRequested)
                {
                    if (noCost || HandleStatChanges(selected)) SpawnAttack(selected);
                    EndCharge(type, bypassCooldown);
                    yield break;
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            AttackData chargeSource = selected.chargeAttack != null ? selected.chargeAttack : selected;

            if (!noCost && !HandleStatChanges(chargeSource))
            {
                EndCharge(type, bypassCooldown);
                yield break;
            }

            castStateHeld = true;
            esm.AddStat(new StatBuff(StatType.IsAttacking, 1f));

            if (!selected.canMoveWhileCasting)
            {
                castMovementHeld = true;
                esm.AddStat(new StatBuff(StatType.CanMove, -1f));
            }

            CastBar.Acquire(castBarPrefab, castBarTextPrefab, out castBarInstance, out castBarTextInstance);

            SpawnAttack(chargeSource);

            TryGetComponent<EntityProjectileHandler>(out var eph);

            float chargeElapsed = 0f;
            float sinceTick = 0f;

            while (chargeElapsed < maxTime)
            {
                if (esm.GetStat(StatType.isAlive) <= 0f) castCancelled = true;
                else if (esm.GetStat(StatType.interruptResist) < 2f && esm.GetStat(StatType.CanAttack) <= 0f) castCancelled = true;

                if (castCancelled) break;
                if (chargeReleaseRequested && chargeElapsed >= selected.minChargeTime) break;

                CastBar.Tick(castBarInstance, castBarTextInstance, transform, castBarOffset, chargeElapsed, maxTime);

                yield return null;

                chargeElapsed += Time.deltaTime;
                sinceTick += Time.deltaTime;

                if (sinceTick < interval) continue;

                sinceTick -= interval;

                if (!noCost && !HandleStatChanges(chargeSource)) break;

                if (eph != null) eph.TickChargedProjectiles(chargeSource);
            }

            EndCharge(type, bypassCooldown);
        }

        private void EndCharge(AttackType type, bool bypassCooldown)
        {
            CastBar.Release(ref castBarInstance, ref castBarTextInstance);

            if (castMovementHeld)
            {
                castMovementHeld = false;
                if (esm != null) esm.AddStat(new StatBuff(StatType.CanMove, 1f));
            }

            if (castStateHeld)
            {
                castStateHeld = false;
                if (esm != null) esm.AddStat(new StatBuff(StatType.IsAttacking, -1f));
            }

            isCharging = false;
            chargeReleaseRequested = false;
            castCancelled = false;

            if (!bypassCooldown && (chargingAttack == null || !chargingAttack.cooldownOnAttackStart))
                lastAttackTimes[type] = Time.time;

            chargingAttack = null;

            if (a != null)
            {
                a.SetInteger(AttackIndexHash, -1);
                a.speed = 1f;
            }
        }

        private void EndAllAttackStates()
        {
            if (isCharging) EndCharge(chargingType, true);
            EndCast();
        }

        private void ApplyAttackAnimator(AttackType type)
        {
            if (a == null) return;

            int attackIndex = type switch
            {
                AttackType.Basic => 0,
                AttackType.Skill => 1,
                AttackType.Ultimate => 2,
                _ => -1
            };

            a.SetInteger(AttackIndexHash, attackIndex);
            a.speed = Mathf.Max(0.1f, 1f + (esm.GetStat(StatType.attackSpeedPct) * 0.01f));
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
            if (pum == null) return;

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

            if (pum != null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnCalculateAttackCost);

            var (hp, sp, mp) = GetCosts(attack, esm);
            (hp, sp) = HandleHexCast(hp, sp);

            if (sp > esm.GetStat(StatType.CurrentStamina) || hp > esm.GetStat(StatType.currentHp) || mp > esm.GetStat(StatType.CurrentMana)) return false;

            var dp = DamagePacketBuilder.BuildDamagePacket(hp, DamageType.Consume, false, Color.red, gameObject, false, 1f);
            if (ph != null) ph.TakeDamage(dp);

            if (pr != null) pr.TrySpend(ResourceType.Stamina, sp);
            if (pr != null) pr.TrySpend(ResourceType.Mana, mp);

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
                DestroyAttackDeferred(current);
            }

            AttackData runtimeAttackCopy = Instantiate(newAttack);
            runtimeAttackCopy.name = NormalizeAttackName(newAttack.name);
            runtimeAttackCopy.type = type;
            runtimeAttackCopy.InitializeRuntimeCopy();

            attacks.Add(runtimeAttackCopy);

            if (pum != null && pum.HasUpgradeOfType<SoulRendPU>() && (type == AttackType.Basic || type == AttackType.Skill))
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
                DestroyAttackDeferred(current);
            }
            lastAttackTimes.Remove(type);

            if (spawnedUIElements.ContainsKey(type))
            {
                Destroy(spawnedUIElements[type]);
                spawnedUIElements.Remove(type);
            }
        }

        private (int finalHpCost, int finalStaminaCost) HandleHexCast(float hpCost, float staminaCost)
        {
            if (pum == null || !pum.HasUpgradeOfType<HexCast>() || esm.GetStat(StatType.CurrentStamina) >= staminaCost)
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
            if (attack == null || esm == null) return 0f;

            float cdrPct = attack.type switch
            {
                AttackType.Basic => esm.GetStat(StatType.basicCdRedPct),
                AttackType.Skill => esm.GetStat(StatType.skillCdRedPct),
                AttackType.Ultimate => esm.GetStat(StatType.ultCdRedPct),
                _ => 0f
            };

            return attack.cooldown *
                Mathf.Clamp(1f - (esm.GetStat(StatType.attackSpeedPct) * 0.01f), 0.3f, 10f) *
                Mathf.Clamp(1f - (cdrPct * 0.01f), 0.1f, 1f);
        }
    }
}
