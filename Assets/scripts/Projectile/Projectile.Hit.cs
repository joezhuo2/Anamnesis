using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalFlux.ProjectileSystem
{
    public partial class Projectile
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (pierced >= pd.NumPierce && pd.DestroyOnMaxPierce)
            {
                Despawn();
                return;
            }

            if (pierced >= pd.NumPierce) return;
            if (hit.Contains(other.gameObject)) return;

            if (other.TryGetComponent<IStatProvider>(out var statManager) && ownerObj != other.gameObject)
                HandleHitEntity(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (pierced >= pd.NumPierce) return;
            if (hit.Contains(other.gameObject)) return;

            if (other.TryGetComponent<IStatProvider>(out var statManager) && ownerObj != other.gameObject)
                HandleHitEntity(other.gameObject);
        }

        private void HandleHitEntity(GameObject target)
        {
            if (target == null || ownerObj == null || target == ownerObj) return;
            if (!target.TryGetComponent<IDamageable>(out var eh) || !eh.IsAlive) return;

            var tid = target.TryGetComponent<ITeamMember>(out var itm) ? itm.TeamID : 0;

            if (ownerTeam == tid) return;

            DamagePacket dp = DamagePacketBuilder.BuildDamagePacket(pd, damageSnapshot, true, ownerObj, false, 1f);

            bool prevApplyingHit = ApplyingProjectileHit;
            ApplyingProjectileHit = true;
            try { eh.TakeDamage(dp); }
            finally { ApplyingProjectileHit = prevApplyingHit; }

            if (PlayerHit != null && ownerObj != null && ownerObj.CompareTag("Player")) PlayerHit(ChainOrigin);

            TriggerImpact();

            if (pd.KbForce > 0f && (eh as Component).TryGetComponent<Rigidbody2D>(out var rb2d))
            {
                Vector2 kbDir = (rb2d.transform.position - transform.position).normalized;

                float kbf = ownerStats != null ?
                    pd.KbForce * (1f + (ownerStats.GetStat(StatType.kbPct) * 0.01f)) : pd.KbForce;

                if (target.TryGetComponent<IKnockbackable>(out var kb))
                    kb.ApplyKnockback(kbDir, kbf, pd.KnockbackTime);
                else if (rb2d.bodyType == RigidbodyType2D.Dynamic)
                    rb2d.AddForce(kbDir * kbf, ForceMode2D.Impulse);
            }

            var (hp, stamina, mana) = CalculateStatGains(ownerStats, pd.MainAttack, dp.GetTotalDamage());
            DamagePacket.Release(dp);
            TriggerStatGains(hp, stamina, mana, ownerObj);

            pierced++;
            hit.Add(target);

            if (onHit != null)
                for (int i = 0; i < onHit.Count; i++)
                    onHit[i].OnHit(proxyOwner, target, transform.position);
            if (pd.MainAttack != null && pd.MainAttack.SummonCondition == SummonCondition.OnHit && Random.value <= pd.MainAttack.SummonChance)
            {
                if (ownerSummon != null)
                    ownerSummon.TrySummon(target.transform.position);
            }

            if (pd.TimeBeforeSameEnemy > 0f)
            {
                hitExpiryTargets.Add(target);
                hitExpiryTimes.Add(Time.time + pd.TimeBeforeSameEnemy);
            }

            if (pd.AdditionalChance > 0f && pd.AdditionalAttack != null && Random.value <= pd.AdditionalChance)
                HandleAdditionalSpawns(target);
            else if (canTriggerAdd) TryRetriggerChain();

            canTriggerAdd = false;

            var hitExtras = ExtraEffects();

            if (pd.Effects != null)
                for (int i = 0; i < pd.Effects.Count; i++) ApplyOnHit(pd.Effects[i], target);

            if (hitExtras != null)
                for (int i = 0; i < hitExtras.Count; i++) ApplyOnHit(hitExtras[i], target);

            if (pd.DestroyOnMaxPierce && pierced >= pd.NumPierce) Despawn();
        }

        private void TriggerImpact()
        {
            var ad = pd.MainAttack;
            if (ad == null || (oc != null && oc.Hazard)) return;

            HitFeedback.Stop(ad.HitStop, ad.HitStopCooldown);
            HitFeedback.Shake(ad.ScreenShake);
        }

        private IReadOnlyList<EffectData> ExtraEffects()
            => effectSource != null && pd != null && pd.MainAttack != null
                ? effectSource.GetExtraEffects(pd.MainAttack.type)
                : null;

        private void ApplyOnCast(EffectData ed)
        {
            if (ed.effect != null && ed.applyCondition == ApplyCondition.OnCast && ed.selfApply)
                ApplyEffect(null, ed);
        }

        private void ApplyOnHit(EffectData ed, GameObject target)
        {
            if (ed.effect == null || ed.applyCondition != ApplyCondition.OnHit) return;

            if (ed.selfApply) ApplyEffect(null, ed);
            else if (ownerObj != target) ApplyEffect(target, ed);
        }

        private void HandleAdditionalSpawns(GameObject hitTarget = null)
        {
            if (!canTriggerAdd) return;
            if (ProjectileSpawner.Instance == null) return;

            if (pd.AdditionalAttack == null || pd.AdditionalAttack.ProjectilePrefab == null)
            {
                TryRetriggerChain();
                return;
            }

            Vector2? addDir = pd.AdditionalFollowsMouse ? null : dir;

            ProjectileSpawner.Instance.Spawn(pd.AdditionalAttack, ownerObj, transform.position, addDir, AddSpawnDist, ChainOrigin, ignoreTarget: hitTarget);
        }

        private void TryRetriggerChain()
        {
            if (chainRoot == null || chainRoot.ProjectilePrefab == null) return;
            if (ChainRetriggerChance <= 0f || ownerObj == null) return;
            if (ownerTeam != 1) return;
            if (ProjectileSpawner.Instance == null) return;
            if (Random.Range(0f, 100f) > ChainRetriggerChance) return;

            ProjectileSpawner.Instance.Spawn(chainRoot, ownerObj, null, null, null, chainRoot);
        }

        private void ApplyEffect(GameObject target, EffectData ed)
        {
            if (ed.effect == null) return;

            if (target == null) target = ownerObj;

            if (target.TryGetComponent<IStatusEffectReceiver>(out var sem))
            {
                if (ed.chance <= 0f) return;

                if (Random.value <= ed.chance)
                    sem.Apply(ed.effect, ownerObj);
            }
        }

        private void TickHitHistory()
        {
            if (hitExpiryTimes.Count == 0) return;

            float now = Time.time;
            for (int i = hitExpiryTimes.Count - 1; i >= 0; i--)
            {
                if (now < hitExpiryTimes[i]) continue;

                GameObject t = hitExpiryTargets[i];
                int last = hitExpiryTimes.Count - 1;
                hitExpiryTargets[i] = hitExpiryTargets[last];
                hitExpiryTimes[i] = hitExpiryTimes[last];
                hitExpiryTargets.RemoveAt(last);
                hitExpiryTimes.RemoveAt(last);

                if (t != null) hit.Remove(t);
                canTriggerAdd = true;
            }
        }

        public static (float hp, float stamina, float mana) CalculateStatGains(GameObject target, AttackData a, float totalDmg = 0f)
        {
            if (target == null || !target.TryGetComponent<IStatProvider>(out var esm)) return (0f, 0f, 0f);
            return CalculateStatGains(esm, a, totalDmg);
        }

        public static (float hp, float stamina, float mana) CalculateStatGains(IStatProvider esm, AttackData a, float totalDmg = 0f)
        {
            if (esm == null || a == null) return (0f, 0f, 0f);

            float totalStamina = a.StaminaGainOnHit;
            float totalHp = a.HealthGainOnHit;
            float totalMana = a.ManaGainOnHit;

            if (a.BasedOnDmgDealt)
            {
                totalStamina += totalDmg * 0.01f * a.StaminaPctGainOnHit;
                totalHp += totalDmg * 0.01f * a.HealthPctGainOnHit;
                totalMana += totalDmg * 0.01f * a.ManaPctGainOnHit;
            }
            else if (totalDmg > 0f)
            {
                totalStamina += a.StaminaPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxStamina);
                totalHp += a.HealthPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxHp);
                totalMana += a.ManaPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxMana);
            }
            return (totalHp, totalStamina, totalMana);
        }

        private void TriggerStatGains(float hp, float stamina, float mana, GameObject target)
        {
            if (target == null) return;
            if (ownerPool != null)
            {
                ownerPool.TryGain(ResourceType.Stamina, stamina);
                ownerPool.TryGain(ResourceType.Mana, mana);
            }

            if (ownerDmg != null)
            {
                var dp = DamagePacketBuilder.BuildDamagePacket(hp, DamageType.Heal, false, Color.green, target, true, 1f);
                ownerDmg.TakeDamage(dp);
                DamagePacket.Release(dp);
            }
        }

        private void CaptureSnapshot() => damageSnapshot = ProjectileSnapshot.CaptureSnapshot(pd, ownerObj);
    }
}
