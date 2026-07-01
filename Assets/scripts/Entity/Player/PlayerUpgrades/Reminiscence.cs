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
            if (availableTypes.Count == 0) return;

            AttackType chosen = availableTypes[Random.Range(0, availableTypes.Count)];
            pah.PerformAttack(chosen, true, true, true);
        }
    }
}