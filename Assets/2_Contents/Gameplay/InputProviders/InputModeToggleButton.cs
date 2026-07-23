using System;
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
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;
        private IDisposable subscription;

        private void Start()
        {
            var modeSubscription = inputMode.Subscribe(Refresh);
            var buttonSubscription = button.OnClickAsObservable().Subscribe(_ => ToggleNext());
            subscription = new CompositeDisposable(modeSubscription, buttonSubscription);
            Refresh(inputMode.Value);
        }
        
        private void ToggleNext()
        {
            var next = inputMode.Value switch
            {
                InputModeEnum.Auto => InputModeEnum.FaceTracking,
                InputModeEnum.FaceTracking => InputModeEnum.Pointer,
                _ => InputModeEnum.Auto
            };
            inputMode.Value = next;
        }

        private void Refresh(InputModeEnum newInputMode)
        {
            label.text = newInputMode switch
            {
                InputModeEnum.Auto => "Auto",
                InputModeEnum.FaceTracking => "Face Tracking",
                InputModeEnum.Pointer => "Pointer",
                _ => throw new ArgumentOutOfRangeException(nameof(newInputMode), newInputMode, null)
            };
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
