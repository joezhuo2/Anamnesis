using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackCooldownUI : MonoBehaviour
{
    public Image cooldownImage;
    public Image iconImage;

    private AttackType ctype;
    private AttackData cad;
    private PlayerAttackHandler cpah;
    private PlayerStats cps;
    private EntityStatManager cesm;

    public void Setup(PlayerAttackHandler pah, PlayerStats ps, AttackType type, EntityStatManager esm)
    {
        cpah = pah;
        cps = ps;
        ctype = type;
        cesm = esm;

        cad = cpah.attacks.Find(a => a.type == ctype);

        if (cad != null && cad.icon != null && iconImage != null) iconImage.sprite = cad.icon;

        if (TryGetComponent<TooltipTrigger>(out var trigger)) trigger.SetupTooltipData(cad, cps, cesm);

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

        float effCd = cad.cooldown * Mathf.Clamp(1f - (cps.attackSpeedPct * 0.01f), 0.1f, 10f);

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