using UnityEngine;

public enum AnomalyType { TimeTrial, NoDamage, ResourceRestriction, ComboMastery }

[CreateAssetMenu(fileName = "amd", menuName = "Data/Anomaly")]
public class AnomalyData : ScriptableObject
{
    public string anomalyName;
    [TextArea(3, 10)] public string desc;
    public int minWave;
    public int maxWave;
    public AnomalyType anomalyType;
    public float anomalyValue;

    public AnomalyInstance CreateInstance()
    {
        return anomalyType switch
        {
            AnomalyType.TimeTrial => new TimeTrialInstance(this),
            AnomalyType.NoDamage => new NoDamageTrialInstance(this),
            _ => new AnomalyInstance(this)
        };
    }
}