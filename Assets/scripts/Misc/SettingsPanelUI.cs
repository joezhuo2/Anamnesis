using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SettingsSystem
{
    public class SettingsPanelUI : MonoBehaviour
    {
        public ControlsPanelUI controlsPanel;
        public RestartConfirmPanelUI restartConfirmPanel;

        private bool isOpen;
        private GameObject inputBlocker;

        private void Awake()
        {
            CreateInputBlocker();
            gameObject.SetActive(false);
        }

        private void CreateInputBlocker()
        {
            var canvas = GetComponentInParent<Canvas>(true);
            if (canvas == null) return;

            inputBlocker = new GameObject("InputBlocker", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)inputBlocker.transform;
            rt.SetParent(canvas.transform, false);
            rt.SetSiblingIndex(transform.GetSiblingIndex());
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            inputBlocker.GetComponent<Image>().color = Color.clear;
            inputBlocker.SetActive(false);
        }

        public bool IsOpen => isOpen;

        public void HandleEscape()
        {
            if (restartConfirmPanel != null && restartConfirmPanel.IsOpen)
            {
                restartConfirmPanel.Close();
                if (!isOpen) Toggle();
                return;
            }

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
            if (!isOpen && DeathScreenUI.IsPlayerDead) return;
            if (!isOpen && Time.timeScale == 0f && !MenuPause.IsPaused) return;

            isOpen = !isOpen;
            gameObject.SetActive(isOpen);
            if (inputBlocker != null) inputBlocker.SetActive(isOpen);

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

        public void OpenRestartPanel()
        {
            if (restartConfirmPanel != null)
                restartConfirmPanel.Toggle();
            ClosePanel();
        }

        public void QuitGame()
        {
            if (!isOpen || GameRestart.IsRestarting) return;

            GameSettings.Save();
            GameRestart.QuitGame();
        }

        public void ClosePanel()
        {
            if (isOpen)
                Toggle();
        }
    }
}