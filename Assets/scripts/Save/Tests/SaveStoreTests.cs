using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CrystalFlux.SaveSystem.Tests
{
    public class SaveStoreTests
    {
        private string root;
        private string previousRoot;

        [SetUp]
        public void SetUp()
        {
            previousRoot = SaveStore.Root;
            root = Path.Combine(Path.GetTempPath(), "anamnesis-save-tests-" + System.Guid.NewGuid().ToString("N"));
            SaveStore.Root = root;
        }

        [TearDown]
        public void TearDown()
        {
            SaveStore.Root = previousRoot;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        private static SaveFile Make(SaveKind kind, string name, int wave = 5)
        {
            var f = new SaveFile();
            f.meta.kind = kind;
            f.meta.id = System.Guid.NewGuid().ToString("N");
            f.meta.name = name;
            f.meta.wave = wave;
            f.build.statsJson = "{\"gold\":123}";
            f.build.upgradeIds.Add("abc");
            f.run.waveIndex = wave;
            return f;
        }

        [Test]
        public void SaveStore_WriteThenRead_RoundTrips()
        {
            SaveFile f = Make(SaveKind.Run, "Alpha", 12);
            Assert.IsTrue(SaveStore.Write(f));

            SaveFile r = SaveStore.Read(SaveKind.Run, f.meta.id);
            Assert.AreEqual("Alpha", r.meta.name);
            Assert.AreEqual(12, r.run.waveIndex);
            Assert.AreEqual("abc", r.build.upgradeIds[0]);
            Assert.AreEqual("{\"gold\":123}", r.build.statsJson);
        }

        [Test]
        public void SaveStore_List_SeparatesKinds()
        {
            SaveStore.Write(Make(SaveKind.Run, "R"));
            SaveStore.Write(Make(SaveKind.Build, "B1"));
            SaveStore.Write(Make(SaveKind.Build, "B2"));

            Assert.AreEqual(1, SaveStore.List(SaveKind.Run).Count);
            Assert.AreEqual(2, SaveStore.List(SaveKind.Build).Count);
        }

        [Test]
        public void SaveStore_List_SkipsCorruptFiles()
        {
            SaveStore.Write(Make(SaveKind.Build, "Good"));
            string dir = SaveStore.DirFor(SaveKind.Build);
            File.WriteAllText(Path.Combine(dir, "bad.json"), "{not json");
            File.WriteAllText(Path.Combine(dir, "empty.json"), "");

            LogAssert.ignoreFailingMessages = true;
            var list = SaveStore.List(SaveKind.Build);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Good", list[0].meta.name);
        }

        [Test]
        public void SaveStore_Delete_RemovesFile()
        {
            SaveFile f = Make(SaveKind.Run, "X");
            SaveStore.Write(f);
            Assert.IsTrue(SaveStore.Delete(SaveKind.Run, f.meta.id));
            Assert.IsNull(SaveStore.Read(SaveKind.Run, f.meta.id));
        }

        [Test]
        public void SaveStore_Rename_UpdatesName()
        {
            SaveFile f = Make(SaveKind.Build, "Old");
            SaveStore.Write(f);
            Assert.IsTrue(SaveStore.Rename(SaveKind.Build, f.meta.id, "  New  "));
            Assert.AreEqual("New", SaveStore.Read(SaveKind.Build, f.meta.id).meta.name);
        }

        [Test]
        public void SaveStore_FindByName_IgnoresCaseAndTrim()
        {
            SaveFile f = Make(SaveKind.Build, "My Build");
            SaveStore.Write(f);
            Assert.AreEqual(f.meta.id, SaveStore.FindByName(SaveKind.Build, "  my build ").meta.id);
            Assert.IsNull(SaveStore.FindByName(SaveKind.Run, "my build"));
        }

        [Test]
        public void SaveStore_NormalizeName_TrimsAndClamps()
        {
            Assert.AreEqual("", SaveStore.NormalizeName("   "));
            Assert.AreEqual(SaveStore.MaxNameLength, SaveStore.NormalizeName(new string('a', 100)).Length);
        }
    }
}
