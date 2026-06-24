using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BossBarUI : MonoBehaviour
{
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI bossHPText;
    public Slider healthSlider;
    public Image fillImage;
    private int cMaxHp;
    private int cCurHp;

    private EntityStatManager bsm;

    public void Setup(string bossName, EntityStatManager esm)
    {
        bossNameText.text = bossName;
        bsm = esm;

        cMaxHp = bsm.s.EffMaxHp;
        cCurHp = bsm.s.currentHp;

        healthSlider.maxValue = cMaxHp;
        healthSlider.value = cMaxHp;

        if (fillImage != null && cMaxHp > 0)
            fillImage.fillAmount = (float)cCurHp / cMaxHp;

        bossHPText.text = $"{cCurHp}/{cMaxHp}";
    }

    private void Update()
    {
        if (bsm == null) return;

        int curHp = Mathf.Max(bsm.s.currentHp, 0);
        int maxHp = bsm.s.EffMaxHp;
        if (cCurHp == curHp && cMaxHp == maxHp) return;

        if (cCurHp != curHp) healthSlider.value = curHp;
        if (cMaxHp != maxHp) healthSlider.maxValue = maxHp;

        if (fillImage != null && cMaxHp > 0)
            fillImage.fillAmount = (float)curHp / maxHp;

        bossHPText.text = $"{curHp}/{maxHp}";

        cCurHp = curHp;
        cMaxHp = maxHp;
    }
}