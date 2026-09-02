using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    public static class MenuPause
    {
        private static int depth;
        private static float restoreTimeScale = 1f;

        public static bool IsPaused => depth > 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            depth = 0;
            restoreTimeScale = 1f;
        }

        public static void Push()
        {
            if (depth++ > 0) return;

            restoreTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
        }

        public static void Pop()
        {
            if (depth == 0) return;
            if (--depth > 0) return;

            Time.timeScale = restoreTimeScale;
        }
    }
}
