using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextIndicatorSpawner : MonoBehaviour
{
    public static TextIndicatorSpawner Instance;
    public TextIndicator prefab;
    public Canvas canvas;
    public int initialPoolSize = 100;

    private readonly Queue<TextIndicator> _pool = new();
    private readonly List<TextIndicator> _activeIndicators = new();

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

    public void SpawnTextIndicator(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay, bool xpWrapperText = false)
    => StartCoroutine(SpawnAfterDelay(damage, sourcePos, color, scale, lifetime, floatSpeed, delay, xpWrapperText));

    private IEnumerator SpawnAfterDelay(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay, bool xpWrapperText = false)
    {
        yield return new WaitForSeconds(delay);
        SpawnTextIndicator(damage, sourcePos, color, scale, lifetime, floatSpeed, xpWrapperText);
    }

    private void SpawnTextIndicator(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed, bool xpWrapperText = false)
    {
        TextIndicator indicator;

        if (_pool.Count > 0)
        {
            indicator = _pool.Dequeue();
            indicator.gameObject.SetActive(true);
        }
        else
        {
            indicator = Instantiate(prefab, canvas.transform);
        }

        indicator.Initialize(damage, sourcePos, color, scale, lifetime, floatSpeed, xpWrapperText);
        _activeIndicators.Add(indicator);
    }

    public void ReturnToPool(TextIndicator indicator)
    {
        if (indicator == null) return;

        _activeIndicators.Remove(indicator);
        indicator.gameObject.SetActive(false);
        _pool.Enqueue(indicator);
    }
}