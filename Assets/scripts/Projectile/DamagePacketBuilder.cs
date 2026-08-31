
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    public static class DamagePacketBuilder
    {
        public static DamagePacket BuildDamagePacket(ProjectileData pd, ProjectileDamageSnapshot snapshot, bool rollCrits, GameObject owner, bool bypassIFrames, float sizeOverride)
        {
            DamagePacket dp = new() { source = owner, bypassIFrames = bypassIFrames, sizeOverride = sizeOverride };
            if (pd == null || !snapshot.isValid) return dp;

            float attackTypeBonus = pd.mainAttack != null ? DamageCalculator.AttackTypeBonus(pd.mainAttack.type, snapshot) : 1f;

            void AddDamageIfValid(DamageType type, float mult)
            {
                float addMultPct = DamageCalculator.GetAdditionalScaling(snapshot, type);

                if (mult == 0f && addMultPct == 0f) return;

                float dmgMult = 1f + (snapshot.damagePct * 0.01f);
                float finalMult = mult + (addMultPct * 0.01f);

                float damage = snapshot.scalingValue *
                    snapshot.specialMult *
                    dmgMult * finalMult *
                    DamageCalculator.TypeBonus(type, snapshot) *
                    attackTypeBonus;
                var (finalDamage, isCrit) = rollCrits ? DamageCalculator.RollCrits(damage, snapshot.critChance, snapshot.critDamage) : (damage, false);
                dp.AddInstance(type, finalDamage, isCrit, default, owner);
            }

            AddDamageIfValid(DamageType.Physical, pd.physicalMult);
            AddDamageIfValid(DamageType.Spell, pd.spellMult);
            AddDamageIfValid(DamageType.True, pd.trueMult);

            return dp;
        }

        public static DamagePacket BuildDamagePacket(float baseDamage, DamageType type, bool rollCrits, Color indicatorColor, GameObject owner, bool bypassIFrames, float sizeOverride)
            => DamageRoll.Build(baseDamage, type, rollCrits, indicatorColor, owner, bypassIFrames, sizeOverride);
    }
}
