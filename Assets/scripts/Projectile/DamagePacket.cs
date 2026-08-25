using System.Collections.Generic;
using UnityEngine;

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
