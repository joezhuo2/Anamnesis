using System.Collections;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour 
{
    public static ProjectileSpawner Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject SpawnProjectile(
        GameObject prefab,
        Vector2 spawnPos,
        Vector2 dir,
        bool rotateToDir,
        EntityStats source = null
    )
    {
        if (prefab == null) return null;
        GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (rotateToDir) proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (p != null)
        {
            p.dir = dir;
            p.pierced = 0;
            if (p.pd != null) p.pd.owner = source;
        }

        return proj;
    }

    public IEnumerator SpawnCircle(GameObject prefab, Vector2 center, float radius, EntityStats source = null)
    {
        ProjectileData pd = prefab.GetComponent<Projectile>().pd;
        AttackData ad = pd.mainAttack;

        int finalCount = ad.projectileCount + Random.Range(0, ad.randomCount);

        for (int i = 0; i < finalCount; i++)
        {
            float angle = i * (360f / finalCount);

            Vector2 dir = new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 spawnPos = center + (dir * radius);

            SpawnProjectile(prefab, spawnPos, dir, true, source);
            yield return new WaitForSeconds(Random.Range(ad.minDelay, ad.maxDelay));
        }
    }

    public IEnumerator SpawnSpread(GameObject prefab, Vector2 origin, Vector2 dir, float radius, EntityStats source = null)
    {
        ProjectileData pd = prefab.GetComponent<Projectile>().pd;
        AttackData ad = pd.mainAttack;

        int finalCount = ad.projectileCount + Random.Range(0, ad.randomCount);

        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - (ad.spread * (finalCount - 1) / 2f);

        for (int i = 0; i < finalCount; i++)
        {
            float angle = startAngle + (i * ad.spread);

            if (ad.randomSpread > 0f) angle += Random.Range(-ad.randomSpread / 2f, ad.randomSpread / 2f);

            Vector2 targetDir = Quaternion.Euler(0, 0, angle - baseAngle) * dir.normalized;
            Vector2 spawnPos = origin + (targetDir * radius);

            SpawnProjectile(prefab, spawnPos, targetDir, true, source);

            yield return new WaitForSeconds(Random.Range(ad.minDelay, ad.maxDelay));
        }
    }

    public IEnumerator SpawnBarrage(GameObject prefab, Vector2 origin, Vector2 dir, EntityStats source)
    {
        ProjectileData pd = prefab.GetComponent<Projectile>().pd;
        AttackData ad = pd.mainAttack;

        int finalCount = ad.projectileCount + Random.Range(0, ad.randomCount);

        for (int i = 0; i < finalCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * ad.spread;
            Vector2 spawnPos = origin + randomOffset;

            SpawnProjectile(prefab, spawnPos, dir, true, source);
            yield return new WaitForSeconds(Random.Range(ad.minDelay, ad.maxDelay));
        }
    }
}
