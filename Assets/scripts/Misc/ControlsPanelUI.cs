using System.Collections.Generic;
using CrystalFlux.EntitySystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CrystalFlux.SettingsSystem
{
    public class ControlsPanelUI : MonoBehaviour
    {
        public const string PauseActionName = "Pause";

        [Header("Gameplay Toggles")]
        public Toggle enemyHealthBarToggle;
        public Toggle xpDropToggle;
        public Toggle goldDropToggle;
        public Toggle waveCompletionMessageToggle;
        public Toggle damageNumberToggle;

        [Header("Keybinds")]
        public RebindButtonUI rebindRowPrefab;
        public RectTransform rebindRowContainer;
        public Button resetBindingsButton;

        [Header("Panel")]
        public Button closeButton;
        public TextMeshProUGUI statusText;

        private readonly List<RebindButtonUI> rows = new();
        private bool isOpen;
        private bool suppressToggleCallbacks;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (rebindRowContainer == null)
                rebindRowContainer = transform.Find("KeybindList")?.GetComponent<RectTransform>();

            WireToggles();
            BuildRebindRows();

            if (resetBindingsButton != null) resetBindingsButton.onClick.AddListener(OnResetBindings);
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            RefreshToggles();
            ShowStatus("Settings Menu");

            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            gameObject.SetActive(isOpen);

            if (isOpen)
            {
                MenuPause.Push();

                RefreshToggles();
                RefreshRows();
                ShowStatus("Settings Menu");
            }
            else
            {
                MenuPause.Pop();
                GameSettings.Save();
            }
        }

        public void Close()
        {
            if (isOpen) Toggle();
        }

        public void ShowStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        public bool HasDuplicate(RebindButtonUI row)
        {
            if (row == null || row.Action == null || row.BindingIndex < 0) return false;

            string path = row.Action.bindings[row.BindingIndex].effectivePath;
            if (string.IsNullOrEmpty(path)) return false;

            foreach (InputActionMap map in GameInput.Controls.asset.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    for (int i = 0; i < action.bindings.Count; i++)
                    {
                        if (action == row.Action && i == row.BindingIndex) continue;

                        InputBinding b = action.bindings[i];
                        if (b.isComposite) continue;
                        if (b.effectivePath == path) return true;
                    }
                }
            }

            return false;
        }

        private void WireToggles()
        {
            if (enemyHealthBarToggle != null)
                enemyHealthBarToggle.onValueChanged.AddListener(v => Apply(s => s.showEnemyHealthBars = v));

            if (xpDropToggle != null)
                xpDropToggle.onValueChanged.AddListener(v => Apply(s => s.xpDropsEnabled = v));

            if (goldDropToggle != null)
                goldDropToggle.onValueChanged.AddListener(v => Apply(s => s.goldDropsEnabled = v));

            if (waveCompletionMessageToggle != null)
                waveCompletionMessageToggle.onValueChanged.AddListener(v => Apply(s => s.showWaveCompletionMessage = v));

            if (damageNumberToggle != null)
                damageNumberToggle.onValueChanged.AddListener(v => Apply(s => s.showDamageNumbers = v));
        }

        private void Apply(System.Action<GameSettings> change)
        {
            if (suppressToggleCallbacks) return;

            change(GameSettings.Current);
            GameSettings.RaiseChanged();
        }

        private void RefreshToggles()
        {
            GameSettings s = GameSettings.Current;

            suppressToggleCallbacks = true;

            enemyHealthBarToggle?.SetIsOnWithoutNotify(s.showEnemyHealthBars);
            xpDropToggle?.SetIsOnWithoutNotify(s.xpDropsEnabled);
            goldDropToggle?.SetIsOnWithoutNotify(s.goldDropsEnabled);
            waveCompletionMessageToggle?.SetIsOnWithoutNotify(s.showWaveCompletionMessage);
            damageNumberToggle?.SetIsOnWithoutNotify(s.showDamageNumbers);

            suppressToggleCallbacks = false;
        }

        private void BuildRebindRows()
        {
            if (rebindRowPrefab == null || rebindRowContainer == null) return;

            foreach (InputActionMap map in GameInput.Controls.asset.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    if (action.name == PauseActionName) continue;

                    for (int i = 0; i < action.bindings.Count; i++)
                    {
                        InputBinding b = action.bindings[i];
                        if (b.isComposite || !IsKeyboardBinding(b)) continue;

                        RebindButtonUI row = Instantiate(rebindRowPrefab, rebindRowContainer);
                        row.Setup(this, action, i);
                        rows.Add(row);
                    }
                }
            }
        }

        private void RefreshRows()
        {
            for (int i = 0; i < rows.Count; i++)
                if (rows[i] != null) rows[i].Refresh();
        }

        private void OnResetBindings()
        {
            GameInput.ResetAllBindings();
            RefreshRows();
            ShowStatus("Keybinds reset to defaults.");
        }

        private static bool IsKeyboardBinding(InputBinding b)
        {
            string path = b.effectivePath;
            return !string.IsNullOrEmpty(path) && path.StartsWith("<Keyboard>");
        }
    }
}