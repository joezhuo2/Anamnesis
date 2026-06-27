using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/GainMana")]
public class GainMana : PlayerUpgrade
{
    public int amount;
    public float pctAmt;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player.TryGetComponent<PlayerMana>(out var pm))
            pm.ChangeMana(amount, pctAmt);
    }
}