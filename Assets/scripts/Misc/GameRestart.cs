using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalFlux.SettingsSystem
{
    public static class GameRestart
    {
        private static bool restarting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => restarting = false;

        public static bool IsRestarting => restarting;

        public static void ToHomeScreen()
        {
            if (restarting) return;
            restarting = true;

            MenuPause.ResetDepth();
            Time.timeScale = 1f;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        public static void QuitGame()
        {
            if (restarting) return;

            MenuPause.ResetDepth();
            Time.timeScale = 1f;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            restarting = false;

            MenuPause.ResetDepth();
            Time.timeScale = 1f;
        }
    }
}
