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
    /// </summary>
    public class FaceTrackingAdapter : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private Variable<bool> faceTrackingEnabled;

        [Header("Output")]
        [SerializeField] private Variable<bool> faceTrackingAvailable;

        private ARFaceManager faceManager;
        private IDisposable subscription;

        private void Start()
        {
            faceManager = FindAnyObjectByType<ARFaceManager>(FindObjectsInactive.Include);

            var descriptors = new List<XRFaceSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            faceTrackingAvailable.Value = faceManager != null && descriptors.Count > 0;

            subscription = faceTrackingEnabled.Subscribe(SetFaceTrackingActive);
            SetFaceTrackingActive(faceTrackingEnabled.Value);
        }

        private void SetFaceTrackingActive(bool active)
        {
            if (faceManager == null) return;

            faceManager.enabled = active && faceTrackingAvailable.Value;

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
