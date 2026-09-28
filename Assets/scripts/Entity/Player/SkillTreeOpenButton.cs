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
        private ITooltipDisplay td;
        private int lastGold = int.MinValue;
        private int lastSp = int.MinValue;

        private void Awake()
        {
            btn = GetComponent<Button>();
            TryGetComponent(out td);
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
            if (td != null) td.HideTooltip();
        }

        private void Update()
        {
            if (!hovered) return;

            ResolveHolders();
            int g = ich != null ? ich.CurrentAmount : int.MinValue;
            int sp = isph != null ? isph.SkillPoints : int.MinValue;
            if (g != lastGold || sp != lastSp) RefreshTooltip();
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
            if (td != null) td.HideTooltip();
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
            if (td != null) td.HideTooltip();
        }

        private void RefreshTooltip()
        {
            if (td == null) return;

            var (tt, st, os) = GetSkillTreeTooltip();
            td.ShowTooltip(tt, st, os);
        }

        private (string title, string subtitle, Vector2 offset) GetSkillTreeTooltip()
        {
            ResolveHolders();

            lastGold = ich != null ? ich.CurrentAmount : int.MinValue;
            lastSp = isph != null ? isph.SkillPoints : int.MinValue;

            string s = $"Open with <color=#FFFFFF>{GetToggleKeyLabel()}</color>";
            if (ich != null) s += $"\n<color=#FFD700>Gold: {lastGold}</color>";
            if (isph != null) s += $"\n<color=#66CCFF>Skill Points: {lastSp}</color>";

            return (tooltipTitle, s, tooltipOffset);
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
