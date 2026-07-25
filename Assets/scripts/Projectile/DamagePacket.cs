using System.Collections.Generic;
using UnityEngine;

public enum DamageType { Physical, Spell, DoT, True }

public struct DamageInstance
{
    public DamageType type;
    public float amount;
    public bool isCrit;
    public Color indicatorColor;
    public GameObject owner;

    public DamageInstance(DamageType type, float amount, bool isCrit, Color indicatorColor, GameObject owner)
    {
        this.type = type;
        this.amount = amount;
        this.isCrit = isCrit;
        this.indicatorColor = indicatorColor;
        this.owner = owner;
    }
}

public class DamagePacket
{
    public List<DamageInstance> instances = new();
    public void AddInstance(DamageType type, float amount, bool isCrit, GameObject owner)
        => instances.Add(new DamageInstance(type, amount, isCrit, default, owner));

    public void AddInstance(DamageType type, float amount, bool isCrit, Color indicatorColor, GameObject owner)
        => instances.Add(new DamageInstance(type, amount, isCrit, indicatorColor, owner));

    public float GetTotalDamage()
    {
        float total = 0f;
        foreach (var i in instances)
            total += i.amount;
        return total;
    }
}
