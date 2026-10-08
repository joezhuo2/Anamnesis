using CrystalFlux.SettingsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalFlux.EntitySystem
{
    public class DpsCounterUI : MonoBehaviour
    {
        private const float RefreshInterval = 0.2f;

        public TextMeshProUGUI label;

        private float nextRefresh;

        private void Awake()
        {
            if (label == null) TryGetComponent(out label);
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => DpsMeter.Reset();

        private void Update()
        {
            if (label == null) return;

            bool show = GameSettings.Current.showDpsCounter;
            if (label.enabled != show) label.enabled = show;
            if (!show || Time.unscaledTime < nextRefresh) return;

            nextRefresh = Time.unscaledTime + RefreshInterval;
            label.text = $"DPS {NumberFormat.Abbrev(Mathf.RoundToInt(DpsMeter.Current))}";
        }
    }
}
