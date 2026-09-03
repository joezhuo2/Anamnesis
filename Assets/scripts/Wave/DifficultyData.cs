using System.Text;
using UnityEngine;

namespace CrystalFlux.WaveSystem
{
    [CreateAssetMenu(fileName = "dd", menuName = "Data/Difficulty")]
    public class DifficultyData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Normal";
        [TextArea(3, 10)] public string description;
        public Sprite frameSprite;
        public Color nameColor = Color.white;

        [Header("Enemy Scaling")]
        public int enemyLevelAdd;
        public float enemyLevelPerWaveAdd;
        public int maxTotalEnemiesAdd;
        public int maxCurrentEnemiesAdd;

        [Header("Reward Offsets")]
        public int rewardChoicesAdd;
        public float qualityBonusAdd;
        public int milestoneRewardChoicesAdd;

        [Header("Occasional Wave Rewards")]
        public float occasionalRerollChanceAdd;
        public float occasionalSkillPointChanceAdd;

        [Header("Anomaly Offsets")]
        public float anomalyChanceAdd;
        public int minAnomalyCountAdd;
        public int maxAnomalyCountAdd;
        public int anomalyRerollMinAdd;
        public int anomalyRerollMaxAdd;
        public int anomalySkillPointAdd;
        public float anomalyQualityAdd;

        [Header("Corruption Offsets")]
        public float corruptChanceAdd;
        public float corruptPositiveChanceAdd;
        public float maxCorruptBoostAdd;

        [Header("Economy Offsets")]
        public int rerollGoldCostAdd;
        public int startingRerollsAdd;
        public int startingSkillPointsAdd;

        [Header("Pre-Run Free Pick")]
        public int preRunPickCount;
        [Range(0f, 100f)] public float preRunTreasureChance = 25f;

        private static DifficultyData neutral;

        public static DifficultyData Neutral
        {
            get
            {
                if (neutral == null)
                {
                    neutral = CreateInstance<DifficultyData>();
                    neutral.hideFlags = HideFlags.HideAndDontSave;
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

            AppendInt(sb, "Enemy level", enemyLevelAdd);
            AppendFloat(sb, "Enemy level per wave", enemyLevelPerWaveAdd, "F2");
            AppendInt(sb, "Total enemies", maxTotalEnemiesAdd);
            AppendInt(sb, "Enemies at once", maxCurrentEnemiesAdd);

            AppendInt(sb, "Reward choices", rewardChoicesAdd);
            AppendFloat(sb, "Reward quality", qualityBonusAdd, "F2");
            AppendInt(sb, "Milestone choices", milestoneRewardChoicesAdd);

            AppendPercent(sb, "Bonus reroll chance", occasionalRerollChanceAdd * 100f);
            AppendPercent(sb, "Bonus skill point chance", occasionalSkillPointChanceAdd * 100f);

            AppendPercent(sb, "Anomaly chance", anomalyChanceAdd);
            AppendInt(sb, "Min anomaly choices", minAnomalyCountAdd);
            AppendInt(sb, "Max anomaly choices", maxAnomalyCountAdd);
            AppendInt(sb, "Min anomaly rerolls", anomalyRerollMinAdd);
            AppendInt(sb, "Max anomaly rerolls", anomalyRerollMaxAdd);
            AppendInt(sb, "Anomaly skill points", anomalySkillPointAdd);
            AppendFloat(sb, "Anomaly quality", anomalyQualityAdd, "F2");

            AppendPercent(sb, "Corrupt chance", corruptChanceAdd);
            AppendPercent(sb, "Corrupt positive chance", corruptPositiveChanceAdd);
            AppendPercent(sb, "Max corrupt boost", maxCorruptBoostAdd);

            AppendInt(sb, "Reroll cost", rerollGoldCostAdd, "g");
            AppendInt(sb, "Starting rerolls", startingRerollsAdd);
            AppendInt(sb, "Starting skill points", startingSkillPointsAdd);

            if (preRunPickCount > 0)
                AppendRaw(sb, $"{preRunPickCount} free pick{(preRunPickCount > 1 ? "s" : "")} before wave 1");

            return sb.ToString();
        }

        private static void AppendRaw(StringBuilder sb, string line)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(line);
        }

        private static void AppendInt(StringBuilder sb, string label, int v, string suffix = "")
        {
            if (v == 0) return;
            AppendRaw(sb, $"{label} {(v > 0 ? "+" : "")}{v}{suffix}");
        }

        private static void AppendFloat(StringBuilder sb, string label, float v, string fmt)
        {
            if (Mathf.Approximately(v, 0f)) return;
            AppendRaw(sb, $"{label} {(v > 0f ? "+" : "")}{v.ToString(fmt)}");
        }

        private static void AppendPercent(StringBuilder sb, string label, float v)
        {
            if (Mathf.Approximately(v, 0f)) return;
            AppendRaw(sb, $"{label} {(v > 0f ? "+" : "")}{v:F0}%");
        }
    }
}
