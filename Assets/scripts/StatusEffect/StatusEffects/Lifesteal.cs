using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_lifesteal", menuName = "Status Effects/Buff/Lifesteal")]
    public class Lifesteal : StatusEffect
    {
        [Header("Lifesteal")]
        [Tooltip("Chance to heal when the user deals damage")] [Range(0f, 100f)] public float procChance = 100f;
        [Tooltip("% of damage dealt healed, multiplied by current stacks")] public float healPctPerStack = 5f;
        [Tooltip("Seconds between procs")] public float procCooldown = 1f;
        public Color indicatorColor = Color.green;

        private float lastProcTime = float.NegativeInfinity;

        public override void OnApply() => lastProcTime = float.NegativeInfinity;

        public void TryLifesteal(float damageDealt)
        {
            if (damageDealt <= 0f || target == null) return;
            if (procCooldown > 0f && Time.time < lastProcTime + procCooldown) return;
            if (Random.Range(0f, 100f) > procChance) return;
            if (!target.TryGetComponent<IDamageable>(out var eh)) return;

            float heal = damageDealt * 0.01f * healPctPerStack * currentStacks * potencyMultiplier;
            if (heal <= 0f) return;

            lastProcTime = Time.time;
            eh.TakeDamage(DamageRoll.Build(heal, DamageType.Heal, false, indicatorColor, target, true, 1f));
        }
    }
}
