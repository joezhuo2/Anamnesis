using TMPro;
using UnityEngine;

namespace CrystalFlux.Core
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextIndicator : MonoBehaviour
    {
        public float floatSpeed;
        public Vector2 maxRandomOffset = new(0.5f, 0.5f);
        private TextMeshProUGUI text;
        private Vector3 worldPos;
        private Camera mainCam;
        private float timer;
        private float baseFontSize;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            baseFontSize = text.fontSize;
        }

        public void Initialize(int val, Vector3 sourcePos, Color color, float scale, float lifetime, float floatSpeed, bool xpWrapperText = false, bool isGold = false)
        {
            mainCam = mainCam != null ? mainCam : Camera.main;

            worldPos = sourcePos + new Vector3(Random.Range(-maxRandomOffset.x, maxRandomOffset.x), Random.Range(-maxRandomOffset.y, maxRandomOffset.y), 0f);

            if (isGold) text.text = $"{(val >= 0 ? "+" : "")}{val}g";
            else if (xpWrapperText) text.text = $"{(val >= 0 ? "+" : "")}{val} xp";
            else text.text = val.ToString();

            text.color = color;
            text.fontSize = baseFontSize * scale;

            timer = lifetime;
            this.floatSpeed = floatSpeed;
        }

        private void Update()
        {
            if (mainCam == null) return;

            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
            transform.position = screenPos;

            worldPos += floatSpeed * Time.deltaTime * Vector3.up;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (TextIndicatorSpawner.Instance != null) TextIndicatorSpawner.Instance.ReturnToPool(this);
                else Destroy(gameObject);
            }
        }
    }
}
