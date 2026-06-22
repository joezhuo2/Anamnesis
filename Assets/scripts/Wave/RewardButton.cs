using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardButton : MonoBehaviour
{
    [Header("UI Visual Elements")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image cardBackground;
    public Image borderHighlight;
    public Image iconImage;

    private GeneratedReward rewardData;
    private Action<GeneratedReward> onClaimCallback;

    public void Setup(GeneratedReward reward, Action<GeneratedReward> claimCallback)
    {
        rewardData = reward;
        onClaimCallback = claimCallback;

        descriptionText.text = reward.GetDescription();

        if (reward.rd != null)
        {
            if (borderHighlight != null) borderHighlight.color = reward.rd.displayColor;
            titleText.text = reward.br.baseBuff.type.ToString().ToUpper();
        }

        if (iconImage != null && reward.br.icon != null)
            iconImage.sprite = reward.br.icon;

        if (TryGetComponent<Button>(out var btn))
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(HandleClick);
        }
    }
    private void HandleClick()
    {
        onClaimCallback?.Invoke(rewardData);
    }
}