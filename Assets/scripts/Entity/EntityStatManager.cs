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

        s.attack += 4 * levelOffset;
        s.atkPct += 3f * levelOffset;

        s.maxHp += 12 * levelOffset;
        s.hpPct += 8f * levelOffset;

        s.hpRegen = Mathf.RoundToInt(s.hpRegen * (1f + (0.02f * levelOffset)));
        s.hpRegPct += levelOffset;

        s.armor += 4 * levelOffset;
        s.armorPct += 2f * levelOffset;

        s.damagePct += levelOffset * 1.5f;

        s.moveSpeedPct = Mathf.Clamp(s.moveSpeedPct * (1f + (0.03f * levelOffset)), -100f, 100f);

        if (levelOffset % 5 == 0)
        {
            s.physicalDmgPct  += 0.6f * levelOffset; //3% per 5 lvs
            s.spellDmgPct += 0.6f * levelOffset; // 3% per 5 lvs
            s.aoePct += 2f * levelOffset; // 10% per 5 lvs

            s.critChance = Mathf.Clamp(s.critChance * (1f + (0.03f * levelOffset)), 0f, 100f); // 1.15x per 5 lvs
            s.critDamage += 2f * levelOffset; // 10% per 5 lvs

            s.damageRes = Mathf.Clamp(s.damageRes + (0.4f * levelOffset), 0f, 50f); // 2% per 5 lvs (125)
            s.physicalRes = Mathf.Clamp(s.physicalRes + (0.6f * levelOffset), -100f, 60f); // 3% per 5 lvs (100)
            s.spellRes = Mathf.Clamp(s.spellRes + (0.6f * levelOffset), -100f, 60f); // 3% per 5 lvs (100)

            s.dodgeChance = Mathf.Clamp(s.dodgeChance + (0.3f * levelOffset), 0f, 45f); // 1.5% per 5 lvs (150)
            s.dodgeResPct = Mathf.Clamp(s.dodgeResPct + (0.5f * levelOffset), 0f, 60f); // 2.5% per 5 lvs (120)
        }
    }

    private void OnDestroy()
    {
        if (s != null) Destroy(s);
    }
    public float GetStat(StatType type)
    {
        return s == null ? 0f : s.GetValue(type);
    }

    public void AddStat(StatBuff b, bool isAdding = true)
    {
        if (s == null || b.IsUnityNull()) return;
        float mod = b.value * (isAdding ? 1f : -1f);
        s.Apply(b.type, mod);
    }
}
