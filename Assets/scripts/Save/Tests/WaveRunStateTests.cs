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
        public void RunState_Mutations_RoundTripThroughJson()
        {
            var m = new MutationState { type = MutationType.Overhealth };
            m.LevelUp();
            m.LevelUp();
            var dto = new RunStateDto { lastMutationWave = 25, mutationBonusChance = 10f };
            dto.mutations.Add(m);
            dto.mutations.Add(new MutationState { type = MutationType.Thorns });

            wm.RestoreRunState(dto);
            var back = new RunStateDto();
            wm.CaptureRunState(back);

            var json = JsonUtility.ToJson(new SaveSystem.RunData { mutations = back.mutations });
            var loaded = JsonUtility.FromJson<SaveSystem.RunData>(json).mutations;

            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual(MutationType.Overhealth, loaded[0].type);
            Assert.AreEqual(2, loaded[0].level);
            Assert.AreEqual(m.value, loaded[0].value);
            Assert.AreEqual(m.value2, loaded[0].value2);
            Assert.AreEqual(25, back.lastMutationWave);
            Assert.AreEqual(10f, back.mutationBonusChance);
        }

        [Test]
        public void Mutation_LevelUp_CapsAtMaxAndRollsInRange()
        {
            var m = new MutationState { type = MutationType.IFrames };
            for (int i = 0; i < 5; i++) m.LevelUp();

            Assert.IsTrue(m.IsMaxed);
            Assert.AreEqual(MutationState.MaxLevel, m.level);
            Assert.That(m.value, Is.InRange(0.3f, 0.7f));
            Assert.AreEqual("Evasive III", m.Label);
        }

        [Test]
        public void IsIdleBetweenWaves_FalseBeforeRunStarts()
        {
            Assert.IsFalse(wm.IsRunStarted);
            Assert.IsFalse(wm.IsIdleBetweenWaves);
        }
    }
}
