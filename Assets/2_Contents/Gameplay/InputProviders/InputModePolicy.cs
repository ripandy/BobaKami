using System.Collections.Generic;

namespace BobaKami.Gameplay
{
    internal static class InputModePolicy
    {
        public static InputModeEnum Resolve(InputModeEnum mode, bool faceTrackingAvailable)
        {
            return mode == InputModeEnum.Auto
                ? ResolveAuto(faceTrackingAvailable)
                : mode;
            
            static InputModeEnum ResolveAuto(bool faceTrackingAvailable)
            {
#if UNITY_IOS && !UNITY_EDITOR
                return faceTrackingAvailable ? InputModeEnum.FaceTracking : InputModeEnum.PointerAndKeyButton;
#else
                return InputModeEnum.PointerAndKeyButton;
#endif
            }
        }

        /// <summary>The platform-legal modes, most-preferred first, with Auto prepended when more than one exists.</summary>
        public static IList<InputModeEnum> AvailableModes(bool faceTrackingAvailable)
        {
            var modes = new List<InputModeEnum>();
#if UNITY_IOS
            if (faceTrackingAvailable)
                modes.Add(InputModeEnum.FaceTracking);
#endif

            modes.Add(InputModeEnum.Pointer);

#if !UNITY_IOS && !UNITY_ANDROID || UNITY_EDITOR
            modes.Add(InputModeEnum.KeyButton);
            modes.Add(InputModeEnum.PointerAndKeyButton);
#endif

            if (modes.Count > 1)
                modes.Insert(0, InputModeEnum.Auto);

            return modes;
        }

        /// <summary>Returns <paramref name="mode"/> if it is legal on this platform, otherwise the preferred fallback.</summary>
        public static InputModeEnum Coerce(InputModeEnum mode, bool faceTrackingAvailable)
        {
            var modes = AvailableModes(faceTrackingAvailable);
            return modes.Contains(mode) ? mode : modes[0];
        }
    }
}

