using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;
    private PlayerMovement pm;
    private PlayerAttackHandler pah;
    [HideInInspector] public PlayerSkillTree pst;
    public static Vector2 mousePos;

    private void Awake()
    {
        controls = new PlayerControls();
        pm = GetComponent<PlayerMovement>();
        pah = GetComponent<PlayerAttackHandler>();
        pst = GetComponent<PlayerSkillTree>();
    }
    private void OnEnable()
    {
        controls.Player.Enable();
        controls.UI.Enable();

        mousePos = controls.Player.MousePosition.ReadValue<Vector2>();

        controls.Player.Move.performed += ctx => pm.moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => pm.moveInput = Vector2.zero;
        controls.Player.Dash.performed += _ => pm.TryStartDash();

        controls.Player.BasicAttack.performed += _ => pah.PerformAttack(AttackType.Basic);
        controls.Player.Skill.performed += _ => pah.PerformAttack(AttackType.Skill);
        controls.Player.Ultimate.performed += _ => pah.PerformAttack(AttackType.Ultimate);
        controls.Player.Technique.performed += _ => pah.PerformAttack(AttackType.Technique);

        controls.UI.ToggleSkillTree.performed += _ => ToggleSkillTree();
    }
    private void Update() => mousePos = controls.Player.MousePosition.ReadValue<Vector2>();
    private void OnDisable() => controls.Player.Disable();

    private void ToggleSkillTree()
    {
        var skillTreeUI = FindAnyObjectByType<SkillTreeUI>(FindObjectsInactive.Include);
        if (skillTreeUI != null) skillTreeUI.Toggle(gameObject);
    }
}