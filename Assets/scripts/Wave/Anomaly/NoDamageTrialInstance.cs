using CrystalFlux.Core;
using UnityEngine;

public class NoDamageTrialInstance : AnomalyInstance
{
    public NoDamageTrialInstance(AnomalyData data) : base(data) {}

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        PlayerEvents.OnPlayerTakeDamage += OnPlayerDamaged;
    }

    public override void Cleanup()
    {
        PlayerEvents.OnPlayerTakeDamage -= OnPlayerDamaged;
        base.Cleanup();
    }

    private void OnPlayerDamaged(IDamageable player)
    {
        if (isActive) FailAnomaly();
    }
}
