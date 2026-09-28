using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SkillTree
{
    public class SkillTreeLineRenderer : MonoBehaviour
    {
        [Header("Line Settings")]
        [Tooltip("Parent for the line images. Defaults to the SkillTreeUI's node container so lines pan/zoom with the nodes.")]
        public RectTransform lineParent;
        [Tooltip("Optional sprite for the lines. Falls back to a white 1x1 sprite.")]
        public Sprite lineSprite;
        public float lineWidth = 3f;
        public Color lockedColor = new(0.4f, 0.4f, 0.4f, 0.6f);
        public Color availableColor = new(1f, 0.8f, 0.1f, 0.8f);
        public Color unlockedColor = new(0.2f, 0.8f, 0.2f, 0.8f);

        private readonly List<Image> activeLines = new();
        private SkillTreeManager manager;
        private static Sprite defaultSprite;
        private readonly Dictionary<SkillNodeDef, RectTransform> nodeUIMap = new();
        private readonly HashSet<string> drawnConnections = new();
        private int usedLines;

        void Awake() => manager = FindAnyObjectByType<SkillTreeManager>();

        public void Redraw(IReadOnlyList<SkillNodeDef> nodes)
        {
            usedLines = 0;

            if (manager == null) manager = FindAnyObjectByType<SkillTreeManager>();

            RectTransform parent = nodes == null ? null : GetLineParent();
            if (parent == null)
            {
                HideUnusedLines();
                return;
            }

            nodeUIMap.Clear();
            foreach (var nodeUI in FindObjectsByType<SkillNodeUI>(FindObjectsInactive.Include))
            {
                if (nodeUI != null && nodeUI.node != null && nodeUI.transform is RectTransform rt)
                    nodeUIMap[nodeUI.node] = rt;
            }

            drawnConnections.Clear();

            foreach (var node in nodes)
            {
                if (node == null || node.prerequisites == null) continue;

                foreach (var prereq in node.prerequisites)
                {
                    if (prereq == null) continue;
                    if (!drawnConnections.Add(GetConnectionKey(prereq.nodeID, node.nodeID))) continue;

                    if (nodeUIMap.TryGetValue(prereq, out var fromRect) &&
                        nodeUIMap.TryGetValue(node, out var toRect))
                    {
                        DrawLine(parent, fromRect, toRect, GetLineColor(node, prereq));
                    }
                }
            }

            HideUnusedLines();
        }

        private void HideUnusedLines()
        {
            for (int i = usedLines; i < activeLines.Count; i++)
                if (activeLines[i] != null) activeLines[i].gameObject.SetActive(false);
        }

        private static string GetConnectionKey(string nodeA, string nodeB)
            => string.CompareOrdinal(nodeA, nodeB) < 0 ? $"{nodeA}|{nodeB}" : $"{nodeB}|{nodeA}";

        private RectTransform GetLineParent()
        {
            if (lineParent != null) return lineParent;

            var treeUI = GetComponentInParent<SkillTreeUI>();
            if (treeUI != null && treeUI.nodeContainer != null)
                return treeUI.nodeContainer;

            return transform as RectTransform;
        }

        private Color GetLineColor(SkillNodeDef node, SkillNodeDef prereq)
        {
            if (manager == null) return lockedColor;

            bool nodeUnlocked = manager.IsNodeUnlocked(node);
            bool prereqUnlocked = manager.IsNodeUnlocked(prereq);

            if (nodeUnlocked && prereqUnlocked) return unlockedColor;

            if (nodeUnlocked || prereqUnlocked)
            {
                SkillNodeDef target = nodeUnlocked ? prereq : node;
                return manager.CanUnlock(target).canUnlock ? availableColor : lockedColor;
            }

            return lockedColor;
        }

        private void DrawLine(RectTransform parent, RectTransform from, RectTransform to, Color color)
        {
            Vector2 fromLocal = parent.InverseTransformPoint(from.position);
            Vector2 toLocal = parent.InverseTransformPoint(to.position);

            Vector2 dir = toLocal - fromLocal;
            float dist = dir.magnitude;
            if (dist < 0.01f) return;

            Image image = AcquireLine(parent);
            var rt = (RectTransform)image.transform;

            image.sprite = lineSprite != null ? lineSprite : GetDefaultSprite();
            image.color = color;
            image.raycastTarget = false;

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(dist, lineWidth);
            rt.localPosition = new Vector3((fromLocal.x + toLocal.x) * 0.5f, (fromLocal.y + toLocal.y) * 0.5f, 0f);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        private Image AcquireLine(RectTransform parent)
        {
            while (usedLines < activeLines.Count && activeLines[usedLines] == null)
                activeLines.RemoveAt(usedLines);

            if (usedLines < activeLines.Count)
            {
                Image existing = activeLines[usedLines++];
                if (existing.transform.parent != parent) existing.transform.SetParent(parent, false);
                existing.gameObject.SetActive(true);
                existing.rectTransform.SetAsFirstSibling();
                return existing;
            }

            var go = new GameObject("SkillTreeLine", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.SetAsFirstSibling();

            Image image = go.GetComponent<Image>();
            activeLines.Add(image);
            usedLines++;
            return image;
        }

        private void OnDestroy()
        {
            activeLines.Clear();
            nodeUIMap.Clear();
            drawnConnections.Clear();
            defaultSprite = null;
        }

        private static Sprite GetDefaultSprite()
        {
            if (defaultSprite == null)
            {
                var tex = Texture2D.whiteTexture;
                defaultSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            return defaultSprite;
        }
    }
}
