using System.Collections.Generic;
using UnityEngine;

public enum StatRollType
{
    GuaranteedStatAndRoll,
    GuaranteedStatRandomRoll,
    PureRandomStatAndRoll
}

[System.Serializable]
public struct StatRoll
{
    public StatRollType rollType;
    public string name;
    public float maxRoll;
    public float minRoll;
}

public enum EquipmentSlot
{
    Head,
    Chest,
    Legs,
    Feet,
    Weapon,
    Offhand
}

public class GearItem : Item
{
    public EquipmentSlot slot;
    public StatRoll potentialBaseRoll;
    public StatBuff baseRoll;
    public List<StatRoll> potentialRolls;
    public List<StatBuff> rolls;
}