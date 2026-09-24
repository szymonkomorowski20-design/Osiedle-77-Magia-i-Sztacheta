using NUnit.Framework;
using Osiedle.Player;

namespace Osiedle.Tests
{
    public class ResourcePoolTests
    {
        [Test]
        public void StartIsClampedToMax()
        {
            var pool = new ResourcePool(30f, 50f);
            Assert.AreEqual(30f, pool.Current);
            Assert.IsTrue(pool.IsFull);
        }

        [Test]
        public void AddReturnsOnlyWhatFits()
        {
            var pool = new ResourcePool(30f, 28f);
            Assert.AreEqual(2f, pool.Add(5f));
            Assert.AreEqual(30f, pool.Current);
        }

        [Test]
        public void NegativeAddIsIgnored()
        {
            var pool = new ResourcePool(30f, 10f);
            Assert.AreEqual(0f, pool.Add(-5f));
            Assert.AreEqual(10f, pool.Current);
        }

        [Test]
        public void SpendFailsWithoutEnough()
        {
            var pool = new ResourcePool(30f, 2f);
            Assert.IsFalse(pool.TrySpend(3f));
            Assert.AreEqual(2f, pool.Current);
            Assert.IsTrue(pool.TrySpend(2f));
            Assert.AreEqual(0f, pool.Current);
        }

        [Test]
        public void LowerMaxDropsExcess()
        {
            var pool = new ResourcePool(30f, 25f);
            pool.SetMax(20f);
            Assert.AreEqual(20f, pool.Current);
        }
    }
}
