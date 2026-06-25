using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerStamina))]
public class PlayerUI : MonoBehaviour
{
    public GameObject resourceHoverZone;
    [HideInInspector] public PlayerStats p;
    [Header("Levelling")]
    public Slider manaBar;
    public TextMeshProUGUI manaText;
    private int lastMana = -1;
    private int lastMaxMana = -1;
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
        if (p != null) Initialize(p);
    }
    public void Initialize(PlayerStats stats)
    {
        if (resourceHoverZone.TryGetComponent<TooltipTrigger>(out var trigger))
            trigger.SetupTooltipData(stats);
    }
    private void Update() => UpdateUI();
    private void UpdateUI()
    {
        if (p == null) return;
        UpdateManaBar();
        UpdateHealthBar();
        UpdateStaminaBar();
    }
    private void UpdateManaBar()
    {
        if (p.currentMana == lastMana && p.maxMana == lastMaxMana) return;

        manaBar.maxValue = p.maxMana;
        manaBar.value = p.currentMana;

        manaText.text = $"{p.currentMana}/{p.maxMana}";

        lastMana = p.currentMana;
        lastMaxMana = p.maxMana;
    }
    private void UpdateHealthBar()
    {
        if (!p.isAlive)
        {
            if (healthUI.value != 0)
            {
                healthUI.maxValue = p.EffMaxHp;
                healthUI.value = 0;
                healthText.text = $"0/{p.EffMaxHp}";
            }

            lastHp = 0;
            lastMaxHp = p.EffMaxHp;
            return;
        }

        if (p.currentHp == lastHp && p.EffMaxHp == lastMaxHp) return;

        healthUI.maxValue = p.EffMaxHp;
        healthUI.value = p.currentHp;
        healthText.text = $"{p.currentHp}/{p.EffMaxHp}";

        lastHp = p.currentHp;
        lastMaxHp = p.EffMaxHp;
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