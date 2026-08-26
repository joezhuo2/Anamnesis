using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "se_ar", menuName = "Status Effects/Buff/Attack Replacement")]

public class AttackReplacement : StatusEffect
{
    public AttackData replacement;
    private AttackData originalAttack = null;
    private bool setAttack = false;

    public override void OnApply()
    {
        if (target == null || replacement == null) return;
        if (!target.TryGetComponent<PlayerAttackHandler>(out var pah)) return;

        setAttack = true;

        AttackData original = pah.FindAttackOfType(replacement.type);
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
        if (!target.TryGetComponent<PlayerAttackHandler>(out var pah)) return;

        pah.UpdateAttack(replacement.type, originalAttack);
    }

    private void OnDestroy()
    {
        if (originalAttack != null)
            Destroy(originalAttack);
    }
}