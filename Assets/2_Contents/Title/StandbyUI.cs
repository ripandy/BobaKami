using System;
using Cysharp.Threading.Tasks;
using BobaKami.Gameplay;
using R3;
using Soar.Events;
using Soar.Variables;
using UnityEngine;

namespace BobaKami.MainMenu
{
    /// <summary>
    /// Title "press to start" gate. Activated by <see cref="MangaPanel"/> once the manga
    /// animation completes, it waits for the game's bite semantic — a <c>mouthOpenEvent</c>
    /// open→close edge, raised alike by a face bite, a tap release, and a Space/pad release —
    /// then advances the app state. This object must start inactive in the scene so its
    /// Start (and thus the input wait) does not run until MangaPanel hands off.
    /// </summary>
    public class StandbyUI : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private GameEvent<string> setNextStateEvent;
        [SerializeField] private string nextState = "Gameplay";

        [Header("Start Input")]
        [SerializeField] private GameEvent<bool> mouthOpenEvent;
        [SerializeField] private Variable<InputModeEnum> inputMode;

        // Swallows the release edge left over from the animation-skip press and from
        // the tap that operates the input-mode toggle button.
        private const float StartInputGraceSeconds = 0.5f;
        private float ignoreStartUntil;

        private async UniTaskVoid Start()
        {
            ignoreStartUntil = Time.unscaledTime + StartInputGraceSeconds;
            using var toggleSubscription = inputMode == null
                ? null
                : inputMode.Subscribe(_ => ignoreStartUntil = Time.unscaledTime + StartInputGraceSeconds);

            try
            {
                await mouthOpenEvent.AsObservable()
                    .FirstAsync(open => !open && Time.unscaledTime >= ignoreStartUntil,
                        cancellationToken: destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            setNextStateEvent.Raise(nextState);
        }
    }
}
