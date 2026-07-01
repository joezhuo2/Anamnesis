using UnityEngine;

[CreateAssetMenu(fileName = "se_stun", menuName = "Status Effects/Debuff/Stun")]
public class Stun : StatusEffect
{
    public override void OnApply()
    {
        if (target.TryGetComponent<EntityStatManager>(out var esm))
        {
            esm.s.canAttack = false;
            esm.s.canMove = false;
            esm.s.canDash = false;
        }
    }
    public override void OnExpire()
    {
        if (target.TryGetComponent<EntityStatManager>(out var esm))
        {
            esm.s.canAttack = true;
            esm.s.canMove = true;
            esm.s.canDash = true;
        }
    }
}