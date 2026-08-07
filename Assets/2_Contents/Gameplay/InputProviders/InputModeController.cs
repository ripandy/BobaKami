using System;
using R3;
using Soar.Variables;
using UnityEngine;

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Applies the selected input mode by activating its input-source GameObjects. The
    /// platform/Auto rules live in <see cref="InputModePolicy"/>.
    /// <para>
    /// This component lives on <c>AlternativeInput</c> in <b>Gameplay.unity</b>, alongside the
    /// pointer/key input sources it toggles — so it does not exist while Title is up. The
    /// cross-scene <c>FaceTrackingEnabledVariable</c> is therefore written by
    /// <see cref="FaceTrackingAdapter"/> (Core scene), not here: this component used to write it
    /// too, which left the Title standby prompt reading a flag last set by the previous
    /// Gameplay session.
    /// </para>
    /// - FaceTracking: pointer/key sources stay off; the face session is owned by FaceTrackingAdapter.
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

        [Header("Mode Objects")]
        [SerializeField] private GameObject pointerInput;
        [SerializeField] private GameObject keyButtonInput;

        private IDisposable subscription;

        private void Start()
        {
            // Also re-apply when availability changes: FaceTrackingAdapter publishes it from
            // Core, so Auto may be resolved here before that value has landed.
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
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
