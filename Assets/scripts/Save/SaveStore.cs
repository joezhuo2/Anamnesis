using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CrystalFlux.SaveSystem
{
    public static class SaveStore
    {
        public const int MaxNameLength = 40;

        private static string root;

        public static string Root
        {
            get => root ??= Path.Combine(Application.persistentDataPath, "saves");
            set => root = value;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => root = null;

        public static string DirFor(SaveKind kind) => Path.Combine(Root, kind == SaveKind.Build ? "builds" : "runs");

        private static string PathFor(SaveKind kind, string id) => Path.Combine(DirFor(kind), id + ".json");

        public static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            name = name.Trim();
            return name.Length > MaxNameLength ? name.Substring(0, MaxNameLength).TrimEnd() : name;
        }

        public static bool SameName(string a, string b)
            => string.Equals(NormalizeName(a), NormalizeName(b), StringComparison.OrdinalIgnoreCase);

        public static List<SaveFile> List(SaveKind kind)
        {
            var result = new List<SaveFile>();
            string dir = DirFor(kind);
            if (!Directory.Exists(dir)) return result;

            foreach (string path in Directory.GetFiles(dir, "*.json"))
            {
                SaveFile f = ReadPath(path);
                if (f != null && f.meta.kind == kind) result.Add(f);
            }

            return result;
        }

        public static SaveFile Read(SaveKind kind, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string path = PathFor(kind, id);
            return File.Exists(path) ? ReadPath(path) : null;
        }

        private static SaveFile ReadPath(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return null;

                SaveFile f = JsonUtility.FromJson<SaveFile>(json);
                if (f == null || f.meta == null || string.IsNullOrEmpty(f.meta.id)) return null;

                f.build ??= new BuildData();
                f.run ??= new RunData();
                return f;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveStore] Skipping unreadable save '{path}': {e.Message}");
                return null;
            }
        }

        public static bool Write(SaveFile file)
        {
            if (file == null || file.meta == null) return false;
            if (string.IsNullOrEmpty(file.meta.id)) file.meta.id = Guid.NewGuid().ToString("N");
            file.meta.name = NormalizeName(file.meta.name);

            string path = PathFor(file.meta.kind, file.meta.id);
            string tmp = path + ".tmp";

            try
            {
                Directory.CreateDirectory(DirFor(file.meta.kind));
                File.WriteAllText(tmp, JsonUtility.ToJson(file, true));

                if (File.Exists(path)) File.Replace(tmp, path, null);
                else File.Move(tmp, path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveStore] Save failed: {e.Message}");

                try { if (File.Exists(tmp)) File.Delete(tmp); }
                catch { }

                return false;
            }
        }

        public static bool Delete(SaveKind kind, string id)
        {
            try
            {
                string path = PathFor(kind, id);
                if (!File.Exists(path)) return false;

                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveStore] Delete failed: {e.Message}");
                return false;
            }
        }

        public static bool Rename(SaveKind kind, string id, string name)
        {
            string n = NormalizeName(name);
            if (n.Length == 0) return false;

            SaveFile f = Read(kind, id);
            if (f == null) return false;

            f.meta.name = n;
            return Write(f);
        }

        public static SaveFile FindByName(SaveKind kind, string name)
        {
            foreach (SaveFile f in List(kind))
                if (SameName(f.meta.name, name)) return f;
            return null;
        }
    }
}
