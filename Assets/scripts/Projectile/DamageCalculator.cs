using Unity.VisualScripting;
using UnityEngine;

public static class DamageCalculator
{
    public static DamagePacket BuildDamagePacket(ProjectileData pd, GameObject source)
    {
        DamagePacket dp = new();

        void AddDamageIfValid(DamageType type, float mult)
        {
            if (mult <= 0) return;
            if (!source.TryGetComponent<EntityStatManager>(out var esm)) return;

            float effAtk = esm.s.attack * (1f + (esm.s.atkPct * 0.01f));
            float damage = effAtk * (1f + (esm.s.damagePct * 0.01f)) * mult * TypeBonus(type, esm.s);
            var (finalDamage, isCrit) = RollCrits(damage, esm.s);
            dp.AddInstance(type, finalDamage, isCrit);
        }

        AddDamageIfValid(DamageType.Physical, pd.physicalMult);
        AddDamageIfValid(DamageType.Spell, pd.spellMult);

        return dp;
    }
    private static float TypeBonus(DamageType type, EntityStats stats) => type switch
    {
        DamageType.Physical => 1f + (stats.physicalDmgPct * 0.01f),
        DamageType.Spell => 1f + (stats.spellDmgPct * 0.01f),
        _ => 1f
    };

    public static (float damage, bool isCrit) RollCrits(float baseDamage, EntityStats stats)
    {
        float roll = Random.Range(0f, 1000f) / 10f;
        if (roll <= stats.critChance)
        {
            float critDamage = baseDamage * (100f + stats.critDamage) * 0.01f;
            return (critDamage, true);
        }
        return (baseDamage, false);
    }
}