using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image backgroundImage;
    public Image iconImage;
    public GameObject lockedOverlay;
    public GameObject unlockedCheckmark;
    public GameObject availableGlow;
    public TooltipTrigger tooltipTrigger;

    [HideInInspector] public SkillNodeDef node;
    private SkillTreeManager manager;
    private PlayerSkillTree playerSkillTree;

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

        if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked && !canUnlock);
        if (unlockedCheckmark != null) unlockedCheckmark.SetActive(unlocked);
        if (availableGlow != null) availableGlow.SetActive(!unlocked && canUnlock);
        if (iconImage != null && node.icon != null) iconImage.sprite = node.icon;

        if (backgroundImage != null)
        {
            if (unlocked) backgroundImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f);
            else if (canUnlock) backgroundImage.color = new Color(0.8f, 0.7f, 0.1f, 0.8f);
            else backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager == null || node == null) return;

        var (_, failMessage) = manager.CanUnlock(node);
        if (tooltipTrigger != null) tooltipTrigger.SetupTooltipData(node, failMessage);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipTrigger != null) tooltipTrigger.OnPointerExit(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager == null || node == null || manager.tree == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            bool unlocked = manager.IsNodeUnlocked(node);
            if (unlocked && !node.isStartingNode) UndoNode();
            else UnlockNode();
        }
    }

    private void UnlockNode()
    {
        var (canUnlock, _) = manager.CanUnlock(node);
        if (canUnlock)
        {
            manager.UnlockNode(node);

            var treeUI = GetComponentInParent<SkillTreeUI>();
            if (treeUI != null) treeUI.OnNodeStateChanged(node);
        }
    }

    private void UndoNode()
    {
        if (manager.tree != null)
        {
            var (canUndo, _) = manager.tree.CanUndo(node);
            if (canUndo)
            {
                manager.tree.UndoNode(node);

                var treeUI = GetComponentInParent<SkillTreeUI>();
                if (treeUI != null) treeUI.OnNodeStateChanged(node);
            }
        }
    }
}