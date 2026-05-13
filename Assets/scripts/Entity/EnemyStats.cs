using UnityEngine;

[CreateAssetMenu(fileName = "enemy_stats", menuName = "Scriptable Objects/stats/enemy_stats")]
public class EnemyStats : Stats
{
    public bool facingRight;
    public float detectionRange;
}
