using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using BobaKami.Interfaces;
using LitMotion;
using UnityEngine;

namespace BobaKami.Gameplay
{
    public class IntroPresenter : MonoBehaviour, IIntroPresenter
    {
        [SerializeField] private Transform[] introObjects;

        private bool firstRun = true;
        
        public async ValueTask Show(CancellationToken cancellationToken = default)
        {
            const float duration = 0.5f;
            const int delayFactor = 100;

            var tasks = introObjects.Select((obj, i) =>
            {
                var startScale = obj.localScale;
                return LMotion.Create(Vector3.zero, startScale, duration)
                    .WithDelay(delayFactor * i)
                    .WithEase(Ease.InBounce)
                    .Bind(newScale => obj.localScale = newScale)
                    .ToUniTask(destroyCancellationToken);
            });
            
            if (firstRun)
            {
                tasks = tasks.Prepend(UniTask.Delay(1000, cancellationToken: cancellationToken));
                firstRun = false;
            }

            await UniTask.WhenAll(tasks);
        }
    }
}