using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class EntitySplitting : MonoBehaviour
    {
        public GameObject prefab;
        public int splitLevel;
        [Tooltip("Chance (0-1) to split on death, matching summonChance and EffectData.chance. 0 = never.")]
        [Range(0f, 1f)] public float splitChance = 1f;
        public int minSplits;
        public int maxSplits;
        public float splitRadius;

        public void Split()
        {
            if (Random.value <= splitChance)
            {
                int numSplits = Random.Range(minSplits, maxSplits + 1);
                for (int i = 0; i < numSplits; i++)
                    EnemySpawner.SpawnEnemy(prefab, (Vector2)transform.position, splitRadius, splitLevel);
            }
        }
    }
}
