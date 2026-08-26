using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RegularWaveButtonController : MonoBehaviour
{
    [Tooltip("The base WaveManager to start. If empty, found in the scene (excludes UnlimitedWaveManager).")]
    public WaveManager waveManager;

    private void Start()
    {
        if (waveManager == null)
            waveManager = FindBaseWaveManager();

        GetComponent<Button>().onClick.AddListener(OnClick);

        var tt = gameObject.AddComponent<TooltipTrigger>();
        tt.SetupTooltipData(
            "Regular Waves",
            "Play the standard wave sequence.\nWaves follow the configured sequence with fixed enemy counts, levels, and rewards."
        );
    }

    private void OnClick()
    {
        if (waveManager == null) return;

        GameController.Instance.OnGameStart();
        waveManager.CloseAllButtons();
        waveManager.StartNextWave();
    }

    private WaveManager FindBaseWaveManager()
    {
        foreach (var wm in FindObjectsByType<WaveManager>(FindObjectsInactive.Include))
            if (wm.GetType() == typeof(WaveManager)) return wm;
        return null;
    }
}