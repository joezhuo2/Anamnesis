using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TooltipType { Attack, Resources, StatusEffect, Dash, SkillTree, PlayerUpgrade, AttackReward, StatReward, MilestoneReward, ActionButton }
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AttackData cad;
    private IStatProvider cesm;
    private ICurrencyHolder cich;
    private StatusEffect cse;
    private SkillNodeDef snd;
    private PlayerUpgradeReward pur;
    private AttackReward ar;
    private GeneratedReward gr;
    private MilestoneRewardData mrd;
    private string skillTreeFailMessage = "";
    private TooltipType tooltipType;
    private string actionButtonTitle = "";
    private string actionButtonDescription = "";

    public void SetupTooltipData(AttackData ad, IStatProvider esm)
    {
        tooltipType = TooltipType.Attack;
        cad = ad;
        cesm = esm;
    }

    public void SetupTooltipData(IStatProvider esm, ICurrencyHolder ich)
    {
        tooltipType = TooltipType.Resources;
        cesm = esm;
        cich = ich;
    }

    public void SetupTooltipData(StatusEffect se, IStatProvider esm)
    {
        tooltipType = TooltipType.StatusEffect;
        cse = se;
        cesm = esm;
    }

    public void SetupTooltipData(SkillNodeDef node, string failMessage)
    {
        tooltipType = TooltipType.SkillTree;
        snd = node;
        skillTreeFailMessage = failMessage ?? "";
    }

    public void SetupDashTooltipData(IStatProvider esm)
    {
        tooltipType = TooltipType.Dash;
        cesm = esm;
    }

    public void SetupTooltipData(PlayerUpgradeReward upgradeReward)
    {
        tooltipType = TooltipType.PlayerUpgrade;
        pur = upgradeReward;
    }

    public void SetupTooltipData(AttackReward attackReward)
    {
        tooltipType = TooltipType.AttackReward;
        ar = attackReward;
    }

    public void SetupTooltipData(GeneratedReward generatedReward)
    {
        tooltipType = TooltipType.StatReward;
        gr = generatedReward;
    }

    public void SetupTooltipData(MilestoneRewardData milestoneReward)
    {
        tooltipType = TooltipType.MilestoneReward;
        mrd = milestoneReward;
    }

    public void SetupTooltipData(string title, string description)
    {
        tooltipType = TooltipType.ActionButton;
        actionButtonTitle = title;
        actionButtonDescription = description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (tooltipType)
        {
            case TooltipType.Attack: ShowAttackTooltip(); break;
            case TooltipType.Resources: ShowResourcesTooltip(); break;
            case TooltipType.StatusEffect: ShowStatusEffectTooltip(); break;
            case TooltipType.Dash: ShowDashTooltip(); break;
            case TooltipType.SkillTree: ShowSkillTreeTooltip(); break;
            case TooltipType.PlayerUpgrade: ShowPlayerUpgradeTooltip(); break;
            case TooltipType.AttackReward: ShowAttackRewardTooltip(); break;
            case TooltipType.StatReward: ShowStatRewardTooltip(); break;
            case TooltipType.MilestoneReward: ShowMilestoneRewardTooltip(); break;
            case TooltipType.ActionButton: ShowActionButtonTooltip(); break;
            default: break;
        }
    }
    private void ShowStatusEffectTooltip()
    {
        List<string> lines = new();
        if (!string.IsNullOrEmpty(cse.desc)) lines.Add(cse.desc);

        string name = cse.effName + $"[{cse.currentStacks}]";

        TooltipUI.Instance.ShowTooltip(name, string.Join("\n", lines), new(100, -100));
    }
    private void ShowDashTooltip()
    {
        List<string> lines = new();
        if (cesm.GetStat(StatType.dodgeChance) != 0f)
            lines.Add($"Dodge: {cesm.GetStat(StatType.dodgeChance):F0}% (-{cesm.GetStat(StatType.dodgeResPct):F0}%)");
        if (cesm.GetStat(StatType.EffSpd) != 0)
            lines.Add($"Speed: {cesm.GetStat(StatType.EffSpd):F2} (+{cesm.GetStat(StatType.moveSpeedPct):F0}%)");
        if (cesm.GetStat(StatType.EffDashCooldown) != 0)
            lines.Add($"Dash Cooldown: {cesm.GetStat(StatType.EffDashCooldown):F1}s");
        if (cesm.GetStat(StatType.EffDashDistance) != 0)
            lines.Add($"Dash Distance: {cesm.GetStat(StatType.EffDashDistance):F1}");
        if (cesm.GetStat(StatType.EffDashStaminaCost) != 0)
            lines.Add($"Dash Stamina Cost: {cesm.GetStat(StatType.EffDashStaminaCost):F1}");

        TooltipUI.Instance.ShowTooltip("Movement", string.Join("\n", lines), new(100, 30));
    }
    public void ShowSkillTreeTooltip()
    {
        if (snd == null) return;

        List<string> lines = new();
        if (!string.IsNullOrEmpty(snd.desc)) lines.Add(snd.desc);
        if (!string.IsNullOrEmpty(skillTreeFailMessage))
            lines.Add($"<color=#FF4444>{skillTreeFailMessage}</color>");

        if (!snd.isStartingNode)
        {
            var playerSkillTree = FindAnyObjectByType<PlayerSkillTree>();
            if (playerSkillTree != null && playerSkillTree.IsNodeUnlocked(snd))
            {
                var (canUndo, _) = playerSkillTree.CanUndo(snd);
                if (canUndo) lines.Add($"<color=#FFD700>Left-click to undo ({snd.undoCost}g)</color>");
                else lines.Add($"<color=#888888>Undo cost: {snd.undoCost}g (insufficient gold)</color>");
            }
        }

        TooltipUI.Instance.ShowTooltip(snd.nodeName, string.Join("\n", lines), new(100, -100));
    }

    public void HideSkillTreeTooltip()
    {
        skillTreeFailMessage = "";
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }

    private void ShowResourcesTooltip()
    {
        float staminaPerSecond = cesm.GetStat(StatType.EffStReg) / 5f;
        float healthPerSecond = cesm.GetStat(StatType.EffHpReg) / 5f;

        List<string> lines = new();
        if (staminaPerSecond != 0) lines.Add($"Stamina: {staminaPerSecond:F1}/s (+{cesm.GetStat(StatType.stRegPct):F0}%)");
        if (healthPerSecond != 0) lines.Add($"Health: {healthPerSecond:F1}/s (+{cesm.GetStat(StatType.hpRegPct):F0}%)");
        if (cesm.GetStat(StatType.EffArmor) != 0) lines.Add($"Armor: {cesm.GetStat(StatType.EffArmor)} (+{cesm.GetStat(StatType.armorPct):F0}%) [-{cesm.GetStat(StatType.ArmorRes)*100f:F1}%P]");
        if (cesm.GetStat(StatType.EffAtk) != 0) lines.Add($"Attack: {cesm.GetStat(StatType.EffAtk):F0} (+{cesm.GetStat(StatType.atkPct):F0}%)");
        if (cesm.GetStat(StatType.EffInt) != 0) lines.Add($"Int: {cesm.GetStat(StatType.EffInt):F0} (+{cesm.GetStat(StatType.IntPct):F0}%)");
        if (cesm.GetStat(StatType.EffectRes) != 0) lines.Add($"Effect Res: {cesm.GetStat(StatType.EffectRes):F0}%");

        List<string> resTypes = new();
        if (cesm.GetStat(StatType.damageRes) != 0f) resTypes.Add($"{cesm.GetStat(StatType.damageRes):F1}%");
        if (cesm.GetStat(StatType.physicalRes) != 0f) resTypes.Add($"P:{cesm.GetStat(StatType.physicalRes):F1}%");
        if (cesm.GetStat(StatType.spellRes) != 0f) resTypes.Add($"S:{cesm.GetStat(StatType.spellRes):F1}%");

        if (resTypes.Count > 0) lines.Add($"Res: {string.Join(" ", resTypes)}");

        if (cich.CurrentAmount > 0) lines.Add($"Gold: {cich.CurrentAmount}");

        TooltipUI.Instance.ShowTooltip("Resources", string.Join("\n", lines), new(100, -100));
    }
    private void ShowAttackTooltip()
    {
        if (cad == null || cesm == null || tooltipType != TooltipType.Attack) return;

        var (sp, hp, mp) = PlayerAttackHandler.GetCosts(cad, cesm);
        var (spg, hpg, mpg) = Projectile.CalculateStatGains((cesm as Component).gameObject, cad);
        var effCd = PlayerAttackHandler.GetEffCd(cad, cesm);

        float basePhysDmg = 0f, baseSplDmg = 0f, trueDmg = 0f;
        if (cad.pd != null)
        {
            var previewSnapshot = ProjectileSnapshot.CaptureSnapshot(cad.pd, (cesm as Component).gameObject);
            var previewPacket = DamagePacket.BuildDamagePacket(cad.pd, previewSnapshot, false, gameObject);

            foreach (var instance in previewPacket.instances)
            {
                switch (instance.type)
                {
                    case DamageType.Physical: basePhysDmg += instance.amount; break;
                    case DamageType.Spell: baseSplDmg += instance.amount; break;
                    case DamageType.True: trueDmg += instance.amount; break;
                    default: break;
                }
            }
        }

        List<string> lines = new() { $"{cad.type}" };
        if (effCd != 0f) lines.Add($"Cooldown: {effCd:F1}s");
        if (hp != 0f || hpg != 0f) lines.Add($"Health: -{hp:F0} +{hpg:F0} +{cad.healthPctGainOnHit:F1}%");
        if (sp != 0f || spg != 0f) lines.Add($"Stamina: -{sp:F0} +{spg:F0} +{cad.staminaPctGainOnHit:F1}%");
        if (mp != 0f || mpg != 0f) lines.Add($"Mana: -{mp:F0} +{mpg:F0} +{cad.manaPctGainOnHit:F1}%");
        if (cesm.GetStat(StatType.critChance) != 0f || cesm.GetStat(StatType.critDamage) != 0f)
            lines.Add($"Crit: {cesm.GetStat(StatType.critChance):F1}% +{cesm.GetStat(StatType.critDamage):F1}%");
        if (cesm.GetStat(StatType.defShred) != 0f || cesm.GetStat(StatType.resPen) != 0f)
            lines.Add($"Shred: {cesm.GetStat(StatType.defShred):F0}A {cesm.GetStat(StatType.resPen):F0}R");

        List<string> dmgTypes = new();
        if (basePhysDmg != 0f) dmgTypes.Add($"{basePhysDmg:F0}P");
        if (baseSplDmg != 0f) dmgTypes.Add($"{baseSplDmg:F0}S");
        if (trueDmg != 0f) dmgTypes.Add($"{trueDmg:F0}T");

        if (dmgTypes.Count > 0)
            lines.Add($"Base: {string.Join(" ", dmgTypes)}");

        TooltipUI.Instance.ShowTooltip(cad.displayName, string.Join("\n", lines), new(0, -100));
    }
    private void ShowPlayerUpgradeTooltip()
    {
        if (pur == null || pur.upgrade == null) return;

        var upgrade = pur.upgrade;
        List<string> lines = new();

        if (!string.IsNullOrEmpty(pur.desc)) lines.Add(pur.desc);

        lines.Add($"Trigger: {string.Join(", ", upgrade.conditions)}");
        if (upgrade.chance < 1f) lines.Add($"Chance: {upgrade.chance * 100:F0}%");
        if (upgrade.cooldown > 0f) lines.Add($"Cooldown: {upgrade.cooldown:F1}s");
        if (upgrade.delay > 0f) lines.Add($"Delay: {upgrade.delay:F1}s");

        TooltipUI.Instance.ShowTooltip(pur.upgradeName, string.Join("\n", lines), new(100, -100));
    }
    private void ShowAttackRewardTooltip()
    {
        if (ar == null || ar.newAttack == null) return;

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

        TooltipUI.Instance.ShowTooltip(ar.attackName, string.Join("\n", lines), new(100, -100));
    }
    private void ShowStatRewardTooltip()
    {
        if (gr == null || gr.br == null || gr.rd == null) return;

        List<string> lines = new()
        {
            $"Rarity: {gr.rd.rarityName} (x{gr.rd.mult:F2})",
            $"Base Value: {gr.br.baseBuff.value:F2}",
            $"Final Value: {(gr.finalVal >= 0 ? "+" : "")}{gr.finalVal:F2}",
            $"Stat: {gr.br.baseBuff.ToString()}"
        };

        TooltipUI.Instance.ShowTooltip(gr.br.baseBuff.ToString(), string.Join("\n", lines), new(100, -100));
    }
    private void ShowMilestoneRewardTooltip()
    {
        if (mrd == null) return;

        List<string> lines = new() { mrd.GetDescription() };

        TooltipUI.Instance.ShowTooltip(mrd.rewardName, string.Join("\n", lines), new(100, -100));
    }
    private void ShowActionButtonTooltip()
    {
        if (string.IsNullOrEmpty(actionButtonTitle)) return;

        TooltipUI.Instance.ShowTooltip(actionButtonTitle, actionButtonDescription, new(100, -100));
    }
    public void OnPointerExit(PointerEventData eventData) => CloseTooltip();
    private void OnDisable() => CloseTooltip();
    private void CloseTooltip()
    {
        if (tooltipType == TooltipType.SkillTree)
        {
            HideSkillTreeTooltip();
            return;
        }

        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }
}