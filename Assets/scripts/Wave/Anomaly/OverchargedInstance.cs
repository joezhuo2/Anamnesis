using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using UnityEngine;

public class OverchargedInstance : AnomalyInstance
{
    private static readonly StatType[] CdStats = { StatType.basicCdRedPct, StatType.skillCdRedPct, StatType.ultCdRedPct };

    public float cdrAmount;
    public float costPct;

    private readonly string description;
    private readonly List<StatBuff> appliedBuffs = new();
    private IStatProvider cpsm;
    private bool costApplied;

    public OverchargedInstance(AnomalyData data) : base(data)
    {
        cdrAmount = Mathf.Round(data.anomalyMinVal < data.anomalyMaxVal ? Random.Range(data.anomalyMinVal, data.anomalyMaxVal) : data.anomalyMinVal);
        costPct = Mathf.Round(data.anomalyValue);
        description = $"Your attacks gain +{cdrAmount}% Cooldown Reduction, but every cast costs {costPct}% more resources";
    }

    public override string Description => description;

    public override void StartAnomaly()
    {
        base.StartAnomaly();

        if (!costApplied)
        {
            PlayerAttackHandler.CostPct += costPct;
            costApplied = true;
        }

        if (cpsm != null || cdrAmount == 0f) return;

        GameObject p = GameObject.FindWithTag("Player");
        if (p == null || !p.TryGetComponent<IStatProvider>(out var psm)) return;

        cpsm = psm;
        for (int i = 0; i < CdStats.Length; i++)
        {
            StatBuff b = new(CdStats[i], cdrAmount);
            cpsm.AddStat(b);
            appliedBuffs.Add(b);
        }
    }

    public override void ResetForWave()
    {
        if (costApplied)
        {
            PlayerAttackHandler.CostPct -= costPct;
            costApplied = false;
        }

        if ((cpsm as Object) != null)
            for (int i = 0; i < appliedBuffs.Count; i++) cpsm.AddStat(appliedBuffs[i], false);

        appliedBuffs.Clear();
        cpsm = null;
        base.ResetForWave();
    }
}
