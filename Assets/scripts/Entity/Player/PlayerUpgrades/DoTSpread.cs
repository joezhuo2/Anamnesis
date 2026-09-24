using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "PlayerUpgrade/DoTSpread")]
public class DoTSpread : PlayerUpgrade
{
    [Tooltip("spread radius in tiles (world units)")] public float radius = 3f;
    [Tooltip("seconds between spread attempts per debuff")] public float spreadInterval = 1f;
    [Tooltip("use the debuff's own tickInterval instead of spreadInterval")] public bool useTickInterval;
    [Range(0f, 100f)] public float spreadChance = 25f;

    private readonly Dictionary<StatusEffect, (float time, int gen)> nextSpread = new();
    private readonly List<StatusEffect> staleKeys = new();
    private MonoBehaviour host;
    private Coroutine loop;
    private float nextPrune;
    private const float tickRate = 0.1f;
    private static readonly WaitForSeconds tickWait = new(tickRate);

    public override void OnUnlock(GameObject player)
    {
        if (player == null || !player.TryGetComponent<PlayerUpgradeManager>(out var pum)) return;

        StopLoop();
        nextSpread.Clear();
        nextPrune = 0f;
        host = pum;
        loop = host.StartCoroutine(SpreadLoop(player));
    }

    public override void OnRemove(GameObject player)
    {
        StopLoop();
        nextSpread.Clear();
    }

    private void OnDestroy() => StopLoop();

    private void StopLoop()
    {
        if (host != null && loop != null) host.StopCoroutine(loop);
        loop = null;
        host = null;
    }

    private IEnumerator SpreadLoop(GameObject player)
    {
        while (player != null)
        {
            yield return tickWait;
            if (Time.timeScale == 0f) continue;
            Tick(player);
        }
    }

    private void Tick(GameObject player)
    {
        float now = Time.time;
        float r2 = radius * radius;
        var enemies = EnemyMovement.Active;

        for (int i = 0; i < enemies.Count; i++)
        {
            var em = enemies[i];
            if (em == null) continue;
            var sem = em.Sem;
            if (sem == null) continue;

            var effects = sem.activeEffects;
            for (int j = effects.Count - 1; j >= 0; j--)
            {
                if (j >= effects.Count) continue;
                StatusEffect e = effects[j];
                if (e == null || e.isBuff || e.source != player) continue;

                float interval = useTickInterval && e.tickInterval > 0f ? e.tickInterval : spreadInterval;
                if (interval <= 0f) continue;

                if (!nextSpread.TryGetValue(e, out var ns) || ns.gen != e.Generation)
                {
                    nextSpread[e] = (now + interval, e.Generation);
                    continue;
                }
                if (now < ns.time) continue;

                nextSpread[e] = (now + interval, e.Generation);
                if (Random.Range(0f, 100f) >= spreadChance) continue;

                Spread(e, em, player, r2, enemies);
            }
        }

        if (now >= nextPrune) Prune(now);
    }

    private static void Spread(StatusEffect e, EnemyMovement from, GameObject player, float r2, IReadOnlyList<EnemyMovement> enemies)
    {
        StatusEffect asset = e.origin != null ? e.origin : e;
        Vector2 center = from.transform.position;

        for (int k = 0; k < enemies.Count; k++)
        {
            var other = enemies[k];
            if (other == null || other == from) continue;

            Vector2 pos = other.transform.position;
            if ((pos - center).sqrMagnitude > r2) continue;
            var sp = other.Stats;
            if (sp != null && sp.GetStat(StatType.isAlive) <= 0f) continue;
            var osem = other.Sem;
            if (osem == null || osem.HasEffect(asset)) continue;

            osem.Apply(asset, player, pos);
        }
    }

    private void Prune(float now)
    {
        nextPrune = now + 2f;
        staleKeys.Clear();
        foreach (var kv in nextSpread)
            if (kv.Key == null || kv.Key.Released || kv.Key.Generation != kv.Value.gen) staleKeys.Add(kv.Key);
        for (int i = 0; i < staleKeys.Count; i++)
            nextSpread.Remove(staleKeys[i]);
        staleKeys.Clear();
    }

    public override void GetTooltipLines(List<string> lines)
    {
        string every = useTickInterval ? "each debuff tick" : $"every {spreadInterval:F1}s";
        lines.Add($"Debuffs on enemies have a {spreadChance:F0}% chance {every} to spread to enemies within {radius:F1} tiles");
    }
}
