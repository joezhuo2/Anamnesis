using System.Collections.Generic;
using NUnit.Framework;

namespace CrystalFlux.SaveSystem.Tests
{
    public class SaveInfoFormatterTests
    {
        [Test]
        public void FormatPlaytime_MinutesAndHours()
        {
            Assert.AreEqual("12m 05s", SaveInfoFormatter.FormatPlaytime(725f));
            Assert.AreEqual("1h 02m", SaveInfoFormatter.FormatPlaytime(3725f));
            Assert.AreEqual("0m 00s", SaveInfoFormatter.FormatPlaytime(-3f));
        }

        [Test]
        public void Lines_ContainsRequiredInfo()
        {
            var m = new SaveMeta
            {
                wave = 23,
                modeName = "Master",
                difficultyName = "Hard",
                ironman = true,
                peakDps = 12345,
                savedAtUtc = "2026-10-13T10:00:00.0000000Z",
                playtimeSeconds = 725f,
                attackNames = new List<string> { "Lacerate", "Aeternus" }
            };

            string s = SaveInfoFormatter.Lines(m, null);
            StringAssert.Contains("Wave 23", s);
            StringAssert.Contains("Master", s);
            StringAssert.Contains("Hard", s);
            StringAssert.Contains("Ironman", s);
            StringAssert.Contains("12,345", s);
            StringAssert.Contains("Lacerate", s);
            StringAssert.Contains("Aeternus", s);
            StringAssert.Contains("12m 05s", s);
            StringAssert.Contains("2026", s);
            StringAssert.DoesNotContain("Incompatible", s);
        }

        [Test]
        public void Lines_ShowsIncompatibleAndReadOnly()
        {
            var m = new SaveMeta { readOnly = true };
            string s = SaveInfoFormatter.Lines(m, "2 referenced assets missing in this version");
            StringAssert.Contains("missing", s);
            StringAssert.Contains("Read only", s);
        }
    }
}
