using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Node", menuName = "Skill Tree/Node")]
public class SkillNodeDef : ScriptableObject
{
    [Header("Visuals")]
    public string nodeName;
    [TextArea(3, 5)] public string desc;
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

    [Header("Costs")]
    [Tooltip("Gold cost to undo this node (refunds skill point)")]
    public int undoCost = 50;
}