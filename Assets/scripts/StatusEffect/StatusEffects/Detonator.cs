using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_dotDetonator", menuName = "Status Effects/Debuff/DoTDetonator")]
    public class Detonator : StatusEffect
    {
        [Tooltip("Percentage of Tick Damage Dealt")] public float dmgMult = 1f;
        public DamageType dmgType;
        public Color indicatorColor = Color.red;

        public override void OnApply()
        {
            if (target == null || source == null) return;
            if (!target.TryGetComponent<IStatusEffectReceiver>(out var sem)) return;
            if (!target.TryGetComponent<IDamageable>(out var eh)) return;
            if (!source.TryGetComponent<IStatProvider>(out var ssm)) return;

            List<DoT> dots = new();
            sem.GetActiveEffectsOfType<DoT>(dots);
            if (dots.Count == 0) return;

            float total = 0f;

            foreach (var dot in dots)
            {
                if (dot == null || dot.tickInterval <= 0f) continue;

                float dotTickDmg = dot.dpt * 0.01f * ssm.GetStat(dot.scalingStat) * dot.currentStacks;
                int ticksRemaining = Mathf.Max(0, Mathf.CeilToInt((dot.duration - dot.currentTime) / dot.tickInterval));

                total += dmgMult * dotTickDmg * ticksRemaining;
            }

            sem.RemoveEffect<DoT>();

            if (total <= 0f) return;

            DamagePacket dp = DamageRoll.Build(total, dmgType, true, indicatorColor, source, true, 2f);
            eh.TakeDamage(dp);
        }
    }
}
