using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class EntityStatManager : MonoBehaviour
{
    public EntityStats s;
    public List<StatBuff> currentBuffs = new();
    private void Awake()
    {
        if (s != null) s = Instantiate(s);

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

        s.attack += 5 * levelOffset;
        s.atkPct += 2f * levelOffset;

        s.maxHp += 15 * levelOffset;
        s.hpPct += 5f * levelOffset;
        s.currentHp = s.EffMaxHp;

        s.hpRegen = Mathf.RoundToInt(s.hpRegen * (1f + (0.05f * levelOffset)));
        s.hpRegPct += 2f * levelOffset;

        s.armor += 5 * levelOffset;
        s.armorPct += 2f * levelOffset;

        s.damagePct += levelOffset;
        s.physicalDmgPct *= (1f + (0.05f * levelOffset));
        s.spellDmgPct *= (1f + (0.05f * levelOffset));

        if (levelOffset % 5 == 0)
        {
            s.aoePct += 2f * levelOffset;

            s.critChance = Mathf.Clamp(s.critChance * (1f + (0.03f * levelOffset)), 0f, 100f);
            s.critDamage += 2f * levelOffset;

            s.damageRes = Mathf.Clamp(s.damageRes + (0.3f * levelOffset), 0f, 50f);
            s.physicalRes = Mathf.Clamp(s.physicalRes * (1f + (0.1f * levelOffset)), -100f, 70f);
            s.spellRes = Mathf.Clamp(s.spellRes * (1f + (0.1f * levelOffset)), -100f, 70f);

            s.dodgeChance = Mathf.Clamp(s.dodgeChance + (0.3f * levelOffset), 0f, 40f);
            s.dodgeResPct = Mathf.Clamp(s.dodgeResPct + (0.5f * levelOffset), 0f, 60f);

            s.moveSpeedPct = Mathf.Clamp(2f * levelOffset, 0f, 300f);
        }
    }

    private void OnDestroy()
    {
        if (s != null) Destroy(s);
    }
    private static readonly Dictionary<StatType, FieldInfo> cachedFields = new();

    public void AddStat(StatBuff b, bool isAdding = true, bool show = false)
    {
        float factor = isAdding ? 1f : -1f;

        if (!cachedFields.TryGetValue(b.type, out FieldInfo field))
        {
            field = typeof(EntityStats).GetField(b.type.ToString(), BindingFlags.Public | BindingFlags.Instance);
            cachedFields[b.type] = field;
        }

        if (field != null)
        {
            if (field.FieldType == typeof(int))
            {
                int current = (int)field.GetValue(s);
                field.SetValue(s, current + Mathf.RoundToInt(b.value * factor));
            }
            else if (field.FieldType == typeof(float))
            {
                float current = (float)field.GetValue(s);
                field.SetValue(s, current + (b.value * factor));
            }
        }

        if (show)
        {
            if (isAdding) currentBuffs.Add(b);
            else if (currentBuffs.Contains(b)) currentBuffs.Remove(b);
        }
    }
    public void AddBuffForDuration(StatBuff b, float duration, bool show) => StartCoroutine(AddBuffForDurationRoutine(b, duration, show));
    private IEnumerator AddBuffForDurationRoutine(StatBuff b, float duration, bool show)
    {
        AddStat(b, true, show);
        yield return new WaitForSeconds(duration);
        AddStat(b, false, show);
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
            _ => type.ToString()
        };
        return name;
    }
}