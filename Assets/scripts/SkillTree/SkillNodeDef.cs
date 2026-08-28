using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
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
        public int cost = 1;

        [Header("Requirements")]
        public bool isStartingNode;
        public List<SkillNodeDef> prerequisites;
        public List<SkillNodeDef> incompatibleNodes;
        public List<AttackData> requiredAttacks;
        public List<PlayerUpgrade> requiredPlayerUpgrades;

        [Header("Upgrades")]
        public List<StatBuff> statBuffs;
        public List<AttackData> attackUpgrades;
        public List<PlayerUpgrade> playerUpgrades;

        [Header("Costs")]
        [Tooltip("Gold cost to undo this node (refunds skill point)")]
        public int undoCost = 50;

        public void Apply(GameObject target)
        {
            if (statBuffs != null && statBuffs.Count > 0) HandleStatUpgrades(target);
            if (attackUpgrades != null && attackUpgrades.Count > 0) HandleAttackUpgrades(target);
            if (playerUpgrades != null && playerUpgrades.Count > 0) HandlePlayerUpgrades(target);
        }

        public void Remove(GameObject target)
        {
            if (statBuffs != null && statBuffs.Count > 0) HandleStatDowngrades(target);
            if (attackUpgrades != null && attackUpgrades.Count > 0) HandleAttackDowngrades(target);
            if (playerUpgrades != null && playerUpgrades.Count > 0) HandlePlayerDowngrades(target);
        }

        private void HandleStatUpgrades(GameObject target)
        {
            if (target.TryGetComponent<IStatProvider>(out var esm))
            {
                foreach (var sb in statBuffs)
                    esm.AddStat(sb);
            }
        }

        private void HandlePlayerUpgrades(GameObject target)
        {
            if (target.TryGetComponent<PlayerUpgradeManager>(out var pum))
            {
                foreach (var pu in playerUpgrades)
                    if (pu != null) pum.AddUpgrade(pu);
            }
        }

        private void HandleAttackUpgrades(GameObject target)
        {
            if (target.TryGetComponent<PlayerAttackHandler>(out var pah))
            {
                foreach (var ad in attackUpgrades)
                    if (ad != null) pah.UpdateAttack(ad.type, ad);
            }
        }

        private void HandleStatDowngrades(GameObject target)
        {
            if (target.TryGetComponent<IStatProvider>(out var esm))
            {
                foreach (var sb in statBuffs)
                    esm.AddStat(sb, false);
            }
        }

        private void HandlePlayerDowngrades(GameObject target)
        {
            if (target.TryGetComponent<PlayerUpgradeManager>(out var pum))
            {
                foreach (var pu in playerUpgrades)
                    if (pu != null) pum.RemoveUpgrade(pu);
            }
        }

        private void HandleAttackDowngrades(GameObject target)
        {
            if (target.TryGetComponent<PlayerAttackHandler>(out var pah))
            {
                foreach (var ad in attackUpgrades)
                    if (ad != null) pah.RemoveAttack(ad.type);
            }
        }
    }
}
