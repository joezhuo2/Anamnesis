using System;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class PlayerResourcePool : MonoBehaviour, IResourcePool
    {
        private IStatProvider esm;
        private float regenTimer = 0f;
        private readonly float ri = 0.2f;
        private readonly float frf = 5f;
        private float accumaltedRegen = 0f;

        private void Start()
        {
            esm = GetComponent<IStatProvider>();

            if (esm != null) esm.AddStat(new(StatType.CanGainMana, 1));
            if (esm != null) esm.AddStat(new(StatType.CanGainStamina, 1));
        }

        public void Update() => RegenStamina();

        public bool TryGain(ResourceType type, float amount)
        {
            switch (type)
            {
                case ResourceType.Mana: return ChangeMana(amount);
                case ResourceType.Stamina: return ChangeStamina(amount);
                default: return false;
            }
        }

        public bool TrySpend(ResourceType type, float amount)
        {
            switch (type)
            {
                case ResourceType.Mana: return ChangeMana(-amount);
                case ResourceType.Stamina: return ChangeStamina(-amount);
                default: return false;
            }
        }

        public bool ChangeMana(float amount)
        {
            if (esm == null || esm.GetStat(StatType.isAlive) <= 0f) return false;
            if (amount > 0 && esm.GetStat(StatType.CanGainMana) <= 0f) return false;
            if (esm.GetStat(StatType.CurrentMana) + amount < 0f) return false;


            int newMana = Math.Min(
                Mathf.RoundToInt(esm.GetStat(StatType.CurrentMana) + amount),
                Mathf.RoundToInt(esm.GetStat(StatType.EffMaxMana))
            );
            int targetChange = newMana - Mathf.RoundToInt(esm.GetStat(StatType.CurrentMana));
            if (targetChange > amount) targetChange = Mathf.RoundToInt(amount);

            if (amount > 0) targetChange = Mathf.RoundToInt(targetChange * (1f + (esm.GetStat(StatType.manaGainPct) * 0.01f)));

            esm.AddStat(new(StatType.CurrentMana, targetChange));
            return true;
        }

        public bool ChangeStamina(float amount)
        {
            if (esm == null || esm.GetStat(StatType.isAlive) <= 0f) return false;
            if (amount > 0&& esm.GetStat(StatType.CanGainStamina) <= 0f) return false;
            if (esm.GetStat(StatType.CurrentStamina) + amount < 0f) return false;

            int newStamina = Math.Min(
                Mathf.RoundToInt(esm.GetStat(StatType.CurrentStamina) + amount),
                Mathf.RoundToInt(esm.GetStat(StatType.EffMaxStamina))
            );
            int targetChange = newStamina - Mathf.RoundToInt(esm.GetStat(StatType.CurrentStamina));
            if (targetChange > amount) targetChange = Mathf.RoundToInt(amount);
            esm.AddStat(new(StatType.CurrentStamina, targetChange));
            return true;
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
                    TryGain(ResourceType.Stamina, intRegen);
                }
                regenTimer -= ri;
            }
        }
    }
}
