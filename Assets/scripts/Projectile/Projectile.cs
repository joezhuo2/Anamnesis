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

        private static readonly List<Collider2D> OverlapBuffer = new();
        private static Camera cachedMainCam;
        private static Camera MainCam => cachedMainCam != null ? cachedMainCam : cachedMainCam = Camera.main;

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
