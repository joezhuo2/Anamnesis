using UnityEngine;
using System;

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
    }

    public void Update()
    {
        if (p.isAlive)
            RegenStamina();
    }
    public void ChangeStamina(int amount)
    {
        if (amount > 0 && !p.canGainStamina) return;
        p.currentStamina = Math.Min(p.currentStamina + amount, p.maxStamina);
    }
    public void RegenStamina()
    {
        if (p == null || p.currentStamina > p.maxStamina || p.staminaRegen == 0) return;

        regenTimer += Time.deltaTime;

        float regenPerTick = p.staminaRegen / fullRegenFrequency * regenInterval;

        accumaltedRegen += regenPerTick;

        if (accumaltedRegen >= 1f)
        {
            int intRegen = Mathf.FloorToInt(accumaltedRegen);
            accumaltedRegen -= intRegen;
            ChangeStamina(intRegen);
        }
    }
}