using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using BobaKami;
using BobaKami.Interfaces;
using Soar.Collections;
using Soar.Events;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BobaKami.Gameplay
{
    public class BobaPresenter : MonoBehaviour, IBobaPresenter
    {
        [SerializeField] private SoarDictionary<int, GameObject> bobas;
        [SerializeField] private GameObject bobaPrefab;
        [SerializeField] private Transform launcher;
        [SerializeField] private Transform bobaContainer;
        [SerializeField] private float launchSpeed = 13.2f;
        [SerializeField] private float rotationAngle = 3f;

        [Header("Debug")]
        [SerializeField] private GameEvent<int> bittenBobaEvent;
        
        // TODO: To Unity's Official ObjectPool
        private readonly Stack<GameObject> bobaPool = new();

        private const float VariationFactor = 0.25f;
        
        private IDisposable subscription;

        private void Start()
        {
            bobas.Clear();
        }
        
        public async ValueTask<bool> Show(int id, DirectionEnum throwDirection, CancellationToken cancellationToken = default)
        {
            if (!bobas.TryGetValue(id, out var boba))
            {
                if (!bobaPool.TryPop(out boba))
                {
                    boba = Instantiate(bobaPrefab, bobaContainer);
                }
                
                bobas.Add(id, boba);
            }
            
            var angle = throwDirection switch
            {
                DirectionEnum.Left => -rotationAngle,
                DirectionEnum.Right => rotationAngle,
                _ => 0
            };
            
            var launcherTransform = launcher.transform;
            var variationPosition = new Vector2(Random.Range(-VariationFactor, VariationFactor), launcherTransform.localPosition.y);
            var variationRotation = Quaternion.Euler(0, 0, angle + Random.Range(-VariationFactor, VariationFactor));
            launcherTransform.SetLocalPositionAndRotation(variationPosition, variationRotation); 
            
            boba.transform.SetLocalPositionAndRotation(launcherTransform.localPosition, default);
            boba.SetActive(true);
            
            if (!boba.TryGetComponent<Rigidbody2D>(out var bobaRigidbody))
            {
                bobaRigidbody = boba.AddComponent<Rigidbody2D>();
            }

            Vector2 direction = launcherTransform.up;
            bobaRigidbody.AddForce(direction * launchSpeed, ForceMode2D.Impulse);
            
            var droppedTask = UniTask.WaitWhile(() => boba.transform.localPosition.y >= -1, cancellationToken: cancellationToken);
            var hiddenTask = UniTask.WaitWhile(() => boba.activeInHierarchy, cancellationToken: cancellationToken);
            
            // Dropped means the boba hit the ground, while hidden means the boba was eaten.
            var (canceled, result) = await UniTask.WhenAny(droppedTask, hiddenTask).SuppressCancellationThrow();

            return canceled || result == 0;
        }

        public void Hide(int id)
        {
            if (!bobas.Remove(id, out var boba)) return;
            bobaPool.Push(boba);
            boba.SetActive(false);
        }
        
        private void OnDestroy()
        {
            bobaPool.Clear();
            subscription?.Dispose();
        }
    }
}