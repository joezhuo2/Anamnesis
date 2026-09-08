using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    public enum ApplyCondition { OnHit, OnCast }
    public enum SpecialScalingAttribute { None, Orbits, HpConsumed }
    public enum MovementType { Default, Wave, Spiral, FollowCursor }

    [CreateAssetMenu(fileName = "projectile_data", menuName = "Data/Projectile")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Basic")]
        [SerializeField] private AttackData mainAttack;
        [SerializeField] private float lifetime;
        [SerializeField] private float size = 1f;

        [Header("Piercing")]
        [SerializeField] private int numPierce = 1;
        [SerializeField] private bool destroyOnMaxPierce = false;

        [Header("Movement")]
        [Tooltip("Overrides the straight-line movement along the direction the spawner's pattern assigns. Default = follow that pattern direction. A non-Default type authored on the projectile prefab also survives an attack swapping in its own ProjectileData")]
        [SerializeField] private MovementType movementType;
        [SerializeField] private float speed;
        [Tooltip("Wave only: peak sideways offset from the straight-line path, in world units")]
        [SerializeField] private float waveAmplitude = 1f;
        [Tooltip("Wave only: full sine cycles per second")]
        [SerializeField] private float waveFrequency = 1f;
        [Tooltip("Spiral only: world-unit gap between consecutive rings of the spiral")]
        [SerializeField] private float spiralSpacing = 1f;

        [Header("Damage Multipliers")]
        [SerializeField] private float physicalMult;
        [SerializeField] private float spellMult;
        [SerializeField] private float trueMult;
        [SerializeField] private StatType scalingStat = StatType.EffAtk;
        [SerializeField] private float specialMult = 1f;
        [SerializeField] private SpecialScalingAttribute specialSclaing = SpecialScalingAttribute.None;

        [Header("Advanced")]
        [Tooltip("Time before the projectile can hit the same enemy")]
        [SerializeField] private float timeBeforeSameEnemy;
        [Tooltip("Maximum distance between the projectile and the enemy for it to follow the enemy")]
        [SerializeField] private float followDistance;
        [Tooltip("If true, overrides projectile speed and makes the projectile mimic the source's (player or enemy) movement each frame")]
        [SerializeField] private bool followSource;
        [SerializeField] private float rotationOffset;
        [Tooltip("If > 0, projectile reverses direction after traveling this distance (boomerang effect)")]
        [SerializeField] private float maxBoomerangDist = 0f;

        [Header("Angle Overrides")]
        [SerializeField] private float angleOverride;
        [SerializeField] private bool useTrueAngle;
        [SerializeField] private bool bypassIFrames;
        [Tooltip("If true, the projectile travels in a random direction instead of its spawn/aim direction")]
        [SerializeField] private bool randomDir;

        [Header("Additional Attacks")]
        [SerializeField] private AttackData additionalAttack;
        [Range(0, 1)] [SerializeField] private float additionalChance = 0;
        [SerializeField] private bool addAttackRequiresHit = true;
        [SerializeField] private bool additionalFollowsMouse = false;
        [Tooltip("Distance from location where projectile splits (must be positive to work)")] [SerializeField] private float? distFromCenter = 0f;

        [Header("Effects")]
        [SerializeField] private List<EffectData> effects = new();

        [Header("Orbit")]
        [SerializeField] private float orbitRadius;
        [Tooltip("Random max additional orbit radius")]
        [SerializeField] private float randOrbRadOffset;
        [Tooltip("If true, orbit the owner, otherwise orbit first target")]
        [SerializeField] private bool orbitSelf;
        [Tooltip("Whether the orbiting projectile rotates clockwise")]
        [SerializeField] private bool rotateClockwise;

        [Header("Knockback")]
        [SerializeField] private float kbForce = 0f;
        [Tooltip("How long the enemy is locked out of its own movement while being knocked back")]
        [SerializeField] private float knockbackTime = 0.15f;
        public AttackData MainAttack => mainAttack;
        public float Lifetime => lifetime;
        public float Size => size;
        public int NumPierce => numPierce;
        public bool DestroyOnMaxPierce => destroyOnMaxPierce;
        public MovementType MovementType => movementType;
        public float Speed => speed;
        public float WaveAmplitude => waveAmplitude;
        public float WaveFrequency => waveFrequency;
        public float SpiralSpacing => spiralSpacing;
        public float PhysicalMult => physicalMult;
        public float SpellMult => spellMult;
        public float TrueMult => trueMult;
        public StatType ScalingStat => scalingStat;
        public float SpecialMult => specialMult;
        public SpecialScalingAttribute SpecialSclaing => specialSclaing;
        public float TimeBeforeSameEnemy => timeBeforeSameEnemy;
        public float FollowDistance => followDistance;
        public bool FollowSource => followSource;
        public float RotationOffset => rotationOffset;
        public float MaxBoomerangDist => maxBoomerangDist;
        public float AngleOverride => angleOverride;
        public bool UseTrueAngle => useTrueAngle;
        public bool BypassIFrames => bypassIFrames;
        public bool RandomDir => randomDir;
        public AttackData AdditionalAttack => additionalAttack;
        public bool AddAttackRequiresHit => addAttackRequiresHit;
        public bool AdditionalFollowsMouse => additionalFollowsMouse;
        public float AdditionalChance => additionalChance;
        public float? DistFromCenter => distFromCenter;
        public IReadOnlyList<EffectData> Effects => effects;
        public float OrbitRadius => orbitRadius;
        public float RandOrbRadOffset => randOrbRadOffset;
        public bool OrbitSelf => orbitSelf;
        public bool RotateClockwise => rotateClockwise;
        public float KbForce => kbForce;
        public float KnockbackTime => knockbackTime;

    }

    [System.Serializable]
    public struct EffectData
    {
        public EffectAsset effect;
        public bool selfApply;
        public ApplyCondition applyCondition;
        [Range(0, 1)] public float chance;
    }
}
