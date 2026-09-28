using UnityEngine;

namespace CrystalFlux.SkillTree
{
    public class SkillTreeManager : MonoBehaviour
    {
        [HideInInspector] public GameObject player = null;
        [HideInInspector] public PlayerSkillTree tree;

        public void Start()
        {
            if (player == null)
                player = GameObject.FindWithTag("Player");

            if (player == null) return;

            if (player.TryGetComponent<PlayerSkillTree>(out var tree)) this.tree = tree;
        }

        public void SetPlayer(GameObject player)
        {
            this.player = player;
            this.tree = player != null && player.TryGetComponent<PlayerSkillTree>(out var tree) ? tree : null;
        }

        public (bool canUnlock, string failMessage) CanUnlock(SkillNodeDef node)
            => tree != null ? tree.CanUnlock(node) : (false, "No player skill tree");

        public void UnlockNode(SkillNodeDef node)
        {
            if (tree != null) tree.UnlockNode(node);
        }

        public bool IsNodeUnlocked(SkillNodeDef node) => tree != null && tree.IsNodeUnlocked(node);

        public (bool canRefund, string failMessage) CanRefundAll()
            => tree != null ? tree.CanRefundAll() : (false, "No player skill tree");

        public int GetRefundAllCost() => tree != null ? tree.GetRefundAllCost() : 0;
        public int GetRefundAllPoints() => tree != null ? tree.GetRefundAllPoints() : 0;
        public int UnlockedNodeCount => tree != null ? tree.UnlockedNodeCount : 0;
        public bool RefundAll() => tree != null && tree.RefundAll();

    }
}
