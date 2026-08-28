using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public class PlayerDashCooldownUI : MonoBehaviour
    {
        public Image cooldownImage;
        private PlayerMovement cpm;
        private IStatProvider cesm;

        public void Setup(PlayerMovement pm, IStatProvider esm)
        {
            cpm = pm;
            cesm = esm;

            if (TryGetComponent<ITooltipDisplay>(out var td))
            {
                var (tt, st, os) = GetDashTooltip();
                td.ShowTooltip(tt, st, os);
            }

                if (cooldownImage != null)
            {
                Color orig = cooldownImage.color;
                orig.a = 0.9f;
                cooldownImage.color = orig;

                cooldownImage.fillAmount = 0f;
            }
        }
        private void Update()
        {
            if (cesm == null || cpm == null || cooldownImage == null) return;
            float cd = cesm.GetStat(StatType.EffDashCooldown);

            if (cd <= 0f)
            {
                cooldownImage.fillAmount = 0f;
                return;
            }

            float timeElapsed = Time.time - cpm.lastDashTime;
            float cooldownRemainingPct = 1f - (timeElapsed / cd);

            cooldownImage.fillAmount = Mathf.Clamp01(cooldownRemainingPct);
        }

        private (string title, string subtitle, Vector2 offset) GetDashTooltip()
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

            return("Movement", string.Join("\n", lines), new(100, 30));
        }
    }
}
