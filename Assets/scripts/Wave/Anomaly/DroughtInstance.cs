using CrystalFlux.EntitySystem;

public class DroughtInstance : AnomalyInstance
{
    public DroughtInstance(AnomalyData data) : base(data) { }

    public override string Description => "Collectibles do not spawn this wave, and your health and stamina do not naturally regenerate";

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        PlayerResourcePool.RegenLocked = true;
    }

    public override void ResetForWave()
    {
        PlayerResourcePool.RegenLocked = false;
        base.ResetForWave();
    }
}
