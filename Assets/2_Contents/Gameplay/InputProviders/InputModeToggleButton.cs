using System;
using System.Collections.Generic;
using R3;
using Soar.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BobaKami.Gameplay
{
    public class InputModeToggleButton : MonoBehaviour
    {
        [SerializeField] private Variable<InputModeEnum> inputMode;
        [SerializeField] private Variable<bool> faceTrackingAvailable;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;
        
        private IList<InputModeEnum> modes;
        private IDisposable subscription;

        private void Start()
        {
            modes = InputModeController.AvailableModes(faceTrackingAvailable.Value);
            
            var modeSubscription = inputMode.Subscribe(Refresh);
            var buttonSubscription = button.OnClickAsObservable().Subscribe(_ => ToggleNext());
            subscription = new CompositeDisposable(modeSubscription, buttonSubscription);
            
            inputMode.Value = modes[0];
            button.interactable = modes.Count > 1;
            Refresh(inputMode.Value);
        }
        
        private void ToggleNext()
        {
            var index = modes.IndexOf(inputMode.Value);
            inputMode.Value = modes[(index + 1) % modes.Count];
        }

        private void Refresh(InputModeEnum newInputMode)
        {
            label.text = newInputMode switch
            {
                InputModeEnum.Auto => "Auto",
                InputModeEnum.FaceTracking => "Face Tracking",
                InputModeEnum.Pointer => PointerText,
                InputModeEnum.KeyButton => "Key Button",
                InputModeEnum.PointerAndKeyButton => $"{PointerText} And Key Button",
                _ => throw new ArgumentOutOfRangeException(nameof(newInputMode), newInputMode, null)
            };
        }

        private static string PointerText =>
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            "Touch Screen";
#else
            "Mouse/Trackpad";
#endif
        

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
