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
        private int lastOverhealth = -1;
        private IStatProvider esm;
        private ICurrencyHolder ich;
        private ISkillPointHolder isph;
        private EntityHealth eh;
        private int Overhealth => eh != null ? Mathf.FloorToInt(eh.Overhealth) : 0;
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
            isph ??= GetComponent<ISkillPointHolder>();
            eh ??= GetComponent<EntityHealth>();
            UpdateUI();

            if (pui != null) pui.Setup(esm, ich, isph);
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

        private static void SetBar(Slider bar, TextMeshProUGUI label, int value, int max, int over = 0)
        {
            if (bar != null)
            {
                bar.maxValue = max;
                bar.value = value;
            }

            if (label != null) label.text = over > 0 ? $"{value+over}/{max}" : $"{value}/{max}";
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
            if (xpText != null) xpText.text = $"{Xp:F0}/{XpReq:F0}";

            lastLevel = Level;
            lastXp = Xp;
        }
    }
}
