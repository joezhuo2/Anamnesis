using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RewardType { Mixed, Basic, Rare, Treasure, Anomaly, Milestone }

public class RewardButton : MonoBehaviour
{
    [Header("UI Visual Elements")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image borderHighlight;
    public Image iconImage;

    [HideInInspector] public GeneratedReward gr;
    private AttackReward ar;
    private PlayerUpgradeReward pur;
    private MilestoneRewardData mrd;
    private RewardType type = RewardType.Basic;
    private bool isCorrupted;
    private Action<GeneratedReward> onStatClaimedCallback;
    private Action<AttackReward> onAttackClaimedCallback;
    private Action<PlayerUpgradeReward> onPlayerUpgradeClaimedCallback;
    private Action<MilestoneRewardData> onMilestoneClaimedCallback;

    public void Setup(GeneratedReward reward, Action<GeneratedReward> claimCallback, string statChangeLine)
    {
        gr = reward;
        onStatClaimedCallback = claimCallback;
        type = RewardType.Basic;
        isCorrupted = false;

        List<string> descLines = new()
        {
            reward.GetDescription(),
            statChangeLine
        };
        descriptionText.text = string.Join("\n", descLines);

        if (reward.rd != null)
        {
            if (borderHighlight != null) borderHighlight.color = reward.rd.displayColor;
            titleText.text = reward.br.baseBuff.ToString();
        }

        if (iconImage != null && reward.br.icon != null)
            iconImage.sprite = reward.br.icon;

        if (TryGetComponent<ITooltipDisplay>(out var td))
        {
            var (tt, st, os) = GetStatRewardTooltip();
            td.ShowTooltip(tt, st, os);
        }

        LinkButtonComponent();
    }

    public void CorruptButton(string statChangeLine, float corruptMult)
    {
        if (isCorrupted) return;

        type = RewardType.Basic;

        List<string> descLines = new()
        {
            gr.GetDescription(),
            statChangeLine,
            $"Corrupted: {corruptMult:F2}x"
        };
        descriptionText.text = string.Join("\n", descLines);

        borderHighlight.color = Color.darkRed;

        isCorrupted = true;

        if (TryGetComponent<ITooltipDisplay>(out var td))
        {
            var (tt, st, os) = GetStatRewardTooltip();
            td.ShowTooltip(tt, st, os);
        }

        LinkButtonComponent();
    }

    public void Setup(AttackReward attackReward, Action<AttackReward> claimCallback)
    {
        ar = attackReward;
        onAttackClaimedCallback = claimCallback;
        type = RewardType.Rare;

        titleText.text = attackReward.attackName;
        descriptionText.text = attackReward.desc;

        if (borderHighlight != null) borderHighlight.color = Color.red;

        if (iconImage != null && attackReward.icon != null)
            iconImage.sprite = attackReward.icon;

        if (TryGetComponent<ITooltipDisplay>(out var td))
        {
            var (tt, st, os) = GetAttackRewardTooltip();
            td.ShowTooltip(tt, st, os);
        }

        LinkButtonComponent();
    }

    public void Setup(PlayerUpgradeReward upgradeReward, Action<PlayerUpgradeReward> claimCallback)
    {
        pur = upgradeReward;
        onPlayerUpgradeClaimedCallback = claimCallback;
        type = RewardType.Treasure;

        titleText.text = upgradeReward.upgradeName;
        descriptionText.text = upgradeReward.desc;

        if (borderHighlight != null) borderHighlight.color = Color.purple;

        if (iconImage != null && upgradeReward.icon != null)
            iconImage.sprite = upgradeReward.icon;

        if (TryGetComponent<ITooltipDisplay>(out var td))
        {
            var (tt, st, os) = GetPlayerUpgradeTooltip();
            td.ShowTooltip(tt, st, os);
        }

        LinkButtonComponent();
    }

    public void Setup(MilestoneRewardData milestoneReward, Action<MilestoneRewardData> claimCallback)
    {
        mrd = milestoneReward;
        onMilestoneClaimedCallback = claimCallback;
        type = RewardType.Milestone;

        titleText.text = milestoneReward.rewardName;
        descriptionText.text = milestoneReward.GetDescription();

        if (borderHighlight != null) borderHighlight.color = Color.teal;

        if (iconImage != null) iconImage.sprite = null;

        if (TryGetComponent<ITooltipDisplay>(out var td))
        {
            var (tt, st, os) = GetMilestoneRewardTooltip();
            td.ShowTooltip(tt, st, os);
        }

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
            case RewardType.Rare: onAttackClaimedCallback?.Invoke(ar); break;
            case RewardType.Basic: onStatClaimedCallback?.Invoke(gr); break;
            case RewardType.Treasure: onPlayerUpgradeClaimedCallback?.Invoke(pur); break;
            case RewardType.Milestone: onMilestoneClaimedCallback?.Invoke(mrd); break;
            default: break;
        }
    }

    public void ResetForPooling()
    {
        onStatClaimedCallback = null;
        onAttackClaimedCallback = null;
        onPlayerUpgradeClaimedCallback = null;
        onMilestoneClaimedCallback = null;
        gr = null;
        ar = null;
        pur = null;
        mrd = null;
        type = RewardType.Basic;
        isCorrupted = false;

        if (TryGetComponent<Button>(out var btn))
            btn.onClick.RemoveAllListeners();

        if (titleText != null) titleText.text = "";
        if (descriptionText != null) descriptionText.text = "";
        if (borderHighlight != null) borderHighlight.color = Color.white;
        if (iconImage != null) iconImage.sprite = null;
    }

