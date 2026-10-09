using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/BloodBank")]
public class BloodBank : PlayerUpgrade
{
    [Tooltip("Permanent status effect that holds the stored health")] public BloodPool bloodPool;
    [Tooltip("% of all health lost that is stored")] public float storePct = 50f;

    public override void OnUnlock(GameObject player)
    {
        if (bloodPool == null || player == null) return;
        if (!player.TryGetComponent<StatusEffectManager>(out var sem)) return;

        sem.ApplyPermanent(bloodPool, player);
        if (sem.GetActiveFirstEffectOfType<BloodPool>() is BloodPool bp) bp.storePct = storePct;
    }

    public override void OnRemove(GameObject player)
    {
        if (bloodPool == null || player == null) return;
        if (player.TryGetComponent<StatusEffectManager>(out var sem)) sem.RemoveEffect(bloodPool);
    }

    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player == null) return;
        if (!player.TryGetComponent<IStatusEffectReceiver>(out var sem)) return;
        if (sem.GetActiveFirstEffectOfType<BloodPool>() is not BloodPool bp || bp.Stored < 1f) return;
        if (!player.TryGetComponent<EntityHealth>(out var eh)) return;

        eh.GrantOverhealth(bp.Withdraw());
    }

    public override void GetTooltipLines(List<string> lines)
    {
        base.GetTooltipLines(lines);
        lines.Add($"Stores {storePct:F0}% of all health lost in a blood pool");
        lines.Add("Triggering releases all stored health as overhealth");
    }
}
