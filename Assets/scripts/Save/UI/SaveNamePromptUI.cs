using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SaveSystem
{
    public class SaveNamePromptUI : MonoBehaviour
    {
        public static SaveNamePromptUI Instance { get; private set; }

        public GameObject root;
        public TextMeshProUGUI titleText;
        public TMP_InputField input;
        public TextMeshProUGUI errorText;
        public Button confirmButton;
        public Button cancelButton;

        private SaveKind kind;
        private string ignoreId;
        private Func<string, bool, bool> onSubmit;

        public bool IsOpen => root != null && root.activeSelf;
        public static bool IsTyping => Instance != null && Instance.IsOpen && Instance.input != null && Instance.input.isFocused;

        private void Awake()
        {
            Instance = this;
            if (root == null) root = gameObject;

            if (confirmButton != null) confirmButton.onClick.AddListener(Submit);
            if (cancelButton != null) cancelButton.onClick.AddListener(Close);
            if (input != null)
            {
                input.characterLimit = SaveStore.MaxNameLength;
                input.onSubmit.AddListener(OnInputSubmit);
            }

            root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(Submit);
            if (cancelButton != null) cancelButton.onClick.RemoveListener(Close);
            if (input != null) input.onSubmit.RemoveListener(OnInputSubmit);
            if (ReferenceEquals(Instance, this)) Instance = null;
        }

        public void Show(string title, string defaultName, SaveKind saveKind, string ignore, Func<string, bool, bool> submit)
        {
            kind = saveKind;
            ignoreId = ignore;
            onSubmit = submit;

            if (titleText != null) titleText.text = title;
            SetError("");

            root.SetActive(true);
            root.transform.SetAsLastSibling();

            if (input != null)
            {
                input.text = defaultName ?? "";
                input.ActivateInputField();
                input.selectionAnchorPosition = 0;
                input.selectionFocusPosition = input.text.Length;
            }
        }

        public void Close()
        {
            root.SetActive(false);
            onSubmit = null;
        }

        private void OnInputSubmit(string _) => Submit();

        private void Submit()
        {
            if (onSubmit == null) return;

            string name = SaveStore.NormalizeName(input != null ? input.text : "");
            if (name.Length == 0)
            {
                SetError("Enter a name");
                return;
            }

            SaveFile existing = SaveStore.FindByName(kind, name);
            if (existing != null && existing.meta.id != ignoreId)
            {
                if (ConfirmDialogUI.Instance == null)
                {
                    SetError("Name already used");
                    return;
                }

                ConfirmDialogUI.Instance.Show($"Overwrite '{existing.meta.name}'?", () => Finish(name, true));
                return;
            }

            Finish(name, false);
        }

        private void Finish(string name, bool overwrite)
        {
            Func<string, bool, bool> cb = onSubmit;
            if (cb == null) return;

            if (cb(name, overwrite)) Close();
            else SetError("Save failed");
        }

        private void SetError(string msg)
        {
            if (errorText != null) errorText.text = msg;
        }
    }
}
