using System.Collections;
using System.Collections.Generic;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.WaveEventSystem
{
    [CreateAssetMenu(fileName = "Spawn Enemy Event", menuName = "Data/Events/Spawn Enemy")]
    public class SpawnEnemyEventData : WaveEventData
    {
        [Header("Spawn Enemy")]
        [Tooltip("Picked uniformly for each spawn")] public List<GameObject> enemies = new();
        [Min(0)] public int minSpawns = 6;
        [Min(0)] public int maxSpawns = 10;
        [Tooltip("Seconds between consecutive spawns")] [Min(0f)] public float minInterval = 0.2f;
        [Min(0f)] public float maxInterval = 0.6f;
        [Tooltip("Added to the current wave enemy level")] public int levelBonus;

        public override IEnumerator Run(WaveEventContext ctx)
        {
            int n = RollCount(minSpawns, maxSpawns);
            if (n <= 0 || PickEnemy() == null || !WaveManager.ReserveEnemies(n)) yield break;

            int wave = WaveManager.CurrentWave;

            for (int i = 0; i < n; i++)
            {
                // Never pause here: pending reservations hold the wave open, so pausing would deadlock it.
                if (i > 0)
                {
                    float wait = RollInterval(minInterval, maxInterval);
                    if (wait > 0f) yield return new WaitForSeconds(wait);
                }

                if (!WaveManager.WaveActive || WaveManager.CurrentWave != wave) yield break;

                GameObject prefab = PickEnemy();
                if (prefab != null && ctx.TryGetSpawnPoint(out Vector2 pos))
                    WaveManager.SpawnAmbushEnemy(prefab, pos, 0f, levelBonus);

                WaveManager.ConsumeReservation();
            }
        }

        private GameObject PickEnemy()
        {
            if (enemies == null) return null;

            int count = 0;
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i] != null) count++;

            if (count == 0) return null;

            int k = Random.Range(0, count);
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null) continue;
                if (k-- == 0) return enemies[i];
            }

            return null;
        }
    }
}
