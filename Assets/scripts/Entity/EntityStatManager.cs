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

        s.canAttack = true;
        s.isAttacking = false;
        s.canGainHp = true;
        s.isAlive = true;
        s.isImmune = false;
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
}