using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;
namespace CrystalFlux.ProjectileSystem
{
    public enum ProjectilePattern { Single, Spread, Circle, Barrage, SpreadBarrage, TopDown, LeftRight, Diagonal, DiagonalReverse, FullX, LeftRightTopDown, CircleInverse }

    public class ProjectileSpawner : MonoBehaviour
    {
        public static ProjectileSpawner Instance;

        private const int ProjectilePoolCap = 256;

        public static event System.Action<GameObject, GameObject, Vector2> ProjectileSpawned;
        public static event System.Action<GameObject, Vector2> PreTeleport;
        public static event System.Action<GameObject, Vector2> Teleported;

        private static readonly Dictionary<float, WaitForSeconds> waitCache = new();
        private readonly HashSet<GameObject> cappedPrefabs = new();

        private static Camera cachedMainCam;
        private static Camera MainCam => cachedMainCam != null ? cachedMainCam : cachedMainCam = Camera.main;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public GameObject SpawnProjectile(
            GameObject prefab,
            Vector2 spawnPos,
            Vector2 dir,
            bool rotateToDir,
            GameObject sourceObj,
            ProjectileData pdOverride,
            AttackData chainRoot = null
        )
        {
            if (prefab == null) return null;

            if (cappedPrefabs.Add(prefab)) PrefabPool.SetCap(prefab, ProjectilePoolCap);

            GameObject proj = PrefabPool.Acquire(prefab, null);
            if (proj == null) return null;

            proj.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (rotateToDir) proj.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (proj.TryGetComponent<Projectile>(out var p)) p.Setup(dir, sourceObj, pdOverride, chainRoot);
            else if (proj.TryGetComponent<Rigidbody2D>(out var rb)) rb.gravityScale = 0f;

            ProjectileSpawned?.Invoke(sourceObj, proj, spawnPos);

            return proj;
        }

        private static WaitForSeconds Wait(float t)
        {
            if (!waitCache.TryGetValue(t, out var w))
            {
                w = new WaitForSeconds(t);
                waitCache[t] = w;
            }
            return w;
        }

        private bool TeleportOnce(bool pending, AttackData ad, GameObject src, GameObject proj)
        {
            if (!pending || proj == null) return pending;
            StartCoroutine(TeleportToProjectile(src, proj, ad.TeleportDelay));
            return false;
        }

        public IEnumerator SpawnCircle(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 center, float radius, GameObject sourceObj = null, AttackData chainRoot = null, bool teleport = false, bool inverse = false)
        {
            int finalCount = Mathf.Max(1, ad.ProjectileCount + Random.Range(0, ad.RandomCount + 1));
            float startAngle = ad.Spread + Random.Range(-ad.RandomSpread / 2f, ad.RandomSpread / 2f);

            float wait = 0f;
            for (int i = 0; i < finalCount; i++)
            {
                float angle = startAngle + (i * (360f / finalCount));

                Vector2 dir = new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 spawnPos = center + (dir * radius);

                teleport = TeleportOnce(teleport, ad, sourceObj, SpawnProjectile(prefab, spawnPos, inverse ? -dir : dir, true, sourceObj, pd, chainRoot));
                wait += Random.Range(ad.MinDelay, ad.MaxDelay);
                while (wait > 0f) { yield return null; wait -= Time.deltaTime; }
            }
        }

        public IEnumerator SpawnSpread(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, Vector2 dir, float dist, GameObject sourceObj = null, AttackData chainRoot = null, bool teleport = false)
        {
            int finalCount = ad.ProjectileCount + Random.Range(0, ad.RandomCount + 1);

            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - (ad.Spread * (finalCount - 1) / 2f);

            float wait = 0f;
            for (int i = 0; i < finalCount; i++)
            {
                float angle = startAngle + (i * ad.Spread);

                if (ad.RandomSpread > 0f) angle += Random.Range(-ad.RandomSpread / 2f, ad.RandomSpread / 2f);

                Vector2 targetDir = Quaternion.Euler(0, 0, angle - baseAngle) * dir.normalized;
                Vector2 spawnPos = origin + (targetDir * dist);

                teleport = TeleportOnce(teleport, ad, sourceObj, SpawnProjectile(prefab, spawnPos, targetDir, true, sourceObj, pd, chainRoot));

                wait += Random.Range(ad.MinDelay, ad.MaxDelay);
                while (wait > 0f) { yield return null; wait -= Time.deltaTime; }
            }
        }

