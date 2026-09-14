using CrystalFlux.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    public class CombatFeedback : MonoBehaviour
    {
        [Header("Screen Shake")]
        public CinemachineImpulseSource source;
        [Tooltip("Real-time seconds each shake lasts")]
        public float impulseDuration = 0.2f;
        [Tooltip("Upper clamp on a single shake's force")]
        public float maxShake = 1.5f;

        [Header("Player Hurt")]
        public float playerHurtShake = 0.25f;
        public float playerHurtHitStop = 0f;
        public float playerHurtHitStopCooldown = 0f;

        private static CombatFeedback instance;
        private CinemachineImpulseListener listener;
        private float pendingShake;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null || FindFirstObjectByType<CombatFeedback>() != null) return;

            var go = new GameObject(nameof(CombatFeedback));
            DontDestroyOnLoad(go);
            go.AddComponent<CombatFeedback>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
            EnsureSource();
        }

        private void OnEnable()
        {
            HitFeedback.ShakeRequested += OnShake;
            PlayerEvents.OnPlayerTakeDamage += OnPlayerTakeDamage;
        }

        private void OnDisable()
        {
            HitFeedback.ShakeRequested -= OnShake;
            PlayerEvents.OnPlayerTakeDamage -= OnPlayerTakeDamage;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void OnPlayerTakeDamage(IDamageable _)
        {
            HitFeedback.Stop(playerHurtHitStop, playerHurtHitStopCooldown);
            HitFeedback.Shake(playerHurtShake);
        }

        private void Update()
        {
            if (pendingShake <= 0f || HitFeedback.Active) return;

            float force = pendingShake;
            pendingShake = 0f;

            if (Time.timeScale == 0f) return;
            GenerateShake(force);
        }

        private void OnShake(float force)
        {
            if (HitFeedback.Active)
            {
                pendingShake = Mathf.Max(pendingShake, force);
                return;
            }

            GenerateShake(force);
        }

        private void GenerateShake(float force)
        {
            if (!EnsureSource() || !EnsureListener()) return;

            Vector2 dir = Random.insideUnitCircle;
            dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.up;

            source.GenerateImpulseWithVelocity(dir * Mathf.Min(force, maxShake));
        }

        private bool EnsureSource()
        {
            if (source != null) return true;
            if (TryGetComponent(out source)) return true;

            source = gameObject.AddComponent<CinemachineImpulseSource>();
            source.ImpulseDefinition = new CinemachineImpulseDefinition
            {
                ImpulseChannel = 1,
                ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump,
                CustomImpulseShape = new AnimationCurve(),
                ImpulseDuration = impulseDuration,
                ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
                DissipationDistance = 100f,
                DissipationRate = 0.25f,
                PropagationSpeed = 343f
            };
            source.DefaultVelocity = Vector3.down;
            return true;
        }

        private bool EnsureListener()
        {
            if (listener != null) return true;

            var cam = FindFirstObjectByType<CinemachineCamera>();
            if (cam == null) return false;
            if (cam.TryGetComponent(out listener)) return true;

            listener = cam.gameObject.AddComponent<CinemachineImpulseListener>();
            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = 1;
            listener.Gain = 1f;
            listener.Use2DDistance = true;
            listener.UseCameraSpace = true;
            return true;
        }
    }
}
