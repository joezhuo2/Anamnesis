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
            OnDealDamage
        }
        public TriggerCondition[] conditions;
        public float chance;
        public float cooldown;
        public float delay; [Tooltip("delay after triggering before effect activates")]
        public virtual void TriggerUpgradeEffect(GameObject player) {}
        public virtual void TriggerUpgradeEffect(GameObject player, Vector2? spawnCenter = null) {}
        public virtual void TriggerUpgradeEffect(GameObject player, GameObject target, float damageDealt) {}
        public virtual void OnUnlock(GameObject player) {}
    }
}
