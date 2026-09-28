using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_stun", menuName = "Status Effects/Debuff/Stun")]
    public class Stun : StatusEffect
    {
        public override void OnApply()
        {
            if (target != null && target.TryGetComponent<IStatProvider>(out var esm))
            {
                esm.AddStat(new(StatType.CanAttack, -1f));
                esm.AddStat(new(StatType.CanMove, -1f));
                esm.AddStat(new(StatType.CanDash, -1f));
            }
        }
        public override void OnExpire()
        {
            if (target != null && target.TryGetComponent<IStatProvider>(out var esm))
            {
                esm.AddStat(new(StatType.CanAttack, 1f));
                esm.AddStat(new(StatType.CanMove, 1f));
                esm.AddStat(new(StatType.CanDash, 1f));
            }
        }
    }
}
