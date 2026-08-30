using UnityEngine;
using UnityEngine.InputSystem;

namespace CrystalFlux.SkillTree
{
    public class SkillTreeInputToggle : MonoBehaviour
    {
        private PlayerControls controls;

        private void Awake() => controls = new PlayerControls();

        private void OnEnable()
        {
            controls.UI.Enable();
            controls.UI.ToggleSkillTree.performed += OnToggleSkillTree;
        }

        private void OnDisable()
        {
            controls.UI.ToggleSkillTree.performed -= OnToggleSkillTree;
            controls.UI.Disable();
        }
        private void OnDestroy() => controls?.Dispose();

        private void OnToggleSkillTree(InputAction.CallbackContext ctx) => ToggleSkillTree();

        private void ToggleSkillTree()
        {
            var skillTreeUI = FindAnyObjectByType<SkillTreeUI>(FindObjectsInactive.Include);
            if (skillTreeUI != null) skillTreeUI.Toggle(gameObject);
        }
    }
}