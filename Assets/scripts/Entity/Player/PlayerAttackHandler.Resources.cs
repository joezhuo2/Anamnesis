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
    public partial class PlayerAttackHandler
    {
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
