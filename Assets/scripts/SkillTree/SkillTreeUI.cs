using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CrystalFlux.SettingsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CrystalFlux.SkillTree
{
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("References")]
        public SkillTreeManager manager;
        public RectTransform nodeContainer;
        public SkillTreeLineRenderer lineRenderer;
        public SkillTreePanZoom panZoom;
        public SkillTreeRefundAllButton refundAllButton;

        [Header("Search")]
        public TMP_InputField searchField;
        [Tooltip("Builds a default search field at runtime when none is assigned")]
        public bool createSearchFieldIfMissing = true;
        public Vector2 searchFieldSize = new(360f, 44f);
        public Vector2 searchFieldOffset = new(0f, -30f);
        public string searchPlaceholder = "Search nodes (name, stat, attack...)";

        private readonly Dictionary<SkillNodeDef, SkillNodeUI> nodeUIMap = new();
        private readonly Dictionary<SkillNodeDef, string> searchTextCache = new();
        private readonly List<string> tooltipBuf = new();
        private string[] searchTerms = Array.Empty<string>();
        private int searchEndFrame = -1;
        private bool isOpen;
        private float timeScaleBeforeOpen = 1f;

        private static SkillTreeUI openInstance;
        private static int escapeConsumedFrame = -1;

        public bool IsOpen => isOpen;
        public static bool IsAnyOpen => openInstance != null;
        public static bool EscapeConsumedThisFrame => escapeConsumedFrame == Time.frameCount;
        public static bool IsTypingSearch => openInstance != null && openInstance.searchField != null && openInstance.searchField.isFocused;

        public static bool IsPointerOverSearch(Vector2 screenPos)
        {
            if (openInstance == null || openInstance.searchField == null || !openInstance.searchField.isActiveAndEnabled) return false;
            return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)openInstance.searchField.transform, screenPos, null);
        }

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
            if (refundAllButton == null) refundAllButton = GetComponentInChildren<SkillTreeRefundAllButton>(true);

            SetupSearchField();

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

            HitFeedback.Cancel();
            timeScaleBeforeOpen = Time.timeScale;
            Time.timeScale = 0f;

            if (manager != null && manager.tree != null) manager.tree.BeginGrace();

            BuildTree();
        }

        public void Close()
        {
            if (!isOpen) return;

            isOpen = false;
            if (openInstance == this) openInstance = null;
            gameObject.SetActive(false);

            if (manager != null && manager.tree != null) manager.tree.EndGrace();

            Time.timeScale = timeScaleBeforeOpen;
        }

        public void CloseFromEscape()
        {
            if (!isOpen) return;

            escapeConsumedFrame = Time.frameCount;

            if (searchField != null && (searchField.isFocused || searchEndFrame == Time.frameCount))
            {
                searchField.DeactivateInputField();
                return;
            }

            Close();
        }

        private void Update()
        {
            if (!isOpen || searchField == null || Mouse.current == null) return;
            if (!Mouse.current.rightButton.wasPressedThisFrame) return;
            if (!IsPointerOverSearch(Mouse.current.position.ReadValue())) return;

            ClearSearch();
        }

        public void ClearSearch()
        {
            if (searchField == null) return;

            searchField.DeactivateInputField();
            searchField.text = string.Empty;
        }

        private void OnDestroy()
        {
            if (openInstance == this) openInstance = null;

            if (searchField != null)
            {
                searchField.onValueChanged.RemoveListener(OnSearchChanged);
                searchField.onEndEdit.RemoveListener(OnSearchEndEdit);
            }
        }

        private void SetupSearchField()
        {
            if (searchField == null && createSearchFieldIfMissing)
            {
                GameObject go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
                go.name = "SkillTreeSearch";
                go.transform.SetParent(transform, false);

                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = searchFieldSize;
                rt.anchoredPosition = searchFieldOffset;

                if (go.TryGetComponent<Image>(out var bg)) bg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);

                searchField = go.GetComponent<TMP_InputField>();
                if (searchField.textComponent != null) searchField.textComponent.color = Color.white;
                if (searchField.placeholder is TMP_Text ph)
                {
                    ph.text = searchPlaceholder;
                    ph.color = new Color(1f, 1f, 1f, 0.4f);
                }
            }

            if (searchField == null) return;

            searchField.transform.SetAsLastSibling();
            searchField.restoreOriginalTextOnEscape = false;
            searchField.onValueChanged.AddListener(OnSearchChanged);
            searchField.onEndEdit.AddListener(OnSearchEndEdit);
        }

        private void OnSearchEndEdit(string _) => searchEndFrame = Time.frameCount;

        private void OnSearchChanged(string q)
        {
            searchTerms = string.IsNullOrWhiteSpace(q)
                ? Array.Empty<string>()
                : q.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            ApplySearch();
        }

        private void ApplySearch()
        {
            bool active = searchTerms.Length > 0;

            foreach (var kv in nodeUIMap)
            {
                if (kv.Value == null) continue;
                kv.Value.SetSearchState(!active ? 0 : NodeMatches(kv.Key) ? 1 : -1);
            }
        }

        private bool NodeMatches(SkillNodeDef n)
        {
            if (n == null) return false;

            if (!searchTextCache.TryGetValue(n, out var hay))
            {
                hay = BuildSearchText(n);
                searchTextCache[n] = hay;
            }

            foreach (var t in searchTerms)
                if (hay.IndexOf(t, StringComparison.OrdinalIgnoreCase) < 0) return false;

            return true;
        }

        private string BuildSearchText(SkillNodeDef n)
        {
            StringBuilder sb = new();
            sb.Append(n.nodeName).Append('\n').Append(n.desc).Append('\n').Append(n.nodeID);

            if (n.unlockEffects == null) return sb.ToString();

            foreach (var e in n.unlockEffects)
            {
                if (e is not UnlockEffect ue) continue;

                foreach (var b in ue.buffs)
                    sb.Append('\n').Append(b.type).Append('\n').Append(b.ToString());

                tooltipBuf.Clear();
                foreach (var a in ue.attacks)
                {
                    if (a == null) continue;
                    sb.Append('\n').Append(a.name).Append('\n').Append(a.type);
                    a.GetTooltipLines(tooltipBuf);
                }
                foreach (var u in ue.awakenings)
                {
                    if (u == null) continue;
                    sb.Append('\n').Append(u.name).Append('\n').Append(u.upgradeName);
                    u.GetTooltipLines(tooltipBuf);
                }
                foreach (var line in tooltipBuf) sb.Append('\n').Append(line);
            }

            return sb.ToString();
        }

        public void BuildTree()
        {
            if (manager == null || manager.tree == null || nodeContainer == null)
                return;

            nodeUIMap.Clear();
            searchTextCache.Clear();

            var existingNodeUIs = nodeContainer.GetComponentsInChildren<SkillNodeUI>(true);
            var runtimeNodes = manager.tree.runtimeNodes;

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
            RefreshRefundAllButton();
            ApplySearch();
        }

        private void RefreshRefundAllButton()
        {
            if (refundAllButton == null) refundAllButton = GetComponentInChildren<SkillTreeRefundAllButton>(true);
            if (refundAllButton == null) return;

            if (refundAllButton.manager == null) refundAllButton.manager = manager;
            if (refundAllButton.treeUI == null) refundAllButton.treeUI = this;
            refundAllButton.RefreshVisuals();
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

            RefreshRefundAllButton();
        }
    }
}
