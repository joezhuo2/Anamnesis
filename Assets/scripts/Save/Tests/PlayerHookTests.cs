using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using NUnit.Framework;
using UnityEngine;

namespace CrystalFlux.SaveSystem.Tests
{
    public class PlayerHookTests
    {
        [Test]
        public void SynergyState_RoundTrips_WithApplied()
        {
            var go = new GameObject("syn");
            var ssm = go.AddComponent<StatSynergyManager>();

            ssm.RestoreState(new List<(StatType, StatType, float, float)>
            {
                (StatType.EffMaxHp, StatType.attack, 12f, 34f)
            });

            var state = new List<(StatType source, StatType target, float pct, float applied)>();
            ssm.GetState(state);

            Assert.AreEqual(1, state.Count);
            Assert.AreEqual(StatType.EffMaxHp, state[0].source);
            Assert.AreEqual(StatType.attack, state[0].target);
            Assert.AreEqual(12f, state[0].pct);
            Assert.AreEqual(34f, state[0].applied);
            Assert.AreEqual(1, ssm.Count);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DpsMeter_SetPeak_ClampsAndSets()
        {
            DpsMeter.SetPeak(4321f);
            Assert.AreEqual(4321f, DpsMeter.Peak);
            DpsMeter.SetPeak(-5f);
            Assert.AreEqual(0f, DpsMeter.Peak);
            DpsMeter.Reset();
        }
    }
}
