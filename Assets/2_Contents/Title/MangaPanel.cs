using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using Soar.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DelayType = LitMotion.DelayType;

namespace BobaKami.MainMenu
{
    /// <summary>
    /// The intro cutscene, played <b>after</b> <see cref="StandbyUI"/> takes the start input and
    /// immediately before gameplay, so it always has an audience. It animates the panels (any
    /// button press or touch skips), then raises the next app state. This object must start
    /// <b>inactive</b> in the scene; StandbyUI activates it on handoff.
    /// </summary>
    public class MangaPanel : MonoBehaviour
    {
        [Header("Manga Panel")]
        [SerializeField] private CanvasGroup mangaGroup;
        [SerializeField] private Image[] mangaPanels;

        [Header("Handoff")]
        [SerializeField] private GameEvent<string> setNextStateEvent;
        [SerializeField] private string nextState = "Gameplay";

        // The press that started the game is still fresh when this object wakes up — StandbyUI
        // advances on the *release* edge, so a tap or Space press lands moments earlier. Without
        // this window the manga would instantly skip itself on the very input that summoned it.
        private const float SkipGraceSeconds = 0.5f;

        private async UniTaskVoid Start()
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            var ignoreSkipUntil = Time.unscaledTime + SkipGraceSeconds;

            // Skip the manga animation on any button press OR a touch tap.
            using var subscription = AnyPressObservable()
                .Where(_ => Time.unscaledTime >= ignoreSkipUntil)
                .Take(1)
                .Subscribe(_ => CancelAndDispose());

            try
            {
                await AnimatePanels(cts.Token).SuppressCancellationThrow();
            }
            finally
            {
                CancelAndDispose();
            }

            if (destroyCancellationToken.IsCancellationRequested) return;

            // Last step in the Title scene: StandbyUI already took the start input.
            setNextStateEvent.Raise(nextState);

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
            
        private static Observable<Unit> AnyPressObservable()
        {
            var anyButton = InputSystem.onAnyButtonPress
                .ToObservable()
                .AsUnitObservable();
                
            var touch = Observable.EveryUpdate()
                .Where(_ => Touchscreen.current != null &&
                            Touchscreen.current.press.wasPressedThisFrame);
                
            return anyButton.Merge(touch);
        }
    }
}