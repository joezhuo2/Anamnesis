using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/Paradox")]
public class Paradox : PlayerUpgrade
{
    public override void OnUnlock(GameObject player)
    {
        if (player != null && player.TryGetComponent<IStatProvider>(out var isp))
            isp.AddStat(new StatBuff(StatType.globalDoTCanCrit, 1f));
    }
}
