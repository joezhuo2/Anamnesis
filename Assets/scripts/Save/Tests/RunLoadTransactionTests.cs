using System.IO;
using NUnit.Framework;

namespace CrystalFlux.SaveSystem.Tests
{
    public class RunLoadTransactionTests
    {
        private string root;
        private string prev;

        [SetUp]
        public void SetUp()
        {
            prev = SaveStore.Root;
            root = Path.Combine(Path.GetTempPath(), "anam-tx-" + System.Guid.NewGuid().ToString("N"));
            SaveStore.Root = root;
        }

        [TearDown]
        public void TearDown()
        {
            SaveStore.Root = prev;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        private static SaveFile Make(SaveKind kind)
        {
            var f = new SaveFile();
            f.meta.kind = kind;
            f.meta.id = "run1";
            f.meta.name = "R";
            return f;
        }

        [Test]
        public void RunLoadTransaction_DeletesOnBegin()
        {
            SaveStore.Write(Make(SaveKind.Run));
            var tx = RunLoadTransaction.Begin(SaveStore.Read(SaveKind.Run, "run1"));
            Assert.IsNull(SaveStore.Read(SaveKind.Run, "run1"));
            tx.Commit();
            tx.Rollback();
            Assert.IsNull(SaveStore.Read(SaveKind.Run, "run1"));
        }

        [Test]
        public void RunLoadTransaction_RestoresFile_OnFailure()
        {
            SaveStore.Write(Make(SaveKind.Run));
            var tx = RunLoadTransaction.Begin(SaveStore.Read(SaveKind.Run, "run1"));
            tx.Rollback();
            Assert.AreEqual("R", SaveStore.Read(SaveKind.Run, "run1").meta.name);
        }

        [Test]
        public void RunLoadTransaction_BuildIsNeverDeleted()
        {
            var f = Make(SaveKind.Build);
            SaveStore.Write(f);
            RunLoadTransaction.Begin(f).Commit();
            Assert.IsNotNull(SaveStore.Read(SaveKind.Build, "run1"));
        }
    }
}
