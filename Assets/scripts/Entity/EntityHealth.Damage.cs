using System;
using System.Collections;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SettingsSystem;
using CrystalFlux.StatusEffectSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public partial class EntityHealth
    {
        public void SetOverhealth(float convPct, float decayPct, float decayInterval, bool convertRegen)
        {
            overhealthConvPct = Mathf.Max(0f, convPct);
            overhealthDecayPct = Mathf.Max(0f, decayPct);
            overhealthDecayInterval = Mathf.Max(0f, decayInterval);
            overhealthDecayTimer = 0f;
            regenOverHealth = convertRegen;
            if (overhealthConvPct <= 0f) AddOverhealth(-Overhealth);
        }

        public void GrantOverhealth(float amount)
        {
            if (amount > 0f) AddOverhealth(amount);
        }

        private void AddOverhealth(float delta)
        {
            if (esm == null || delta == 0f) return;
            esm.AddStat(new StatBuff(StatType.overhealth, delta));
            RefreshHealthBar();
        }

        private void DecayOverhealth()
        {
            if (Time.timeScale == 0f) return;
            if (overhealthDecayPct <= 0f || overhealthDecayInterval <= 0f) return;

            float cur = Overhealth;
            if (cur <= 0f) return;

            overhealthDecayTimer += Time.deltaTime;
            if (overhealthDecayTimer < overhealthDecayInterval) return;

            overhealthDecayTimer -= overhealthDecayInterval;

            float loss = cur * overhealthDecayPct * 0.01f;
            if (cur - loss < 1f) loss = cur;
            AddOverhealth(-loss);
        }

        public void TakeDamage(DamagePacket dp)
        {
            if (dp == null || !IsAlive) return;

            bool tookHit = false;
            bool tookDamage = false;
            bool counterDodged = false;
            bool prevSuppress = _suppressHurtIFrames;
            bool prevPending = _pendingHurtIFrames;
            _suppressHurtIFrames = true;
            _pendingHurtIFrames = false;

            GameObject lastOwner = null;
            bool ownerResolved = false;
            IStatProvider atk = null;
            PlayerUpgradeManager pum = null;
            int atkTeam = 0;
            GameObject src = OwnerProxy.Resolve(dp.source);

            try
            {
            foreach (var i in dp.instances)
            {
                if (!ownerResolved || i.owner != lastOwner)
                {
                    ownerResolved = true;
                    lastOwner = i.owner;
                    atk = null;
                    pum = null;
                    atkTeam = 0;
                    if (i.owner != null)
                    {
                        i.owner.TryGetComponent(out atk);
                        OwnerProxy.Resolve(i.owner).TryGetComponent(out pum);
                        atkTeam = i.owner.TryGetComponent<ITeamMember>(out var oitm) ? oitm.TeamID : 0;
                    }
                }

                var (dmg, sizeMult) = i.type switch
                {
                    DamageType.True => (i.amount, 1f),
                    DamageType.Physical => DamageCalculator.CalculateDamageTaken(i.type, i.amount, esm, atk, dp),
                    DamageType.Spell => DamageCalculator.CalculateDamageTaken(i.type, i.amount, esm, atk, dp),
                    DamageType.DoT => (i.amount * (1f - (esm.GetStat(StatType.EffectRes) * 0.01f)), 1f),
                    DamageType.Heal => (-i.amount, 1f),
                    DamageType.Consume => (i.amount, 1f),
                    _ => (0f, 1f)
                };

                bool consume = i.type == DamageType.Consume;
                if (dmg > 0f && !consume)
                {
                    dmg *= DamageTakenMult;
                    if (EnemyGlobalBuffs.AppliesTo(esm)) dmg *= EnemyGlobalBuffs.DamageTakenMult;
                }

                if (Immune && !dp.bypassIFrames && dmg > 0 && !consume)
                {
                    if (!counterDodged && cpum != null && esm.GetStat(StatType.IsDashing) > 0f && IsEnemyHit(dp, i, atkTeam))
                    {
                        counterDodged = true;
                        cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnCounterDodge);
                    }
                    continue;
                }

                Color color = i.indicatorColor != default ? i.indicatorColor : i.type switch
                {
                    DamageType.Physical => Color.gray,
                    DamageType.Spell => Color.purple,
                    DamageType.True => Color.lightBlue,
                    _ => Color.white
                };

                if (pum != null && !consume)
                    pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnTargetReceivedHit);

                if (cpum != null && dmg > 0)
                    cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnTakeDamage);

                bool enemyHit = dmg > 0 && IsEnemyHit(dp, i, atkTeam);

                bool directHit = enemyHit && (Projectile.ApplyingProjectileHit || RushState.ApplyingImpact);

                if (cpum != null && enemyHit)
                {
                    cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnTakeHit);
                    if (directHit) tookHit = true;
                }

                if (cpum != null && dmg > 0 && i.type == DamageType.DoT && i.owner != null && i.owner != gameObject && (ownTeam != null ? ownTeam.TeamID : 0) != atkTeam)
                    tookDamage = true;

                if (directHit)
                {
                    TryThorns(i.owner, dmg);
                    if (castHandler != null) castHandler.CancelCast();
                }

                if (i.isCrit)
                {
                    if (pum!= null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnCrit, gameObject);

                    sizeMult *= 1.5f;
                }

                if (dp.sizeOverride != 1f) sizeMult = dp.sizeOverride;

                killSrc = !isMirage && !consume && src != gameObject ? src : null;
                bool died = ChangeHealth(-dmg, true, sizeMult, color, dp.bypassIFrames || consume, src, consume);
                killSrc = null;
                if (died && !isMirage && !consume)
                {
                    if (GameSettings.Current.xpDropsEnabled && src != null && src.TryGetComponent<PlayerLevel>(out var pl))
                        pl.GainExp(esm.GetStat(StatType.XpDrop) * (Mathf.Pow(1.05f, esm.GetStat(StatType.Level) - 1)) * UnityEngine.Random.Range(0.8f, 1.2f));
                    DropGold(src);
                }

                if (!_isTriggeringOnDealDamage && pum != null && !consume)
                {
                    _isTriggeringOnDealDamage = true;
                    pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnDealDamage, gameObject, dmg);
                    _isTriggeringOnDealDamage = false;
                }

                if (dmg > 0 && i.owner != null && i.owner != gameObject) TryLifesteal(OwnerProxy.Resolve(i.owner), dmg);
                if (dmg > 0 && !consume && pum != null && cpum == null && !isMirage && atkTeam != (ownTeam != null ? ownTeam.TeamID : 0)) DpsMeter.Record(dmg);
            }
            }
            finally
            {
                bool fireIFrames = _pendingHurtIFrames;
                _suppressHurtIFrames = prevSuppress;
                _pendingHurtIFrames = prevPending || (prevSuppress && fireIFrames);
                if (fireIFrames && !prevSuppress) TriggerIFrames(hurtIFrameDuration);
            }

            if (cpum != null && tookHit) PlayerEvents.RaisePlayerTakeDamage(this);
            if (cpum != null && (tookHit || tookDamage)) PlayerEvents.RaisePlayerDamaged(this);
        }

        private static void TryLifesteal(GameObject dealer, float damageDealt)
        {
            if (!dealer.TryGetComponent<IStatusEffectReceiver>(out var sem)) return;
            if (sem.GetActiveFirstEffectOfType<Lifesteal>() is Lifesteal ls) ls.TryLifesteal(damageDealt);
        }

        private void TryThorns(GameObject attacker, float damageTaken)
        {
            if (attacker == null || ownSem == null) return;
            if (ownSem.GetActiveFirstEffectOfType<Thorns>() is Thorns th) th.TryReflect(attacker, damageTaken);
        }

        private void TryStoreBlood(int healthLost)
        {
            if (healthLost <= 0 || ownSem == null) return;
            if (ownSem.GetActiveFirstEffectOfType<BloodPool>() is BloodPool bp) bp.Store(healthLost);
        }

        private bool IsEnemyHit(DamagePacket dp, DamageInstance i, int atkTeam)
        {
            if (dp.bypassIFrames) return false;
            if (i.type != DamageType.Physical && i.type != DamageType.Spell && i.type != DamageType.True) return false;
            if (i.owner == null || i.owner == gameObject) return false;

            return (ownTeam != null ? ownTeam.TeamID : 0) != atkTeam;
        }

        public bool ChangeHealth(float amount, bool showIndicator = true, float sizeMult = 1f, Color colorOverride = default, bool bypassIFrames = false, GameObject source = null, bool ignoreImmune = false)
        {
            if (amount > 0f && esm != null)
            {
                float healingPct = esm.GetStat(StatType.healingPct);
                if (healingPct != 0f) amount *= Mathf.Max(0f, 1f + (healingPct * 0.01f));
            }

            int finalAmount = Mathf.RoundToInt(amount);
            if (finalAmount == 0) return false;

            if (finalAmount < 0 && Immune && !ignoreImmune) return false;
            if (finalAmount > 0 && (esm.GetStat(StatType.CanGainHp)) <= 0f) return false;

            if (finalAmount < 0 && CurHp > 0 && Mathf.Abs(finalAmount) >= CurHp * 3f)
            {
                if (source != null && source.TryGetComponent<PlayerUpgradeManager>(out var pum))
                    pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnOverkill);
            }

            int targetChange = finalAmount;
            float curOverhealth = Overhealth;

            if (targetChange < 0 && curOverhealth >= 1f)
            {
                int absorbed = Mathf.Min(Mathf.FloorToInt(curOverhealth), -targetChange);
                AddOverhealth(-absorbed);
                targetChange += absorbed;
            }
            else if (targetChange > 0 && overhealthConvPct > 0f && CurHp >= MaxHp)
            {
                int converted = Mathf.RoundToInt(targetChange * overhealthConvPct * 0.01f);
                AddOverhealth(converted);
                targetChange -= converted;
            }

            if (targetChange < 0 && DamageRedirect != null && DamageRedirect(this, -targetChange)) targetChange = 0;
            if (targetChange > 0) targetChange = Mathf.Min(targetChange, MaxHp - CurHp);
            else if (targetChange < 0) TryStoreBlood(Mathf.Min(-targetChange, CurHp));
            esm.AddStat(new StatBuff(StatType.currentHp, targetChange));

            UpdatePhase();

            TextIndicatorSpawner tis = TextIndicatorSpawner.Instance;
            Vector3 pos = transform.position;

            if (tis != null && showIndicator && (finalAmount > 0 || GameSettings.Current.showDamageNumbers))
            {
                Color indicatorColor = colorOverride != default ? colorOverride : (finalAmount < 0 ? Color.red : Color.green);

                tis.SpawnTextIndicator(
                    Mathf.Abs(finalAmount),
                    pos,
                    indicatorColor,
                    sizeMult + UnityEngine.Random.Range(0f, 0.15f),
                    UnityEngine.Random.Range(0.5f, 0.7f),
                    UnityEngine.Random.Range(0.8f, 1.2f),
                    UnityEngine.Random.Range(0f, 0.2f),
                    TextType.Standard
                );
            }

            if (finalAmount < 0 && animator != null && CurHp > 0)
            {
                animator.SetBool(IsHurtHash, true);
                if (!hurtPending)
                {
                    hurtPending = true;
                    hurtResetTime = Time.time + esm.GetStat(StatType.HurtTime);
                }
                if (!bypassIFrames && isPlayerEntity)
                {
                    if (_suppressHurtIFrames) _pendingHurtIFrames = true;
                    else TriggerIFrames(hurtIFrameDuration);
                }
            }

            if (CurHp <= 0 && IsAlive)
            {
                StartDeathSequence();
                return true;
            }
            else
            {
                RefreshHealthBar();
            }
            return false;
        }

        private void RegenHp()
        {
            if (Time.timeScale == 0f) return;

            regenTimer += Time.deltaTime;
            if (regenTimer < regenInterval) return;

            regenTimer -= regenInterval;

            if (esm == null || !IsAlive || esm.GetStat(StatType.CanGainHp) != 1) return;
            if (isPlayerEntity && PlayerResourcePool.RegenLocked) return;
            if (ownSem is StatusEffectManager sm && sm.Frozen) return;
            if (CurHp >= MaxHp && !(regenOverHealth && overhealthConvPct > 0f)) return;

            float hpPerSecond = esm.GetStat(StatType.EffHpReg) / fullRegenFrequency;
            accumulatedRegen += hpPerSecond * regenInterval;

            int intRegen = Mathf.FloorToInt(accumulatedRegen);
            if (intRegen == 0f) return;
            accumulatedRegen -= intRegen;
            ChangeHealth(intRegen, false);

            if (cpum != null) cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnHealthRegen);
        }
    }
}
