using CrystalFlux.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CrystalFlux.UISystem
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipDisplay
    {
        private string title;
        private string subtitle;
        private Vector2 offset;
        private bool hovered;

        private void Awake()
        {
            title = "";
            subtitle = "";
            offset = Vector2.zero;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            TooltipUI.Instance?.ShowTooltip(title, subtitle, offset);
        }

        public void OnPointerExit(PointerEventData eventData) => HideTooltip();
        private void OnDisable() => HideTooltip();
        public void HideTooltip()
        {
            hovered = false;
            if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
        }

        public void ShowTooltip(string title, string description, Vector2 offset = default)
        {
            bool changed = this.title != title || this.subtitle != description || this.offset != offset;

            this.title = title;
            this.subtitle = description;
            this.offset = offset;

            if (hovered && changed && TooltipUI.Instance != null) TooltipUI.Instance.ShowTooltip(title, description, offset);
        }
    }
}
