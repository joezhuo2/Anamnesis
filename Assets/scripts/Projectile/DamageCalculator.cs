using Unity.VisualScripting;
using UnityEngine;

public struct ProjectileDamageSnapshot
{
    public float scalingValue;
    public float damagePct;
    public float addPhysDmgPct;
    public float addSplDmgPct;
    public float physicalDmgPct;
    public float spellDmgPct;
    public float basicDmgPct;
    public float skillDmgPct;
    public float ultDmgPct;
    public float critChance;
    public float critDamage;
    public int defShred;
    public float resPen;
    public bool isValid;
}

public static class DamageCalculator
{
    public static ProjectileDamageSnapshot CaptureSnapshot(ProjectileData pd, GameObject source)
    {
        ProjectileDamageSnapshot snapshot = new() { isValid = false };
        if (pd == null || source == null) return snapshot;
        if (!source.TryGetComponent<EntityStatManager>(out var esm) || esm.s == null) return snapshot;

        snapshot.scalingValue = esm.GetStat(pd.scalingStat);
        snapshot.damagePct = esm.s.damagePct;
        snapshot.addPhysDmgPct = esm.s.addPhysDmgPct;
        snapshot.addSplDmgPct = esm.s.addSplDmgPct;
        snapshot.physicalDmgPct = esm.s.physicalDmgPct;
        snapshot.spellDmgPct = esm.s.spellDmgPct;
        snapshot.basicDmgPct = esm.s.basicDmgPct;
        snapshot.skillDmgPct = esm.s.skillDmgPct;
        snapshot.ultDmgPct = esm.s.ultDmgPct;
        snapshot.critChance = esm.s.critChance;
        snapshot.critDamage = esm.s.critDamage;
        snapshot.defShred = esm.s.defShred;
        snapshot.resPen = esm.s.resPen;
        snapshot.isValid = true;
        return snapshot;
    }

    public static DamagePacket BuildDamagePacket(ProjectileData pd, GameObject source)
    {
        ProjectileDamageSnapshot snapshot = CaptureSnapshot(pd, source);
        return BuildDamagePacket(pd, snapshot);
    }

    public static DamagePacket BuildDamagePacket(ProjectileData pd, ProjectileDamageSnapshot snapshot)
    {
        DamagePacket dp = new();
        if (pd == null || !snapshot.isValid) return dp;

        void AddDamageIfValid(DamageType type, float mult)
        {
            float addMultPct = type switch
            {
                DamageType.Physical => snapshot.addPhysDmgPct,
                DamageType.Spell => snapshot.addSplDmgPct,
                _ => 0f
            };

            float dmgMult = 1f + (snapshot.damagePct * 0.01f);
            float finalMult = mult + (addMultPct * 0.01f);

            float damage = snapshot.scalingValue * dmgMult * finalMult * TypeBonus(type, snapshot) * AttackTypeBonus(pd.mainAttack.type, snapshot);
            var (finalDamage, isCrit) = RollCrits(damage, snapshot);
            dp.AddInstance(type, finalDamage, isCrit);
        }

        AddDamageIfValid(DamageType.Physical, pd.physicalMult);
        AddDamageIfValid(DamageType.Spell, pd.spellMult);
        AddDamageIfValid(DamageType.True, pd.trueMult);

        return dp;
    }
    private static float TypeBonus(DamageType type, ProjectileDamageSnapshot snapshot) => type switch
    {
        DamageType.Physical => 1f + (snapshot.physicalDmgPct * 0.01f),
        DamageType.Spell => 1f + (snapshot.spellDmgPct * 0.01f),
        _ => 1f
    };

    private static float AttackTypeBonus(AttackType type, ProjectileDamageSnapshot snapshot) => type switch
    {
        AttackType.Basic => 1f + (snapshot.basicDmgPct * 0.01f),
        AttackType.Skill => 1f + (snapshot.skillDmgPct * 0.01f),
        AttackType.Ultimate => 1f + (snapshot.ultDmgPct * 0.01f),
        _ => 1f
    };

    private static float TypeBonus(DamageType type, EntityStats stats) => type switch
    {
        DamageType.Physical => 1f + (stats.physicalDmgPct * 0.01f),
        DamageType.Spell => 1f + (stats.spellDmgPct * 0.01f),
        _ => 1f
    };
    private static float AttackTypeBonus(AttackType type, EntityStats stats) => type switch
    {
        AttackType.Basic => 1f + (stats.basicDmgPct * 0.01f),
        AttackType.Skill => 1f + (stats.skillDmgPct * 0.01f),
        AttackType.Ultimate => 1f + (stats.ultDmgPct * 0.01f),
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

    public static (float damage, bool isCrit) RollCrits(float baseDamage, ProjectileDamageSnapshot snapshot)
    {
        float roll = Random.Range(0f, 1000f) / 10f;
        if (roll <= snapshot.critChance)
        {
            float critDamage = baseDamage * (100f + snapshot.critDamage) * 0.01f;
            return (critDamage, true);
        }
        return (baseDamage, false);
    }
}