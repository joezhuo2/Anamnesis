using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(PlayerAttackHandler))]
[RequireComponent(typeof(PlayerUI))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerUpgradeManager))]
[RequireComponent(typeof(PlayerLevel))]
public class Player : Entity {}