using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TooltipType { Attack, Resources, StatusEffect, Dash }
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AttackData cad;
    private PlayerStats cps;
    private EntityStatManager cesm;
    private StatusEffect cse;
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
    public void SetupDashTooltipData(PlayerStats ps)
    {
        tooltipType = TooltipType.Dash;
        cps = ps;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (tooltipType)
        {
            case TooltipType.Attack: ShowAttackTooltip(); break;
            case TooltipType.Resources: ShowResourcesTooltip(); break;
            case TooltipType.StatusEffect: ShowStatusEffectTooltip(); break;
            case TooltipType.Dash: ShowDashTooltip(); break;
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
        if (cps.FinalSpd != 0) lines.Add($"Speed: {cps.FinalSpd} (+{cps.moveSpeedPct:F0}%)");
        if (cps.dashCooldown != 0) lines.Add($"Dash Cooldown: {cps.dashCooldown:F1}s");
        if (cps.dashDistance != 0) lines.Add($"Dash Distance: {cps.dashDistance:F1}");
        if (cps.dashStaminaCost != 0) lines.Add($"Dash Stamina Cost: {cps.dashStaminaCost:F1}");

        TooltipUI.Instance.ShowTooltip("Movement", string.Join("\n", lines), new(100, 30));
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

        List<string> resTypes = new();
        if (cps.damageRes != 0f) resTypes.Add($"{cps.damageRes:F1}%");
        if (cps.physicalRes != 0f) resTypes.Add($"P:{cps.physicalRes:F1}%");
        if (cps.spellRes != 0f) resTypes.Add($"S:{cps.spellRes:F1}%");

        if (resTypes.Count > 0)
            lines.Add($"Res: {string.Join(" ", resTypes)}");

        TooltipUI.Instance.ShowTooltip("Resources", string.Join("\n", lines), new(100, -100));
    }
    private void ShowAttackTooltip()
    {
        if (cad == null || cps == null || cesm == null || tooltipType != TooltipType.Attack) return;

        float staminaCost = Mathf.Abs(cad.staminaCost + (cps.maxStamina * (cad.staminaCostPct * 0.01f)));
        float staminaGain = Mathf.Abs(cad.staminaGainOnHit + (cps.maxStamina * (cad.staminaPctGainOnHit * 0.01f)));
        float manaCost = Mathf.Abs(cad.manaCost + (cps.maxMana * (cad.manaCostPct * 0.01f)));
        float manaGain = Mathf.Abs(cad.manaGainOnHit + (cps.maxMana * (cad.manaPctGainOnHit * 0.01f)));
        float hpCost = Mathf.Abs(cad.healthCost + (cps.EffMaxHp * (cad.healthCostPct * 0.01f)));
        float hpGain = Mathf.Abs(cad.healthGainOnHit + (cps.EffMaxHp * (cad.healthPctGainOnHit * 0.01f)));
        float cooldown = cad.cooldown * Mathf.Clamp(1f - (cps.attackSpeedPct * 0.01f), 0.1f, 10f);

        float basePhysDmg = 0f, baseSplDmg = 0f, trueDmg = 0f;
        if (cad.pd != null)
        {
            var previewSnapshot = DamageCalculator.CaptureSnapshot(cad.pd, cesm.gameObject);
            var previewPacket = DamageCalculator.BuildDamagePacket(cad.pd, previewSnapshot, false);

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
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }
}