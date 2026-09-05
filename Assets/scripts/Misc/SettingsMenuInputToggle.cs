using CrystalFlux.EntitySystem;
using CrystalFlux.SkillTree;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CrystalFlux.SettingsSystem
{
    public class SettingsMenuInputToggle : MonoBehaviour
    {
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

        private void ToggleMenu()
        {
            if (SkillTreeUI.IsAnyOpen || SkillTreeUI.EscapeConsumedThisFrame) return;

            if (cachedMenu == null) cachedMenu = FindAnyObjectByType<SettingsPanelUI>(FindObjectsInactive.Include);
            if (cachedMenu != null) cachedMenu.HandleEscape();
        }
    }
}