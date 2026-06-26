using System;
using R3;
using UnityEngine;

namespace BobaKami.Gameplay
{
    public class BeanFallOnlyCollision : MonoBehaviour
    {
        private IDisposable subscription;
        
        private void Start()
        {
            if (!TryGetComponent(out Rigidbody2D beanRigidbody) || !TryGetComponent(out Collider2D beanCollider)) return;
            
            subscription = Observable.EveryValueChanged(beanRigidbody, rb => rb.linearVelocityY < 0)
                .Subscribe(isFalling => beanCollider.enabled = isFalling);
        }
        
        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}