using NUnit.Framework;
using Osiedle.Combat;

namespace Osiedle.Tests
{
    public class ScrapRollTests
    {
        const int Rolls = 1000;

        [Test]
        public void StaysWithinRangeAndHitsBothEnds()
        {
            var rng = new System.Random(77);
            bool sawMin = false, sawMax = false;
            for (int i = 0; i < Rolls; i++)
            {
                int n = ScrapRoll.Count(1, 3, rng);
                Assert.That(n, Is.InRange(1, 3));
                sawMin |= n == 1;
                sawMax |= n == 3;
            }
            Assert.IsTrue(sawMin && sawMax, "Powinny wypadać i 1, i 3 śrubki.");
        }

        [Test]
        public void SwappedRangeIsFixed()
        {
            var rng = new System.Random(1);
            for (int i = 0; i < Rolls; i++)
                Assert.That(ScrapRoll.Count(3, 1, rng), Is.InRange(1, 3));
        }

        [Test]
        public void NegativeValuesBecomeZero()
        {
            var rng = new System.Random(1);
            Assert.AreEqual(0, ScrapRoll.Count(-2, -1, rng));
        }
    }
}
