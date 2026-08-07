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
    /// Title "press to start" gate, and the <b>first</b> thing the Title scene shows. It waits
    /// for the game's bite semantic — a <c>mouthOpenEvent</c> open→close edge, raised alike by a
    /// face bite, a tap release, and a Space/pad release — then hands off to
    /// <see cref="MangaPanel"/>, which plays the intro and advances the app state from there.
    /// <para>
    /// The manga deliberately runs <i>after</i> this gate rather than before it: on the old
    /// order the Title scene reloaded straight into the manga, so at a booth it played to an
    /// empty machine while the previous player was walking away, and whoever arrived next only
    /// ever saw the idle screen. Now it plays to someone who just asked to start.
    /// </para>
    /// This object must start <b>active</b> in the scene, and <see cref="mangaIntro"/> must be a
    /// sibling rather than a child, since this GameObject disables itself on handoff.
    /// </summary>
    public class StandbyUI : MonoBehaviour
    {
        [Header("Handoff")]
        [Tooltip("Activated once the start input arrives; MangaPanel plays the intro and raises " +
                 "the next app state from there. Must be a sibling, not a child, of this object.")]
        [SerializeField] private GameObject mangaIntro;

        [Header("Start Input")]
        [SerializeField] private GameEvent<bool> mouthOpenEvent;
        [SerializeField] private Variable<InputModeEnum> inputMode;

        // Swallows the release edge left over from the tap that operates the input-mode toggle
        // button, and from the press that skipped the Core splash — this gate now runs the moment
        // Title loads rather than ~11 s later behind the manga, so that press is still in flight.
        private const float StartInputGraceSeconds = 0.5f;
        private float ignoreStartUntil;

        private async UniTaskVoid Start()
        {
            // Booth hygiene: InputModeVariable is a ScriptableObject, so it survives
            // resetAppCommand's LoadScene("Core"). Without this, one player switching to Touch
            // leaves every player after them in Touch until the app is force-killed. Resetting
            // here (rather than on app quit) means the machine re-arms itself between visitors.
            if (inputMode != null) inputMode.Value = InputModeEnum.Auto;

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
            catch (Exception exception)
                when (exception is OperationCanceledException or InvalidOperationException)
            {
                // OperationCanceledException: destroyCancellationToken fired.
                // InvalidOperationException("Sequence contains no elements"): R3's FirstAsync
                // throws this when the SOAR observable *completes* without ever matching, which
                // is what PlayMode stop and scene unload look like. Both mean the same thing —
                // no start input is coming — but only the first was being caught, so stopping
                // PlayMode on the standby screen surfaced an unobserved-task exception.
                return;
            }

            // Hand off to the manga intro, then step aside.
            mangaIntro.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
