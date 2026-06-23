using UnityEngine;

[CreateAssetMenu(fileName = "player_stats", menuName = "Scriptable Objects/stats/player")]
public class PlayerStats : EntityStats
{
    [Header("Dash")]
    public float dashSpeedMult;
    public float dashCooldown;
    public float dashDistance;
    public int dashStaminaCost;
    public bool dashShouldApplyIFrame;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public bool canDash;
    [HideInInspector] public bool canGainStamina;
    [HideInInspector] public bool canGainMana;

    // [Header("Skill points")]
    // public int agility;
    // public int defense;
    // public int strength;
    // public int dexterity;
    // public int intellegence;
    // public int vitality;

    // [Header("Levelling")]
    // public int currentXp;
    // public int xpBonus;
}
