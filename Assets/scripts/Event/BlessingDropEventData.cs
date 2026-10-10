using System.Collections;
using System.Collections.Generic;
using CrystalFlux.CollectibleSystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.WaveEventSystem
{
    [CreateAssetMenu(fileName = "Blessing Drop Event", menuName = "Data/Events/Blessing Drop")]
    public class BlessingDropEventData : WaveEventData
    {
        [Header("Blessing Drop")]
        [Tooltip("Weighted collectible pool. Spawner boxes are never dropped")] public List<SpawnerBoxReward> rewards = new();
        [Min(0)] public int minDrops = 3;
        [Min(0)] public int maxDrops = 6;
        [Tooltip("Seconds between consecutive drops. Paused between waves and during Drought")] [Min(0f)] public float minInterval = 0.5f;
        [Min(0f)] public float maxInterval = 1.5f;

        public override IEnumerator Run(WaveEventContext ctx)
        {
            int n = RollCount(minDrops, maxDrops);

            for (int i = 0; i < n; i++)
            {
                if (i > 0) yield return ctx.Wait(RollInterval(minInterval, maxInterval));
                yield return ctx.WaitWhilePaused();

                CollectibleSpawner cs = CollectibleSpawner.Active;
                if (cs == null) yield break;

                CollectibleData d = CollectibleData.PickWeighted(rewards, IronmanSelector.Enabled);
                if (d != null && ctx.TryGetSpawnPoint(out Vector2 pos)) cs.SpawnAt(d, pos);
            }
        }
    }
}
