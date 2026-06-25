using UnityEngine;
using UnityEngine.UI;

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
    [Range(0f, 100f)] [Tooltip("Minimum Hp % for enemy to use this attack")] public float minHpPct = 0f;
    [Range(0f, 100f)] [Tooltip("Maximum Hp % for enemy to use this attack")] public float maxHpPct = 100f;
    [Tooltip("Phase required to use this attack, -1 => no phase required")] public int phaseReq = -1;
    public AttackData nextAttack;

    [Header("Resource Costs (Player Only)")]
    public float staminaCost;
    public float staminaCostPct;
    public float healthCost;
    public float healthCostPct;
    public float manaCost;
    public float manaCostPct;

    [Header("Resource gains on hit (Player Only)")]
    public float staminaGainOnHit;
    public float staminaPctGainOnHit;
    public float healthGainOnHit;
    public float healthPctGainOnHit;
    public float manaGainOnHit;
    public float manaPctGainOnHit;

    [Header("Misc - Player Only")]
    public AttackType type;
    public Sprite icon;
}