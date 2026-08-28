using UnityEngine;
using CrystalFlux.EntitySystem;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/AdditionalDamage")]
public class AdditionalDamage : PlayerUpgrade
{
    public float pctAmt;
    public DamageType type;

    public override void TriggerUpgradeEffect(GameObject player, GameObject target, float damageDealt)
    {
        if (player == null || target == null || damageDealt <= 0f) return;

        if (!target.TryGetComponent<IDamageable>(out var id)) return;

        if (!player.TryGetComponent<IStatProvider>(out var _)) return;

        float bonusDamage = damageDealt * (pctAmt / 100f);
        if (bonusDamage <= 0f) return;

        Color indicatorColor = type switch
        {
            DamageType.Physical => Color.gray,
            DamageType.Spell => Color.purple,
            DamageType.True => Color.lightBlue,
            _ => Color.white
        };

        DamagePacket dp = DamagePacket.BuildDamagePacket(bonusDamage, type, false, indicatorColor, player, true, 1.25f);
        if (dp.GetTotalDamage() > 0f) id.TakeDamage(dp);
    }
}