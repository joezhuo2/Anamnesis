using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DamageIndicator : MonoBehaviour 
{
    public float floatSpeed;
    public Vector2 maxRandomOffset = new Vector2(0.5f, 0.5f);
    private TextMeshProUGUI text;
    private Vector3 worldPos;
    private Camera mainCam;
    private float timer;

    public void Initialize(int damage, Vector3 sourcePos, Color color, float scale, float lifetime, float floatSpeed)
    {
        text ??= GetComponent<TextMeshProUGUI>();
        mainCam ??= Camera.main;

        worldPos = sourcePos + new Vector3(Random.Range(-maxRandomOffset.x, maxRandomOffset.x), Random.Range(-maxRandomOffset.y, maxRandomOffset.y), 0f);
        
        text.text = damage.ToString();
        text.color = color;
        text.fontSize *= scale;

        timer = lifetime;
        this.floatSpeed = floatSpeed;
    }

    void Update()
    {
        if (mainCam == null) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        transform.position = screenPos;

        worldPos += floatSpeed * Time.deltaTime * Vector3.up;

        timer -= Time.deltaTime;
        if (timer <= 0f) Destroy(gameObject);
    }
}