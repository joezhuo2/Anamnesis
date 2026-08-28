using System.Collections.Generic;
using System.Linq;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

namespace CrystalFlux.SkillTree
{
    public class PlayerSkillTree : MonoBehaviour, ISkillPointHolder
    {
        public SkillTreeDefinition definition;
        public int SkillPoints { get; private set; }

        [HideInInspector] public readonly List<SkillNodeDef> runtimeNodes = new();
        [HideInInspector] public bool choseStarting;
        private readonly HashSet<string> unlockedNodes = new();
        private List<SkillNodeDef> allNodes = new();

        private void Awake()
        {
            if (definition != null && definition.allNodes != null)
                allNodes = definition.allNodes;
            GenerateRuntimeNodes();
        }

        private void CleanupNodes()
        {
            foreach (var node in runtimeNodes)
            {
                if (node == null) continue;

                if (node.playerUpgrades != null)
                {
                    foreach (var pu in node.playerUpgrades)
                        if (pu != null) DestroyImmediate(pu, true);
                    node.playerUpgrades.Clear();
                }

                if (node.attackUpgrades != null)
                {
                    foreach (var ad in node.attackUpgrades)
                        if (ad != null) DestroyImmediate(ad, true);
                    node.attackUpgrades.Clear();
                }

                DestroyImmediate(node, true);
            }
            runtimeNodes.Clear();
        }

        private void OnDestroy() => CleanupNodes();

        public void SetNodes(List<SkillNodeDef> nodes)
        {
            CleanupNodes();

            allNodes = nodes ?? new List<SkillNodeDef>();
            GenerateRuntimeNodes();
        }

        public void GenerateRuntimeNodes()
        {
            runtimeNodes.Clear();
            var runtimeNodeMap = new Dictionary<SkillNodeDef, SkillNodeDef>();

            foreach (var n in allNodes)
            {
                if (n == null) continue;
                SkillNodeDef runtimeNode = Instantiate(n);

                if (n.playerUpgrades != null && n.playerUpgrades.Count > 0)
                {
                    runtimeNode.playerUpgrades.Clear();
                    foreach (var pu in n.playerUpgrades)
                    {
                        if (pu == null) continue;
                        runtimeNode.playerUpgrades.Add(Instantiate(pu));
                    }
                }

                if (n.attackUpgrades != null && n.attackUpgrades.Count > 0)
                {
                    runtimeNode.attackUpgrades.Clear();
                    foreach (var ad in n.attackUpgrades)
                    {
                        if (ad == null) continue;
                        AttackData rad = Instantiate(ad);
                        rad.DeepClone();
                        runtimeNode.attackUpgrades.Add(rad);
                    }
                }

                runtimeNodes.Add(runtimeNode);
                runtimeNodeMap[n] = runtimeNode;
            }

            UpdateRuntimeNodeRequirements(runtimeNodeMap);
            RestoreUnlockedNodes();
        }

        private void UpdateRuntimeNodeRequirements(Dictionary<SkillNodeDef, SkillNodeDef> runtimeNodeMap)
        {
            foreach (var node in allNodes)
            {
                if (node == null) continue;
                if (!runtimeNodeMap.TryGetValue(node, out var runtimeNode)) continue;

                runtimeNode.prerequisites = RemapNodeList(node.prerequisites, runtimeNodeMap);
                runtimeNode.incompatibleNodes = RemapNodeList(node.incompatibleNodes, runtimeNodeMap);
            }
        }

        private static List<SkillNodeDef> RemapNodeList(List<SkillNodeDef> sourceNodes, IReadOnlyDictionary<SkillNodeDef, SkillNodeDef> runtimeNodeMap)
        {
            if (sourceNodes == null) return new List<SkillNodeDef>();

            var remappedNodes = new List<SkillNodeDef>();
            foreach (var sourceNode in sourceNodes)
            {
                if (sourceNode == null) continue;
                if (runtimeNodeMap.TryGetValue(sourceNode, out var runtimeNode))
                    remappedNodes.Add(runtimeNode);
            }

            return remappedNodes;
        }

        private void RestoreUnlockedNodes()
        {
            if (unlockedNodes.Count == 0) return;

            var workingSavedIds = new HashSet<string>(unlockedNodes);
            unlockedNodes.Clear();

            foreach (var rn in runtimeNodes)
            {
                if (rn == null || string.IsNullOrEmpty(rn.nodeID)) continue;

                if (workingSavedIds.Contains(rn.nodeID))
                    unlockedNodes.Add(rn.nodeID);
            }
        }

