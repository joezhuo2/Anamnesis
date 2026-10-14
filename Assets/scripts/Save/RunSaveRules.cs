using System.Collections.Generic;

namespace CrystalFlux.SaveSystem
{
    public static class RunSaveRules
    {
        public static bool CanSaveProgress(bool runStarted, bool waveActive, bool idleBetweenWaves, bool hasWaveStartSnapshot, bool playerDead)
        {
            if (playerDead || !runStarted) return false;
            if (idleBetweenWaves) return true;
            return waveActive && hasWaveStartSnapshot;
        }

        public static bool CanSaveBuild(bool runStarted, bool waveActive, bool hasWaveStartSnapshot, bool playerDead)
            => playerDead || runStarted && (!waveActive || hasWaveStartSnapshot);

        public static string DefaultRunName(string mode, string difficulty, int wave)
            => SaveStore.NormalizeName($"{mode} {difficulty} - Wave {wave}");

        public static string DefaultBuildName(IList<string> attackNames, int wave)
        {
            string head = attackNames == null || attackNames.Count == 0
                ? "Build"
                : attackNames.Count == 1 ? attackNames[0] : $"{attackNames[0]} / {attackNames[1]}";
            return SaveStore.NormalizeName($"{head} - Wave {wave}");
        }
    }
}
