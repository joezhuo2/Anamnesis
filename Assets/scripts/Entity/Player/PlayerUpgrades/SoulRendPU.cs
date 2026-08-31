using System.Collections.Generic;
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
        if (soulRend == null) return;

        if (player.TryGetComponent<PlayerAttackHandler>(out var pah))
        {
            EffectData soulRendEffect = new EffectData
            {
                effect = soulRend,
                selfApply = true,
                applyCondition = ApplyCondition.OnHit,
                chance = 1f
            };

            AddOnce(pah.FindAttackOfType(AttackType.Basic), soulRendEffect);
            AddOnce(pah.FindAttackOfType(AttackType.Skill), soulRendEffect);
        }
    }

    private void AddOnce(AttackData attack, EffectData ed)
    {
        if (attack == null || attack.pd == null) return;

        attack.pd.effects ??= new List<EffectData>();

        foreach (var existing in attack.pd.effects)
            if (existing.effect == ed.effect) return;

        attack.pd.effects.Add(ed);
    }
}
