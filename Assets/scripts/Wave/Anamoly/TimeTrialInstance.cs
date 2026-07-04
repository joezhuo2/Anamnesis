using UnityEngine;

public class TimeTrialInstance : AnomalyInstance
{
    public float timeRemaining;

    public TimeTrialInstance(AnomalyData data) : base(data)
    {
        timeRemaining = data.anomalyValue;
    }

    public override void StartAnomaly()
    {
        base.StartAnomaly();

        // TODO: Optional - Hook into a UI element or overlay to display a countdown timer to the player
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

    public override void FailAnomaly()
    {
        base.FailAnomaly();

        // TODO: Optional penalty logic (e.g., hurt the player, buff remaining enemies, or just deny rewards)
        // Since WaveManager checks (currentAnamoly != null && currentAnamoly.isActive) at EndWave,
        // setting isActive = false here guarantees they don't get the bonus rewards screen!
    }

    public override void CompleteAnomaly()
    {
        if (timeRemaining > 0f)
            base.CompleteAnomaly();
    }
}