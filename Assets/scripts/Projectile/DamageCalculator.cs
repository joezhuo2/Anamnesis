using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    public static class DamageCalculator
    {
        public static (float dmg, float size) CalculateDamageTaken(DamageType type, float rawDamage, IStatProvider esm, IStatProvider atk = null)
        {
            float typeRes = type switch
            {
                DamageType.Physical => esm.GetStat(StatType.physicalRes),
                DamageType.Spell => esm.GetStat(StatType.spellRes),
                _ => 0f
            };

            float resPen = atk != null ? atk.GetStat(StatType.resPen) : 0f;
            float defShred = atk != null ? atk.GetStat(StatType.defShred) : 0f;

            float effRes = Mathf.Max(-100f, esm.GetStat(StatType.damageRes) + typeRes - resPen);
            float resMult = 1f - (effRes * 0.01f);

            float effArmor = esm.GetStat(StatType.EffArmor) - defShred;
            float armorMult = 1f;
            if (type == DamageType.Physical)
                armorMult = effArmor >= 0f ? 100f / (effArmor + 100f) : 3f - (100f / (100f - effArmor));

            float finalDamage = rawDamage * resMult * armorMult;
            float size = 1f;

            float dc = esm.GetStat(StatType.dodgeChance) * 0.01f;
            float dodgeMult = 1f - (esm.GetStat(StatType.dodgeResPct) * 0.01f);

            if (UnityEngine.Random.Range(0f, 1f) < dc)
            {
                finalDamage *= dodgeMult;
                size = 0.7f;
            }

            return (finalDamage, size);
        }

        public static float GetAdditionalScaling(ProjectileDamageSnapshot snapshot, DamageType type) => type switch
        {
            DamageType.True => snapshot.addTrueDmgPct,
            DamageType.Physical => snapshot.addPhysDmgPct,
            DamageType.Spell => snapshot.addSplDmgPct,
            _ => 0f
        };

        public static float TypeBonus(DamageType type, ProjectileDamageSnapshot snapshot) => type switch
        {
            DamageType.Physical => 1f + (snapshot.physicalDmgPct * 0.01f),
            DamageType.Spell => 1f + (snapshot.spellDmgPct * 0.01f),
            _ => 1f
        };

        public static float AttackTypeBonus(AttackType type, ProjectileDamageSnapshot snapshot) => type switch
        {
            AttackType.Basic => 1f + (snapshot.basicDmgPct * 0.01f),
            AttackType.Skill => 1f + (snapshot.skillDmgPct * 0.01f),
            AttackType.Ultimate => 1f + (snapshot.ultDmgPct * 0.01f),
            AttackType.Additional => 1f + (snapshot.addDmgPct * 0.01f),
            _ => 1f
        };

        public static (float damage, bool isCrit) RollCrits(float baseDamage, float critChance, float critDamage)
            => DamageRoll.RollCrits(baseDamage, critChance, critDamage);

        public static float CalculateHpConsumedMult(ProjectileData pd, IStatProvider esm)
        {
            if (pd.mainAttack == null) return 1f;

            float totalHealthCost = Mathf.Abs(pd.mainAttack.healthCost + (esm.GetStat(StatType.EffMaxHp) * (pd.mainAttack.healthCostPct * 0.01f)));
            if (totalHealthCost <= 0f) return 1f;

            float hpConsumedPct = totalHealthCost / esm.GetStat(StatType.EffMaxHp) * 100f;
            return 1f + (hpConsumedPct * pd.specialMult * 0.01f);
        }
    }
}
