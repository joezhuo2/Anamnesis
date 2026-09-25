using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_freeze", menuName = "Status Effects/Debuff/Freeze")]
    public class Freeze : StatusEffect
    {
        private bool held;

        protected override void ResetRuntime() => held = false;

        public override void OnApply()
        {
            if (target == null || held) return;
            held = true;

            if (target.TryGetComponent<IStatProvider>(out var esm))
            {
                esm.AddStat(new(StatType.CanAttack, -1f));
                esm.AddStat(new(StatType.CanMove, -1f));
                esm.AddStat(new(StatType.CanDash, -1f));
            }

            if (target.TryGetComponent<StatusEffectManager>(out var sem)) sem.SetFrozen(true);
        }

        public override void OnExpire()
        {
            if (target == null || !held) return;
            held = false;

            if (target.TryGetComponent<IStatProvider>(out var esm))
            {
                esm.AddStat(new(StatType.CanAttack, 1f));
                esm.AddStat(new(StatType.CanMove, 1f));
                esm.AddStat(new(StatType.CanDash, 1f));
            }

            if (target.TryGetComponent<StatusEffectManager>(out var sem)) sem.SetFrozen(false);
        }
    }
}
