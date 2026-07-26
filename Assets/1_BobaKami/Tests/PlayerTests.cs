using NUnit.Framework;

namespace BobaKami.Tests
{
    public class PlayerTests
    {
        [Test]
        public void Initialize_ResetsAllState()
        {
            var player = new Player();
            player.EatBoba();
            player.Damaged();
            player.Direction = Interfaces.DirectionEnum.Left;

            player.Initialize();

            Assert.AreEqual(player.hp, player.CurrentHp);
            Assert.AreEqual(0, player.BobaEatenCount);
            Assert.AreEqual(0, player.Score);
            Assert.AreEqual(0, player.ComboCount);
            Assert.AreEqual(0, player.MaxComboCount);
            Assert.AreEqual(Interfaces.DirectionEnum.Forward, player.Direction);
        }

        [Test]
        public void EatBoba_IncrementsBobasAndCombo_AndHeals()
        {
            var player = new Player();
            player.Damaged(); // 100 -> 80 so healing is observable

            player.EatBoba();

            Assert.AreEqual(1, player.BobaEatenCount);
            Assert.AreEqual(1, player.ComboCount);
            Assert.AreEqual(81, player.CurrentHp);
        }

        [Test]
        public void EatBoba_AccruesScore_AtComboTierMultiplier()
        {
            var player = new Player();

            // Bobas 1-4 score at x1 (100 each) -> 400 after 4.
            for (var i = 0; i < 4; i++) player.EatBoba();
            Assert.AreEqual(400, player.Score);

            // The 5th boba crosses into tier 2 (x2) -> +200.
            player.EatBoba();
            Assert.AreEqual(600, player.Score);
            Assert.AreEqual(2, player.GameStats.Multiplier);
        }

        [Test]
        public void Initialize_ResetsScoreAndMaxCombo()
        {
            var player = new Player();
            for (var i = 0; i < 6; i++) player.EatBoba();
            Assert.Greater(player.Score, 0);
            Assert.AreEqual(6, player.MaxComboCount);

            player.Initialize();

            Assert.AreEqual(0, player.Score);
            Assert.AreEqual(0, player.MaxComboCount);
        }

        [Test]
        public void GameStats_ReportsMaxCombo_IndependentlyOfCurrentCombo()
        {
            var player = new Player();
            player.EatBoba();
            player.EatBoba();
            player.EatBoba();
            player.Damaged(); // current combo -> 0, peak stays 3

            var stats = player.GameStats;
            Assert.AreEqual(0, stats.Combo);
            Assert.AreEqual(3, stats.MaxCombo);
        }

        [Test]
        public void EatBoba_HealIsCappedAtMaxHp()
        {
            var player = new Player();

            player.EatBoba();

            Assert.AreEqual(player.hp, player.CurrentHp);
        }

        [Test]
        public void Damaged_ReducesHpByTwenty_AndResetsCombo()
        {
            var player = new Player();
            player.EatBoba();
            player.EatBoba();
            player.EatBoba();

            player.Damaged();

            Assert.AreEqual(0, player.ComboCount);
            Assert.AreEqual(80, player.CurrentHp);
        }

        [Test]
        public void Damaged_FloorsAtZero_AndPlayerDies()
        {
            var player = new Player();

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(player.IsAlive, $"Player should still be alive after {i} hits.");
                player.Damaged();
            }

            Assert.AreEqual(0, player.CurrentHp);
            Assert.IsFalse(player.IsAlive);

            player.Damaged(); // extra hit must not underflow
            Assert.AreEqual(0, player.CurrentHp);
        }

        [Test]
        public void MaxComboCount_SurvivesComboResets()
        {
            var player = new Player();
            player.EatBoba();
            player.EatBoba();
            player.EatBoba();
            player.Damaged();
            player.EatBoba();

            Assert.AreEqual(1, player.ComboCount);
            Assert.AreEqual(3, player.MaxComboCount);
        }

        [Test]
        public void HealthPercentage_ReflectsCurrentHp()
        {
            var player = new Player();
            player.Damaged();

            Assert.AreEqual(0.8f, player.HealthPercentage, 1e-5f);
        }

        [Test]
        public void CustomHp_RequiresInitialize_ToTakeEffect()
        {
            // Documents a construction quirk: the ctor runs Initialize() before an object
            // initializer assigns hp, so CurrentHp still reflects the default until
            // Initialize() is called again (production always re-initializes via IntroGameState).
            var player = new Player { hp = 40 };
            Assert.AreEqual(100, player.CurrentHp);

            player.Initialize();
            Assert.AreEqual(40, player.CurrentHp);
            Assert.IsFalse(player.HealthPercentage > 1f);
        }
    }
}
