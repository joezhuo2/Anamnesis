using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    // Temporary buffs shared by every enemy, alive or spawned later (e.g. Blood Moon).
    // Sources add on start and remove the same values on end, so overlapping sources stack.
    public static class EnemyGlobalBuffs
    {
        public const int EnemyTeam = 0;

        public static float DamagePct { get; private set; }
        public static float DamageTakenPct { get; private set; }
        public static float MoveSpeedPct { get; private set; }

        public static float DamageTakenMult => Mathf.Max(0f, 1f + (DamageTakenPct * 0.01f));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            DamagePct = 0f;
            DamageTakenPct = 0f;
            MoveSpeedPct = 0f;
        }

        public static void Add(float damagePct, float damageTakenPct, float moveSpeedPct, bool isAdding = true)
        {
            float sign = isAdding ? 1f : -1f;
            DamagePct += damagePct * sign;
            DamageTakenPct += damageTakenPct * sign;
            MoveSpeedPct += moveSpeedPct * sign;
        }

        public static bool AppliesTo(IStatProvider esm) => esm is EnemyStatManager e && e.teamID == EnemyTeam;
    }
}
