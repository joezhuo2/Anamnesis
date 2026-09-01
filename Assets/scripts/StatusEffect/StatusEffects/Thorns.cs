using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_thorns", menuName = "Status Effects/Buff/Thorns")]
    public class Thorns : StatusEffect
    {
        [Header("Thorns")]
        [Tooltip("Chance to reflect when hit by an enemy projectile")] [Range(0f, 100f)] public float procChance = 100f;
        [Tooltip("% of the damage taken dealt back to the projectile owner")] public float reflectPct = 25f;
        [Tooltip("Seconds between procs")] public float procCooldown = 1f;
        public DamageType dmgType = DamageType.True;
        public Color indicatorColor = Color.red;
        public bool canCrit = false;

        private float lastProcTime = float.NegativeInfinity;

        public override void OnApply() => lastProcTime = float.NegativeInfinity;

        public void TryReflect(GameObject attacker, float damageTaken)
        {
            if (attacker == null || target == null || damageTaken <= 0f) return;
            if (procCooldown > 0f && Time.time < lastProcTime + procCooldown) return;
            if (!attacker.TryGetComponent<IDamageable>(out var eh)) return;
            if (Random.Range(0f, 100f) > procChance) return;

            float dmg = damageTaken * 0.01f * reflectPct * potencyMultiplier;
            if (dmg <= 0f) return;

            lastProcTime = Time.time;
            eh.TakeDamage(DamageRoll.Build(dmg, dmgType, canCrit, indicatorColor, target, true, 1f));
        }
    }
}
