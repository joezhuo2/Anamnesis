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

    private GeneratedReward statRewardData;
    private AttackReward attackRewardData;
    private bool isAttackReward = false;

    private Action<GeneratedReward> onStatClaimedCallback;
    private Action<AttackReward> onAttackClaimedCallback;

    public void Setup(GeneratedReward reward, Action<GeneratedReward> claimCallback)
    {
        statRewardData = reward;
        onStatClaimedCallback = claimCallback;
        isAttackReward = false;

        descriptionText.text = reward.GetDescription();

        if (reward.rd != null)
        {
            if (borderHighlight != null) borderHighlight.color = reward.rd.displayColor;
            titleText.text = reward.br.baseBuff.type.ToString().ToUpper();
        }

        if (iconImage != null && reward.br.icon != null)
            iconImage.sprite = reward.br.icon;

        LinkButtonComponent();
    }
    public void Setup(AttackReward attackReward, Action<AttackReward> claimCallback)
    {
        attackRewardData = attackReward;
        onAttackClaimedCallback = claimCallback;
        isAttackReward = true;

        titleText.text = attackReward.attackName;
        descriptionText.text = attackReward.desc;

        if (borderHighlight != null) borderHighlight.color = Color.red;

        if (iconImage != null && attackReward.icon != null)
            iconImage.sprite = attackReward.icon;

        LinkButtonComponent();
    }

    private void LinkButtonComponent()
    {
        if (TryGetComponent<Button>(out var btn))
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        if (isAttackReward)
        {
            onAttackClaimedCallback?.Invoke(attackRewardData);
        }
        else
        {
            onStatClaimedCallback?.Invoke(statRewardData);
        }
    }
}