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
    moveSpeedPct,
    maxStamina,
    staminaRegen,
    stRegPct,
    addPhysDmgPct,
    addSplDmgPct,
    EffMaxHp,
    EffAtk,
    EffHpReg,
    EffStReg,
    EffSpd,
    EffArmor,
    maxMana,
    globalDoTCanCrit,
    UltDmgPct,
    SkillDmgPct,
    BasicDmgPct,
    EffectRes,
    Intelligence,
    IntPct,
    EffInt,
    ProjSpd,
    addTrueDmgPct,
    stCostPct,
    dashCooldownRedPct,
    dashDistancePct,
    dashStaminaCostRedPct,
    addDmgPct,
    kbRes,
    kbPct,
}

public class EntityStats : ScriptableObject
{
    [Header("Offense")]
    public float damagePct;
    public float physicalDmgPct;
    public float spellDmgPct;
    public float EffAtk => attack * (1f + (atkPct * 0.01f));
    public int attack;
    public float atkPct;
    public float EffInt => intelligence * (1f + (intPct * 0.01f)) * (Mathf.Max(1f, 1f + ((maxMana - 100) * 0.01f)));
    public int intelligence;
    public float intPct;
    public float attackSpeedPct;
    [Range(0f, 100f)] public float critChance;
    public float critDamage;
    public float aoePct;
    public int defShred;
    public float resPen;
    public float addPhysDmgPct;
    public float addSplDmgPct;
    public float addTrueDmgPct;
    public float addDmgPct;
    public float basicDmgPct;
    public float skillDmgPct;
    public float ultDmgPct;
    public float projSpd;
    public float stCostPct;
    public float kbPct;
    // lifeStealPct, effectChance

    [Header("Defense")]
    public int currentHp;
    public float CurHpPct => (currentHp / maxHp) * 100f;
    public int EffMaxHp => Mathf.RoundToInt(maxHp * (1f + (hpPct * 0.01f)));
    public int maxHp;
    public float hpPct;
    public float EffHpReg => hpRegen * (1f + (hpRegPct * 0.01f));
    public float hpRegen;
    public float hpRegPct;
    public float EffArmor => armor * (1f + (armorPct * 0.01f));
    public float ArmorRes => EffArmor / (EffArmor + 100f);
    public int armor;
    public float armorPct;
    [Range(-100f, 100f)] public float damageRes;
    [Range(0f, 100f)] public float dodgeChance;
    [Range(0f, 100f)] public float dodgeResPct;
    [Range(-100f, 100f)] public float physicalRes;
    [Range(-100f, 100f)] public float spellRes;
    public float hurtTime = 0.3f;
    public float effectRes = 0f;
    public float kbRes;
    // critRes, healingPct

    [Header("Movement")]
    public float FinalSpd => moveSpeed * (1f + (moveSpeedPct * 0.01f));
    public float moveSpeed;
    public float moveSpeedPct;

    [Header("Stamina - Player Only")]
    public int currentStamina;
    public int maxStamina;
    public float EffStReg => staminaRegen * (1f + (stRegPct * 0.01f)) * (Mathf.Max(1f, 1f + ((maxStamina - 100) * 0.01f)));
    public float staminaRegen;
    public float stRegPct;

    [Header("Mana - Player Only")]
    public int currentMana;
    public int maxMana;
    [HideInInspector] public bool canGainMana;

    [Header("Dash - Player Only")]
    public float dashSpeedMult;
    public float dashCooldown;
    public float dashCooldownRedPct;
    public float EffDashCooldown => Mathf.Max(0f, dashCooldown * (1f - (dashCooldownRedPct * 0.01f)));
    public float dashDistance;
    public float dashDistancePct;
    public float EffDashDistance => Mathf.Max(0f, dashDistance * (1f + (dashDistancePct * 0.01f)));
    public int dashStaminaCost;
    public float dashStaminaCostRedPct;
    public float EffDashStaminaCost => Mathf.Max(0f, dashStaminaCost * (1f - (dashStaminaCostRedPct * 0.01f)));
    public bool dashShouldApplyIFrame;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public bool canDash;
    [HideInInspector] public bool canGainStamina;

    [Header("Levelling")]
    public int level;

    [Header("Misc")]
    public bool isAlive;
    public bool isImmune;
    public bool canAttack;
    public bool isAttacking;
    public bool canMove;
    public bool canGainHp;
}
