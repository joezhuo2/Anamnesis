using System.Collections;
using CrystalFlux.EntitySystem;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.WaveEventSystem
{
    // Owns the Blood Moon buff and screen tint. Runs its own lifetime so the buff is removed
    // in OnDestroy even if the event coroutine is cut short (scene reload, spawner disabled).
    public class BloodMoonEffect : MonoBehaviour
    {
        private Image tint;
        private Color color;
        private float alpha;
        private float targetAlpha;
        private float fadeInSpeed;
        private float fadeOutSpeed;
        private float bonusPct;
        private bool applied;

        public static BloodMoonEffect Create(BloodMoonEventData data, WaveEventContext ctx)
        {
            GameObject go = new($"Blood Moon ({data.name})", typeof(RectTransform), typeof(Canvas));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = data.tintSortingOrder;

            GameObject img = new("Tint", typeof(RectTransform), typeof(Image));
            RectTransform rt = (RectTransform)img.transform;
            rt.SetParent(go.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            BloodMoonEffect fx = go.AddComponent<BloodMoonEffect>();
            fx.tint = img.GetComponent<Image>();
            fx.tint.raycastTarget = false;
            fx.color = data.tintColor;
            fx.fadeInSpeed = data.fadeIn > 0f ? data.tintOpacity / data.fadeIn : -1f;
            fx.fadeOutSpeed = data.fadeOut > 0f ? data.tintOpacity / data.fadeOut : -1f;
            fx.ApplyAlpha();

            fx.StartCoroutine(fx.Run(ctx, data.duration, data.bonusPct, data.tintOpacity));
            return fx;
        }

        private IEnumerator Run(WaveEventContext ctx, float duration, float bonus, float opacity)
        {
            bonusPct = bonus;
            EnemyGlobalBuffs.Add(bonusPct, bonusPct, bonusPct);
            applied = true;
            targetAlpha = opacity;

            yield return ctx.Wait(duration);

            RemoveBuff();
            targetAlpha = 0f;
            while (alpha > 0f) yield return null;

            Destroy(gameObject);
        }

        private void Update()
        {
            if (alpha == targetAlpha) return;

            // Negative speed = no fade, snap straight to the target.
            float speed = targetAlpha > alpha ? fadeInSpeed : fadeOutSpeed;
            alpha = speed < 0f ? targetAlpha : Mathf.MoveTowards(alpha, targetAlpha, speed * Time.deltaTime);
            ApplyAlpha();
        }

        private void ApplyAlpha() => tint.color = new Color(color.r, color.g, color.b, alpha);

        private void RemoveBuff()
        {
            if (!applied) return;

            applied = false;
            EnemyGlobalBuffs.Add(bonusPct, bonusPct, bonusPct, false);
        }

        private void OnDestroy() => RemoveBuff();
    }
}
