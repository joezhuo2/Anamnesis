using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class Targetable : MonoBehaviour
    {
        [Tooltip("Used only when no ITeamMember is on this object")] public int teamID = 1;

        private static readonly List<Targetable> active = new();

        private ITeamMember itm;
        private IDamageable dmg;

        public int TeamID => itm != null ? itm.TeamID : teamID;
        public bool Valid => dmg == null || dmg.IsAlive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => active.Clear();

        private void Awake()
        {
            TryGetComponent(out itm);
            TryGetComponent(out dmg);
        }

        private void OnEnable() => active.Add(this);
        private void OnDisable() => active.Remove(this);

        public static GameObject FindNearest(Vector2 pos, float range, int ownTeam, GameObject fallback)
        {
            if (range < 0f) return null;

            float bestSqr = range * range;
            GameObject best = null;

            if (fallback != null)
            {
                float d = ((Vector2)fallback.transform.position - pos).sqrMagnitude;
                if (d <= bestSqr)
                {
                    bestSqr = d;
                    best = fallback;
                }
            }

            for (int i = 0; i < active.Count; i++)
            {
                Targetable t = active[i];
                if (t == null || t.TeamID == ownTeam || !t.Valid) continue;

                float d = ((Vector2)t.transform.position - pos).sqrMagnitude;
                if (d > bestSqr) continue;

                bestSqr = d;
                best = t.gameObject;
            }

            return best;
        }
    }
}
