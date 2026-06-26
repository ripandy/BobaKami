using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using BobaKami.DataTransferObjects;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BobaKami.Gameplay.HUD
{
    public class GameStatPresenter : MonoBehaviour
    {
        [SerializeField] private GameStatsVariable gameStatsVariable;
        [SerializeField] private TMP_Text scoreText;

        [SerializeField] private GameObject comboParent;
        [SerializeField] private TMP_Text comboValue;
        [SerializeField] private TMP_Text comboLabel;
        [SerializeField] private Image effectImage;

        private CancellationTokenSource cts;
        
        private IDisposable subscription;

        private void Start()
        {
            subscription = gameStatsVariable.Subscribe(UpdateStats);
            UpdateScore(0);
            ShowCombo(0).Forget();
        }

        private void UpdateStats(GameStatsDto stats)
        {
            UpdateScore(stats.Score);
            ShowCombo(stats.Combo).Forget();
        }
        
        private void UpdateScore(int score)
        {
            scoreText.text = score.ToString();
        }
        
        private async UniTaskVoid ShowCombo(int combo)
        {
            const int minCombo = 5;
            
            comboValue.text = combo < minCombo ? string.Empty : combo.ToString();
            comboParent.SetActive(combo >= minCombo);
            
            if (combo < minCombo) return;
            
            RefreshToken();
            
            const float animDuration = 1f;
            
            var textColorTask = LMotion.Create(new Color(0.2735849f, 0.1391513f, 0.06581523f), Color.white, animDuration)
                .WithEase(Ease.InCubic)
                .Bind(SetColor)
                .ToUniTask(cancellationToken: cts.Token);

            var canceled = await UniTask.WhenAll(textColorTask, EffectTaskWithDelay()).SuppressCancellationThrow();
            if (canceled) return;
            
            comboParent.SetActive(false);
            
            void SetColor(Color color)
            {
                comboValue.color = color;
                comboLabel.color = color;
                effectImage.color = color;
            }

            async UniTask EffectTaskWithDelay()
            {
                 await LMotion.Create(Vector2.zero, Vector2.one, animDuration * 0.4f)
                    .WithEase(Ease.OutBack)
                    .Bind(value => effectImage.transform.localScale = value)
                    .ToUniTask(cancellationToken: cts.Token);
                await UniTask.Delay(TimeSpan.FromSeconds(animDuration * 0.6f), cancellationToken: cts.Token);
            }
        }

        private void RefreshToken()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}