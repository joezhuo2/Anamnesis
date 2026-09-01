using CrystalFlux.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public static class CastBar
    {
        private static Camera mainCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => mainCamera = null;

        public static void Acquire(Slider barPrefab, TextMeshProUGUI textPrefab, out Slider bar, out TextMeshProUGUI text)
        {
            bar = null;
            text = null;

            Canvas canvas = EntityHealth.ResolveHealthBarCanvas();
            if (canvas == null) return;

            if (barPrefab != null)
            {
                bar = PrefabPool.Acquire(barPrefab, canvas.transform);
                if (bar != null) ResetBar(bar);
            }

            if (textPrefab != null)
            {
                text = PrefabPool.Acquire(textPrefab, canvas.transform);
                if (text != null)
                {
                    text.text = string.Empty;
                    text.transform.SetAsLastSibling();
                }
            }
        }

        public static void Release(ref Slider bar, ref TextMeshProUGUI text)
        {
            if (bar != null) ResetBar(bar);
            if (text != null) text.text = string.Empty;

            PrefabPool.Release(ref bar);
            PrefabPool.Release(ref text);
        }

        public static void Tick(Slider bar, TextMeshProUGUI text, Transform t, Vector3 offset, float elapsed, float total)
        {
            if (t == null || total <= 0f) return;

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(t.position + offset);
            bool visible = screenPos.z > 0f;
            screenPos.z = 0f;

            if (bar != null)
            {
                if (bar.gameObject.activeSelf != visible) bar.gameObject.SetActive(visible);
                if (visible)
                {
                    bar.maxValue = total;
                    bar.value = Mathf.Clamp(elapsed, 0f, total);
                    bar.transform.position = screenPos;
                }
            }

            if (text != null)
            {
                if (text.gameObject.activeSelf != visible) text.gameObject.SetActive(visible);
                if (visible)
                {
                    text.text = $"{Mathf.Max(0f, total - elapsed):F1}s";
                    text.transform.position = screenPos;
                }
            }
        }

        private static void ResetBar(Slider bar)
        {
            bar.minValue = 0f;
            bar.maxValue = 1f;
            bar.value = 0f;
            bar.transform.SetAsLastSibling();
        }
    }
}
