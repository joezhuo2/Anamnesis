using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    [CreateAssetMenu(fileName = "ad", menuName = "Data/Attack")]
    public class AttackData : AttackAsset
    {
        [Header("Basic")]
        public float cooldown;
        public GameObject projectilePrefab;
        public ProjectileData pd;
        public ProjectilePattern pattern;
        [Tooltip("Windup time before the attack resolves. 0 = instant")]
        public float castTime;
        [Tooltip("Whether the entity can move during the cast window")]
        public bool canMoveWhileCasting = true;
        [Tooltip("Time after performing the attack before projectiles spawn")]
        public float spawnDelay;
        public float spawnDistance;
        [Tooltip("Whether to spawn the projectile at a fixed distance according to spawn distance")]
        public bool fixedDistance;
        [Tooltip("Time after attack is performed before resetting the attack animation")]
        public float animationLength;

        [Header("Spawn Logic")]
        public int projectileCount = 1;
        [Tooltip("Random additional projectiles to spawn")]
        public int randomCount;
        [Tooltip("Spread angle for 'spread' attacks, and barrage radius for barrage attacks")]
        public float spread;
        [Tooltip("Maximum random increase/decrease to spread")]
        public float randomSpread;
        [Tooltip("Minimum delay between each projectile spawn if projectile count > 0")]
        public float minDelay;
        [Tooltip("Maximum delay between each projectile spawn if projectile count > 0")]
        public float maxDelay;

        [Header("Enemy Only")]
        [Tooltip("Maximum range for enemies to be able to use this attack")]
        public float maxRange;
        [Tooltip("Whether the enemy can move while performing this attaack")]
        public bool canMoveDuringAttack;
        [Range(0f, 100f)]
        [Tooltip("Minimum Hp % for enemy to use this attack")]
        public float minHpPct = 0f;
        [Range(0f, 100f)]
        [Tooltip("Maximum Hp % for enemy to use this attack")]
        public float maxHpPct = 100f;
        [Tooltip("Phase required to use this attack, -1 => no phase required")]
        public int phaseReq = -1;
        [Tooltip("Next attack for the enemy to use")]
        public AttackData nextAttack;

        [Header("Resource Costs (Player Only)")]
        public float staminaCost;
        public float staminaCostPct;
        public float healthCost;
        public float healthCostPct;
        public float manaCost;
        public float manaCostPct;

        [Header("Resource gains on hit (Player Only)")]
        public bool basedOnDmgDealt = true;
        public float staminaGainOnHit;
        public float staminaPctGainOnHit;
        public float healthGainOnHit;
        public float healthPctGainOnHit;
        public float manaGainOnHit;
        public float manaPctGainOnHit;

        [Header("Summoning")]
        [Tooltip("Chance (0-1) to summon an entity on the given condition. 0 = disabled.")]
        [Range(0f, 1f)] public float summonChance = 0f;
        public SummonCondition summonCondition = SummonCondition.None;

        [Header("Orbit Interactions")]
        [Tooltip("Fires all currently orbiting projectiles toward the mouse direction before spawning this attack.")]
        public bool fireOrbits;
        [Tooltip("Absorbs all currently orbiting projectiles, granting stat returns per projectile.")]
        public float absorbOrbitPct;
        [Tooltip("Redirects all currently orbiting projectiles toward the nearest enemy.")]
        public bool redirectOrbits;
        [Tooltip("Causes all currently orbiting projectiles to explode, triggering their additionalAttack at their current position.")]
        public bool explodeOrbits;
        public int redirectCount;

        [Header("Misc - Player Only")]
        public Sprite icon;
        public string displayName;

        [System.NonSerialized] private bool isRuntimeCopy;
        public override bool IsRuntimeCopy => isRuntimeCopy;

        public float GetEffCastTime(IStatProvider esm)
        {
            if (castTime <= 0f) return 0f;
            if (esm == null) return castTime;
            return castTime * Mathf.Clamp(1f - (esm.GetStat(StatType.castTimeRedPct) * 0.01f), 0.1f, 1f);
        }

        public void InitializeRuntimeCopy() => DeepClone();
        public override void DeepClone()
        {
            var visited = new HashSet<AttackData>();
            DeepCloneInternal(visited);
        }

        private void DeepCloneInternal(HashSet<AttackData> visited)
        {
            isRuntimeCopy = true;
            if (!visited.Add(this)) return;

            if (pd != null)
            {
                pd = Instantiate(pd);
                pd.mainAttack = this;

                if (pd.effects != null)
                {
                    var clonedEffects = new List<EffectData>(pd.effects.Count);
                    foreach (var ef in pd.effects)
                    {
                        var clonedEf = ef;
                        if (ef.effect != null)
                            clonedEf.effect = Instantiate(ef.effect);
                        clonedEffects.Add(clonedEf);
                    }
                    pd.effects = clonedEffects;
                }

                if (pd.additionalAttack != null)
                {
                    if (pd.additionalAttack == this)
                    {
                        pd.additionalAttack = this;
                    }
                    else
                    {
                        pd.additionalAttack = Instantiate(pd.additionalAttack);
                        pd.additionalAttack.DeepCloneInternal(visited);
                    }
                }
            }

            if (nextAttack != null)
            {
                nextAttack = Instantiate(nextAttack);
                nextAttack.DeepCloneInternal(visited);
            }
        }

        private void OnDestroy()
        {
            if (!isRuntimeCopy) return;

            if (pd != null)
            {
                if (pd.effects != null)
                {
                    foreach (var ef in pd.effects)
                        if (ef.effect != null) Destroy(ef.effect);
                }

                if (pd.additionalAttack != null && pd.additionalAttack != this) Destroy(pd.additionalAttack);

                Destroy(pd);
            }

            if (nextAttack != null) Destroy(nextAttack);
        }
        public override void GetTooltipLines(List<string> lines)
        {
            lines.Add($"Type: {type} ({pattern})");
            if (cooldown > 0f) lines.Add($"Cooldown: {cooldown:F1}s");
            if (castTime > 0f) lines.Add($"Cast Time: {castTime:F1}s{(canMoveWhileCasting ? string.Empty : " (rooted)")}");

            if (staminaCost > 0f || staminaCostPct > 0f) lines.Add($"Stamina Cost: {staminaCost:F0} +{staminaCostPct:F1}%");
            if (manaCost > 0f || manaCostPct > 0f) lines.Add($"Mana Cost: {manaCost:F0} +{manaCostPct:F1}%");
            if (healthCost > 0f || healthCostPct > 0f) lines.Add($"Health Cost: {healthCost:F0} +{healthCostPct:F1}%");

            if (healthGainOnHit > 0f || healthPctGainOnHit > 0f) lines.Add($"Health Gain: {healthGainOnHit:F0} +{healthPctGainOnHit:F1}%");
            if (staminaGainOnHit > 0f || staminaPctGainOnHit > 0f) lines.Add($"Stamina Gain: {staminaGainOnHit:F0} +{staminaPctGainOnHit:F1}%");
            if (manaGainOnHit > 0f || manaPctGainOnHit > 0f) lines.Add($"Mana Gain: {manaGainOnHit:F0} +{manaPctGainOnHit:F1}%");

            if (explodeOrbits) lines.Add($"Explodes all orbiting projectiles");
            if (fireOrbits) lines.Add($"Fires all orbiting projectiles");
            if (absorbOrbitPct > 0f) lines.Add($"Absorbs all orbiting projectiles ({absorbOrbitPct:F1}% stat returns)");
            if (redirectOrbits && redirectCount > 0) lines.Add($"Redirects {redirectCount} orbiting projectiles to nearest enemy");

            if (pd != null)
            {
                List<string> dmgTypes = new();
                if (pd.speed > 0f) lines.Add($"Speed: {pd.speed:F1}");
                if (pd.physicalMult > 0f) dmgTypes.Add($"{pd.physicalMult:F0}P");
                if (pd.spellMult > 0f) dmgTypes.Add($"{pd.spellMult:F0}S");
                if (pd.trueMult > 0f) dmgTypes.Add($"{pd.trueMult:F0}T");
                if (dmgTypes.Count > 0) lines.Add($"Damage: {string.Join(" ", dmgTypes)}");
                if (pd.followDistance > 0f) lines.Add($"Homing Distance: {pd.followDistance:F1}");
                if (pd.maxBoomerangDist > 0f) lines.Add($"Boomerang Distance: {pd.maxBoomerangDist:F1}");
                if (pd.orbitSelf) lines.Add($"Orbits Owner at a radius of {pd.orbitRadius:F1}-{pd.orbitRadius + pd.randOrbRadOffset:F1}");
                if (pd.kbForce > 0f) lines.Add($"Knockback: {pd.kbForce:F1} for {pd.knockbackTime:F2}s");
            }
        }
    }
}
