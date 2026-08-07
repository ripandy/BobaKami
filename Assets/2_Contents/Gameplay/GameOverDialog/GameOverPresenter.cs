using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using BobaKami.DataTransferObjects;
using BobaKami.Interfaces;
using LitMotion;
using R3;
using Soar.Events;
using Soar.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BobaKami.Gameplay
{
    public class GameOverPresenter : MonoBehaviour, IGameOverPresenter
    {
        [SerializeField] private HighScoreData highScoreData;
        [SerializeField] private Variable<Vector2> faceVector;
        [SerializeField] private GameEvent<bool> mouthOpenEvent;
        [SerializeField] private Transform[] animationObjects;
        [SerializeField] private GameObject[] playerDirectionOverlays;
        [SerializeField] private Button[] buttons;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text bobaCountText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private GameObject newHighScoreObject;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        [Tooltip("Booth safety net: if nobody answers the game-over screen within this many " +
                 "seconds, return to the Title standby screen. Set to 0 to wait forever.")]
        [SerializeField] private float idleTimeoutSeconds = 20f;

        // OnFullView's return codes. Index 0 is the RestartButton and index 1 the ExitButton,
        // so these double as the button indices; Show maps RestartResult to "replay".
        private const int RestartResult = 0;
        private const int ExitResult = 1;

        private IDisposable subscription;

        private void Start()
        {
            subscription = faceVector.Subscribe(value =>
            {
                if (playerDirectionOverlays.Length < 2) return;
                playerDirectionOverlays[0].SetActive(value.x < 0);
                playerDirectionOverlays[1].SetActive(value.x > 0);
            });
        }

        public async ValueTask<bool> Show(GameStatsDto stats, int highScoreRank,
            CancellationToken cancellationToken = default)
        {
            canvasGroup.gameObject.SetActive(true);

            int result;
            try
            {
                await AnimateStats(stats, highScoreRank, cancellationToken);
                result = await OnFullView(cancellationToken);
                await OnFadeOut(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // forced exit button
                result = ExitResult;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);

            return result == RestartResult;
        }

        private UniTask OnFadeIn(CancellationToken cancellationToken = default)
        {
            return LMotion.Create(0, 1, fadeDuration).Bind(alpha => canvasGroup.alpha = alpha).ToUniTask(cancellationToken: cancellationToken);
        }

        private async UniTask AnimateStats(GameStatsDto stats, int highScoreRank,
            CancellationToken cancellationToken = default)
        {
            const float duration = 0.5f;
            const float delayFactor = 0.1f;
            
            scoreText.text = stats.Score.ToString();
            bobaCountText.text = stats.BobaEaten.ToString();
            comboText.text = stats.MaxCombo.ToString();
            bestScoreText.text = highScoreData.Value.BestScore.ToString();
            
            // Rank 1 is the only new record. Comparing BestScore to Score instead would also
            // light up on a tie, since TrySubmit places equal scores after the incumbent.
            newHighScoreObject.SetActive(highScoreRank == 1);

            var tasks = animationObjects.Select((obj, i) =>
            {
                var startScale = obj.localScale;
                return LMotion.Create(Vector3.zero, startScale, duration)
                    .WithDelay(delayFactor * i)
                    .WithEase(Ease.InBounce)
                    .Bind(newScale => obj.localScale = newScale)
                    .ToUniTask(cancellationToken);
            }).ToArray();

            foreach (var obj in animationObjects)
            {
                obj.localScale = Vector3.zero;
            }
            
            await OnFadeIn(cancellationToken);
            await UniTask.WhenAll(tasks);
        }

        private async UniTask<int> OnFullView(CancellationToken cancellationToken = default)
        {
            // Linked so the losing awaiters (button handlers, the mouth subscription, the idle
            // delay) are torn down when this call resolves instead of living until app quit.
            using var viewCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = viewCts.Token;

            var tasks = buttons.Select(button => button.OnClickAsync(token)).ToList();

            var mouthIndex = tasks.Count;
            tasks.Add(mouthOpenEvent.AsObservable().Distinct().Where(opened => !opened)
                .FirstAsync(cancellationToken: token).AsUniTask());

            var timeoutIndex = -1;
            if (idleTimeoutSeconds > 0f)
            {
                timeoutIndex = tasks.Count;
                tasks.Add(UniTask.Delay(TimeSpan.FromSeconds(idleTimeoutSeconds), cancellationToken: token));
            }

            var result = await UniTask.WhenAny(tasks);

            // Nobody answered — the player walked off. Exit to the Title standby rather than
            // restarting, so the booth returns to its "insert coin" prompt for whoever arrives
            // next instead of dropping them mid-run. Safe now that the standby gate runs before
            // the manga: the only thing between here and that prompt is the splash, which ends
            // on its own. (Before that reversal the manga sat in the way, unskippable by bite.)
            if (result == timeoutIndex) return ExitResult;

            if (result == mouthIndex)
            {
                return faceVector.Value.x > 0 ? ExitResult : RestartResult;
            }
            return result;
        }
        
        private UniTask OnFadeOut(CancellationToken cancellationToken = default)
        {
            return LMotion.Create(1, 0, fadeDuration).Bind(alpha => canvasGroup.alpha = alpha).ToUniTask(cancellationToken: cancellationToken);
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}