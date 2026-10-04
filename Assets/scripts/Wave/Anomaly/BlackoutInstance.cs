using System.Collections.Generic;
using CrystalFlux.EntitySystem;
using UnityEngine;

public class BlackoutInstance : AnomalyInstance
{
    private const float Darkness = 0.94f;

    public float baseRadius;
    public float perKill;

    private readonly string description;
    private readonly List<EntityHealth> tracked = new();
    private BlackoutVision vision;

    public BlackoutInstance(AnomalyData data) : base(data)
    {
        baseRadius = Mathf.Round(Random.Range(data.anomalyMinVal, data.anomalyMaxVal) * 2f) * 0.5f;
        perKill = data.anomalyValue > 0f ? data.anomalyValue : 1f;
        description = $"Your vision shrinks to {baseRadius:0.#} units around you. Enemies outside it are hidden, but their attacks are not. Each kill briefly widens your vision by {perKill:0.#}";
    }

    public override string Description => description;

    public override void StartAnomaly()
    {
        base.StartAnomaly();

        if (vision != null) Object.Destroy(vision.gameObject);

        vision = new GameObject("BlackoutVision").AddComponent<BlackoutVision>();
        vision.Setup(baseRadius, perKill, Darkness);
    }

    public override void OnEnemySpawned(GameObject enemy, GameObject prefab, int level)
    {
        if (enemy == null || !isActive) return;

        if (!enemy.TryGetComponent<BlackoutHider>(out _)) enemy.AddComponent<BlackoutHider>();

        if (enemy.TryGetComponent<EntityHealth>(out var eh))
        {
            eh.OnDeath += OnEnemyDeath;
            tracked.Add(eh);
        }
    }

    private void OnEnemyDeath(GameObject _)
    {
        if (vision != null) vision.Widen();
    }

    public override void ResetForWave()
    {
        for (int i = 0; i < tracked.Count; i++)
            if (tracked[i] != null) tracked[i].OnDeath -= OnEnemyDeath;
        tracked.Clear();

        if (vision != null) Object.Destroy(vision.gameObject);
        vision = null;

        base.ResetForWave();
    }
}
