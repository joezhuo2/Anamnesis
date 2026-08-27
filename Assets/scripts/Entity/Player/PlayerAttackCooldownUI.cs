using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackCooldownUI : MonoBehaviour
{
    public Image cooldownImage;
    public Image iconImage;

    private AttackType ctype;
    private AttackData cad;
    private PlayerAttackHandler cpah;
    private IStatProvider cesm;

    public void Setup(PlayerAttackHandler pah, AttackType type, IStatProvider esm)
    {
        cpah = pah;
        ctype = type;
        cesm = esm;

        cad = cpah.attacks.Find(a => a.type == ctype);

        if (cad != null && cad.icon != null && iconImage != null) iconImage.sprite = cad.icon;

        if (TryGetComponent<TooltipTrigger>(out var trigger)) trigger.SetupTooltipData(cad, cesm);

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

        if (effCd <= 0f)
        {
            cooldownImage.fillAmount = 0f;
            return;
        }

        float timeElapsed = Time.time - cpah.lastAttackTimes[ctype];
        float cooldownRemainingPct = 1f - (timeElapsed / effCd);

        cooldownImage.fillAmount = Mathf.Clamp01(cooldownRemainingPct);
    }
}