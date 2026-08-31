using System.Collections;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/Decoy")]
public class Decoy : PlayerUpgrade
{
    public float lifetime = 3f;
    public Vector3 spawnOffset = new(2f, 0f, 0f);
    public Color tint = new(1f, 1f, 1f, 0.75f);
    public StatusEffect cooldownEffect;
    public GameObject projectilePrefab;

    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player == null) return;

        GameObject decoy = new("Decoy");
        decoy.transform.SetPositionAndRotation(player.transform.position + spawnOffset, player.transform.rotation);
        decoy.transform.localScale = player.transform.localScale;

        SpriteRenderer sourceRenderer = player.GetComponentInChildren<SpriteRenderer>(true);
        SpriteRenderer decoyRenderer = decoy.AddComponent<SpriteRenderer>();

        if (sourceRenderer != null)
        {
            decoyRenderer.sprite = sourceRenderer.sprite;
            decoyRenderer.material = sourceRenderer.material;
            decoyRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            decoyRenderer.sortingOrder = sourceRenderer.sortingOrder;
            decoyRenderer.flipX = sourceRenderer.flipX;
            decoyRenderer.flipY = sourceRenderer.flipY;
            decoyRenderer.drawMode = sourceRenderer.drawMode;
            decoyRenderer.size = sourceRenderer.size;
        }

        decoyRenderer.color = tint;

        var enemies = EnemyMovement.Active;

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyMovement em = enemies[i];
            if (em == null || !em.TryGetComponent<IStatProvider>(out var esm)) continue;

            float maxDist = esm.GetStat(StatType.DetectionRange);
            float dist = Vector2.Distance(decoy.transform.position, em.transform.position);

            if (dist < maxDist) em.SetTarget(decoy);
        }

        if (cooldownEffect != null && player.TryGetComponent<IStatusEffectReceiver>(out var sem))
            sem.Apply(cooldownEffect, player);

        if (lifetime <= 0f) return;

        ProjectileSpawner ps = ProjectileSpawner.Instance;

        if (ps != null) ps.StartCoroutine(DecoyLifetimeRoutine(decoy, player, lifetime));
        else Destroy(decoy, lifetime);
    }

    private IEnumerator DecoyLifetimeRoutine(GameObject decoy, GameObject player, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (decoy == null) yield break;

        Vector2 deathPos = decoy.transform.position;
        Destroy(decoy);

        ProjectileSpawner ps = ProjectileSpawner.Instance;

        if (projectilePrefab != null && ps != null && player != null)
            yield return ps.SpawnFromPattern(projectilePrefab, player, deathPos);
    }
}
