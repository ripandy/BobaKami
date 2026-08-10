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
            var highScoreTable = new HighScoreTable();
            var highScoreStore = new RecordingHighScoreStore();
            var gameOverGameState = new GameOverGameState(player, gameOverPresenter, highScoreTable, highScoreStore);

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

            // Both runs auto-dropped every boba, so neither scored and neither took a table slot.
            Assert.AreEqual(0, highScoreStore.SaveCount);
            Assert.AreEqual(0, highScoreTable.Entries.Count);
        }

        [Test]
        public async Task GameOver_SubmitsAndSavesScore_WhenPlayerDied()
        {
            // hp must be assigned before Initialize: the ctor runs Initialize first.
            var player = new Player { hp = 20 };
            player.Initialize();
            player.EatBoba();  // Score = 100
            player.Damaged();  // hp 20 -> 0, dead

            var highScoreTable = new HighScoreTable();
            var highScoreStore = new RecordingHighScoreStore();
            var gameOverPresenter = new ScriptedGameOverPresenter(false);
            var gameOverGameState = new GameOverGameState(
                player, gameOverPresenter, highScoreTable, highScoreStore);

            Assert.AreEqual(GameStateEnum.None, await gameOverGameState.Running());

            Assert.AreEqual(1, highScoreStore.SaveCount);
            Assert.AreSame(highScoreTable, highScoreStore.LastSaved);
            Assert.AreEqual(100, highScoreTable.BestScore);
            // Rank 1 is the signal the NEW-record badge reads.
            Assert.AreEqual(new[] { 1 }, gameOverPresenter.ShownRanks);
        }

        [Test]
        public async Task GameOver_ReportsPlacedButNotRecord_WhenScoreOnlyTiesTheBest()
        {
            // Regression for the NEW badge firing on a tie: the presenter used to infer
            // "new record" from BestScore == Score, but TrySubmit places ties *after* the
            // incumbent, so matching the best is a placement (rank 2), never a record.
            var player = new Player { hp = 20 };
            player.Initialize();
            player.EatBoba();  // Score = 100
            player.Damaged();  // dead

            var highScoreTable = new HighScoreTable();
            highScoreTable.TrySubmit(100, System.DateTime.UtcNow); // an incumbent 100 already stands
            var highScoreStore = new RecordingHighScoreStore();
            var gameOverPresenter = new ScriptedGameOverPresenter(false);
            var gameOverGameState = new GameOverGameState(
                player, gameOverPresenter, highScoreTable, highScoreStore);

            await gameOverGameState.Running();

            Assert.AreEqual(100, highScoreTable.BestScore);
            Assert.AreEqual(new[] { 2 }, gameOverPresenter.ShownRanks);
        }

        [Test]
        public async Task GameOver_DoesNotSubmit_WhenRunWasCancelledRatherThanLost()
        {
            // PlayGameState returns GameOver on external cancellation too, with the player alive.
            var player = new Player();
            player.Initialize();
            player.EatBoba();

            var highScoreTable = new HighScoreTable();
            var highScoreStore = new RecordingHighScoreStore();
            var gameOverPresenter = new ScriptedGameOverPresenter(false);
            var gameOverGameState = new GameOverGameState(
                player, gameOverPresenter, highScoreTable, highScoreStore);

            Assert.IsTrue(player.IsAlive);
            await gameOverGameState.Running();

            Assert.AreEqual(0, highScoreStore.SaveCount);
            Assert.AreEqual(0, highScoreTable.Entries.Count);
            Assert.AreEqual(new[] { 0 }, gameOverPresenter.ShownRanks);
        }
    }
}
