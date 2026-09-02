using CrystalFlux.EntitySystem;
using UnityEngine;
using CrystalFlux.Core;

public enum CooldownAdvanceType { All, Basic, Skill, Ult, Dash }

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/CooldownAdvance")]
public class CooldownAdvance : PlayerUpgrade
{
    public float amt;
    public CooldownAdvanceType type;

    public override void TriggerUpgradeEffect(GameObject player)
    {
        if (player.TryGetComponent<PlayerAttackHandler>(out var pah))
        {
            switch (type)
            {
                case CooldownAdvanceType.All: pah.AdvanceAllCooldowns(amt); break;
                case CooldownAdvanceType.Basic: pah.AdvanceCooldown(AttackType.Basic, amt); break;
                case CooldownAdvanceType.Skill: pah.AdvanceCooldown(AttackType.Skill, amt); break;
                case CooldownAdvanceType.Ult: pah.AdvanceCooldown(AttackType.Ultimate, amt); break;
                default: break;
            }
        }
        if (type == CooldownAdvanceType.Dash && player.TryGetComponent<PlayerMovement>(out var pm))
        {
            pm.AdvanceDash(amt);
        }
    }
}
