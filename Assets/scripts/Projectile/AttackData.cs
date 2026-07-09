using UnityEngine;

public enum SummonCondition { None, OnHit, OnCast }

[CreateAssetMenu(fileName = "ad", menuName = "Data/Attack")]
public class AttackData : ScriptableObject
{
    [Header("Basic")]
    public float cooldown;
    public GameObject projectilePrefab;
    public ProjectileData pd;
    public ProjectilePattern pattern;
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
    public bool absorbOrbits;
    [Tooltip("Redirects all currently orbiting projectiles toward the nearest enemy.")]
    public bool redirectOrbits;
    [Tooltip("Causes all currently orbiting projectiles to explode, triggering their additionalAttack at their current position.")]
    public bool explodeOrbits;

    [Header("Misc - Player Only")]
    public AttackType type;
    public Sprite icon;
    public string displayName;

    public void InitializeRuntimeCopy()
    {
        if (pd != null)
        {
            pd = Instantiate(pd);
            pd.mainAttack = this;
        }
        if (nextAttack != null)
        {
            nextAttack = Instantiate(nextAttack);
            nextAttack.InitializeRuntimeCopy();
        }
    }

    private void OnDestroy()
    {
        if (pd != null) Destroy(pd);
        if (nextAttack != null) Destroy(nextAttack);
    }
}