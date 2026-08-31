using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public abstract class PlayerUpgrade : UpgradeAsset
    {
        public enum TriggerCondition
        {
            OnBasicAttack,
            OnSkillAttack,
            OnUltAttack,
            OnAttack,
            OnTakeHit,
            OnTakeDamage,
            OnStartDash,
            OnEndDash,
            OnHealthRegen,
            OnDeath,
            OnStaminaRegen,
            OnCalculateAttackCost,
            OnOverkill,
            OnCounterDodge,
            OnProjectileHit,
            OnCrit,
            OnTargetRecievedHit,
            OnDealDamage,
            OnManaRegen,
            OnKill,
            OnLevelUp,
            OnSpawnProjectile
        }
        public TriggerCondition[] conditions;
        public float chance;
        public float cooldown;
        [Tooltip("delay after triggering before effect activates")] public float delay;
        public virtual void TriggerUpgradeEffect(GameObject player) {}
        public virtual void TriggerUpgradeEffect(GameObject player, Vector2? spawnCenter) {}
        public virtual void TriggerUpgradeEffect(GameObject player, GameObject target, float damageDealt) {}
        public virtual void OnUnlock(GameObject player) {}
        public virtual void OnRemove(GameObject player) {}
        public override void GetTooltipLines(List<string> lines)
        {
            lines.Add($"Trigger: {string.Join(", ", conditions)}");
            if (chance < 100f) lines.Add($"Chance: {chance:F0}%");
            if (cooldown > 0f) lines.Add($"Cooldown: {cooldown:F1}s");
            if (delay > 0f) lines.Add($"Delay: {delay:F1}s");
        }
    }
}
