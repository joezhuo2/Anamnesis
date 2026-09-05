using CrystalFlux.EntitySystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CrystalFlux.SkillTree
{
    public class SkillTreeInputToggle : MonoBehaviour
    {
        private PlayerControls controls;

        private void Awake() => controls = GameInput.Controls;

        private void OnEnable()
        {
            GameInput.EnableUIMap();
            controls.UI.ToggleSkillTree.performed += OnToggleSkillTree;
            controls.UI.Pause.performed += OnPause;
        }

        private void OnDisable()
        {
            controls.UI.ToggleSkillTree.performed -= OnToggleSkillTree;
            controls.UI.Pause.performed -= OnPause;
            GameInput.DisableUIMap();
        }

        private void OnToggleSkillTree(InputAction.CallbackContext ctx) => ToggleSkillTree();

        private void OnPause(InputAction.CallbackContext ctx) => CloseSkillTree();

        private SkillTreeUI cachedUI;

        private SkillTreeUI ResolveUI()
        {
            if (cachedUI == null) cachedUI = FindAnyObjectByType<SkillTreeUI>(FindObjectsInactive.Include);
            return cachedUI;
        }

        private void ToggleSkillTree()
        {
            var ui = ResolveUI();
            if (ui != null) ui.Toggle(gameObject);
        }

        private void CloseSkillTree()
        {
            if (!SkillTreeUI.IsAnyOpen) return;

            var ui = ResolveUI();
            if (ui != null) ui.CloseFromEscape();
        }
    }
}
