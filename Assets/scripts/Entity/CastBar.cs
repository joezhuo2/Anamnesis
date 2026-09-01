using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public static class CastBar
    {
        private static readonly Dictionary<EntityId, Queue<Slider>> barPools = new();
        private static readonly Dictionary<EntityId, Queue<TextMeshProUGUI>> textPools = new();
        private static readonly Dictionary<EntityId, EntityId> barOrigin = new();
        private static readonly Dictionary<EntityId, EntityId> textOrigin = new();
        private static Camera mainCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            barPools.Clear();
            textPools.Clear();
            barOrigin.Clear();
            textOrigin.Clear();
            mainCamera = null;
        }

        public static void Acquire(Slider barPrefab, TextMeshProUGUI textPrefab, out Slider bar, out TextMeshProUGUI text)
        {
            bar = null;
            text = null;

            Canvas canvas = EntityHealth.ResolveHealthBarCanvas();
            if (canvas == null) return;

            if (barPrefab != null)
            {
                EntityId key = barPrefab.GetEntityId();
                bar = Rent(barPools, key, barPrefab, canvas);
                if (bar != null)
                {
                    barOrigin[bar.GetEntityId()] = key;
                    ResetBar(bar);
                }
            }

            if (textPrefab != null)
            {
                EntityId key = textPrefab.GetEntityId();
                text = Rent(textPools, key, textPrefab, canvas);
                if (text != null)
                {
                    textOrigin[text.GetEntityId()] = key;
                    text.text = string.Empty;
                    text.transform.SetAsLastSibling();
                }
            }
        }

        public static void Release(ref Slider bar, ref TextMeshProUGUI text)
        {
            if (bar != null)
            {
                EntityId id = bar.GetEntityId();
                if (barOrigin.TryGetValue(id, out EntityId key))
                {
                    barOrigin.Remove(id);
                    ResetBar(bar);
                    bar.gameObject.SetActive(false);
                    Pool(barPools, key).Enqueue(bar);
                }
                else Object.Destroy(bar.gameObject);
            }

            if (text != null)
            {
                EntityId id = text.GetEntityId();
                if (textOrigin.TryGetValue(id, out EntityId key))
                {
                    textOrigin.Remove(id);
                    text.text = string.Empty;
                    text.gameObject.SetActive(false);
                    Pool(textPools, key).Enqueue(text);
                }
                else Object.Destroy(text.gameObject);
            }

            bar = null;
            text = null;
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

        private static T Rent<T>(Dictionary<EntityId, Queue<T>> pools, EntityId key, T prefab, Canvas canvas) where T : Component
        {
            Queue<T> pool = Pool(pools, key);

            while (pool.Count > 0)
            {
                T pooled = pool.Dequeue();
                if (pooled == null) continue;

                if (pooled.transform.parent != canvas.transform) pooled.transform.SetParent(canvas.transform, false);
                pooled.gameObject.SetActive(true);
                pooled.transform.SetAsLastSibling();
                return pooled;
            }

            T created = Object.Instantiate(prefab, canvas.transform);
            created.transform.SetAsLastSibling();
            return created;
        }

        private static Queue<T> Pool<T>(Dictionary<EntityId, Queue<T>> pools, EntityId key)
        {
            if (!pools.TryGetValue(key, out var pool))
            {
                pool = new Queue<T>();
                pools[key] = pool;
            }
            return pool;
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
