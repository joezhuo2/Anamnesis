using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.WaveSystem
{
    public class IronmanSelector : MonoBehaviour
    {
        public static IronmanSelector Current { get; private set; }

        [Header("Display")]
        public GameObject root;
        public Vector2 tooltipOffset;
        public Sprite toggleOnSprite;
        public Sprite toggleOffSprite;

        [Header("Tooltip")]
        public string displayName = "Ironman Mode";
        [TextArea(3, 6)] public string description = "No rerolls, no corruption, no skill point refunds.\nEvery choice is permanent.";

        [Header("Toggle")]
        public Button toggleButton;

        private bool lockedIn;

        public static bool Enabled => GameSettings.Current.ironmanMode;

        private void Awake()
        {
            if (Current == null) Current = this;

            if (root == null) root = gameObject;
            if (toggleButton == null) TryGetComponent(out toggleButton);
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(Current, this)) Current = null;

            if (toggleButton != null) toggleButton.onClick.RemoveListener(OnToggleClicked);
        }

        private void Start()
        {
            if (toggleButton != null) toggleButton.onClick.AddListener(OnToggleClicked);

            Refresh();
        }

        private void OnToggleClicked()
        {
            if (lockedIn) return;

            GameSettings.Current.ironmanMode = !GameSettings.Current.ironmanMode;
            GameSettings.Save();
            GameSettings.RaiseChanged();

            Refresh();
        }

        private void Refresh()
        {
            bool on = Enabled;

            if (toggleButton.image != null && toggleButton.image.sprite != null)
            {
                if (on && toggleOnSprite != null) toggleButton.image.sprite = toggleOnSprite;
                else if (toggleOffSprite != null) toggleButton.image.sprite = toggleOffSprite;
            }

            if (TryGetComponent<ITooltipDisplay>(out var tt))
                tt.ShowTooltip($"{displayName} [{(on ? "ON" : "OFF")}]", description, tooltipOffset);
        }

        public void LockIn()
        {
            if (lockedIn) return;

            lockedIn = true;

            if (root != null) root.SetActive(false);
        }
    }
}
