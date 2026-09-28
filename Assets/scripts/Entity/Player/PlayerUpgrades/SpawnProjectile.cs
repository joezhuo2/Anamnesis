using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/SpawnProjectile")]
public class SpawnProjectile : PlayerUpgrade
{
    public GameObject projectilePrefab;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        var ps = ProjectileSpawner.Instance;
        if (projectilePrefab != null && ps != null)
            ps.Spawn(projectilePrefab, player);
    }
    public override void TriggerUpgradeEffect(GameObject player, Vector2? spawnCenter)
    {
        var ps = ProjectileSpawner.Instance;
        if (projectilePrefab != null && ps != null && spawnCenter.HasValue)
            ps.Spawn(projectilePrefab, player, spawnCenter.Value);
    }
}
