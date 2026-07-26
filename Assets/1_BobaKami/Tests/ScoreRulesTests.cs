using NUnit.Framework;

namespace BobaKami.Tests
{
    public class ScoreRulesTests
    {
        [TestCase(0, 1)]
        [TestCase(4, 1)]   // last of tier 1
        [TestCase(5, 2)]   // first of tier 2 (matches the combo popup threshold)
        [TestCase(9, 2)]
        [TestCase(10, 3)]  // first of tier 3
        [TestCase(19, 3)]
        [TestCase(20, 4)]  // first of tier 4
        [TestCase(49, 4)]  // last of tier 4
        [TestCase(50, 5)]  // first of tier 5 (cap)
        [TestCase(999, 5)]
        public void MultiplierFor_StepsAtTierBoundaries(int combo, int expectedMultiplier)
        {
            Assert.AreEqual(expectedMultiplier, ScoreRules.MultiplierFor(combo));
        }

        [TestCase(1, 100)]    // tier 1: 100 x 1
        [TestCase(5, 200)]    // tier 2: 100 x 2
        [TestCase(50, 500)]   // tier 5: 100 x 5
        public void GetScore_IsBaseTimesMultiplier(int combo, int expectedScore)
        {
            Assert.AreEqual(expectedScore, ScoreRules.GetScore(combo));
        }

        [Test]
        public void BobaBaseScore_IsOneHundred()
        {
            Assert.AreEqual(100, ScoreRules.BobaBaseScore);
        }
    }
}
