using System.Collections;
using CrystalFlux.Core;
using UnityEngine;
namespace CrystalFlux.ProjectileSystem
{
    public enum ProjectilePattern { Single, Spread, Circle, Barrage, SpreadBarrage, TopDown, LeftRight, Diagonal, DiagonalReverse, FullX }

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
            GameObject sourceObj,
            ProjectileData pdOverride
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
                p.ownerObj = sourceObj;
                if (pdOverride != null) p.pd = pdOverride;
            }

            return proj;
        }

        public IEnumerator SpawnCircle(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 center, float radius, GameObject sourceObj = null)
        {
            int finalCount = ad.projectileCount + Random.Range(0, ad.randomCount + 1);
            float startAngle = ad.spread + Random.Range(-ad.randomSpread / 2f, ad.randomSpread / 2f);

            for (int i = 0; i < finalCount; i++)
            {
                float angle = startAngle + (i * (360f / finalCount));

                Vector2 dir = new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 spawnPos = center + (dir * radius);

                SpawnProjectile(prefab, spawnPos, dir, true, sourceObj, pd);
                yield return new WaitForSeconds(Random.Range(ad.minDelay, ad.maxDelay));
            }
        }

        public IEnumerator SpawnSpread(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, Vector2 dir, float dist, GameObject sourceObj = null)
        {
            int finalCount = ad.projectileCount + Random.Range(0, ad.randomCount + 1);

            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - (ad.spread * (finalCount - 1) / 2f);

            for (int i = 0; i < finalCount; i++)
            {
                float angle = startAngle + (i * ad.spread);

                if (ad.randomSpread > 0f) angle += Random.Range(-ad.randomSpread / 2f, ad.randomSpread / 2f);

                Vector2 targetDir = Quaternion.Euler(0, 0, angle - baseAngle) * dir.normalized;
                Vector2 spawnPos = origin + (targetDir * dist);

                SpawnProjectile(prefab, spawnPos, targetDir, true, sourceObj, pd);

                yield return new WaitForSeconds(Random.Range(ad.minDelay, ad.maxDelay));
            }
        }

        public IEnumerator SpawnSpreadBarrage(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, Vector2 dir, float dist, GameObject sourceObj = null)
        {
            int finalCount = ad.projectileCount + Random.Range(0, ad.randomCount + 1);
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float start = baseAngle - (ad.spread / 2f);
            float end = baseAngle + (ad.spread / 2f);

            for (int i = 0; i < finalCount; i++)
            {
                float angle = Random.Range(start, end);
                if (ad.randomSpread > 0f) angle += Random.Range(-ad.randomSpread / 2f, ad.randomSpread / 2f);

                Vector2 targetDir = Quaternion.Euler(0, 0, angle - baseAngle) * dir.normalized;
                Vector2 spawnPos = origin + (targetDir * dist);

                SpawnProjectile(prefab, spawnPos, targetDir, true, sourceObj, pd);

                yield return new WaitForSeconds(Random.Range(ad.minDelay, ad.maxDelay));
            }
        }

        public IEnumerator SpawnBarrage(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, Vector2 dir, GameObject sourceObj)
        {
            int finalCount = ad.projectileCount + Random.Range(0, ad.randomCount + 1);

            for (int i = 0; i < finalCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * ad.spread;
                Vector2 spawnPos = origin + randomOffset;

                SpawnProjectile(prefab, spawnPos, dir, true, sourceObj, pd);
                yield return new WaitForSeconds(Random.Range(ad.minDelay, ad.maxDelay));
            }
        }

        public IEnumerator SpawnTopDown(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, Vector2.down);

        public IEnumerator SpawnLeftRight(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, Vector2.right);

        public IEnumerator SpawnDiagonal(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, new Vector2(1f, 1f));

        public IEnumerator SpawnDiagonalReverse(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, new Vector2(1f, -1f));

        public IEnumerator SpawnFullX(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, new Vector2(1f, 1f), new Vector2(1f, -1f));

        private IEnumerator SpawnOpposingLines(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj, params Vector2[] travelDirs)
        {
            if (travelDirs == null || travelDirs.Length == 0) yield break;

            int perSide = Mathf.Max(1, ad.projectileCount + Random.Range(0, ad.randomCount + 1));
            float halfExtent = ad.spread / 2f;
            int lineCount = travelDirs.Length * 2;

            for (int i = 0; i < perSide * lineCount; i++)
            {
                int slot = i / lineCount;
                int line = i % lineCount;

                Vector2 travel = travelDirs[line / 2].normalized;
                float sideSign = line % 2 == 0 ? 1f : -1f;

                float offset = (perSide == 1 ? 0f : (slot / (float)(perSide - 1)) - 0.5f) * ad.spread;
                if (ad.randomSpread > 0f) offset += Random.Range(-ad.randomSpread / 2f, ad.randomSpread / 2f);

                Vector2 dir = travel * sideSign;
                Vector2 spawnPos = origin + (Vector2.Perpendicular(travel) * offset) - (dir * halfExtent);

                SpawnProjectile(prefab, spawnPos, dir, true, sourceObj, pd);
                yield return new WaitForSeconds(Random.Range(ad.minDelay, ad.maxDelay));
            }
        }

        public IEnumerator SpawnFromPattern(
            AttackData ad,
            GameObject source,
            Vector2? center = null,
            Vector2? dirOverride = null,
            float? distOverride = null
        )
        {
            if (ad == null || ad.projectilePrefab == null) yield break;
            yield return SpawnFromPatternInternal(ad.projectilePrefab, ad.pd, ad, source, center, dirOverride, distOverride);
        }

        public IEnumerator SpawnFromPattern(
            GameObject prefab,
            GameObject source,
            Vector2? center = null,
            Vector2? dirOverride = null,
            float? distOverride = null
        )
        {
            if (prefab == null) yield break;
            Projectile p = prefab.GetComponent<Projectile>();
            ProjectileData pd = p != null ? p.pd : null;
            AttackData ad = pd != null ? pd.mainAttack : null;
            yield return SpawnFromPatternInternal(prefab, pd, ad, source, center, dirOverride, distOverride);
        }

        private IEnumerator SpawnFromPatternInternal(
            GameObject prefab,
            ProjectileData pd,
            AttackData ad,
            GameObject source,
            Vector2? center = null,
            Vector2? dirOverride = null,
            float? distOverride = null
        )
        {
            Vector2 mouse = Camera.main.ScreenToWorldPoint(InputState.mousePos);

            Vector2 spawnCenter = center ?? (Vector2)source.transform.position;
            Vector2 dir = dirOverride ?? (source.TryGetComponent<ITeamMember>(out var itm) && itm.TeamID == 1 ? (mouse - spawnCenter).normalized : Vector2.right);
            float finalDist = distOverride ?? (ad != null ? ad.spawnDistance : 0f);

            if (ad != null && !ad.fixedDistance && source.TryGetComponent<ITeamMember>(out var itm2) && itm2.TeamID == 1)
            {
                float mouseDist = Vector2.Distance(spawnCenter, mouse);
                finalDist = Mathf.Min(mouseDist, finalDist);
            }

            Vector2 spawnPos = spawnCenter + (dir * finalDist);

            if (ad != null && ad.spawnDelay > 0)
                yield return new WaitForSeconds(ad.spawnDelay);

            if (ad == null)
            {
                SpawnProjectile(prefab, spawnPos, dir, true, source, pd);
                yield break;
            }

            switch (ad.pattern)
            {
                case ProjectilePattern.Single:
                    SpawnProjectile(prefab, spawnPos, dir, true, source, pd);
                    break;
                case ProjectilePattern.Spread:
                    yield return SpawnSpread(prefab, pd, ad, spawnPos, dir, finalDist, source);
                    break;
                case ProjectilePattern.Circle:
                    yield return SpawnCircle(prefab, pd, ad, spawnPos, finalDist, source);
                    break;
                case ProjectilePattern.Barrage:
                    yield return SpawnBarrage(prefab, pd, ad, spawnPos, dir, source);
                    break;
                case ProjectilePattern.SpreadBarrage:
                    yield return SpawnSpreadBarrage(prefab, pd, ad, spawnPos, dir, finalDist, source);
                    break;
                case ProjectilePattern.TopDown:
                    yield return SpawnTopDown(prefab, pd, ad, spawnPos, source);
                    break;
                case ProjectilePattern.LeftRight:
                    yield return SpawnLeftRight(prefab, pd, ad, spawnPos, source);
                    break;
                case ProjectilePattern.Diagonal:
                    yield return SpawnDiagonal(prefab, pd, ad, spawnPos, source);
                    break;
                case ProjectilePattern.DiagonalReverse:
                    yield return SpawnDiagonalReverse(prefab, pd, ad, spawnPos, source);
                    break;
                case ProjectilePattern.FullX:
                    yield return SpawnFullX(prefab, pd, ad, spawnPos, source);
                    break;
                default: break;
            }
        }
    }
}
