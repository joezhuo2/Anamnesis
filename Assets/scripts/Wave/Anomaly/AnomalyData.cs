using UnityEngine;

public enum AnomalyType { TimeTrial, NoDamage, StatModifier, Swarm, Duel, Split, Sealed, Blackout }

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
            _ => new AnomalyInstance(this)
        };
    }
}
