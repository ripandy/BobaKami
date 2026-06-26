using System.Threading;
using System.Threading.Tasks;
using BobaKami.GameStates;
using NUnit.Framework;
using UnityEngine;

namespace BobaKami.Tests
{
    public class PlayGameStateTests
    {
        [Test]
        public async Task PlayGameState_ShouldReturnGameOver_OnPlayerDeath()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            
            var player = new Player { hp = 100 };
            
            var beanLauncher = new BeanLauncher { launchRate = 10 };
            
            var inputProvider = new DummyPlayerInputProvider(player, beanLauncher);
            
            var playerPresenter = new DummyPlayerPresenter();
            
            using var playGameState = new PlayGameState(
                player,
                beanLauncher,
                playerPresenter,
                playerPresenter,
                playerPresenter,
                inputProvider,
                inputProvider,
                new DummyBeanPresenter());

            beanLauncher.Initialize();
            var result = await playGameState.Running(token);

            var gameStats = player.GameStats;
            
            Assert.GreaterOrEqual(gameStats.Score, 3, "Player should eat at least 3 beans.");
            
            Debug.Log($"Final Score: {gameStats.Score} beans eaten, with {gameStats.Combo} combo.");
            
            Assert.AreEqual(GameStateEnum.GameOver, result);
        }
    }
}