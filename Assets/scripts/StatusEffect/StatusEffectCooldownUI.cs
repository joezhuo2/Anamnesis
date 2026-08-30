using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrystalFlux.StatusEffectSystem
{
    public class StatusEffectCooldownUI : MonoBehaviour, IPointerEnterHandler
    {
        public Image cooldownImage;
        public Image iconImage;

        private StatusEffect cse;
        private IStatProvider cesm;

        public void Setup(StatusEffect se, IStatProvider esm)
        {
            cse = se;
            cesm = esm;

            if (cse != null && cse.icon != null && iconImage != null) iconImage.sprite = cse.icon;

            if (cooldownImage != null)
            {
                Color orig = cooldownImage.color;
                orig.a = 0.7f;
                cooldownImage.color = orig;

                cooldownImage.fillAmount = 1f;
            }

            ShowTooltip();
        }

        private void ShowTooltip()
        {
            if (TryGetComponent<ITooltipDisplay>(out var td))
            {
                var (tt, st, os) = GetStatusEffectTooltip();
                td.ShowTooltip(tt, st, os);
            }
        }

        private void Update()
        {
            if (cse == null || (cesm != null && cesm.GetStat(StatType.isAlive) <= 0f))
            {
                Destroy(gameObject);
                return;
            }

            float effRes = cesm != null ? cesm.GetStat(StatType.EffectRes) : 0f;
            float effDur = cse.isBuff ? cse.duration : cse.duration * (1f - (effRes * 0.01f));

            if (cooldownImage == null) return;

            if (effDur <= 0f)
            {
                cooldownImage.fillAmount = 0f;
                return;
            }

            float timeElapsed = cse.currentTime;
            float cooldownRemainingPct = 1f - (timeElapsed / effDur);

            cooldownImage.fillAmount = Mathf.Clamp01(cooldownRemainingPct);
        }

        public (string title, string subtitle, Vector2 offset) GetStatusEffectTooltip()
        {
            List<string> lines = new();
            if (!string.IsNullOrEmpty(cse.desc)) lines.Add(cse.desc);

            string name = cse.effName + ((cse.maxStacks > 1 && cse.currentStacks > 1) ? $"[{cse.currentStacks}]" : "");

            return(name, string.Join("\n", lines), new(100, -100));
        }

        public void OnPointerEnter(PointerEventData eventData) => ShowTooltip();
    }
}
