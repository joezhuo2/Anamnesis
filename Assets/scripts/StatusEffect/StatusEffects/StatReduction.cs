using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_stat_reduction", menuName = "Status Effects/Debuff/Stat Reduction")]
    public class StatReduction : StatusEffect
    {
        [Tooltip("% reduction per stack")] public float redPerStack = 10f;
        private StatBuff? currentActiveDebuff = null;
        public StatType statType = StatType.EffAtk;
        public StatType scalingStat = StatType.EffAtk;
        public float maxRed = 0.9f;
        public float minRed = 0f;

        public override void OnApply() => ApplyReduction();
        public override void OnStack()
        {
            UndoCurrentDebuff();
            ApplyReduction();
        }
        public override void OnExpire() => UndoCurrentDebuff();
        private static StatType ToBaseStat(StatType t) => t switch
        {
            StatType.EffAtk => StatType.attack,
            StatType.EffMaxHp => StatType.maxHp,
            StatType.EffArmor => StatType.armor,
            StatType.EffSpd => StatType.moveSpeed,
            StatType.EffInt => StatType.Intelligence,
            StatType.EffHpReg => StatType.hpRegen,
            StatType.EffStReg => StatType.staminaRegen,
            StatType.EffMaxMana => StatType.maxMana,
            StatType.EffMaxStamina => StatType.maxStamina,
            _ => t
        };

        private void ApplyReduction()
        {
            if (target == null || !target.TryGetComponent<IStatProvider>(out var esm)) return;

            float redPct = Mathf.Clamp(redPerStack * 0.01f * currentStacks, minRed, maxRed);

            StatType writeStat = ToBaseStat(statType);
            float basis = esm.GetStat(ToBaseStat(scalingStat));

            StatBuff newDebuff = new(writeStat, redPct * basis);
            currentActiveDebuff = newDebuff;

            esm.AddStat(newDebuff, false);
        }

        private void UndoCurrentDebuff()
        {
            if (target != null && target.TryGetComponent<IStatProvider>(out var esm))
            {
                if (currentActiveDebuff.HasValue)
                {
                    esm.AddStat(currentActiveDebuff.Value, true);
                    currentActiveDebuff = null;
                }
            }
        }
    }
}
