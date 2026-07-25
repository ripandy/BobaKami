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
            modes = InputModePolicy.AvailableModes(faceTrackingAvailable.Value);
            
            var modeSubscription = inputMode.Subscribe(Refresh);
            var buttonSubscription = button.OnClickAsObservable().Subscribe(_ => ToggleNext());
            subscription = new CompositeDisposable(modeSubscription, buttonSubscription);
            
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
            label.text = newInputMode.ToLabelString();
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
