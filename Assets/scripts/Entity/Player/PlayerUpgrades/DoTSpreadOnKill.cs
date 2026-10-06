using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/DoTSpreadOnKill")]
public class DoTSpreadOnKill : PlayerUpgrade
{
    [Min(1)] public int maxTargets = 3;
    [Tooltip("spread radius in tiles (world units)")] public float radius = 3f;
    [Tooltip("% of the DoT's remaining duration given to spread copies")] public float durationPct = 50f;

    private readonly List<DoT> dots = new();
    private readonly List<(EnemyMovement em, float d2)> candidates = new();

    public override void TriggerUpgradeEffect(GameObject player, GameObject target)
    {
        if (player == null || target == null) return;
        if (!target.TryGetComponent<StatusEffectManager>(out var tsem)) return;

        tsem.GetActiveEffectsOfType(dots);
        if (dots.Count == 0) return;

        Vector2 center = target.transform.position;
        float r2 = radius * radius;
        var enemies = EnemyMovement.Active;

        for (int i = 0; i < dots.Count; i++)
        {
            DoT d = dots[i];
            if (d == null || d.source != player) continue;

            float dur = (d.duration - d.currentTime) * durationPct * 0.01f;
            if (dur <= 0f) continue;

            StatusEffect asset = d.origin != null ? d.origin : d;
            GatherNearest(asset, target, center, r2, enemies);

            for (int k = 0; k < candidates.Count; k++)
            {
                var osem = candidates[k].em.Sem;
                osem.Apply(asset, player, candidates[k].em.transform.position);

                int last = osem.activeEffects.Count - 1;
                if (last < 0) continue;
                StatusEffect e = osem.activeEffects[last];
                if (e == null || e.origin != asset) continue;

                e.duration = dur;
                e.currentTime = 0f;
                e.currentStacks = Mathf.Clamp(d.currentStacks, 1, Mathf.Max(1, e.maxStacks));
                e.OnStack();
            }
        }

        dots.Clear();
        candidates.Clear();
    }

    private void GatherNearest(StatusEffect asset, GameObject from, Vector2 center, float r2, IReadOnlyList<EnemyMovement> enemies)
    {
        candidates.Clear();
        for (int k = 0; k < enemies.Count; k++)
        {
            var em = enemies[k];
            if (em == null || em.gameObject == from) continue;

            float d2 = ((Vector2)em.transform.position - center).sqrMagnitude;
            if (d2 > r2) continue;
            var sp = em.Stats;
            if (sp != null && sp.GetStat(StatType.isAlive) <= 0f) continue;
            var osem = em.Sem;
            if (osem == null || osem.HasEffect(asset)) continue;

            candidates.Add((em, d2));
        }

        candidates.Sort((a, b) => a.d2.CompareTo(b.d2));
        if (candidates.Count > maxTargets) candidates.RemoveRange(maxTargets, candidates.Count - maxTargets);
    }

    public override void GetTooltipLines(List<string> lines)
    {
        base.GetTooltipLines(lines);
        lines.Add($"On kill, your DoTs on the enemy spread to the {maxTargets} nearest enemies within {radius:F1} tiles at {durationPct:F0}% of their remaining duration");
    }
}
