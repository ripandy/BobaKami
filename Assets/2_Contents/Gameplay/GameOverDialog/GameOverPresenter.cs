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

        public async ValueTask<bool> Show(GameStatsDto stats, CancellationToken cancellationToken = default)
        {
            canvasGroup.gameObject.SetActive(true);
            
            int result;
            try
            {
                await AnimateStats(stats, cancellationToken);
                result = await OnFullView(cancellationToken);
                await OnFadeOut(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // forced exit button
                result = 1;
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
            
            return result == 0;
        }

        private UniTask OnFadeIn(CancellationToken cancellationToken = default)
        {
            return LMotion.Create(0, 1, fadeDuration).Bind(alpha => canvasGroup.alpha = alpha).ToUniTask(cancellationToken: cancellationToken);
        }

        private async UniTask AnimateStats(GameStatsDto stats, CancellationToken cancellationToken = default)
        {
            const float duration = 0.5f;
            const float delayFactor = 0.1f;
            
            scoreText.text = stats.Score.ToString();
            bobaCountText.text = stats.BobaEaten.ToString();
            comboText.text = stats.MaxCombo.ToString();
            bestScoreText.text = highScoreData.Value.BestScore.ToString();
            
            var isNew = highScoreData.Value.BestScore == stats.Score;
            newHighScoreObject.SetActive(isNew);

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
            var buttonTasks = buttons.Select(button => button.OnClickAsync(cancellationToken));
            var mouthTasks = mouthOpenEvent.AsObservable().Distinct().Where(opened => !opened)
                .FirstAsync(cancellationToken: cancellationToken).AsUniTask();
            var result = await UniTask.WhenAny(buttonTasks.Append(mouthTasks));
            if (result == 2)
            {
                return faceVector.Value.x > 0 ? 1 : 0;
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