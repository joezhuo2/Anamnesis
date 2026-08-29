using UnityEngine;

namespace CrystalFlux.Core
{
    public static class DamageRoll
    {
        public static (float damage, bool isCrit) RollCrits(float baseDamage, float critChance, float critDamage)
        {
            float roll = Random.Range(0f, 1000f) / 10f;
            if (roll <= critChance)
            {
                float critDmg = baseDamage * (100f + critDamage) * 0.01f;
                return (critDmg, true);
            }
            return (baseDamage, false);
        }

        public static DamagePacket Build(float baseDamage, DamageType type, bool rollCrits, Color indicatorColor, GameObject owner, bool bypassIFrames, float sizeOverride)
        {
            DamagePacket dp = new() { source = owner, bypassIFrames = bypassIFrames, sizeOverride = sizeOverride };
            if (!owner.TryGetComponent<IStatProvider>(out var esm) || baseDamage <= 0f) return dp;

            var (finalDamage, isCrit) = rollCrits ? RollCrits(baseDamage, esm.GetStat(StatType.critChance), esm.GetStat(StatType.critDamage)) : (baseDamage, false);
            dp.AddInstance(type, finalDamage, isCrit, indicatorColor, owner);
            return dp;
        }
    }
}
