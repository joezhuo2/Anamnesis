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
    public float aoePct;

    [Header("Defense")]
    public int currentHp;
    public int maxHp;
    public int hpRegen;
    public int armor;
    [Range(0, 1)] public float damageRes;
    [Range(0, 100)] public float dodgeChance;
    [Range(0, 1)] public float dodgeResPct;
    [Range(0, 1)] public float physicalRes;
    [Range(0, 1)] public float spellRes;

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
