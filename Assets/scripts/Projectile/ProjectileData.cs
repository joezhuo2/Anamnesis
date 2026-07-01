using System.Collections.Generic;
using UnityEngine;

public enum ApplyCondition { OnHit, OnCast }

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

    [Header("Advanced")]
    public float timeBeforeSameEnemy;
    public float followDistance;
    public float rotationOffset;
    public float angleOverride;
    public bool useTrueAngle;
    public bool bypassIFrames;

    [Header("Additional Attacks")]
    public AttackData additionalAttack;
    [Range(0, 1)] public float additionalChance = 0;
    public bool addAttackRequiresHit = true;
    public bool additionalFollowsMouse = false;
    [Tooltip("Distance from location where projectile splits (must be positive to work)")] public float? distFromCenter = 0f;

    [Header("Effects")]
    public List<EffectData> effects;
}

[System.Serializable]
public struct EffectData
{
    public StatusEffect effect;
    public bool selfApply;
    public ApplyCondition applyCondition;
    [Range(0, 1)] public float chance;
}