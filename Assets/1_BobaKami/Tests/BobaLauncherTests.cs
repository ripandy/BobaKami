using NUnit.Framework;

namespace BobaKami.Tests
{
    public class BobaLauncherTests
    {
        [TestCase(10f, 100)]
        [TestCase(2f, 500)]
        [TestCase(3f, 333)]
        [TestCase(1f, 1000)]
        public void LaunchDelay_IsFlooredMillisecondsPerLaunch(float launchRate, int expectedDelay)
        {
            var launcher = new BobaLauncher { launchRate = launchRate };

            Assert.AreEqual(expectedDelay, launcher.LaunchDelay);
        }

        [Test]
        public void LaunchBoba_AssignsSequentialIds_PerInstance()
        {
            var launcherA = new BobaLauncher();
            var launcherB = new BobaLauncher();
            launcherA.Initialize();
            launcherB.Initialize();

            var a0 = launcherA.LaunchBoba();
            var a1 = launcherA.LaunchBoba();
            var b0 = launcherB.LaunchBoba();

            Assert.AreEqual(0, a0.Id);
            Assert.AreEqual(1, a1.Id);
            Assert.AreEqual(0, b0.Id, "Launcher instances must not share id state.");
        }

        [Test]
        public void Initialize_ResetsIdsAndBobas()
        {
            var launcher = new BobaLauncher();
            launcher.Initialize();
            launcher.LaunchBoba();
            launcher.LaunchBoba();

            launcher.Initialize();

            Assert.AreEqual(0, launcher.LaunchedBobaCount);
            Assert.AreEqual(0, launcher.LaunchBoba().Id);
        }

        [Test]
        public void TryGetBoba_AndRemoveBoba_ManageLaunchedBobas()
        {
            var launcher = new BobaLauncher();
            launcher.Initialize();
            var boba = launcher.LaunchBoba();

            Assert.IsTrue(launcher.TryGetBoba(boba.Id, out var found));
            Assert.AreEqual(boba.ThrowDirection, found.ThrowDirection);

            launcher.RemoveBoba(boba.Id);

            Assert.IsFalse(launcher.TryGetBoba(boba.Id, out _));
            Assert.AreEqual(0, launcher.LaunchedBobaCount);
        }

        [Test]
        public void SeededLaunchers_ProduceIdenticalDirectionSequences()
        {
            var launcherA = new BobaLauncher(seed: 42);
            var launcherB = new BobaLauncher(seed: 42);
            launcherA.Initialize();
            launcherB.Initialize();

            for (var i = 0; i < 20; i++)
            {
                Assert.AreEqual(launcherA.LaunchBoba().ThrowDirection, launcherB.LaunchBoba().ThrowDirection);
            }
        }

        [TestCase(1, 1f)]      // log2(1) = 0 -> clamped to initialLaunchRate (1)
        [TestCase(4, 1f)]      // log2(4)*0.5 = 1
        [TestCase(16, 2f)]     // log2(16)*0.5 = 2
        [TestCase(99, 3.3147f)]
        public void UpdateLaunchRate_FollowsComboCurve(int peakCombo, float expectedRate)
        {
            var launcher = new BobaLauncher { launchRate = 10f };

            launcher.UpdateLaunchRate(peakCombo);

            Assert.AreEqual(expectedRate, launcher.launchRate, 1e-3f,
                "launchRate = max(initialLaunchRate, log2(peakCombo) * 0.5)");
        }

        [Test]
        public void Initialize_RestoresInitialLaunchRate()
        {
            var launcher = new BobaLauncher { initialLaunchRate = 2f, launchRate = 9f };

            launcher.Initialize();

            Assert.AreEqual(2f, launcher.launchRate,
                "A restart must not inherit the previous run's pace.");
        }

        [Test]
        public void UpdateLaunchRate_ClampsToInitialLaunchRate_NotHardcodedOne()
        {
            // Low combos map below the floor; the floor is the configured start pace, not 1.
            var launcher = new BobaLauncher { initialLaunchRate = 3f, launchRate = 3f };

            launcher.UpdateLaunchRate(2); // log2(2)*0.5 = 0.5, well under 3

            Assert.AreEqual(3f, launcher.launchRate);
        }

        [Test]
        public void UpdateLaunchRate_NeverDecreases_ForNonDecreasingPeakCombo()
        {
            // Regression: pace is fed the run's *peak* combo, so it must be monotonic even as
            // the current combo collapses to 0 after a hit.
            var launcher = new BobaLauncher();
            launcher.Initialize();

            var previous = launcher.launchRate;
            foreach (var peak in new[] { 0, 4, 4, 16, 16, 64, 200 })
            {
                launcher.UpdateLaunchRate(peak);
                Assert.GreaterOrEqual(launcher.launchRate, previous,
                    $"launchRate dropped at peakCombo={peak}");
                previous = launcher.launchRate;
            }
        }
    }
}
