using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/Decoy")]
public class Decoy : PlayerUpgrade
{
    public float lifetime = 3f;
    public Vector3 spawnOffset = new(2f, 0f, 0f);
    public Color tint = new(1f, 1f, 1f, 0.75f);

    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player == null) return;

        GameObject decoy = new("Decoy") { tag = "Player" };
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

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var e in enemies)
        {
            if (!e.TryGetComponent<EntityStatManager>(out var esm)) return;
            var es = esm.s as EnemyStats;

            float maxDist = es.detectionRange;
            float dist = Vector2.Distance(decoy.transform.position, e.transform.position);

            if (dist < maxDist && e.TryGetComponent<EnemyMovement>(out var em)) em.SetTarget(decoy);
        }

        if (lifetime > 0f)
            Object.Destroy(decoy, lifetime);
    }
}