using CrystalFlux.Core;
using CrystalFlux.UISystem;
using CrystalFlux.WaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SaveSystem
{
    public enum SaveMenuAction { LoadBuild, LoadRun, SaveProgress, SaveBuild }

    [RequireComponent(typeof(Button))]
    public class SaveMenuButtons : MonoBehaviour
    {
        public SaveMenuAction action;
        public string tooltipTitle;
        [TextArea] public string tooltipDescription;
        [TextArea] public string savedDescription = "Build saved for this wave.";
        public Vector2 tooltipOffset;
        [Tooltip("Hidden once a run starts (load buttons only). Defaults to this object")]
        public GameObject hideRoot;

        private Button button;
        private bool saved;

        private bool IsLoad => action == SaveMenuAction.LoadBuild || action == SaveMenuAction.LoadRun;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            if (hideRoot == null) hideRoot = gameObject;
            if (!TryGetComponent<ITooltipDisplay>(out _)) gameObject.AddComponent<TooltipTrigger>();

            foreach (TextMeshProUGUI t in GetComponentsInChildren<TextMeshProUGUI>(true))
                if (t.gameObject != gameObject) t.gameObject.SetActive(false);

            WaveManager.WaveStarting += OnWaveStarting;
        }

        private void Start() => ApplyTooltip();

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(OnClick);
            WaveManager.WaveStarting -= OnWaveStarting;
        }

        private void OnWaveStarting(WaveManager _)
        {
            if (IsLoad && hideRoot != null) hideRoot.SetActive(false);
            SetSaved(false);
        }

        private void Update()
        {
            if (IsLoad)
            {
                if (WaveManager.Active != null && hideRoot != null && hideRoot.activeSelf) hideRoot.SetActive(false);
                return;
            }

            RunSaveController c = RunSaveController.Instance;
            bool can = c != null && !saved && (action == SaveMenuAction.SaveProgress ? c.CanSaveProgress : c.CanSaveBuild);
            if (button.interactable != can) button.interactable = can;
        }

        private void OnClick()
        {
            RunSaveController c = RunSaveController.Instance;

            switch (action)
            {
                case SaveMenuAction.LoadBuild:
                    if (SaveListPanelUI.Instance != null) SaveListPanelUI.Instance.Open(SaveKind.Build);
                    break;

                case SaveMenuAction.LoadRun:
                    if (SaveListPanelUI.Instance != null) SaveListPanelUI.Instance.Open(SaveKind.Run);
                    break;

                case SaveMenuAction.SaveProgress:
                    if (c == null || !c.CanSaveProgress || SaveNamePromptUI.Instance == null) return;
                    SaveNamePromptUI.Instance.Show("Save Progress", c.DefaultProgressName, SaveKind.Run, null, c.SaveProgress);
                    break;

                case SaveMenuAction.SaveBuild:
                    if (c == null || !c.CanSaveBuild || SaveNamePromptUI.Instance == null) return;
                    SaveNamePromptUI.Instance.Show("Save Build", c.DefaultBuildName, SaveKind.Build, null, (name, overwrite) =>
                    {
                        bool ok = c.SaveBuild(name, overwrite);
                        if (ok) SetSaved(true);
                        return ok;
                    });
                    break;
            }
        }

        public void SetSaved(bool value)
        {
            saved = value;
            ApplyTooltip();
        }

        private void ApplyTooltip()
        {
            if (!TryGetComponent<ITooltipDisplay>(out var tt)) return;

            string title = string.IsNullOrEmpty(tooltipTitle) ? DefaultTitle : tooltipTitle;
            string description = saved ? savedDescription : string.IsNullOrEmpty(tooltipDescription) ? DefaultDescription : tooltipDescription;
            tt.ShowTooltip(title, description, tooltipOffset);
        }

        private string DefaultTitle => action switch
        {
            SaveMenuAction.LoadBuild => "Load Build",
            SaveMenuAction.LoadRun => "Load Run",
            SaveMenuAction.SaveProgress => "Save Progress",
            _ => "Save Build",
        };

        private string DefaultDescription => action switch
        {
            SaveMenuAction.LoadBuild => "Start a new run from a saved build.",
            SaveMenuAction.LoadRun => "Resume a saved run. The save is removed once loaded.",
            SaveMenuAction.SaveProgress => "Save the current run so it can be resumed later.",
            _ => "Save the current build to reuse in a new run.",
        };
    }
}
