using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public GameObject player = null;
    public List<SkillNodeDef> allNodes = new();
    public int skillPoints;

    private readonly List<SkillNodeDef> runtimeNodes = new();
    private readonly HashSet<string> unlockedNodes = new();

    public void Start ()
    {
        // TODO: load player
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

                    if (rad.pd != null)
                        rad.pd = Instantiate(rad.pd);
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

    public bool CanUnlock(SkillNodeDef node)
    {
        if (node == null || skillPoints < 1 || unlockedNodes.Contains(node.nodeID)) return false;

        if (!node.isStartingNode)
        {
            if (node.prerequisites == null || node.prerequisites.Count == 0) return false;

            foreach (var n in node.prerequisites)
                if (!unlockedNodes.Contains(n.nodeID)) return false;
        }

        if (node.incompatibleNodes != null && node.incompatibleNodes.Count > 0)
        {
            foreach (var n in node.incompatibleNodes)
                if (unlockedNodes.Contains(n.nodeID)) return false;
        }

        if (node.requiredAttacks != null && node.requiredAttacks.Count > 0 && player.TryGetComponent<PlayerAttackHandler>(out var pah))
        {
            foreach (var a in node.requiredAttacks)
                if (!pah.HasAttack(a)) return false;
        }

        if (node.requiredPlayerUpgrades != null && node.requiredPlayerUpgrades.Count > 0)
        {
            if (player.TryGetComponent<PlayerUpgradeManager>(out var pum) && pum.activeUpgrades != null && pum.activeUpgrades.Count > 0)
            {
                foreach (var p in node.requiredPlayerUpgrades)
                    if (!pum.HasUpgrade(p)) return false;
            }
        }

        return true;
    }

    public void UnlockNode(SkillNodeDef node)
    {
        if (!CanUnlock(node)) return;

        skillPoints--;
        unlockedNodes.Add(node.nodeID);

        if (player == null) return;
        if (node.statBuffs != null && node.statBuffs.Count > 0) HandleStatUpgrades(node);
        if (node.attackUpgrades != null && node.attackUpgrades.Count > 0) HandleAttackUpgrades(node);
        if (node.playerUpgrades != null && node.playerUpgrades.Count > 0) HandlePlayerUpgrades(node);
    }

    private void HandleStatUpgrades(SkillNodeDef node)
    {
        if (player.TryGetComponent<EntityStatManager>(out var esm))
        {
            foreach (var sb in node.statBuffs)
                esm.AddStat(sb);
        }
    }

    private void HandlePlayerUpgrades(SkillNodeDef node)
    {
        if (player.TryGetComponent<PlayerUpgradeManager>(out var pum))
        {
            foreach (var pu in node.playerUpgrades)
                if (pu != null) pum.AddUpgrade(pu);
        }
    }

    private void HandleAttackUpgrades(SkillNodeDef node)
    {
        if (player.TryGetComponent<PlayerAttackHandler>(out var pah))
        {
            foreach (var ad in node.attackUpgrades)
                if (ad != null) pah.UpdateAttack(ad.type, ad);
        }
    }

    public bool IsNodeUnlocked(SkillNodeDef node) => node != null && unlockedNodes.Contains(node.nodeID);
}