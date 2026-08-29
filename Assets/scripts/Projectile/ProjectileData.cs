using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    public enum ApplyCondition { OnHit, OnCast }
    public enum SpecialScalingAttribute { None, Orbits, HpConsumed }

    [CreateAssetMenu(fileName = "projectile_data", menuName = "Data/Projectile")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Basic")]
        public AttackData mainAttack;
        public float speed;
        public float lifetime;
        public int numPierce = 1;
        public float size = 1f;

        [Header("Damage Multipliers")]
        public float physicalMult;
        public float spellMult;
        public float trueMult;
        public StatType scalingStat = StatType.EffAtk;
        public float specialMult = 1f;
        public SpecialScalingAttribute specialSclaing = SpecialScalingAttribute.None;

        [Header("Advanced")]
        [Tooltip("Time before the projectile can hit the same enemy")]
        public float timeBeforeSameEnemy;
        [Tooltip("Maximum distance between the projectile and the enemy for it to follow the enemy")]
        public float followDistance;
        [Tooltip("If true, overrides projectile speed and makes the projectile mimic the source's (player or enemy) movement each frame")]
        public bool followSource;
        public float rotationOffset;
        [Tooltip("If > 0, projectile reverses direction after traveling this distance (boomerang effect)")]
        public float maxBoomerangDist = 0f;
        public float angleOverride;
        public bool useTrueAngle;
        public bool bypassIFrames;
        public bool randomDir;

        [Header("Additional Attacks")]
        public AttackData additionalAttack;
        [Range(0, 1)] public float additionalChance = 0;
        public bool addAttackRequiresHit = true;
        public bool additionalFollowsMouse = false;
        [Tooltip("Distance from location where projectile splits (must be positive to work)")] public float? distFromCenter = 0f;

        [Header("Effects")]
        public List<EffectData> effects;

        [Header("Orbit")]
        public float orbitRadius;
        [Tooltip("Random max additional orbit radius")]
        public float randOrbRadOffset;
        [Tooltip("If true, orbit the owner, otherwise orbit first target")]
        public bool orbitSelf;
        [Tooltip("Whether the orbiting projectile rotates clockwise")]
        public bool rotateClockwise;

        [Header("Knockback")]
        public float kbForce = 0f;
        [Tooltip("How long the enemy is locked out of its own movement while being knocked back")]
        public float knockbackTime = 0.15f;
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
