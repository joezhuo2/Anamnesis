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
        private int cseGen;
        private IStatProvider cesm;
        private const float PollInterval = 0.1f;
        private float nextPollTime;
        private float cachedEffDur;

        public void Setup(StatusEffect se, IStatProvider esm)
        {
            cse = se;
            cseGen = se != null ? se.Generation : 0;
            cesm = esm;
            nextPollTime = 0f;
            cachedEffDur = 0f;

            if (iconImage != null) iconImage.sprite = cse != null ? cse.icon : null;

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
            if (cse == null || cse.Released || cse.Generation != cseGen)
            {
                ReturnToPool();
                return;
            }

            if (Time.unscaledTime >= nextPollTime)
            {
                nextPollTime = Time.unscaledTime + PollInterval;

                if (cesm != null && cesm.GetStat(StatType.isAlive) <= 0f)
                {
                    ReturnToPool();
                    return;
                }

                float effRes = cesm != null ? cesm.GetStat(StatType.EffectRes) : 0f;
                cachedEffDur = cse.isBuff ? cse.duration : cse.duration * (1f - (effRes * 0.01f));
            }

            if (cooldownImage == null) return;

            float fill = cachedEffDur <= 0f ? 0f : Mathf.Clamp01(1f - (cse.currentTime / cachedEffDur));
            if (cooldownImage.fillAmount != fill) cooldownImage.fillAmount = fill;
        }

        private void ReturnToPool()
        {
            cse = null;
            cesm = null;
            if (cooldownImage != null) cooldownImage.fillAmount = 0f;
            if (iconImage != null) iconImage.sprite = null;

            GameObject go = gameObject;
            PrefabPool.Release(ref go);
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
