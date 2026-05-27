using UnityEngine;

[CreateAssetMenu(fileName = "enemy_stats", menuName = "Scriptable Objects/stats/enemy_stats")]
public class EnemyStats : EntityStats
{
    public int enemyDirection;
    public float detectionRange;
    public GameObject target;
}
