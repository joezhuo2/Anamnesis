using System.Collections;
using UnityEngine;

namespace CrystalFlux.WaveEventSystem
{
    public abstract class WaveEventData : ScriptableObject
    {
        [Header("Display")]
        public string title;
        public string subtitle;
        public Color titleColor = Color.white;
        [Tooltip("Total title on-screen time in seconds. 0 = use the spawner's titleDuration")] [Min(0f)] public float titleDurationOverride;

        [Header("Spawning")]
        [Tooltip("Chance (0-100) to fire on each spawner roll")] [Range(0f, 100f)] public float chance = 10f;
        [Tooltip("Added to the spawner's baseRadius. Can be negative")] public float radiusIncrease;

        [Header("Gating")]
        [Tooltip("Minimum mode tier required for this event. 0 = Simple, 1 = Expert, 2 = Master")] [Min(0)] public int minMode;
        [Tooltip("First wave this event can fire on. 0 = no lower bound")] [Min(0)] public int minWave;
        [Tooltip("Last wave this event can fire on. 0 = no upper bound")] [Min(0)] public int maxWave;
        public bool allowOnIronman = true;

        public bool IsEligible(int wave, int tier, bool ironman)
        {
            if (chance <= 0f || tier < minMode) return false;
            if (minWave > 0 && wave < minWave) return false;
            if (maxWave > 0 && wave > maxWave) return false;
            return allowOnIronman || !ironman;
        }

        public abstract IEnumerator Run(WaveEventContext ctx);

        protected static int RollCount(int min, int max) => Random.Range(Mathf.Max(0, Mathf.Min(min, max)), Mathf.Max(min, max) + 1);

        protected static float RollInterval(float min, float max) => Mathf.Max(0f, Random.Range(Mathf.Min(min, max), Mathf.Max(min, max)));
    }
}
