using System;
using System.Collections;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.SettingsSystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.SaveSystem
{
    public class RunSaveController : MonoBehaviour
    {
        public static RunSaveController Instance { get; private set; }
        public static event Action PendingBuildChanged;

        private GameObject player;
        private SaveAssetCatalog catalog;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
            PendingBuildChanged = null;
        }

        public static bool HasPendingBuild => RunSession.PendingLoad?.meta != null && RunSession.PendingLoad.meta.kind == SaveKind.Build;
        public static string PendingBuildName => HasPendingBuild ? RunSession.PendingLoad.meta.name : "";

        private void Awake()
        {
            Instance = this;
            catalog = SaveAssetCatalog.Instance;

            SaveFile pending = RunSession.PendingLoad;
            RunSession.ClearForNewScene();
            RunSession.PendingLoad = pending;

            WaveManager.WaveStarting += OnWaveStarting;
            WaveManager.PendingLoadStarter = StartPendingBuild;
        }

        private void OnDestroy()
        {
            WaveManager.WaveStarting -= OnWaveStarting;
            if (WaveManager.PendingLoadStarter == (Func<WaveManager, bool>)StartPendingBuild) WaveManager.PendingLoadStarter = null;
            if (ReferenceEquals(Instance, this)) Instance = null;
        }

        private IEnumerator Start()
        {
            yield return null;

            if (RunSession.PendingLoad?.meta?.kind == SaveKind.Run) ApplyPendingRun();
            else PendingBuildChanged?.Invoke();
        }

        private void Update() => RunSession.Tick(Time.deltaTime);

        private GameObject Player => player != null ? player : player = GameObject.FindGameObjectWithTag("Player");

        private static WaveManager Wm => WaveManager.Active;

        private bool PlayerDead
        {
            get
            {
                if (RunSession.IsPlayerDead != null) return RunSession.IsPlayerDead();
                return Player != null && Player.TryGetComponent<IStatProvider>(out var sp) && sp.GetStat(StatType.isAlive) <= 0f;
            }
        }

        private void OnWaveStarting(WaveManager wm)
        {
            RunSession.ClockRunning = true;
            if (catalog == null || wm == null) return;

            RunSession.WaveStartBuild = BuildCapture.Capture(Player, catalog, wm.CurrentContract);
            RunSession.WaveStartRun = BuildCapture.CaptureRun(wm, catalog);
            RunSession.WaveStartNumber = wm.CurrentWaveNumber + 1;
        }

        public bool CanSaveProgress
        {
            get
            {
                WaveManager wm = Wm;
                return catalog != null && wm != null && RunSaveRules.CanSaveProgress(
                    wm.IsRunStarted, wm.IsWaveActive, wm.IsIdleBetweenWaves, RunSession.WaveStartRun != null, PlayerDead);
            }
        }

        public bool CanSaveBuild
        {
            get
            {
                WaveManager wm = Wm;
                return catalog != null && wm != null && RunSaveRules.CanSaveBuild(wm.IsRunStarted, wm.IsIdleBetweenWaves, PlayerDead);
            }
        }

        private static int NextWave(WaveManager wm) => wm.IsIdleBetweenWaves ? wm.CurrentWaveNumber + 1 : RunSession.WaveStartNumber;

        public string DefaultProgressName
        {
            get
            {
                if (!string.IsNullOrEmpty(RunSession.CurrentRunName)) return RunSession.CurrentRunName;

                WaveManager wm = Wm;
                if (wm == null) return "Run";

                return RunSaveRules.DefaultRunName(
                    wm.mode != null ? wm.mode.displayName : "",
                    wm.difficulty != null ? wm.difficulty.displayName : "",
                    NextWave(wm));
            }
        }

        public string DefaultBuildName
        {
            get
            {
                WaveManager wm = Wm;
                return RunSaveRules.DefaultBuildName(BuildCapture.AttackNames(Player), wm != null ? Mathf.Max(1, wm.CurrentWaveNumber) : 1);
            }
        }

        public bool SaveProgress(string name, bool overwriteExisting)
        {
            if (!CanSaveProgress) return false;

            WaveManager wm = Wm;
            BuildData b;
            RunData r;
            int wave;

            if (wm.IsIdleBetweenWaves)
            {
                b = BuildCapture.Capture(Player, catalog, wm.CurrentContract);
                r = BuildCapture.CaptureRun(wm, catalog);
                wave = wm.CurrentWaveNumber + 1;
            }
            else
            {
                b = RunSession.WaveStartBuild;
                r = RunSession.WaveStartRun;
                wave = RunSession.WaveStartNumber;
            }

            if (!Write(SaveKind.Run, name, overwriteExisting, b, r, wm, wave)) return false;

            RunSession.CurrentRunName = SaveStore.NormalizeName(name);
            GameSettings.Save();
            RunSession.RequestHomeScreen?.Invoke();
            return true;
        }

        public bool SaveBuild(string name, bool overwriteExisting)
        {
            if (!CanSaveBuild) return false;

            WaveManager wm = Wm;
            BuildData b = BuildCapture.Capture(Player, catalog, wm.CurrentContract);
            return Write(SaveKind.Build, name, overwriteExisting, b, null, wm, Mathf.Max(1, wm.CurrentWaveNumber));
        }

        private bool Write(SaveKind kind, string name, bool overwrite, BuildData b, RunData r, WaveManager wm, int wave)
        {
            string n = SaveStore.NormalizeName(name);
            if (n.Length == 0 || b == null) return false;

            SaveFile existing = SaveStore.FindByName(kind, n);
            if (existing != null && !overwrite) return false;

            var f = new SaveFile { build = b, run = r ?? new RunData() };
            f.meta = BuildCapture.MakeMeta(kind, n, b, r, wm, wave, BuildCapture.AttackNames(Player));
            if (existing != null) f.meta.id = existing.meta.id;

            return SaveStore.Write(f);
        }

        public void QueueLoad(SaveFile f)
        {
            if (f?.meta == null) return;

            RunSession.PendingLoad = f;

            WaveManager wm = Wm;
            if (wm != null && wm.IsRunStarted)
            {
                RunSession.RequestHomeScreen?.Invoke();
                return;
            }

            if (f.meta.kind == SaveKind.Run) ApplyPendingRun();
            else PendingBuildChanged?.Invoke();
        }

        public void ClearPendingBuild()
        {
            if (!HasPendingBuild) return;

            RunSession.PendingLoad = null;
            PendingBuildChanged?.Invoke();
        }

        private static WaveManager FindManager(WaveManagerKind kind)
        {
            foreach (var wm in FindObjectsByType<WaveManager>(FindObjectsInactive.Include))
            {
                bool unlimited = wm is UnlimitedWaveManager;
                if (kind == WaveManagerKind.Unlimited ? unlimited : wm.GetType() == typeof(WaveManager)) return wm;
            }

            return null;
        }

        private void ApplyPendingRun()
        {
            SaveFile f = RunSession.PendingLoad;
            RunSession.PendingLoad = null;
            if (f == null || catalog == null) return;

            RunLoadTransaction tx = RunLoadTransaction.Begin(f);
            try
            {
                WaveManager wm = FindManager(f.run.waveManagerKind);
                DifficultyData d = catalog.Get<DifficultyData>(f.run.difficultyId);
                ModeData m = catalog.Get<ModeData>(f.run.modeId);
                if (wm == null || d == null || m == null) throw new InvalidOperationException("Run save references missing data");
                if (!BuildApplier.CanResolve(f.build, catalog)) throw new InvalidOperationException("Build references missing data");

                IronmanSelector.SetEnabled(f.run.ironman);
                DifficultySelector.Current?.Select(d);
                ModeSelector.Current?.Select(m);

                if (DifficultySelector.Current != null) DifficultySelector.Current.LockIn(wm);
                else wm.ApplyDifficulty(d);

                if (ModeSelector.Current != null) ModeSelector.Current.LockIn(wm);
                else wm.ApplyMode(m);

                IronmanSelector.Current?.LockIn();

                if (!BuildApplier.ApplyBuild(Player, f.build, catalog)) throw new InvalidOperationException("Build restore failed");
                wm.RestoreContract(BuildApplier.RecreateContract(f.build, catalog));
                if (!BuildApplier.ApplyRun(wm, f.run, catalog)) throw new InvalidOperationException("Run restore failed");

                RunSession.CurrentRunName = f.meta.name;
                tx.Commit();
                wm.StartLoadedRun();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RunSaveController] Load failed: {e.Message}");
                tx.Rollback();
                RunSession.RequestHomeScreen?.Invoke();
            }
        }

        private bool StartPendingBuild(WaveManager wm)
        {
            if (!HasPendingBuild || catalog == null || wm == null) return false;

            SaveFile f = RunSession.PendingLoad;
            RunSession.PendingLoad = null;

            if (!BuildApplier.ApplyBuild(Player, f.build, catalog))
            {
                Debug.LogWarning("[RunSaveController] Build restore failed");
                PendingBuildChanged?.Invoke();
                return false;
            }

            wm.RestoreContract(BuildApplier.RecreateContract(f.build, catalog));
            if (Player != null) wm.RemoveOwnedFromPools(Player.GetComponent<IAttackHandler>(), Player.GetComponent<IUpgradeHolder>());

            RunSession.Playtime = 0f;
            DpsMeter.SetPeak(0f);
            PendingBuildChanged?.Invoke();
            wm.StartLoadedRun();
            return true;
        }
    }
}
