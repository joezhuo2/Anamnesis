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
        }

        private void OnDisable()
        {
            controls.UI.ToggleSkillTree.performed -= OnToggleSkillTree;
            GameInput.DisableUIMap();
        }

        private void OnToggleSkillTree(InputAction.CallbackContext ctx) => ToggleSkillTree();

        private SkillTreeUI cachedUI;

        private void ToggleSkillTree()
        {
            if (cachedUI == null) cachedUI = FindAnyObjectByType<SkillTreeUI>(FindObjectsInactive.Include);
            if (cachedUI != null) cachedUI.Toggle(gameObject);
        }
    }
}
