using UnityEngine;

public class NoDamageTrialInstance : AnomalyInstance
{
    public NoDamageTrialInstance(AnomalyData data) : base(data) {}

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        EntityHealth.OnPlayerTakeDamage += OnPlayerDamaged;
    }

    public override void Cleanup()
    {
        EntityHealth.OnPlayerTakeDamage -= OnPlayerDamaged;
        base.Cleanup();
    }

    private void OnPlayerDamaged(EntityHealth eh)
    {
        FailAnomaly();
    }
}