using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;
using CrystalFlux.Core;

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
            if (player.TryGetComponent<IStatusEffectReceiver>(out var sem))
            {
                if (sem.GetActiveFirstEffectOfType<SoulRend>() != null && sem.GetActiveFirstEffectOfType<SoulRend>().currentStacks >= 50)
                {
                    ps.StartCoroutine(ps.SpawnFromPattern(projectilePrefab, player));
                    sem.RemoveEffectAfterDelay<SoulRend>(0.3f);
                }
            }
        }
    }
    public override void OnUnlock(GameObject player)
    {
        if (soulRend == null || player == null) return;
        if (!player.TryGetComponent<PlayerUpgradeManager>(out var pum)) return;

        EffectData ed = new EffectData
        {
            effect = soulRend,
            selfApply = true,
            applyCondition = ApplyCondition.OnHit,
            chance = 1f
        };

        pum.RegisterAttackEffect(AttackType.Basic, ed);
        pum.RegisterAttackEffect(AttackType.Skill, ed);
    }

    public override void OnRemove(GameObject player)
    {
        if (soulRend == null || player == null) return;
        if (!player.TryGetComponent<PlayerUpgradeManager>(out var pum)) return;

        pum.UnregisterAttackEffect(AttackType.Basic, soulRend);
        pum.UnregisterAttackEffect(AttackType.Skill, soulRend);
    }
}
