using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;

namespace CrystalFlux.SkillTree
{
    public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI References")]
        public Image backgroundImage;
        public Image iconImage;
        public GameObject lockedOverlay;
        public GameObject unlockedCheckmark;
        public GameObject availableGlow;
        public Color queuedColor = new(0.3f, 0.45f, 0.95f, 0.8f);

        [Header("Search Highlight")]
        [Tooltip("Optional. Recolored by the search filter. Null = an Outline on the background image is used instead")]
        public Image borderImage;
        public Color matchBorderColor = new(0.35f, 0.9f, 1f, 1f);
        public Color dimBorderColor = new(0.5f, 0.5f, 0.5f, 1f);
        public Color darkBorderColor = new(0.2f, 0.2f, 0.2f, 1f);
        public Vector2 outlineDistance = new(4f, -4f);

        [HideInInspector] public SkillNodeDef node;
        private SkillTreeManager manager;
        private PlayerSkillTree playerSkillTree;
        private Outline outline;
        private bool hasBaseBorder;
        private bool baseOutlineEnabled;
        private Color baseBorderColor;
        private bool reachable;
        private int searchState;

        public void Initialize(SkillNodeDef node, SkillTreeManager manager)
        {
            this.node = node;
            this.manager = manager ?? FindAnyObjectByType<SkillTreeManager>();
            this.playerSkillTree = manager?.player?.GetComponent<PlayerSkillTree>() ?? FindAnyObjectByType<PlayerSkillTree>();

            if (backgroundImage != null) backgroundImage.raycastTarget = true;

            RefreshVisuals();
        }

        public void RefreshVisuals()
        {
            if (node == null || manager == null) return;
            if (playerSkillTree == null)
                playerSkillTree = manager?.player?.GetComponent<PlayerSkillTree>()  ?? FindAnyObjectByType<PlayerSkillTree>();

            if (backgroundImage != null) backgroundImage.raycastTarget = true;

            bool unlocked = manager.IsNodeUnlocked(node);
            var (canUnlock, _) = manager.CanUnlock(node);
            bool queued = !unlocked && manager.tree != null && manager.tree.IsNodeQueued(node);

            if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked && !canUnlock && !queued);
            if (unlockedCheckmark != null) unlockedCheckmark.SetActive(unlocked);
            if (availableGlow != null) availableGlow.SetActive(!unlocked && canUnlock);
            if (iconImage != null && node.icon != null) iconImage.sprite = node.icon;

            if (backgroundImage != null)
            {
                if (unlocked) backgroundImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f);
                else if (queued) backgroundImage.color = queuedColor;
                else if (canUnlock) backgroundImage.color = new Color(0.8f, 0.7f, 0.1f, 0.8f);
                else backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            }

            reachable = unlocked || canUnlock || queued;
            ApplyBorder();
        }

        public void SetSearchState(int state)
        {
            if (searchState == state) return;

            searchState = state;
            ApplyBorder();
        }

        private Color SearchBorderColor()
        {
            if (searchState > 0) return matchBorderColor;
            return reachable ? dimBorderColor : darkBorderColor;
        }

        private void ApplyBorder()
        {
            if (borderImage != null)
            {
                if (!hasBaseBorder)
                {
                    baseBorderColor = borderImage.color;
                    hasBaseBorder = true;
                }
                borderImage.color = searchState == 0 ? baseBorderColor : SearchBorderColor();
                return;
            }

            if (backgroundImage == null) return;

            if (outline == null)
            {
                if (backgroundImage.TryGetComponent(out outline))
                {
                    baseBorderColor = outline.effectColor;
                    baseOutlineEnabled = outline.enabled;
                }
                else if (searchState != 0)
                {
                    outline = backgroundImage.gameObject.AddComponent<Outline>();
                    outline.effectDistance = outlineDistance;
                    baseBorderColor = outline.effectColor;
                    baseOutlineEnabled = false;
                }
                else return;
            }

            outline.enabled = searchState != 0 || baseOutlineEnabled;
            outline.effectColor = searchState == 0 ? baseBorderColor : SearchBorderColor();
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (manager == null || node == null) return;

            if (TryGetComponent<ITooltipDisplay>(out var td))
            {
                var (tt, st, os) = GetSkillTreeTooltip();
                td.ShowTooltip(tt, st, os);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TryGetComponent<ITooltipDisplay>(out var td))
                td.HideTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (manager == null || node == null || manager.tree == null) return;
            if (SkillTreePanZoom.DraggedThisPress) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (manager.IsNodeUnlocked(node)) UndoNode();
                else if (manager.tree.IsNodeQueued(node)) DequeueNode();
                else UnlockNode();
            }
        }

        private void UnlockNode()
        {
            var (canUnlock, _) = manager.CanUnlock(node);
            if (canUnlock) manager.UnlockNode(node);
            else if (!manager.tree.QueueNode(node)) return;

            NotifyChanged();
        }

        private void DequeueNode()
        {
            manager.tree.DequeueNode(node);
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            var treeUI = GetComponentInParent<SkillTreeUI>();
            if (treeUI != null) treeUI.OnNodeStateChanged(node);
        }

        private void UndoNode()
        {
            if (manager.tree != null)
            {
                var (canUndo, _) = manager.tree.CanUndo(node);
                if (canUndo)
                {
                    manager.tree.UndoNode(node);
                    NotifyChanged();
                }
            }
        }

        private (string title, string subtitle, Vector2 offset) GetSkillTreeTooltip()
        {
            if (node == null) return ("", "", Vector2.zero);

            List<string> lines = new();
            if (!string.IsNullOrEmpty(node.desc)) lines.Add(node.desc);

            var tree = manager.tree;
            int qi = tree != null ? tree.QueueIndex(node) : -1;
            var (canUnlock, failMessage) = manager.CanUnlock(node);

            if (qi >= 0)
            {
                lines.Add($"<color=#6F8CFF>Queued (#{qi + 1}). Unlocks automatically when affordable</color>");
                if (!string.IsNullOrEmpty(failMessage)) lines.Add($"<color=#888888>{failMessage}</color>");
                lines.Add("<color=#6F8CFF>Left-click to remove from queue</color>");
            }
            else
            {
                if (!string.IsNullOrEmpty(failMessage))
                    lines.Add($"<color=#FF4444>{failMessage}</color>");

                if (!canUnlock && tree != null && !tree.IsNodeUnlocked(node) && tree.CanQueue(node).canQueue)
                    lines.Add("<color=#6F8CFF>Left-click to queue</color>");
            }

            if (!GameSettings.Current.ironmanMode)
            {
                var playerSkillTree = FindAnyObjectByType<PlayerSkillTree>();
                if (playerSkillTree != null && playerSkillTree.IsNodeUnlocked(node))
                {
                    var (canUndo, undoFail) = playerSkillTree.CanUndo(node);
                    int cost = playerSkillTree.GetUndoCost(node);
                    string costText = playerSkillTree.InGrace(node) ? "free until the tree is closed" : $"{cost}g";
                    if (canUndo) lines.Add($"<color=#FFD700>Left-click to undo ({costText})</color>");
                    else lines.Add($"<color=#888888>Undo cost: {costText} ({undoFail})</color>");
                }
            }

            return(node.nodeName, string.Join("\n", lines), new(100, -100));
        }
    }
}
