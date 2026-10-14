using System;
using UnityEngine;

namespace CrystalFlux.SettingsSystem
{
    public static class UIModal
    {
        public static Func<bool> IsTypingCheck;
        public static Func<bool> TryCloseTopCheck;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsTypingCheck = null;
            TryCloseTopCheck = null;
        }

        public static bool IsTyping => IsTypingCheck != null && IsTypingCheck();

        public static bool TryCloseTop() => TryCloseTopCheck != null && TryCloseTopCheck();
    }
}
