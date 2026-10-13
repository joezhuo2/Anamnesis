using System.Collections.Generic;
using CrystalFlux.Core;
using NUnit.Framework;
using UnityEngine;

namespace CrystalFlux.SaveSystem.Tests
{
    public class StatsMathTests
    {
        [Test]
        public void CleanStatsJson_SubtractsEffectStats_SkipsFlags()
        {
            var live = ScriptableObject.CreateInstance<EntityStats>();
            live.critDamage = 150f;
            live.gold = 77;

            string json = StatsMath.CleanStatsJson(live, new List<StatBuff>
            {
                new(StatType.critDamage, 20f),
                new(StatType.CanMove, -1f)
            });

            var clean = ScriptableObject.CreateInstance<EntityStats>();
            JsonUtility.FromJsonOverwrite(json, clean);

            Assert.AreEqual(130f, clean.critDamage, 0.001f);
            Assert.AreEqual(77, clean.gold);
            Assert.AreEqual(live.canMove, clean.canMove);
            Assert.AreEqual(150f, live.critDamage, 0.001f);

            Object.DestroyImmediate(live);
            Object.DestroyImmediate(clean);
        }

        [Test]
        public void CleanStatsJson_NullLive_ReturnsEmpty()
            => Assert.AreEqual("", StatsMath.CleanStatsJson(null, null));
    }
}
