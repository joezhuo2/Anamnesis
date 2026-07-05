using UnityEngine;
using UnityEngine.UI;

public class PlayerDashCooldownUI : MonoBehaviour
{
    public Image cooldownImage;
    private PlayerStats cps;
    private PlayerMovement cpm;

    public void Setup(PlayerMovement pm, PlayerStats ps)
    {
        cpm = pm;
        cps = ps;

        if (TryGetComponent<TooltipTrigger>(out var trigger)) trigger.SetupDashTooltipData(cps);

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
        float cd = cps.dashCooldown;

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