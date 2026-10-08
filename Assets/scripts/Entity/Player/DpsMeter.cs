using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public static class DpsMeter
    {
        public const float Window = 1f;

        private struct Sample
        {
            public float time;
            public float amount;
        }

        private static readonly Queue<Sample> samples = new();
        private static float windowSum;
        private static float segmentStart;
        private static float peak;
        private static float total;

        public static float Peak => peak;
        public static float Total => total;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Reset();

        public static void Reset()
        {
            samples.Clear();
            windowSum = 0f;
            segmentStart = 0f;
            peak = 0f;
            total = 0f;
        }

        public static void Record(float amount)
        {
            if (amount <= 0f) return;

            float now = Time.time;
            Prune(now);
            if (samples.Count == 0) segmentStart = now;

            samples.Enqueue(new Sample { time = now, amount = amount });
            windowSum += amount;
            total += amount;

            float dps = Current;
            if (dps > peak) peak = dps;
        }

        public static float Current
        {
            get
            {
                float now = Time.time;
                Prune(now);
                if (samples.Count == 0) return 0f;
                return windowSum / Mathf.Clamp(now - segmentStart, 1f, Window);
            }
        }

        private static void Prune(float now)
        {
            while (samples.Count > 0 && now - samples.Peek().time > Window)
                windowSum -= samples.Dequeue().amount;

            if (samples.Count == 0) windowSum = 0f;
        }
    }
}
