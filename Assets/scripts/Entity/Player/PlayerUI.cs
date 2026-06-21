using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [HideInInspector] public PlayerStats p;
    [Header("Levelling")]
    public Slider xpBar;
    public TextMeshProUGUI xpBarText;
    public TextMeshProUGUI levelText;
    private int lastXp = -1;
    private int lastMaxXp = -1;
    private int lastLevel = -1;
    [Header("Health")]
    public Slider healthUI;
    public TextMeshProUGUI healthText;
    private int lastHp = -1;
    private int lastMaxHp = -1;
    [Header("Stamina")]
    public Slider staminaUI;
    public TextMeshProUGUI staminaText;
    private int lastStamina = -1;
    private int lastMaxStamina = -1;

    private void Start()
    {
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;
        UpdateUI();
    }
    private void Update() => UpdateUI();
    private void UpdateUI()
    {
        if (p == null) return;
        UpdateXpBar();
        UpdateHealthBar();
        UpdateStaminaBar();
    }
    private void UpdateXpBar()
    {
        int maxXp = (p.level * p.level * 100) + (p.level * 150);
        if (p.currentXp == lastXp && maxXp == lastMaxXp && p.level == lastLevel) return;

        xpBar.maxValue = maxXp;
        xpBar.value = p.currentXp;

        xpBarText.text = $"{p.currentXp}/{maxXp}";
        levelText.text = $"{p.level}";

        lastXp = p.currentXp;
        lastMaxXp = maxXp;
        lastLevel = p.level;
    }
    private void UpdateHealthBar()
    {
        if (!p.isAlive)
        {
            if (healthUI.value != 0)
            {
                healthUI.maxValue = p.maxHp;
                healthUI.value = 0;
                healthText.text = $"0/{p.maxHp}";
            }

            lastHp = 0;
            lastMaxHp = p.maxHp;
            return;
        }

        if (p.currentHp == lastHp && p.maxHp == lastMaxHp) return;

        healthUI.maxValue = p.maxHp;
        healthUI.value = p.currentHp;
        healthText.text = $"{p.currentHp}/{p.maxHp}";

        lastHp = p.currentHp;
        lastMaxHp = p.maxHp;
    }
    private void UpdateStaminaBar()
    {
        if (!p.isAlive) return;

        if (p.currentStamina == lastStamina && p.maxStamina == lastMaxStamina) return;

        staminaUI.maxValue = p.maxStamina;
        staminaUI.value = p.currentStamina;
        staminaText.text = $"{p.currentStamina}/{p.maxStamina}";

        lastStamina = p.currentStamina;
        lastMaxStamina = p.maxStamina;
    }
}