using UnityEngine;

[CreateAssetMenu(fileName = "attack_data", menuName = "Scriptable Objects/data/attack")]
public class AttackData : ScriptableObject
{
    [Header("Basic")]
    public float cooldown;
    public GameObject projectilePrefab;

    [Header("Spawn Logic")]
    public int projectileCount = 1;
    public int randomCount; // max number of additional projectiles to spawn
    public float spread;
    public float randomSpread; // max increase/decrease to spread
    public float minDelay; // min delay between each projectile
    public float maxDelay; // max delay between each projectile
    
    [Header("Player Only")]
    public float staminaCost;
    public float staminaCostPct;
    public float healthCost;
    public float healthCostPct;
}