using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_statbuff", menuName = "Status Effects/Buff/Stat Buffs")]
    public class StatBuffs : StatusEffect
    {
        public List<StatBuff> buffs = new();

        private readonly List<StatBuff> curActiveBuff = new();

        protected override void ResetRuntime() => curActiveBuff.Clear();

        public override void OnApply() => ApplyBuffs();
        public override void OnStack()
        {
            UndoCurrentBuffs();
            ApplyBuffs();
        }
        public override void OnExpire() => UndoCurrentBuffs();
        private void ApplyBuffs()
        {
            if (target == null || !target.TryGetComponent<IStatProvider>(out var esm)) return;

            foreach (var buff in buffs)
            {
                float mult = IsFlag(buff.type) ? 1f : potencyMultiplier;
                var b = new StatBuff(buff.type, buff.value * currentStacks * mult);
                esm.AddStat(b, true);
                curActiveBuff.Add(b);
            }
        }
        private static bool IsFlag(StatType t) => t switch
        {
            StatType.isImmune or StatType.isAlive or StatType.IsDashing or StatType.IsAttacking
                or StatType.CanMove or StatType.CanDash or StatType.CanAttack or StatType.CanGainHp
                or StatType.CanGainMana or StatType.CanGainStamina or StatType.DashShouldApplyIFrame
                or StatType.globalDoTCanCrit or StatType.Level => true,
            _ => false
        };

        private void UndoCurrentBuffs()
        {
            if (target != null && target.TryGetComponent<IStatProvider>(out var esm))
            {
                foreach (var b in curActiveBuff)
                    esm.AddStat(b, false);
            }

            curActiveBuff.Clear();
        }
    }
}
