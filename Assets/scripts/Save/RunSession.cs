using System;
using UnityEngine;

namespace CrystalFlux.SaveSystem
{
    public static class RunSession
    {
        public static SaveFile PendingLoad;
        public static string CurrentRunName;
        public static RunData WaveStartRun;
        public static BuildData WaveStartBuild;
        public static int WaveStartNumber;
        public static float Playtime;
        public static bool ClockRunning;

        public static Action RequestHomeScreen;
        public static Action PushPause;
        public static Action PopPause;
        public static Func<bool> IsPlayerDead;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            PendingLoad = null;
            ClearForNewScene();
        }

        public static void ClearForNewScene()
        {
            CurrentRunName = null;
            WaveStartRun = null;
            WaveStartBuild = null;
            WaveStartNumber = 0;
            Playtime = 0f;
            ClockRunning = false;
        }

        public static void Tick(float dt)
        {
            if (ClockRunning && dt > 0f) Playtime += dt;
        }
    }
}
