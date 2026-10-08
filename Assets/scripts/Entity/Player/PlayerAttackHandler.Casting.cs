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
        public void PerformAttack(AttackType type, bool bypassCooldown = false, bool noCost = false, bool triggerUpgrades = true, bool registerStreak = true)
        {
            if (IsSlotLocked(type))
            {
                NotifyBlocked(type);
                return;
            }

            if (isCasting || isCharging || RushLocked)
            {
                EnqueueAttack(type, bypassCooldown, noCost, triggerUpgrades, registerStreak);
                return;
            }

            if (esm == null || Time.timeScale == 0f) return;

            if (esm.GetStat(StatType.isAlive) <= 0f) return;

            if (esm.GetStat(StatType.CanAttack) <= 0f)
            {
                NotifyBlocked(type);
                return;
            }

            AttackData selected = FindAttackOfType(type);
            if (selected == null) return;

            if (triggerUpgrades && registerStreak && IsFreeCast(type))
            {
                bypassCooldown = true;
                noCost = true;
            }

            if (!bypassCooldown && RefreshStacks(type, selected) <= 0)
            {
                NotifyBlocked(type);
                return;
            }

            float castTime = selected.GetEffCastTime(esm);
            bool stampCooldownNow = !bypassCooldown && (!selected.CanCharge || selected.CooldownOnAttackStart);
            bool deferredCost = castTime > 0f || selected.CanCharge;
            bool costUpgradesTriggered = false;

            if (!noCost && deferredCost)
            {
                TriggerCostUpgrades();
                costUpgradesTriggered = true;

                if (!CanAfford(selected))
                {
                    NotifyBlocked(type);
                    return;
                }
            }

            if (castTime > 0f)
            {
                if (stampCooldownNow) ConsumeStack(type, selected);

                StartCoroutine(CastRoutine(selected, type, castTime, noCost, triggerUpgrades, bypassCooldown, costUpgradesTriggered, registerStreak));
                return;
            }

            if (!noCost && !selected.CanCharge && !HandleStatChanges(selected))
            {
                NotifyBlocked(type);
                return;
            }

            if (stampCooldownNow) ConsumeStack(type, selected);

            ExecuteAttack(selected, type, triggerUpgrades, noCost, bypassCooldown, costUpgradesTriggered, registerStreak);
        }

        private IEnumerator CastRoutine(AttackData selected, AttackType type, float castTime, bool noCost, bool triggerUpgrades, bool bypassCooldown, bool costUpgradesTriggered, bool registerStreak)
        {
            isCasting = true;
            castCancelled = false;

            castStateHeld = true;
            esm.AddStat(new StatBuff(StatType.IsAttacking, 1f));

            if (!selected.CanMoveWhileCasting)
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

            if (completed)
            {
                bool paid = noCost || selected.CanCharge;

                if (!paid)
                {
                    paid = HandleStatChanges(selected, !costUpgradesTriggered);
                    costUpgradesTriggered = false;
                }

                if (paid)
                {
                    ExecuteAttack(selected, type, triggerUpgrades, noCost, bypassCooldown, costUpgradesTriggered, registerStreak);
                    yield break;
                }
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

        private void ExecuteAttack(AttackData selected, AttackType type, bool triggerUpgrades, bool noCost = false, bool bypassCooldown = false, bool costUpgradesTriggered = false, bool registerStreak = true)
        {
            HandleCleanse(selected);

            if (!selected.CanCharge)
            {
                HandleOrbitInteractions(selected);
                HandleOnCastSummon(selected);
                SpawnAttack(selected);
            }

            if (selected.Rushes && pm != null) pm.StartRush(selected);

            if (triggerUpgrades && registerStreak) AttackCast?.Invoke(selected);

            if (triggerUpgrades)
            {
                FreeCast fc = GetFreeCast();
                if (fc != null && registerStreak) fc.RegisterCast(gameObject, type);
                TriggerUpgradesOnAttack(type);
            }

            ApplyAttackAnimator(type);
            StartCoroutine(ResetAttackType(selected.AnimationLength));

            if (selected.CanCharge) StartCoroutine(ChargeRoutine(selected, type, noCost, bypassCooldown, costUpgradesTriggered));
        }

        private void HandleCleanse(AttackData ad)
        {
            if (ad.CleanseDebuffs <= 0) return;
            if (TryGetComponent<StatusEffectManager>(out var sem)) sem.RemoveDebuffs(ad.CleanseDebuffs);
        }

        private void SpawnAttack(AttackData ad)
        {
            ProjectileSpawner ps = ProjectileSpawner.Instance;
            if (ps == null) return;

            Vector2 c = transform.position;
            ps.Spawn(ad, gameObject, c, host: this);
            MirageClone.NotifyCast(gameObject, ad, c);
        }

        public void PressAttack(AttackType type, bool bypassCooldown = false, bool noCost = false, bool triggerUpgrades = true)
        {
            heldInputs.Add(type);
            PerformAttack(type, bypassCooldown, noCost, triggerUpgrades);
        }

        public void ReleaseAttack(AttackType type)
        {
            heldInputs.Remove(type);
            if (isCharging && chargingType == type) chargeReleaseRequested = true;
        }

        private IEnumerator ChargeRoutine(AttackData selected, AttackType type, bool noCost, bool bypassCooldown, bool costUpgradesTriggered)
        {
            isCharging = true;
            chargeReleaseRequested = !heldInputs.Contains(type);
            chargingType = type;
            chargingAttack = selected;
            castCancelled = false;

            float maxTime = Mathf.Max(selected.MaxChargeTime, selected.MinChargeTime);
            float interval = Mathf.Max(selected.ChargeTickInterval, 0.05f);
            float elapsed = 0f;

            while (elapsed < selected.ChargeThreshold)
            {
                if (esm.GetStat(StatType.isAlive) <= 0f) castCancelled = true;
                else if (esm.GetStat(StatType.interruptResist) < 2f && esm.GetStat(StatType.CanAttack) <= 0f) castCancelled = true;

                if (castCancelled || chargeReleaseRequested)
                {
                    if (!castCancelled && (noCost || HandleStatChanges(selected, !costUpgradesTriggered)))
                    {
                        HandleOrbitInteractions(selected);
                        HandleOnCastSummon(selected);
                        SpawnAttack(selected);
                    }

                    EndCharge(type, bypassCooldown);
                    yield break;
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            AttackData chargeSource = selected.ChargeAttack != null ? selected.ChargeAttack : selected;

            if (!noCost && !HandleStatChanges(chargeSource, !costUpgradesTriggered))
            {
                EndCharge(type, bypassCooldown);
                yield break;
            }

            costUpgradesTriggered = false;

            castStateHeld = true;
            esm.AddStat(new StatBuff(StatType.IsAttacking, 1f));

            if (!selected.CanMoveWhileCasting && !selected.Rushes)
            {
                castMovementHeld = true;
                esm.AddStat(new StatBuff(StatType.CanMove, -1f));
            }

            CastBar.Acquire(castBarPrefab, castBarTextPrefab, out castBarInstance, out castBarTextInstance);

            TryGetComponent<EntityProjectileHandler>(out var eph);
            if (eph != null) eph.BeginChargeWindow(chargeSource);

            HandleOrbitInteractions(chargeSource);
            HandleOnCastSummon(chargeSource);
            SpawnAttack(chargeSource);

            float chargeElapsed = 0f;
            float sinceTick = 0f;

            while (chargeElapsed < maxTime)
            {
                if (esm.GetStat(StatType.isAlive) <= 0f) castCancelled = true;
                else if (esm.GetStat(StatType.interruptResist) < 2f && esm.GetStat(StatType.CanAttack) <= 0f) castCancelled = true;

                if (castCancelled) break;
                if (chargeReleaseRequested && chargeElapsed >= selected.MinChargeTime) break;

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
            if (TryGetComponent<EntityProjectileHandler>(out var eph)) eph.EndChargeWindow();

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

            if (!bypassCooldown && (chargingAttack == null || !chargingAttack.CooldownOnAttackStart))
                ConsumeStack(type, chargingAttack != null ? chargingAttack : FindAttackOfType(type));

            chargingAttack = null;

            if (a != null)
            {
                a.SetInteger(AttackIndexHash, -1);
                a.speed = 1f;
            }
        }

        private void EndAllAttackStates()
        {
            attackQueue.Clear();

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

            if (attack.FireOrbits) handler.ReleaseOrbits(attack.RedirectCount);
            else if (attack.AbsorbOrbitPct > 0f) handler.AbsorbOrbits(attack.RedirectCount, attack.AbsorbOrbitPct);
            else if (attack.RedirectOrbits) handler.RedirectOrbits(attack.RedirectCount);
            else if (attack.ExplodeOrbits) handler.ExplodeOrbits(attack.RedirectCount);
        }

        private void HandleOnCastSummon(AttackData ad)
        {
            if (ad == null || ad.SummonChance <= 0f || ad.SummonCondition != SummonCondition.OnCast) return;
            if (UnityEngine.Random.value > ad.SummonChance) return;

            if (TryGetComponent<EntitySummonHandler>(out var summonHandler))
                summonHandler.Summon();
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
    }
}
