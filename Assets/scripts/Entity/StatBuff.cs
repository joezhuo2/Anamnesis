using System;
using UnityEngine;

namespace CrystalFlux.Core
{
    [System.Serializable]
    public struct StatBuff : IEquatable<StatBuff>
    {
        public StatType type;
        public float value;

        public StatBuff(StatType type, float value) : this()
        {
            this.type = type;
            this.value = value;
        }

        // public void Apply(GameObject target)
        // {
        //     if (target.TryGetComponent<IStatProvider>(out var isp))
        //         isp.AddStat(new(type, value));
        // }

        public readonly bool Equals(StatBuff other)
            => type == other.type && Mathf.Approximately(value, other.value);

        public override readonly bool Equals(object obj)
            => obj is StatBuff other && Equals(other);

        public override readonly int GetHashCode()
            => HashCode.Combine(type, value);

        public void Remove(GameObject target)
        {
            if (target.TryGetComponent<IStatProvider>(out var isp))
                isp.AddStat(new(type, -value));
        }

        public override readonly string ToString()
        {
            string name = type switch
            {
                StatType.attack =>              "Attack",
                StatType.atkPct =>              "Increased Attack %",
                StatType.damagePct =>           "Increased Damage %",
                StatType.physicalDmgPct =>      "Increased Physical Damage %",
                StatType.spellDmgPct =>         "Increased Spell Damage %",
                StatType.critChance =>          "Crit Chance",
                StatType.critDamage =>          "Crit Damage",
                StatType.aoePct =>              "Increased AoE %",
                StatType.maxHp =>               "Max Health",
                StatType.hpPct =>               "Increased Health %",
                StatType.hpRegen =>             "HP Regen",
                StatType.hpRegPct =>            "Increased HP Regen %",
                StatType.armor =>               "Armor",
                StatType.armorPct =>            "Increased Armor %",
                StatType.damageRes =>           "Damage Resistance",
                StatType.physicalRes =>         "Physical Resistance",
                StatType.spellRes =>            "Spell Resistance",
                StatType.dodgeChance =>         "Dodge Chance",
                StatType.dodgeResPct =>         "Dodge Resistance",
                StatType.moveSpeedPct =>        "Increased Move Speed %",
                StatType.attackSpeedPct =>      "Increased Attack Speed %",
                StatType.defShred =>            "Defense Shred",
                StatType.resPen =>              "Resistance Penetration",
                StatType.maxStamina =>          "Max Stamina",
                StatType.staminaRegen =>        "Stamina Regen",
                StatType.stRegPct =>            "Increased Stamina Regen %",
                StatType.addPhysDmgPct =>       "Added Physical Damage %",
                StatType.addSplDmgPct =>        "Added Spell Damage %",
                StatType.maxMana =>             "Max Mana",
                StatType.SkillDmgPct =>         "Increased Skill Damage %",
                StatType.BasicDmgPct =>         "Increased Basic Damage %",
                StatType.UltDmgPct =>           "Increased Ultimate Damage %",
                StatType.EffectRes =>           "Effect Resistance",
                StatType.Intelligence =>        "Intelligence",
                StatType.IntPct =>              "Increased Intelligence %",
                StatType.ProjSpd =>             "Increased Projectile Speed %",
                StatType.stCostPct =>           "Reduced Stamina Cost %",
                StatType.dashCooldownRedPct =>  "Reduced Dash Cooldown %",
                StatType.dashDistancePct =>     "Increased Dash Distance %",
                StatType.dashStaminaCostRedPct => "Reduced Dash Stamina Cost %",
                StatType.addDmgPct =>           "Additional Damage %",
                StatType.kbRes =>               "Knockback Resistance",
                StatType.kbPct =>               "Increased Knockback %",
                StatType.ExpBonus =>            "Increased Experience %",
                StatType.sePotPct =>            "Increased Status Effect Potency %",
                StatType.seDurPct =>            "Increased Status Effect Duration %",
                StatType.manaGainPct =>         "Increased Mana Gain %",
                StatType.seTickRatePct =>       "Increased Status Effect Tick Rate %",
                StatType.maxManaPct =>          "Increased Max Mana %",
                StatType.maxStaminaPct =>       "Increased Max Stamina %",
                StatType.basicCdRedPct =>       "Reduced Basic Cooldown %",
                StatType.skillCdRedPct =>       "Reduced Skill Cooldown %",
                StatType.ultCdRedPct =>         "Reduced Ultimate Cooldown %",
                StatType.EffMaxMana =>          "Effective Max Mana",
                StatType.EffMaxStamina =>       "Effective Max Stamina",
                _ => type.ToString()
            };
            return name;
        }
    }
}
