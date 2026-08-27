using System;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    private float regenTimer = 0f;
    private readonly float ri = 0.2f;
    private readonly float frf = 5f;
    private float accumaltedRegen = 0f;
    private EntityStatManager esm;

    private void Start()
    {
        esm = GetComponent<EntityStatManager>();

        if (esm != null) esm.AddStat(new(StatType.CanGainStamina, 1));
    }

    public void Update()
    {
        if (esm != null && esm.GetStat(StatType.isAlive) > 0f) RegenStamina();
    }

    public void ChangeStamina(float amount, float pctAmt = 0)
    {
        if ((amount > 0 || pctAmt > 0) && esm.GetStat(StatType.CanGainStamina) <= 0f) return;

        int newStamina = Math.Min(
            Mathf.RoundToInt(esm.GetStat(StatType.CurrentStamina) + amount + (pctAmt * esm.GetStat(StatType.EffMaxStamina))),
            Mathf.RoundToInt(esm.GetStat(StatType.EffMaxStamina)
        ));
        int targetChange = newStamina - Mathf.RoundToInt(esm.GetStat(StatType.CurrentStamina));
        if (targetChange > amount) targetChange = Mathf.RoundToInt(amount);
        esm.AddStat(new(StatType.CurrentStamina, targetChange));
    }

    public void RegenStamina()
    {
        if (esm == null || esm.GetStat(StatType.isAlive) <= 0f) return; 
        if (esm.GetStat(StatType.CurrentStamina) >= esm.GetStat(StatType.EffMaxStamina)) return;
        if (esm.GetStat(StatType.CanGainStamina) <= 0f || esm.GetStat(StatType.EffStReg) == 0) return;

        regenTimer += Time.deltaTime;

        if (regenTimer >= ri)
        {
            float regenPerTick = esm.GetStat(StatType.EffStReg) / frf * ri;

            accumaltedRegen += regenPerTick;

            if (accumaltedRegen >= 1f)
            {
                int intRegen = Mathf.FloorToInt(accumaltedRegen);
                accumaltedRegen -= intRegen;
                ChangeStamina(intRegen);
            }
            regenTimer -= ri;
        }
    }
}