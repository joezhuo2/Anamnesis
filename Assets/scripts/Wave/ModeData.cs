using System.Text;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SettingsSystem;
using UnityEngine;

namespace CrystalFlux.WaveSystem
{
    [CreateAssetMenu(fileName = "md", menuName = "Data/Mode")]
    public class ModeData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Simple";
        [TextArea(3, 10)] public string description;
        public Sprite buttonSprite;
        [Tooltip("Compared against AttackData.minMode, PlayerUpgrade.minMode and SkillNodeDef.minMode. 0 = Simple, 1 = Expert, 2 = Master")]
        [Min(0)] public int tier;

        [Header("Unlocks")]
        public bool allowCorruption;
        public bool allowCorruptionSpecials;
        public bool unlockUltimates;
        [Tooltip("Ultimate injected into the player on run start (requires unlockUltimates)")]
        public AttackData startingUlt;

        private static ModeData neutral;

        public static ModeData Neutral
        {
            get
            {
                if (neutral == null)
                {
                    neutral = CreateInstance<ModeData>();
                    neutral.hideFlags = HideFlags.HideAndDontSave;
                    neutral.allowCorruption = true;
                    neutral.allowCorruptionSpecials = true;
                    neutral.unlockUltimates = true;
                }
                return neutral;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => neutral = null;

        public string BuildTooltipDescription()
        {
            StringBuilder sb = new();
            if (!string.IsNullOrWhiteSpace(description)) sb.Append(description.TrimEnd());

            AppendLine(sb, allowCorruption ? "Corruption unlocked" : "Corruption locked");
            if (allowCorruption && allowCorruptionSpecials) AppendLine(sb, "Corruption specials unlocked");
            AppendLine(sb, unlockUltimates ? "Ultimates unlocked" : "Ultimates locked");
            if (unlockUltimates && startingUlt != null) AppendLine(sb, $"Starting ultimate: {startingUlt.name}");
            if (tier > 0) AppendLine(sb, $"{RunMode.TierName(tier)} attacks, awakenings and nodes unlocked");

            return sb.ToString();
        }

        private static void AppendLine(StringBuilder sb, string line)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(line);
        }
    }
}
