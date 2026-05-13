using UnityEngine;

public class DamageIndicatorSpawner : MonoBehaviour
{
    public static DamageIndicatorSpawner Instance;
    public DamageIndicator prefab;
    public Canvas canvas;
    void Awake() => Instance = this;
    
    public void SpawnDamageIndicator(int damage, Vector2 sourcePos, Color color, float scale, float lifetime, float floatSpeed)
    {
        DamageIndicator indicator = Instantiate(prefab, canvas.transform);
        indicator.Initialize(damage, sourcePos, color, scale, lifetime, floatSpeed);
    }
}