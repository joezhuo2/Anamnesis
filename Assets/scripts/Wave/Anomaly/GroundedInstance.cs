using CrystalFlux.EntitySystem;

public class GroundedInstance : AnomalyInstance
{
    public GroundedInstance(AnomalyData data) : base(data) {}

    public override string Description => "Fails if you press dash";

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        PlayerMovement.OnDashPressed += OnDashPressed;
    }

    public override void ResetForWave()
    {
        PlayerMovement.OnDashPressed -= OnDashPressed;
        base.ResetForWave();
    }

    private void OnDashPressed()
    {
        if (isActive) FailAnomaly();
    }
}
