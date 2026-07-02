using UnityEngine;

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
        if (source == null || target == null || !target.TryGetComponent<EntityHealth>(out var eh) || !source.TryGetComponent<EntityStatManager>(out var ssm)) return;

        float damage = dpt * 0.01f * ssm.GetStat(scalingStat) * currentStacks;

        bool globalDoTCanCrit = source.TryGetComponent<PlayerUpgradeManager>(out var pum) && pum.HasUpgradeOfType<Paradox>();

        DamagePacket damagePacket = DamageCalculator.BuildDamagePacket(damage, damageType, ssm.s, globalDoTCanCrit || canCrit, indicatorColor);
        eh.TakeDamage(damagePacket, true, source, ssm.s.resPen, ssm.s.defShred);
    }
}