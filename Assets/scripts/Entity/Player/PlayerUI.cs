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

        private float lastMana = -1f;
        private float lastMaxMana = -1f;
        private float lastHp = -1f;
        private float lastMaxHp = -1f;
        private float lastStamina = -1f;
        private float lastMaxStamina = -1f;
        private int lastLevel = -1;
        private float lastXp = -1;
        private float lastOverhealth = -1f;
        private IStatProvider esm;
        private EntityHealth eh;
        private float Overhealth => eh != null ? Mathf.Floor(eh.Overhealth) : 0f;
        private float CurMana => Mathf.Round(esm.GetStat(StatType.CurrentMana));
        private float CurHp => Mathf.Round(esm.GetStat(StatType.currentHp));
        private float CurStamina => Mathf.Round(esm.GetStat(StatType.CurrentStamina));
        private float MaxMana => Mathf.Round(esm.GetStat(StatType.EffMaxMana));
        private float MaxHp => Mathf.Round(esm.GetStat(StatType.EffMaxHp));
        private float MaxStamina => Mathf.Round(esm.GetStat(StatType.EffMaxStamina));
        private int Level => Mathf.RoundToInt(esm.GetStat(StatType.Level));
        private float Xp => Mathf.Round(esm.GetStat(StatType.Xp));
        private float XpReq => Mathf.Round(esm.GetStat(StatType.XpReq));
        private bool Alive => Mathf.RoundToInt(esm.GetStat(StatType.isAlive)) > 0;

        private void Start()
        {
            esm ??= GetComponent<IStatProvider>();
            eh ??= GetComponent<EntityHealth>();
            UpdateUI();

            if (pui != null) pui.Setup(esm);
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

        private static void SetBar(Slider bar, TextMeshProUGUI label, float value, float max, float over = 0f)
        {
            if (bar != null)
            {
                bar.maxValue = max;
                bar.value = value;
            }

            if (label != null) label.text = NumberFormat.Bar(value, max, over);
        }

        private void UpdateManaBar()
        {
            if (CurMana == lastMana && MaxMana == lastMaxMana) return;

            SetBar(manaBar, manaText, CurMana, MaxMana);

            lastMana = CurMana;
            lastMaxMana = MaxMana;
        }
        private void UpdateHealthBar()
        {
            if (!Alive)
            {
                if (healthUI == null || healthUI.value != 0)
                    SetBar(healthUI, healthText, 0, MaxHp);

                lastHp = 0;
                lastMaxHp = MaxHp;
                lastOverhealth = 0;
                return;
            }

            if (CurHp == lastHp && MaxHp == lastMaxHp && Overhealth == lastOverhealth) return;

            SetBar(healthUI, healthText, CurHp, MaxHp, Overhealth);

            lastHp = CurHp;
            lastMaxHp = MaxHp;
            lastOverhealth = Overhealth;
        }
        private void UpdateStaminaBar()
        {
            if (!Alive || (CurStamina == lastStamina && MaxStamina == lastMaxStamina)) return;

            SetBar(staminaUI, staminaText, CurStamina, MaxStamina);

            lastStamina = CurStamina;
            lastMaxStamina = MaxStamina;
        }
        private void UpdateXpBar()
        {
            if (!Alive || (Level == lastLevel && Mathf.Abs(Xp - lastXp) < 0.01f)) return;

            if (xpBar != null)
            {
                xpBar.maxValue = XpReq;
                xpBar.value = Xp;
            }

            if (levelText != null) levelText.text = $"Lv.{Level}";
            if (xpText != null) xpText.text = NumberFormat.Bar(Xp, XpReq);

            lastLevel = Level;
            lastXp = Xp;
        }
    }
}
