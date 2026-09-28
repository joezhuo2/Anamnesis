using UnityEngine;

namespace CrystalFlux.Core
{
    public class TextIndicatorSpawner : MonoBehaviour
    {
        public static TextIndicatorSpawner Instance;
        public TextIndicator prefab;
        public Canvas canvas;
        public int initialPoolSize = 100;

        private RectTransform root;

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

            PrefabPool.Prewarm(prefab.gameObject, Root(), initialPoolSize, initialPoolSize);
        }

        private Transform Root()
        {
            if (root != null) return root;
            if (canvas == null) return null;

            GameObject go = new("TextIndicators", typeof(RectTransform), typeof(Canvas));
            root = (RectTransform)go.transform;
            root.SetParent(canvas.transform, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            Canvas c = go.GetComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingLayerID = canvas.sortingLayerID;
            c.sortingOrder = canvas.sortingOrder + 1;

            return root;
        }

        public void SpawnTextIndicator(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay, TextType type)
        {
            if (prefab == null || canvas == null) return;

            TextIndicator indicator = PrefabPool.Acquire(prefab, Root());
            if (indicator == null) return;

            indicator.Initialize(damage, sourcePos, color, scale, lifetime, floatSpeed, type, delay);
        }

        public void SpawnTextIndicator(string content, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay = 0f)
        {
            if (prefab == null || canvas == null || string.IsNullOrEmpty(content)) return;

            TextIndicator indicator = PrefabPool.Acquire(prefab, Root());
            if (indicator == null) return;

            indicator.Initialize(content, sourcePos, color, scale, lifetime, floatSpeed, delay);
        }

        public void ReturnToPool(TextIndicator indicator)
        {
            if (indicator == null) return;

            PrefabPool.Release(ref indicator);
        }
    }
}
