using System.Collections.Generic;
using System.Linq;
using CrystalFlux.Core;
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

        private void Start() => SkillPoints++;

        private void CleanupNodes()
        {
            foreach (var node in runtimeNodes)
            {
                if (node == null) continue;

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

            if (node.requirements != null)
            {
                foreach (var req in node.requirements)
                {
                    if (req == null) continue;
                    if (!req.Has(gameObject)) return (false, $"Requirement not met");
                }
            }

            return (true, string.Empty);
        }

        public void UnlockNode(SkillNodeDef node)
        {
            var (canUnlock, _) = CanUnlock(node);
            if (!canUnlock) return;

            TrySpend(node.cost);
            unlockedNodes.Add(node.nodeID);

            ApplyNodeEffects(node);

            if (node.isStartingNode) choseStarting = true;
        }

        public bool IsNodeUnlocked(SkillNodeDef node) => node != null && unlockedNodes.Contains(node.nodeID);

        public (bool canUndo, string failMessage) CanUndo(SkillNodeDef node)
        {
            if (node == null) return (false, "Node is null");
            if (!IsNodeUnlocked(node)) return (false, "Node not unlocked");
            if (node.isStartingNode) return (false, "Cannot undo starting node");

            if (!TryGetComponent<ICurrencyHolder>(out var esm)) return (false, "No stat manager found");
            if (esm.CurrentAmount < node.undoCost) return (false, $"Not enough gold ({node.undoCost}g required)");

            return (true, string.Empty);
        }

        public void UndoNode(SkillNodeDef node)
        {
            var (canUndo, _) = CanUndo(node);
            if (!canUndo) return;

            if (TryGetComponent<ICurrencyHolder>(out var esm) && esm.TrySpend(node.undoCost))
            {
                AddSkillPoints(node.cost);
                unlockedNodes.Remove(node.nodeID);
                RemoveNodeEffects(node);
            }
        }

        private void ApplyNodeEffects(SkillNodeDef node)
        {
            if (node.unlockEffects != null)
                foreach (var effect in node.unlockEffects)
                    if (effect != null) effect.Apply(gameObject);
        }

        private void RemoveNodeEffects(SkillNodeDef node)
        {
            if (node.unlockEffects != null)
                foreach (var effect in node.unlockEffects)
                    if (effect != null) effect.Remove(gameObject);
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