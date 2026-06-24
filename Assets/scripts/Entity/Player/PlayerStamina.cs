using System;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    private float regenTimer = 0f;
    private readonly float regenInterval = 0.2f;
    private readonly float fullRegenFrequency = 5f;
    private float accumaltedRegen = 0f;
    [HideInInspector] public PlayerStats p;

    private void Start()
    {
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;

        p.canGainStamina = true;
    }
    public void Update()
    {
        if (p != null && p.isAlive)
            RegenStamina();
    }
    public void ChangeStamina(float amount, float pctAmt = 0)
    {
        if ((amount > 0 || pctAmt > 0) && !p.canGainStamina) return;
        p.currentStamina = Math.Min(Mathf.RoundToInt(p.currentStamina + (amount + (pctAmt * p.maxStamina))), p.maxStamina);
    }
    public void RegenStamina()
    {
        if (p == null || p.currentStamina >= p.maxStamina || p.EffStReg == 0) return;

        regenTimer += Time.deltaTime;

        if (regenTimer >= regenInterval)
        {
            float regenPerTick = p.EffStReg / fullRegenFrequency * regenInterval;

            accumaltedRegen += regenPerTick;

            if (accumaltedRegen >= 1f)
            {
                int intRegen = Mathf.FloorToInt(accumaltedRegen);
                accumaltedRegen -= intRegen;
                ChangeStamina(intRegen);
            }
            regenTimer -= regenInterval;
        }
    }
}