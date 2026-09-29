using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/GoldBuff")]
public class GoldBuff : PlayerUpgrade
{
    [Min(1)] public int goldPerStack = 100;
    [Min(1)] public int maxStacks = 5;
    public StatBuff[] buffs;

    private IStatProvider isp;
    private MonoBehaviour host;
    private Coroutine loop;
    private int applied;
    private static readonly WaitForSecondsRealtime tickWait = new(0.2f);

    public int CurrentStacks => applied;

    public override void OnUnlock(GameObject player)
    {
        if (player == null || !player.TryGetComponent<PlayerUpgradeManager>(out var pum)) return;
        if (!player.TryGetComponent(out isp)) return;

        StopLoop();
        applied = 0;
        host = pum;
        Refresh();
        loop = host.StartCoroutine(PollLoop(player));
    }

    public override void OnRemove(GameObject player)
    {
        StopLoop();
        SetStacks(0);
        isp = null;
    }

    private void OnDestroy() => StopLoop();

    private void StopLoop()
    {
        if (host != null && loop != null) host.StopCoroutine(loop);
        loop = null;
        host = null;
    }

    private IEnumerator PollLoop(GameObject player)
    {
        while (player != null)
        {
            yield return tickWait;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (isp == null) return;

        int gold = Mathf.Max(0, Mathf.FloorToInt(isp.GetStat(StatType.Gold)));
        SetStacks(Mathf.Min(gold / Mathf.Max(1, goldPerStack), maxStacks));
    }

    private void SetStacks(int target)
    {
        int delta = target - applied;
        if (delta == 0 || isp == null || buffs == null) return;

        for (int i = 0; i < buffs.Length; i++)
            isp.AddStat(new StatBuff(buffs[i].type, buffs[i].value * delta));

        applied = target;
    }

    public override void GetTooltipLines(List<string> lines)
    {
        lines.Add($"Every {goldPerStack} gold held grants (max {maxStacks} stacks):");
        if (buffs == null) return;

        for (int i = 0; i < buffs.Length; i++)
            lines.Add($"{buffs[i].value:+0.##;-0.##} {buffs[i]}");
    }
}
