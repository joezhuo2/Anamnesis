using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/FreeCast")]
public class FreeCast : PlayerUpgrade
{
    [Min(1)] public int castsRequired = 3;
    public StatusEffect effect;
    [Min(0)] public int stacks = 1;

    private bool hasLast;
    private AttackType lastType;
    private int streak;
    private bool pending;
    private float lastProcTime = float.NegativeInfinity;

    public bool IsPending(AttackType type) => pending && hasLast && lastType == type;

    public override void OnUnlock(GameObject player) => ResetState();

    public override void OnRemove(GameObject player)
    {
        ResetState();

        if (effect == null || player == null) return;
        if (player.TryGetComponent<StatusEffectManager>(out var sem)) sem.RemoveEffect(effect);
    }

    public void RegisterCast(GameObject player, AttackType type)
    {
        if (!hasLast || lastType != type)
        {
            hasLast = true;
            lastType = type;
            streak = 0;
            pending = false;
        }

        if (pending)
        {
            pending = false;
            streak = 0;
            return;
        }

        if (++streak < castsRequired) return;

        streak = 0;

        float now = Time.time;
        if (cooldown > 0f && now < lastProcTime + cooldown) return;
        if (Random.Range(0f, 100f) > chance) return;

        lastProcTime = now;
        pending = true;
        ApplyTo(player);
    }

    private void ApplyTo(GameObject player)
    {
        if (effect == null || player == null || stacks <= 0) return;
        if (!player.TryGetComponent<IStatusEffectReceiver>(out var sem)) return;

        for (int i = 0; i < stacks; i++)
            sem.Apply(effect, player, player.transform.position);
    }

    private void ResetState()
    {
        hasLast = false;
        streak = 0;
        pending = false;
        lastProcTime = float.NegativeInfinity;
    }

    public override void GetTooltipLines(List<string> lines)
    {
        lines.Add($"Cast the same attack {castsRequired}x in a row: next cast is free");
        if (chance < 100f) lines.Add($"Chance: {chance:F0}%");
        if (cooldown > 0f) lines.Add($"Cooldown: {cooldown:F1}s");
        if (effect == null || stacks <= 0) return;

        string label = string.IsNullOrEmpty(effect.effName) ? effect.name : effect.effName;
        lines.Add(stacks > 1 ? $"Grants {label} x{stacks}" : $"Grants {label}");
        if (effect.duration > 0f) lines.Add($"Duration: {effect.duration:F1}s");
        if (!string.IsNullOrEmpty(effect.desc)) lines.Add(effect.desc);
    }
}
