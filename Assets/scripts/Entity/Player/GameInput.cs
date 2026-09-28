using System.Collections.Generic;
using CrystalFlux.SettingsSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CrystalFlux.EntitySystem
{
    public static class GameInput
    {
        private static PlayerControls controls;
        private static readonly List<InputActionAsset> externalAssets = new();
        private static int playerMapRefs;
        private static int uiMapRefs;

        public static PlayerControls Controls
        {
            get
            {
                if (controls == null)
                {
                    controls = new PlayerControls();
                    ApplyOverrides();
                }
                return controls;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            controls = null;
            externalAssets.Clear();
            playerMapRefs = 0;
            uiMapRefs = 0;
        }

        public static void EnablePlayerMap()
        {
            if (playerMapRefs++ == 0) Controls.Player.Enable();
        }

        public static void DisablePlayerMap()
        {
            if (playerMapRefs > 0 && --playerMapRefs == 0 && controls != null) controls.Player.Disable();
        }

        public static void EnableUIMap()
        {
            if (uiMapRefs++ == 0) Controls.UI.Enable();
        }

        public static void DisableUIMap()
        {
            if (uiMapRefs > 0 && --uiMapRefs == 0 && controls != null) controls.UI.Disable();
        }

        public static void RegisterExternalAsset(InputActionAsset asset)
        {
            if (asset == null || externalAssets.Contains(asset)) return;

            externalAssets.Add(asset);
            ApplyTo(asset, GameSettings.Current.bindingOverridesJson);
        }

        public static void UnregisterExternalAsset(InputActionAsset asset)
        {
            if (asset == null) return;
            externalAssets.Remove(asset);
        }

        public static void ApplyOverrides()
        {
            string json = GameSettings.Current.bindingOverridesJson;

            if (controls != null) ApplyTo(controls.asset, json);

            for (int i = externalAssets.Count - 1; i >= 0; i--)
            {
                if (externalAssets[i] == null)
                {
                    externalAssets.RemoveAt(i);
                    continue;
                }
                ApplyTo(externalAssets[i], json);
            }
        }

        public static void SaveOverrides()
        {
            if (controls == null) return;

            GameSettings.Current.bindingOverridesJson = controls.asset.SaveBindingOverridesAsJson();
            ApplyOverrides();
            GameSettings.RaiseChanged();
        }

        public static void ResetAllBindings()
        {
            GameSettings.Current.bindingOverridesJson = "";
            ApplyOverrides();
            GameSettings.RaiseChanged();
        }

        private static void ApplyTo(InputActionAsset asset, string json)
        {
            if (asset == null) return;

            if (string.IsNullOrEmpty(json)) asset.RemoveAllBindingOverrides();
            else asset.LoadBindingOverridesFromJson(json);
        }
    }
}
