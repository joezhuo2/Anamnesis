using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class RewardButton : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public Image icon;
    public Image border;
    private GeneratedReward gr;
    private System.Action<GeneratedReward> onRewardSelectedCallback;

    public void Setup(GeneratedReward data, System.Action<GeneratedReward> onSelected)
    {
        gr = data;
        onRewardSelectedCallback = onSelected;

        title.text = data.GetDisplayName();
        title.color = data.rd.displayColor;

        if (desc != null) desc.text = data.GetDescription();
        if (icon != null) icon.sprite = data.br.icon;
        if (border != null) border.color = data.rd.displayColor;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => onRewardSelectedCallback?.Invoke(gr));
    }
}