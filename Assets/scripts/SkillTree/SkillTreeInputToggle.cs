using UnityEngine;

public class SkillTreeInputToggle : MonoBehaviour
{
    private PlayerControls controls;

    private void Awake() => controls = new PlayerControls();

    private void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.ToggleSkillTree.performed += _ => ToggleSkillTree();
    }

    private void OnDisable() => controls.UI.Disable();

    private void ToggleSkillTree()
    {
        var skillTreeUI = FindAnyObjectByType<SkillTreeUI>(FindObjectsInactive.Include);
        if (skillTreeUI != null) skillTreeUI.Toggle(gameObject);
    }
}
