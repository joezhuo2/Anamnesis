using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public class PlayerAttackCooldownUI : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Image cooldownImage;
        public Image iconImage;
        public Image borderImage;

        [Header("Stacks")]
        [Tooltip("Border color for attacks that can store more than one stack")]
        public Color stackedBorderColor = new(0.3f, 0.85f, 0.35f, 1f);
        [Tooltip("Optional. Shows the stored stack count on attacks with more than one stack")]
        public TextMeshProUGUI stackText;
        [Tooltip("Border color while more than zero but fewer than max stacks are stored")]
        public Color partialStackBorderColor = new(0.95f, 0.85f, 0.2f, 1f);

        [Header("Blocked Feedback")]
        public Color blockedBorderColor = new(0.85f, 0.15f, 0.15f, 1f);
        public Color flashBorderColor = new(1f, 0.45f, 0.45f, 1f);
        public int flashCount = 3;
        public float flashInterval = 0.06f;

        [Header("Ready Feedback")]
        public Color readyFlashColor = new(0.3f, 1f, 0.35f, 1f);
        public int readyFlashCount = 2;
        public float readyFlashInterval = 0.08f;
        [Tooltip("Attacks with a shorter effective cooldown than this do not flash when ready")]
        public float minReadyFlashCooldown = 0.5f;

        [Header("Free Cast")]
        public Color freeCastBorderColor = new(0.35f, 0.85f, 1f, 1f);

        [Header("Sealed")]
        public Color lockedIconColor = new(0.35f, 0.35f, 0.35f, 1f);
        public Color lockedBorderColor = new(0.4f, 0.4f, 0.4f, 1f);
        [Tooltip("Optional. Shown while the attack slot is sealed")]
        public GameObject lockOverlay;

        private AttackType ctype;
        private AttackData cad;
        private PlayerAttackHandler cpah;
        private IStatProvider cesm;

        private ITooltipDisplay tooltipDisplay;
        private string cachedTitle;
        private string cachedSubtitle;
        private Vector2 cachedOffset;

        private static readonly Vector2 TooltipOffset = new(0, -100);
        private const float PollInterval = 0.1f;
        private float nextPollTime;
        private float cachedEffCd;

        private Color normalBorderColor = Color.white;
        private bool borderColorCached;
        private Color normalIconColor = Color.white;
        private bool iconColorCached;
        private bool lastLocked;
        private bool pressed;
        private Coroutine flashRoutine;
        private int lastStacks = -1;

        private bool IsStacked => cad != null && cad.Stacks > 1;
        private Color IdleBorderColor => IsStacked ? stackedBorderColor : normalBorderColor;

        public void Setup(PlayerAttackHandler pah, AttackType type, IStatProvider esm)
        {
            ReleasePress();

            cpah = pah;
            ctype = type;
            cesm = esm;

            cad = cpah.attacks.Find(a => a.type == ctype);

            if (cad != null && cad.Icon != null && iconImage != null) iconImage.sprite = cad.Icon;

            if (iconImage != null && !iconColorCached)
            {
                normalIconColor = iconImage.color;
                iconColorCached = true;
            }

            tooltipDisplay = GetComponent<ITooltipDisplay>();
            cachedTitle = null;
            cachedSubtitle = null;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            if (borderImage != null)
            {
                if (!borderColorCached)
                {
                    normalBorderColor = borderImage.color;
                    borderColorCached = true;
                }
                borderImage.color = IdleBorderColor;
            }

            nextPollTime = 0f;
            cachedEffCd = 0f;
            lastStacks = -1;
            if (stackText != null) stackText.gameObject.SetActive(IsStacked);

            ApplyLocked(cpah.IsSlotLocked(ctype));

            RefreshTooltip();

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
            if (cpah == null || cad == null) return;

            bool due = Time.unscaledTime >= nextPollTime;
            if (!due && cachedEffCd > 0f && cpah.lastAttackTimes.TryGetValue(ctype, out float st) && Time.time - st >= cachedEffCd) due = true;
            if (due) Poll();

            if (cooldownImage == null) return;

            if (lastLocked)
            {
                if (cooldownImage.fillAmount != 1f) cooldownImage.fillAmount = 1f;
                return;
            }

            if (cachedEffCd <= 0f || !cpah.lastAttackTimes.TryGetValue(ctype, out float lat))
            {
                if (cooldownImage.fillAmount != 0f) cooldownImage.fillAmount = 0f;
                return;
            }

            float fill = Mathf.Clamp01(1f - ((Time.time - lat) / cachedEffCd));
            if (cooldownImage.fillAmount != fill) cooldownImage.fillAmount = fill;
        }

        private void Poll()
        {
            nextPollTime = Time.unscaledTime + PollInterval;
            cachedEffCd = PlayerAttackHandler.GetEffCd(cad, cesm);

            bool locked = cpah.IsSlotLocked(ctype);
            if (locked != lastLocked) ApplyLocked(locked);

            int stacks = cpah.GetStacks(ctype);
            bool gained = lastStacks >= 0 && stacks > lastStacks;

            if (stacks != lastStacks)
            {
                lastStacks = stacks;
                if (stackText != null && IsStacked) stackText.SetText("{0}", stacks);
            }

            if (gained && !locked && cachedEffCd >= minReadyFlashCooldown) FlashReady();
            else UpdateBorder();
        }

        private void ApplyLocked(bool locked)
        {
            lastLocked = locked;

            if (iconImage != null && iconColorCached) iconImage.color = locked ? lockedIconColor : normalIconColor;
            if (lockOverlay != null) lockOverlay.SetActive(locked);

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            UpdateBorder();
        }

        private Color ResolveBorderColor()
        {
            if (lastLocked) return lockedBorderColor;
            if (cpah != null && cpah.IsFreeCast(ctype)) return freeCastBorderColor;
            if (cpah == null || !cpah.CanCast(ctype)) return blockedBorderColor;
            if (IsStacked && lastStacks > 0 && lastStacks < cad.Stacks) return partialStackBorderColor;
            return IdleBorderColor;
        }

        private void UpdateBorder()
        {
            if (borderImage == null || flashRoutine != null) return;

            Color c = ResolveBorderColor();
            if (borderImage.color != c) borderImage.color = c;
        }

        public void FlashBlocked()
        {
            if (borderImage == null || !isActiveAndEnabled) return;
            StartFlash(flashBorderColor, blockedBorderColor, flashCount, flashInterval);
        }

        private void FlashReady()
        {
            if (borderImage == null || !isActiveAndEnabled) return;
            StartFlash(readyFlashColor, ResolveBorderColor(), readyFlashCount, readyFlashInterval);
        }

        private void StartFlash(Color on, Color off, int count, float interval)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(on, off, count, interval));
        }

        private IEnumerator FlashRoutine(Color on, Color off, int count, float interval)
        {
            int n = Mathf.Max(1, count);
            float iv = Mathf.Max(0.01f, interval);

            for (int i = 0; i < n; i++)
            {
                borderImage.color = on;
                yield return new WaitForSecondsRealtime(iv);
                borderImage.color = off;
                yield return new WaitForSecondsRealtime(iv);
            }

            flashRoutine = null;
            borderImage.color = ResolveBorderColor();
        }

        private void OnDisable()
        {
            ReleasePress();

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            if (borderImage != null && borderColorCached) borderImage.color = lastLocked ? lockedBorderColor : IdleBorderColor;
            nextPollTime = 0f;
        }

        private void RefreshTooltip()
        {
            if (tooltipDisplay == null || cad == null) return;

            var (tt, st, os) = GetAttackTooltip();

            if (tt == cachedTitle && st == cachedSubtitle && os == cachedOffset) return;

            cachedTitle = tt;
            cachedSubtitle = st;
            cachedOffset = os;
            tooltipDisplay.ShowTooltip(tt, st, os);
        }

        private (string title, string subtitle, Vector2 offset) GetAttackTooltip()
        {
            if (cad == null || cesm == null) return ("", "", Vector2.zero);

            GameObject owner = cesm is Component c ? c.gameObject : null;
            if (owner == null) return ("", "", Vector2.zero);

            var (hp, sp, mp) = PlayerAttackHandler.GetCosts(cad, cesm);
            var (hpg, spg, mpg) = Projectile.CalculateStatGains(owner, cad);
            var effCd = PlayerAttackHandler.GetEffCd(cad, cesm);

            float basePhysDmg = 0f, baseSplDmg = 0f, trueDmg = 0f;
            if (cad.Pd != null)
            {
                var previewSnapshot = ProjectileSnapshot.CaptureSnapshot(cad.Pd, owner);
                var previewPacket = DamagePacketBuilder.BuildDamagePacket(cad.Pd, previewSnapshot, false, owner, false, 1f);

                foreach (var instance in previewPacket.instances)
                {
                    switch (instance.type)
                    {
                        case DamageType.Physical: basePhysDmg += instance.amount; break;
                        case DamageType.Spell: baseSplDmg += instance.amount; break;
                        case DamageType.True: trueDmg += instance.amount; break;
                        default: break;
                    }
                }

                DamagePacket.Release(previewPacket);
            }

            List<string> lines = new() { $"{cad.type}" };
            if (cpah.IsSlotLocked(ctype)) lines.Add("Sealed");
            else if (cpah.IsFreeCast(ctype)) lines.Add("Resonance: next cast free");
            if (effCd != 0f) lines.Add($"Cooldown: {effCd:F1}s");
            if (cad.Stacks > 1) lines.Add($"Stacks: {cpah.GetStacks(ctype)}/{cad.Stacks}");
            if (hp != 0f || hpg != 0f) lines.Add($"Health: -{hp:F0} +{hpg:F0} +{cad.HealthPctGainOnHit:F1}%");
            if (sp != 0f || spg != 0f) lines.Add($"Stamina: -{sp:F0} +{spg:F0} +{cad.StaminaPctGainOnHit:F1}%");
            if (mp != 0f || mpg != 0f) lines.Add($"Mana: -{mp:F0} +{mpg:F0} +{cad.ManaPctGainOnHit:F1}%");
            if (cesm.GetStat(StatType.critChance) != 0f || cesm.GetStat(StatType.critDamage) != 0f)
                lines.Add($"Crit: {cesm.GetStat(StatType.critChance):F1}% +{cesm.GetStat(StatType.critDamage):F1}%");
            if (cesm.GetStat(StatType.defShred) != 0f || cesm.GetStat(StatType.resPen) != 0f)
                lines.Add($"Shred: {cesm.GetStat(StatType.defShred):F0}A {cesm.GetStat(StatType.resPen):F0}R");

            List<string> dmgTypes = new();
            if (basePhysDmg != 0f) dmgTypes.Add($"{basePhysDmg:F0}P");
            if (baseSplDmg != 0f) dmgTypes.Add($"{baseSplDmg:F0}S");
            if (trueDmg != 0f) dmgTypes.Add($"{trueDmg:F0}T");

            if (dmgTypes.Count > 0) lines.Add($"Base: {string.Join(" ", dmgTypes)}");

            return (cad.DisplayName, string.Join("\n", lines), TooltipOffset);
        }

        public void OnPointerEnter(PointerEventData eventData) => RefreshTooltip();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || cpah == null || pressed) return;

            pressed = true;
            cpah.PressAttack(ctype);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            ReleasePress();
        }

        private void ReleasePress()
        {
            if (!pressed) return;

            pressed = false;
            if (cpah != null) cpah.ReleaseAttack(ctype);
        }
    }
}
