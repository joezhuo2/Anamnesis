using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "se_dotDetonator", menuName = "Status Effects/Debuff/DoTDetonator")]
public class Detonator : StatusEffect
{
    [Tooltip("Percentage of Tick Damage Dealt")] public float dmgMult = 1f;
    public DamageType dmgType;
    public Color indicatorColor = Color.red;

    public override void OnApply()
    {
        if (target.TryGetComponent<StatusEffectManager>(out var sem))
        {
            List<DoT> dots = new();
            sem.GetActiveEffectsOfType<DoT>(dots);

            foreach (var dot in dots)
            {
                if (target.TryGetComponent<IDamageable>(out var eh) && source.TryGetComponent<IStatProvider>(out var ssm))
                {
                    float dotTickDmg = dot.dpt * 0.01f * ssm.GetStat(dot.scalingStat) * dot.currentStacks;
                    int ticksRemaining = Mathf.CeilToInt((dot.duration - dot.currentTime) / dot.tickInterval);
                    float dmg = dmgMult * dotTickDmg * ticksRemaining;

                    DamagePacket dp = DamagePacket.BuildDamagePacket(dmg, dmgType, true, indicatorColor, source, true, 2f);
                    eh.TakeDamage(dp);
                }
                sem.RemoveEffect(dot);
            }
        }
    }
}