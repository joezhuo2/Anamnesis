using UnityEngine;

[CreateAssetMenu(fileName = "ad", menuName = "Data/Attack")]
public class AttackData : ScriptableObject
{
    [Header("Basic")]
    public float cooldown;
    public GameObject projectilePrefab;
    public ProjectilePattern pattern;
    public float spawnDelay;
    public float spawnDistance;
    public bool fixedDistance;
    public float animationLength;
    public AttackType type;
    [Header("Spawn Logic")]
    public int projectileCount = 1;
    [Tooltip("Additional projectiles to spawn")] public int randomCount;
    [Tooltip("Spread radius for barrage attacks")] public float spread;
    [Tooltip("Maximum increase/decrease to spread")] public float randomSpread;
    [Tooltip("Minimum delay between each projectile")] public float minDelay;
    [Tooltip("Maximum delay between each projectile")] public float maxDelay;
    [Header("Enemy Only")]
    public float maxRange;
    public bool canMoveDuringAttack;
    [Header("Resource Costs (Player Only)")]
    public float staminaCost;
    public float staminaCostPct;
    public float healthCost;
    public float healthCostPct;
}