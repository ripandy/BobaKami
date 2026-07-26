using System.Threading.Tasks;
using BobaKami.GameStates;
using NUnit.Framework;

namespace BobaKami.Tests
{
    [Timeout(5000)]
    public class GameStateTests
    {
        [Test]
        public async Task GameStates_FullLoop_RestartsThenExits()
        {
            var player = new Player();
            // initialLaunchRate is what Initialize() restores; set it (not just launchRate) so
            // ticks stay fast across the re-initialize each IntroGameState.Running performs.
            var bobaLauncher = new BobaLauncher(seed: 42) { initialLaunchRate = 100 };

            var playerPresenter = new DummyPlayerPresenter();
            var introPresenter = new InstantIntroPresenter();
            var bobaPresenter = new ScriptedBobaPresenter { AutoDrop = true }; // every run dies quickly
            var inputProvider = new ScriptedInputProvider();
            var gameOverPresenter = new ScriptedGameOverPresenter(true, false); // restart once, then exit

            var introGameState = new IntroGameState(player, bobaLauncher, playerPresenter, playerPresenter, introPresenter);
            using var playGameState = new PlayGameState(
                player,
                bobaLauncher,
                playerPresenter,
                playerPresenter,
                playerPresenter,
                inputProvider,
                inputProvider,
                bobaPresenter);
            var gameOverGameState = new GameOverGameState(player, gameOverPresenter);

            // Round 1: Intro -> GamePlay -> GameOver -> restart (Intro).
            Assert.AreEqual(GameStateEnum.GamePlay, await introGameState.Running());
            Assert.AreEqual(GameStateEnum.GameOver, await playGameState.Running());
            Assert.IsFalse(player.IsAlive);
            Assert.AreEqual(GameStateEnum.Intro, await gameOverGameState.Running());

            // Round 2: Intro re-initializes, then GameOver exits (None).
            Assert.AreEqual(GameStateEnum.GamePlay, await introGameState.Running());
            Assert.IsTrue(player.IsAlive, "Intro must re-initialize the player.");
            Assert.AreEqual(GameStateEnum.GameOver, await playGameState.Running());
            Assert.AreEqual(GameStateEnum.None, await gameOverGameState.Running());

            Assert.AreEqual(2, introPresenter.ShowCount);
            Assert.AreEqual(2, gameOverPresenter.ShownStats.Count);
        }
    }
}
