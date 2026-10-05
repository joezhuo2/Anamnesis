using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

public class TwinCrownsInstance : AnomalyInstance
{
    public float penaltyAmount;

    private readonly string description;
    private readonly EntityHealth[] crowns = new EntityHealth[2];
    private readonly IStatProvider[] stats = new IStatProvider[2];
    private readonly int[] cachedMax = new int[2];
    private readonly bool[] seenAlive = new bool[2];
    private int count;
    private bool enraged;
    private GameObject crownPrefab;

    public TwinCrownsInstance(AnomalyData data) : base(data)
    {
        penaltyAmount = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal));
        string eff = data.anomalyEffect is StatusEffect se && !string.IsNullOrWhiteSpace(se.effName) ? se.effName : "Enraged";
        description = $"A second copy of the boss spawns with -{penaltyAmount}% Health and Damage. Slaying one grants the other {eff}";
    }

    public override string Description => description;

    public GameObject CrownPrefab => count > 0 ? crownPrefab : null;

    public override void StartAnomaly()
    {
        base.StartAnomaly();
        Clear();
    }

    public override void OnEnemySpawned(GameObject enemy, GameObject prefab, int level)
    {
        if (!isActive || enemy == null || count >= 2) return;
        if (count > 0 && prefab != crownPrefab) return;
        if (!enemy.TryGetComponent<EntityHealth>(out var eh) || !enemy.TryGetComponent<IStatProvider>(out var esm)) return;

        if (count == 0) crownPrefab = prefab;
        else
        {
            esm.AddStat(new StatBuff(StatType.hpPct, -penaltyAmount));
            esm.AddStat(new StatBuff(StatType.damagePct, -penaltyAmount));
        }

        crowns[count] = eh;
        stats[count] = esm;
        cachedMax[count] = Mathf.RoundToInt(esm.GetStat(StatType.EffMaxHp));
        count++;
    }

    public int PoolCur()
    {
        int sum = 0;
        for (int i = 0; i < count; i++)
            if (Alive(i)) sum += Mathf.Max(0, Mathf.RoundToInt(stats[i].GetStat(StatType.currentHp)));
        return sum;
    }

    public int PoolMax()
    {
        int sum = 0;
        for (int i = 0; i < count; i++)
        {
            if (crowns[i] != null && stats[i] != null) cachedMax[i] = Mathf.RoundToInt(stats[i].GetStat(StatType.EffMaxHp));
            sum += cachedMax[i];
        }
        return sum;
    }

    public override void UpdateCheck(float dt)
    {
        if (enraged || count < 2) return;

        for (int i = 0; i < 2; i++)
            if (Alive(i)) seenAlive[i] = true;

        bool aDead = seenAlive[0] && !Alive(0);
        bool bDead = seenAlive[1] && !Alive(1);
        if (aDead == bDead) return;

        enraged = true;
        EntityHealth survivor = aDead ? crowns[1] : crowns[0];
        if (amd != null && amd.anomalyEffect != null && survivor != null && survivor.TryGetComponent<StatusEffectManager>(out var sem))
            sem.ApplyPermanent(amd.anomalyEffect, null);
    }

    private bool Alive(int i) => crowns[i] != null && crowns[i].IsAlive;

    private void Clear()
    {
        for (int i = 0; i < 2; i++)
        {
            crowns[i] = null;
            stats[i] = null;
            cachedMax[i] = 0;
            seenAlive[i] = false;
        }
        count = 0;
        enraged = false;
        crownPrefab = null;
    }

    public override void ResetForWave()
    {
        Clear();
        base.ResetForWave();
    }
}
