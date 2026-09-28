using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    public class GameSettingsLifecycle : MonoBehaviour
    {
        private static GameSettingsLifecycle instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;

            _ = GameSettings.Current;

            var go = new GameObject(nameof(GameSettingsLifecycle));
            instance = go.AddComponent<GameSettingsLifecycle>();
            DontDestroyOnLoad(go);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) GameSettings.Save();
        }

        private void OnApplicationQuit() => GameSettings.Save();

        private void OnDestroy()
        {
            if (ReferenceEquals(instance, this)) instance = null;
        }
    }
}
