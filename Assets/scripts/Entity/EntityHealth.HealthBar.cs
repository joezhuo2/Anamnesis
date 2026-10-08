using System;
using System.Collections;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SettingsSystem;
using CrystalFlux.StatusEffectSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public partial class EntityHealth
    {
        public void SetBarHidden(bool v)
        {
            if (barHidden == v) return;

            barHidden = v;
            lastBarWorldPos = new Vector3(float.NaN, float.NaN, float.NaN);
        }

        internal static Canvas ResolveHealthBarCanvas()
        {
            if (sharedCanvas != null && sharedCanvas.isActiveAndEnabled) return sharedCanvas;

            sharedCanvas = null;

            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
            foreach (var c in canvases)
            {
                if (c == null || !c.isActiveAndEnabled) continue;
                if (c.name != HealthBarCanvasName) continue;
                if (c.renderMode == RenderMode.WorldSpace) continue;

                sharedCanvas = c.rootCanvas != null ? c.rootCanvas : c;
                return sharedCanvas;
            }

            var go = new GameObject(HealthBarCanvasName, typeof(Canvas), typeof(CanvasScaler));
            var created = go.GetComponent<Canvas>();
            created.renderMode = RenderMode.ScreenSpaceOverlay;
            created.sortingOrder = healthBarSortingOrder;

            CopyScalerSettings(go.GetComponent<CanvasScaler>(), canvases);

            sharedCanvas = created;
            return sharedCanvas;
        }

        private static void CopyScalerSettings(CanvasScaler dst, Canvas[] canvases)
        {
            if (dst == null) return;

            dst.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            dst.scaleFactor = 1f;

            foreach (var c in canvases)
            {
                if (c == null || c.renderMode == RenderMode.WorldSpace) continue;
                if (!c.TryGetComponent<CanvasScaler>(out var src)) continue;

                dst.uiScaleMode = src.uiScaleMode;
                dst.scaleFactor = src.scaleFactor;
                dst.referenceResolution = src.referenceResolution;
                dst.screenMatchMode = src.screenMatchMode;
                dst.matchWidthOrHeight = src.matchWidthOrHeight;
                dst.referencePixelsPerUnit = src.referencePixelsPerUnit;
                return;
            }
        }

        private void InitializeHealthBar()
        {
            if (healthBarPrefab == null || barRetired || !BarsAllowed) return;

            cachedCanvas = ResolveHealthBarCanvas();

            if (cachedCanvas == null) return;

            healthBarInstance = PrefabPool.Acquire(healthBarPrefab, cachedCanvas.transform);
            EnsureOwnCanvas(healthBarInstance);

            if (healthBarTextPrefab != null)
            {
                healthBarTextInstance = PrefabPool.Acquire(healthBarTextPrefab, cachedCanvas.transform);
                EnsureOwnCanvas(healthBarTextInstance);
            }

            barCurHp = int.MinValue;
            barMaxHp = int.MinValue;
            barOverhealth = int.MinValue;
            lastBarWorldPos = new Vector3(float.NaN, float.NaN, float.NaN);
            lastBarCamPos = new Vector3(float.NaN, float.NaN, float.NaN);
            RefreshHealthBar();
        }

        private static void EnsureOwnCanvas(Component c)
        {
            if (c == null || c.TryGetComponent<Canvas>(out _)) return;
            c.gameObject.AddComponent<Canvas>();
        }

        private void RefreshHealthBar()
        {
            if (healthBarInstance == null) return;

            int cur = CurHp;
            int max = MaxHp;
            int over = Mathf.FloorToInt(Overhealth);
            if (cur == barCurHp && max == barMaxHp && over == barOverhealth) return;

            barCurHp = cur;
            barMaxHp = max;
            barOverhealth = over;

            healthBarInstance.maxValue = max;
            healthBarInstance.value = cur;

            if (healthBarTextInstance != null)
                healthBarTextInstance.text = NumberFormat.Bar(cur, max, over);
        }

        private void MoveHealthBar()
        {
            if (healthBarInstance == null && (healthBarPrefab == null || barRetired || !BarsAllowed)) return;

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null || !IsAlive) return;

            if (!BarsAllowed)
            {
                if (healthBarInstance != null || healthBarTextInstance != null)
                {
                    PrefabPool.Release(ref healthBarInstance);
                    PrefabPool.Release(ref healthBarTextInstance);
                }
                return;
            }

            if (healthBarInstance == null || cachedCanvas == null || !cachedCanvas.isActiveAndEnabled)
            {
                if (healthBarPrefab == null || barRetired || ResolveHealthBarCanvas() == null) return;

                PrefabPool.Release(ref healthBarInstance);
                PrefabPool.Release(ref healthBarTextInstance);

                InitializeHealthBar();
                if (healthBarInstance == null) return;
            }

            Vector3 worldPos = transform.position;
            Vector3 camPos = mainCamera.transform.position;

            if ((worldPos - lastBarWorldPos).sqrMagnitude < barMoveEpsilonSqr && (camPos - lastBarCamPos).sqrMagnitude < barMoveEpsilonSqr)
                return;

            lastBarWorldPos = worldPos;
            lastBarCamPos = camPos;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos + healthBarOffset);
            bool visible = screenPos.z > 0f && !barHidden;

            if (healthBarInstance.gameObject.activeSelf != visible)
                healthBarInstance.gameObject.SetActive(visible);

            if (healthBarTextInstance != null && healthBarTextInstance.gameObject.activeSelf != visible)
                healthBarTextInstance.gameObject.SetActive(visible);

            if (!visible) return;

            screenPos.z = 0f;
            healthBarInstance.transform.position = screenPos;

            if (healthBarTextInstance != null)
                healthBarTextInstance.transform.position = screenPos + healthBarOffset;
        }
    }
}