        public (bool canUnlock, string failMessage) CanUnlock(SkillNodeDef node)
        {
            if (node == null) return (false, "Node is null");
            if (unlockedNodes.Contains(node.nodeID)) return (false, "Node already unlocked");
            if (node.isStartingNode && choseStarting) return (false, "Starting node already chosen");
            if (SkillPoints < node.cost) return (false, "Not enough skill points");

            if (!node.isStartingNode)
            {

                bool hasUnlockedConnection = false;
                string missingConnectionMsg = "No connected nodes unlocked";

                if (node.prerequisites != null && node.prerequisites.Count > 0)
                {
                    foreach (var prereq in node.prerequisites)
                    {
                        if (prereq != null && unlockedNodes.Contains(prereq.nodeID))
                        {
                            hasUnlockedConnection = true;
                            break;
                        }
                    }
                    if (!hasUnlockedConnection)
                    {
                        var prereqNames = node.prerequisites.Where(p => p != null).Select(p => p.nodeName);
                        missingConnectionMsg = $"Requires one of: {string.Join(", ", prereqNames)}";
                    }
                }

                if (!hasUnlockedConnection)
                {
                    foreach (var otherNode in runtimeNodes)
                    {
                        if (otherNode == null || otherNode == node) continue;
                        if (otherNode.prerequisites != null)
                        {
                            foreach (var prereq in otherNode.prerequisites)
                            {
                                if (prereq != null && prereq.nodeID == node.nodeID && unlockedNodes.Contains(otherNode.nodeID))
                                {
                                    hasUnlockedConnection = true;
                                    break;
                                }
                            }
                            if (hasUnlockedConnection) break;
                        }
                    }
                    if (!hasUnlockedConnection)
                    {
                        var reverseConnections = runtimeNodes
                            .Where(n => n != null && n != node && n.prerequisites != null && n.prerequisites.Any(p => p != null && p.nodeID == node.nodeID))
                            .Select(n => n.nodeName)
                            .ToList();
                        if (reverseConnections.Count > 0)
                            missingConnectionMsg = $"Requires one of: {string.Join(", ", reverseConnections)}";
                    }
                }

                bool hasAnyConnection = (node.prerequisites != null && node.prerequisites.Count > 0) ||
                    runtimeNodes.Any(n => n != null && n != node && n.prerequisites != null && n.prerequisites.Any(p => p != null && p.nodeID == node.nodeID));

                if (!hasAnyConnection) return (false, "Node has no connections");

                if (!hasUnlockedConnection) return (false, missingConnectionMsg);
            }

            if (node.incompatibleNodes != null && node.incompatibleNodes.Count > 0)
            {
                foreach (var n in node.incompatibleNodes)
                    if (unlockedNodes.Contains(n.nodeID)) return (false, $"Incompatible node unlocked: {n.nodeName}");
            }

            if (node.requiredAttacks != null && node.requiredAttacks.Count > 0)
            {
                if (!TryGetComponent<PlayerAttackHandler>(out var pah)) return (false, "Player attack handler not found");

                foreach (var a in node.requiredAttacks)
                    if (!pah.HasAttack(a)) return (false, $"Missing required attack: {a.displayName}");
            }

            if (node.requiredPlayerUpgrades != null && node.requiredPlayerUpgrades.Count > 0)
            {
                if (!TryGetComponent<PlayerUpgradeManager>(out var pum)) return (false, "Player upgrade manager not found");
                if (pum.activeUpgrades == null || pum.activeUpgrades.Count == 0) return (false, "Missing required player upgrades: No active upgrades found");
                foreach (var p in node.requiredPlayerUpgrades)
                    if (!pum.HasUpgrade(p)) return (false, $"Missing required player upgrade: {p.upgradeName}");
            }

            return (true, string.Empty);
        }

        public void UnlockNode(SkillNodeDef node)
        {
            var (canUnlock, _) = CanUnlock(node);
            if (!canUnlock) return;

            TrySpend(node.cost);
            unlockedNodes.Add(node.nodeID);

            node.Apply(gameObject);

            if (node.isStartingNode) choseStarting = true;
        }

        public bool IsNodeUnlocked(SkillNodeDef node) => node != null && unlockedNodes.Contains(node.nodeID);

        public (bool canUndo, string failMessage) CanUndo(SkillNodeDef node)
        {
            if (node == null) return (false, "Node is null");
            if (!IsNodeUnlocked(node)) return (false, "Node not unlocked");
            if (node.isStartingNode) return (false, "Cannot undo starting node");

            if (TryGetComponent<ICurrencyHolder>(out var esm) && esm.CurrentAmount < node.undoCost)
                return (false, $"Not enough gold ({node.undoCost}g required)");
            else return (false, "No stat manager found");
        }

        public void UndoNode(SkillNodeDef node)
        {
            var (canUndo, _) = CanUndo(node);
            if (!canUndo) return;

            if (TryGetComponent<ICurrencyHolder>(out var esm) && esm.TrySpend(node.undoCost))
            {
                AddSkillPoints(node.cost);
                unlockedNodes.Remove(node.nodeID);
            }

            node.Remove(gameObject);
        }

        public void AddSkillPoints(int amount) => SkillPoints += amount;

        public bool TrySpend(int amount)
        {
            if (SkillPoints < amount) return false;
            SkillPoints -= amount;
            return true;
        }
    }
}