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
    ExpBonus,
    Stealing,
    sePotPct,
    seDurPct,
    manaGainPct,
    seTickRatePct,
    maxManaPct,
    maxStaminaPct,
    basicCdRedPct,
    skillCdRedPct,
    ultCdRedPct,
    EffMaxMana,
    EffMaxStamina,
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
    public float EffInt => intelligence * (1f + (intPct * 0.01f)) * (Mathf.Max(1f, 1f + ((EffMaxMana - 100) * 0.01f)));
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
    public float sePotPct;
    public float manaGainPct;
    public float basicCdRedPct;
    public float skillCdRedPct;
    public float ultCdRedPct;
    // lifeStealPct, effectChance

    [Header("Defense")]
    public int currentHp;
    public float CurHpPct => (currentHp / EffMaxHp) * 100f;
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
    public float seDurPct;
    public float seTickRatePct;
    // critRes, healingPct

    [Header("Movement")]
    public float FinalSpd => moveSpeed * (1f + (moveSpeedPct * 0.01f));
    public float moveSpeed;
    public float moveSpeedPct;

    [Header("Stamina - Player Only")]
    public int currentStamina;
    public int maxStamina;
    public float maxStaminaPct;
    public float EffMaxStamina => Mathf.RoundToInt(maxStamina * (1f + (maxStaminaPct * 0.01f)));
    public float EffStReg => staminaRegen * (1f + (stRegPct * 0.01f)) * (Mathf.Max(1f, 1f + ((EffMaxStamina - 100) * 0.01f)));
    public float staminaRegen;
    public float stRegPct;

    [Header("Mana - Player Only")]
    public int currentMana;
    public int maxMana;
    public float maxManaPct;
    public float EffMaxMana => Mathf.RoundToInt(maxMana * (1f + (maxManaPct * 0.01f)));
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
    public float ExpReq => 100 * Mathf.Pow(1.3f, level - 1);
    public float exp;
    public float expBonus;

    [Header("Experience")]
    public float xpDrop;
    public float goldDrop;

    [Header("Gold")]
    public int gold;
    public float stealing;

    [Header("States")]
    public bool isAlive;
    public bool isImmune;
    public bool canAttack;
    public bool isAttacking;
    public bool canMove;
    public bool canGainHp;
}
