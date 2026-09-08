using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    [CreateAssetMenu(fileName = "ad", menuName = "Data/Attack")]
    public class AttackData : AttackAsset
    {
        [Header("Basic")]
        [SerializeField] private float cooldown;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private ProjectileData pd;
        [SerializeField] private ProjectilePattern pattern;
        [Tooltip("Windup time before the attack resolves. 0 = instant")]
        [SerializeField] private float castTime;
        [Tooltip("Whether the entity can move during the cast window")]
        [SerializeField] private bool canMoveWhileCasting = true;
        [Tooltip("Time after performing the attack before projectiles spawn")]
        [SerializeField] private float spawnDelay;
        [SerializeField] private float spawnDistance;
        [Tooltip("Whether to spawn the projectile at a fixed distance according to spawn distance")]
        [SerializeField] private bool fixedDistance;
        [Tooltip("Time after attack is performed before resetting the attack animation")]
        [SerializeField] private float animationLength;

        [Header("Charging")]
        [Tooltip("Whether this attack can be held down to sustain it after it resolves")]
        [SerializeField] private bool canCharge;
        [Tooltip("How long the button must be held before this registers as a charge. Released sooner, the press resolves as a plain tap. Also the delay before a tap's projectiles spawn")]
        [SerializeField] private float chargeThreshold = 0.225f;
        [Tooltip("Once charging, the minimum charge duration. An early release is queued until this elapses. 0 = release ends the charge immediately")]
        [SerializeField] private float minChargeTime = 0f;
        [Tooltip("Maximum hold duration. The entity is forced to release at this point")]
        [SerializeField] private float maxChargeTime = 5f;
        [Tooltip("Seconds between each resource drain / lifetime refresh tick while held")]
        [SerializeField] private float chargeTickInterval = 1f;
        [Tooltip("Chargeable attacks only. True = the cooldown starts when the attack is performed, so it ticks down during the hold. False = it starts when the charge ends, which makes short charges cycle faster than long ones")]
        [SerializeField] private bool cooldownOnAttackStart = true;
        [Tooltip("Optional separate attack spawned once the hold is confirmed. Its projectiles are the ones sustained by each charge tick, and its resource costs are what the tick drains. Null = sustain this attack's own projectiles")]
        [SerializeField] private AttackData chargeAttack;

        [Header("Spawn Logic")]
        [SerializeField] private int projectileCount = 1;
        [Tooltip("Random additional projectiles to spawn")]
        [SerializeField] private int randomCount;
        [Tooltip("Spread angle for 'spread' attacks, and barrage radius for barrage attacks")]
        [SerializeField] private float spread;
        [Tooltip("Maximum random increase/decrease to spread")]
        [SerializeField] private float randomSpread;
        [Tooltip("Minimum delay between each projectile spawn if projectile count > 0")]
        [SerializeField] private float minDelay;
        [Tooltip("Maximum delay between each projectile spawn if projectile count > 0")]
        [SerializeField] private float maxDelay;

        [Header("Enemy Only")]
        [Tooltip("Maximum range for enemies to be able to use this attack")]
        [SerializeField] private float maxRange;
        [Tooltip("Whether the enemy can move while performing this attaack")]
        [SerializeField] private bool canMoveDuringAttack;
        [Range(0f, 100f)]
        [Tooltip("Minimum Hp % for enemy to use this attack")]
        [SerializeField] private float minHpPct = 0f;
        [Range(0f, 100f)]
        [Tooltip("Maximum Hp % for enemy to use this attack")]
        [SerializeField] private float maxHpPct = 100f;
        [Tooltip("Phase required to use this attack, -1 => no phase required")]
        [SerializeField] private int phaseReq = -1;
        [Tooltip("Next attack for the enemy to use")]
        [SerializeField] private AttackData nextAttack;

        [Header("Resource Costs (Player Only)")]
        [SerializeField] private float staminaCost;
        [SerializeField] private float staminaCostPct;
        [SerializeField] private float healthCost;
        [SerializeField] private float healthCostPct;
        [SerializeField] private float manaCost;
        [SerializeField] private float manaCostPct;

        [Header("Resource gains on hit (Player Only)")]
        [SerializeField] private bool basedOnDmgDealt = true;
        [SerializeField] private float staminaGainOnHit;
        [SerializeField] private float staminaPctGainOnHit;
        [SerializeField] private float healthGainOnHit;
        [SerializeField] private float healthPctGainOnHit;
        [SerializeField] private float manaGainOnHit;
        [SerializeField] private float manaPctGainOnHit;

        [Header("Summoning")]
        [Tooltip("Chance (0-1) to summon an entity on the given condition. 0 = disabled.")]
        [Range(0f, 1f)] [SerializeField] private float summonChance = 0f;
        [SerializeField] private SummonCondition summonCondition = SummonCondition.None;

        [Header("Cleanse")]
        [Tooltip("Number of active debuffs removed from the attacker when this attack is performed. 0 = disabled")]
        [SerializeField] private int cleanseDebuffs;

        [Header("Orbit Interactions")]
        [Tooltip("Fires all currently orbiting projectiles toward the mouse direction before spawning this attack.")]
        [SerializeField] private bool fireOrbits;
        [Tooltip("Absorbs all currently orbiting projectiles, granting stat returns per projectile.")]
        [SerializeField] private float absorbOrbitPct;
        [Tooltip("Redirects all currently orbiting projectiles toward the nearest enemy.")]
        [SerializeField] private bool redirectOrbits;
        [Tooltip("Causes all currently orbiting projectiles to explode, triggering their additionalAttack at their current position.")]
        [SerializeField] private bool explodeOrbits;
        [SerializeField] private int redirectCount;

        [Header("Misc - Player Only")]
        [SerializeField] private Sprite icon;
        [SerializeField] private string displayName;

        public float Cooldown => cooldown;
        public GameObject ProjectilePrefab => projectilePrefab;
        public ProjectileData Pd => pd;
        public ProjectilePattern Pattern => pattern;
        public float CastTime => castTime;
        public bool CanMoveWhileCasting => canMoveWhileCasting;
        public float SpawnDelay => spawnDelay;
        public float SpawnDistance => spawnDistance;
        public bool FixedDistance => fixedDistance;
        public float AnimationLength => animationLength;
        public bool CanCharge => canCharge;
        public float ChargeThreshold => chargeThreshold;
        public float MinChargeTime => minChargeTime;
        public float MaxChargeTime => maxChargeTime;
        public float ChargeTickInterval => chargeTickInterval;
        public bool CooldownOnAttackStart => cooldownOnAttackStart;
        public AttackData ChargeAttack => chargeAttack;
        public int ProjectileCount => projectileCount;
        public int RandomCount => randomCount;
        public float Spread => spread;
        public float RandomSpread => randomSpread;
        public float MinDelay => minDelay;
        public float MaxDelay => maxDelay;
        public float MaxRange => maxRange;
        public bool CanMoveDuringAttack => canMoveDuringAttack;
        public float MinHpPct => minHpPct;
        public float MaxHpPct => maxHpPct;
        public int PhaseReq => phaseReq;
        public AttackData NextAttack => nextAttack;
        public float StaminaCost => staminaCost;
        public float StaminaCostPct => staminaCostPct;
        public float HealthCost => healthCost;
        public float HealthCostPct => healthCostPct;
        public float ManaCost => manaCost;
        public float ManaCostPct => manaCostPct;
        public bool BasedOnDmgDealt => basedOnDmgDealt;
        public float StaminaGainOnHit => staminaGainOnHit;
        public float StaminaPctGainOnHit => staminaPctGainOnHit;
        public float HealthGainOnHit => healthGainOnHit;
        public float HealthPctGainOnHit => healthPctGainOnHit;
        public float ManaGainOnHit => manaGainOnHit;
        public float ManaPctGainOnHit => manaPctGainOnHit;
        public float SummonChance => summonChance;
        public SummonCondition SummonCondition => summonCondition;
        public int CleanseDebuffs => cleanseDebuffs;
        public bool FireOrbits => fireOrbits;
        public float AbsorbOrbitPct => absorbOrbitPct;
        public bool RedirectOrbits => redirectOrbits;
        public bool ExplodeOrbits => explodeOrbits;
        public int RedirectCount => redirectCount;
        public Sprite Icon => icon;
        public string DisplayName => displayName;

        public override bool IsRuntimeCopy => false;

        public float GetEffCastTime(IStatProvider esm)
        {
            if (castTime <= 0f) return 0f;
            if (esm == null) return castTime;
            return castTime * Mathf.Clamp(1f - (esm.GetStat(StatType.castTimeRedPct) * 0.01f), 0.1f, 1f);
        }

        public override void DeepClone() { }

        public override void GetTooltipLines(List<string> lines)
        {
            lines.Add($"Type: {type} ({pattern})");
            if (cooldown > 0f) lines.Add($"Cooldown: {cooldown:F1}s");
            if (castTime > 0f) lines.Add($"Cast Time: {castTime:F1}s{(canMoveWhileCasting ? string.Empty : " (rooted)")}");
            if (canCharge) lines.Add($"Hold {chargeThreshold:F2}s to charge (max {maxChargeTime:F1}s, drains every {chargeTickInterval:F1}s)");
            if (canCharge && chargeAttack != null) lines.Add($"Held: {chargeAttack.displayName}");

            if (staminaCost > 0f || staminaCostPct > 0f) lines.Add($"Stamina Cost: {staminaCost:F0} +{staminaCostPct:F1}%");
            if (manaCost > 0f || manaCostPct > 0f) lines.Add($"Mana Cost: {manaCost:F0} +{manaCostPct:F1}%");
            if (healthCost > 0f || healthCostPct > 0f) lines.Add($"Health Cost: {healthCost:F0} +{healthCostPct:F1}%");

            if (healthGainOnHit > 0f || healthPctGainOnHit > 0f) lines.Add($"Health Gain: {healthGainOnHit:F0} +{healthPctGainOnHit:F1}%");
            if (staminaGainOnHit > 0f || staminaPctGainOnHit > 0f) lines.Add($"Stamina Gain: {staminaGainOnHit:F0} +{staminaPctGainOnHit:F1}%");
            if (manaGainOnHit > 0f || manaPctGainOnHit > 0f) lines.Add($"Mana Gain: {manaGainOnHit:F0} +{manaPctGainOnHit:F1}%");

            if (cleanseDebuffs > 0) lines.Add($"Cleanses {cleanseDebuffs} debuff{(cleanseDebuffs == 1 ? string.Empty : "s")}");

            if (explodeOrbits) lines.Add($"Explodes all orbiting projectiles");
            if (fireOrbits) lines.Add($"Fires all orbiting projectiles");
            if (absorbOrbitPct > 0f) lines.Add($"Absorbs all orbiting projectiles ({absorbOrbitPct:F1}% stat returns)");
            if (redirectOrbits && redirectCount > 0) lines.Add($"Redirects {redirectCount} orbiting projectiles to nearest enemy");

            if (pd != null)
            {
                List<string> dmgTypes = new();
                if (pd.Speed > 0f) lines.Add($"Speed: {pd.Speed:F1}");
                if (pd.PhysicalMult > 0f) dmgTypes.Add($"{pd.PhysicalMult:F0}P");
                if (pd.SpellMult > 0f) dmgTypes.Add($"{pd.SpellMult:F0}S");
                if (pd.TrueMult > 0f) dmgTypes.Add($"{pd.TrueMult:F0}T");
                if (dmgTypes.Count > 0) lines.Add($"Damage: {string.Join(" ", dmgTypes)}");
                if (pd.FollowDistance > 0f) lines.Add($"Homing Distance: {pd.FollowDistance:F1}");
                if (pd.MaxBoomerangDist > 0f) lines.Add($"Boomerang Distance: {pd.MaxBoomerangDist:F1}");
                if (pd.OrbitSelf) lines.Add($"Orbits Owner at a radius of {pd.OrbitRadius:F1}-{pd.OrbitRadius + pd.RandOrbRadOffset:F1}");
                if (pd.KbForce > 0f) lines.Add($"Knockback: {pd.KbForce:F1} for {pd.KnockbackTime:F2}s");
            }
        }
    }
}
