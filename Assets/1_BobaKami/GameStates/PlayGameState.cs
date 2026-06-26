using System;
using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami.GameStates
{
    public class PlayGameState : IGameState, IDisposable
    {
        private readonly Player player;
        private readonly BeanLauncher beanLauncher;
        private readonly IPlayerHealthPresenter playerHealthPresenter;
        private readonly IPlayerStatsPresenter playerStatsPresenter;
        private readonly IPlayerDirectionPresenter playerDirectionPresenter;
        private readonly IPlayerDirectionInputProvider playerDirectionInputProvider;
        private readonly IPlayerBiteInputProvider playerBiteInputProvider;
        private readonly IBeanPresenter beanPresenter;

        public GameStateEnum Id => GameStateEnum.GamePlay;

        private CancellationTokenSource cts;
        private CancellationToken Token => cts.Token;

        private TaskCompletionSource<bool> tcs;

        public PlayGameState(
            Player player,
            BeanLauncher beanLauncher,
            IPlayerHealthPresenter playerHealthPresenter,
            IPlayerStatsPresenter playerStatsPresenter,
            IPlayerDirectionPresenter playerDirectionPresenter,
            IPlayerDirectionInputProvider playerDirectionInputProvider,
            IPlayerBiteInputProvider playerBiteInputProvider,
            IBeanPresenter beanPresenter)
        {
            this.player = player;
            this.beanLauncher = beanLauncher;
            this.playerHealthPresenter = playerHealthPresenter;
            this.playerStatsPresenter = playerStatsPresenter;
            this.playerDirectionPresenter = playerDirectionPresenter;
            this.playerDirectionInputProvider = playerDirectionInputProvider;
            this.playerBiteInputProvider = playerBiteInputProvider;
            this.beanPresenter = beanPresenter;
        }
        
        public async ValueTask<GameStateEnum> Running(CancellationToken cancellationToken = default)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tcs = new TaskCompletionSource<bool>();
            
            HandlePlayerDirectionInput();
            ExecuteBeanLauncher();
            HandlePlayerBiteInput();

            await tcs.Task;
            await Task.Yield();
            
            return GameStateEnum.GameOver;
        }

        private async void HandlePlayerDirectionInput()
        {
            try
            {
                var direction = await playerDirectionInputProvider.WaitForDirectionInput(Token);
                player.Direction = direction;
                playerDirectionPresenter.Show(player.Direction);
            }
            catch (OperationCanceledException)
            {
                // state's over
                playerDirectionPresenter.Show(DirectionEnum.Forward);
            }
            
            if (cts == null || Token.IsCancellationRequested) return;
            HandlePlayerDirectionInput(); // Recursive Call
        }

        private async void ExecuteBeanLauncher()
        {
            try
            {
                LaunchBean();
                await Task.Delay(beanLauncher.LaunchDelay, Token);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            
            if (cts == null || Token.IsCancellationRequested) return;
            ExecuteBeanLauncher(); // Recursive Call
        }

        private async void LaunchBean()
        {
            try
            {
                var bean = beanLauncher.LaunchBean();
                var dropped = await beanPresenter.Show(bean.Id, bean.ThrowDirection, Token);
                await Task.Yield();
                beanLauncher.RemoveBean(bean.Id);
                if (!dropped) return;
                
                // Bean was dropped, hide it. Eaten beans are hidden by the player bite input.
                beanPresenter.Hide(bean.Id);
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetResult(false);
                return;
            }
            
            player.Damaged();
            playerHealthPresenter.Show(player.HealthPercentage);
            if (player.IsAlive) return;

            cts?.Cancel();
            tcs.TrySetResult(true);
        }

        private async void HandlePlayerBiteInput()
        {
            try
            {
                var bittenId = await playerBiteInputProvider.WaitForBite(Token);
                if (beanLauncher.TryGetBean(bittenId, out var bittenBean))
                {
                    player.EatBean();
                    playerHealthPresenter.Show(player.HealthPercentage);
                    playerStatsPresenter.Show(player.GameStats);
                    beanLauncher.UpdateLaunchRate(player.ComboCount);
                    beanPresenter.Hide(bittenBean.Id);
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            
            if (cts == null || Token.IsCancellationRequested) return;
            HandlePlayerBiteInput(); // Recursive Call
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}