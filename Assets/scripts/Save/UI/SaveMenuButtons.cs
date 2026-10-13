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
        public TextMeshProUGUI label;
        public string savedLabel = "Build Saved";
        [Tooltip("Hidden once a run starts (load buttons only). Defaults to this object")]
        public GameObject hideRoot;

        private Button button;
        private string defaultLabel;
        private bool saved;

        private bool IsLoad => action == SaveMenuAction.LoadBuild || action == SaveMenuAction.LoadRun;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            if (hideRoot == null) hideRoot = gameObject;
            if (label == null) label = GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) defaultLabel = label.text;

            WaveManager.WaveStarting += OnWaveStarting;
        }

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
            if (label != null && defaultLabel != null) label.text = saved ? savedLabel : defaultLabel;
        }
    }
}
