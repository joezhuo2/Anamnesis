using System;
using CrystalFlux.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public class BossBarUI : MonoBehaviour, IBossBar
    {
        public TextMeshProUGUI bossNameText;
        public TextMeshProUGUI bossHPText;
        public Slider healthSlider;
        public Image fillImage;
        private float cMaxHp;
        private float cCurHp;

        private IStatProvider bsm;
        private Func<float> curSrc;
        private Func<float> maxSrc;

        public void Setup(string bossName, IStatProvider esm)
        {
            if (bossNameText != null) bossNameText.text = bossName;
            bsm = esm;
            curSrc = null;
            maxSrc = null;

            if (bsm == null)
            {
                Debug.LogError($"BossBarUI '{name}' was set up without a stat provider.", this);
                return;
            }

            cMaxHp = Mathf.Round(bsm.GetStat(StatType.EffMaxHp));
            cCurHp = Mathf.Round(bsm.GetStat(StatType.currentHp));

            if (healthSlider != null)
            {
                healthSlider.maxValue = cMaxHp;
                healthSlider.value = cCurHp;
            }

            if (fillImage != null && cMaxHp > 0)
                fillImage.fillAmount = cCurHp / cMaxHp;

            if (bossHPText != null) bossHPText.text = NumberFormat.Bar(cCurHp, cMaxHp);
        }

        public void SetupPool(string barName, Func<float> cur, Func<float> max)
        {
            if (bossNameText != null) bossNameText.text = barName;
            bsm = null;
            curSrc = cur;
            maxSrc = max;
            cCurHp = float.MinValue;
            cMaxHp = float.MinValue;
        }

        private void Update()
        {
            float curHp;
            float maxHp;

            if (curSrc != null && maxSrc != null)
            {
                curHp = Mathf.Max(curSrc(), 0f);
                maxHp = maxSrc();
            }
            else if (bsm != null)
            {
                curHp = Mathf.Max(Mathf.Round(bsm.GetStat(StatType.currentHp)), 0f);
                maxHp = Mathf.Round(bsm.GetStat(StatType.EffMaxHp));
            }
            else return;

            if (cCurHp == curHp && cMaxHp == maxHp) return;

            if (healthSlider != null)
            {
                if (cMaxHp != maxHp) healthSlider.maxValue = maxHp;
                if (cCurHp != curHp) healthSlider.value = curHp;
            }

            if (fillImage != null && maxHp > 0)
                fillImage.fillAmount = curHp / maxHp;

            if (bossHPText != null) bossHPText.text = NumberFormat.Bar(curHp, maxHp);

            cCurHp = curHp;
            cMaxHp = maxHp;
        }
    }
}
