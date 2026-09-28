using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public static class KnockbackHandler
    {
        public struct AppliedForce
        {
            public float force;
            public float time;
            public Vector2 dir;
        }

        public static Vector2 UpdateForces(List<AppliedForce> forces, float dt)
        {
            if (forces.Count == 0) return Vector2.zero;

            Vector2 totalForce = Vector2.zero;

            for (int i = forces.Count - 1; i >= 0; i--)
            {
                var f = forces[i];
                float timeRemaining = f.time - dt;

                if (timeRemaining <= 0f)
                {
                    forces.RemoveAt(i);
                    continue;
                }

                forces[i] = new AppliedForce { dir = f.dir, force = f.force, time = timeRemaining };
                totalForce += f.dir * f.force;
            }

            return totalForce;
        }

        public static void ApplyKnockback(List<AppliedForce> forces, Vector2 direction, float force, float duration, float kbRes = 0f)
            => forces.Add(new AppliedForce
            {
                dir = direction,
                force = force * Mathf.Max(0f, 1f - (kbRes * 0.01f)),
                time = duration
            });
    }
}
