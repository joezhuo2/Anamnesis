using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AttackData cad;
    private PlayerStats cps;

    public void SetupTooltipData(AttackData ad, PlayerStats ps)
    {
        cad = ad;
        cps = ps;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowAttackTooltip();
    }
    public void ShowAttackTooltip()
    {
        if (cad == null || cps == null) return;

        float staminaCost = Mathf.Abs(cad.staminaCost + (cps.maxStamina * (cad.staminaCostPct * 0.01f)));
        float manaCost = Mathf.Abs(cad.manaCost + (cps.maxMana * (cad.manaCostPct * 0.01f)));
        float hpCost = Mathf.Abs(cad.healthCost + (cps.EffMaxHp * (cad.healthCostPct * 0.01f)));
        float cooldown = cad.cooldown * Mathf.Clamp(1f - (cps.attackSpeedPct * 0.01f), 0.1f, 10f);

        string details = "";
        if (cooldown != 0f) details += $"Cooldown: {cooldown:F1}s\n";
        if (hpCost != 0f) details += $"Health Cost: {hpCost:F0}\n";
        if (staminaCost != 0f) details += $"Stamina Cost: {staminaCost:F0}\n";
        if (manaCost != 0f) details += $"Mana Cost: {manaCost:F0}\n";

        string title = cad.displayName;

        TooltipUI.Instance.ShowTooltip(title, details);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }
}