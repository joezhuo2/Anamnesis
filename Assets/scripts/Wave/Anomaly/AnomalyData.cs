using UnityEngine;

public enum AnomalyType { TimeTrial, NoDamage, StatModifier, Swarm, Duel }

[CreateAssetMenu(fileName = "amd", menuName = "Data/Anomaly")]
public class AnomalyData : ScriptableObject
{
    public string anomalyName;
    [TextArea(3, 10)] public string desc;
    public int minWave;
    public int maxWave;
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
            _ => new AnomalyInstance(this)
        };
    }
}
