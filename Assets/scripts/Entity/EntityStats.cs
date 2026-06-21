using UnityEngine;

public enum StatType
{
    damagePct,
    attack,
    attackSpeedPct,
    physicalDmgPct,
    spellDmgPct,
    critChance,
    critDamage,
    aoePct,
    currentHp,
    maxHp,
    hpRegen,
    armor,
    damageRes,
    dodgeChance,
    dodgeResPct,
    physicalRes,
    spellRes,
    moveSpeed,
    moveSpeedPct
}

[CreateAssetMenu(fileName = "stats", menuName = "Scriptable Objects/stats/entity")]
public class EntityStats : ScriptableObject
{
    [Header("Offense")]
    public float damagePct;
    public int attack;
    public float attackSpeedPct;
    public float physicalDmgPct;
    public float spellDmgPct;
    [Range(0f, 100f)] public float critChance;
    public float critDamage;
    public float aoePct;

    [Header("Defense")]
    public int currentHp;
    public int maxHp;
    public int hpRegen;
    public int armor;
    [Range(0f, 1f)] public float damageRes;
    [Range(0f, 100f)] public float dodgeChance;
    [Range(0f, 1f)] public float dodgeResPct;
    [Range(0f, 1f)] public float physicalRes;
    [Range(0f, 1f)] public float spellRes;

    [Header("Movement")]
    public float moveSpeed;
    public float moveSpeedPct;

    [Header("Misc")]
    public int level;
    public bool isAlive;
    public bool isImmune;
    public bool canAttack;
    public bool isAttacking;
    public bool canMove;
    public bool canGainHp;
}
