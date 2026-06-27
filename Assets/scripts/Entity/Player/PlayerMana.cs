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
        if ((amount > 0 || pctAmt > 0) && !p.canGainMana) return;
        p.currentMana = Math.Min(Mathf.RoundToInt(p.currentMana + (amount + (pctAmt * 0.01f * p.maxMana))), p.maxMana);
    }
}