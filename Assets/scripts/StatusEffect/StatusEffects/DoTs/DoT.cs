using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_dot", menuName = "Status Effects/Debuff/DoT")]
    public class DoT : StatusEffect
    {
        [Tooltip("Damage per tick - % of source gameobject's stat type")] public float dpt;
        public StatType scalingStat = StatType.EffAtk;
        public DamageType damageType = DamageType.DoT;
        public Color indicatorColor = Color.red;
        public bool canCrit = false;

        public override void OnTick()
        {
            if (source == null || target == null || !target.TryGetComponent<IDamageable>(out var eh) || !source.TryGetComponent<IStatProvider>(out var ssm)) return;

            float damage = dpt * 0.01f * ssm.GetStat(scalingStat) * currentStacks;

            bool globalDoTCanCrit = ssm.GetStat(StatType.globalDoTCanCrit) > 0f;

            DamagePacket damagePacket = DamageRoll.Build(damage, damageType, globalDoTCanCrit || canCrit, indicatorColor, source, true, 1f);
            eh.TakeDamage(damagePacket);
        }
    }
}
