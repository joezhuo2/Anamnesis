using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/Reminiscence")]
public class Reminiscence : PlayerUpgrade
{
    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player.TryGetComponent(out PlayerAttackHandler pah))
        {
            List<AttackType> availableTypes = pah.attacks.ConvertAll(atk => atk.type);
            pah.PerformAttack(availableTypes[Random.Range(0, availableTypes.Count)], true, true);
        }
    }
}