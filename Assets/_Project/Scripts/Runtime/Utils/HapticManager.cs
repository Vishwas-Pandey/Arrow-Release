using UnityEngine;

namespace ReleaseTheArrow.Utils
{
    /// Thin wrapper over Unity's cross-platform vibration call, gated by the player's setting.
    public static class HapticManager
    {
        public static bool VibrationOn = true;

        public static void LightTap()
        {
            if (VibrationOn) Handheld.Vibrate();
        }

        public static void Notify()
        {
            if (VibrationOn) Handheld.Vibrate();
        }
    }
}
