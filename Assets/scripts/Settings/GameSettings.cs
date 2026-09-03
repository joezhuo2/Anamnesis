using System;
using System.IO;
using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    [Serializable]
    public class GameSettings
    {
        public const int CurrentVersion = 2;
        public const string FileName = "settings.json";

        public int version = CurrentVersion;
        public int difficultyIndex = 1;
        public bool showEnemyHealthBars = true;
        public bool xpDropsEnabled = true;
        public bool goldDropsEnabled = true;
        public bool showWaveCompletionMessage = true;
        public bool showDamageNumbers = true;
        public string bindingOverridesJson = "";

        private static GameSettings current;

        public static event Action Changed;

        public static GameSettings Current
        {
            get
            {
                if (current == null) Load();
                return current;
            }
        }

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            current = null;
            Changed = null;
        }

        public static void RaiseChanged() => Changed?.Invoke();

        public static void Load()
        {
            current = ReadFromDisk() ?? new GameSettings();
            Changed?.Invoke();
        }

        public static void Save()
        {
            if (current == null) return;

            string path = FilePath;
            string tmp = path + ".tmp";

            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                File.WriteAllText(tmp, JsonUtility.ToJson(current, true));

                if (File.Exists(path)) File.Replace(tmp, path, null);
                else File.Move(tmp, path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameSettings] Save failed: {e.Message}");

                try { if (File.Exists(tmp)) File.Delete(tmp); }
                catch { }
            }
        }

        private static GameSettings ReadFromDisk()
        {
            try
            {
                string path = FilePath;
                if (!File.Exists(path)) return null;

                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return null;

                GameSettings loaded = JsonUtility.FromJson<GameSettings>(json);
                if (loaded == null) return null;

                if (loaded.version != CurrentVersion) loaded.version = CurrentVersion;
                loaded.bindingOverridesJson ??= "";

                return loaded;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameSettings] Load failed, using defaults: {e.Message}");
                return null;
            }
        }
    }
}
