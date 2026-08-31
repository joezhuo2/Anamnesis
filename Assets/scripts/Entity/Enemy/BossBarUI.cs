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
        private int cMaxHp;
        private int cCurHp;

        private IStatProvider bsm;

        public void Setup(string bossName, IStatProvider esm)
        {
            if (bossNameText != null) bossNameText.text = bossName;
            bsm = esm;

            if (bsm == null)
            {
                Debug.LogError($"BossBarUI '{name}' was set up without a stat provider.", this);
                return;
            }

            cMaxHp = Mathf.RoundToInt(bsm.GetStat(StatType.EffMaxHp));
            cCurHp = Mathf.RoundToInt(bsm.GetStat(StatType.currentHp));

            if (healthSlider != null)
            {
                healthSlider.maxValue = cMaxHp;
                healthSlider.value = cCurHp;
            }

            if (fillImage != null && cMaxHp > 0)
                fillImage.fillAmount = (float)cCurHp / cMaxHp;

            if (bossHPText != null) bossHPText.text = $"{cCurHp}/{cMaxHp}";
        }

        private void Update()
        {
            if (bsm == null) return;

            int curHp = Mathf.Max(Mathf.RoundToInt(bsm.GetStat(StatType.currentHp)), 0);
            int maxHp = Mathf.RoundToInt(bsm.GetStat(StatType.EffMaxHp));
            if (cCurHp == curHp && cMaxHp == maxHp) return;

            if (healthSlider != null)
            {
                if (cMaxHp != maxHp) healthSlider.maxValue = maxHp;
                if (cCurHp != curHp) healthSlider.value = curHp;
            }

            if (fillImage != null && maxHp > 0)
                fillImage.fillAmount = (float)curHp / maxHp;

            if (bossHPText != null) bossHPText.text = $"{curHp}/{maxHp}";

            cCurHp = curHp;
            cMaxHp = maxHp;
        }
    }
}
