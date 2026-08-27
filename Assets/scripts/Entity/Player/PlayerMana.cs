using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    private EntityStatManager esm;

    private void Start()
    {
        esm = GetComponent<EntityStatManager>();

        if (esm != null) esm.AddStat(new(StatType.CanGainMana, 1));
    }
    public void ChangeMana(float amount, float pctAmt = 0)
    {
        if (esm == null || esm.GetStat(StatType.isAlive) <= 0f || (amount > 0 || pctAmt > 0) && esm.GetStat(StatType.CanGainMana) <= 0f) return;
        float manaGainMultiplier = 1f + (esm.GetStat(StatType.manaGainPct) * 0.01f);
        float finalAmount = amount * manaGainMultiplier;
        float finalPctAmt = pctAmt * manaGainMultiplier;
        esm.AddStat(new(StatType.CurrentMana, finalAmount + (finalPctAmt * 0.01f * esm.GetStat(StatType.maxMana))));
    }
}