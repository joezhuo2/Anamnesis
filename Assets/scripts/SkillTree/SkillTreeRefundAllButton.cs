using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrystalFlux.SkillTree
{
    [RequireComponent(typeof(Button))]
    public class SkillTreeRefundAllButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        public SkillTreeManager manager;
        public SkillTreeUI treeUI;

        [Header("Visuals")]
        public Image backgroundImage;
        public Color availableColor = new(0.8f, 0.3f, 0.2f, 0.8f);
        public Color blockedColor = new(0.3f, 0.3f, 0.3f, 0.8f);
        public Vector2 tooltipOffset = new(100, -100);

        private Button btn;

        private void Awake()
        {
            btn = GetComponent<Button>();
            if (manager == null) manager = FindAnyObjectByType<SkillTreeManager>();
            if (treeUI == null) treeUI = GetComponentInParent<SkillTreeUI>();
            if (backgroundImage == null) TryGetComponent(out backgroundImage);
        }

        private void OnEnable()
        {
            if (btn != null)
            {
                btn.onClick.RemoveListener(HandleClick);
                btn.onClick.AddListener(HandleClick);
            }

            RefreshVisuals();
        }

        private void OnDisable()
        {
            if (btn != null) btn.onClick.RemoveListener(HandleClick);
            if (TryGetComponent<ITooltipDisplay>(out var td)) td.HideTooltip();
        }

        public void RefreshVisuals()
        {
            if (manager == null) return;

            var (canRefund, _) = manager.CanRefundAll();
            bool hidden = GameSettings.Current.ironmanMode;

            if (btn != null) btn.interactable = canRefund;
            if (backgroundImage != null) backgroundImage.color = canRefund ? availableColor : blockedColor;

            gameObject.SetActive(!hidden);
        }

        private void HandleClick()
        {
            if (manager == null) return;

            var (canRefund, _) = manager.CanRefundAll();
            if (!canRefund) return;

            if (!manager.RefundAll()) return;

            if (treeUI == null) treeUI = GetComponentInParent<SkillTreeUI>();
            if (treeUI != null) treeUI.OnNodeStateChanged(null);
            else RefreshVisuals();

            if (TryGetComponent<ITooltipDisplay>(out var td))
            {
                var (tt, st, os) = GetRefundAllTooltip();
                td.ShowTooltip(tt, st, os);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (manager == null) return;

            if (TryGetComponent<ITooltipDisplay>(out var td))
            {
                var (tt, st, os) = GetRefundAllTooltip();
                td.ShowTooltip(tt, st, os);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TryGetComponent<ITooltipDisplay>(out var td)) td.HideTooltip();
        }

        private (string title, string subtitle, Vector2 offset) GetRefundAllTooltip()
        {
            if (manager == null) return ("Refund All", "", tooltipOffset);

            int nodes = manager.UnlockedNodeCount;
            int cost = manager.GetRefundAllCost();
            int points = manager.GetRefundAllPoints();

            List<string> lines = new()
            {
                $"Undo every unlocked node ({nodes})",
                $"<color=#FFD700>Total cost: {cost}g</color>",
                $"<color=#66CCFF>Refunds {points} skill point{(points == 1 ? "" : "s")}</color>"
            };

            var (canRefund, failMessage) = manager.CanRefundAll();
            if (!canRefund && !string.IsNullOrEmpty(failMessage))
                lines.Add($"<color=#FF4444>{failMessage}</color>");

            return ("Refund All", string.Join("\n", lines), tooltipOffset);
        }
    }
}
