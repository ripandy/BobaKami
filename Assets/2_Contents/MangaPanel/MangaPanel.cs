using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using DelayType = LitMotion.DelayType;

namespace BobaKami.MainMenu
{
    public class MangaPanel : MonoBehaviour
    {
        [SerializeField] private GameObject titleScreenObject;
        [SerializeField] private CanvasGroup mangaGroup;
        [SerializeField] private Image[] mangaPanels;

        private async UniTaskVoid Start()
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            using var subscription = InputSystem.onAnyButtonPress.CallOnce(_ => CancelAndDispose());
            
            titleScreenObject.SetActive(false);
            
            try
            {
                await AnimatePanels(cts.Token).SuppressCancellationThrow();
                titleScreenObject.SetActive(true);
                gameObject.SetActive(false);
            }
            finally
            {
                CancelAndDispose();
            }

            void CancelAndDispose()
            {
                if (cts.IsCancellationRequested) return;
                cts.Cancel();
                cts.Dispose();
            }
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
            
            var panelBaseOut = LMotion.Create(1f, 0f, duration)
                .WithEase(Ease.OutBack)
                .Bind(alpha => mangaGroup.alpha = alpha);
            
            await LSequence.Create()
                .Append(panelBase)
                .Append(panel1)
                .Append(panelMidFadeIn)
                .Join(panel2)
                .Append(panel3)
                .Append(panel4)
                .AppendInterval(duration * 2)
                .Append(panelBaseOut)
                .Run()
                .ToUniTask(CancelBehavior.Complete, cancellationToken: token);
        }
        
        private static void UpdateAlpha(Image panel, float alpha)
        {
            var color = panel.color;
            color.a = alpha;
            panel.color = color;
        }
    }
}