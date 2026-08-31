using UnityEngine;
using CrystalFlux.Core;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_ar", menuName = "Status Effects/Buff/Attack Replacement")]

    public class AttackReplacement : StatusEffect
    {
        public AttackAsset replacement;
        private AttackAsset originalAttack = null;
        private bool setAttack = false;

        public override void OnApply()
        {
            if (target == null || replacement == null) return;
            if (!target.TryGetComponent<IAttackHandler>(out var pah)) return;

            setAttack = true;

            AttackAsset original = pah.FindAttackOfType(replacement.type);
            if (original != null)
            {
                originalAttack = Instantiate(original);
                originalAttack.DeepClone();
            }

            pah.UpdateAttack(replacement.type, replacement);
        }

        public override void OnExpire()
        {
            if (!setAttack || replacement == null || target == null) return;
            if (!target.TryGetComponent<IAttackHandler>(out var pah)) return;

            setAttack = false;

            if (originalAttack != null) pah.UpdateAttack(replacement.type, originalAttack);
            else pah.RemoveAttack(replacement.type);
        }

        private void OnDestroy()
        {
            if (originalAttack != null && originalAttack.IsRuntimeCopy)
                Destroy(originalAttack);
        }
    }
}
