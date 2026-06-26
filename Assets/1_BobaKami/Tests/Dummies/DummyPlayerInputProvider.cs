using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;
using UnityEngine;

namespace BobaKami.Tests
{
    internal class DummyPlayerInputProvider : IPlayerDirectionInputProvider, IPlayerBiteInputProvider
    {
        private readonly Player player;
        private readonly BeanLauncher beanLauncher;
        
        public DummyPlayerInputProvider(Player player, BeanLauncher beanLauncher)
        {
            this.player = player;
            this.beanLauncher = beanLauncher;
        }
        
        public async ValueTask<DirectionEnum> WaitForDirectionInput(CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(beanLauncher.LaunchDelay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }

            var gameStats = player.GameStats;
            var direction = gameStats.Score < 3 && beanLauncher.TryGetBean(gameStats.Score, out var bean)
                ? bean.ThrowDirection
                : (DirectionEnum)(Random.Range(0, 3) - 1);
            Debug.Log($"Player Direction update: {direction}");
            return  direction;
        }

        public async ValueTask<int> WaitForBite(CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(beanLauncher.LaunchDelay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
            
            var gameStats = player.GameStats;
            var beanId = gameStats.Score < 3
                ? gameStats.Score
                : Random.Range(0, beanLauncher.LaunchedBeanCount * 2);
            if (beanLauncher.TryGetBean(beanId, out var bean) && bean.ThrowDirection == player.Direction)
            {
                Debug.Log($"Player Bite beanId: {beanId}");
                return beanId;
            }
            
            Debug.Log("Player Bite Nothing..!");
            return -1;
        }
    }
}