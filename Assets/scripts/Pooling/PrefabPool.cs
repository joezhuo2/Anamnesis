using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalFlux.Core
{
    public static class PrefabPool
    {
        public const int DefaultCap = 64;

        private static readonly Dictionary<EntityId, Queue<GameObject>> pools = new();
        private static readonly Dictionary<EntityId, EntityId> origin = new();
        private static readonly Dictionary<EntityId, int> caps = new();
        private static readonly Dictionary<EntityId, IPoolable[]> hooks = new();
        private static readonly Dictionary<EntityId, Component> compCache = new();
        private static readonly List<IPoolable> hookBuffer = new();
        private static bool hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            pools.Clear();
            origin.Clear();
            caps.Clear();
            hooks.Clear();
            compCache.Clear();

            if (hooked) return;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            hooked = true;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            pools.Clear();
            origin.Clear();
            hooks.Clear();
            compCache.Clear();
        }

        public static GameObject Acquire(GameObject prefab, Transform parent)
        {
            if (prefab == null) return null;

            EntityId key = prefab.GetEntityId();
            Queue<GameObject> pool = Pool(key);

            while (pool.Count > 0)
            {
                GameObject pooled = pool.Dequeue();
                if (pooled == null) continue;

                if (parent != null && pooled.transform.parent != parent) pooled.transform.SetParent(parent, false);
                pooled.SetActive(true);
                if (parent != null) pooled.transform.SetAsLastSibling();
                origin[pooled.GetEntityId()] = key;
                InvokeHooks(pooled, true);
                return pooled;
            }

            GameObject created = Create(prefab, parent);
            if (parent != null) created.transform.SetAsLastSibling();
            origin[created.GetEntityId()] = key;
            InvokeHooks(created, true);
            return created;
        }

        public static T Acquire<T>(T prefab, Transform parent) where T : Component
        {
            if (prefab == null) return null;

            GameObject go = Acquire(prefab.gameObject, parent);
            if (go == null) return null;

            EntityId id = go.GetEntityId();
            if (compCache.TryGetValue(id, out var c) && c is T cached && cached != null) return cached;

            T t = go.GetComponent<T>();
            compCache[id] = t;
            return t;
        }

        public static void Release(ref GameObject instance)
        {
            if (instance != null) ReleaseInternal(instance);
            instance = null;
        }

        public static void Release<T>(ref T instance) where T : Component
        {
            if (instance != null) ReleaseInternal(instance.gameObject);
            instance = null;
        }

        public static void Prewarm(GameObject prefab, Transform parent, int count, int cap = 0)
        {
            if (prefab == null || count <= 0) return;

            EntityId key = prefab.GetEntityId();
            if (cap > 0) caps[key] = cap;

            Queue<GameObject> pool = Pool(key);
            int target = Mathf.Min(count, Cap(key));

            while (pool.Count < target)
            {
                GameObject created = Create(prefab, parent);
                created.SetActive(false);
                pool.Enqueue(created);
            }
        }

        public static void SetCap(GameObject prefab, int cap)
        {
            if (prefab == null || cap <= 0) return;
            caps[prefab.GetEntityId()] = cap;
        }

        public static int CountInactive(GameObject prefab)
            => prefab != null && pools.TryGetValue(prefab.GetEntityId(), out var pool) ? pool.Count : 0;

        private static void ReleaseInternal(GameObject go)
        {
            EntityId id = go.GetEntityId();
            InvokeHooks(go, false);

            if (!origin.TryGetValue(id, out EntityId key))
            {
                Forget(id);
                Object.Destroy(go);
                return;
            }

            origin.Remove(id);

            Queue<GameObject> pool = Pool(key);
            if (pool.Count >= Cap(key))
            {
                Forget(id);
                Object.Destroy(go);
                return;
            }

            go.SetActive(false);
            pool.Enqueue(go);
        }

        private static void Forget(EntityId id)
        {
            hooks.Remove(id);
            compCache.Remove(id);
        }

        private static IPoolable[] Hooks(GameObject go)
        {
            EntityId id = go.GetEntityId();
            if (hooks.TryGetValue(id, out var arr)) return arr;

            go.GetComponentsInChildren(true, hookBuffer);
            arr = hookBuffer.ToArray();
            hookBuffer.Clear();
            hooks[id] = arr;
            return arr;
        }

        private static void InvokeHooks(GameObject go, bool acquire)
        {
            IPoolable[] arr = Hooks(go);
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] is Object o && o == null) continue;
                if (acquire) arr[i].OnPoolAcquire();
                else arr[i].OnPoolRelease();
            }
        }

        private static GameObject Create(GameObject prefab, Transform parent)
        {
            GameObject go = parent != null ? Object.Instantiate(prefab, parent) : Object.Instantiate(prefab);
            Hooks(go);
            return go;
        }

        private static int Cap(EntityId key) => caps.TryGetValue(key, out int c) ? c : DefaultCap;

        private static Queue<GameObject> Pool(EntityId key)
        {
            if (!pools.TryGetValue(key, out var pool))
            {
                pool = new Queue<GameObject>();
                pools[key] = pool;
            }
            return pool;
        }
    }
}
