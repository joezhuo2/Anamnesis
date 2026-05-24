using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;
    private PlayerMovement pm;
    public static Vector2 mousePos;
    private void Awake()
    {
        controls = new PlayerControls();
        pm = GetComponent<PlayerMovement>();
    }
    private void OnEnable() {
        controls.Player.Enable();

        mousePos = controls.Player.MousePosition.ReadValue<Vector2>();

        controls.Player.Move.performed += ctx => pm.moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => pm.moveInput = Vector2.zero;
        controls.Player.Dash.performed += _ => pm.TryStartDash();
    }
    private void Update()
    {
        mousePos = controls.Player.MousePosition.ReadValue<Vector2>();
    }
    private void OnDisable() => controls.Player.Disable();
}