using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class EnemySpawner : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSpawnHook() => EnemySpawning.Spawn = SpawnEnemy;

        public static GameObject SpawnEnemy(GameObject prefab, Vector2 location, float radius, int level)
        {
            Vector2 spawnPosition = location + (Random.insideUnitCircle * radius);
            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);

            if (enemy.TryGetComponent<EnemyStatManager>(out var esm)) esm.ScaleStatsToLevel(level);

            return enemy;
        }
    }
}
