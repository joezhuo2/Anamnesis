using CrystalFlux.SettingsSystem;
using UnityEngine;

namespace CrystalFlux.SaveSystem
{
    public static class SaveUiModal
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Register()
        {
            UIModal.IsTypingCheck = () => SaveNamePromptUI.IsTyping;
            UIModal.TryCloseTopCheck = TryCloseTop;
        }

        public static bool AnyOpen
            => ConfirmDialogUI.Instance != null && ConfirmDialogUI.Instance.IsOpen
            || SaveNamePromptUI.Instance != null && SaveNamePromptUI.Instance.IsOpen;

        public static bool TryCloseTop()
        {
            if (ConfirmDialogUI.Instance != null && ConfirmDialogUI.Instance.IsOpen)
            {
                ConfirmDialogUI.Instance.Close();
                return true;
            }

            if (SaveNamePromptUI.Instance != null && SaveNamePromptUI.Instance.IsOpen)
            {
                SaveNamePromptUI.Instance.Close();
                return true;
            }

            return false;
        }
    }
}
