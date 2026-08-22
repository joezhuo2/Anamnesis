using System.Collections.Generic;
using UnityEngine;

public static class KnockbackHandler
{
    public struct AppliedForce
    {
        public float force;
        public float time;
        public Vector2 dir;
    }

    public static Vector2 UpdateForces(List<AppliedForce> forces, float kbRes)
    {
        if (forces.Count == 0) return Vector2.zero;

        Vector2 totalForce = Vector2.zero;

        for (int i = forces.Count - 1; i >= 0; i--)
        {
            var f = forces[i];
            float timeRemaining = f.time - Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                forces.RemoveAt(i);
                continue;
            }

            float kbf = f.force * (1f - (kbRes * 0.01f));
            forces[i] = new AppliedForce { dir = f.dir, force = kbf, time = timeRemaining };
            totalForce += f.dir * kbf;
        }

        return totalForce;
    }

    public static void ApplyKnockback(List<AppliedForce> forces, Vector2 direction, float force, float duration)
    {
        forces.Add(new AppliedForce { dir = direction, force = force, time = duration });
    }
}