using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AnomalyButtonUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    private AnomalyData cachedData;
    private Action<AnomalyData> onSelectedCallback;

    public void Setup(AnomalyData data, Action<AnomalyData> onSelect)
    {
        cachedData = data;
        onSelectedCallback = onSelect;

        var b = GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(OnClick);

        if (titleText != null) titleText.text = data.anamolyName;
        if (descText != null) descText.text = data.desc;
    }

    public void OnClick() => onSelectedCallback?.Invoke(cachedData);
}