        public IEnumerator SpawnSpreadBarrage(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, Vector2 dir, float dist, GameObject sourceObj = null, AttackData chainRoot = null, bool teleport = false)
        {
            int finalCount = ad.ProjectileCount + Random.Range(0, ad.RandomCount + 1);
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float start = baseAngle - (ad.Spread / 2f);
            float end = baseAngle + (ad.Spread / 2f);

            float wait = 0f;
            for (int i = 0; i < finalCount; i++)
            {
                float angle = Random.Range(start, end);
                if (ad.RandomSpread > 0f) angle += Random.Range(-ad.RandomSpread / 2f, ad.RandomSpread / 2f);

                Vector2 targetDir = Quaternion.Euler(0, 0, angle - baseAngle) * dir.normalized;
                Vector2 spawnPos = origin + (targetDir * dist);

                teleport = TeleportOnce(teleport, ad, sourceObj, SpawnProjectile(prefab, spawnPos, targetDir, true, sourceObj, pd, chainRoot));

                wait += Random.Range(ad.MinDelay, ad.MaxDelay);
                while (wait > 0f) { yield return null; wait -= Time.deltaTime; }
            }
        }

        public IEnumerator SpawnBarrage(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, Vector2 dir, GameObject sourceObj, AttackData chainRoot = null, bool teleport = false)
        {
            int finalCount = ad.ProjectileCount + Random.Range(0, ad.RandomCount + 1);

            float wait = 0f;
            for (int i = 0; i < finalCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * ad.Spread;
                Vector2 spawnPos = origin + randomOffset;

                teleport = TeleportOnce(teleport, ad, sourceObj, SpawnProjectile(prefab, spawnPos, dir, true, sourceObj, pd, chainRoot));
                wait += Random.Range(ad.MinDelay, ad.MaxDelay);
                while (wait > 0f) { yield return null; wait -= Time.deltaTime; }
            }
        }

        public IEnumerator SpawnTopDown(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj, AttackData chainRoot = null, bool teleport = false)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, chainRoot, teleport, Vector2.down, default, 1);

