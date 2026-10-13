using System;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

public class BloodtideInstance : AnomalyInstance
{
    private const int MaxIndicatorsPerDeath = 8;

    public float healPct;

    private readonly HashSet<EntityHealth> tracked = new();
    private readonly List<EntityHealth> living = new();
    private readonly Action<GameObject> onDeath;

    public BloodtideInstance(AnomalyData data) : base(data)
    {
        onDeath = OnEnemyDeath;
        Roll();
    }

    public override string Description =>
        $"Whenever an enemy dies, every other living enemy heals {healPct}% of its Max Health";

    private void Roll()
    {
        if (amd == null) return;
        healPct = Mathf.Round(UnityEngine.Random.Range(amd.anomalyMinVal, Mathf.Max(amd.anomalyMinVal, amd.anomalyMaxVal)));
    }

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        Untrack();
    }

    public override void OnEnemySpawned(GameObject enemy, GameObject prefab, int level)
    {
        if (!isActive || enemy == null) return;
        if (enemy.TryGetComponent<EntityHealth>(out var eh)) Track(eh);
    }

    // Picks up enemies the spawn hooks never saw (split clones, ambushes) so their deaths count as well.
    public override void UpdateCheck(float dt)
    {
        if (!isActive) return;

        WaveManager.GetLivingEnemies(living);
        for (int i = 0; i < living.Count; i++) Track(living[i]);
        living.Clear();
    }

    private void Track(EntityHealth eh)
    {
        if (eh == null || !tracked.Add(eh)) return;
        eh.OnDeath += onDeath;
    }

    private void OnEnemyDeath(GameObject dead)
    {
        if (!isActive || WaveManager.HivemindActive) return;

        // The dying enemy is already flagged dead, so it is excluded. Healing never kills and never fires OnDeath.
        WaveManager.GetLivingEnemies(living);

        int shown = 0;
        for (int i = 0; i < living.Count; i++)
        {
            EntityHealth eh = living[i];
            if (eh == null || eh.gameObject == dead || !eh.IsAlive) continue;
            if (!eh.TryGetComponent<IStatProvider>(out var esm)) continue;

            float max = esm.GetStat(StatType.EffMaxHp);
            if (max <= 0f || esm.GetStat(StatType.currentHp) >= max) continue;

            float amount = Mathf.Max(1f, Mathf.Round(max * healPct * 0.01f));
            eh.ChangeHealth(amount, shown < MaxIndicatorsPerDeath);
            shown++;
        }
        living.Clear();
    }

    private void Untrack()
    {
        foreach (EntityHealth eh in tracked)
            if (eh != null) eh.OnDeath -= onDeath;
        tracked.Clear();
        living.Clear();
    }

    public override void ResetForWave()
    {
        Untrack();
        Roll();
        base.ResetForWave();
    }
}
