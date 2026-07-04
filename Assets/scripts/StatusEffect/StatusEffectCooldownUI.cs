using UnityEngine;
using UnityEngine.UI;

public class StatusEffectCooldownUI : MonoBehaviour
{
    public Image cooldownImage;
    public Image iconImage;

    private StatusEffect cse;
    private PlayerStats cps;
    private EntityStatManager cesm;

    public void Setup(StatusEffect se, PlayerStats ps, EntityStatManager esm)
    {
        cse = se;
        cps = ps;
        cesm = esm;

        if (cse != null && cse.icon != null && iconImage != null) iconImage.sprite = cse.icon;

        if (TryGetComponent<TooltipTrigger>(out var trigger)) trigger.SetupTooltipData(cse, cps, cesm);

        if (cooldownImage != null)
        {
            Color orig = cooldownImage.color;
            orig.a = 0.7f;
            cooldownImage.color = orig;

            cooldownImage.fillAmount = 1f;
        }
    }
    private void Update()
    {
        if (cse == null || !cesm.s.isAlive)
        {
            Destroy(gameObject);
            return;
        }

        float dura = cse.duration;

        if (dura <= 0f)
        {
            cooldownImage.fillAmount = 0f;
            return;
        }

        float timeElapsed = cse.currentTime;
        float cooldownRemainingPct = 1f - (timeElapsed / dura);

        cooldownImage.fillAmount = Mathf.Clamp01(cooldownRemainingPct);
    }
}