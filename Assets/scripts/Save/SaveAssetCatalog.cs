using System;
using System.Collections.Generic;
using CrystalFlux.EntitySystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrystalFlux.SaveSystem
{
    public class SaveAssetCatalog : ScriptableObject
    {
        public const string ResourcePath = "SaveAssetCatalog";

        [Serializable]
        public class Entry
        {
            public string id;
            public string name;
            public Object asset;
        }

        public List<Entry> entries = new();

        private Dictionary<string, Entry> byId;
        private Dictionary<Object, string> byRef;

        private static SaveAssetCatalog instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        public static SaveAssetCatalog Instance
        {
            get
            {
                if (instance == null) instance = Resources.Load<SaveAssetCatalog>(ResourcePath);
                return instance;
            }
        }

        public void SetEntries(List<Entry> list)
        {
            entries = list ?? new List<Entry>();
            byId = null;
            byRef = null;
        }

        private void EnsureLookup()
        {
            if (byId != null) return;

            byId = new Dictionary<string, Entry>();
            byRef = new Dictionary<Object, string>();

            foreach (Entry e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.id) || e.asset == null) continue;

                byId[e.id] = e;
                byRef[e.asset] = e.id;
            }
        }

        public bool Has(string id)
        {
            EnsureLookup();
            return !string.IsNullOrEmpty(id) && byId.ContainsKey(id);
        }

        public T Get<T>(string id) where T : Object
        {
            EnsureLookup();
            return !string.IsNullOrEmpty(id) && byId.TryGetValue(id, out Entry e) ? e.asset as T : null;
        }

        public string GetId(Object asset)
        {
            if (asset == null) return "";

            EnsureLookup();
            if (byRef.TryGetValue(asset, out string id)) return id;

            string wanted = PlayerAttackHandler.NormalizeAttackName(asset.name);
            Type t = asset.GetType();

            foreach (Entry e in entries)
            {
                if (e == null || e.asset == null || e.asset.GetType() != t) continue;
                if (PlayerAttackHandler.NormalizeAttackName(e.name).Equals(wanted, StringComparison.OrdinalIgnoreCase)) return e.id;
            }

            return "";
        }

        public static string CompatibilityIssue(SaveMeta meta, SaveAssetCatalog catalog)
        {
            if (meta == null) return "Unreadable save";
            if (meta.formatVersion > SaveMeta.CurrentFormat) return "Saved by a newer game version";
            if (catalog == null) return "Save catalog missing";

            int missing = 0;
            if (meta.referencedIds != null)
                foreach (string id in meta.referencedIds)
                    if (!catalog.Has(id)) missing++;

            return missing == 0 ? null : $"{missing} referenced asset{(missing > 1 ? "s" : "")} missing in this version";
        }
    }
}
