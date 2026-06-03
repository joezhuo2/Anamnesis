using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnController : MonoBehaviour
{
    public EnemySpawner spawnerData;
    public List<GameObject> currentEnemies = new();
    public Transform player;
    private float spawnTimer;

    void Update()
    {
        currentEnemies.RemoveAll(e => e == null);

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= GetNextSpawnTime() && currentEnemies.Count < spawnerData.maxEnemies && IsPlayerNearby())
            SpawnEnemy();
    }

    private float GetNextSpawnTime()
    {
        float interval = spawnerData.interval;
        float randomness = spawnerData.randomness;
        if (randomness > 0)
        {
            interval += Random.Range(-randomness, randomness);
            interval = Mathf.Max(0, interval);
        }
        return interval;
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPos = (Vector2) transform.position + (Random.insideUnitCircle * spawnerData.spawnRadius);
        GameObject enemyInstance = Instantiate(spawnerData.enemyToSpawn, spawnPos, Quaternion.identity);

        currentEnemies.Add(enemyInstance);
        spawnTimer = 0f;
    }

    private bool IsPlayerNearby()
    {
        if (player == null) return false;
        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= spawnerData.activationRadius;
    }
}