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
    private static readonly Dictionary<string, FieldInfo> cachedFields = new();

    public void AddStat(StatBuff b, bool isAdding = true, bool show = false)
    {
        float factor = isAdding ? 1f : -1f;

        if (!cachedFields.TryGetValue(b.name, out FieldInfo field))
        {
            field = typeof(EntityStats).GetField(b.name, BindingFlags.Public | BindingFlags.Instance);
            cachedFields[b.name] = field;
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
    public string name;
    public float value;

    public readonly bool Equals(StatBuff other)
    {
        return name == other.name && Mathf.Approximately(value, other.value);
    }

    public override bool Equals(object obj) => obj is StatBuff other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(name, value);
}