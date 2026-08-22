using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicatorSpawner : MonoBehaviour
{
    public static DamageIndicatorSpawner Instance;
    public DamageIndicator prefab;
    public Canvas canvas;
    public int initialPoolSize = 20;

    private readonly Queue<DamageIndicator> _pool = new();
    private readonly List<DamageIndicator> _activeIndicators = new();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var indicator = Instantiate(prefab, canvas.transform);
            indicator.gameObject.SetActive(false);
            _pool.Enqueue(indicator);
        }
    }

    public void SpawnDamageIndicator(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay)
    => StartCoroutine(SpawnAfterDelay(damage, sourcePos, color, scale, lifetime, floatSpeed, delay));

    private IEnumerator SpawnAfterDelay(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnDamageIndicator(damage, sourcePos, color, scale, lifetime, floatSpeed);
    }

    private void SpawnDamageIndicator(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed)
    {
        DamageIndicator indicator;

        if (_pool.Count > 0)
        {
            indicator = _pool.Dequeue();
            indicator.gameObject.SetActive(true);
        }
        else
        {
            indicator = Instantiate(prefab, canvas.transform);
        }

        indicator.Initialize(damage, sourcePos, color, scale, lifetime, floatSpeed);
        _activeIndicators.Add(indicator);
    }

    public void ReturnToPool(DamageIndicator indicator)
    {
        if (indicator == null) return;

        _activeIndicators.Remove(indicator);
        indicator.gameObject.SetActive(false);
        _pool.Enqueue(indicator);
    }
}