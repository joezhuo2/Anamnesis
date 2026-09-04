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
        private ICurrencyHolder ich;
        private ISkillPointHolder isph;
        public GameObject resourceHoverZone;

        public void Setup(IStatProvider isp, ICurrencyHolder ch, ISkillPointHolder sph)
        {
            esm = isp;
            ich = ch;
            isph = sph;
            ShowTooltip();
        }

        private (string title, string subtitle, Vector2 offset) GetResourcesTooltip()
        {
            if (esm == null) return ("Resources", "", new(100, -100));

            float staminaPerSecond = esm.GetStat(StatType.EffStReg) / 5f;
            float healthPerSecond = esm.GetStat(StatType.EffHpReg) / 5f;

            List<string> lines = new();
            if (staminaPerSecond != 0) lines.Add($"Stamina: {staminaPerSecond:F1}/s (+{esm.GetStat(StatType.stRegPct):F0}%)");
            if (healthPerSecond != 0) lines.Add($"Health: {healthPerSecond:F1}/s (+{esm.GetStat(StatType.hpRegPct):F0}%)");
            if (esm.GetStat(StatType.EffArmor) != 0) lines.Add($"Armor: {esm.GetStat(StatType.EffArmor):F0} (+{esm.GetStat(StatType.armorPct):F0}%) [-{esm.GetStat(StatType.ArmorRes)*100f:F1}%P]");
            if (esm.GetStat(StatType.EffAtk) != 0) lines.Add($"Attack: {esm.GetStat(StatType.EffAtk):F0} (+{esm.GetStat(StatType.atkPct):F0}%)");
            if (esm.GetStat(StatType.EffInt) != 0) lines.Add($"Int: {esm.GetStat(StatType.EffInt):F0} (+{esm.GetStat(StatType.IntPct):F0}%)");
            if (esm.GetStat(StatType.EffectRes) != 0) lines.Add($"Effect Res: {esm.GetStat(StatType.EffectRes):F0}%");

            List<string> resTypes = new();
            if (esm.GetStat(StatType.damageRes) != 0f) resTypes.Add($"{esm.GetStat(StatType.damageRes):F1}%");
            if (esm.GetStat(StatType.physicalRes) != 0f) resTypes.Add($"P:{esm.GetStat(StatType.physicalRes):F1}%");
            if (esm.GetStat(StatType.spellRes) != 0f) resTypes.Add($"S:{esm.GetStat(StatType.spellRes):F1}%");

            if (resTypes.Count > 0) lines.Add($"Res: {string.Join(" ", resTypes)}");
            if (ich != null && ich.CurrentAmount > 0) lines.Add($"Gold: {ich.CurrentAmount}");
            if (isph != null && isph.SkillPoints > 0) lines.Add($"Skill Points: {isph.SkillPoints}");

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
