using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CrystalFlux.SkillTree
{
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("References")]
        public SkillTreeManager manager;
        public RectTransform nodeContainer;
        public SkillTreeLineRenderer lineRenderer;
        public SkillTreePanZoom panZoom;

        private readonly Dictionary<SkillNodeDef, SkillNodeUI> nodeUIMap = new();
        private bool isOpen;

        void Awake()
        {
            if (manager == null) manager = FindAnyObjectByType<SkillTreeManager>();
            if (panZoom == null) panZoom = FindAnyObjectByType<SkillTreePanZoom>();
            if (lineRenderer == null) lineRenderer = FindAnyObjectByType<SkillTreeLineRenderer>();
            if (nodeContainer == null) nodeContainer = transform.Find("NodesContainer")?.GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        void Start()
        {
            if (manager != null && manager.tree != null)
                BuildTree();
        }

        public void Toggle(GameObject player)
        {
            if (Time.timeScale == 0f && !isOpen) return;

            if (manager != null) manager.SetPlayer(player);

            isOpen = !isOpen;
            gameObject.SetActive(isOpen);

            if (isOpen)
            {
                Time.timeScale = 0f;

                BuildTree();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        public void BuildTree()
        {
            if (manager == null || manager.tree == null || nodeContainer == null)
                return;

            nodeUIMap.Clear();

            var existingNodeUIs = nodeContainer.GetComponentsInChildren<SkillNodeUI>(true);
            var runtimeNodes = manager.tree.runtimeNodes;

            var idList = new List<string>();
            foreach (var n in runtimeNodes) idList.Add(n != null ? n.nodeID : "<null>");

            foreach (var nodeUI in existingNodeUIs)
            {
                var node = FindMatchingRuntimeNode(nodeUI, runtimeNodes);
                if (node != null)
                {
                    nodeUI.Initialize(node, manager);
                    nodeUIMap[node] = nodeUI;
                }
            }

            if (lineRenderer != null) lineRenderer.Redraw(runtimeNodes);
        }

        private SkillNodeDef FindMatchingRuntimeNode(SkillNodeUI nodeUI, IReadOnlyList<SkillNodeDef> runtimeNodes)
        {
            string nodeIdFromName = NormalizeId(nodeUI.name.Replace("Node_", ""));
            foreach (var node in runtimeNodes)
            {
                if (node == null) continue;

                if (NormalizeId(node.nodeID) == nodeIdFromName) return node;
                if (!string.IsNullOrEmpty(node.nodeName) && NormalizeId(node.nodeName) == nodeIdFromName) return node;
            }
            return null;
        }

        private static string NormalizeId(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return Regex.Replace(s.ToLowerInvariant(), "[^a-z0-9]", "");
        }

        public void OnNodeStateChanged(SkillNodeDef node)
        {
            foreach (var nUI in nodeUIMap.Values)
                if (nUI != null) nUI.RefreshVisuals();

            if (lineRenderer != null) lineRenderer.Redraw(manager.tree.runtimeNodes);
        }
    }
}