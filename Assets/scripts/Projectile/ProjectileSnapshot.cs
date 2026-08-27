using UnityEngine;

public struct ProjectileDamageSnapshot
{
    public float scalingValue;
    public float specialMult;
    public float damagePct;
    public float addPhysDmgPct;
    public float addSplDmgPct;
    public float addTrueDmgPct;
    public float physicalDmgPct;
    public float spellDmgPct;
    public float basicDmgPct;
    public float skillDmgPct;
    public float ultDmgPct;
    public float addDmgPct;
    public float critChance;
    public float critDamage;
    public int defShred;
    public float resPen;
    public bool isValid;
    public GameObject owner;
}

public static class ProjectileSnapshot
{
    public static ProjectileDamageSnapshot CaptureSnapshot(ProjectileData pd, GameObject source)
    {
        ProjectileDamageSnapshot snapshot = new() { isValid = false };
        if (pd == null || source == null) return snapshot;
        if (!source.TryGetComponent<EntityStatManager>(out var esm) || esm == null) return snapshot;

        snapshot.scalingValue = esm.GetStat(pd.scalingStat);
        snapshot.specialMult = (pd.specialSclaing) switch
        {
            SpecialScalingAttribute.Orbits => source.TryGetComponent<EntityProjectileHandler>(out var eph) ? 1f + (eph.OrbitCount * pd.specialMult) : 1f,
            SpecialScalingAttribute.HpConsumed => DamageCalculator.CalculateHpConsumedMult(pd, esm),
            _ => 1f
        };
        snapshot.damagePct = esm.GetStat(StatType.damagePct);
        snapshot.addPhysDmgPct = esm.GetStat(StatType.addPhysDmgPct);
        snapshot.addSplDmgPct = esm.GetStat(StatType.addSplDmgPct);
        snapshot.addTrueDmgPct = esm.GetStat(StatType.addTrueDmgPct);
        snapshot.physicalDmgPct = esm.GetStat(StatType.physicalDmgPct);
        snapshot.spellDmgPct = esm.GetStat(StatType.spellDmgPct);
        snapshot.basicDmgPct = esm.GetStat(StatType.BasicDmgPct);
        snapshot.skillDmgPct = esm.GetStat(StatType.SkillDmgPct);
        snapshot.ultDmgPct = esm.GetStat(StatType.UltDmgPct);
        snapshot.critChance = esm.GetStat(StatType.critChance);
        snapshot.critDamage = esm.GetStat(StatType.critDamage);
        snapshot.defShred = Mathf.RoundToInt(esm.GetStat(StatType.defShred));
        snapshot.resPen = esm.GetStat(StatType.resPen);
        snapshot.addDmgPct = esm.GetStat(StatType.addDmgPct);
        snapshot.isValid = true;
        snapshot.owner = source;
        return snapshot;
    }
}