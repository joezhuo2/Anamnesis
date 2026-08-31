using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/GrantStatusEffect")]
public class GrantStatusEffect : PlayerUpgrade
{
    public StatusEffect effect;
    [Min(1)] public int stacks = 1;

    public override void TriggerUpgradeEffect(GameObject player) => ApplyTo(player);

    public override void TriggerUpgradeEffect(GameObject player, Vector2? spawnCenter) => ApplyTo(player);

    public override void TriggerUpgradeEffect(GameObject player, GameObject target, float damageDealt) => ApplyTo(player);

    public override void OnRemove(GameObject player)
    {
        if (effect == null || player == null) return;
        if (!player.TryGetComponent<IStatusEffectReceiver>(out var sem)) return;

        var remove = typeof(IStatusEffectReceiver).GetMethod(nameof(IStatusEffectReceiver.RemoveEffect));
        if (remove != null) remove.MakeGenericMethod(effect.GetType()).Invoke(sem, null);
    }

    private void ApplyTo(GameObject player)
    {
        if (effect == null || player == null) return;
        if (!player.TryGetComponent<IStatusEffectReceiver>(out var sem)) return;

        for (int i = 0; i < stacks; i++)
            sem.Apply(effect, player, player.transform.position);
    }

    public override void GetTooltipLines(List<string> lines)
    {
        base.GetTooltipLines(lines);
        if (effect == null) return;

        string label = string.IsNullOrEmpty(effect.effName) ? effect.name : effect.effName;
        lines.Add(stacks > 1 ? $"Grants {label} x{stacks}" : $"Grants {label}");
        if (effect.duration > 0f) lines.Add($"Duration: {effect.duration:F1}s");
        if (!string.IsNullOrEmpty(effect.desc)) lines.Add(effect.desc);
    }
}
