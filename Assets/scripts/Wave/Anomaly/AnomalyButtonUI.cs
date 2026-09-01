using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AnomalyButtonUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    private AnomalyInstance cachedInstance;
    private Action<AnomalyInstance> onSelectedCallback;

    public void Setup(AnomalyInstance instance, Action<AnomalyInstance> onSelect)
    {
        cachedInstance = instance;
        onSelectedCallback = onSelect;

        var b = GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(OnClick);

        bool valid = instance != null && instance.amd != null;

        if (titleText != null) titleText.text = valid ? instance.amd.anomalyName : "";
        if (descText != null) descText.text = valid ? instance.Description : "";
    }

    public void OnClick() => onSelectedCallback?.Invoke(cachedInstance);

    public void ResetForPooling()
    {
        onSelectedCallback = null;
        cachedInstance = null;

        if (TryGetComponent<Button>(out var btn)) btn.onClick.RemoveAllListeners();

        if (titleText != null) titleText.text = "";
        if (descText != null) descText.text = "";
    }

    private void OnDestroy()
    {
        onSelectedCallback = null;
        cachedInstance = null;
    }
}
