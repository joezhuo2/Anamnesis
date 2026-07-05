using UnityEngine;

public class AnomalyInstance
{
    public AnomalyData amd;
    public bool isActive;
    public bool isCompleted;

    public AnomalyInstance(AnomalyData data)
    {
        amd = data;
    }

    public virtual void StartAnomaly()
    {
        isActive = true;
        isCompleted = false;
    }

    public virtual void UpdateCheck(float dt) { }
    public virtual void FailAnomaly()
    {
        if (!isActive) return;
        isActive = false;
        isCompleted = false;
    }

    public virtual void CompleteAnomaly()
    {
        if (!isActive) return;
        isActive = false;
        isCompleted = true;
    }
    public virtual void Cleanup()
    {
        amd = null;
        isActive = false;
        isCompleted = false;
    }
}