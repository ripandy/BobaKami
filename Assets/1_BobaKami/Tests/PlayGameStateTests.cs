using System.Threading;
using System.Threading.Tasks;
using BobaKami.GameStates;
using BobaKami.Interfaces;
using NUnit.Framework;

namespace BobaKami.Tests
{
    [Timeout(5000)]
    public class PlayGameStateTests
    {
        private Player player;
        private BobaLauncher bobaLauncher;
        private ScriptedInputProvider inputProvider;
        private ScriptedBobaPresenter bobaPresenter;
        private DummyPlayerPresenter playerPresenter;
        private PlayGameState playGameState;

        [SetUp]
        public void SetUp()
        {
            player = new Player();
            bobaLauncher = new BobaLauncher(seed: 42) { initialLaunchRate = 100 }; // 10 ms launch ticks after Initialize
            inputProvider = new ScriptedInputProvider();
            bobaPresenter = new ScriptedBobaPresenter();
            playerPresenter = new DummyPlayerPresenter();

            playGameState = new PlayGameState(
                player,
                bobaLauncher,
                playerPresenter,
                playerPresenter,
                playerPresenter,
                inputProvider,
                inputProvider,
                bobaPresenter);

            player.Initialize();
            bobaLauncher.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            playGameState.Dispose();
        }

        [Test]
        public async Task Running_ReturnsGameOver_WhenHpDepleted()
        {
            bobaPresenter.AutoDrop = true; // every boba drops -> 5 drops kill (100 hp / 20 dmg)

            var result = await playGameState.Running();

            Assert.AreEqual(GameStateEnum.GameOver, result);
            Assert.IsFalse(player.IsAlive);
            Assert.AreEqual(0f, playerPresenter.LastHealthPercentage);
        }

        [Test]
        public async Task EatingBoba_UpdatesScoreCombo_AndHidesBoba()
        {
            using var externalCts = new CancellationTokenSource();
            var runTask = playGameState.Running(externalCts.Token).AsTask();

            await bobaPresenter.WaitForShown(0);
            inputProvider.PushBite(0);

            // Wait for the eat to be processed (boba 0 hidden).
            while (bobaPresenter.HiddenBobas.Count == 0 && !runTask.IsCompleted)
            {
                await Task.Delay(10);
            }

            var stats = player.GameStats;
            Assert.AreEqual(100, stats.Score); // first boba: 100 base * x1 tier
            Assert.AreEqual(1, stats.Combo);
            Assert.AreEqual(1, stats.BobaEaten);
            CollectionAssert.Contains(bobaPresenter.HiddenBobas, 0);
            Assert.AreEqual(1, playerPresenter.ShownStats.Count);

            externalCts.Cancel();
            Assert.AreEqual(GameStateEnum.GameOver, await runTask);
        }

        [Test]
        public async Task DirectionInput_UpdatesPlayer_AndPresenter()
        {
            using var externalCts = new CancellationTokenSource();
            var runTask = playGameState.Running(externalCts.Token).AsTask();

            inputProvider.PushDirection(DirectionEnum.Left);

            while (playerPresenter.ShownDirections.Count == 0 && !runTask.IsCompleted)
            {
                await Task.Delay(10);
            }

            Assert.AreEqual(DirectionEnum.Left, player.Direction);
            Assert.AreEqual(DirectionEnum.Left, playerPresenter.LastDirection);

            externalCts.Cancel();
            await runTask;
        }

        [Test]
        public async Task Running_Completes_OnExternalCancellation_WithNoBobaInFlight()
        {
            // Slow launcher: after the first boba resolves there is a ~2 s gap with no boba
            // in flight. Cancelling in that gap used to hang Running forever (tcs never set).
            bobaLauncher.launchRate = 0.5f;
            bobaPresenter.AutoDrop = true;

            using var externalCts = new CancellationTokenSource();
            var runTask = playGameState.Running(externalCts.Token).AsTask();

            await bobaPresenter.WaitForShown(0);
            await Task.Delay(50); // let the drop/damage settle; launch gap is 2000 ms

            externalCts.Cancel();

            var completed = await Task.WhenAny(runTask, Task.Delay(1000));
            Assert.AreSame(runTask, completed, "Running must complete when externally cancelled.");
            Assert.AreEqual(GameStateEnum.GameOver, await runTask);
        }

        [Test]
        public async Task InputLoops_Stop_WhenProviderThrowsOCE_WhileStateStillRunning()
        {
            // Simulates Application.exitCancellationToken firing (SOAR EventAsync links it)
            // while the game state's own token is alive - the exact shape of the PlayMode-end
            // crash. Loops must exit instead of spinning.
            using var externalCts = new CancellationTokenSource();
            var runTask = playGameState.Running(externalCts.Token).AsTask();

            await bobaPresenter.WaitForShown(0);
            inputProvider.CancelPendingWaits();

            // Direction loop's OCE side effect: presenter reset to Forward.
            while (playerPresenter.ShownDirections.Count == 0 && !runTask.IsCompleted)
            {
                await Task.Delay(10);
            }
            Assert.AreEqual(DirectionEnum.Forward, playerPresenter.LastDirection);

            // Bite loop is dead: a pushed bite must no longer eat anything.
            inputProvider.PushBite(0);
            await Task.Delay(100);
            Assert.AreEqual(0, player.GameStats.Score);

            externalCts.Cancel();
            Assert.AreEqual(GameStateEnum.GameOver, await runTask);
        }
    }
}
