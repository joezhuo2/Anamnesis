using UnityEngine;

[CreateAssetMenu(fileName = "se_dot", menuName = "Status Effects/Debuff/DoT")]
public class DoT : StatusEffect
{
    [Tooltip("Damage per tick - % of source gameobject's stat type")] public float dpt;
    public StatType scalingStat = StatType.EffAtk;
    public Color indicatorColor = Color.red;

    public override void OnTick()
    {
        if (target == null || !target.TryGetComponent<EntityHealth>(out var eh) || !source.TryGetComponent<EntityStatManager>(out var ssm)) return;

        float damage = dpt * 0.01f * ssm.GetStat(scalingStat) * currentStacks;
        eh.ChangeHealth(-damage, 0f, true, false, indicatorColor, false);
    }

}