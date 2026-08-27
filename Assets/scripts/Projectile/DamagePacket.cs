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

    public static DamagePacket BuildDamagePacket(ProjectileData pd, ProjectileDamageSnapshot snapshot, bool rollCrits, GameObject owner)
    {
        DamagePacket dp = new();
        if (pd == null || !snapshot.isValid) return dp;

        void AddDamageIfValid(DamageType type, float mult)
        {
            float addMultPct = DamageCalculator.GetAdditionalScaling(snapshot, type);
            float dmgMult = 1f + (snapshot.damagePct * 0.01f);
            float finalMult = mult + (addMultPct * 0.01f);

            float damage = snapshot.scalingValue *
                snapshot.specialMult *
                dmgMult * finalMult *
                DamageCalculator.TypeBonus(type, snapshot) *
                DamageCalculator.AttackTypeBonus(pd.mainAttack.type, snapshot);
            var (finalDamage, isCrit) = rollCrits ? DamageCalculator.RollCrits(damage, snapshot.critChance, snapshot.critDamage) : (damage, false);
            dp.AddInstance(type, finalDamage, isCrit, default, owner);
        }

        AddDamageIfValid(DamageType.Physical, pd.physicalMult);
        AddDamageIfValid(DamageType.Spell, pd.spellMult);
        AddDamageIfValid(DamageType.True, pd.trueMult);

        return dp;
    }

    public static DamagePacket BuildDamagePacket(float baseDamage, DamageType type, bool rollCrits, Color indicatorColor, GameObject owner)
    {
        DamagePacket dp = new();
        if (!owner.TryGetComponent<EntityStatManager>(out var esm) || baseDamage <= 0f) return dp;

        var (finalDamage, isCrit) = rollCrits ? DamageCalculator.RollCrits(baseDamage, esm.GetStat(StatType.critChance), esm.GetStat(StatType.critDamage)) : (baseDamage, false);
        dp.AddInstance(type, finalDamage, isCrit, indicatorColor, owner);
        return dp;
    }
}
