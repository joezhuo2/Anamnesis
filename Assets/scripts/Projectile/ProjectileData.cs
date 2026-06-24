using UnityEngine;

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
    public bool canHitSameEntity;
    public int followDistance;
    public float rotationOffset;
    [Header("Additional Attacks")]
    public AttackData additionalAttack;
    [Range(0, 1)] public float additionalChance = 0;
    public bool additionalFollowsMouse = false;
    public float? distFromCenter = 0f; // distance from location where projectile splits (must be positive to work)
    [Header("Effects")]
    public StatusEffect effect;
    public bool selfApply;
    [Range(0, 1)] public float effectChance = 0;
}