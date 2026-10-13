using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.SaveSystem
{
    public static class StatsMath
    {
        public static bool IsFlag(StatType t) => t switch
        {
            StatType.isImmune or StatType.isAlive or StatType.IsDashing or StatType.IsAttacking
                or StatType.CanMove or StatType.CanDash or StatType.CanAttack or StatType.CanGainHp
                or StatType.CanGainMana or StatType.CanGainStamina or StatType.DashShouldApplyIFrame => true,
            _ => false
        };

        public static string CleanStatsJson(EntityStats live, List<StatBuff> effectStats)
        {
            if (live == null) return "";

            EntityStats copy = Object.Instantiate(live);
            try
            {
                if (effectStats != null)
                    foreach (StatBuff b in effectStats)
                        if (!IsFlag(b.type)) copy.Apply(b.type, -b.value);

                return JsonUtility.ToJson(copy);
            }
            finally
            {
                Object.DestroyImmediate(copy);
            }
        }
    }
}
