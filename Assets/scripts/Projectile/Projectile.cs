using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalFlux.ProjectileSystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(RectTransform))]
    public partial class Projectile : MonoBehaviour, IPoolable
    {
        public ProjectileData pd;

        [HideInInspector] public GameObject ownerObj;
        public static bool ApplyingProjectileHit { get; private set; }
        public static event System.Action<AttackData> PlayerHit;
        [HideInInspector] public Vector2 dir;
        [HideInInspector] public int pierced;
        private float effSpd;
        private float lifeRemaining;
        private HashSet<GameObject> hit;
        private List<GameObject> hitExpiryTargets;
        private List<float> hitExpiryTimes;
        private ProjectileOwnerCache oc;
        private IReadOnlyList<IOnHitEffect> onHit;
        private ProjectileDamageSnapshot damageSnapshot;
        private Transform followTarget;
        private Transform orbitTarget;
        private float orbitDirectionSign;
        private float effectiveOrbitRadius;
        private float orbitAngleOffset;
        private bool orbitInitialized;
        private Rigidbody2D rb;
        private bool boomerangActive;
        private bool boomerangReturning;
        private float boomerangDecel;
        private float boomerangSpeed;
        private bool orbitCancelled;
        private bool canTriggerAdd;
        private Rigidbody2D sourceRb;
        private Vector2 patternOrigin;
        private float patternTime;
        private float patternBaseAngle;
        private float spiralTheta;
        private bool patternSuspended;
        private MovementType prefabMoveType;
        private float prefabWaveAmp;
        private float prefabWaveFreq;
        private float prefabSpiralSpacing;
        private ProjectileData defaultPd;
        private Vector3 defaultScale;
        private AttackData chainRoot;
        private bool chargeRegistered;
        private GameObject proxyOwner;
        private IAttackEffectSource effectSource;
        private int ownerTeam;
        private IStatProvider ownerStats;
        private ISummonTrigger ownerSummon;
        private IResourcePool ownerPool;
        private IDamageable ownerDmg;
        private float nextRetarget;
        private const float retargetInterval = 0.15f;

        public static float ChainRetriggerChance;

        private static bool hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ClearStatics();

            if (hooked) return;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            hooked = true;
        }

        private static void OnSceneUnloaded(Scene scene) => ClearStatics();

        private static void ClearStatics()
        {
            ChainRetriggerChance = 0f;
            ApplyingProjectileHit = false;
        }

        private bool UsePrefabMove => prefabMoveType != MovementType.Default;
        private MovementType MoveType => UsePrefabMove ? prefabMoveType : pd.MovementType;
        private float WaveAmp => UsePrefabMove ? prefabWaveAmp : pd.WaveAmplitude;
        private float WaveFreq => UsePrefabMove ? prefabWaveFreq : pd.WaveFrequency;
        private float SpiralSpacing => UsePrefabMove ? prefabSpiralSpacing : pd.SpiralSpacing;

        private void Awake()
        {
            hit = new();
            hitExpiryTargets = new();
            hitExpiryTimes = new();
            canTriggerAdd = true;
            defaultPd = pd;
            defaultScale = transform.localScale;
            rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.gravityScale = 0f;
            CachePrefabMovement();
        }

        private void CachePrefabMovement()
        {
            if (pd == null) { prefabMoveType = MovementType.Default; return; }

            prefabMoveType = pd.MovementType;
            prefabWaveAmp = pd.WaveAmplitude;
            prefabWaveFreq = pd.WaveFrequency;
            prefabSpiralSpacing = pd.SpiralSpacing;
        }

        private void OnDestroy()
        {
            UnregisterFromOwner();
        }

        private void UnregisterFromOwner()
        {
            bool live = ownerObj != null && oc != null;

            if (live && pd != null && pd.OrbitRadius > 0 && pd.OrbitSelf && oc.Orbit != null)
                oc.Orbit.UnregisterOrbitingProjectile(this);

            if (live && chargeRegistered && oc.Charge != null)
                oc.Charge.UnregisterChargedProjectile(this);

            chargeRegistered = false;
        }

        private void Despawn()
        {
            GameObject go = gameObject;
            PrefabPool.Release(ref go);
        }

        public void OnPoolAcquire() { }

        public void OnPoolRelease()
        {
            StopAllCoroutines();
            UnregisterFromOwner();

            hit?.Clear();
            hitExpiryTargets?.Clear();
            hitExpiryTimes?.Clear();
            ownerObj = null;
            ClearOwnerCache();
            followTarget = null;
            orbitTarget = null;
            sourceRb = null;

            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        public void Setup(Vector2 direction, GameObject owner, ProjectileData pdOverride, AttackData chainRootOverride = null)
        {
            pd = pdOverride != null ? pdOverride : defaultPd;
            chainRoot = chainRootOverride;

            if (pd == null)
            {
                Debug.LogError($"Projectile '{name}' has no ProjectileData assigned.", this);
                Despawn();
                return;
            }

            chargeRegistered = false;
            ownerObj = owner;
            dir = direction;

            CacheOwner();

            hit.Clear();
            hitExpiryTargets.Clear();
            hitExpiryTimes.Clear();
            canTriggerAdd = true;
            pierced = 0;

            orbitCancelled = false;
            orbitInitialized = false;
            orbitTarget = null;
            orbitDirectionSign = 0f;
            orbitAngleOffset = 0f;
            effectiveOrbitRadius = 0f;

            boomerangActive = false;
            boomerangReturning = false;
            boomerangSpeed = 0f;
            boomerangDecel = 0f;

            followTarget = null;
            sourceRb = null;
            nextRetarget = 0f;

            patternSuspended = false;
            patternTime = 0f;
            spiralTheta = 0f;

            effSpd = ownerStats != null
                ? pd.Speed * (1f + (ownerStats.GetStat(StatType.ProjSpd) * 0.01f))
                : pd.Speed;

            CaptureSnapshot();

            transform.localScale = defaultScale;
            HandleSize();
            HandleDirection();

            if (rb != null) rb.linearVelocity = Vector2.zero;

            InitBoomerang();
            HandleMovement(true);

            var castExtras = ExtraEffects();

            if (pd.Effects != null)
                for (int i = 0; i < pd.Effects.Count; i++) ApplyOnCast(pd.Effects[i]);

            if (castExtras != null)
                for (int i = 0; i < castExtras.Count; i++) ApplyOnCast(castExtras[i]);

            if (pd.OrbitRadius > 0 && pd.OrbitSelf && oc != null && oc.Orbit != null)
                oc.Orbit.RegisterOrbitingProjectile(this);

            if (pd.MainAttack != null && oc != null && oc.Charge != null &&
                oc.Charge.ActiveChargeSource == pd.MainAttack)
            {
                oc.Charge.RegisterChargedProjectile(this);
                chargeRegistered = true;
            }

            lifeRemaining = pd.Lifetime;
        }

        private void CacheOwner()
        {
            ClearOwnerCache();
            if (ownerObj == null) return;

            oc = ProjectileOwnerCache.Get(ownerObj);

            proxyOwner = oc.Proxy;
            effectSource = oc.EffectSource;
            ownerStats = oc.Stats;
            ownerSummon = oc.Summon;
            ownerPool = oc.Pool;
            ownerDmg = oc.Dmg;
            onHit = oc.OnHit;
            ownerTeam = oc.Team;
        }

        private void ClearOwnerCache()
        {
            proxyOwner = null;
            effectSource = null;
            ownerStats = null;
            ownerSummon = null;
            ownerPool = null;
            ownerDmg = null;
            ownerTeam = 0;
            oc = null;
            onHit = null;
        }

        private bool RetargetReady()
        {
            if (Time.time < nextRetarget) return false;
            nextRetarget = Time.time + retargetInterval;
            return true;
        }

        private void FixedUpdate() => HandleMovement(false);

        private void Update()
        {
            if (pd == null) return;

            TickHitHistory();

            lifeRemaining -= Time.deltaTime;
            if (lifeRemaining > 0f) return;

            if (!pd.AddAttackRequiresHit) HandleAdditionalSpawns();

            Despawn();
        }

        public void OnChargeTick()
        {
            if (pd == null) return;

            lifeRemaining = pd.Lifetime;
            CaptureSnapshot();
        }

        private float AddSpawnDist => pd.DistFromCenter > 0f ? pd.DistFromCenter : pd.AdditionalAttack.SpawnDistance;

        private AttackData ChainOrigin => chainRoot != null ? chainRoot : (pd != null ? pd.MainAttack : null);

        private void HandleSize()
        {
            if (ownerObj == null || pd == null || ownerStats == null) return;

            float sizeMult = Mathf.Max(0f, pd.Size * (1f + (ownerStats.GetStat(StatType.aoePct) * 0.01f)));
            transform.localScale = new Vector3(defaultScale.x * sizeMult, defaultScale.y * sizeMult, defaultScale.z);
        }

        private void HandleDirection()
        {
            if (ownerObj == null || pd == null) return;

            if (pd.RandomDir)
            {
                float randAngle = Random.Range(0f, 360f);
                dir = new Vector2(Mathf.Cos(randAngle * Mathf.Deg2Rad), Mathf.Sin(randAngle * Mathf.Deg2Rad));
                transform.rotation = Quaternion.Euler(0f, 0f, randAngle + pd.RotationOffset);
                return;
            }

            if (ownerTeam == 1 && dir == Vector2.zero)
            {
                Camera cam = MainCam;
                if (cam != null)
                {
                    Vector3 mouseWorldPos = cam.ScreenToWorldPoint(InputState.mousePos);
                    mouseWorldPos.z = 0f;

                    dir = (mouseWorldPos - transform.position).normalized;
                }
            }

            transform.rotation = Quaternion.Euler(0f, 0f, GetSpriteAngle(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg));
        }

        private float GetSpriteAngle(float moveAngle)
        {
            if (!pd.UseTrueAngle) return moveAngle + pd.RotationOffset;

            Vector2 trueAngle = new(Mathf.Cos(pd.AngleOverride * Mathf.Deg2Rad), Mathf.Sin(pd.AngleOverride * Mathf.Deg2Rad));
            return Mathf.Atan2(trueAngle.y, trueAngle.x) * Mathf.Rad2Deg;
        }

        private void InitBoomerang()
        {
            if (pd.MaxBoomerangDist > 0f)
            {
                boomerangActive = true;
                boomerangReturning = false;
                boomerangSpeed = effSpd;
                boomerangDecel = effSpd * effSpd / (2f * pd.MaxBoomerangDist);
            }
        }

        private void HandleMovement(bool start)
        {
            if (rb == null || ownerObj == null || pd == null) return;

            if (pd.FollowSource)
            {
                HandleFollowSourceMovement();
                return;
            }

            if (effSpd <= 0) return;

            if (MoveType == MovementType.FollowCursor)
            {
                HandleCursorFollow();
                return;
            }

            if (MoveType != MovementType.Default)
            {
                HandlePatternMovement(start);
                return;
            }

            if (pd.OrbitRadius > 0 && !orbitCancelled)
            {
                HandleOrbitMovement();
                return;
            }

            if (start) rb.linearVelocity = dir.normalized * effSpd;

            if (pd.FollowDistance > 0 && TryHome()) return;

            if (boomerangActive) UpdateBoomerang();
        }

        private bool TryHome()
        {
            if (followTarget != null && !followTarget.gameObject.activeInHierarchy) followTarget = null;
            if (followTarget == null && RetargetReady())
                followTarget = FindClosestTargetInRange(pd.FollowDistance, ownerTeam == 0);

            if (followTarget == null) return false;

            boomerangActive = false;
            FollowTarget();
            return true;
        }

        private void HandleFollowSourceMovement()
        {
            if (sourceRb == null)
            {
                if (ownerObj == null) return;
                sourceRb = ownerObj.GetComponent<Rigidbody2D>();
                if (sourceRb == null) return;
            }

            rb.linearVelocity = sourceRb.linearVelocity;
        }

        private void HandlePatternMovement(bool start)
        {
            if (start)
            {
                ResetPattern();
                return;
            }

            if (pd.FollowDistance > 0 && TryHome())
            {
                patternSuspended = true;
                return;
            }

            if (patternSuspended)
            {
                patternSuspended = false;
                if (rb.linearVelocity.sqrMagnitude > 0.0001f) dir = rb.linearVelocity.normalized;
                ResetPattern();
            }

            float dt = Time.fixedDeltaTime;
            if (dt <= 0f) return;

            patternTime += dt;

            Vector2 target = MoveType switch
            {
                MovementType.Wave => GetWavePosition(),
                MovementType.Spiral => GetSpiralPosition(dt),
                _ => (Vector2)transform.position
            };
            rb.linearVelocity = (target - (Vector2)transform.position) / dt;
        }

        private void HandleCursorFollow()
        {
            Vector2 targetPos;

            if (ownerTeam == 1)
            {
                Camera cam = MainCam;
                if (cam == null) return;

                Vector3 mouseWorld = cam.ScreenToWorldPoint(InputState.mousePos);
                mouseWorld.z = 0f;
                targetPos = mouseWorld;
            }
            else
            {
                if (followTarget != null && !followTarget.gameObject.activeInHierarchy) followTarget = null;
                if (followTarget == null && RetargetReady())
                    followTarget = FindClosestTargetInRange(pd.FollowDistance > 0f ? pd.FollowDistance : 50f, true);

                if (followTarget == null) return;
                targetPos = followTarget.position;
            }

            Vector2 toTarget = targetPos - (Vector2)transform.position;
            float dt = Time.fixedDeltaTime;

            if (toTarget.sqrMagnitude > 0.0001f) dir = toTarget.normalized;

            rb.linearVelocity = dt > 0f && toTarget.magnitude <= effSpd * dt ? toTarget / dt : dir * effSpd;
            transform.rotation = Quaternion.Euler(0f, 0f, GetSpriteAngle(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg));
        }

        private void ResetPattern()
        {
            patternOrigin = transform.position;
            patternTime = 0f;
            spiralTheta = 0f;
            patternSuspended = false;
            patternBaseAngle = dir != Vector2.zero ? Mathf.Atan2(dir.y, dir.x) : 0f;
        }

        private Vector2 GetWavePosition()
        {
            Vector2 fwd = dir.normalized;
            Vector2 perp = Vector2.Perpendicular(fwd);
            float offset = WaveAmp * Mathf.Sin(2f * Mathf.PI * WaveFreq * patternTime);

            return patternOrigin + (fwd * (effSpd * patternTime)) + (perp * offset);
        }

        private Vector2 GetSpiralPosition(float dt)
        {
            float b = Mathf.Max(SpiralSpacing, 0.01f) / (2f * Mathf.PI);
            float r = b * spiralTheta;

            spiralTheta += effSpd * dt / Mathf.Sqrt((r * r) + (b * b));

            float sign = pd.RotateClockwise ? -1f : 1f;
            float angle = patternBaseAngle + (sign * spiralTheta);

            return patternOrigin + (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (b * spiralTheta));
        }

        private void UpdateBoomerang()
        {
            float dt = Time.fixedDeltaTime;

            if (!boomerangReturning)
            {
                boomerangSpeed -= boomerangDecel * dt;

                if (boomerangSpeed <= 0f)
                {
                    boomerangSpeed = 0f;
                    boomerangReturning = true;
                }

                rb.linearVelocity = dir.normalized * boomerangSpeed;
            }
            else
            {
                boomerangSpeed += boomerangDecel * dt;
                boomerangSpeed = Mathf.Min(boomerangSpeed, effSpd);

                rb.linearVelocity = -dir.normalized * boomerangSpeed;
            }
        }

        private void FollowTarget()
        {
            if (followTarget == null) return;

            float dist = Vector2.Distance(transform.position, followTarget.position);
            if (dist <= pd.FollowDistance)
            {
                Vector2 newDir = (followTarget.position - transform.position).normalized;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, newDir * effSpd, 0.1f);
            }
        }

        private void HandleOrbitMovement()
        {
            if (orbitTarget == null || !orbitTarget.gameObject.activeInHierarchy)
            {
                if (pd != null && pd.OrbitSelf && ownerObj != null) orbitTarget = ownerObj.transform;
                else orbitTarget = RetargetReady() ? FindClosestEnemyInDirection() : null;
                orbitDirectionSign = 0f;
                orbitInitialized = false;
            }

            if (orbitTarget == null) return;

            Vector2 center = orbitTarget.position;
            Vector2 offset = (Vector2)transform.position - center;
            float dist = offset.magnitude;

            if (dist < 0.01f)
            {
                rb.linearVelocity = dir.normalized * effSpd;
                return;
            }

            if (!orbitInitialized)
            {
                effectiveOrbitRadius = pd.OrbitRadius + Random.Range(0f, pd.RandOrbRadOffset);
                orbitDirectionSign = pd.RotateClockwise ? -1f : 1f;

                if (dist < effectiveOrbitRadius * 0.5f && dir != Vector2.zero)
                    orbitAngleOffset = Mathf.Atan2(dir.y, dir.x);
                else
                    orbitAngleOffset = Mathf.Atan2(offset.y, offset.x);

                orbitInitialized = true;
            }

            float currentAngle = Mathf.Atan2(offset.y, offset.x);
            float targetAngle = orbitAngleOffset + (orbitDirectionSign * effSpd * Time.fixedDeltaTime / effectiveOrbitRadius);

            orbitAngleOffset = targetAngle;

            Vector2 desiredPos = center + (new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle)) * effectiveOrbitRadius);
            Vector2 toDesired = desiredPos - (Vector2)transform.position;

            Vector2 tangent = Vector2.Perpendicular(desiredPos - center).normalized;
            Vector2 orbitalVelocity = orbitDirectionSign * effSpd * tangent;

            float radiusError = Vector2.Distance(transform.position, center) - effectiveOrbitRadius;
            Vector2 radialCorrection = -5f * radiusError * (desiredPos - center).normalized;

            rb.linearVelocity = orbitalVelocity + radialCorrection + (toDesired * 5f);
        }

        private static readonly List<Collider2D> OverlapBuffer = new();
        private static Camera cachedMainCam;
        private static Camera MainCam => cachedMainCam != null ? cachedMainCam : cachedMainCam = Camera.main;

        private static int OverlapCircle(Vector2 position, float radius)
        {
            ContactFilter2D filter = default;
            filter.useTriggers = false;

            return Physics2D.OverlapCircle(position, radius, filter, OverlapBuffer);
        }

        private static bool IsDead(GameObject go)
            => go.TryGetComponent<IStatProvider>(out var esm)
               && (esm.GetStat(StatType.isAlive) <= 0f || esm.GetStat(StatType.currentHp) <= 0f);

        private Transform FindClosestEnemyInDirection()
        {
            Transform closest = null;
            float closestDist = float.MaxValue;

            int targetTeam = ownerTeam == 0 ? 1 : 0;
            float searchRadius = effSpd * pd.Lifetime;
            int count = OverlapCircle(transform.position, searchRadius);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = OverlapBuffer[i];
                if (!col.gameObject.TryGetComponent<ITeamMember>(out var itm) || itm.TeamID != targetTeam) continue;
                if (hit.Contains(col.gameObject)) continue;
                if (col.gameObject == ownerObj) continue;

                Vector2 toEnemy = (col.transform.position - transform.position).normalized;
                float dot = Vector2.Dot(dir.normalized, toEnemy);
                if (dot <= 0) continue;

                if (IsDead(col.gameObject)) continue;

                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = col.transform;
                }
            }
            return closest;
        }

        private Transform FindClosestTargetInRange(float range, bool searchForPlayer)
        {
            Transform closest = null;
            float minDist = range;

            int count = OverlapCircle(transform.position, range);
            int targetTeam = searchForPlayer ? 1 : 0;

            for (int i = 0; i < count; i++)
            {
                Collider2D col = OverlapBuffer[i];
                if (!col.gameObject.TryGetComponent<ITeamMember>(out var itm) || itm.TeamID != targetTeam) continue;

                if (hit.Contains(col.gameObject)) continue;

                if (IsDead(col.gameObject)) continue;

                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col.transform;
                }
            }
            return closest;
        }

        public void Launch(Vector2 direction)
        {
            orbitCancelled = true;
            orbitTarget = null;
            dir = direction.normalized;
            if (pd != null && MoveType != MovementType.Default) ResetPattern();
            if (rb != null) rb.linearVelocity = dir * effSpd;
        }

        public void Explode()
        {
            if (pd.AdditionalAttack != null && pd.AdditionalAttack.ProjectilePrefab != null && ProjectileSpawner.Instance != null)
            {
                Vector2? addDir = pd.AdditionalFollowsMouse ? null : dir;
                ProjectileSpawner.Instance.Spawn(pd.AdditionalAttack, ownerObj, transform.position, addDir, AddSpawnDist, ChainOrigin);
            }
            Despawn();
        }
    }
}
