using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(PlayerAttackHandler))]
[RequireComponent(typeof(PlayerUI))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerUpgradeManager))]
public class Player : Entity {}