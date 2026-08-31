using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.UISystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public class PlayerAttackCooldownUI : MonoBehaviour, IPointerEnterHandler
    {
        public Image cooldownImage;
        public Image iconImage;

        private AttackType ctype;
        private AttackData cad;
        private PlayerAttackHandler cpah;
        private IStatProvider cesm;

        private ITooltipDisplay tooltipDisplay;
        private string cachedTitle;
        private string cachedSubtitle;
        private Vector2 cachedOffset;

        private static readonly Vector2 TooltipOffset = new(0, -100);

        public void Setup(PlayerAttackHandler pah, AttackType type, IStatProvider esm)
        {
            cpah = pah;
            ctype = type;
            cesm = esm;

            cad = cpah.attacks.Find(a => a.type == ctype);

            if (cad != null && cad.icon != null && iconImage != null) iconImage.sprite = cad.icon;

            tooltipDisplay = GetComponent<ITooltipDisplay>();
            cachedTitle = null;
            cachedSubtitle = null;

            RefreshTooltip();

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
            if (cpah == null || cad == null || !cpah.lastAttackTimes.ContainsKey(ctype)) return;

            float effCd = PlayerAttackHandler.GetEffCd(cad, cesm);

            if (cooldownImage == null) return;

            if (effCd <= 0f)
            {
                cooldownImage.fillAmount = 0f;
            }
            else
            {
                float timeElapsed = Time.time - cpah.lastAttackTimes[ctype];
                float cooldownRemainingPct = 1f - (timeElapsed / effCd);

                cooldownImage.fillAmount = Mathf.Clamp01(cooldownRemainingPct);
            }
        }

        private void RefreshTooltip()
        {
            if (tooltipDisplay == null || cad == null) return;

            var (tt, st, os) = GetAttackTooltip();

            if (tt == cachedTitle && st == cachedSubtitle && os == cachedOffset) return;

            cachedTitle = tt;
            cachedSubtitle = st;
            cachedOffset = os;
            tooltipDisplay.ShowTooltip(tt, st, os);
        }

        private (string title, string subtitle, Vector2 offset) GetAttackTooltip()
        {
            if (cad == null || cesm == null) return ("", "", Vector2.zero);

            GameObject owner = cesm is Component c ? c.gameObject : null;
            if (owner == null) return ("", "", Vector2.zero);

            var (hp, sp, mp) = PlayerAttackHandler.GetCosts(cad, cesm);
            var (hpg, spg, mpg) = Projectile.CalculateStatGains(owner, cad);
            var effCd = PlayerAttackHandler.GetEffCd(cad, cesm);

            float basePhysDmg = 0f, baseSplDmg = 0f, trueDmg = 0f;
            if (cad.pd != null)
            {
                var previewSnapshot = ProjectileSnapshot.CaptureSnapshot(cad.pd, owner);
                var previewPacket = DamagePacketBuilder.BuildDamagePacket(cad.pd, previewSnapshot, false, owner, false, 1f);

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

            if (dmgTypes.Count > 0) lines.Add($"Base: {string.Join(" ", dmgTypes)}");

            return (cad.displayName, string.Join("\n", lines), TooltipOffset);
        }

        public void OnPointerEnter(PointerEventData eventData) => RefreshTooltip();
    }
}
