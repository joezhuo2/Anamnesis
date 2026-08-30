using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.SkillTree
{
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
        public List<SkillNodeDef> incompatibleNodes;
        [SerializeReference, TypeSelector] public List<IUnlockRequirement> requirements;

        [Header("Upgrades")]
        [SerializeReference, TypeSelector] public List<IUnlockEffect> unlockEffects;

        [Header("Costs")]
        [Tooltip("Skill points required to unlock the node")] public int cost = 1;
        [Tooltip("Currency cost to undo this node (refunds skill point)")] public int undoCost = 50;
    }
}
