using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TooltipType { Attack, Resources, StatusEffect, Dash, SkillTree, PlayerUpgrade, AttackReward, StatReward, MilestoneReward }
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AttackData cad;
    private PlayerStats cps;
    private EntityStatManager cesm;
    private StatusEffect cse;
    private SkillNodeDef snd;
    private PlayerUpgradeReward pur;
    private AttackReward ar;
    private GeneratedReward gr;
    private MilestoneRewardData mrd;
    private string skillTreeFailMessage = "";
    private TooltipType tooltipType;

    public void SetupTooltipData(AttackData ad, PlayerStats ps, EntityStatManager esm)
    {
        tooltipType = TooltipType.Attack;
        cad = ad;
        cps = ps;
        cesm = esm;
    }
    public void SetupTooltipData(PlayerStats ps)
    {
        tooltipType = TooltipType.Resources;
        cps = ps;
    }
    public void SetupTooltipData(StatusEffect se, PlayerStats ps, EntityStatManager esm)
    {
        tooltipType = TooltipType.StatusEffect;
        cse = se;
        cps = ps;
        cesm = esm;
    }

    public void SetupTooltipData(SkillNodeDef node, string failMessage)
    {
        tooltipType = TooltipType.SkillTree;
        snd = node;
        skillTreeFailMessage = failMessage ?? "";
    }
    public void SetupDashTooltipData(PlayerStats ps)
    {
        tooltipType = TooltipType.Dash;
        cps = ps;
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
        if (cps.dodgeChance != 0f) lines.Add($"Dodge: {cps.dodgeChance:F0}% (-{cps.dodgeResPct:F0}%)");
        if (cps.FinalSpd != 0) lines.Add($"Speed: {cps.FinalSpd:F2} (+{cps.moveSpeedPct:F0}%)");
        if (cps.EffDashCooldown != 0) lines.Add($"Dash Cooldown: {cps.EffDashCooldown:F1}s");
        if (cps.EffDashDistance != 0) lines.Add($"Dash Distance: {cps.EffDashDistance:F1}");
        if (cps.EffDashStaminaCost != 0) lines.Add($"Dash Stamina Cost: {cps.EffDashStaminaCost:F1}");

        TooltipUI.Instance.ShowTooltip("Movement", string.Join("\n", lines), new(100, 30));
    }
    public void ShowSkillTreeTooltip()
    {
        if (snd == null) return;

        List<string> lines = new();
        if (!string.IsNullOrEmpty(snd.desc)) lines.Add(snd.desc);
        if (!string.IsNullOrEmpty(skillTreeFailMessage)) lines.Add($"<color=#FF4444>{skillTreeFailMessage}</color>");

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
        float staminaPerSecond = cps.EffStReg / 5f;
        float healthPerSecond = cps.EffHpReg / 5f;

        List<string> lines = new();
        if (staminaPerSecond != 0) lines.Add($"Stamina: {staminaPerSecond:F1}/s (+{cps.stRegPct:F0}%)");
        if (healthPerSecond != 0) lines.Add($"Health: {healthPerSecond:F1}/s (+{cps.hpRegPct:F0}%)");
        if (cps.EffArmor != 0) lines.Add($"Armor: {cps.EffArmor} (+{cps.armorPct:F0}%) [-{cps.ArmorRes*100f:F1}%P]");
        if (cps.EffAtk != 0) lines.Add($"Attack: {cps.EffAtk:F0} (+{cps.atkPct:F0}%)");
        if (cps.EffInt != 0) lines.Add($"Int: {cps.EffInt:F0} (+{cps.intPct:F0}%)");
        if (cps.effectRes != 0) lines.Add($"Effect Res: {cps.effectRes:F0}%");

        List<string> resTypes = new();
        if (cps.damageRes != 0f) resTypes.Add($"{cps.damageRes:F1}%");
        if (cps.physicalRes != 0f) resTypes.Add($"P:{cps.physicalRes:F1}%");
        if (cps.spellRes != 0f) resTypes.Add($"S:{cps.spellRes:F1}%");

        if (resTypes.Count > 0) lines.Add($"Res: {string.Join(" ", resTypes)}");

        if (cps.gold > 0) lines.Add($"Gold: {cps.gold}");

        TooltipUI.Instance.ShowTooltip("Resources", string.Join("\n", lines), new(100, -100));
    }
    private void ShowAttackTooltip()
    {
        if (cad == null || cps == null || cesm == null || tooltipType != TooltipType.Attack) return;

        float staminaCost = Mathf.Abs(cad.staminaCost + (cps.EffMaxStamina * (cad.staminaCostPct * 0.01f))) * (1f + (cps.stCostPct * 0.01f));
        float staminaGain = Mathf.Abs(cad.staminaGainOnHit + (cps.EffMaxStamina * (cad.staminaPctGainOnHit * 0.01f)));
        float manaCost = Mathf.Abs(cad.manaCost + (cps.EffMaxMana * (cad.manaCostPct * 0.01f)));
        float manaGain = Mathf.Abs(cad.manaGainOnHit + (cps.EffMaxMana * (cad.manaPctGainOnHit * 0.01f)));
        float hpCost = Mathf.Abs(cad.healthCost + (cps.EffMaxHp * (cad.healthCostPct * 0.01f)));
        float hpGain = Mathf.Abs(cad.healthGainOnHit + (cps.EffMaxHp * (cad.healthPctGainOnHit * 0.01f)));

        float cdRedPct = cad.type switch
        {
            AttackType.Basic => cps.basicCdRedPct,
            AttackType.Skill => cps.skillCdRedPct,
            AttackType.Ultimate => cps.ultCdRedPct,
            _ => 0f
        };

        float cooldown = cad.cooldown * Mathf.Clamp(1f - (cps.attackSpeedPct * 0.01f), 0.3f, 10f) * Mathf.Clamp(1f - (cdRedPct * 0.01f), 0f, 0.9f);

        float basePhysDmg = 0f, baseSplDmg = 0f, trueDmg = 0f;
        if (cad.pd != null)
        {
            var previewSnapshot = DamageCalculator.CaptureSnapshot(cad.pd, cesm.gameObject);
            var previewPacket = DamageCalculator.BuildDamagePacket(cad.pd, previewSnapshot, false, gameObject);

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

        List<string> lines = new();
        lines.Add($"{cad.type}");
        if (cooldown != 0f) lines.Add($"Cooldown: {cooldown:F1}s");
        if (hpCost != 0f || hpGain != 0f) lines.Add($"Health: -{hpCost:F0} +{cad.healthGainOnHit:F0} +{cad.healthPctGainOnHit:F1}%");
        if (staminaCost != 0f || staminaGain != 0f) lines.Add($"Stamina: -{staminaCost:F0} +{cad.staminaGainOnHit:F0} +{cad.staminaPctGainOnHit:F1}%");
        if (manaCost != 0f || manaGain != 0f) lines.Add($"Mana: -{manaCost:F0} +{cad.manaGainOnHit:F0} +{cad.manaPctGainOnHit:F1}%");
        if (cps.critChance != 0f || cps.critDamage != 0f) lines.Add($"Crit: {cps.critChance:F1}% +{cps.critDamage:F1}%");
        if (cps.defShred != 0f || cps.resPen != 0f) lines.Add($"Shred: {cps.defShred}A {cps.resPen}R");

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
        List<string> lines = new();

        lines.Add($"Type: {attack.type} ({attack.pattern})");
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

        List<string> lines = new();
        lines.Add($"Rarity: {gr.rd.rarityName} (x{gr.rd.mult:F2})");
        lines.Add($"Base Value: {gr.br.baseBuff.value:F2}");
        lines.Add($"Final Value: {(gr.finalVal >= 0 ? "+" : "")}{gr.finalVal:F2}");
        lines.Add($"Stat: {gr.br.baseBuff.ToString()}");

        TooltipUI.Instance.ShowTooltip(gr.br.baseBuff.ToString(), string.Join("\n", lines), new(100, -100));
    }
    private void ShowMilestoneRewardTooltip()
    {
        if (mrd == null) return;

        List<string> lines = new() { mrd.GetDescription() };

        TooltipUI.Instance.ShowTooltip(mrd.rewardName, string.Join("\n", lines), new(100, -100));
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