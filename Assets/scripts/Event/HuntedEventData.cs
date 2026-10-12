using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.WaveEventSystem
{
    [CreateAssetMenu(fileName = "Hunted Event", menuName = "Data/Events/Hunted")]
    public class HuntedEventData : WaveEventData
    {
        [Header("Hunted")]
        [Tooltip("Picked uniformly. Its own movement and attacks decide how it hunts the player")] public List<GameObject> hunters = new();
        [Tooltip("Seconds the hunter lasts. Paused between waves and during Drought; it also despawns when the wave ends")] [Min(0f)] public float duration = 20f;
        [Tooltip("Added to the current wave enemy level, plus the difficulty's eventLevelBonusAdd")] public int levelBonus;

        public override IEnumerator Run(WaveEventContext ctx)
        {
            GameObject prefab = PickHunter();
            if (prefab == null || duration <= 0f || !WaveManager.WaveActive || !ctx.TryGetSpawnPoint(out Vector2 pos)) yield break;

            int wave = WaveManager.CurrentWave;
            int level = WaveManager.EnemyLevelFor(levelBonus + WaveManager.Difficulty.eventLevelBonusAdd);

            // Deliberately not registered with the wave: it is not counted, not killable and must never hold the wave open.
            GameObject hunter = EnemySpawning.SpawnEnemy(prefab, pos, 0f, level);
            if (hunter == null) yield break;

            // EntityStatManager.Start resets the immune flag, so apply it once Start has run.
            yield return null;
            if (hunter != null && hunter.TryGetComponent<EntityStatManager>(out var esm)) esm.AddStat(new StatBuff(StatType.isImmune, 1f));

            float remaining = duration;
            while (remaining > 0f && hunter != null && WaveManager.WaveActive && WaveManager.CurrentWave == wave)
            {
                yield return null;
                if (!WaveEventContext.Paused) remaining -= Time.deltaTime;
            }

            if (hunter != null) Destroy(hunter);
        }

        private GameObject PickHunter()
        {
            if (hunters == null) return null;

            int count = 0;
            for (int i = 0; i < hunters.Count; i++)
                if (hunters[i] != null) count++;

            if (count == 0) return null;

            int k = Random.Range(0, count);
            for (int i = 0; i < hunters.Count; i++)
            {
                if (hunters[i] == null) continue;
                if (k-- == 0) return hunters[i];
            }

            return null;
        }
    }
}
