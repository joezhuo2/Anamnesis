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

        s.attack = Mathf.RoundToInt(s.attack * (1f + (0.10f * levelOffset)));
        s.atkPct += 2f * levelOffset;

        s.damagePct += 0.01f * levelOffset;
        s.physicalDmgPct *= (1f + (0.03f * levelOffset));
        s.spellDmgPct *= (1f + (0.03f * levelOffset));

        s.critChance = Mathf.Clamp(s.critChance * (1f + (0.01f * levelOffset)), 0f, 100f);
        s.critDamage *= (1f + (0.03f * levelOffset));

        s.aoePct += 0.3f * levelOffset;

        s.maxHp = Mathf.RoundToInt(s.maxHp * (1f + (0.10f * levelOffset)));
        s.hpPct += 5f * levelOffset;

        s.currentHp = s.EffMaxHp;

        s.hpRegen = Mathf.RoundToInt(s.hpRegen * (1f + (0.05f * levelOffset)));
        s.hpRegPct += 2f * levelOffset;
        s.armor = Mathf.RoundToInt(s.armor * (1f + (0.03f * levelOffset)));
        s.armorPct += 2f * levelOffset;

        s.damageRes = Mathf.Clamp(s.damageRes + (0.2f * levelOffset), 0f, 50f);
        s.physicalRes = Mathf.Clamp(s.physicalRes + (0.3f * levelOffset), 0f, 70f);
        s.spellRes = Mathf.Clamp(s.spellRes + (0.3f * levelOffset), 0f, 70f);

        s.dodgeChance = Mathf.Clamp(s.dodgeChance + (0.3f * levelOffset), 0f, 40f);
        s.dodgeResPct = Mathf.Clamp(s.dodgeResPct + (0.5f * levelOffset), 0f, 60f);

        s.moveSpeed = Mathf.Clamp(s.moveSpeed * (1f + (0.01f * levelOffset)), 0.1f, 2.0f);
        s.moveSpeedPct = Mathf.Clamp(s.moveSpeedPct + (0.3f * levelOffset), 0f, 80f);
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
    public float GetBaseStat(StatType type)
    {
        if (!cachedFields.TryGetValue(type, out FieldInfo field))
        {
            field = typeof(EntityStats).GetField(type.ToString(), BindingFlags.Public | BindingFlags.Instance);
            cachedFields[type] = field;
        }

        if (field == null) return 0f;

        float currentVal = 0f;
        if (field.FieldType == typeof(int)) currentVal = (int)field.GetValue(s);
        else if (field.FieldType == typeof(float)) currentVal = (float)field.GetValue(s);

        foreach (var buff in currentBuffs)
            if (buff.type == type) currentVal -= buff.value;

        return currentVal;
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
}