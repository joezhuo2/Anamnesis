using System.Collections.Generic;
using NUnit.Framework;

namespace CrystalFlux.SaveSystem.Tests
{
    public class SaveQueryTests
    {
        private static SaveFile F(string name, int wave, float dps, string date, params string[] attacks)
        {
            var f = new SaveFile();
            f.meta.name = name;
            f.meta.wave = wave;
            f.meta.peakDps = dps;
            f.meta.savedAtUtc = date;
            f.meta.modeName = "Expert";
            f.meta.difficultyName = "Hard";
            f.meta.attackNames = new List<string>(attacks);
            return f;
        }

        private List<SaveFile> list;

        [SetUp]
        public void SetUp() => list = new List<SaveFile>
        {
            F("Bravo", 30, 500, "2026-10-02T00:00:00Z", "Lacerate", "Aphelion"),
            F("alpha", 10, 900, "2026-10-03T00:00:00Z", "Astral Nova"),
            F("Charlie", 50, 100, "2026-10-01T00:00:00Z", "Lacerate"),
        };

        [Test]
        public void SaveQuery_Search_MultiTermCaseInsensitive()
        {
            var r = SaveQuery.Apply(list, "  LACER   bra ", SaveSort.Name);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual("Bravo", r[0].meta.name);
        }

        [Test]
        public void SaveQuery_Search_MatchesModeAndDifficulty()
            => Assert.AreEqual(3, SaveQuery.Apply(list, "expert hard", SaveSort.Name).Count);

        [Test]
        public void SaveQuery_Search_NoMatch()
            => Assert.AreEqual(0, SaveQuery.Apply(list, "zzz", SaveSort.Name).Count);

        [Test]
        public void SaveQuery_EmptySearch_ReturnsAll()
            => Assert.AreEqual(3, SaveQuery.Apply(list, "", SaveSort.Date).Count);

        [Test]
        public void SaveQuery_Sort_DateDescending()
            => Assert.AreEqual("alpha", SaveQuery.Apply(list, null, SaveSort.Date)[0].meta.name);

        [Test]
        public void SaveQuery_Sort_WaveDescending()
            => Assert.AreEqual("Charlie", SaveQuery.Apply(list, null, SaveSort.Wave)[0].meta.name);

        [Test]
        public void SaveQuery_Sort_DpsDescending()
            => Assert.AreEqual("alpha", SaveQuery.Apply(list, null, SaveSort.Dps)[0].meta.name);

        [Test]
        public void SaveQuery_Sort_NameAscendingIgnoreCase()
            => Assert.AreEqual("alpha", SaveQuery.Apply(list, null, SaveSort.Name)[0].meta.name);
    }
}
