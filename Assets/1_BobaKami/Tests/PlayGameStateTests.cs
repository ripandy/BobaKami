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
        private BeanLauncher beanLauncher;
        private ScriptedInputProvider inputProvider;
        private ScriptedBeanPresenter beanPresenter;
        private DummyPlayerPresenter playerPresenter;
        private PlayGameState playGameState;

        [SetUp]
        public void SetUp()
        {
            player = new Player();
            beanLauncher = new BeanLauncher(seed: 42) { launchRate = 100 }; // 10 ms launch ticks
            inputProvider = new ScriptedInputProvider();
            beanPresenter = new ScriptedBeanPresenter();
            playerPresenter = new DummyPlayerPresenter();

            playGameState = new PlayGameState(
                player,
                beanLauncher,
                playerPresenter,
                playerPresenter,
                playerPresenter,
                inputProvider,
                inputProvider,
                beanPresenter);

            player.Initialize();
            beanLauncher.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            playGameState.Dispose();
        }

        [Test]
        public async Task Running_ReturnsGameOver_WhenHpDepleted()
        {
            beanPresenter.AutoDrop = true; // every bean drops -> 5 drops kill (100 hp / 20 dmg)

            var result = await playGameState.Running();

            Assert.AreEqual(GameStateEnum.GameOver, result);
            Assert.IsFalse(player.IsAlive);
            Assert.AreEqual(0f, playerPresenter.LastHealthPercentage);
        }

        [Test]
        public async Task EatingBean_UpdatesScoreCombo_AndHidesBean()
        {
            using var externalCts = new CancellationTokenSource();
            var runTask = playGameState.Running(externalCts.Token).AsTask();

            await beanPresenter.WaitForShown(0);
            inputProvider.PushBite(0);

            // Wait for the eat to be processed (bean 0 hidden).
            while (beanPresenter.HiddenBeans.Count == 0 && !runTask.IsCompleted)
            {
                await Task.Delay(10);
            }

            var stats = player.GameStats;
            Assert.AreEqual(1, stats.Score);
            Assert.AreEqual(1, stats.Combo);
            CollectionAssert.Contains(beanPresenter.HiddenBeans, 0);
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
        public async Task Running_Completes_OnExternalCancellation_WithNoBeanInFlight()
        {
            // Slow launcher: after the first bean resolves there is a ~2 s gap with no bean
            // in flight. Cancelling in that gap used to hang Running forever (tcs never set).
            beanLauncher.launchRate = 0.5f;
            beanPresenter.AutoDrop = true;

            using var externalCts = new CancellationTokenSource();
            var runTask = playGameState.Running(externalCts.Token).AsTask();

            await beanPresenter.WaitForShown(0);
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

            await beanPresenter.WaitForShown(0);
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
