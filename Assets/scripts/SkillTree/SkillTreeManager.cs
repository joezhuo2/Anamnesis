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
    }
}