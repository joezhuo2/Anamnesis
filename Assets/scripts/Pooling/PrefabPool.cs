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
        private static readonly List<IPoolable> hookBuffer = new();
        private static bool hookBufferInUse;
        private static bool hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            pools.Clear();
            origin.Clear();
            caps.Clear();

            if (hooked) return;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            hooked = true;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            pools.Clear();
            origin.Clear();
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
            return go != null ? go.GetComponent<T>() : null;
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
                Object.Destroy(go);
                return;
            }

            origin.Remove(id);

            Queue<GameObject> pool = Pool(key);
            if (pool.Count >= Cap(key))
            {
                Object.Destroy(go);
                return;
            }

            go.SetActive(false);
            pool.Enqueue(go);
        }

        private static void InvokeHooks(GameObject go, bool acquire)
        {
            List<IPoolable> buffer = hookBufferInUse ? new List<IPoolable>() : hookBuffer;
            bool owned = !hookBufferInUse;
            hookBufferInUse = true;

            try
            {
                go.GetComponentsInChildren(true, buffer);
                for (int i = 0; i < buffer.Count; i++)
                {
                    if (acquire) buffer[i].OnPoolAcquire();
                    else buffer[i].OnPoolRelease();
                }
            }
            finally
            {
                buffer.Clear();
                if (owned) hookBufferInUse = false;
            }
        }

        private static GameObject Create(GameObject prefab, Transform parent)
            => parent != null ? Object.Instantiate(prefab, parent) : Object.Instantiate(prefab);

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
