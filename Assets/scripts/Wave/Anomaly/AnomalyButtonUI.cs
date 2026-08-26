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

        if (instance != null && instance.amd == null) return;

        if (titleText != null) titleText.text = instance.amd.anomalyName;
        if (descText != null) descText.text = instance.amd.desc;
    }

    public void OnClick() => onSelectedCallback?.Invoke(cachedInstance);

    private void OnDestroy()
    {
        onSelectedCallback = null;
        cachedInstance = null;
    }
}