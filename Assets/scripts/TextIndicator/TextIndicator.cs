using TMPro;
using UnityEngine;

namespace CrystalFlux.Core
{
    public enum TextType { Standard, Gold, Exp }

    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextIndicator : MonoBehaviour
    {
        public float floatSpeed;
        public Vector2 maxRandomOffset = new(0.5f, 0.5f);
        private TextMeshProUGUI text;
        private Vector3 worldPos;
        private Camera mainCam;
        private float timer;
        private float delayTimer;
        private float baseFontSize;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            baseFontSize = text.fontSize;
        }

        public void Initialize(int val, Vector3 sourcePos, Color color, float scale, float lifetime, float floatSpeed, TextType textType, float delay = 0f)
        {
            string result = val >= 1_000_000 ? (val / 1_000_000f).ToString("0.#") + "M"
              : val >= 1_000     ? (val / 1_000f).ToString("0.#") + "k"
              : val.ToString();

            string content = textType switch
            {
                TextType.Gold => $"{(val >= 0 ? "+" : "")}{result} g",
                TextType.Exp => $"{(val >= 0 ? "+" : "")}{result} xp",
                _ => result,
            };

            Initialize(content, sourcePos, color, scale, lifetime, floatSpeed, delay);
        }

        public void Initialize(string content, Vector3 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay = 0f)
        {
            mainCam = mainCam != null ? mainCam : Camera.main;

            worldPos = sourcePos + new Vector3(
                Random.Range(-maxRandomOffset.x, maxRandomOffset.x),
                Random.Range(-maxRandomOffset.y, maxRandomOffset.y),
                0f
            );

            if (mainCam != null) transform.position = mainCam.WorldToScreenPoint(worldPos);

            text.text = content;
            text.color = color;
            text.fontSize = baseFontSize * scale;

            timer = lifetime;
            this.floatSpeed = floatSpeed;
            delayTimer = Mathf.Max(0f, delay);
            text.enabled = delayTimer <= 0f;
        }

        private void Update()
        {
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer > 0f) return;
                text.enabled = true;
            }

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
