using NUnit.Framework;

namespace BobaKami.Tests
{
    public class PlayerTests
    {
        [Test]
        public void Initialize_ResetsAllState()
        {
            var player = new Player();
            player.EatBean();
            player.Damaged();
            player.Direction = Interfaces.DirectionEnum.Left;

            player.Initialize();

            Assert.AreEqual(player.hp, player.CurrentHp);
            Assert.AreEqual(0, player.BeanEatenCount);
            Assert.AreEqual(0, player.ComboCount);
            Assert.AreEqual(Interfaces.DirectionEnum.Forward, player.Direction);
        }

        [Test]
        public void EatBean_IncrementsScoreAndCombo_AndHeals()
        {
            var player = new Player();
            player.Damaged(); // 100 -> 80 so healing is observable

            player.EatBean();

            Assert.AreEqual(1, player.BeanEatenCount);
            Assert.AreEqual(1, player.ComboCount);
            Assert.AreEqual(81, player.CurrentHp);
        }

        [Test]
        public void EatBean_HealIsCappedAtMaxHp()
        {
            var player = new Player();

            player.EatBean();

            Assert.AreEqual(player.hp, player.CurrentHp);
        }

        [Test]
        public void Damaged_ReducesHpByTwenty_AndResetsCombo()
        {
            var player = new Player();
            player.EatBean();
            player.EatBean();
            player.EatBean();

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
            player.EatBean();
            player.EatBean();
            player.EatBean();
            player.Damaged();
            player.EatBean();

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
