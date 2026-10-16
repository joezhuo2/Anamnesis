using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.Core
{
    public static class StatCaps
    {
        private static readonly Dictionary<StatType, float> Max = new()
        {
            { StatType.critChance, 100f },
            { StatType.aoePct, 100f },
            { StatType.dodgeChance, 80f },
            { StatType.EffectRes, 90f },
            { StatType.kbRes, 100f },
            { StatType.dashCooldownRedPct, 100f },
            { StatType.dashStaminaCostRedPct, 100f },
            { StatType.castTimeRedPct, 90f },
            { StatType.damageRes, 90f },
            { StatType.physicalRes, 90f },
            { StatType.spellRes, 90f },
            { StatType.attackSpeedPct, 70f },
            { StatType.basicCdRedPct, 90f },
            { StatType.skillCdRedPct, 90f },
            { StatType.ultCdRedPct, 90f },
            { StatType.resPen, 300f },
            { StatType.ProjSpd, 300f },
            { StatType.kbPct, 300f }
        };

        private static readonly Dictionary<StatType, float> Min = new()
        {
            { StatType.seTickRatePct, -90f },
            { StatType.seDurPct, -90f },
            { StatType.stCostPct, -90f },
            { StatType.moveSpeedPct, -90f }
        };

        public static float Clamp(StatType type, float value)
        {
            if (Max.TryGetValue(type, out float cap)) value = Mathf.Min(value, cap);
            if (Min.TryGetValue(type, out float floor)) value = Mathf.Max(value, floor);
            return value;
        }

        public static bool IsMaxed(StatType type, float value) => Max.TryGetValue(type, out float cap) && value >= cap;
    }
}
