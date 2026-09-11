using System.Collections;
using CrystalFlux.EntitySystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrystalFlux.SettingsSystem
{
    public class DeathScreenUI : MonoBehaviour
    {
        private const string DefaultTitle = "You Died";
        private const string DefaultMessage = "The waves claim another.";

        [Header("Panel")]
        public GameObject panelRoot;

        [Header("Content")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI messageText;
        public string title = DefaultTitle;
        [TextArea] public string message = DefaultMessage;

        [Header("Buttons")]
        public Button restartButton;
        public GameObject firstSelected;

        [Header("Timing")]
        public float showDelay = 1f;

        private static DeathScreenUI instance;
        private CoroutineHost host;
        private EntityHealth playerHealth;
        private bool isOpen;
        private bool pending;
        private bool pushed;

        public static bool IsOpen => instance != null && instance.isOpen;
        public static bool IsPlayerDead => instance != null && (instance.isOpen || instance.pending);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        private void Awake()
        {
            instance = this;

            if (panelRoot == null) panelRoot = gameObject;

            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);

            ApplyText();
            panelRoot.SetActive(false);

            host = CoroutineHost.Create(name);
            host.StartCoroutine(BindPlayer());
        }

        private void OnDestroy()
        {
            if (playerHealth != null) playerHealth.OnDeath -= OnPlayerDeath;
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestart);
            if (host != null) Destroy(host.gameObject);
            ReleasePause();
            if (ReferenceEquals(instance, this)) instance = null;
        }

        private IEnumerator BindPlayer()
        {
            while (playerHealth == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                    playerHealth = player.GetComponentInChildren<EntityHealth>(true);

                if (playerHealth == null) yield return null;
            }

            playerHealth.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath(GameObject player)
        {
            if (isOpen || pending || GameRestart.IsRestarting) return;

            pending = true;
            if (host != null) host.StartCoroutine(ShowAfterDelay());
            else Show();
        }

        private IEnumerator ShowAfterDelay()
        {
            float elapsed = 0f;
            while (elapsed < showDelay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Show();
        }

        public void Show()
        {
            pending = false;
            if (isOpen || GameRestart.IsRestarting) return;

            SettingsPanelUI settings = FindAnyObjectByType<SettingsPanelUI>(FindObjectsInactive.Include);
            if (settings != null)
            {
                if (settings.restartConfirmPanel != null) settings.restartConfirmPanel.Close();
                if (settings.controlsPanel != null) settings.controlsPanel.Close();
                settings.ClosePanel();
            }

            isOpen = true;
            panelRoot.SetActive(true);
            ApplyText();

            if (!pushed)
            {
                MenuPause.Push();
                pushed = true;
            }

            GameObject select = firstSelected != null ? firstSelected
                : restartButton != null ? restartButton.gameObject : null;

            if (select != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(select);
            }
        }

        public void Hide()
        {
            if (!isOpen) return;

            isOpen = false;
            if (panelRoot != null) panelRoot.SetActive(false);
            ReleasePause();
        }

        private void ReleasePause()
        {
            if (!pushed) return;

            pushed = false;
            MenuPause.Pop();
        }

        public void SetText(string newTitle, string newMessage)
        {
            title = newTitle;
            message = newMessage;
            ApplyText();
        }

        private void ApplyText()
        {
            if (titleText != null) titleText.text = string.IsNullOrEmpty(title) ? DefaultTitle : title;
            if (messageText != null) messageText.text = string.IsNullOrEmpty(message) ? DefaultMessage : message;
        }

        private class CoroutineHost : MonoBehaviour
        {
            public static CoroutineHost Create(string owner)
            {
                var go = new GameObject($"[{owner}] Runner");
                return go.AddComponent<CoroutineHost>();
            }
        }

        private void OnRestart()
        {
            if (!isOpen || GameRestart.IsRestarting) return;

            Hide();
            GameRestart.ToHomeScreen();
        }
    }
}
