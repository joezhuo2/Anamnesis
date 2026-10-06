using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_shatter", menuName = "Status Effects/Debuff/Shatter")]
    public class Shatter : StatusEffect
    {
        [Tooltip("Multiplier of source's crit damage stat dealt per consumed CC (4 = 400%)")] public float critDmgMult = 4f;
        public Color indicatorColor = Color.lightBlue;

        private static readonly List<Stun> stuns = new();
        private static readonly List<Freeze> freezes = new();

        public override void OnApply() => Consume();
        public override void OnStack() => Consume();

        private void Consume()
        {
            if (target == null || source == null) return;
            if (!target.TryGetComponent<IStatusEffectReceiver>(out var sem)) return;
            if (!target.TryGetComponent<IDamageable>(out var eh)) return;
            if (!source.TryGetComponent<IStatProvider>(out var ssm)) return;

            sem.GetActiveEffectsOfType<Stun>(stuns);
            sem.GetActiveEffectsOfType<Freeze>(freezes);
            int count = stuns.Count + freezes.Count;
            stuns.Clear();
            freezes.Clear();
            if (count == 0) return;

            for (int i = 0; i < count && sem.GetActiveFirstEffectOfType<Stun>() != null; i++)
                sem.RemoveEffect<Stun>();
            for (int i = 0; i < count && sem.GetActiveFirstEffectOfType<Freeze>() != null; i++)
                sem.RemoveEffect<Freeze>();

            float dmg = critDmgMult * ssm.GetStat(StatType.critDamage);
            if (dmg <= 0f) return;

            for (int i = 0; i < count; i++)
            {
                if (target == null) return;
                DamagePacket dp = DamageRoll.Build(dmg, DamageType.True, false, indicatorColor, source, true, 1.5f);
                eh.TakeDamage(dp);
                DamagePacket.Release(dp);
            }
        }
    }
}
