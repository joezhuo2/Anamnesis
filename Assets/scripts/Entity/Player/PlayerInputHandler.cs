using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CrystalFlux.EntitySystem
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerControls controls;
        private PlayerMovement pm;
        private PlayerAttackHandler pah;

        private void Awake()
        {
            controls = new PlayerControls();
            pm = GetComponent<PlayerMovement>();
            pah = GetComponent<PlayerAttackHandler>();
        }
        private void OnEnable()
        {
            controls.Player.Enable();

            InputState.mousePos = controls.Player.MousePosition.ReadValue<Vector2>();

            controls.Player.Move.performed += OnMovePerformed;
            controls.Player.Move.canceled += OnMoveCanceled;
            controls.Player.Dash.performed += OnDashPerformed;

            controls.Player.BasicAttack.performed += OnBasicAttackPerformed;
            controls.Player.Skill.performed += OnSkillPerformed;
            controls.Player.Ultimate.performed += OnUltimatePerformed;
            controls.Player.Technique.performed += OnTechniquePerformed;
        }
        private void Update() => InputState.mousePos = controls.Player.MousePosition.ReadValue<Vector2>();
        private void OnDisable()
        {
            controls.Player.Move.performed -= OnMovePerformed;
            controls.Player.Move.canceled -= OnMoveCanceled;
            controls.Player.Dash.performed -= OnDashPerformed;

            controls.Player.BasicAttack.performed -= OnBasicAttackPerformed;
            controls.Player.Skill.performed -= OnSkillPerformed;
            controls.Player.Ultimate.performed -= OnUltimatePerformed;
            controls.Player.Technique.performed -= OnTechniquePerformed;

            controls.Player.Disable();
        }
        private void OnDestroy() => controls?.Dispose();

        private void OnMovePerformed(InputAction.CallbackContext ctx) => pm.moveInput = ctx.ReadValue<Vector2>();
        private void OnMoveCanceled(InputAction.CallbackContext ctx) => pm.moveInput = Vector2.zero;
        private void OnDashPerformed(InputAction.CallbackContext ctx) => pm.TryStartDash();
        private void OnBasicAttackPerformed(InputAction.CallbackContext ctx) => pah.PerformAttack(AttackType.Basic);
        private void OnSkillPerformed(InputAction.CallbackContext ctx) => pah.PerformAttack(AttackType.Skill);
        private void OnUltimatePerformed(InputAction.CallbackContext ctx) => pah.PerformAttack(AttackType.Ultimate);
        private void OnTechniquePerformed(InputAction.CallbackContext ctx) => pah.PerformAttack(AttackType.Technique);
    }
}
