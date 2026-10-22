using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class EnemyMutationEffects : MonoBehaviour
    {
        public float lifestealPct;
        public float thornsPct;
        public DamageType thornsType = DamageType.Physical;
        public float iFrameDuration;
        public float shieldCapPct;

        private static readonly Color thornsColor = new(0.8f, 0.3f, 0.3f);

        public void Lifesteal(float damageDealt)
        {
            if (lifestealPct <= 0f || damageDealt <= 0f || !TryGetComponent<EntityHealth>(out var eh)) return;
            eh.ChangeHealth(damageDealt * lifestealPct * 0.01f, true, 1f, Color.green);
        }

        public void Reflect(GameObject attacker, float damageTaken)
        {
            if (thornsPct <= 0f || damageTaken <= 0f || attacker == null || !attacker.TryGetComponent<IDamageable>(out var target)) return;

            DamagePacket dp = DamageRoll.Build(damageTaken * thornsPct * 0.01f, thornsType, false, thornsColor, gameObject, true, 1f);
            target.TakeDamage(dp);
            DamagePacket.Release(dp);
        }
    }
}
