using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/AdditionalDamage")]
public class AdditionalDamage : PlayerUpgrade
{
    public float pctAmt;
    public DamageType type;

    public override void TriggerUpgradeEffect(GameObject player) { }

    public override void TriggerUpgradeEffect(GameObject player, GameObject target, float damageDealt)
    {
        if (player == null || target == null || damageDealt <= 0f) return;

        if (!target.TryGetComponent<EntityHealth>(out var targetHealth)) return;

        if (!player.TryGetComponent<EntityStatManager>(out var esm) || esm.s == null) return;

        float bonusDamage = damageDealt * (pctAmt / 100f);
        if (bonusDamage <= 0f) return;

        Color indicatorColor = type switch
        {
            DamageType.Physical => Color.gray,
            DamageType.Spell => Color.purple,
            DamageType.True => Color.lightBlue,
            _ => Color.white
        };

        DamagePacket dp = DamageCalculator.BuildDamagePacket(bonusDamage, type, esm.s, false, indicatorColor, player);
        if (dp.GetTotalDamage() > 0f)
            targetHealth.TakeDamage(dp, true, player);
    }
}