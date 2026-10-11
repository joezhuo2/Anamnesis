using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/SpawnProjectile")]
public class SpawnProjectile : PlayerUpgrade
{
    public GameObject projectilePrefab;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player != null) SpawnAt(player, player.transform.position);
    }
    public override void TriggerUpgradeEffect(GameObject player, Vector2? spawnCenter)
    {
        if (spawnCenter.HasValue) SpawnAt(player, spawnCenter.Value);
    }
    public override void TriggerUpgradeEffect(GameObject player, GameObject target)
    {
        if (target != null) SpawnAt(player, target.transform.position);
        else TriggerUpgradeEffect(player);
    }

    // Awakening projectiles spawn where they were triggered, never toward the cursor
    private void SpawnAt(GameObject player, Vector2 center)
    {
        var ps = ProjectileSpawner.Instance;
        if (projectilePrefab != null && ps != null)
            ps.SpawnUnaimed(projectilePrefab, player, center);
    }
}
