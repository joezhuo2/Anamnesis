using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class EntityStatManager : MonoBehaviour
{
    public EntityStats baseStats;
    [HideInInspector] public EntityStats s;
    [HideInInspector] public List<StatBuff> currentBuffs = new();
    private void Awake()
    {
        if (baseStats != null) s = Instantiate(baseStats);

        if (gameObject.CompareTag("Enemy") && s.level > 1)
            ScaleBaseStats(s.level);
    }
    private void Start()
    {
        s.currentHp = s.EffMaxHp;
        s.canAttack = true;
        s.isAttacking = false;
        s.canMove = true;
        s.canGainHp = true;
        s.isAlive = true;
        s.isImmune = false;
    }

    public void ScaleStatsToLevel(int targetLevel)
    {
        if (s == null) return;

        s.level = targetLevel;

        if (s.level > 1) ScaleBaseStats(s.level);
    }
    private void ScaleBaseStats(int currentLevel)
    {
        int levelOffset = currentLevel - 1;

        s.attack += 3 * levelOffset;
        s.atkPct += 3f * levelOffset;

        s.maxHp += 10 * levelOffset;
        s.hpPct += 10f * levelOffset;

        s.hpRegen = Mathf.RoundToInt(s.hpRegen * (1f + (0.05f * levelOffset)));
        s.hpRegPct += 2f * levelOffset;

        s.armor += 4 * levelOffset;
        s.armorPct += 2f * levelOffset;

        s.damagePct += levelOffset;

        if (levelOffset % 5 == 0)
        {
            s.physicalDmgPct  += 0.5f * levelOffset;
            s.spellDmgPct += 0.5f * levelOffset;
            s.aoePct += 2f * levelOffset;

            s.critChance = Mathf.Clamp(s.critChance * (1f + (0.03f * levelOffset)), 0f, 100f);
            s.critDamage += 2f * levelOffset;

            s.damageRes = Mathf.Clamp(s.damageRes + (0.2f * levelOffset), 0f, 40f);
            s.physicalRes = Mathf.Clamp(s.physicalRes + (0.4f * levelOffset), -100f, 40f);
            s.spellRes = Mathf.Clamp(s.spellRes + (0.4f * levelOffset), -100f, 40f);

            s.dodgeChance = Mathf.Clamp(s.dodgeChance + (0.3f * levelOffset), 0f, 50f);
            s.dodgeResPct = Mathf.Clamp(s.dodgeResPct + (0.5f * levelOffset), 0f, 60f);

            s.moveSpeedPct += Mathf.Clamp(2f * levelOffset, 0f, 200f);
        }
    }

    private void OnDestroy()
    {
        if (s != null) Destroy(s);
    }
    private static readonly Dictionary<StatType, FieldInfo> cachedFields = new();
    public float GetStat(StatType type)
    {
        float value = type switch
        {
            StatType.attack => s.attack,
            StatType.atkPct => s.atkPct,
            StatType.damagePct => s.damagePct,
            StatType.physicalDmgPct => s.physicalDmgPct,
            StatType.spellDmgPct => s.spellDmgPct,
            StatType.critChance => s.critChance,
            StatType.critDamage => s.critDamage,
            StatType.aoePct => s.aoePct,
            StatType.maxHp => s.maxHp,
            StatType.hpPct => s.hpPct,
            StatType.hpRegen => s.hpRegen,
            StatType.hpRegPct => s.hpRegPct,
            StatType.armor => s.armor,
            StatType.armorPct => s.armorPct,
            StatType.damageRes => s.damageRes,
            StatType.physicalRes => s.physicalRes,
            StatType.spellRes => s.spellRes,
            StatType.dodgeChance => s.dodgeChance,
            StatType.dodgeResPct => s.dodgeResPct,
            StatType.moveSpeedPct => s.moveSpeedPct,
            StatType.attackSpeedPct => s.attackSpeedPct,
            StatType.defShred => s.defShred,
            StatType.resPen => s.resPen,
            StatType.maxStamina => s.maxStamina,
            StatType.staminaRegen => s.staminaRegen,
            StatType.stRegPct => s.stRegPct,
            StatType.addPhysDmgPct => s.addPhysDmgPct,
            StatType.addSplDmgPct => s.addSplDmgPct,
            StatType.currentHp => s.currentHp,
            StatType.moveSpeed => s.moveSpeed,
            StatType.EffMaxHp => s.EffMaxHp,
            StatType.EffAtk => s.EffAtk,
            StatType.EffHpReg => s.EffHpReg,
            StatType.EffStReg => s.EffStReg,
            StatType.EffSpd => s.FinalSpd,
            StatType.EffArmor => s.EffArmor,
            StatType.maxMana => s.maxMana,
            StatType.SkillDmgPct => s.skillDmgPct,
            StatType.BasicDmgPct => s.basicDmgPct,
            StatType.UltDmgPct => s.ultDmgPct,
            StatType.EffectRes => s.effectRes,
            _ => 0f,
        };
        return value;
    }

    public void AddStat(StatBuff b, bool isAdding = true)
    {
        float factor = isAdding ? 1f : -1f;
        float mod = b.value * factor;

        switch (b.type)
        {
            case StatType.attack: s.attack += Mathf.RoundToInt(mod); break;
            case StatType.atkPct: s.atkPct += mod; break;
            case StatType.damagePct: s.damagePct += mod; break;
            case StatType.physicalDmgPct: s.physicalDmgPct += mod; break;
            case StatType.spellDmgPct: s.spellDmgPct += mod; break;
            case StatType.critChance: s.critChance += mod; break;
            case StatType.critDamage: s.critDamage += mod; break;
            case StatType.aoePct: s.aoePct += mod; break;
            case StatType.maxHp: s.maxHp += Mathf.RoundToInt(mod); break;
            case StatType.hpPct: s.hpPct += mod; break;
            case StatType.hpRegen: s.hpRegen += Mathf.RoundToInt(mod); break;
            case StatType.hpRegPct: s.hpRegPct += mod; break;
            case StatType.armor: s.armor += Mathf.RoundToInt(mod); break;
            case StatType.armorPct: s.armorPct += mod; break;
            case StatType.damageRes: s.damageRes += mod; break;
            case StatType.physicalRes: s.physicalRes += mod; break;
            case StatType.spellRes: s.spellRes += mod; break;
            case StatType.dodgeChance: s.dodgeChance += mod; break;
            case StatType.dodgeResPct: s.dodgeResPct += mod; break;
            case StatType.moveSpeedPct: s.moveSpeedPct += mod; break;
            case StatType.attackSpeedPct: s.attackSpeedPct += mod; break;
            case StatType.defShred: s.defShred += Mathf.RoundToInt(mod); break;
            case StatType.resPen: s.resPen += mod; break;
            case StatType.maxStamina: s.maxStamina += Mathf.RoundToInt(mod); break;
            case StatType.staminaRegen: s.staminaRegen += Mathf.RoundToInt(mod); break;
            case StatType.stRegPct: s.stRegPct += mod; break;
            case StatType.addPhysDmgPct: s.addPhysDmgPct += mod; break;
            case StatType.addSplDmgPct: s.addSplDmgPct += mod; break;
            case StatType.moveSpeed: s.moveSpeed += mod; break;
            case StatType.maxMana: s.maxMana += Mathf.RoundToInt(mod); break;
            case StatType.SkillDmgPct: s.skillDmgPct += mod; break;
            case StatType.BasicDmgPct: s.basicDmgPct += mod; break;
            case StatType.UltDmgPct: s.ultDmgPct += mod; break;
            case StatType.EffectRes: s.effectRes += mod; break;
            default: break;

        }
    }
}

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
    public readonly bool Equals(StatBuff other)
    {
        return type == other.type && Mathf.Approximately(value, other.value);
    }

    public override bool Equals(object obj) => obj is StatBuff other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(type, value);
    public override string ToString()
    {
        string name = type switch
        {
            StatType.attack => "Attack",
            StatType.atkPct => "Attack %",
            StatType.damagePct => "Damage %",
            StatType.physicalDmgPct => "Physical Damage %",
            StatType.spellDmgPct => "Spell Damage %",
            StatType.critChance => "Crit Chance",
            StatType.critDamage => "Crit Damage",
            StatType.aoePct => "AoE %",
            StatType.maxHp => "Max HP",
            StatType.hpPct => "HP %",
            StatType.hpRegen => "HP Regen",
            StatType.hpRegPct => "HP Regen %",
            StatType.armor => "Armor",
            StatType.armorPct => "Armor %",
            StatType.damageRes => "Damage Resistance",
            StatType.physicalRes => "Physical Resistance",
            StatType.spellRes => "Spell Resistance",
            StatType.dodgeChance => "Dodge Chance",
            StatType.dodgeResPct => "Dodge Resistance %",
            StatType.moveSpeedPct => "Move Speed %",
            StatType.attackSpeedPct => "Attack Speed %",
            StatType.defShred => "Defense Shred",
            StatType.resPen => "Resistance Penetration",
            StatType.maxStamina => "Max Stamina",
            StatType.staminaRegen => "Stamina Regen",
            StatType.stRegPct => "Stamina Regen %",
            StatType.addPhysDmgPct => "Add Phys Dmg %",
            StatType.addSplDmgPct => "Add Spl Dmg %",
            StatType.maxMana => "Max Mana",
            StatType.SkillDmgPct => "Skill Damage %",
            StatType.BasicDmgPct => "Basic Damage %",
            StatType.UltDmgPct => "Ultimate Damage %",
            StatType.EffectRes => "Effect Resistance",
            _ => type.ToString()
        };
        return name;
    }
}