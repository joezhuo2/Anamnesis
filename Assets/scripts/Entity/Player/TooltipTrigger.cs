using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipDisplay
{
    private string title;
    private string subtitle;
    private Vector2 offset;

    private void Awake()
    {
        title = "";
        subtitle = "";
        offset = Vector2.zero;
    }

    public void OnPointerEnter(PointerEventData eventData)
        => TooltipUI.Instance?.ShowTooltip(title, subtitle, offset);

    public void OnPointerExit(PointerEventData eventData) => HideTooltip();
    private void OnDisable() => HideTooltip();
    public void HideTooltip() => TooltipUI.Instance?.HideTooltip();

    public void ShowTooltip(string title, string description, Vector2 offset = default)
    {
        this.title = title;
        this.subtitle = description;
        this.offset = offset;
    }
}