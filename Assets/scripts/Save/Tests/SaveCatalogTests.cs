using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CrystalFlux.SaveSystem.Tests
{
    public class SaveCatalogTests
    {
        private SaveAssetCatalog catalog;
        private ScriptableObject a;

        [SetUp]
        public void SetUp()
        {
            a = ScriptableObject.CreateInstance<SaveAssetCatalog>();
            a.name = "Thing";
            catalog = ScriptableObject.CreateInstance<SaveAssetCatalog>();
            catalog.SetEntries(new List<SaveAssetCatalog.Entry> { new() { id = "id-a", name = "Thing", asset = a } });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Catalog_GetId_ByReference() => Assert.AreEqual("id-a", catalog.GetId(a));

        [Test]
        public void Catalog_GetId_ByCloneName()
        {
            var clone = Object.Instantiate(a);
            Assert.AreEqual("id-a", catalog.GetId(clone));
            Object.DestroyImmediate(clone);
        }

        [Test]
        public void Catalog_GetId_UnknownIsEmpty()
        {
            var other = ScriptableObject.CreateInstance<SaveAssetCatalog>();
            other.name = "Other";
            Assert.AreEqual("", catalog.GetId(other));
            Object.DestroyImmediate(other);
        }

        [Test]
        public void Catalog_Get_ReturnsAsset() => Assert.AreSame(a, catalog.Get<SaveAssetCatalog>("id-a"));

        [Test]
        public void Compatibility_Ok_WhenAllIdsPresent()
        {
            var meta = new SaveMeta { referencedIds = new List<string> { "id-a" } };
            Assert.IsNull(SaveAssetCatalog.CompatibilityIssue(meta, catalog));
        }

        [Test]
        public void Compatibility_Reports_MissingId()
        {
            var meta = new SaveMeta { referencedIds = new List<string> { "id-a", "gone" } };
            StringAssert.Contains("missing", SaveAssetCatalog.CompatibilityIssue(meta, catalog));
        }

        [Test]
        public void Compatibility_Rejects_NewerFormat()
        {
            var meta = new SaveMeta { formatVersion = SaveMeta.CurrentFormat + 1 };
            Assert.IsNotNull(SaveAssetCatalog.CompatibilityIssue(meta, catalog));
        }

        [Test]
        public void Compatibility_Rejects_NullCatalog()
            => Assert.IsNotNull(SaveAssetCatalog.CompatibilityIssue(new SaveMeta(), null));
    }
}
