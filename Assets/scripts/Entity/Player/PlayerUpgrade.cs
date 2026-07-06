using UnityEngine;

public abstract class PlayerUpgrade : ScriptableObject
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
        OnProjectileHit
    }
    public string upgradeName;
    public TriggerCondition[] conditions;
    public float chance;
    public float cooldown;
    public float delay; [Tooltip("delay after triggering before effect activates")]
    public abstract void TriggerUpgradeEffect(GameObject player);
    public virtual void TriggerUpgradeEffect(GameObject player, Vector2? spawnCenter = null) {}
    public virtual void OnUnlock(GameObject player) {}
}