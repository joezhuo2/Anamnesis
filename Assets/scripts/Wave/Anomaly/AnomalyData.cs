using UnityEngine;

public enum AnomalyType { TimeTrial, NoDamage, StatModifier, Swarm, Duel, Split, Sealed, Blackout, Hivemind, Drought }

[CreateAssetMenu(fileName = "amd", menuName = "Data/Anomaly")]
public class AnomalyData : ScriptableObject
{
    public string anomalyName;
    [TextArea(3, 10)] public string desc;
    public int minWave;
    public int maxWave;
    [Tooltip("Minimum mode tier required for this anomaly to be offered. 0 = Simple, 1 = Expert, 2 = Master")] [Min(0)] public int minMode;
    public AnomalyType anomalyType;
    public float anomalyValue;
    public float anomalyMinVal;
    public float anomalyMaxVal;
    public bool disallowOnBossWave;

    [Header("Contract")]
    [Tooltip("Also offered in the run-start contract pool. A held contract removes every anomaly of the same AnomalyType from the anomaly pool")]
    public bool isContract;
    [Tooltip("Rerolls granted each wave the contract holds (0 on Ironman)")] [Min(0)] public int contractRerolls = 1;
    [Tooltip("Chance (0-100) for 1 skill point each wave the contract holds")] [Range(0f, 100f)] public float contractSkillPointChance = 10f;
    [Tooltip("Chance (0-100) for an extra mixed reward pool each wave the contract holds")] [Range(0f, 100f)] public float contractMixedPoolChance = 10f;

    public AnomalyInstance CreateInstance()
    {
        return anomalyType switch
        {
            AnomalyType.TimeTrial => new TimeTrialInstance(this),
            AnomalyType.NoDamage => new NoDamageTrialInstance(this),
            AnomalyType.StatModifier => new StatModifierInstance(this),
            AnomalyType.Swarm => new SwarmInstance(this),
            AnomalyType.Duel => new DuelInstance(this),
            AnomalyType.Split => new SplitInstance(this),
            AnomalyType.Sealed => new SealedInstance(this),
            AnomalyType.Blackout => new BlackoutInstance(this),
            AnomalyType.Hivemind => new HivemindInstance(this),
            AnomalyType.Drought => new DroughtInstance(this),
            _ => new AnomalyInstance(this)
        };
    }
}
