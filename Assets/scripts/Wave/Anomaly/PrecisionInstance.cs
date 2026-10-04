using System.Collections.Generic;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

public class PrecisionInstance : AnomalyInstance
{
    public float threshold;

    private readonly Dictionary<AttackData, int> open = new();
    private readonly List<AttackData> closeBuffer = new();
    private readonly System.Action<AttackData> onCast;
    private readonly System.Action<AttackData> onHit;
    private readonly string baseDesc;
    private string desc;
    private int casts;
    private int landed;
    private int dCasts = -1;
    private int dLanded = -1;
    private bool hooked;

    public PrecisionInstance(AnomalyData data) : base(data)
    {
        threshold = Mathf.Round(data.anomalyMinVal < data.anomalyMaxVal ? Random.Range(data.anomalyMinVal, data.anomalyMaxVal) : data.anomalyValue);
        baseDesc = $"Land at least {threshold}% of your attacks this wave. Fails at wave end if your accuracy is under the threshold";
        desc = baseDesc;
        onCast = OnCast;
        onHit = OnHit;
    }

    public float Accuracy => casts > 0 ? landed * 100f / casts : 100f;

    public override string Description
    {
        get
        {
            if (!isActive) return baseDesc;
            if (casts == dCasts && landed == dLanded) return desc;

            dCasts = casts;
            dLanded = landed;
            desc = casts > 0
                ? $"Land at least {threshold}% of your attacks. Accuracy: {Mathf.FloorToInt(Accuracy)}% ({landed}/{casts})"
                : $"Land at least {threshold}% of your attacks. Accuracy: -";
            return desc;
        }
    }

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        Clear();

        if (hooked) return;
        PlayerAttackHandler.AttackCast += onCast;
        Projectile.PlayerHit += onHit;
        hooked = true;
    }

    private static bool Deals(AttackData ad)
    {
        if (ad == null || ad.ProjectilePrefab == null || ad.Pd == null) return false;
        ProjectileData pd = ad.Pd;
        return pd.PhysicalMult + pd.SpellMult + pd.TrueMult + pd.SpecialMult > 0f;
    }

    private void OnCast(AttackData ad)
    {
        if (!isActive || ad == null) return;

        AttackData ca = ad.ChargeAttack;
        if (!Deals(ad) && !(ad.CanCharge && Deals(ca))) return;

        casts++;
        open[ad] = casts;
        if (ca != null) open[ca] = casts;
    }

    private void OnHit(AttackData root)
    {
        if (!isActive || root == null || !open.TryGetValue(root, out int id)) return;

        landed++;

        closeBuffer.Clear();
        foreach (var kv in open)
            if (kv.Value == id) closeBuffer.Add(kv.Key);
        for (int i = 0; i < closeBuffer.Count; i++) open.Remove(closeBuffer[i]);
        closeBuffer.Clear();
    }

    public override void OnWaveEnd()
    {
        if (isActive && casts > 0 && landed * 100f < threshold * casts) FailAnomaly();
    }

    private void Clear()
    {
        open.Clear();
        closeBuffer.Clear();
        casts = 0;
        landed = 0;
        dCasts = -1;
        dLanded = -1;
        desc = baseDesc;
    }

    public override void ResetForWave()
    {
        if (hooked)
        {
            PlayerAttackHandler.AttackCast -= onCast;
            Projectile.PlayerHit -= onHit;
            hooked = false;
        }

        Clear();
        base.ResetForWave();
    }
}
