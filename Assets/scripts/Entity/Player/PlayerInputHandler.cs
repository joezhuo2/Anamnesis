using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;
    public static Vector2 mousePos;
    private void Awake()
    {
        controls = new PlayerControls();
    }
    private void OnEnable() {
        controls.Player.Enable();

        mousePos = controls.Player.MousePosition.ReadValue<Vector2>();
    }
    private void Update()
    {
        mousePos = controls.Player.MousePosition.ReadValue<Vector2>();
    }
    private void OnDisable() => controls.Player.Disable();
}