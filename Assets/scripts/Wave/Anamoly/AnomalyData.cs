using UnityEngine;

public enum AnomalyType { TimeTrial, NoDamage, ResourceRestriction, ComboMastery }

[CreateAssetMenu(fileName = "amd", menuName = "Data/Anomaly")]
public class AnomalyData : ScriptableObject
{
    public string anamolyName;
    [TextArea(3, 10)] public string desc;
    public int minWave;
    public AnomalyType anamolyType;
    public float anamolyVal;
    public AnomalyInstance CreateInstance()
    {
        return anamolyType switch
        {
            AnomalyType.TimeTrial => new TimeTrialInstance(this),
            _ => new AnomalyInstance(this)
        };
    }
}