    private (string title, string subtitle, Vector2 offset) GetMilestoneRewardTooltip()
    {
        if (mrd == null) return ("", "", Vector2.zero);

        List<string> lines = new() { mrd.GetDescription() };

        return (mrd.rewardName, string.Join("\n", lines), new(100, -100));
    }

    private (string title, string subtitle, Vector2 offset) GetStatRewardTooltip()
    {
        if (gr == null || gr.br == null || gr.rd == null) return ("", "", Vector2.zero);

        List<string> lines = new()
        {
            $"Rarity: {gr.rd.rarityName} (x{gr.rd.mult:F2})",
            $"Base Value: {gr.br.baseBuff.value:F2}",
            $"Final Value: {(gr.finalVal >= 0 ? "+" : "")}{gr.finalVal:F2}",
            $"Stat: {gr.br.baseBuff.ToString()}"
        };

        return (gr.br.baseBuff.ToString(), string.Join("\n", lines), new(100, -100));
    }

    private (string title, string subtitle, Vector2 offset) GetAttackRewardTooltip()
    {
        if (ar == null || ar.newAttack == null) return ("", "", Vector2.zero);

        var attack = ar.newAttack;
        List<string> lines = new() { $"Type: {attack.type} ({attack.pattern})" };
        if (attack.cooldown > 0f) lines.Add($"Cooldown: {attack.cooldown:F1}s");

        if (attack.staminaCost > 0f || attack.staminaCostPct > 0f) lines.Add($"Stamina Cost: {attack.staminaCost:F0} +{attack.staminaCostPct:F1}%");
        if (attack.manaCost > 0f || attack.manaCostPct > 0f) lines.Add($"Mana Cost: {attack.manaCost:F0} +{attack.manaCostPct:F1}%");
        if (attack.healthCost > 0f || attack.healthCostPct > 0f) lines.Add($"Health Cost: {attack.healthCost:F0} +{attack.healthCostPct:F1}%");

        if (attack.healthGainOnHit > 0f || attack.healthPctGainOnHit > 0f) lines.Add($"Health Gain: {attack.healthGainOnHit:F0} +{attack.healthPctGainOnHit:F1}%");
        if (attack.staminaGainOnHit > 0f || attack.staminaPctGainOnHit > 0f) lines.Add($"Stamina Gain: {attack.staminaGainOnHit:F0} +{attack.staminaPctGainOnHit:F1}%");
        if (attack.manaGainOnHit > 0f || attack.manaPctGainOnHit > 0f) lines.Add($"Mana Gain: {attack.manaGainOnHit:F0} +{attack.manaPctGainOnHit:F1}%");

        if (attack.explodeOrbits) lines.Add($"Explodes all orbiting projectiles");
        if (attack.fireOrbits) lines.Add($"Fires all orbiting projectiles");
        if (attack.absorbOrbitPct > 0f) lines.Add($"Absorbs all orbiting projectiles ({attack.absorbOrbitPct:F1}% stat returns)");
        if (attack.redirectOrbits && attack.redirectCount > 0) lines.Add($"Redirects {attack.redirectCount} orbiting projectiles to nearest enemy");

        if (attack.pd != null)
        {
            List<string> dmgTypes = new();
            if (attack.pd.speed > 0f) lines.Add($"Speed: {attack.pd.speed:F1}");
            if (attack.pd.physicalMult > 0f) dmgTypes.Add($"{attack.pd.physicalMult:F0}P");
            if (attack.pd.spellMult > 0f) dmgTypes.Add($"{attack.pd.spellMult:F0}S");
            if (attack.pd.trueMult > 0f) dmgTypes.Add($"{attack.pd.trueMult:F0}T");
            if (dmgTypes.Count > 0) lines.Add($"Damage: {string.Join(" ", dmgTypes)}");
            if (attack.pd.followDistance > 0f) lines.Add($"Homing Distance: {attack.pd.followDistance:F1}");
            if (attack.pd.maxBoomerangDist > 0f) lines.Add($"Boomerang Distance: {attack.pd.maxBoomerangDist:F1}");
            if (attack.pd.orbitSelf) lines.Add($"Orbits Owner at a radius of {attack.pd.orbitRadius:F1}-{attack.pd.orbitRadius + attack.pd.randOrbRadOffset:F1}");
            if (attack.pd.kbForce > 0f) lines.Add($"Knockback: {attack.pd.kbForce:F1} for {attack.pd.knockbackTime:F2}s");
        }

        return (ar.attackName, string.Join("\n", lines), new(100, -100));
    }

    private (string title, string subtitle, Vector2 offset) GetPlayerUpgradeTooltip()
    {
        if (pur == null || pur.upgrade == null) return ("", "", Vector2.zero);

        var upgrade = pur.upgrade;
        List<string> lines = new();

        if (!string.IsNullOrEmpty(pur.desc)) lines.Add(pur.desc);

        lines.Add($"Trigger: {string.Join(", ", upgrade.conditions)}");
        if (upgrade.chance < 1f) lines.Add($"Chance: {upgrade.chance * 100:F0}%");
        if (upgrade.cooldown > 0f) lines.Add($"Cooldown: {upgrade.cooldown:F1}s");
        if (upgrade.delay > 0f) lines.Add($"Delay: {upgrade.delay:F1}s");

        return (pur.upgradeName, string.Join("\n", lines), new(100, -100));
    }
}
