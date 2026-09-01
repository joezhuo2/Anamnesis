using System.Collections.Generic;
using UnityEngine;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.Core;

namespace CrystalFlux.EntitySystem
{
    public class EntityProjectileHandler : MonoBehaviour, IOrbitRegister, IChargeRegister
    {
        [Tooltip("Maximum orbiting projectiles tracked. 0 = unlimited.")]
        public int maxOrbiting = 0;

        private readonly List<Projectile> orbitingProjectiles = new();
        private readonly List<Projectile> chargedProjectiles = new();
        public int Count => orbitingProjectiles.Count;

        private Camera mainCam;
        private Camera MainCam => mainCam != null ? mainCam : mainCam = Camera.main;

        private void OnDestroy()
        {
            orbitingProjectiles.Clear();
            chargedProjectiles.Clear();
        }

        private readonly List<Projectile> takeBuffer = new();
        private static readonly List<Collider2D> overlapBuffer = new();

        private static int OverlapCircle(Vector2 position, float radius)
        {
            ContactFilter2D filter = default;
            filter.useTriggers = Physics2D.queriesHitTriggers;

            return Physics2D.OverlapCircle(position, radius, filter, overlapBuffer);
        }

        private List<Projectile> TakeOrbits(int count)
        {
            var taken = takeBuffer;
            taken.Clear();

            int wanted = count <= 0 ? orbitingProjectiles.Count : Mathf.Min(count, orbitingProjectiles.Count);

            for (int i = orbitingProjectiles.Count - 1; i >= 0 && taken.Count < wanted; i--)
            {
                Projectile p = orbitingProjectiles[i];
                orbitingProjectiles.RemoveAt(i);
                if (p != null && p.gameObject != null) taken.Add(p);
            }

            return taken;
        }

        public void RegisterOrbitingProjectile(Projectile p)
        {
            if (p == null || orbitingProjectiles.Contains(p)) return;

            if (maxOrbiting > 0 && Count >= maxOrbiting)
            {
                Projectile oldest = orbitingProjectiles[0];
                orbitingProjectiles.RemoveAt(0);
                if (oldest != null && oldest.gameObject != null)
                    Destroy(oldest.gameObject);
            }

            orbitingProjectiles.Add(p);
        }
        public void UnregisterOrbitingProjectile(Projectile p)
        {
            if (p != null) orbitingProjectiles.Remove(p);
        }
        public void RegisterChargedProjectile(Projectile p)
        {
            if (p == null || chargedProjectiles.Contains(p)) return;

            chargedProjectiles.Add(p);
        }
        public void UnregisterChargedProjectile(Projectile p)
        {
            if (p != null) chargedProjectiles.Remove(p);
        }
        public void TickChargedProjectiles(AttackData source)
        {
            for (int i = chargedProjectiles.Count - 1; i >= 0; i--)
            {
                Projectile p = chargedProjectiles[i];

                if (p == null)
                {
                    chargedProjectiles.RemoveAt(i);
                    continue;
                }

                if (source != null && p.pd != null && p.pd.mainAttack != source) continue;

                p.OnChargeTick();
            }
        }
        public void ReleaseOrbits(int count = 0)
        {
            Camera cam = MainCam;
            if (cam == null) return;

            Vector3 mouseWorld = cam.ScreenToWorldPoint(InputState.mousePos);
            mouseWorld.z = 0f;

            foreach (var p in TakeOrbits(count))
                p.Launch(((Vector2)mouseWorld - (Vector2)p.transform.position).normalized);
        }
        public void ReleaseOrbits(Vector2 dir, int count = 0)
        {
            foreach (var p in TakeOrbits(count))
                p.Launch(dir.normalized);
        }
        public int AbsorbOrbits(int count = 0, float absorbPct = 0f)
        {
            var absorbed = TakeOrbits(count);
            int n = absorbed.Count;

            foreach (var p in absorbed)
            {
                TriggerStatGain(p, absorbPct * 0.01f);
                Destroy(p.gameObject);
            }

            return n;
        }
        public void RedirectOrbits(int count = 0)
        {
            foreach (var p in TakeOrbits(count))
            {
                Transform target = FindNearestEnemy(p.transform.position);
                if (target != null)
                    p.Launch(((Vector2)target.position - (Vector2)p.transform.position).normalized);
            }
        }
        public void ExplodeOrbits(int count = 0)
        {
            foreach (var p in TakeOrbits(count))
                p.Explode();
        }
        private void TriggerStatGain(Projectile p, float mult = 1f)
        {
            if (p == null || p.pd == null || p.ownerObj == null) return;

            if (p.pd.mainAttack == null) return;

            float hpGain = p.pd.mainAttack.healthGainOnHit * 0.01f * mult;
            float staminaGain = p.pd.mainAttack.staminaGainOnHit * 0.01f * mult;
            float manaGain = p.pd.mainAttack.manaGainOnHit * 0.01f * mult;

            var dp = DamagePacketBuilder.BuildDamagePacket(hpGain, DamageType.Heal, false, Color.green, p.ownerObj, true, 1f);

            GameObject target = p.ownerObj;
            if (target.TryGetComponent<IDamageable>(out var eh)) eh.TakeDamage(dp);
            if (target.TryGetComponent<IResourcePool>(out var pr))
            {
                pr.TryGain(ResourceType.Stamina, staminaGain);
                pr.TryGain(ResourceType.Mana, manaGain);
            }
        }

        private Transform FindNearestEnemy(Vector3 position)
        {
            Transform closest = null;
            float closestDist = float.MaxValue;

            int ownTeam = TryGetComponent<ITeamMember>(out var self) ? self.TeamID : 0;
            int count = OverlapCircle(position, 20f);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = overlapBuffer[i];
                if (!col.gameObject.TryGetComponent<ITeamMember>(out var itm) || itm.TeamID == ownTeam) continue;
                if (col.gameObject == gameObject) continue;

                if (col.TryGetComponent<IStatProvider>(out var esm)
                    && (esm.GetStat(StatType.isAlive) <= 0f || esm.GetStat(StatType.currentHp) <= 0f))
                    continue;

                float dist = Vector2.Distance(position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = col.transform;
                }
            }
            return closest;
        }
    }
}
