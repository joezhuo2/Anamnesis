using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_statbuff", menuName = "Status Effects/Buff/Stat Buffs")]
    public class StatBuffs : StatusEffect
    {
        public List<StatBuff> buffs = new();
        public Dictionary<StatBuff, StatBuff> curActiveBuff = new();

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
                var b = new StatBuff(buff.type, buff.value * currentStacks);
                esm.AddStat(b, true);
                curActiveBuff[buff] = b;
            }
        }
        private void UndoCurrentBuffs()
        {
            if (target != null && target.TryGetComponent<IStatProvider>(out var esm))
            {
                foreach (var kvp in curActiveBuff)
                    esm.AddStat(kvp.Value, false);
                curActiveBuff.Clear();
            }
        }
    }
}
