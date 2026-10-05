using System;
using System.Collections.Generic;
using CrystalFlux.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ContractButtonUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Vector2 tooltipOffset = new(100, -100);

    private AnomalyInstance cachedInstance;
    private Action<AnomalyInstance> onSelectedCallback;

    public void Setup(AnomalyInstance instance, Action<AnomalyInstance> onSelect, string rewardLine = "")
    {
        cachedInstance = instance;
        onSelectedCallback = onSelect;

        var b = GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(OnClick);

        bool valid = instance != null && instance.amd != null;

        if (titleText != null) titleText.text = valid ? instance.amd.anomalyName : "";
        if (descText != null) descText.text = valid ? instance.Description : "";

        if (TryGetComponent<ITooltipDisplay>(out var td))
        {
            var (tt, st, os) = GetTooltip(rewardLine);
            td.ShowTooltip(tt, st, os);
        }
    }

    public void OnClick() => onSelectedCallback?.Invoke(cachedInstance);

    public void ResetForPooling()
    {
        onSelectedCallback = null;
        cachedInstance = null;

        if (TryGetComponent<Button>(out var btn)) btn.onClick.RemoveAllListeners();

        if (titleText != null) titleText.text = "";
        if (descText != null) descText.text = "";

        if (TryGetComponent<ITooltipDisplay>(out var td))
        {
            td.HideTooltip();
            td.ShowTooltip("", "");
        }
    }

    private (string title, string subtitle, Vector2 offset) GetTooltip(string rewardLine)
    {
        if (cachedInstance == null || cachedInstance.amd == null) return ("", "", Vector2.zero);

        var amd = cachedInstance.amd;
        List<string> lines = new()
        {
            "Lasts the whole run and applies every wave"
        };

        switch (cachedInstance)
        {
            case TimeTrialInstance tti: lines.Add($"Clear each wave within {tti.timeRemaining:F0}s to earn its reward"); break;
            case NoDamageTrialInstance: lines.Add("Taking damage forfeits that wave's reward"); break;
        }

        if (amd.disallowOnBossWave) lines.Add("Paused on boss waves (still rewarded)");
        lines.Add($"Removes {amd.anomalyType} anomalies from the anomaly pool");

        if (!string.IsNullOrEmpty(rewardLine)) lines.Add(rewardLine);

        return ($"Contract: {amd.anomalyName}", string.Join("\n", lines), tooltipOffset);
    }

    private void OnDestroy()
    {
        onSelectedCallback = null;
        cachedInstance = null;
    }
}
