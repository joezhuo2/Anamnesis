using UnityEngine;
[RequireComponent(typeof(PlayerMovement))]

[RequireComponent(typeof(PlayerAttackHandler))]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;
    private PlayerMovement pm;
    private PlayerAttackHandler pah;
    public static Vector2 mousePos;
    private void Awake()
    {
        controls = new PlayerControls();
        pm = GetComponent<PlayerMovement>();
        pah = GetComponent<PlayerAttackHandler>();
    }
    private void OnEnable() {
        controls.Player.Enable();

        mousePos = controls.Player.MousePosition.ReadValue<Vector2>();

        controls.Player.Move.performed += ctx => pm.moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => pm.moveInput = Vector2.zero;
        controls.Player.Dash.performed += _ => pm.TryStartDash();

        controls.Player.BasicAttack.performed += _ => pah.PerformAttack(AttackType.Basic);
        controls.Player.Skill.performed += _ => pah.PerformAttack(AttackType.Skill);
        controls.Player.Ultimate.performed += _ => pah.PerformAttack(AttackType.Ultimate);
        controls.Player.Technique.performed += _ => pah.PerformAttack(AttackType.Technique);
    }
    private void Update() => mousePos = controls.Player.MousePosition.ReadValue<Vector2>();
    private void OnDisable() => controls.Player.Disable();
}