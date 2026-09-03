using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.WaveSystem
{
    public class DifficultySelector : MonoBehaviour
    {
        public static DifficultySelector Current { get; private set; }

        [Header("Difficulties")]
        public List<DifficultyData> difficulties = new();

        [Header("Display")]
        public GameObject root;
        public Image frameImage;
        public TextMeshProUGUI nameText;
        public Vector2 tooltipOffset;

        [Header("Cycle Buttons")]
        public Button leftButton;
        public Button rightButton;

        private int index;
        private bool lockedIn;

        public DifficultyData Selected =>
            difficulties != null && index >= 0 && index < difficulties.Count ? difficulties[index] : null;

        private void Awake()
        {
            if (Current == null) Current = this;

            if (root == null) root = gameObject;
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(Current, this)) Current = null;

            if (leftButton != null) leftButton.onClick.RemoveListener(OnLeftClicked);
            if (rightButton != null) rightButton.onClick.RemoveListener(OnRightClicked);
        }

        private void Start()
        {
            if (difficulties == null || difficulties.Count == 0)
            {
                root.SetActive(false);
                return;
            }

            difficulties.RemoveAll(d => d == null);

            if (difficulties.Count == 0)
            {
                root.SetActive(false);
                return;
            }

            if (leftButton != null) leftButton.onClick.AddListener(OnLeftClicked);
            if (rightButton != null) rightButton.onClick.AddListener(OnRightClicked);

            index = Mathf.Clamp(GameSettings.Current.difficultyIndex, 0, difficulties.Count - 1);
            Refresh();
        }

        private void OnLeftClicked() => Cycle(-1);
        private void OnRightClicked() => Cycle(1);

        private void Cycle(int delta)
        {
            if (lockedIn || difficulties.Count == 0) return;

            index = (index + delta + difficulties.Count) % difficulties.Count;

            GameSettings.Current.difficultyIndex = index;
            GameSettings.Save();

            Refresh();
        }

        private void Refresh()
        {
            DifficultyData d = Selected;
            if (d == null) return;

            if (frameImage != null)
            {
                frameImage.sprite = d.frameSprite;
                frameImage.enabled = d.frameSprite != null;
            }

            if (nameText != null)
            {
                nameText.text = d.displayName;
                nameText.color = d.nameColor;
            }

            if (TryGetComponent<ITooltipDisplay>(out var tt))
                tt.ShowTooltip(d.displayName, d.BuildTooltipDescription(), tooltipOffset);
        }

        public void LockIn(WaveManager wm)
        {
            if (lockedIn) return;

            DifficultyData d = Selected;
            if (d == null) return;

            lockedIn = true;

            if (wm != null) wm.ApplyDifficulty(d);

            if (root != null) root.SetActive(false);
        }
    }
}
