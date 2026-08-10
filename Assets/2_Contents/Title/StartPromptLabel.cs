using System;
using Soar.Variables;
using TMPro;
using UnityEngine;

namespace BobaKami.MainMenu
{
    /// <summary>
    /// Keeps the Title standby prompt honest about how the player is expected to start:
    /// "Bite to Start!" when face tracking is the live input, "Tap to Start!" otherwise.
    /// Updates live, because the input-mode toggle sits in the settings overlay on this very
    /// screen — the label has to change under the player's thumb.
    /// <para>
    /// It <b>samples the value on Start</b> before subscribing, which is the whole reason this
    /// is a script rather than a <c>BoolUnityEventBinder</c>: the flag is written by
    /// <c>FaceTrackingAdapter</c> in the Core scene, and Core has finished loading before Title
    /// is added additively, so a listen-only binder never hears the assignment that decided the
    /// answer and is left showing whatever string was serialized.
    /// </para>
    /// </summary>
    public class StartPromptLabel : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private TMP_Text label;

        [Header("Source")]
        [Tooltip("FaceTrackingEnabledVariable — face tracking is both available and selected " +
                 "by the current input mode. Written by FaceTrackingAdapter in the Core scene.")]
        [SerializeField] private Variable<bool> faceTrackingEnabled;

        [Header("Wording")]
        [SerializeField] private string faceTrackingPrompt = "Bite to Start!";
        [SerializeField] private string pointerPrompt = "Tap to Start!";

        private IDisposable subscription;

        private void Start()
        {
            if (label == null || faceTrackingEnabled == null) return;

            Apply(faceTrackingEnabled.Value);
            subscription = faceTrackingEnabled.Subscribe(Apply);
        }

        private void Apply(bool faceTracking)
        {
            label.text = faceTracking ? faceTrackingPrompt : pointerPrompt;
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
