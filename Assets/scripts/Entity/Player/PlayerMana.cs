using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [HideInInspector] public PlayerStats p;

    private void Start()
    {
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;

        p.canGainMana = true;
    }
    public void ChangeMana(float amount, float pctAmt = 0)
    {
        if ((amount > 0 || pctAmt > 0) && !p.canGainMana || p == null || !p.isAlive) return;
        float manaGainMultiplier = 1f + (p.manaGainPct * 0.01f);
        float finalAmount = amount * manaGainMultiplier;
        float finalPctAmt = pctAmt * manaGainMultiplier;
        p.currentMana = Math.Min(
            Mathf.RoundToInt(p.currentMana + finalAmount + (finalPctAmt * 0.01f * p.EffMaxMana)),
            Mathf.RoundToInt(p.EffMaxMana)
        );
    }
}