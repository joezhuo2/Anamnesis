using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/Reminiscence")]
public class Reminiscence : PlayerUpgrade
{
    public StatusEffect cooldownEffect = null;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player.TryGetComponent<PlayerAttackHandler>(out var pah))
        {
            List<AttackType> availableTypes = pah.attacks.ConvertAll(atk => atk.type);
            if (availableTypes.Count == 0) return;

            AttackType chosen = availableTypes[Random.Range(0, availableTypes.Count)];
            pah.PerformAttack(chosen, true, true, true);
        }

        if (cooldownEffect != null && player.TryGetComponent<StatusEffectManager>(out var sem))
            sem.AddEffect(cooldownEffect, player);
    }
}