using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using Soar.Events;
using UnityEngine;
using UnityEngine.UI;
using DelayType = LitMotion.DelayType;

namespace BobaKami.MainMenu
{
    public class MangaPanel : MonoBehaviour
    {
        [SerializeField] private GameEvent<string> setNextStateEvent;
        [SerializeField] private string nextState = "Gameplay";
        [SerializeField] private Image[] mangaPanels;

        private async void Start()
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            
            using var subscription = Observable.EveryValueChanged(this, _ => Input.anyKeyDown)
                .Skip(1)
                .Subscribe(_ => cts.Cancel());
            
            await AnimatePanels(cts.Token).SuppressCancellationThrow();
            
            setNextStateEvent.Raise(nextState);
        }

        private async UniTask AnimatePanels(CancellationToken token = default)
        {
            foreach (var panel in mangaPanels)
            {
                UpdateAlpha(panel, 0);
            }
            
            const float duration = 0.8f;
            
            var panelBase = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.InBack)
                .Bind(alpha => UpdateAlpha(mangaPanels[0], alpha));
            
            var panel1 = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.InBack)
                .WithDelay(duration)
                .Bind(alpha => UpdateAlpha(mangaPanels[1], alpha));
            
            var panelMidFadeIn = LMotion.Create(0f, 1f, duration)
                .WithLoops(2, LoopType.Yoyo)
                .WithEase(Ease.InBack)
                .WithDelay(duration, delayType: DelayType.EveryLoop)
                .Bind(alpha => UpdateAlpha(mangaPanels[2], alpha));
            
            var panel2 = LMotion.Create(0f, 1f, duration).WithEase(Ease.InBack)
                .WithDelay(duration * 2)
                .Bind(alpha => UpdateAlpha(mangaPanels[3], alpha));
            
            var panel3 = LMotion.Create(0f, 1f, duration).WithEase(Ease.InBack)
                .WithDelay(duration)
                .Bind(alpha => UpdateAlpha(mangaPanels[4], alpha));
            
            var panel4 = LMotion.Create(0f, 1f, duration).WithEase(Ease.InBack)
                .WithDelay(duration)
                .Bind(alpha => UpdateAlpha(mangaPanels[5], alpha));
            
            await LSequence.Create()
                .Append(panelBase)
                .Append(panel1)
                .Append(panelMidFadeIn)
                .Join(panel2)
                .Append(panel3)
                .Append(panel4)
                .AppendInterval(duration)
                .Run()
                .ToUniTask(CancelBehavior.Complete, cancellationToken: token);
        }
        
        private void UpdateAlpha(Image panel, float alpha)
        {
            var color = panel.color;
            color.a = alpha;
            panel.color = color;
        }
    }
}