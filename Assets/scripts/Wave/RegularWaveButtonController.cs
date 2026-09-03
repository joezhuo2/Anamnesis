using UnityEngine;
using UnityEngine.UI;
using CrystalFlux.Core;

namespace CrystalFlux.WaveSystem
{
    [RequireComponent(typeof(Button))]
    public class RegularWaveButtonController : MonoBehaviour
    {
        public WaveManager waveManager;

        private void Start()
        {
            if (waveManager == null)
                waveManager = FindBaseWaveManager();

            GetComponent<Button>().onClick.AddListener(OnClick);

            if (gameObject.TryGetComponent<ITooltipDisplay>(out var tt))
                tt.ShowTooltip("Regular Waves", "Play the standard wave sequence.\nWaves follow the configured sequence with fixed enemy counts, levels, and rewards.");
        }

        private void OnClick()
        {
            if (waveManager == null) return;

            waveManager.CloseAllButtons();

            DifficultySelector.Current?.LockIn(waveManager);

            IAnnouncer.Current?.DisableSubtitle();
            IAnnouncer.Current?.DisableTitle();

            if (!waveManager.TryStartPreRunPicks()) waveManager.StartNextWave();
        }

        private WaveManager FindBaseWaveManager()
        {
            foreach (var wm in FindObjectsByType<WaveManager>(FindObjectsInactive.Include))
                if (wm.GetType() == typeof(WaveManager)) return wm;
            return null;
        }
    }
}
