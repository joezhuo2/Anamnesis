using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using UnityEngine;

public class SealedInstance : AnomalyInstance
{
    private static readonly AttackType[] Slots = { AttackType.Basic, AttackType.Skill, AttackType.Ultimate };

    public AttackType sealedSlot;
    public float cdrAmount;

    private readonly string description;
    private readonly List<StatBuff> appliedBuffs = new();
    private PlayerAttackHandler cpah;
    private IStatProvider cpsm;

    public SealedInstance(AnomalyData data) : base(data)
    {
        GameObject p = GameObject.FindWithTag("Player");
        IAttackHandler iah = p != null ? p.GetComponent<IAttackHandler>() : null;

        List<AttackType> owned = new(Slots.Length);
        for (int i = 0; i < Slots.Length; i++)
            if (iah == null || iah.FindAttackOfType(Slots[i]) != null) owned.Add(Slots[i]);
        if (owned.Count == 0) owned.AddRange(Slots);

        sealedSlot = owned[Random.Range(0, owned.Count)];
        cdrAmount = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal));
        description = $"Your {sealedSlot} attack is sealed for the wave, while your other attacks gain +{cdrAmount}% Cooldown Reduction";
    }

    public override string Description => description;

    private static StatType CdrStat(AttackType type) => type switch
    {
        AttackType.Basic => StatType.basicCdRedPct,
        AttackType.Skill => StatType.skillCdRedPct,
        _ => StatType.ultCdRedPct
    };

    public override void StartAnomaly()
    {
        base.StartAnomaly();

        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) return;

        cpah = p.GetComponent<PlayerAttackHandler>();
        cpsm = p.GetComponent<IStatProvider>();

        if (cpah != null) cpah.SetSlotLocked(sealedSlot, true);
        if (cpsm == null || cdrAmount == 0f) return;

        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == sealedSlot) continue;

            StatBuff b = new(CdrStat(Slots[i]), cdrAmount);
            cpsm.AddStat(b);
            appliedBuffs.Add(b);
        }
    }

    public override void ResetForWave()
    {
        if (cpah != null) cpah.SetSlotLocked(sealedSlot, false);

        if ((cpsm as Object) != null)
            for (int i = 0; i < appliedBuffs.Count; i++) cpsm.AddStat(appliedBuffs[i], false);

        appliedBuffs.Clear();
        cpah = null;
        cpsm = null;
        base.ResetForWave();
    }
}
