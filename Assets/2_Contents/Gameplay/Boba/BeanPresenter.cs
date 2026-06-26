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
    public class BeanPresenter : MonoBehaviour, IBeanPresenter
    {
        [SerializeField] private SoarDictionary<int, GameObject> beans;
        [SerializeField] private GameObject beanPrefab;
        [SerializeField] private Transform launcher;
        [SerializeField] private Transform bobaContainer;
        [SerializeField] private float launchSpeed = 13.2f;
        [SerializeField] private float rotationAngle = 3f;

        [Header("Debug")]
        [SerializeField] private GameEvent<int> bittenBeanEvent;
        
        // TODO: To Unity's Official ObjectPool
        private readonly Stack<GameObject> beanPool = new();

        private const float VariationFactor = 0.25f;
        
        private IDisposable subscription;

        private void Start()
        {
            beans.Clear();
        }
        
        public async ValueTask<bool> Show(int id, DirectionEnum throwDirection, CancellationToken cancellationToken = default)
        {
            if (!beans.TryGetValue(id, out var bean))
            {
                if (!beanPool.TryPop(out bean))
                {
                    bean = Instantiate(beanPrefab, bobaContainer);
                }
                
                beans.Add(id, bean);
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
            
            bean.transform.SetLocalPositionAndRotation(launcherTransform.localPosition, default);
            bean.SetActive(true);
            
            if (!bean.TryGetComponent<Rigidbody2D>(out var beanRigidbody))
            {
                beanRigidbody = bean.AddComponent<Rigidbody2D>();
            }

            Vector2 direction = launcherTransform.up;
            beanRigidbody.AddForce(direction * launchSpeed, ForceMode2D.Impulse);
            
            var droppedTask = UniTask.WaitWhile(() => bean.transform.localPosition.y >= -1, cancellationToken: cancellationToken);
            var hiddenTask = UniTask.WaitWhile(() => bean.activeInHierarchy, cancellationToken: cancellationToken);
            
            // Dropped means the bean hit the ground, while hidden means the bean was eaten.
            var (canceled, result) = await UniTask.WhenAny(droppedTask, hiddenTask).SuppressCancellationThrow();

            return canceled || result == 0;
        }

        public void Hide(int id)
        {
            if (!beans.Remove(id, out var bean)) return;
            beanPool.Push(bean);
            bean.SetActive(false);
        }
        
        private void OnDestroy()
        {
            beanPool.Clear();
            subscription?.Dispose();
        }
    }
}