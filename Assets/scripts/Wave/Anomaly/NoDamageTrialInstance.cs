using CrystalFlux.Core;
using UnityEngine;

public class NoDamageTrialInstance : AnomalyInstance
{
    public NoDamageTrialInstance(AnomalyData data) : base(data) {}

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        PlayerEvents.OnPlayerDamaged += OnPlayerDamaged;
    }

    public override void Cleanup()
    {
        PlayerEvents.OnPlayerDamaged -= OnPlayerDamaged;
        base.Cleanup();
    }

    private void OnPlayerDamaged(IDamageable player)
    {
        if (isActive) FailAnomaly();
    }
}
