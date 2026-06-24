using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade")]
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
        OnCalculateAttackCost
    }
    public TriggerCondition[] conditions;
    public float chance;
    public float delay; // delay after triggering before effect activates
    public abstract void TriggerUpgradeEffect(GameObject player);
}