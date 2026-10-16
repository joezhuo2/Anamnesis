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
        private Vector3 lastScreenPos;
        private Camera mainCam;
        private float timer;
        private float delayTimer;
        private float baseFontSize;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            text.raycastTarget = false;
            baseFontSize = text.fontSize;
        }

        private static readonly string[] Suffixes = { "", "k", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        public void Initialize(float val, Vector3 sourcePos, Color color, float scale, float lifetime, float floatSpeed, TextType textType, float delay = 0f)
        {
            if (textType == TextType.Standard && val < 1_000f)
            {
                text.SetText("{0}", val);
                Show(sourcePos, color, scale, lifetime, floatSpeed, delay);
                return;
            }

            float v = val;
            int i = 0;
            while (i < Suffixes.Length - 1 && v >= 1_000f)
            {
                v /= 1_000f;
                i++;
            }
            string result = i == 0 ? v.ToString("0") : v.ToString("0.#") + Suffixes[i];

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
            text.text = content;
            Show(sourcePos, color, scale, lifetime, floatSpeed, delay);
        }

        private void Show(Vector3 sourcePos, Color color, float scale, float lifetime, float floatSpeed, float delay)
        {
            mainCam = mainCam != null ? mainCam : Camera.main;

            worldPos = sourcePos + new Vector3(
                Random.Range(-maxRandomOffset.x, maxRandomOffset.x),
                Random.Range(-maxRandomOffset.y, maxRandomOffset.y),
                0f
            );

            if (mainCam != null) transform.position = lastScreenPos = mainCam.WorldToScreenPoint(worldPos);

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
            if ((screenPos - lastScreenPos).sqrMagnitude >= 1f)
            {
                transform.position = screenPos;
                lastScreenPos = screenPos;
            }

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
