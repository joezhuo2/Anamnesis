using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    public static class RunMode
    {
        private static readonly string[] TierNames = { "Simple", "Expert", "Master" };

        public static int Tier { get; set; }
        public static bool UltimatesUnlocked { get; set; } = true;

        public static string TierName(int tier)
            => tier >= 0 && tier < TierNames.Length ? TierNames[tier] : $"Tier {tier}";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Tier = 0;
            UltimatesUnlocked = true;
        }
    }
}
