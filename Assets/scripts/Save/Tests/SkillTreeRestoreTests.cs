using System.Collections.Generic;
using CrystalFlux.SkillTree;
using NUnit.Framework;
using UnityEngine;

namespace CrystalFlux.SaveSystem.Tests
{
    public class SkillTreeRestoreTests
    {
        private GameObject go;
        private PlayerSkillTree tree;
        private SkillNodeDef a;
        private SkillNodeDef b;

        private static SkillNodeDef Node(string id, bool start)
        {
            var n = ScriptableObject.CreateInstance<SkillNodeDef>();
            n.nodeID = id;
            n.nodeName = id;
            n.isStartingNode = start;
            n.prerequisites = new List<SkillNodeDef>();
            n.incompatibleNodes = new List<SkillNodeDef>();
            return n;
        }

        [SetUp]
        public void SetUp()
        {
            a = Node("a", true);
            b = Node("b", false);
            b.prerequisites.Add(a);

            go = new GameObject("tree");
            tree = go.AddComponent<PlayerSkillTree>();
            tree.SetNodes(new List<SkillNodeDef> { a, b });
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var n in tree.runtimeNodes) Object.DestroyImmediate(n);
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void RestoreUnlocked_UnlocksInOrder_SetsPointsAndStarting()
        {
            tree.RestoreUnlocked(new List<string> { "a", "b" }, 3, true);

            Assert.IsTrue(tree.IsNodeUnlocked(a));
            Assert.IsTrue(tree.IsNodeUnlocked(b));
            CollectionAssert.AreEqual(new[] { "a", "b" }, tree.UnlockOrder);
            Assert.AreEqual(3, tree.SkillPoints);
            Assert.IsTrue(tree.choseStarting);
        }

        [Test]
        public void RestoreUnlocked_SkipsUnknownIds()
        {
            tree.RestoreUnlocked(new List<string> { "a", "ghost" }, 0, true);

            CollectionAssert.AreEqual(new[] { "a" }, tree.UnlockOrder);
            Assert.AreEqual(1, tree.UnlockedNodeCount);
        }

        [Test]
        public void RestoreUnlocked_ClearsNodesNotInSave()
        {
            tree.AddSkillPoints(5);
            tree.UnlockNode(tree.runtimeNodes[0]);

            tree.RestoreUnlocked(new List<string> { "b" }, 2, false);

            Assert.IsFalse(tree.IsNodeUnlocked(a));
            Assert.IsTrue(tree.IsNodeUnlocked(b));
            CollectionAssert.AreEqual(new[] { "b" }, tree.UnlockOrder);
            Assert.IsFalse(tree.choseStarting);
        }

        [Test]
        public void UnlockOrder_TracksUnlockAndRefund()
        {
            tree.AddSkillPoints(5);
            tree.UnlockNode(tree.runtimeNodes[0]);
            tree.UnlockNode(tree.runtimeNodes[1]);
            CollectionAssert.AreEqual(new[] { "a", "b" }, tree.UnlockOrder);
        }
    }
}
