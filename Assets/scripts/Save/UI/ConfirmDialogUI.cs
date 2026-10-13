using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SaveSystem
{
    public class ConfirmDialogUI : MonoBehaviour
    {
        public static ConfirmDialogUI Instance { get; private set; }

        public GameObject root;
        public TextMeshProUGUI messageText;
        public Button yesButton;
        public Button noButton;

        private Action onYes;
        private Action onNo;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            Instance = this;
            if (root == null) root = gameObject;

            if (yesButton != null) yesButton.onClick.AddListener(OnYes);
            if (noButton != null) noButton.onClick.AddListener(OnNo);

            root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (yesButton != null) yesButton.onClick.RemoveListener(OnYes);
            if (noButton != null) noButton.onClick.RemoveListener(OnNo);
            if (ReferenceEquals(Instance, this)) Instance = null;
        }

        public void Show(string message, Action yes, Action no = null)
        {
            onYes = yes;
            onNo = no;

            if (messageText != null) messageText.text = message;

            root.SetActive(true);
            root.transform.SetAsLastSibling();
        }

        public void Close()
        {
            root.SetActive(false);
            onYes = null;
            onNo = null;
        }

        private void OnYes()
        {
            Action a = onYes;
            Close();
            a?.Invoke();
        }

        private void OnNo()
        {
            Action a = onNo;
            Close();
            a?.Invoke();
        }
    }
}
