using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/SoulRend")]
public class SoulRendPU : PlayerUpgrade
{
    public GameObject projectilePrefab;
    public SoulRend soulRend;

    public override void TriggerUpgradeEffect(GameObject player)
    {
        var ps = ProjectileSpawner.Instance;
        if (projectilePrefab != null && ps != null)
        {
            if (player.TryGetComponent<StatusEffectManager>(out var sem))
            {
                if (sem.GetActiveFirstEffectOfType<SoulRend>() != null && sem.GetActiveFirstEffectOfType<SoulRend>().currentStacks >= 50)
                {
                    ps.StartCoroutine(ps.SpawnFromPattern(projectilePrefab, player));
                    sem.StartCoroutine(sem.RemoveEffectAfterDelay<SoulRend>(0.3f));
                }
            }
        }
    }
    public override void OnUnlock(GameObject player)
    {
        if (player.TryGetComponent<PlayerAttackHandler>(out var pah))
        {
            AttackData basic = pah.FindAttackOfType(AttackType.Basic);
            AttackData skill = pah.FindAttackOfType(AttackType.Skill);

            if (basic != null && basic.pd != null)
            {
                var bpd = basic.pd;
                bpd.effectChance = 100f;
                bpd.selfApply = true;
                bpd.applyCondition = ApplyCondition.OnHit;
                bpd.effect = soulRend;
            }

            if (skill != null && skill.pd != null)
            {
                var spd = skill.pd;
                spd.effectChance = 100f;
                spd.selfApply = true;
                spd.applyCondition = ApplyCondition.OnHit;
                spd.effect = soulRend;
            }
        }
    }
}