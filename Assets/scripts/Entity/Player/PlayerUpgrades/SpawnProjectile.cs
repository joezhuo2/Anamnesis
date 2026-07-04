using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/SpawnProjectile")]
public class SpawnProjectile : PlayerUpgrade
{
    public GameObject projectilePrefab;
    public override void TriggerUpgradeEffect(GameObject player)
    {
        var ps = ProjectileSpawner.Instance;
        if (projectilePrefab != null && ps != null)
            ps.StartCoroutine(ps.SpawnFromPattern(projectilePrefab, player));
    }
    public override void TriggerUpgradeEffect(GameObject player, Vector2? spawnCenter = null)
    {
        var ps = ProjectileSpawner.Instance;
        if (projectilePrefab != null && ps != null && spawnCenter.HasValue)
            ps.StartCoroutine(ps.SpawnFromPattern(projectilePrefab, player, spawnCenter.Value));
    }
}