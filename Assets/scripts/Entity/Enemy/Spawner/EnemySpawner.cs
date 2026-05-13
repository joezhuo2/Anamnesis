using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawner", menuName = "ScriptableObjects/EnemySpawner")]
public class EnemySpawner : ScriptableObject
{
    public GameObject enemyToSpawn;
    public int maxEnemies = 1;
    public float interval;
    public float randomness; 
    public float spawnRadius;
    public float activationRadius;
}