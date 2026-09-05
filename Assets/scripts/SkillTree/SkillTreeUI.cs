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
        private float timeScaleBeforeOpen = 1f;

        private static SkillTreeUI openInstance;
        private static int escapeConsumedFrame = -1;

        public bool IsOpen => isOpen;
        public static bool IsAnyOpen => openInstance != null;
        public static bool EscapeConsumedThisFrame => escapeConsumedFrame == Time.frameCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            openInstance = null;
            escapeConsumedFrame = -1;
        }

        void Awake()
        {
            if (manager == null) manager = FindAnyObjectByType<SkillTreeManager>();
            if (panZoom == null) panZoom = FindAnyObjectByType<SkillTreePanZoom>();
            if (lineRenderer == null) lineRenderer = FindAnyObjectByType<SkillTreeLineRenderer>();
            if (nodeContainer == null) nodeContainer = transform.Find("NodesContainer")?.GetComponent<RectTransform>();

            if (manager != null && manager.tree != null) BuildTree();

            gameObject.SetActive(false);
        }

        public void Toggle(GameObject player)
        {
            if (isOpen)
            {
                Close();
                return;
            }

            if (Time.timeScale == 0f) return;

            if (manager != null) manager.SetPlayer(player);

            Open();
        }

        public void Open()
        {
            if (isOpen) return;

            isOpen = true;
            openInstance = this;
            gameObject.SetActive(true);

            timeScaleBeforeOpen = Time.timeScale;
            Time.timeScale = 0f;

            BuildTree();
        }

        public void Close()
        {
            if (!isOpen) return;

            isOpen = false;
            if (openInstance == this) openInstance = null;
            gameObject.SetActive(false);

            Time.timeScale = timeScaleBeforeOpen;
        }

        public void CloseFromEscape()
        {
            if (!isOpen) return;

            escapeConsumedFrame = Time.frameCount;
            Close();
        }

        private void OnDestroy()
        {
            if (openInstance == this) openInstance = null;
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

            if (lineRenderer != null && manager != null && manager.tree != null)
                lineRenderer.Redraw(manager.tree.runtimeNodes);
        }
    }
}
