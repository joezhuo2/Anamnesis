using System.Collections.Generic;
using System.Linq;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
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
        private readonly Dictionary<string, SkillNodeDef> nodesById = new();
        private readonly Dictionary<string, List<SkillNodeDef>> neighbours = new();
        private static readonly List<SkillNodeDef> EmptyNodes = new();

        private void Awake()
        {
            if (definition != null && definition.allNodes != null)
                allNodes = new List<SkillNodeDef>(definition.allNodes);
            GenerateRuntimeNodes();
        }

        private void Start() => SkillPoints++;

        private void CleanupNodes()
        {
            foreach (var node in runtimeNodes)
            {
                if (node == null) continue;

                Destroy(node);
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
            BuildAdjacency();
            RestoreUnlockedNodes();
        }

        private void BuildAdjacency()
        {
            nodesById.Clear();
            neighbours.Clear();

            foreach (var n in runtimeNodes)
            {
                if (n == null || string.IsNullOrEmpty(n.nodeID)) continue;

                nodesById[n.nodeID] = n;
                if (!neighbours.ContainsKey(n.nodeID)) neighbours[n.nodeID] = new List<SkillNodeDef>();
            }

            foreach (var n in runtimeNodes)
            {
                if (n == null || n.prerequisites == null || string.IsNullOrEmpty(n.nodeID)) continue;

                foreach (var prereq in n.prerequisites)
                {
                    if (prereq == null || string.IsNullOrEmpty(prereq.nodeID)) continue;
                    if (prereq.nodeID == n.nodeID) continue;

                    Link(n.nodeID, prereq);
                    Link(prereq.nodeID, n);
                }
            }
        }

        private void Link(string id, SkillNodeDef other)
        {
            if (!neighbours.TryGetValue(id, out var list))
            {
                list = new List<SkillNodeDef>();
                neighbours[id] = list;
            }

            if (!list.Contains(other)) list.Add(other);
        }

        private List<SkillNodeDef> Neighbours(string id)
            => id != null && neighbours.TryGetValue(id, out var list) ? list : EmptyNodes;

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
                var connected = Neighbours(node.nodeID);

                if (connected.Count == 0) return (false, "Node has no connections");

                bool hasUnlockedConnection = false;
                foreach (var nb in connected)
                {
                    if (nb != null && unlockedNodes.Contains(nb.nodeID))
                    {
                        hasUnlockedConnection = true;
                        break;
                    }
                }

                if (!hasUnlockedConnection)
                {
                    var names = connected.Where(n => n != null).Select(n => n.nodeName);
                    return (false, $"Requires one of: {string.Join(", ", names)}");
                }
            }

            if (node.incompatibleNodes != null && node.incompatibleNodes.Count > 0)
            {
                foreach (var n in node.incompatibleNodes)
                {
                    if (n == null) continue;
                    if (unlockedNodes.Contains(n.nodeID)) return (false, $"Incompatible node unlocked: {n.nodeName}");
                }
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

            ConsumeNodeRequirements(node);
            ApplyNodeEffects(node);

            if (node.isStartingNode) choseStarting = true;
        }

        public bool IsNodeUnlocked(SkillNodeDef node) => node != null && unlockedNodes.Contains(node.nodeID);

        public (bool canUndo, string failMessage) CanUndo(SkillNodeDef node)
        {
            if (node == null) return (false, "Node is null");
            if (GameSettings.Current.ironmanMode) return (false, "Disabled in Ironman Mode");
            if (!IsNodeUnlocked(node)) return (false, "Node not unlocked");

            if (!TryGetComponent<ICurrencyHolder>(out var esm)) return (false, "No stat manager found");
            if (esm.CurrentAmount < node.undoCost) return (false, $"Not enough gold ({node.undoCost}g required)");
            if (WouldStrandDependents(node)) return (false, "Other unlocked nodes depend on this one");

            return (true, string.Empty);
        }

        private bool WouldStrandDependents(SkillNodeDef removed)
        {
            var reachable = new HashSet<string>();
            var pending = new Queue<string>();

            foreach (var rn in runtimeNodes)
            {
                if (rn == null || !rn.isStartingNode) continue;
                if (rn.nodeID == removed.nodeID || !unlockedNodes.Contains(rn.nodeID)) continue;

                if (reachable.Add(rn.nodeID)) pending.Enqueue(rn.nodeID);
            }

            while (pending.Count > 0)
            {
                foreach (var nb in Neighbours(pending.Dequeue()))
                {
                    if (nb == null || nb.nodeID == removed.nodeID) continue;
                    if (!unlockedNodes.Contains(nb.nodeID)) continue;

                    if (reachable.Add(nb.nodeID)) pending.Enqueue(nb.nodeID);
                }
            }

            foreach (var id in unlockedNodes)
            {
                if (id == removed.nodeID) continue;
                if (!reachable.Contains(id)) return true;
            }

            return false;
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
                RestoreNodeRequirements(node);

                if (node.isStartingNode) choseStarting = false;
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

        private void ConsumeNodeRequirements(SkillNodeDef node)
        {
            if (node.requirements == null) return;

            foreach (var req in node.requirements)
                if (req is NodeRequirement nr) nr.Consume(gameObject);
        }

        private void RestoreNodeRequirements(SkillNodeDef node)
        {
            if (node.requirements == null) return;

            foreach (var req in node.requirements)
                if (req is NodeRequirement nr) nr.Restore(gameObject);
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
