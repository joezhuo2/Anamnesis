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
        OnCounterDodge
    }
    public TriggerCondition[] conditions;
    public float chance;
    public float delay; // delay after triggering before effect activates
    public abstract void TriggerUpgradeEffect(GameObject player);
}