using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.WaveSystem
{
    public class ModeSelector : MonoBehaviour
    {
        public static ModeSelector Current { get; private set; }

        [Header("Modes")]
        public List<ModeData> modes = new();

        [Header("Display")]
        public GameObject root;
        public Vector2 tooltipOffset;

        [Header("Cycle Button")]
        public Button cycleButton;

        private int index;
        private bool lockedIn;

        public ModeData Selected =>
            modes != null && index >= 0 && index < modes.Count ? modes[index] : null;

        private void Awake()
        {
            if (Current == null) Current = this;

            if (root == null) root = gameObject;
            if (cycleButton == null) TryGetComponent(out cycleButton);
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(Current, this)) Current = null;

            if (cycleButton != null) cycleButton.onClick.RemoveListener(OnCycleClicked);
        }

        private void Start()
        {
            modes?.RemoveAll(m => m == null);

            if (modes == null || modes.Count == 0)
            {
                root.SetActive(false);
                return;
            }

            if (cycleButton != null) cycleButton.onClick.AddListener(OnCycleClicked);

            index = Mathf.Clamp(GameSettings.Current.modeIndex, 0, modes.Count - 1);
            Refresh();
        }

        private void OnCycleClicked()
        {
            if (lockedIn || modes.Count == 0) return;

            index = (index + 1) % modes.Count;

            GameSettings.Current.modeIndex = index;
            GameSettings.Save();

            Refresh();
        }

        private void Refresh()
        {
            ModeData m = Selected;
            if (m == null) return;

            if (cycleButton != null && cycleButton.image != null && m.buttonSprite != null)
                cycleButton.image.sprite = m.buttonSprite;

            if (TryGetComponent<ITooltipDisplay>(out var tt))
                tt.ShowTooltip(m.displayName, m.BuildTooltipDescription(), tooltipOffset);
        }

        public void LockIn(WaveManager wm)
        {
            if (lockedIn) return;

            ModeData m = Selected;
            if (m == null) return;

            lockedIn = true;

            if (wm != null) wm.ApplyMode(m);

            if (root != null) root.SetActive(false);
        }
    }
}
