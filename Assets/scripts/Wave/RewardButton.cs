using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RewardType { Mixed, Basic, Rare, Treasure, Anomaly }

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
    private PlayerUpgradeReward playerUpgradeRewardData;
    private RewardType type = RewardType.Basic;

    private Action<GeneratedReward> onStatClaimedCallback;
    private Action<AttackReward> onAttackClaimedCallback;
    private Action<PlayerUpgradeReward> onPlayerUpgradeClaimedCallback;

    public void Setup(GeneratedReward reward, Action<GeneratedReward> claimCallback)
    {
        statRewardData = reward;
        onStatClaimedCallback = claimCallback;
        type = RewardType.Basic;

        descriptionText.text = reward.GetDescription();

        if (reward.rd != null)
        {
            if (borderHighlight != null) borderHighlight.color = reward.rd.displayColor;
            titleText.text = reward.br.baseBuff.ToString();
        }

        if (iconImage != null && reward.br.icon != null)
            iconImage.sprite = reward.br.icon;

        LinkButtonComponent();
    }
    public void Setup(AttackReward attackReward, Action<AttackReward> claimCallback)
    {
        attackRewardData = attackReward;
        onAttackClaimedCallback = claimCallback;
        type = RewardType.Rare;

        titleText.text = attackReward.attackName;
        descriptionText.text = attackReward.desc;

        if (borderHighlight != null) borderHighlight.color = Color.red;

        if (iconImage != null && attackReward.icon != null)
            iconImage.sprite = attackReward.icon;

        LinkButtonComponent();
    }
    public void Setup(PlayerUpgradeReward upgradeReward, Action<PlayerUpgradeReward> claimCallback)
    {
        playerUpgradeRewardData = upgradeReward;
        onPlayerUpgradeClaimedCallback = claimCallback;
        type = RewardType.Treasure;

        titleText.text = upgradeReward.upgradeName;
        descriptionText.text = upgradeReward.desc;

        if (borderHighlight != null) borderHighlight.color = Color.purple;

        if (iconImage != null && upgradeReward.icon != null)
            iconImage.sprite = upgradeReward.icon;

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
        switch (type)
        {
            case RewardType.Rare: onAttackClaimedCallback?.Invoke(attackRewardData); break;
            case RewardType.Basic: onStatClaimedCallback?.Invoke(statRewardData); break;
            case RewardType.Treasure: onPlayerUpgradeClaimedCallback?.Invoke(playerUpgradeRewardData); break;
            default: break;
        }
    }
}