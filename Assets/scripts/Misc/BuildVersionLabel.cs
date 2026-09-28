using TMPro;
using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class BuildVersionLabel : MonoBehaviour
    {
        public string format = "v{0}";
        public bool showDevTag = true;

        private void Awake()
        {
            if (!TryGetComponent<TextMeshProUGUI>(out var tmp)) return;

            string txt = string.Format(format, Application.version);

            if (showDevTag && Application.isEditor) txt += " (editor)";
            else if (showDevTag && Debug.isDebugBuild) txt += " (dev)";

            tmp.text = txt;
        }
    }
}
