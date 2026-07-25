using System;
using R3;
using Soar.Variables;
using UnityEngine;

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Applies the selected input mode by activating its input-source GameObjects and
    /// raising the cross-scene <c>faceTrackingEnabled</c> flag (read by FaceTrackingAdapter
    /// and the Title standby prompt). The platform/Auto rules live in <see cref="InputModePolicy"/>.
    /// - FaceTracking: only the face flag is raised; pointer/key sources stay off.
    /// - Pointer: screen-normalized pointer input (touch, mouse, or pen).
    /// - KeyButton: discrete direction + button input (keyboard, gamepad).
    /// - PointerAndKeyButton: both non-face sources at once (desktop default).
    /// On start it coerces the persisted/default mode to one legal on this platform, so it
    /// owns the runtime mode state rather than the settings UI.
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
            // Also re-apply when availability changes: this object and FaceTrackingAdapter
            // both live in Core, so Auto may be resolved before availability is published.
            var modeSubscription = inputMode.Subscribe(Apply);
            var availabilitySubscription = faceTrackingAvailable.Subscribe(_ => Apply(inputMode.Value));
            subscription = new CompositeDisposable(modeSubscription, availabilitySubscription);

            // Owns the runtime mode state: correct an illegal persisted/default mode once
            // (assigning triggers Apply through the subscription above).
            inputMode.Value = InputModePolicy.Coerce(inputMode.Value, faceTrackingAvailable.Value);
        }

        private void Apply(InputModeEnum mode)
        {
            var resolved = InputModePolicy.Resolve(mode, faceTrackingAvailable.Value);

            pointerInput.SetActive(resolved is InputModeEnum.Pointer or InputModeEnum.PointerAndKeyButton);
            keyButtonInput.SetActive(resolved is InputModeEnum.KeyButton or InputModeEnum.PointerAndKeyButton);
            faceTrackingEnabled.Value = resolved == InputModeEnum.FaceTracking;
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
