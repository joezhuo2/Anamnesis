using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.WaveEventSystem
{
    public enum HazardAimMode { TowardPlayer, CenteredOnPlayer, RandomDirection }

    [CreateAssetMenu(fileName = "Spawn Projectile Event", menuName = "Data/Events/Spawn Projectile")]
    public class SpawnProjectileEventData : WaveEventData
    {
        private const int HazardTeam = -1;

        [Header("Spawn Projectile")]
        [Tooltip("Picked uniformly for each volley. Projectiles hit both the player and enemies")] public List<AttackData> attacks = new();
        [Min(0)] public int minVolleys = 4;
        [Min(0)] public int maxVolleys = 8;
        [Tooltip("Seconds between consecutive volleys. Paused between waves and during Drought")] [Min(0f)] public float minInterval = 0.5f;
        [Min(0f)] public float maxInterval = 1.5f;
        [Tooltip("TowardPlayer/RandomDirection fire from a ring point. CenteredOnPlayer runs the pattern around the player")] public HazardAimMode aimMode = HazardAimMode.TowardPlayer;

        [Header("Damage")]
        [Tooltip("Stats of the hidden hazard source, scaled to the current enemy level like spawned enemies")] public EntityStats hazardStats;
        [Tooltip("Added to the current wave enemy level")] public int levelBonus;
        [Tooltip("Seconds the hazard source outlives the last volley. Projectiles alive past this deal no damage")] [Min(0f)] public float ownerLinger = 10f;

        public override IEnumerator Run(WaveEventContext ctx)
        {
            int n = RollCount(minVolleys, maxVolleys);
            if (n <= 0 || hazardStats == null || PickAttack() == null) yield break;

            GameObject owner = CreateOwner(out HazardOwner hazard);

            for (int i = 0; i < n; i++)
            {
                if (i > 0) yield return ctx.Wait(RollInterval(minInterval, maxInterval));
                yield return ctx.WaitWhilePaused();

                ProjectileSpawner ps = ProjectileSpawner.Instance;
                GameObject player = ctx.Player;
                if (ps == null || player == null || owner == null) break;

                AttackData ad = PickAttack();
                if (ad != null && TryAim(ctx, player, ad, out Vector2 center, out Vector2 dir, out float dist))
                {
                    hazard.target = player.transform;
                    owner.transform.position = center;
                    ps.Spawn(ad, owner, center, dir, dist, null, true);
                }
            }

            if (owner != null) Destroy(owner, ownerLinger);
        }

        private GameObject CreateOwner(out HazardOwner hazard)
        {
            GameObject go = new($"Hazard ({name})");
            go.SetActive(false);

            EnemyStatManager esm = go.AddComponent<EnemyStatManager>();
            esm.baseStats = hazardStats;
            esm.teamID = HazardTeam;
            esm.displayName = title;
            hazard = go.AddComponent<HazardOwner>();

            go.SetActive(true);
            esm.ScaleStatsToLevel(WaveManager.EnemyLevelFor(levelBonus));
            return go;
        }

        private bool TryAim(WaveEventContext ctx, GameObject player, AttackData ad, out Vector2 center, out Vector2 dir, out float dist)
        {
            Vector2 playerPos = player.transform.position;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (randomDir == Vector2.zero) randomDir = Vector2.right;

            if (aimMode == HazardAimMode.CenteredOnPlayer)
            {
                center = playerPos;
                dir = randomDir;
                dist = ad.SpawnDistance;
                return true;
            }

            dist = 0f;
            if (!ctx.TryGetSpawnPoint(out center))
            {
                dir = default;
                return false;
            }

            Vector2 toPlayer = playerPos - center;
            dir = aimMode == HazardAimMode.TowardPlayer && toPlayer != Vector2.zero ? toPlayer.normalized : randomDir;
            return true;
        }

        private AttackData PickAttack()
        {
            if (attacks == null) return null;

            int count = 0;
            for (int i = 0; i < attacks.Count; i++)
                if (attacks[i] != null && attacks[i].ProjectilePrefab != null) count++;

            if (count == 0) return null;

            int k = Random.Range(0, count);
            for (int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i] == null || attacks[i].ProjectilePrefab == null) continue;
                if (k-- == 0) return attacks[i];
            }

            return null;
        }
    }
}
