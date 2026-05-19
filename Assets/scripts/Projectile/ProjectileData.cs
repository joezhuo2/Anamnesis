using UnityEngine;

[CreateAssetMenu(fileName = "projectile_data", menuName = "Scriptable Objects/data/projectile")]
public class ProjectileData : ScriptableObject
{
    [Header("Basic")]
    public AttackData mainAttack;
    public float speed;
    public float lifetime;
    public int numPierce = 1;

    [Header("Damage Multipliers")]
    public float physicalMult;
    public float spellMult;

    [Header("Advanced")]
    public bool canHitSameEntity;
    public int followDistance;
    public EntityStats owner;
    [Header("Additional Attacks")]
    public AttackData additionalAttack;
    [Range(0, 1)] public float additionalChance = 0; 
}