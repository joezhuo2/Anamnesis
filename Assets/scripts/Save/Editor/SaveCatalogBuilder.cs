using System.Collections.Generic;
using System.IO;
using CrystalFlux.Core;
using CrystalFlux.SkillTree;
using CrystalFlux.WaveSystem;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CrystalFlux.SaveSystem.Editor
{
    public class SaveCatalogBuilder : IPreprocessBuildWithReport
    {
        private const string AssetPath = "Assets/Resources/SaveAssetCatalog.asset";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) => Rebuild();

        [MenuItem("Tools/Anamnesis/Rebuild Save Catalog")]
        public static void Rebuild()
        {
            var entries = new List<SaveAssetCatalog.Entry>();
            Collect<AttackAsset>(entries);
            Collect<UpgradeAsset>(entries);
            Collect<SkillTreeDefinition>(entries);
            Collect<AnomalyData>(entries);
            Collect<DifficultyData>(entries);
            Collect<ModeData>(entries);
            Collect<WaveSequence>(entries);

            var catalog = AssetDatabase.LoadAssetAtPath<SaveAssetCatalog>(AssetPath);
            if (catalog == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
                catalog = ScriptableObject.CreateInstance<SaveAssetCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }

            catalog.SetEntries(entries);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SaveCatalog] {entries.Count} entries");
        }

        private static void Collect<T>(List<SaveAssetCatalog.Entry> into) where T : Object
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (o is not T) continue;
                    if (into.Exists(e => e.asset == o)) continue;

                    string id = AssetDatabase.IsMainAsset(o) ? guid : guid + ":" + o.name;
                    into.Add(new SaveAssetCatalog.Entry { id = id, name = o.name, asset = o });
                }
            }
        }
    }
}
