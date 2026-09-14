using System;
using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    public static class HitFeedback
    {
        public const float FreezeScale = 0.001f;

        private static bool active;
        private static float endTime;
        private static float readyTime;
        private static float restoreScale = 1f;
        private static float pendingCooldown;
        private static HitFeedbackRunner runner;

        public static event Action<float> ShakeRequested;

        public static bool Active => active;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            active = false;
            endTime = 0f;
            readyTime = 0f;
            restoreScale = 1f;
            pendingCooldown = 0f;
            runner = null;
            ShakeRequested = null;
        }

        public static void Shake(float force)
        {
            if (force <= 0f || Time.timeScale == 0f) return;
            ShakeRequested?.Invoke(force);
        }

        public static void Stop(float duration, float cooldown = 0f)
        {
            if (duration <= 0f || !Application.isPlaying) return;

            float now = Time.unscaledTime;

            if (active)
            {
                endTime = Mathf.Max(endTime, now + duration);
                pendingCooldown = Mathf.Max(pendingCooldown, cooldown);
                return;
            }

            if (Time.timeScale == 0f || now < readyTime) return;

            EnsureRunner();

            active = true;
            restoreScale = Time.timeScale;
            endTime = now + duration;
            pendingCooldown = Mathf.Max(0f, cooldown);
            Time.timeScale = FreezeScale;
        }

        public static void Cancel()
        {
            if (!active) return;
            Finish();
        }

        internal static void Tick()
        {
            if (active && Time.unscaledTime >= endTime) Finish();
        }

        private static void Finish()
        {
            active = false;
            readyTime = Time.unscaledTime + pendingCooldown;
            if (Mathf.Approximately(Time.timeScale, FreezeScale)) Time.timeScale = restoreScale;
        }

        private static void EnsureRunner()
        {
            if (runner != null) return;

            var go = new GameObject("[HitFeedback]") { hideFlags = HideFlags.HideInHierarchy };
            UnityEngine.Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<HitFeedbackRunner>();
        }
    }

    internal sealed class HitFeedbackRunner : MonoBehaviour
    {
        private void Update() => HitFeedback.Tick();
    }
}
