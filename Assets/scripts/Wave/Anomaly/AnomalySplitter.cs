using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

public class AnomalySplitter : MonoBehaviour
{
    private const float splitRadius = 0.5f;

    private GameObject prefab;
    private int level;
    private float splitChance;
    private int minSplits;
    private int maxSplits;
    private int levelReduction;
    private EntityHealth health;

    public void Setup(GameObject enemyPrefab, int enemyLevel, float chance, int min, int max, int lvReduction)
    {
        Unsubscribe();

        prefab = enemyPrefab;
        level = enemyLevel;
        splitChance = chance;
        minSplits = Mathf.Max(1, min);
        maxSplits = Mathf.Max(minSplits, max);
        levelReduction = Mathf.Max(0, lvReduction);

        if (TryGetComponent<EntityHealth>(out health)) health.OnDeath += HandleDeath;
    }

    private void HandleDeath(GameObject dead)
    {
        Unsubscribe();

        if (prefab == null || !WaveManager.WaveActive) return;
        if (Random.Range(0f, 100f) >= splitChance) return;

        int count = Random.Range(minSplits, maxSplits + 1);
        int splitLevel = Mathf.Max(1, level - levelReduction);
        Vector2 pos = dead != null ? dead.transform.position : transform.position;

        for (int i = 0; i < count; i++)
        {
            GameObject clone = EnemySpawning.SpawnEnemy(prefab, pos, splitRadius, splitLevel);
            WaveManager.RegisterSplitEnemy(clone);
        }
    }

    private void Unsubscribe()
    {
        if (health != null) health.OnDeath -= HandleDeath;
        health = null;
    }

    private void OnDestroy()
    {
        Unsubscribe();
        prefab = null;
    }
}
