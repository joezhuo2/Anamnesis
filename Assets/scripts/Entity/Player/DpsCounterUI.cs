using CrystalFlux.SettingsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalFlux.EntitySystem
{
    public class DpsCounterUI : MonoBehaviour
    {
        private const float RefreshInterval = 0.2f;

        private TextMeshProUGUI label;
        private float nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<DpsCounterUI>() != null) return;

            var go = new GameObject("DpsCounter", typeof(Canvas));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            go.AddComponent<DpsCounterUI>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(transform, false);

            var rt = (RectTransform)textGo.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
            rt.sizeDelta = new Vector2(320f, 70f);

            label = textGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = 22f;
            label.alignment = TextAlignmentOptions.TopRight;
            label.color = Color.white;
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            label.gameObject.SetActive(false);
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => DpsMeter.Reset();

        private void Update()
        {
            if (label == null) return;

            bool show = GameSettings.Current.showDpsCounter;
            if (label.gameObject.activeSelf != show) label.gameObject.SetActive(show);
            if (!show || Time.unscaledTime < nextRefresh) return;

            nextRefresh = Time.unscaledTime + RefreshInterval;
            label.text = $"DPS {NumberFormat.Abbrev(Mathf.RoundToInt(DpsMeter.Current))}\nPeak {NumberFormat.Abbrev(Mathf.RoundToInt(DpsMeter.Peak))}";
        }
    }
}
