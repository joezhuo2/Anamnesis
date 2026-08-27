using UnityEngine;
using UnityEngine.UI;

public class PlayerDashCooldownUI : MonoBehaviour
{
    public Image cooldownImage;
    private PlayerMovement cpm;
    private IStatProvider cesm;

    public void Setup(PlayerMovement pm, IStatProvider esm)
    {
        cpm = pm;
        cesm = esm;

        if (TryGetComponent<TooltipTrigger>(out var trigger)) trigger.SetupDashTooltipData(cesm);

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
}