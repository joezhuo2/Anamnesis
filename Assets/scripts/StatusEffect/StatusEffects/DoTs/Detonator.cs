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
                if (target.TryGetComponent<EntityHealth>(out var eh) && source.TryGetComponent<EntityStatManager>(out var ssm))
                {
                    float dotTickDmg = dot.dpt * 0.01f * ssm.GetStat(dot.scalingStat) * dot.currentStacks;
                    float dmg = dmgMult * dotTickDmg;

                    DamagePacket damagePacket = DamageCalculator.BuildDamagePacket(dmg, dmgType, ssm.s, true, indicatorColor);
                    eh.TakeDamage(damagePacket, true, source, ssm.s.resPen, ssm.s.defShred, 3f);
                }
                sem.RemoveEffect(dot);
            }
        }
    }
}