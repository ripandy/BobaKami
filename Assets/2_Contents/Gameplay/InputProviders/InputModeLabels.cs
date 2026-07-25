using System;

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Presentation-only mapping from <see cref="InputModeEnum"/> to a player-facing,
    /// platform-worded label. Kept separate from <see cref="InputModeToggleButton"/> so the
    /// widget owns only cycling/interaction and label wording has a single reason to change.
    /// </summary>
    internal static class InputModeLabels
    {
        public static string ToLabelString(this InputModeEnum mode) => mode switch
        {
            InputModeEnum.Auto => "Auto",
            InputModeEnum.FaceTracking => "Face Tracking",
            InputModeEnum.Pointer => PointerText,
            InputModeEnum.KeyButton => "Key Button",
            InputModeEnum.PointerAndKeyButton => $"{PointerText} And Key Button",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        private static string PointerText =>
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            "Touch Screen";
#else
            "Mouse/Trackpad";
#endif
    }
}

