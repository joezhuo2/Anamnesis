using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_bloodpool", menuName = "Status Effects/Buff/BloodPool")]
    public class BloodPool : StatusEffect
    {
        [Header("Blood Pool")]
        [Tooltip("% of health lost that is stored in the pool")] public float storePct = 50f;

        public float Stored { get; private set; }

        protected override void ResetRuntime() => Stored = 0f;

        public void Store(float healthLost)
        {
            if (healthLost <= 0f || target == null) return;
            Stored += healthLost * storePct * 0.01f * potencyMultiplier;
        }

        public float Withdraw()
        {
            float amt = Stored;
            Stored = 0f;
            return amt;
        }

        public override string GetDesc()
        {
            string stored = $"Stored: {Mathf.FloorToInt(Stored)}";
            return string.IsNullOrEmpty(desc) ? stored : $"{desc}\n{stored}";
        }
    }
}
