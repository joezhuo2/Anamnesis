using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UnlimitedWaveButtonController : MonoBehaviour
{
    public UnlimitedWaveManager unlimitedWaveManager;

    private void Start()
    {
        if (unlimitedWaveManager == null) unlimitedWaveManager = FindAnyObjectByType<UnlimitedWaveManager>();

        GetComponent<Button>().onClick.AddListener(OnClick);

        if (gameObject.TryGetComponent<TooltipTrigger>(out var tt))
            tt.SetupTooltipData("Unlimited Waves", "Endless waves with no sequence cap.\nEnemies scale infinitely, with faster spawns and periodic boss waves.\nRewards keep coming every wave.");
    }

    private void OnClick()
    {
        if (unlimitedWaveManager == null) return;

        GameController.Instance.OnGameStart();
        unlimitedWaveManager.CloseAllButtons();
        unlimitedWaveManager.StartNextWave();
    }
}