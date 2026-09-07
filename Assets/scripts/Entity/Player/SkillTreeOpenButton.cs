using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CrystalFlux.SkillTree
{
    [RequireComponent(typeof(Button))]
    public class SkillTreeOpenButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        public SkillTreeUI treeUI;
        public GameObject player;

        [Header("Tooltip")]
        public string tooltipTitle = "Skill Tree";
        public Vector2 tooltipOffset = new(100, -100);

        private Button btn;
        private ICurrencyHolder ich;
        private ISkillPointHolder isph;
        private bool hovered;

        private void Awake()
        {
            btn = GetComponent<Button>();
            if (treeUI == null) treeUI = FindAnyObjectByType<SkillTreeUI>(FindObjectsInactive.Include);
        }

        private void OnEnable()
        {
            if (btn != null)
            {
                btn.onClick.RemoveListener(HandleClick);
                btn.onClick.AddListener(HandleClick);
            }
        }

        private void OnDisable()
        {
            hovered = false;
            if (btn != null) btn.onClick.RemoveListener(HandleClick);
            if (TryGetComponent<ITooltipDisplay>(out var td)) td.HideTooltip();
        }

        private void Update()
        {
            if (hovered) RefreshTooltip();
        }

        private GameObject ResolvePlayer()
        {
            if (player == null) player = GameObject.FindWithTag("Player");
            return player;
        }

        private void ResolveHolders()
        {
            var p = ResolvePlayer();
            if (p == null) return;

            if (ich == null) p.TryGetComponent(out ich);
            if (isph == null) p.TryGetComponent(out isph);
        }

        private void HandleClick()
        {
            if (treeUI == null) treeUI = FindAnyObjectByType<SkillTreeUI>(FindObjectsInactive.Include);
            if (treeUI == null) return;

            treeUI.Toggle(ResolvePlayer());
            if (TryGetComponent<ITooltipDisplay>(out var td)) td.HideTooltip();
            hovered = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            RefreshTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            if (TryGetComponent<ITooltipDisplay>(out var td)) td.HideTooltip();
        }

        private void RefreshTooltip()
        {
            if (!TryGetComponent<ITooltipDisplay>(out var td)) return;

            var (tt, st, os) = GetSkillTreeTooltip();
            td.ShowTooltip(tt, st, os);
        }

        private (string title, string subtitle, Vector2 offset) GetSkillTreeTooltip()
        {
            ResolveHolders();

            List<string> lines = new() { $"Open with <color=#FFFFFF>{GetToggleKeyLabel()}</color>" };

            if (ich != null) lines.Add($"<color=#FFD700>Gold: {ich.CurrentAmount}</color>");
            if (isph != null) lines.Add($"<color=#66CCFF>Skill Points: {isph.SkillPoints}</color>");

            return (tooltipTitle, string.Join("\n", lines), tooltipOffset);
        }

        private static string GetToggleKeyLabel()
        {
            var action = GameInput.Controls.UI.ToggleSkillTree;
            string label = action != null
                ? action.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions)
                : "";

            return string.IsNullOrWhiteSpace(label) ? "K" : label;
        }
    }
}
