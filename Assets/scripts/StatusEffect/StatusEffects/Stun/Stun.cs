using UnityEngine;

[CreateAssetMenu(fileName = "se_stun", menuName = "Status Effects/Debuff/Stun")]
public class Stun : StatusEffect
{
    public override void OnApply()
    {
        if (target.TryGetComponent<EntityStatManager>(out var esm))
        {
            esm.AddStat(new(StatType.CanAttack, -1f));
            esm.AddStat(new(StatType.CanMove, -1f));
            esm.AddStat(new(StatType.CanDash, -1f));
        }
    }
    public override void OnExpire()
    {
        if (target.TryGetComponent<EntityStatManager>(out var esm))
        {
            esm.AddStat(new(StatType.CanAttack, 1f));
            esm.AddStat(new(StatType.CanMove, 1f));
            esm.AddStat(new(StatType.CanDash, 1f));
        }
    }
}