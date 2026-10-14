using CrystalFlux.SaveSystem;
using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    public static class SaveBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Register()
        {
            RunSession.RequestHomeScreen = GameRestart.ToHomeScreen;
            RunSession.PushPause = MenuPause.Push;
            RunSession.PopPause = MenuPause.Pop;
            RunSession.IsPlayerDead = () => DeathScreenUI.IsPlayerDead;
        }
    }
}
