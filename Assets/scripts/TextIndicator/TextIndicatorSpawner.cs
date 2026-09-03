using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.Core
{
    public class TextIndicatorSpawner : MonoBehaviour
    {
        public static TextIndicatorSpawner Instance;
        public TextIndicator prefab;
        public Canvas canvas;
        public int initialPoolSize = 100;

        private readonly List<TextIndicator> _activeIndicators = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializePool();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void InitializePool()
        {
            if (prefab == null || canvas == null)
            {
                Debug.LogError($"TextIndicatorSpawner on '{name}' needs both a prefab and a canvas assigned.", this);
                return;
            }

            PrefabPool.Prewarm(prefab.gameObject, canvas.transform, initialPoolSize, initialPoolSize);
        }

        public void SpawnTextIndicator(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay, TextType type)
        {
            if (delay <= 0f)
            {
                SpawnTextIndicator(damage, sourcePos, color, scale, lifetime, floatSpeed, type);
                return;
            }

            StartCoroutine(SpawnAfterDelay(damage, sourcePos, color, scale, lifetime, floatSpeed, delay, type));
        }

        private IEnumerator SpawnAfterDelay(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay, TextType type)
        {
            yield return new WaitForSeconds(delay);
            SpawnTextIndicator(damage, sourcePos, color, scale, lifetime, floatSpeed, type);
        }

        private void SpawnTextIndicator(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, TextType type)
        {
            if (prefab == null || canvas == null) return;

            TextIndicator indicator = PrefabPool.Acquire(prefab, canvas.transform);
            if (indicator == null) return;

            indicator.Initialize(damage, sourcePos, color, scale, lifetime, floatSpeed, type);
            _activeIndicators.Add(indicator);
        }

        public void ReturnToPool(TextIndicator indicator)
        {
            if (indicator == null) return;

            _activeIndicators.Remove(indicator);
            PrefabPool.Release(ref indicator);
        }
    }
}
