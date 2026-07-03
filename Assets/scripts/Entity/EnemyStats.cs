using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Data/Enemystats")]
public class EnemyStats : EntityStats
{
    public float detectionRange;
    public GameObject target;
    [HideInInspector] public int phase;
    [Tooltip("HP% thresholds (descending) that advance phase. E.g. [75, 50, 25]")] public float[] phaseThresholds;
}
