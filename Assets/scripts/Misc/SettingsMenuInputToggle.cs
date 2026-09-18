using CrystalFlux.EntitySystem;
using CrystalFlux.SkillTree;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CrystalFlux.SettingsSystem
{
    public class SettingsMenuInputToggle : MonoBehaviour
    {
        [Tooltip("Open the pause menu when the game window loses focus.")]
        public bool pauseOnFocusLoss = true;

        private InputAction pauseAction;
        private SettingsPanelUI cachedMenu;

        private void Awake() =>
            pauseAction = GameInput.Controls.asset.FindAction($"UI/{ControlsPanelUI.PauseActionName}", false);

        private void OnEnable()
        {
            if (pauseAction == null)
            {
                Debug.LogWarning("[SettingsMenuInputToggle] No 'UI/Pause' action found. Reimport PlayerControls.inputactions.", this);
                return;
            }

            GameInput.EnableUIMap();
            pauseAction.performed += OnPause;
        }

        private void OnDisable()
        {
            if (pauseAction == null) return;

            pauseAction.performed -= OnPause;
            GameInput.DisableUIMap();
        }

        private void OnPause(InputAction.CallbackContext ctx) => ToggleMenu();

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus || !pauseOnFocusLoss) return;
            if (DeathScreenUI.IsPlayerDead || GameRestart.IsRestarting) return;
            if (SkillTreeUI.IsAnyOpen || MenuPause.IsPaused || Time.timeScale == 0f) return;

            if (cachedMenu == null) cachedMenu = FindAnyObjectByType<SettingsPanelUI>(FindObjectsInactive.Include);
            if (cachedMenu != null && !cachedMenu.IsOpen) cachedMenu.Toggle();
        }

        private void ToggleMenu()
        {
            if (DeathScreenUI.IsPlayerDead || GameRestart.IsRestarting) return;
            if (SkillTreeUI.IsAnyOpen || SkillTreeUI.EscapeConsumedThisFrame) return;

            if (cachedMenu == null) cachedMenu = FindAnyObjectByType<SettingsPanelUI>(FindObjectsInactive.Include);
            if (cachedMenu != null) cachedMenu.HandleEscape();
        }
    }
}