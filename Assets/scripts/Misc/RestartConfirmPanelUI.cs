using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SettingsSystem
{
    public class RestartConfirmPanelUI : MonoBehaviour
    {
        private const string DefaultMessage = "Restart run?\nAll progress will be lost.";

        [Header("Panel")]
        public SettingsPanelUI settingsPanel;
        public Button confirmButton;
        public Button cancelButton;
        public TextMeshProUGUI messageText;

        private bool isOpen;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (settingsPanel == null)
                settingsPanel = FindAnyObjectByType<SettingsPanelUI>(FindObjectsInactive.Include);

            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
            if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);

            ShowMessage(DefaultMessage);

            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            gameObject.SetActive(isOpen);

            if (isOpen)
            {
                MenuPause.Push();
                ShowMessage(DefaultMessage);
            }
            else
            {
                MenuPause.Pop();
            }
        }

        public void Close()
        {
            if (isOpen) Toggle();
        }

        public void ShowMessage(string message)
        {
            if (messageText != null) messageText.text = message;
        }

        private void OnCancel()
        {
            if (!isOpen) return;

            if (settingsPanel != null) settingsPanel.HandleEscape();
            else Close();
        }

        private void OnConfirm()
        {
            if (!isOpen || GameRestart.IsRestarting) return;

            isOpen = false;
            gameObject.SetActive(false);

            GameRestart.ToHomeScreen();
        }
    }
}
