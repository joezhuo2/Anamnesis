using System;
using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SettingsSystem;
using CrystalFlux.StatusEffectSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public partial class PlayerAttackHandler : MonoBehaviour, IAttackHandler, ICastHandler
    {
        bool IAttackHandler.HasAttack(AttackAsset a) => HasAttack(a as AttackData);
        AttackAsset IAttackHandler.FindAttackOfType(AttackType type) => FindAttackOfType(type);
        void IAttackHandler.UpdateAttack(AttackType type, AttackAsset newAttack) => UpdateAttack(newAttack as AttackData);
        void IAttackHandler.RemoveAttack(AttackType type) => RemoveAttack(type);

        private static readonly int AttackIndexHash = Animator.StringToHash("attackIndex");
        public static event Action<AttackData> AttackCast;
        public static float CostPct;
        public List<AttackData> starting = new();
        public GameObject cooldownPrefab;
        public Transform objContainer;

        [Header("Cast Bar")]
        public Slider castBarPrefab;
        public TextMeshProUGUI castBarTextPrefab;
        public Vector3 castBarOffset;

        [Header("Attack Queue")]
        public int maxQueuedAttacks = 1;
        public float queueExpiry = 0.3f;

        private Animator a;
        private IResourcePool pr;
        private IDamageable ph;
        private IStatProvider esm;
        private PlayerUpgradeManager pum;
        private PlayerMovement pm;
        private readonly Dictionary<AttackType, GameObject> spawnedUIElements = new();
        [HideInInspector] public List<AttackData> attacks = new();
        [HideInInspector] public readonly Dictionary<AttackType, float> lastAttackTimes = new();
        private readonly Dictionary<AttackType, int> stackCounts = new();
        private readonly List<AttackType> cdKeyBuffer = new();
        private readonly HashSet<AttackType> lockedSlots = new();
        private readonly HashSet<AttackType> permaLockedSlots = new();
        private int animResetGen;

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
        private readonly HashSet<AttackType> heldInputs = new();
        private readonly List<QueuedAttack> attackQueue = new();

        private struct QueuedAttack
        {
            public AttackType type;
            public float expireAt;
            public bool bypassCooldown;
            public bool noCost;
            public bool triggerUpgrades;
            public bool registerStreak;
        }

        public bool IsCasting => isCasting || isCharging;
        public bool IsCharging => isCharging;
        private bool RushLocked => pm != null && pm.RushBlocksAttacks;

        private void Start()
        {
            a = GetComponent<Animator>();
            esm = GetComponent<IStatProvider>();
            ph = GetComponent<IDamageable>();
            pr = GetComponent<IResourcePool>();
            pum = GetComponent<PlayerUpgradeManager>();
            pm = GetComponent<PlayerMovement>();

            for (int i = 0; i < starting.Count; i++) UpdateAttack(starting[i]);
        }
        private void Update()
        {
            if (Time.timeScale == 0f) return;
            if (attackQueue.Count == 0) return;

            if (esm == null || esm.GetStat(StatType.isAlive) <= 0f || esm.GetStat(StatType.CanAttack) <= 0f)
            {
                attackQueue.Clear();
                return;
            }

            if (Time.time >= attackQueue[0].expireAt)
            {
                attackQueue.RemoveAt(0);
                return;
            }

            if (isCasting || isCharging || RushLocked) return;

            QueuedAttack q = attackQueue[0];
            attackQueue.RemoveAt(0);
            PerformAttack(q.type, q.bypassCooldown, q.noCost, q.triggerUpgrades, q.registerStreak);
        }

        private void EnqueueAttack(AttackType type, bool bypassCooldown, bool noCost, bool triggerUpgrades, bool registerStreak)
        {
            if (maxQueuedAttacks <= 0) return;

            float expireAt = Time.time + queueExpiry;

            for (int i = 0; i < attackQueue.Count; i++)
            {
                if (attackQueue[i].type != type) continue;

                QueuedAttack existing = attackQueue[i];
                existing.expireAt = expireAt;
                attackQueue[i] = existing;
                return;
            }

            if (attackQueue.Count >= maxQueuedAttacks) return;

            attackQueue.Add(new QueuedAttack
            {
                type = type,
                expireAt = expireAt,
                bypassCooldown = bypassCooldown,
                noCost = noCost,
                triggerUpgrades = triggerUpgrades,
                registerStreak = registerStreak
            });
        }

        private void OnDisable() => EndAllAttackStates();

        private void OnDestroy()
        {
            EndAllAttackStates();

            attacks?.Clear();

            foreach (var kvp in spawnedUIElements)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
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

        public AttackData FindAttackOfType(AttackType type)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i] != null && attacks[i].type == type)
                    return attacks[i];
            }
            return null;
        }

        private void CreateButtonUI(AttackData attack) => CreateButtonUI(attack.type);

        private void CreateButtonUI(AttackType type)
        {
            GameObject uiObj = Instantiate(cooldownPrefab, objContainer);
            spawnedUIElements[type] = uiObj;

            if (uiObj.TryGetComponent<PlayerAttackCooldownUI>(out var pacui))
                pacui.Setup(this, type, esm);

            SortButtonUI();
        }

        private static readonly AttackType[] UIOrder = (AttackType[])Enum.GetValues(typeof(AttackType));

        private void SortButtonUI()
        {
            int idx = int.MaxValue;
            foreach (var kvp in spawnedUIElements)
            {
                if (kvp.Value != null && kvp.Value.transform.parent == objContainer)
                    idx = Mathf.Min(idx, kvp.Value.transform.GetSiblingIndex());
            }
            if (idx == int.MaxValue) return;

            for (int i = 0; i < UIOrder.Length; i++)
            {
                if (!spawnedUIElements.TryGetValue(UIOrder[i], out var uiObj) || uiObj == null || uiObj.transform.parent != objContainer) continue;
                uiObj.transform.SetSiblingIndex(idx++);
            }
        }

        public bool IsSlotLocked(AttackType type) => lockedSlots.Contains(type) || permaLockedSlots.Contains(type);
        public bool IsSlotPermaLocked(AttackType type) => permaLockedSlots.Contains(type);

        public void PermaLockSlot(AttackType type)
        {
            if (!permaLockedSlots.Add(type)) return;

            for (int i = attackQueue.Count - 1; i >= 0; i--)
                if (attackQueue[i].type == type) attackQueue.RemoveAt(i);

            if (spawnedUIElements.TryGetValue(type, out var uiObj) && uiObj != null)
            {
                if (uiObj.TryGetComponent<PlayerAttackCooldownUI>(out var pacui)) pacui.Setup(this, type, esm);
                return;
            }

            if (cooldownPrefab != null && objContainer != null) CreateButtonUI(type);
        }

        public void SetSlotLocked(AttackType type, bool locked)
        {
            if (!locked)
            {
                lockedSlots.Remove(type);
                return;
            }

            if (!lockedSlots.Add(type)) return;

            for (int i = attackQueue.Count - 1; i >= 0; i--)
                if (attackQueue[i].type == type) attackQueue.RemoveAt(i);
        }

        public IEnumerator ResetAttackType(float delay)
        {
            int gen = ++animResetGen;
            yield return new WaitForSeconds(delay);
            if (gen != animResetGen || a == null) yield break;
            a.SetInteger(AttackIndexHash, -1);
            a.speed = 1f;
        }

        private void NotifyBlocked(AttackType type)
        {
            if (!spawnedUIElements.TryGetValue(type, out var uiObj) || uiObj == null) return;
            if (uiObj.TryGetComponent<PlayerAttackCooldownUI>(out var pacui)) pacui.FlashBlocked();
        }

        public bool CanCast(AttackType type)
        {
            if (IsSlotLocked(type)) return false;
            if (esm == null || esm.GetStat(StatType.isAlive) <= 0f || esm.GetStat(StatType.CanAttack) <= 0f) return false;

            AttackData selected = FindAttackOfType(type);
            if (selected == null) return false;

            if (IsFreeCast(type)) return true;

            if (RefreshStacks(type, selected) <= 0) return false;

            return CanAfford(selected);
        }

        private FreeCast GetFreeCast() => pum != null ? pum.GetPlayerUpgradeOfType<FreeCast>() as FreeCast : null;

        public bool IsFreeCast(AttackType type)
        {
            FreeCast fc = GetFreeCast();
            return fc != null && fc.IsPending(type);
        }

        public int GetStacks(AttackType type)
        {
            AttackData selected = FindAttackOfType(type);
            return selected == null ? 0 : RefreshStacks(type, selected);
        }

        private int RefreshStacks(AttackType type, AttackData attack)
        {
            int max = attack.Stacks;

            if (!lastAttackTimes.TryGetValue(type, out float start))
            {
                stackCounts.Remove(type);
                return max;
            }

            int count = stackCounts.TryGetValue(type, out int stored) ? Mathf.Min(stored, max - 1) : 0;
            float effCd = GetEffCd(attack, esm);

            if (effCd <= 0f) count = max;
            else
            {
                while (count < max && Time.time - start >= effCd)
                {
                    count++;
                    start += effCd;
                }
            }

            if (count >= max)
            {
                lastAttackTimes.Remove(type);
                stackCounts.Remove(type);
                return max;
            }

            lastAttackTimes[type] = start;
            stackCounts[type] = count;
            return count;
        }

        private void ConsumeStack(AttackType type, AttackData attack)
        {
            if (attack == null)
            {
                lastAttackTimes[type] = Time.time;
                stackCounts[type] = 0;
                return;
            }

            int count = RefreshStacks(type, attack);

            if (count >= attack.Stacks || count <= 0) lastAttackTimes[type] = Time.time;
            stackCounts[type] = Mathf.Max(0, count - 1);
        }

        public bool CanAfford(AttackData attack)
        {
            if (attack == null || esm == null) return false;

            var (hp, sp, mp) = GetCosts(attack, esm);
            (hp, sp) = HandleHexCast(hp, sp);

            return sp <= esm.GetStat(StatType.CurrentStamina)
                && hp <= esm.GetStat(StatType.currentHp)
                && mp <= esm.GetStat(StatType.CurrentMana);
        }

        private void TriggerCostUpgrades()
        {
            if (pum != null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnCalculateAttackCost);
        }

        public bool HandleStatChanges(AttackData attack) => HandleStatChanges(attack, true);

        private bool HandleStatChanges(AttackData attack, bool triggerCostUpgrades)
        {
            if (attack == null) return false;

            if (triggerCostUpgrades) TriggerCostUpgrades();

            var (hp, sp, mp) = GetCosts(attack, esm);
            (hp, sp) = HandleHexCast(hp, sp);

            if (sp > esm.GetStat(StatType.CurrentStamina) || hp > esm.GetStat(StatType.currentHp) || mp > esm.GetStat(StatType.CurrentMana)) return false;

            var dp = DamagePacketBuilder.BuildDamagePacket(hp, DamageType.Consume, false, Color.red, gameObject, false, 1f);
            if (ph != null) ph.TakeDamage(dp);
            DamagePacket.Release(dp);
            if (hp > 0 && pum != null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnConsumeHealth);

            if (pr != null) pr.TrySpend(ResourceType.Stamina, sp);
            if (pr != null) pr.TrySpend(ResourceType.Mana, mp);

            return true;
        }

        public static (int hp, int sp, int mp) GetCosts(AttackData attack, IStatProvider esm)
        {
            if (attack == null || esm == null) return (0, 0, 0);

            float totalStaminaCost = Mathf.Abs(attack.StaminaCost + (esm.GetStat(StatType.EffMaxStamina) * (attack.StaminaCostPct * 0.01f))) * (1f + (esm.GetStat(StatType.stCostPct) * 0.01f));
            float totalHealthCost = Mathf.Abs(attack.HealthCost + (esm.GetStat(StatType.EffMaxHp) * (attack.HealthCostPct * 0.01f)));
            float totalManaCost = Mathf.Abs(attack.ManaCost + (esm.GetStat(StatType.EffMaxMana) * (attack.ManaCostPct * 0.01f)));

            float cm = Mathf.Max(0f, 1f + (CostPct * 0.01f));

            return (Mathf.RoundToInt(totalHealthCost * cm), Mathf.RoundToInt(totalStaminaCost * cm), Mathf.RoundToInt(totalManaCost * cm));
        }

        public void UpdateAttack(AttackData newAttack)
        {
            if (newAttack == null) return;
            if (newAttack.type == AttackType.Ultimate && !RunMode.UltimatesUnlocked) return;

            AttackType type = newAttack.type;
            AttackData current = FindAttackOfType(type);

            if (current != null) attacks.Remove(current);

            attacks.Add(newAttack);

            if (pum != null && pum.HasUpgradeOfType<SoulRendPU>() && (type == AttackType.Basic || type == AttackType.Skill))
                pum.GetPlayerUpgradeOfType<SoulRendPU>().OnUnlock(gameObject);

            if (spawnedUIElements.ContainsKey(type))
            {
                Destroy(spawnedUIElements[type]);
                spawnedUIElements.Remove(type);
            }
            CreateButtonUI(newAttack);
        }

        public void RemoveAttack(AttackType type)
        {
            AttackData current = FindAttackOfType(type);
            if (current != null) attacks.Remove(current);
            lastAttackTimes.Remove(type);
            stackCounts.Remove(type);

            if (spawnedUIElements.ContainsKey(type))
            {
                Destroy(spawnedUIElements[type]);
                spawnedUIElements.Remove(type);
            }

            if (permaLockedSlots.Contains(type) && cooldownPrefab != null && objContainer != null) CreateButtonUI(type);
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
            cdKeyBuffer.Clear();
            foreach (var type in lastAttackTimes.Keys) cdKeyBuffer.Add(type);
            for (int i = 0; i < cdKeyBuffer.Count; i++) AdvanceCooldown(cdKeyBuffer[i], pctAmt);
        }

        public void AdvanceCooldown(AttackType type, float pctAmt)
        {
            if (!lastAttackTimes.ContainsKey(type)) return;

            AttackData attack = FindAttackOfType(type);
            var effCd = GetEffCd(attack, esm);

            if (effCd <= 0f) return;

            RefreshStacks(type, attack);
            if (!lastAttackTimes.TryGetValue(type, out float lastTime)) return;

            float timeElapsed = Time.time - lastTime;
            float cooldownRemainingPct = 1f - (timeElapsed / effCd);
            float newCooldownRemainingPct = Mathf.Clamp01(cooldownRemainingPct - (pctAmt * 0.01f));
            float newLastTime = Time.time - ((1f - newCooldownRemainingPct) * effCd);

            lastAttackTimes[type] = newLastTime;
            RefreshStacks(type, attack);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            AttackCast = null;
            CostPct = 0f;
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

            return attack.Cooldown *
                Mathf.Clamp(1f - (esm.GetStat(StatType.attackSpeedPct) * 0.01f), 0.3f, 10f) *
                Mathf.Clamp(1f - (cdrPct * 0.01f), 0.1f, 1f);
        }
    }
}
