using System.Collections.Generic;
using System.Reflection;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using NUnit.Framework;
using UnityEngine;

namespace CrystalFlux.SaveSystem.Tests
{
    public class FakeStats : MonoBehaviour, IStatProvider
    {
        public float gold;
        public readonly List<StatBuff> added = new();

        public float GetStat(StatType type) => type == StatType.Gold ? gold : 0f;

        public void AddStat(StatBuff buff, bool isAdding = true)
            => added.Add(new StatBuff(buff.type, isAdding ? buff.value : -buff.value));
    }

    public class ReviewFixTests
    {
        [Test]
        public void GoldBuff_ReportsAppliedStacksAsStats()
        {
            var go = new GameObject("p");
            var fake = go.AddComponent<FakeStats>();
            var pum = go.AddComponent<PlayerUpgradeManager>();
            fake.gold = 500f;

            var gb = ScriptableObject.CreateInstance<GoldBuff>();
            gb.goldPerStack = 100;
            gb.maxStacks = 5;
            gb.buffs = new[] { new StatBuff(StatType.atkPct, 2f) };
            gb.OnUnlock(go);

            var into = new List<StatBuff>();
            gb.CollectAppliedStats(into);

            Assert.AreEqual(1, into.Count);
            Assert.AreEqual(StatType.atkPct, into[0].type);
            Assert.AreEqual(10f, into[0].value, 0.001f);

            gb.OnRemove(go);
            Object.DestroyImmediate(gb);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void RestoreStatsJson_ReleasesAllGates()
        {
            var go = new GameObject("e");
            var esm = go.AddComponent<EntityStatManager>();
            esm.baseStats = ScriptableObject.CreateInstance<EntityStats>();
            typeof(EntityStatManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(esm, null);

            var snap = ScriptableObject.CreateInstance<EntityStats>();
            snap.canDash = false;
            snap.canGainStamina = false;
            snap.canGainMana = false;
            snap.isDashing = true;
            string json = JsonUtility.ToJson(snap);

            esm.RestoreStatsJson(json);

            Assert.IsTrue(esm.Stats.canDash);
            Assert.IsTrue(esm.Stats.canGainStamina);
            Assert.IsTrue(esm.Stats.canGainMana);
            Assert.IsFalse(esm.Stats.isDashing);

            Object.DestroyImmediate(esm.Stats);
            Object.DestroyImmediate(snap);
            Object.DestroyImmediate(esm.baseStats);
            Object.DestroyImmediate(go);
        }
    }
}
