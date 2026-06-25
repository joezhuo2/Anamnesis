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

        string details = "";
        if (staminaPerSecond != 0) details += $"Stamina: {staminaPerSecond:F1}/s";
        if (healthPerSecond != 0) details += $"\nHealth: {healthPerSecond:F1}/s";
        if (cps.EffArmor != 0) details += $"\nArmor: {cps.EffArmor} (-{(1f-cps.ArmorRes):F1}%)";
        if (cps.damageRes != 0) details += $"\nRes: {cps.damageRes:F1}%";

        TooltipUI.Instance.ShowTooltip("Resources", details);
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

        string details = "";
        if (cooldown != 0f) details += $"Cooldown: {cooldown:F1}s";
        if (hpCost != 0f) details += $"\nHealth Cost: {hpCost:F0}";
        if (hpGain != 0f) details += $"\nHealth Gain: {hpGain:F0}";
        if (staminaCost != 0f) details += $"\nStamina Cost: {staminaCost:F0}";
        if (staminaGain != 0f) details += $"\nStamina Gain: {staminaGain:F0}";
        if (manaCost != 0f) details += $"\nMana Cost: {manaCost:F0}\n";
        if (manaGain != 0f) details += $"\nMana Gain: {manaGain:F0}";
        if (basePhysDmg != 0f) details += $"\nBase: {basePhysDmg:F0}P";
        if (baseSplDmg != 0f) details += $" {baseSplDmg:F0}S";
        if (trueDmg != 0f) details += $" {trueDmg:F0}T";

        string title = cad.displayName;

        TooltipUI.Instance.ShowTooltip(title, details);
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