        public IEnumerator SpawnLeftRight(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj, AttackData chainRoot = null, bool teleport = false)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, chainRoot, teleport, Vector2.right, default, 1);

        public IEnumerator SpawnDiagonal(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj, AttackData chainRoot = null, bool teleport = false)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, chainRoot, teleport, new Vector2(1f, 1f), default, 1);

        public IEnumerator SpawnDiagonalReverse(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj, AttackData chainRoot = null, bool teleport = false)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, chainRoot, teleport, new Vector2(1f, -1f), default, 1);

        public IEnumerator SpawnFullX(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj, AttackData chainRoot = null, bool teleport = false)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, chainRoot, teleport, new Vector2(1f, 1f), new Vector2(1f, -1f), 2);

        public IEnumerator SpawnLeftRightTopDown(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj, AttackData chainRoot = null, bool teleport = false)
            => SpawnOpposingLines(prefab, pd, ad, origin, sourceObj, chainRoot, teleport, Vector2.right, Vector2.down, 2);

        private IEnumerator SpawnOpposingLines(GameObject prefab, ProjectileData pd, AttackData ad, Vector2 origin, GameObject sourceObj, AttackData chainRoot, bool teleport, Vector2 d0, Vector2 d1, int dirCount)
        {
            int perSide = Mathf.Max(1, ad.ProjectileCount + Random.Range(0, ad.RandomCount + 1));
            float halfExtent = ad.Spread / 2f;
            int lineCount = dirCount * 2;

            float wait = 0f;
            for (int i = 0; i < perSide * lineCount; i++)
            {
                int slot = i / lineCount;
                int line = i % lineCount;

                Vector2 travel = (line / 2 == 0 ? d0 : d1).normalized;
                float sideSign = line % 2 == 0 ? 1f : -1f;

                float offset = (perSide == 1 ? 0f : (slot / (float)(perSide - 1)) - 0.5f) * ad.Spread;
                if (ad.RandomSpread > 0f) offset += Random.Range(-ad.RandomSpread / 2f, ad.RandomSpread / 2f);

                Vector2 dir = travel * sideSign;
                Vector2 spawnPos = origin + (Vector2.Perpendicular(travel) * offset) - (dir * halfExtent);

                teleport = TeleportOnce(teleport, ad, sourceObj, SpawnProjectile(prefab, spawnPos, dir, true, sourceObj, pd, chainRoot));
                wait += Random.Range(ad.MinDelay, ad.MaxDelay);
                while (wait > 0f) { yield return null; wait -= Time.deltaTime; }
            }
        }

        public void Spawn(
            AttackData ad,
            GameObject source,
            Vector2? center = null,
            Vector2? dirOverride = null,
            float? distOverride = null,
            AttackData chainRoot = null,
            bool fixedAim = false,
            MonoBehaviour host = null,
            bool skipDelay = false
        )
        {
            if (ad == null || ad.ProjectilePrefab == null) return;
            SpawnInternal(ad.ProjectilePrefab, ad.Pd, ad, source, center, dirOverride, distOverride, chainRoot, fixedAim, host, skipDelay);
        }

        public void Spawn(
            GameObject prefab,
            GameObject source,
            Vector2? center = null,
            Vector2? dirOverride = null,
            float? distOverride = null,
            AttackData chainRoot = null,
            bool fixedAim = false,
            MonoBehaviour host = null,
            bool skipDelay = false
        )
        {
            if (prefab == null) return;
            ResolvePrefab(prefab, out var pd, out var ad);
            SpawnInternal(prefab, pd, ad, source, center, dirOverride, distOverride, chainRoot, fixedAim, host, skipDelay);
        }

        public IEnumerator SpawnFromPattern(
            AttackData ad,
            GameObject source,
            Vector2? center = null,
            Vector2? dirOverride = null,
            float? distOverride = null,
            AttackData chainRoot = null,
            bool fixedAim = false
        )
        {
            if (ad == null || ad.ProjectilePrefab == null) yield break;
            if (!Aim(ad, source, center, dirOverride, distOverride, fixedAim, out var pos, out var dir, out var dist)) yield break;
            yield return RunPattern(ad.ProjectilePrefab, ad.Pd, ad, source, pos, dir, dist, chainRoot);
        }

        public IEnumerator SpawnFromPattern(
            GameObject prefab,
            GameObject source,
            Vector2? center = null,
            Vector2? dirOverride = null,
            float? distOverride = null,
            AttackData chainRoot = null,
            bool fixedAim = false
        )
        {
            if (prefab == null) yield break;
            ResolvePrefab(prefab, out var pd, out var ad);
            if (!Aim(ad, source, center, dirOverride, distOverride, fixedAim, out var pos, out var dir, out var dist)) yield break;

            if (ad == null)
            {
                SpawnProjectile(prefab, pos, dir, true, source, pd, chainRoot);
                yield break;
            }
            yield return RunPattern(prefab, pd, ad, source, pos, dir, dist, chainRoot);
        }

        private static void ResolvePrefab(GameObject prefab, out ProjectileData pd, out AttackData ad)
        {
            pd = prefab.TryGetComponent<Projectile>(out var p) ? p.pd : null;
            ad = pd != null ? pd.MainAttack : null;
        }

        public static void ResolveAim(AttackData ad, GameObject source, Vector2 center, Vector2? dirOverride, float? distOverride, out Vector2 dir, out float dist)
        {
            bool aimsAtMouse = source != null && source.TryGetComponent<ITeamMember>(out var itm) && itm.TeamID == 1;

            Vector2 mouse = center;
            bool aims = aimsAtMouse;
            if (aimsAtMouse && MainCam != null) mouse = MainCam.ScreenToWorldPoint(InputState.mousePos);
            else if (!aimsAtMouse && source != null && source.TryGetComponent<IAimProvider>(out var iap))
            {
                mouse = iap.AimPoint;
                aims = true;
            }

            Vector2 toAim = mouse - center;
            dir = dirOverride ?? (aims && toAim != Vector2.zero ? toAim.normalized : Vector2.right);
            dist = distOverride ?? (ad != null ? ad.SpawnDistance : 0f);

            if (ad != null && !ad.FixedDistance && aims)
                dist = Mathf.Min(Vector2.Distance(center, mouse), dist);
        }

        private static bool Aim(
            AttackData ad,
            GameObject source,
            Vector2? center,
            Vector2? dirOverride,
            float? distOverride,
            bool fixedAim,
            out Vector2 spawnPos,
            out Vector2 dir,
            out float dist
        )
        {
            spawnPos = default;
            dir = default;
            dist = 0f;
            if (source == null && center == null) return false;

            Vector2 spawnCenter = center ?? (Vector2)source.transform.position;

            if (fixedAim && dirOverride.HasValue && distOverride.HasValue)
            {
                dir = dirOverride.Value;
                dist = distOverride.Value;
            }
            else ResolveAim(ad, source, spawnCenter, dirOverride, distOverride, out dir, out dist);

            spawnPos = spawnCenter + (dir * dist);
            return true;
        }

        private void SpawnInternal(
            GameObject prefab,
            ProjectileData pd,
            AttackData ad,
            GameObject source,
            Vector2? center,
            Vector2? dirOverride,
            float? distOverride,
            AttackData chainRoot,
            bool fixedAim,
            MonoBehaviour host,
            bool skipDelay
        )
        {
            if (!Aim(ad, source, center, dirOverride, distOverride, fixedAim, out var pos, out var dir, out var dist)) return;

            if (ad == null)
            {
                SpawnProjectile(prefab, pos, dir, true, source, pd, chainRoot);
                return;
            }

            if ((ad.SpawnDelay <= 0 || skipDelay) && ad.Pattern == ProjectilePattern.Single)
            {
                TeleportOnce(ad.TeleportToProjectile && source != null, ad, source, SpawnProjectile(prefab, pos, dir, true, source, pd, chainRoot));
                return;
            }

            MonoBehaviour h = host != null && host.isActiveAndEnabled ? host : this;
            h.StartCoroutine(RunPattern(prefab, pd, ad, source, pos, dir, dist, chainRoot, skipDelay));
        }

        private IEnumerator RunPattern(GameObject prefab, ProjectileData pd, AttackData ad, GameObject source, Vector2 spawnPos, Vector2 dir, float finalDist, AttackData chainRoot, bool skipDelay = false)
        {
            if (ad.SpawnDelay > 0 && !skipDelay) yield return Wait(ad.SpawnDelay);

            bool tp = ad.TeleportToProjectile && source != null;

            switch (ad.Pattern)
            {
                case ProjectilePattern.Single:
                    TeleportOnce(tp, ad, source, SpawnProjectile(prefab, spawnPos, dir, true, source, pd, chainRoot));
                    break;
                case ProjectilePattern.Spread:
                    yield return SpawnSpread(prefab, pd, ad, spawnPos, dir, finalDist, source, chainRoot, tp);
                    break;
                case ProjectilePattern.Circle:
                    yield return SpawnCircle(prefab, pd, ad, spawnPos, finalDist, source, chainRoot, tp);
                    break;
                case ProjectilePattern.Barrage:
                    yield return SpawnBarrage(prefab, pd, ad, spawnPos, dir, source, chainRoot, tp);
                    break;
                case ProjectilePattern.SpreadBarrage:
                    yield return SpawnSpreadBarrage(prefab, pd, ad, spawnPos, dir, finalDist, source, chainRoot, tp);
                    break;
                case ProjectilePattern.TopDown:
                    yield return SpawnTopDown(prefab, pd, ad, spawnPos, source, chainRoot, tp);
                    break;
                case ProjectilePattern.LeftRight:
                    yield return SpawnLeftRight(prefab, pd, ad, spawnPos, source, chainRoot, tp);
                    break;
                case ProjectilePattern.Diagonal:
                    yield return SpawnDiagonal(prefab, pd, ad, spawnPos, source, chainRoot, tp);
                    break;
                case ProjectilePattern.DiagonalReverse:
                    yield return SpawnDiagonalReverse(prefab, pd, ad, spawnPos, source, chainRoot, tp);
                    break;
                case ProjectilePattern.FullX:
                    yield return SpawnFullX(prefab, pd, ad, spawnPos, source, chainRoot, tp);
                    break;
                case ProjectilePattern.LeftRightTopDown:
                    yield return SpawnLeftRightTopDown(prefab, pd, ad, spawnPos, source, chainRoot, tp);
                    break;
                case ProjectilePattern.CircleInverse:
                    yield return SpawnCircle(prefab, pd, ad, spawnPos, finalDist, source, chainRoot, tp, true);
                    break;
                default: break;
            }
        }

        private IEnumerator TeleportToProjectile(GameObject src, GameObject proj, float delay)
        {
            if (delay > 0f) yield return Wait(delay);
            if (src == null || proj == null || !proj.activeInHierarchy) yield break;
            if (proj.TryGetComponent<Projectile>(out var p) && p.ownerObj != src) yield break;

            PreTeleport?.Invoke(src, src.transform.position);
            if (src == null || proj == null || !proj.activeInHierarchy) yield break;

            Vector2 pos = proj.transform.position;
            if (src.TryGetComponent<Rigidbody2D>(out var rb)) rb.position = pos;
            src.transform.position = pos;

            Teleported?.Invoke(src, pos);
        }
    }
}
