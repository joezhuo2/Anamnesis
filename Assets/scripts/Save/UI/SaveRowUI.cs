using System;
using CrystalFlux.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SaveSystem
{
    public class SaveRowUI : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI dateText;
        public TextMeshProUGUI badgeText;
        public Image highlight;
        public Button button;
        public Vector2 tooltipOffset;

        public SaveFile File { get; private set; }
        public string Issue { get; private set; }

        private Action<SaveRowUI> onClick;

        private void Awake()
        {
            if (button == null) TryGetComponent(out button);
            if (button != null) button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked() => onClick?.Invoke(this);

        public void Bind(SaveFile f, string issue, bool selected, Action<SaveRowUI> click)
        {
            File = f;
            Issue = issue;
            onClick = click;

            if (nameText != null) nameText.text = f.meta.name;
            if (waveText != null) waveText.text = $"Wave {f.meta.wave}";
            if (dateText != null) dateText.text = SaveInfoFormatter.FormatDate(f.meta.savedAtUtc);
            if (badgeText != null) badgeText.text = issue != null ? "Incompatible" : f.meta.readOnly ? "Read only" : "";

            SetSelected(selected);

            if (TryGetComponent<ITooltipDisplay>(out var tt))
                tt.ShowTooltip(f.meta.name, SaveInfoFormatter.Lines(f.meta, issue), tooltipOffset);
        }

        public void SetSelected(bool selected)
        {
            if (highlight != null) highlight.enabled = selected;
        }
    }
}
