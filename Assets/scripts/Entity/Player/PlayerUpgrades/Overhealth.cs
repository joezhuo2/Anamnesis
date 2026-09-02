using System.Collections.Generic;
using CrystalFlux.EntitySystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/Overhealth")]
public class Overhealth : PlayerUpgrade
{
    [Tooltip("Percent of ALL healing received at full health converted into overhealth")]
    public float conversionPct = 50f;
    public float decayPct = 3f;
    public float decayInterval = 0.5f;

    public override void OnUnlock(GameObject player)
    {
        if (player != null && player.TryGetComponent<EntityHealth>(out var eh))
            eh.SetOverhealth(conversionPct, decayPct, decayInterval);
    }

    public override void OnRemove(GameObject player)
    {
        if (player != null && player.TryGetComponent<EntityHealth>(out var eh))
            eh.SetOverhealth(0f, 0f, 0f);
    }

    public override void GetTooltipLines(List<string> lines)
    {
        base.GetTooltipLines(lines);
        lines.Add($"Converts {conversionPct:F0}% of healing received at full health into overhealth");
        lines.Add($"Overhealth absorbs damage before health");
        lines.Add($"Overhealth decays {decayPct:F0}% per {decayInterval:F1}s");
    }
}
