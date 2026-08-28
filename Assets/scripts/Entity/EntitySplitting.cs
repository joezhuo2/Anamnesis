using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class EntitySplitting : MonoBehaviour
    {
        public GameObject prefab;
        public int splitLevel;
        public float splitChance = 1f;
        public int minSplits;
        public int maxSplits;
        public float splitRadius;

        public void Split()
        {
            if (Random.value < splitChance * 0.01f)
            {
                int numSplits = Random.Range(minSplits, maxSplits + 1);
                for (int i = 0; i < numSplits; i++)
                    EnemySpawner.SpawnEnemy(prefab, (Vector2)transform.position, splitRadius, splitLevel);
            }
        }
    }
}
