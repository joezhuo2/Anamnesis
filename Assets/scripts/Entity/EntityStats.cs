using UnityEngine;

public enum StatType
{
    damagePct,
    attack,
    atkPct,
    attackSpeedPct,
    physicalDmgPct,
    spellDmgPct,
    critChance,
    critDamage,
    aoePct,
    defShred,
    resPen,
    currentHp,
    maxHp,
    hpPct,
    hpRegen,
    hpRegPct,
    armor,
    armorPct,
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
    public float physicalDmgPct;
    public float spellDmgPct;
    public int attack;
    public float atkPct;
    public float attackSpeedPct;
    [Range(0f, 100f)] public float critChance;
    public float critDamage;
    public float aoePct;
    public int defShred;
    public float resPen;
    // lifeStealPct, effectChance

    [Header("Defense")]
    public int currentHp;
    public int EffMaxHp => Mathf.RoundToInt(maxHp * (1f + (hpPct * 0.01f)));
    public int maxHp;
    public float hpPct;
    public int EffHpReg => Mathf.RoundToInt(hpRegen * (1f + (hpRegPct * 0.01f)));
    public int hpRegen;
    public float hpRegPct;
    public int EffArmor => Mathf.RoundToInt(armor * (1f + (armorPct * 0.01f)));
    public int armor;
    public float armorPct;
    [Range(-100f, 100f)] public float damageRes = 0f;
    [Range(0f, 100f)] public float dodgeChance;
    [Range(0f, 100f)] public float dodgeResPct;
    [Range(-100f, 100f)] public float physicalRes = 0f;
    [Range(-100f, 100f)] public float spellRes = 0f;
    // effectRes, critRes, healingPct

    [Header("Movement")]
    public float FinalSpd => moveSpeed * (1f + (moveSpeedPct * 0.01f));
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
