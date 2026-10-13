using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SaveSystem
{
    public class LoadedBuildLabelUI : MonoBehaviour
    {
        public GameObject root;
        public TextMeshProUGUI label;
        public Button clearButton;
        public string format = "Build loaded: {0}";

        private void Awake()
        {
            if (root == null) root = gameObject;
            if (clearButton != null) clearButton.onClick.AddListener(OnClear);

            RunSaveController.PendingBuildChanged += Refresh;
        }

        private void OnDestroy()
        {
            if (clearButton != null) clearButton.onClick.RemoveListener(OnClear);
            RunSaveController.PendingBuildChanged -= Refresh;
        }

        private void Start() => Refresh();

        private void OnClear() => RunSaveController.Instance?.ClearPendingBuild();

        private void Refresh()
        {
            bool show = RunSaveController.HasPendingBuild;
            if (label != null) label.text = show ? string.Format(format, RunSaveController.PendingBuildName) : "";
            if (root != null && root != gameObject) root.SetActive(show);
            else if (label != null) label.gameObject.SetActive(show);
        }
    }
}
