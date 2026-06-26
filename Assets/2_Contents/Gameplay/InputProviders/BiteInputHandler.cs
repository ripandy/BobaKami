using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using Soar.Collections;
using Soar.Events;
using Soar.Variables;
using UnityEngine;

namespace BobaKami.Gameplay
{
    public class BiteInputHandler : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private GameEvent<bool> mouthOpenEvent;
        [SerializeField] private Variable<float> mouthColliderEnableDuration;
        
        [Header("Output")]
        [SerializeField] private GameEvent<int> bittenBeanEvent;
        
        [Header("Dependencies")]
        [SerializeField] private SoarDictionary<int, GameObject> beans;
        [SerializeField] private Collider2D mouthCollider;

        private IDisposable subscription;

        private void Start()
        {
            mouthCollider.enabled = false;
            subscription = mouthOpenEvent.AsObservable().SubscribeAwait(CheckForBite, AwaitOperation.Drop);
        }
        
        private async ValueTask CheckForBite(bool opened, CancellationToken cancellationToken)
        {
            if (opened || mouthCollider.enabled) return;
            
            mouthCollider.enabled = true;
            
            await UniTask.Delay(TimeSpan.FromSeconds(mouthColliderEnableDuration), cancellationToken: cancellationToken);
            
            mouthCollider.enabled = false;
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.gameObject.CompareTag("Bean")) return;
            
            var pair = (beans as IDictionary<int, GameObject>).FirstOrDefault(pair => pair.Value == other.gameObject);
            if (pair.Value == null) return;
            
            bittenBeanEvent.Raise(pair.Key);
            mouthCollider.enabled = false;
        }
        
        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}