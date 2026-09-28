using System.Collections.Generic;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/AddChain")]
public class AddChain : PlayerUpgrade
{
    [Tooltip("Chance for the end of an additionalAttack chain to retrigger the starting attack")]
    public float retriggerChance = 25f;

    public override void OnUnlock(GameObject player) => Projectile.ChainRetriggerChance = Mathf.Max(0f, retriggerChance);

    public override void OnRemove(GameObject player) => Projectile.ChainRetriggerChance = 0f;

    public override void GetTooltipLines(List<string> lines)
    {
        base.GetTooltipLines(lines);
        lines.Add($"{retriggerChance:F0}% chance for an additional attack to retrigger its original attack");
    }
}
