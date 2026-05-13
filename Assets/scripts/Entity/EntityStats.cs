using UnityEngine;

[CreateAssetMenu(fileName = "stats", menuName = "Scriptable Objects/stats/entity")]
public class EntityStats : ScriptableObject
{
    [Header("Offense")]
    public int damagePct;
    public int attack;
    public int attackSpeedPct;
    public int physicalDmgPct;
    public int spellDmgPct;
    public int critChance;
    public int critDamage;

    [Header("Defense")]
    public int currentHp;
    public int maxHp;
    public int hpRegen;
    public int armor;
    public float damageRes;
    public float dodgeChance;
    public float dodgeResPct;
    public float physicalRes;
    public float spellRes;

    [Header("Movement")]
    public int moveSpeed;
    public int moveSpeedPct;

    [Header("Misc")]
    public int level;
    [HideInInspector] public bool isAlive;
    [HideInInspector] public bool isImmune;
    [HideInInspector] public bool canAttack;
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool canMove;
    [HideInInspector] public bool canGainHp;
}
