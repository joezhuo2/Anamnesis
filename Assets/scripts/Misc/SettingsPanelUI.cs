using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    public class SettingsPanelUI : MonoBehaviour
    {
        public ControlsPanelUI controlsPanel;

        private bool isOpen;

        private void Awake() => gameObject.SetActive(false);

        public bool IsOpen => isOpen;

        public void HandleEscape()
        {
            if (controlsPanel != null && controlsPanel.IsOpen)
            {
                controlsPanel.Close();
                if (!isOpen) Toggle();
                return;
            }

            Toggle();
        }

        public void Toggle()
        {
            if (!isOpen && Time.timeScale == 0f && !MenuPause.IsPaused) return;

            isOpen = !isOpen;
            gameObject.SetActive(isOpen);

            if (isOpen)
            {
                MenuPause.Push();
            }
            else
            {
                MenuPause.Pop();
                GameSettings.Save();
            }
        }

        public void OpenControlsPanel()
        {
            if (controlsPanel != null)
                controlsPanel.Toggle();
            ClosePanel();
        }

        public void ClosePanel() 
        {
            if (isOpen)
                Toggle();
        }
    }
}