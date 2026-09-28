using CrystalFlux.EntitySystem;
using CrystalFlux.SkillTree;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerResourcePool))]
[RequireComponent(typeof(PlayerAttackHandler))]
[RequireComponent(typeof(PlayerUI))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerUpgradeManager))]
[RequireComponent(typeof(PlayerLevel))]
[RequireComponent(typeof(PlayerSkillTree))]
[RequireComponent(typeof(SkillTreeInputToggle))]
public class Player : Entity {}
