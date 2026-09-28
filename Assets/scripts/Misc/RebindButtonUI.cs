using CrystalFlux.EntitySystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace CrystalFlux.SettingsSystem
{
    public class RebindButtonUI : MonoBehaviour
    {
        [Header("References")]
        public TextMeshProUGUI labelText;
        public TextMeshProUGUI bindingText;
        public Button rebindButton;

        private ControlsPanelUI owner;
        private InputAction action;
        private int bindingIndex = -1;
        private InputActionRebindingExtensions.RebindingOperation op;

        public InputAction Action => action;
        public int BindingIndex => bindingIndex;

        public void Setup(ControlsPanelUI menu, InputAction boundAction, int index)
        {
            owner = menu;
            action = boundAction;
            bindingIndex = index;

            if (rebindButton == null) rebindButton = GetComponentInChildren<Button>(true);

            if (rebindButton != null)
            {
                rebindButton.onClick.RemoveListener(StartRebind);
                rebindButton.onClick.AddListener(StartRebind);
            }

            if (labelText != null) labelText.text = BuildLabel();

            Refresh();
        }

        public void Refresh()
        {
            if (bindingText == null || action == null || bindingIndex < 0) return;
            bindingText.text = action.GetBindingDisplayString(bindingIndex);
        }

        public void StartRebind()
        {
            if (op != null || action == null || bindingIndex < 0) return;

            bool wasEnabled = action.enabled;
            action.Disable();

            if (bindingText != null) bindingText.text = "...";
            owner?.ShowStatus("Press a key or escape to cancel.");

            op = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>")
                .WithControlsExcluding("<Pen>")
                .WithControlsExcluding("<Touchscreen>")
                .WithControlsExcluding("<Gamepad>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(o => Finish(o, wasEnabled, false))
                .OnComplete(o => Finish(o, wasEnabled, true));

            op.Start();
        }

        private void Finish(InputActionRebindingExtensions.RebindingOperation o, bool reEnable, bool completed)
        {
            o.Dispose();
            op = null;

            if (reEnable) action.Enable();

            if (!completed)
            {
                owner?.ShowStatus("Rebind cancelled.");
                Refresh();
                return;
            }

            if (owner != null && owner.HasDuplicate(this))
            {
                string clash = action.GetBindingDisplayString(bindingIndex);
                action.RemoveBindingOverride(bindingIndex);
                owner.ShowStatus($"'{clash}' is already bound.");
            }
            else
            {
                GameInput.SaveOverrides();
                owner?.ShowStatus("Settings Menu");
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (op == null) return;

            op.Cancel();
            op.Dispose();
            op = null;
        }

        private string BuildLabel()
        {
            if (action == null || bindingIndex < 0) return "";

            InputBinding b = action.bindings[bindingIndex];
            return b.isPartOfComposite && !string.IsNullOrEmpty(b.name)
                ? $"{action.name} {b.name}"
                : action.name;
        }
    }
}