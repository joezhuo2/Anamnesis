using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/GainMana")]
public class GainMana : PlayerUpgrade
{
    public int amount;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player.TryGetComponent<IResourcePool>(out var pm))
            pm.TryGain(ResourceType.Mana, amount);
    }
}
