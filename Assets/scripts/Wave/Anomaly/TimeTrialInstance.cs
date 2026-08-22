using UnityEngine;

public class TimeTrialInstance : AnomalyInstance
{
    public float timeRemaining;

    public TimeTrialInstance(AnomalyData data) : base(data)
    {
        timeRemaining = data.anomalyValue;
    }
    public override void UpdateCheck(float dt)
    {
        if (!isActive || timeRemaining <= 0f) return;

        timeRemaining -= dt;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            FailAnomaly();
        }
    }

    public override void CompleteAnomaly()
    {
        if (timeRemaining > 0f)
            base.CompleteAnomaly();
    }
}