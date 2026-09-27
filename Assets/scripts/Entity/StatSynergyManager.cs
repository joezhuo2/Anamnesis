using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.Core
{
    [System.Serializable]
    public class StatSynergy
    {
        public StatType source = StatType.EffMaxHp;
        public StatType target = StatType.attack;
        public float pct;

        public StatSynergy() { }

        public StatSynergy(StatType source, StatType target, float pct)
        {
            this.source = source;
            this.target = target;
            this.pct = pct;
        }

        public string GetDescription() => $"Increases {StatName(target)} by {pct:0.#}% of {StatName(source)}";

        public static string StatName(StatType t) => t switch
        {
            StatType.EffMaxHp => "Max Health",
            StatType.EffAtk => "Attack",
            StatType.EffArmor => "Armor",
            StatType.EffDefense => "Defense",
            StatType.EffArcaneShield => "Arcane Shield",
            StatType.EffInt => "Intelligence",
            StatType.EffHpReg => "HP Regen",
            StatType.EffStReg => "Stamina Regen",
            StatType.EffSpd => "Move Speed",
            StatType.EffMaxMana => "Max Mana",
            StatType.EffMaxStamina => "Max Stamina",
            _ => new StatBuff(t, 0f).ToString()
        };

        public static StatType Effective(StatType t) => t switch
        {
            StatType.attack or StatType.atkPct => StatType.EffAtk,
            StatType.maxHp or StatType.hpPct => StatType.EffMaxHp,
            StatType.armor or StatType.armorPct => StatType.EffArmor,
            StatType.defense or StatType.defensePct => StatType.EffDefense,
            StatType.arcaneShield or StatType.arcaneShieldPct => StatType.EffArcaneShield,
            StatType.Intelligence or StatType.IntPct => StatType.EffInt,
            StatType.hpRegen or StatType.hpRegPct => StatType.EffHpReg,
            StatType.staminaRegen or StatType.stRegPct => StatType.EffStReg,
            StatType.moveSpeed or StatType.moveSpeedPct => StatType.EffSpd,
            StatType.maxMana or StatType.maxManaPct => StatType.EffMaxMana,
            StatType.maxStamina or StatType.maxStaminaPct => StatType.EffMaxStamina,
            _ => t
        };

        public static bool IsWholeStat(StatType t) => t switch
        {
            StatType.attack or StatType.EffAtk or StatType.maxHp or StatType.EffMaxHp or StatType.armor or StatType.EffArmor
                or StatType.defense or StatType.EffDefense or StatType.arcaneShield or StatType.EffArcaneShield
                or StatType.Intelligence or StatType.EffInt or StatType.maxStamina or StatType.EffMaxStamina
                or StatType.maxMana or StatType.EffMaxMana or StatType.defShred => true,
            _ => false
        };
    }

    public class StatSynergyManager : MonoBehaviour
    {
        private class Entry
        {
            public StatSynergy sy;
            public float applied;
        }

        private readonly List<Entry> entries = new();
        private IStatProvider isp;

        public int Count => entries.Count;

        private void Awake() => TryGetComponent(out isp);

        public void AddSynergy(StatSynergy sy)
        {
            if (sy == null || StatSynergy.Effective(sy.source) == StatSynergy.Effective(sy.target)) return;
            if (isp == null && !TryGetComponent(out isp)) return;

            entries.Add(new Entry { sy = new StatSynergy(sy.source, sy.target, sy.pct) });
            Refresh();
        }

        public bool HasSynergy(StatType source, StatType target)
        {
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].sy.source == source && entries[i].sy.target == target) return true;
            return false;
        }

        public void ClearSynergies()
        {
            if (isp != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    Entry e = entries[i];
                    if (e.applied != 0f) isp.AddStat(new StatBuff(e.sy.target, e.applied), false);
                }
            }
            entries.Clear();
        }

        public void GetDescriptions(List<string> lines)
        {
            if (lines == null) return;
            for (int i = 0; i < entries.Count; i++)
                lines.Add($"{entries[i].sy.GetDescription()} (+{entries[i].applied:0.##})");
        }

        private void LateUpdate() => Refresh();

        private void Refresh()
        {
            if (isp == null || entries.Count == 0) return;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry e = entries[i];
                float want = isp.GetStat(e.sy.source) * e.sy.pct * 0.01f;
                if (StatSynergy.IsWholeStat(e.sy.target)) want = Mathf.Floor(want);

                float delta = want - e.applied;
                if (Mathf.Abs(delta) < 0.001f) continue;

                isp.AddStat(new StatBuff(e.sy.target, delta));
                e.applied = want;
            }
        }
    }
}
