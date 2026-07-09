using System.Collections.Generic;
using UnityEngine;

public enum DamageType { Physical, Spell, DoT, True }

public struct DamageInstance
{
    public DamageType type;
    public float amount;
    public bool isCrit;
    public Color indicatorColor;

    public DamageInstance(DamageType type, float amount, bool isCrit, Color indicatorColor = default)
    {
        this.type = type;
        this.amount = amount;
        this.isCrit = isCrit;
        this.indicatorColor = indicatorColor;
    }
}

public class DamagePacket
{
    public List<DamageInstance> instances = new();
    public void AddInstance(DamageType type, float amount, bool isCrit)
        => instances.Add(new DamageInstance(type, amount, isCrit));

    public void AddInstance(DamageType type, float amount, bool isCrit, Color indicatorColor)
        => instances.Add(new DamageInstance(type, amount, isCrit, indicatorColor));

    public float GetTotalDamage()
    {
        float total = 0f;
        foreach (var i in instances)
            total += i.amount;
        return total;
    }
}
