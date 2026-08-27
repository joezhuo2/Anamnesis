using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityStatManager : MonoBehaviour, IStatProvider, ICurrencyHolder, ITeamMember
{
    public EntityStats baseStats;
    protected EntityStats s;
    [HideInInspector] public List<StatBuff> currentBuffs = new();

    public int teamID = 0;

    public int CurrentAmount => s.gold;
    public int TeamID => teamID;

    private void Awake()
    {
        if (baseStats != null) s = Instantiate(baseStats);
    }
    private void Start()
    {
        if (s != null)
        {
            s.currentHp = s.EffMaxHp;
            s.canAttack = true;
            s.isAttacking = false;
            s.canMove = true;
            s.canGainHp = true;
            s.isAlive = true;
            s.isImmune = false;
        }
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

    public bool TrySpend(int amount)
    {
        if (s == null || s.gold < amount) return false;
        s.gold -= amount;
        return true;
    }

    public bool AddCurrency(int amount)
    {
        if (s == null) return false;
        s.gold += amount;
        return true;
    }
}
