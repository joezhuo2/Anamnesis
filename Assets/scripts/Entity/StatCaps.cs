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
            { StatType.dodgeChance, 100f },
            { StatType.EffectRes, 100f },
            { StatType.kbRes, 100f },
            { StatType.dashCooldownRedPct, 100f },
            { StatType.dashStaminaCostRedPct, 100f },
            { StatType.castTimeRedPct, 90f },
            { StatType.damageRes, 90f },
            { StatType.physicalRes, 90f },
            { StatType.spellRes, 90f },
            { StatType.basicCdRedPct, 90f },
            { StatType.skillCdRedPct, 90f },
            { StatType.ultCdRedPct, 90f },
        };

        public static float Clamp(StatType type, float value) => Max.TryGetValue(type, out float cap) ? Mathf.Min(value, cap) : value;

        public static bool IsMaxed(StatType type, float value) => Max.TryGetValue(type, out float cap) && value >= cap;
    }
}
