using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.UISystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CrystalFlux.EntitySystem
{
    public class PlayerResourceUI : MonoBehaviour, IPointerEnterHandler
    {
        private IStatProvider esm;
        public GameObject resourceHoverZone;

        public void Setup(IStatProvider isp)
        {
            esm = isp;
            ShowTooltip();
        }

        private string F(StatType t) => NumberFormat.Abbrev(esm.GetStat(t));
        private string Pct(StatType t) => NumberFormat.Abbrev(esm.GetStat(t));
        private string Res(StatType t) => NumberFormat.Abbrev(esm.GetStat(t) * 100f);

        private (string title, string subtitle, Vector2 offset) GetResourcesTooltip()
        {
            if (esm == null) return ("Resources", "", new(100, -100));

            float staminaPerSecond = esm.GetStat(StatType.EffStReg) / 5f;
            float healthPerSecond = esm.GetStat(StatType.EffHpReg) / 5f;

            List<string> lines = new();
            if (staminaPerSecond != 0) lines.Add($"Stamina: {NumberFormat.Abbrev(staminaPerSecond)}/s (+{Pct(StatType.stRegPct)}%)");
            if (healthPerSecond != 0) lines.Add($"Health: {NumberFormat.Abbrev(healthPerSecond)}/s (+{Pct(StatType.hpRegPct)}%)");
            if (esm.GetStat(StatType.EffArmor) != 0) lines.Add($"Armor: {F(StatType.EffArmor)} (+{Pct(StatType.armorPct)}%) [-{Res(StatType.ArmorRes)}%P]");
            if (esm.GetStat(StatType.EffArcaneShield) != 0) lines.Add($"Arcane Shield: {F(StatType.EffArcaneShield)} (+{Pct(StatType.arcaneShieldPct)}%) [-{Res(StatType.ArcaneShieldRes)}%S]");
            if (esm.GetStat(StatType.EffDefense) != 0) lines.Add($"Defense: {F(StatType.EffDefense)} (+{Pct(StatType.defensePct)}%) [-{Res(StatType.DefenseRes)}%]");
            if (esm.GetStat(StatType.EffAtk) != 0) lines.Add($"Attack: {F(StatType.EffAtk)} (+{Pct(StatType.atkPct)}%)");
            if (esm.GetStat(StatType.EffInt) != 0) lines.Add($"Int: {F(StatType.EffInt)} (+{Pct(StatType.IntPct)}%)");
            if (esm.GetStat(StatType.EffectRes) != 0) lines.Add($"Effect Res: {F(StatType.EffectRes)}%");

            List<string> resTypes = new();
            if (esm.GetStat(StatType.damageRes) != 0f) resTypes.Add($"{F(StatType.damageRes)}%");
            if (esm.GetStat(StatType.physicalRes) != 0f) resTypes.Add($"P:{F(StatType.physicalRes)}%");
            if (esm.GetStat(StatType.spellRes) != 0f) resTypes.Add($"S:{F(StatType.spellRes)}%");

            if (resTypes.Count > 0) lines.Add($"Res: {string.Join(" ", resTypes)}");

            return ("Resources", string.Join("\n", lines), new(100, -100));
        }
        private void ShowTooltip()
        {
            if (resourceHoverZone != null && resourceHoverZone.TryGetComponent<ITooltipDisplay>(out var td))
            {
                var (tt, st, os) = GetResourcesTooltip();
                td.ShowTooltip(tt, st, os);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => ShowTooltip();
    }
}
