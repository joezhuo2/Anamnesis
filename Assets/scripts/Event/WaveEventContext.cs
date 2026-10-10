using System.Collections;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.WaveEventSystem
{
    public class WaveEventContext
    {
        private readonly System.Func<GameObject> player;

        public GameObject Player => player?.Invoke();
        public float MinDistance { get; }
        public float MaxDistance { get; }

        public WaveEventContext(System.Func<GameObject> player, float minDistance, float maxDistance)
        {
            this.player = player;
            MinDistance = Mathf.Max(0f, minDistance);
            MaxDistance = Mathf.Max(MinDistance, maxDistance);
        }

        public static bool Paused => !WaveManager.WaveAcceptingEvents || WaveManager.DroughtActive;

        public IEnumerator WaitWhilePaused()
        {
            while (Paused) yield return null;
        }

        public IEnumerator Wait(float seconds)
        {
            float remaining = seconds;
            while (remaining > 0f)
            {
                yield return null;
                if (!Paused) remaining -= Time.deltaTime;
            }
        }

        public bool TryGetSpawnPoint(out Vector2 pos)
        {
            GameObject p = player?.Invoke();
            if (p == null)
            {
                pos = default;
                return false;
            }

            float dist = Random.Range(MinDistance, MaxDistance);
            float ang = Random.Range(0f, Mathf.PI * 2f);
            pos = (Vector2)p.transform.position + (new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * dist);
            return true;
        }
    }
}
