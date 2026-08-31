using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/StellarSurge")]
public class StellarSurge : PlayerUpgrade
{
    public float hpPct;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player == null) return;
        if (!player.TryGetComponent<IDamageable>(out var id)) return;
        if (!player.TryGetComponent<IStatProvider>(out var esm)) return;

        var dp = DamagePacketBuilder.BuildDamagePacket(
            hpPct * 0.01f * esm.GetStat(StatType.EffMaxHp),
            DamageType.Heal, false, Color.teal, player, true, 1f
        );
        id.TakeDamage(dp);
    }
}
