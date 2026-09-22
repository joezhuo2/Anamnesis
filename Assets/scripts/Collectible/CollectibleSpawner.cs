using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.CollectibleSystem
{
    public class CollectibleSpawner : MonoBehaviour
    {
        public static CollectibleSpawner Active { get; private set; }

        [Header("Setup")]
        public Collectible prefab;
        public List<CollectibleData> collectibles = new();
        public int prewarmCount = 8;

        [Header("Placement")]
        public float minSpawnDist = 3f;
        public float maxSpawnDist = 8f;

        [Header("Limits")]
        public int maxConcurrent = 5;
        public float tickInterval = 1f;

        private readonly List<Collectible> live = new();
        private readonly Dictionary<CollectibleData, float> cooldowns = new();
        private readonly List<CollectibleData> rollBuffer = new();
        private float tickTimer;
        private GameObject player;

        private void Awake()
        {
            if (Active != null && Active != this)
            {
                Destroy(gameObject);
                return;
            }

            Active = this;
        }

        private void Start()
        {
            if (prefab != null && prewarmCount > 0)
                PrefabPool.Prewarm(prefab.gameObject, null, prewarmCount, Mathf.Max(prewarmCount, maxConcurrent));
        }

        private void OnDisable()
        {
            ReleaseAll();
            tickTimer = 0f;
            cooldowns.Clear();
        }

        private void OnDestroy()
        {
            if (Active == this) Active = null;
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            float dt = Time.deltaTime;
            TickCooldowns(dt);

            if (!WaveManager.WaveActive) return;

            tickTimer += dt;
            if (tickTimer < tickInterval) return;
            tickTimer -= tickInterval;

            PruneLive();
            if (prefab == null || live.Count >= maxConcurrent) return;

            TrySpawnOne();
        }

        public void ReleaseCollectible(Collectible c)
        {
            if (c == null) return;

            live.Remove(c);
            PrefabPool.Release(ref c);
        }

        public void ReleaseAll()
        {
            for (int i = live.Count - 1; i >= 0; i--)
            {
                Collectible c = live[i];
                if (c != null) PrefabPool.Release(ref c);
            }

            live.Clear();
        }

        private void TickCooldowns(float dt)
        {
            if (cooldowns.Count == 0) return;

            rollBuffer.Clear();
            foreach (var kv in cooldowns) rollBuffer.Add(kv.Key);

            for (int i = 0; i < rollBuffer.Count; i++)
            {
                CollectibleData d = rollBuffer[i];
                float remaining = cooldowns[d] - dt;

                if (remaining <= 0f) cooldowns.Remove(d);
                else cooldowns[d] = remaining;
            }

            rollBuffer.Clear();
        }

        private void TrySpawnOne()
        {
            GameObject p = Player();
            if (p == null) return;

            rollBuffer.Clear();

            for (int i = 0; i < collectibles.Count; i++)
            {
                CollectibleData d = collectibles[i];

                if (d == null || d.chance <= 0f) continue;
                if (cooldowns.ContainsKey(d)) continue;
                if (d.type == CollectibleType.Rerolls && IronmanSelector.Enabled) continue;

                rollBuffer.Add(d);
            }

            Shuffle(rollBuffer);

            for (int i = 0; i < rollBuffer.Count; i++)
            {
                CollectibleData d = rollBuffer[i];
                if (Random.value * 100f >= d.chance) continue;

                Spawn(d, p.transform.position);
                if (d.cooldown > 0f) cooldowns[d] = d.cooldown;
                break;
            }

            rollBuffer.Clear();
        }

        private void Spawn(CollectibleData d, Vector2 center)
        {
            Collectible c = PrefabPool.Acquire(prefab, null);
            if (c == null) return;

            float min = Mathf.Min(minSpawnDist, maxSpawnDist);
            float max = Mathf.Max(minSpawnDist, maxSpawnDist);
            float dist = Random.Range(min, max);
            float ang = Random.Range(0f, Mathf.PI * 2f);
            Vector2 pos = center + (new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * dist);

            c.transform.SetPositionAndRotation(pos, Quaternion.identity);
            c.Setup(d, d.RollValue());
            live.Add(c);
        }

        private void PruneLive()
        {
            for (int i = live.Count - 1; i >= 0; i--)
            {
                Collectible c = live[i];
                if (c == null || !c.gameObject.activeSelf) live.RemoveAt(i);
            }
        }

        private GameObject Player()
        {
            if (player == null) player = GameObject.FindWithTag("Player");
            return player;
        }

        private static void Shuffle(List<CollectibleData> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
