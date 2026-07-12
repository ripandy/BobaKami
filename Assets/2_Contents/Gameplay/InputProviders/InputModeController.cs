using System;
using Soar.Variables;
using UnityEngine;

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Activates the input sources for the selected mode:
    /// - FaceTracking: raises the cross-scene faceTrackingEnabled flag consumed by
    ///   FaceTrackingAdapter in Core; pointer/key sources stay off.
    /// - Pointer: screen-normalized pointer input (touch, mouse, or pen via the
    ///   Input System's Pointer layout).
    /// - KeyButton: discrete direction + button input (keyboard, gamepad).
    /// - PointerAndKeyButton: both non-face sources at once (desktop default).
    /// Auto resolves to FaceTracking when available on iOS, else PointerAndKeyButton —
    /// pointer and key/button actions are per-device and inert when the device is absent.
    /// </summary>
    public class InputModeController : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private Variable<InputModeEnum> inputMode;
        [SerializeField] private Variable<bool> faceTrackingAvailable;

        [Header("Output")]
        [SerializeField] private Variable<bool> faceTrackingEnabled;

        [Header("Mode Objects")]
        [SerializeField] private GameObject pointerInput;
        [SerializeField] private GameObject keyButtonInput;

        private IDisposable subscription;

        private void Start()
        {
            subscription = inputMode.Subscribe(Apply);
            Apply(inputMode.Value);
        }

        private void Apply(InputModeEnum mode)
        {
            var resolved = mode == InputModeEnum.Auto ? Resolve() : mode;

            faceTrackingEnabled.Value = resolved == InputModeEnum.FaceTracking;
            pointerInput.SetActive(resolved is InputModeEnum.Pointer or InputModeEnum.PointerAndKeyButton);
            keyButtonInput.SetActive(resolved is InputModeEnum.KeyButton or InputModeEnum.PointerAndKeyButton);
        }

        private InputModeEnum Resolve()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return faceTrackingAvailable.Value ? InputModeEnum.FaceTracking : InputModeEnum.PointerAndKeyButton;
#else
            return InputModeEnum.PointerAndKeyButton;
#endif
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
