using System.Collections.Generic;
using UnityEngine;

public static class DamageCalculator
{
    public static DamagePacket BuildDamagePacket(EntityStats stats, AttackData attack)
    {
        DamagePacket dp = new();
        ProjectileData pd = attack.projectilePrefab.GetComponent<ProjectileData>();

        var typeMults = new Dictionary<DamageType, float>
        {
            { DamageType.Physical, pd.physicalMult },
            { DamageType.Spell, pd.spellMult }
        };

        foreach(var kvp in typeMults)
        {
            float mult = kvp.Value;
            if (mult <= 0) continue;

            DamageType type = kvp.Key;

            float damage = stats.attack * mult * TypeBonus(type, stats);

            var (finalDamage, isCrit) = RollCrits(damage, stats);

            dp.AddInstance(type, finalDamage, isCrit);
        }
        return dp;
    }
    private static float TypeBonus(DamageType type, EntityStats stats) => type switch
    {
        DamageType.Physical => 1f + stats.physicalDmgPct / 100f,
        DamageType.Spell => 1f + stats.spellDmgPct / 100f,
        _ => 1f
    };

    public static (float damage, bool isCrit) RollCrits(float baseDamage, EntityStats stats)
    {
        float roll = Random.Range(0f, 1000f) / 10f;
        if (roll <= stats.critChance)
        {
            float critDamage = baseDamage * (100f + stats.critDamage) / 100f;
            return (critDamage, true);
        }
        return (baseDamage, false);
    }
}