using System.Collections.Generic;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.WaveSystem;
using NUnit.Framework;
using UnityEngine;

namespace CrystalFlux.SaveSystem.Tests
{
    public class WaveRunStateTests
    {
        private GameObject go;
        private WaveManager wm;
        private AttackData atkA;
        private AttackData atkB;

        [SetUp]
        public void SetUp()
        {
            atkA = ScriptableObject.CreateInstance<AttackData>();
            atkA.name = "A";
            atkB = ScriptableObject.CreateInstance<AttackData>();
            atkB.name = "B";

            go = new GameObject("wm");
            wm = go.AddComponent<WaveManager>();
            wm.rarePool = new List<AttackReward> { new() { newAttack = atkA }, new() { newAttack = atkB } };
            wm.treasurePool = new List<PlayerUpgradeReward>();
            wm.corruptionSpecialPool = new List<AttackReward> { new() { newAttack = atkA } };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(atkA);
            Object.DestroyImmediate(atkB);
        }

        [Test]
        public void RunState_RestoreThenCapture_RoundTrips()
        {
            var dto = new RunStateDto
            {
                waveIndex = 3,
                rerolls = 7,
                additionalQuality = 1.5f,
                synergyChanceBonus = 8f,
                pendingOccasionalRerolls = 2,
                pendingAnomalySkillPoints = 1
            };
            dto.availableRare.Add(atkB);

            wm.RestoreRunState(dto);

            var back = new RunStateDto();
            wm.CaptureRunState(back);

            Assert.AreEqual(3, back.waveIndex);
            Assert.AreEqual(7, back.rerolls);
            Assert.AreEqual(1.5f, back.additionalQuality);
            Assert.AreEqual(8f, back.synergyChanceBonus);
            Assert.AreEqual(2, back.pendingOccasionalRerolls);
            Assert.AreEqual(1, back.pendingAnomalySkillPoints);
            CollectionAssert.AreEqual(new[] { atkB }, back.availableRare);
            Assert.AreEqual(0, back.availableCorruptionSpecial.Count);
        }

        [Test]
        public void IsIdleBetweenWaves_FalseBeforeRunStarts()
        {
            Assert.IsFalse(wm.IsRunStarted);
            Assert.IsFalse(wm.IsIdleBetweenWaves);
        }
    }
}
