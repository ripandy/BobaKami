using System;
using R3;
using UnityEngine;

namespace BobaKami.Gameplay
{
    public class BobaFallOnlyCollision : MonoBehaviour
    {
        private IDisposable subscription;
        
        private void Start()
        {
            if (!TryGetComponent(out Rigidbody2D bobaRigidbody) || !TryGetComponent(out Collider2D bobaCollider)) return;
            
            subscription = Observable.EveryValueChanged(bobaRigidbody, rb => rb.linearVelocityY < 0)
                .Subscribe(isFalling => bobaCollider.enabled = isFalling);
        }
        
        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}