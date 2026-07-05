using System.Collections.Generic;
using UnityEngine;

public class SkillNodeDef : ScriptableObject
{
    [Header("Visuals")]
    public string nodeName;
    public string desc;
    public string nodeID;
    public Sprite icon;

    [Header("Requirements")]
    public bool isStartingNode;
    public List<SkillNodeDef> prerequisites;
    public List<AttackData> requiredAttacks;
    public List<PlayerUpgrade> requiredPlayerUpgrades;
    public List<SkillNodeDef> incompatibleNodes;

    [Header("Upgrades")]
    public List<StatBuff> statBuffs;
    public List<AttackData> attackUpgrades;
    public List<PlayerUpgrade> playerUpgrades;
}