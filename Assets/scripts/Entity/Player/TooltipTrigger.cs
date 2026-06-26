using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TooltipType { Attack, Resources }
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AttackData cad;
    private PlayerStats cps;
    private EntityStatManager cesm;
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
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (tooltipType)
        {
            case TooltipType.Attack: ShowAttackTooltip(); break;
            case TooltipType.Resources: ShowResourcesTooltip(); break;
            default: break;
        }
    }
    public void ShowResourcesTooltip()
    {
        float staminaPerSecond = cps.EffStReg / 5f;
        float healthPerSecond = cps.EffHpReg / 5f;

        List<string> lines = new();
        if (staminaPerSecond != 0) lines.Add($"Stamina: {staminaPerSecond:F1}/s");
        if (healthPerSecond != 0) lines.Add($"Health: {healthPerSecond:F1}/s");
        if (cps.EffArmor != 0) lines.Add($"Armor: {cps.EffArmor} (-{(cps.ArmorRes*100f):F1}%)");
        if (cps.damageRes != 0) lines.Add($"Res: {cps.damageRes:F1}%");

        TooltipUI.Instance.ShowTooltip("Resources", string.Join("\n", lines));
    }
    public void ShowAttackTooltip()
    {
        if (cad == null || cps == null || tooltipType != TooltipType.Attack) return;

        float staminaCost = Mathf.Abs(cad.staminaCost + (cps.maxStamina * (cad.staminaCostPct * 0.01f)));
        float staminaGain = Mathf.Abs(cad.staminaGainOnHit + (cps.maxStamina * (cad.staminaPctGainOnHit * 0.01f)));
        float manaCost = Mathf.Abs(cad.manaCost + (cps.maxMana * (cad.manaCostPct * 0.01f)));
        float manaGain = Mathf.Abs(cad.manaGainOnHit + (cps.maxMana * (cad.manaPctGainOnHit * 0.01f)));
        float hpCost = Mathf.Abs(cad.healthCost + (cps.EffMaxHp * (cad.healthCostPct * 0.01f)));
        float hpGain = Mathf.Abs(cad.healthGainOnHit + (cps.EffMaxHp * (cad.healthPctGainOnHit * 0.01f)));
        float cooldown = cad.cooldown * Mathf.Clamp(1f - (cps.attackSpeedPct * 0.01f), 0.1f, 10f);

        float basePhysDmg = 0f, baseSplDmg = 0f, trueDmg = 0f;
        if (cad.projectilePrefab.TryGetComponent<Projectile>(out var p) && p.pd is ProjectileData pd)
        {
            basePhysDmg = (pd.physicalMult + (cps.addPhysDmgPct * 0.01f)) * cesm.GetStat(pd.scalingStat) * (1f + (cps.physicalDmgPct * 0.01f)) * (1f + (cps.damagePct * 0.01f));
            baseSplDmg = (pd.spellMult + (cps.addSplDmgPct * 0.01f)) * cesm.GetStat(pd.scalingStat) * (1f + (cps.spellDmgPct * 0.01f)) * (1f + (cps.damagePct * 0.01f));
            trueDmg = pd.trueMult * cesm.GetStat(pd.scalingStat) * (1f + (cps.damagePct * 0.01f));
        }

        List<string> lines = new();
        if (cooldown != 0f) lines.Add($"Cooldown: {cooldown:F1}s");
        if (hpCost != 0f) lines.Add($"Health Cost: {hpCost:F0}");
        if (hpGain != 0f) lines.Add($"Health Gain: {hpGain:F0}");
        if (staminaCost != 0f) lines.Add($"Stamina Cost: {staminaCost:F0}");
        if (staminaGain != 0f) lines.Add($"Stamina Gain: {staminaGain:F0}");
        if (manaCost != 0f) lines.Add($"Mana Cost: {manaCost:F0}");
        if (manaGain != 0f) lines.Add($"Mana Gain: {manaGain:F0}");

        List<string> dmgTypes = new();
        if (basePhysDmg != 0f) dmgTypes.Add($"{basePhysDmg:F0}P");
        if (baseSplDmg != 0f) dmgTypes.Add($"{baseSplDmg:F0}S");
        if (trueDmg != 0f) dmgTypes.Add($"{trueDmg:F0}T");

        if (dmgTypes.Count > 0)
            lines.Add($"Base: {string.Join(" ", dmgTypes)}");

        TooltipUI.Instance.ShowTooltip(cad.displayName, string.Join("\n", lines));
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