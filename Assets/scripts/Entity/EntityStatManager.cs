using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityStatManager : MonoBehaviour
{
    public EntityStats baseStats;
    [HideInInspector] public EntityStats s;
    [HideInInspector] public List<StatBuff> currentBuffs = new();
    private void Awake()
    {
        if (baseStats != null) s = Instantiate(baseStats);

        if (gameObject.CompareTag("Enemy") && s.level > 1)
            ScaleBaseStats(s.level);
    }
    private void Start()
    {
        s.currentHp = s.EffMaxHp;
        s.canAttack = true;
        s.isAttacking = false;
        s.canMove = true;
        s.canGainHp = true;
        s.isAlive = true;
        s.isImmune = false;
    }

    public void ScaleStatsToLevel(int targetLevel)
    {
        if (s == null) return;

        s.level = targetLevel;

        if (s.level > 1) ScaleBaseStats(s.level);
    }
    private void ScaleBaseStats(int currentLevel)
    {
        int levelOffset = currentLevel - 1;
        if (levelOffset <= 0) return;

        const float atkGrowth = 1.05f;
        const float hpGrowth = 1.1f;
        const float armorGrowth = 1.05f;
        const float utilityGrowth = 1.04f;

        float atkMult = Mathf.Pow(atkGrowth, levelOffset);
        float hpMult = Mathf.Pow(hpGrowth, levelOffset);
        float armorMult = Mathf.Pow(armorGrowth, levelOffset);
        float utilMult = Mathf.Pow(utilityGrowth, levelOffset);

        s.attack = Mathf.RoundToInt(s.attack * atkMult);
        s.critDamage *= atkMult;
        s.maxHp = Mathf.RoundToInt(s.maxHp * hpMult);
        s.hpRegen = Mathf.RoundToInt(s.hpRegen * hpMult);
        s.armor = Mathf.RoundToInt(s.armor * armorMult);

        s.aoePct *= utilMult;
        s.moveSpeedPct = Mathf.Clamp(s.moveSpeedPct * utilMult, -100f, 100f);
        s.critChance = Mathf.Clamp(s.critChance * utilMult, 0f, 100f);
    }

    private void OnDestroy()
    {
        if (s != null) Destroy(s);
    }
    public float GetStat(StatType type) => s == null ? 0f : s.GetValue(type);

    public void AddStat(StatBuff b, bool isAdding = true)
    {
        if (s == null || b.IsUnityNull()) return;
        float mod = b.value * (isAdding ? 1f : -1f);
        s.Apply(b.type, mod);
    }
}
