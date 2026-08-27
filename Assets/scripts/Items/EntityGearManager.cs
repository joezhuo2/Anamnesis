using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityGearManager : MonoBehaviour
{
    public List<GearItem> available;
    public Dictionary<EquipmentSlot, GearItem> equipped = new();
    private Array cachedStatTypes;

    private void Awake()
    {
        cachedStatTypes = Enum.GetValues(typeof(StatType));
    }
    public void IdentifyGear(GearItem item)
    {
        if (item == null || available.Contains(item)) return;

        GearItem i = Instantiate(item);
        i.rolls = new();

        i.baseRoll = ProcessRoll(i.potentialBaseRoll);

        foreach (StatRoll r in i.potentialRolls)
        {
            StatBuff s = ProcessRoll(r);
            i.rolls.Add(s);
        }

        available.Add(i);
    }
    private StatBuff ProcessRoll(StatRoll r)
    {
        StatType type;
        float roll;

        if (r.rollType == StatRollType.PureRandomStatAndRoll)
        {
            int randomIndex = UnityEngine.Random.Range(0, cachedStatTypes.Length);
            type = (StatType)cachedStatTypes.GetValue(randomIndex);
            roll = UnityEngine.Random.Range(r.minRoll, r.maxRoll);
        }
        else if (r.rollType == StatRollType.GuaranteedStatRandomRoll)
        {
            type = r.statType;
            roll = UnityEngine.Random.Range(r.minRoll, r.maxRoll);
        }
        else
        {
            type = r.statType;
            roll = r.minRoll;
        }

        return new StatBuff(type, roll);
    }
    public void EquipGear(EquipmentSlot slot) {}
    public void RemoveGear(EquipmentSlot slot) {}
}