using System.Collections.Generic;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;
using CrystalFlux.Core;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/Reminiscence")]
public class Reminiscence : PlayerUpgrade
{
    public StatusEffect cooldownEffect = null;

    private bool isCasting;

    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (isCasting) return;

        if (player.TryGetComponent<PlayerAttackHandler>(out var pah))
        {
            List<AttackType> availableTypes = pah.attacks.ConvertAll(atk => atk.type);
            if (availableTypes.Count == 0) return;

            AttackType chosen = availableTypes[Random.Range(0, availableTypes.Count)];

            isCasting = true;
            try { pah.PerformAttack(chosen, true, true, true); }
            finally { isCasting = false; }
        }

        if (cooldownEffect != null && player.TryGetComponent<IStatusEffectReceiver>(out var sem))
            sem.Apply(cooldownEffect, player);
    }
}
