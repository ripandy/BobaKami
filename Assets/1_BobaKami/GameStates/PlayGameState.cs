using System;
using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami.GameStates
{
    public class PlayGameState : IGameState, IDisposable
    {
        private readonly Player player;
        private readonly BobaLauncher bobaLauncher;
        private readonly IPlayerHealthPresenter playerHealthPresenter;
        private readonly IPlayerStatsPresenter playerStatsPresenter;
        private readonly IPlayerDirectionPresenter playerDirectionPresenter;
        private readonly IPlayerDirectionInputProvider playerDirectionInputProvider;
        private readonly IPlayerBiteInputProvider playerBiteInputProvider;
        private readonly IBobaPresenter bobaPresenter;

        public GameStateEnum Id => GameStateEnum.GamePlay;

        private CancellationTokenSource cts;
        private CancellationToken Token => cts.Token;

        private TaskCompletionSource<bool> tcs;

        public PlayGameState(
            Player player,
            BobaLauncher bobaLauncher,
            IPlayerHealthPresenter playerHealthPresenter,
            IPlayerStatsPresenter playerStatsPresenter,
            IPlayerDirectionPresenter playerDirectionPresenter,
            IPlayerDirectionInputProvider playerDirectionInputProvider,
            IPlayerBiteInputProvider playerBiteInputProvider,
            IBobaPresenter bobaPresenter)
        {
            this.player = player;
            this.bobaLauncher = bobaLauncher;
            this.playerHealthPresenter = playerHealthPresenter;
            this.playerStatsPresenter = playerStatsPresenter;
            this.playerDirectionPresenter = playerDirectionPresenter;
            this.playerDirectionInputProvider = playerDirectionInputProvider;
            this.playerBiteInputProvider = playerBiteInputProvider;
            this.bobaPresenter = bobaPresenter;
        }
        
        public async ValueTask<GameStateEnum> Running(CancellationToken cancellationToken = default)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tcs = new TaskCompletionSource<bool>();

            // Complete tcs on cancellation even when no boba is in flight; otherwise Running
            // would await forever (loops break on cancel, but only LaunchBoba sets tcs).
            var runTcs = tcs;
            using var cancellationRegistration = cts.Token.Register(() => runTcs.TrySetResult(false));

            _ = HandlePlayerDirectionInput();
            _ = ExecuteBobaLauncher();
            _ = HandlePlayerBiteInput();

            await tcs.Task;
            await Task.Yield();

            return GameStateEnum.GameOver;
        }

        // NOTE: The loops below are while-based (not recursive) and break on OperationCanceledException.
        //       OCE can also originate from Application.exitCancellationToken (linked inside SOAR's
        //       EventAsync), which fires *before* this state's own token on app quit / play mode exit —
        //       continuing the loop there would spin on synchronously-thrown OCEs and overflow the stack.
        private async Task HandlePlayerDirectionInput()
        {
            while (cts != null && !Token.IsCancellationRequested)
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
                    break;
                }
            }
        }

        private async Task ExecuteBobaLauncher()
        {
            while (cts != null && !Token.IsCancellationRequested)
            {
                try
                {
                    _ = LaunchBoba();
                    await Task.Delay(bobaLauncher.LaunchDelay, Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task LaunchBoba()
        {
            try
            {
                var boba = bobaLauncher.LaunchBoba();
                var dropped = await bobaPresenter.Show(boba.Id, boba.ThrowDirection, Token);
                await Task.Yield();
                bobaLauncher.RemoveBoba(boba.Id);
                if (!dropped) return;
                
                // Boba was dropped, hide it. Eaten bobas are hidden by the player bite input.
                bobaPresenter.Hide(boba.Id);
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetResult(false);
                return;
            }
            
            player.Damaged();
            playerHealthPresenter.Show(player.HealthPercentage);
            // A drop resets the pace back to the floor: the barrage eases so the player can
            // recover, and rebuilds via UpdateLaunchRate as the combo climbs again.
            bobaLauncher.ResetLaunchRate();
            if (player.IsAlive) return;

            cts?.Cancel();
            tcs.TrySetResult(true);
        }

        private async Task HandlePlayerBiteInput()
        {
            while (cts != null && !Token.IsCancellationRequested)
            {
                try
                {
                    var bittenId = await playerBiteInputProvider.WaitForBite(Token);
                    if (bobaLauncher.TryGetBoba(bittenId, out var bittenBoba))
                    {
                        player.EatBoba();
                        playerHealthPresenter.Show(player.HealthPercentage);
                        playerStatsPresenter.Show(player.GameStats);
                        bobaLauncher.UpdateLaunchRate(player.ComboCount);
                        bobaPresenter.Hide(bittenBoba.Id);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}