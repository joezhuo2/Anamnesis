using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/StellarSurge")]
public class StellarSurge : PlayerUpgrade
{
    public float hpPct;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (!player.TryGetComponent<EntityHealth>(out var eh)) return;

        eh.ChangeHealth(0, hpPct * 0.01f, true, 1f , Color.teal, false);
    }
}