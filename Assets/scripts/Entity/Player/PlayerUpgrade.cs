using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "Player/Upgrade")]
public abstract class PlayerUpgrade : ScriptableObject
{
    public enum TriggerCondition
    {
        OnBasicAttack,
        OnSkillAttack,
        OnAttack,
        OnTakeHit,
        OnTakeDamage,
        OnStartDash,
        OnEndDash,
        OnHealthRegen,
        OnDeath,
        OnStaminaGain
    }
    public TriggerCondition[] conditions;
    public float chance;
    public float delay; // delay after triggering before effect activates
    public abstract void TriggerUpgradeEffect(GameObject player);
}