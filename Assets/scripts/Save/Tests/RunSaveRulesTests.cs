using System.Collections.Generic;
using CrystalFlux.SettingsSystem;
using NUnit.Framework;

namespace CrystalFlux.SaveSystem.Tests
{
    public class RunSaveRulesTests
    {
        [Test]
        public void RunSaveRules_Blocks_WhenPicksPending()
            => Assert.IsFalse(RunSaveRules.CanSaveProgress(true, false, false, true, false));

        [Test]
        public void RunSaveRules_Allows_IdleBetweenWaves()
            => Assert.IsTrue(RunSaveRules.CanSaveProgress(true, false, true, false, false));

        [Test]
        public void RunSaveRules_Allows_MidWave_WithSnapshot()
            => Assert.IsTrue(RunSaveRules.CanSaveProgress(true, true, false, true, false));

        [Test]
        public void RunSaveRules_Blocks_MidWave_WithoutSnapshot()
            => Assert.IsFalse(RunSaveRules.CanSaveProgress(true, true, false, false, false));

        [Test]
        public void RunSaveRules_Blocks_WhenDead()
            => Assert.IsFalse(RunSaveRules.CanSaveProgress(true, true, false, true, true));

        [Test]
        public void RunSaveRules_Blocks_BeforeRunStarts()
            => Assert.IsFalse(RunSaveRules.CanSaveProgress(false, false, false, false, false));

        [Test]
        public void RunSaveRules_Build_AllowedOnDeath()
            => Assert.IsTrue(RunSaveRules.CanSaveBuild(true, false, true));

        [Test]
        public void RunSaveRules_Build_BlockedMidWave()
            => Assert.IsFalse(RunSaveRules.CanSaveBuild(true, false, false));

        [Test]
        public void RunSaveRules_Build_AllowedBetweenWaves()
            => Assert.IsTrue(RunSaveRules.CanSaveBuild(true, true, false));

        [Test]
        public void RunSaveRules_DefaultNames()
        {
            Assert.AreEqual("Expert Hard - Wave 12", RunSaveRules.DefaultRunName("Expert", "Hard", 12));
            Assert.AreEqual("Lacerate / Cyclone Cleave - Wave 3", RunSaveRules.DefaultBuildName(new List<string> { "Lacerate", "Cyclone Cleave", "Coherent Strike" }, 3));
            Assert.AreEqual("Build - Wave 3", RunSaveRules.DefaultBuildName(new List<string>(), 3));
        }

        [Test]
        public void MakeMeta_CollectsReferencedIds_AndEmptyIdStaysMissing()
        {
            var b = new BuildData();
            b.attacks.Add(new AttackSlot { id = "atk" });
            b.attacks.Add(new AttackSlot { id = "" });
            b.upgradeIds.Add("up");
            var r = new RunData { sequenceId = "seq", difficultyId = "dif", modeId = "mod" };
            r.availableRareIds.Add("atk");

            SaveMeta m = BuildCapture.MakeMeta(SaveKind.Run, " Name ", b, r, null, 4, new List<string> { "X" });

            CollectionAssert.AreEquivalent(new[] { "atk", "", "up", "seq", "dif", "mod" }, m.referencedIds);
            Assert.AreEqual("Name", m.name);
            Assert.AreEqual(4, m.wave);
            Assert.IsFalse(string.IsNullOrEmpty(m.id));
        }

        [Test]
        public void MakeMeta_BuildDuringIronman_IsReadOnly()
        {
            bool prev = GameSettings.Current.ironmanMode;
            GameSettings.Current.ironmanMode = true;
            try
            {
                Assert.IsTrue(BuildCapture.MakeMeta(SaveKind.Build, "b", new BuildData(), null, null, 1, null).readOnly);
                Assert.IsFalse(BuildCapture.MakeMeta(SaveKind.Run, "r", new BuildData(), new RunData(), null, 1, null).readOnly);
            }
            finally
            {
                GameSettings.Current.ironmanMode = prev;
            }
        }
    }
}
