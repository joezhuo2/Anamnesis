using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/StellarSurge")]
public class StellarSurge : PlayerUpgrade
{
    public float hpPct;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (!player.TryGetComponent<IDamageable>(out var id)) return;

        var dp = DamagePacket.BuildDamagePacket(
            hpPct * 0.01f * player.GetComponent<IStatProvider>().GetStat(StatType.EffMaxHp),
            DamageType.Heal, false, Color.teal, player, true, 1f
        );
        id.TakeDamage(dp);
    }
}