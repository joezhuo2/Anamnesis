using UnityEngine;
using UnityEngine.UI;
using CrystalFlux.Core;

namespace CrystalFlux.WaveSystem
{
    [RequireComponent(typeof(Button))]
    public class UnlimitedWaveButtonController : MonoBehaviour
    {
        public UnlimitedWaveManager unlimitedWaveManager;

        private void Start()
        {
            if (unlimitedWaveManager == null) unlimitedWaveManager = FindAnyObjectByType<UnlimitedWaveManager>();

            GetComponent<Button>().onClick.AddListener(OnClick);

            if (gameObject.TryGetComponent<ITooltipDisplay>(out var tt))
                tt.ShowTooltip("Unlimited Waves", "Endless waves with no sequence cap.\nEnemies scale infinitely, with faster spawns and periodic boss waves.\nRewards keep coming every wave.");
        }

        private void OnClick()
        {
            if (unlimitedWaveManager == null) return;

            unlimitedWaveManager.CloseAllButtons();

            DifficultySelector.Current?.LockIn(unlimitedWaveManager);
            IronmanSelector.Current?.LockIn();

            IAnnouncer.Current?.DisableSubtitle();
            IAnnouncer.Current?.DisableTitle();

            if (!unlimitedWaveManager.TryStartPreRunPicks()) unlimitedWaveManager.StartNextWave();
        }
    }
}
