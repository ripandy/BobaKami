using System;
using System.Collections.Generic;
using Soar.Variables;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Core-scene adapter between the input-mode SOAR signals and the ARFaceManager
    /// (which lives inside the ARFaceDetectionOrigin prefab instance, hence located at
    /// runtime instead of a serialized cross-prefab reference).
    /// - Publishes whether face tracking is available on this device/platform.
    /// - Enables/disables face tracking (and hides spawned face trackables) on demand.
    /// - Publishes <see cref="faceTrackingEnabled"/>: face tracking is both available *and*
    ///   selected by the current setting (Auto resolving to it, or FaceTracking outright).
    ///   This adapter is its <b>single writer</b> precisely because it lives in Core and so
    ///   stays loaded on every scene — the Title standby prompt reads it while Title is up,
    ///   and no Gameplay-scene component is around then to keep it truthful.
    /// </summary>
    public class FaceTrackingAdapter : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private Variable<InputModeEnum> inputMode;

        [Header("Output")]
        [SerializeField] private Variable<bool> faceTrackingAvailable;
        [SerializeField] private Variable<bool> faceTrackingEnabled;

        private ARFaceManager faceManager;
        private IDisposable subscription;

        private void Start()
        {
            faceManager = FindAnyObjectByType<ARFaceManager>(FindObjectsInactive.Include);

            var descriptors = new List<XRFaceSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            faceTrackingAvailable.Value = faceManager != null && descriptors.Count > 0;

            subscription = inputMode.Subscribe(SetFaceTrackingActive);
            SetFaceTrackingActive(inputMode);
        }

        private void SetFaceTrackingActive(InputModeEnum mode)
        {
            var resolved = InputModePolicy.Resolve(mode, faceTrackingAvailable.Value);
            var active = resolved == InputModeEnum.FaceTracking && faceTrackingAvailable.Value;

            // Published before the faceManager guard: with no manager the answer is a definite
            // "not face tracking", and the prompt still needs to hear it (editor, non-AR builds).
            faceTrackingEnabled.Value = active;

            if (faceManager == null) return;

            faceManager.enabled = active;

            foreach (var face in faceManager.trackables)
            {
                face.gameObject.SetActive(faceManager.enabled);
            }
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
