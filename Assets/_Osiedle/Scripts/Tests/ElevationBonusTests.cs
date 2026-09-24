using NUnit.Framework;
using Osiedle.Weapons;

namespace Osiedle.Tests
{
    public class ElevationBonusTests
    {
        [Test]
        public void SameLevelHasNoBonus()
        {
            Assert.AreEqual(1f, ElevationBonus.Multiplier(0f, 0f, 0.5f, 0.2f));
        }

        [Test]
        public void HighEnoughGivesBonus()
        {
            Assert.AreEqual(1.2f, ElevationBonus.Multiplier(1.2f, 0f, 0.5f, 0.2f), 0.0001f);
        }

        [Test]
        public void ExactlyAtThresholdCounts()
        {
            Assert.AreEqual(1.2f, ElevationBonus.Multiplier(0.5f, 0f, 0.5f, 0.2f), 0.0001f);
        }

        [Test]
        public void ShootingUpwardHasNoBonus()
        {
            Assert.AreEqual(1f, ElevationBonus.Multiplier(0f, 1.2f, 0.5f, 0.2f));
        }
    }
}
