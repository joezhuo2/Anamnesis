using CrystalFlux.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public class PlayerUI : MonoBehaviour
    {
        public PlayerResourceUI pui;

        [Header("Mana")]
        public Slider manaBar;
        public TextMeshProUGUI manaText;

        [Header("Health")]
        public Slider healthUI;
        public TextMeshProUGUI healthText;

        [Header("Stamina")]
        public Slider staminaUI;
        public TextMeshProUGUI staminaText;

        [Header("Levelling")]
        public Slider xpBar;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI xpText;

        private int lastMana = -1;
        private int lastMaxMana = -1;
        private int lastHp = -1;
        private int lastMaxHp = -1;
        private int lastStamina = -1;
        private int lastMaxStamina = -1;
        private int lastLevel = -1;
        private float lastXp = -1;
        private IStatProvider esm;
        private ICurrencyHolder ich;
        private int CurMana => Mathf.RoundToInt(esm.GetStat(StatType.CurrentMana));
        private int CurHp => Mathf.RoundToInt(esm.GetStat(StatType.currentHp));
        private int CurStamina => Mathf.RoundToInt(esm.GetStat(StatType.CurrentStamina));
        private int MaxMana => Mathf.RoundToInt(esm.GetStat(StatType.EffMaxMana));
        private int MaxHp => Mathf.RoundToInt(esm.GetStat(StatType.EffMaxHp));
        private int MaxStamina => Mathf.RoundToInt(esm.GetStat(StatType.EffMaxStamina));
        private int Level => Mathf.RoundToInt(esm.GetStat(StatType.Level));
        private int Xp => Mathf.RoundToInt(esm.GetStat(StatType.Xp));
        private int XpReq => Mathf.RoundToInt(esm.GetStat(StatType.XpReq));
        private bool Alive => Mathf.RoundToInt(esm.GetStat(StatType.isAlive)) > 0;

        private void Start()
        {
            esm ??= GetComponent<IStatProvider>();
            ich ??= GetComponent<ICurrencyHolder>();
            UpdateUI();

            if (pui != null) pui.Setup(esm, ich);
        }
        private void Update() => UpdateUI();
        private void UpdateUI()
        {
            if (esm == null) return;
            UpdateManaBar();
            UpdateHealthBar();
            UpdateXpBar();
            UpdateStaminaBar();
        }

        private void UpdateManaBar()
        {
            if (CurMana == lastMana && MaxMana == lastMaxMana) return;

            manaBar.maxValue = MaxMana;
            manaBar.value = CurMana;
            manaText.text = $"{CurMana}/{MaxMana}";

            lastMana = CurMana;
            lastMaxMana = MaxMana;
        }
        private void UpdateHealthBar()
        {
            if (!Alive)
            {
                if (healthUI.value != 0)
                {
                    healthUI.maxValue = MaxHp;
                    healthUI.value = 0;
                    healthText.text = $"0/{MaxHp}";
                }

                lastHp = 0;
                lastMaxHp = MaxHp;
                return;
            }

            if (CurHp == lastHp && MaxHp == lastMaxHp) return;

            healthUI.maxValue = MaxHp;
            healthUI.value = CurHp;
            healthText.text = $"{CurHp}/{MaxHp}";

            lastHp = CurHp;
            lastMaxHp = MaxHp;
        }
        private void UpdateStaminaBar()
        {
            if (!Alive || (CurStamina == lastStamina && MaxStamina == lastMaxStamina)) return;

            staminaUI.maxValue = MaxStamina;
            staminaUI.value = CurStamina;
            staminaText.text = $"{CurStamina}/{MaxStamina}";

            lastStamina = CurStamina;
            lastMaxStamina = MaxStamina;
        }
        private void UpdateXpBar()
        {
            if (!Alive || (Level == lastLevel && Mathf.Abs(Xp - lastXp) < 0.01f)) return;

            xpBar.maxValue = XpReq;
            xpBar.value = Xp;
            levelText.text = $"Lv.{Level}";
            xpText.text = $"{Xp:F0}/{XpReq:F0}";

            lastLevel = Level;
            lastXp = Xp;
        }
    }
}
