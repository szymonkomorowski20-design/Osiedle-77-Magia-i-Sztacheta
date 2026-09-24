using NUnit.Framework;
using Osiedle.Combat;

namespace Osiedle.Tests
{
    public class HealthPoolTests
    {
        [Test]
        public void StartsFull()
        {
            var hp = new HealthPool(100f, 0.8f);
            Assert.AreEqual(100f, hp.Current);
            Assert.IsFalse(hp.IsDead);
        }

        [Test]
        public void DamageReducesHealth()
        {
            var hp = new HealthPool(100f, 0f);
            Assert.IsTrue(hp.TryTakeDamage(30f, 0f, out float dealt));
            Assert.AreEqual(30f, dealt);
            Assert.AreEqual(70f, hp.Current);
        }

        [Test]
        public void OverkillIsClampedAndKills()
        {
            var hp = new HealthPool(50f, 0f);
            hp.TryTakeDamage(80f, 0f, out float dealt);
            Assert.AreEqual(50f, dealt);
            Assert.AreEqual(0f, hp.Current);
            Assert.IsTrue(hp.IsDead);
        }

        [Test]
        public void InvulnerabilityBlocksSecondHit()
        {
            var hp = new HealthPool(100f, 0.8f);
            hp.TryTakeDamage(10f, 1f, out _);
            Assert.IsFalse(hp.TryTakeDamage(10f, 1.5f, out _));
            Assert.AreEqual(90f, hp.Current);
        }

        [Test]
        public void InvulnerabilityEnds()
        {
            var hp = new HealthPool(100f, 0.8f);
            hp.TryTakeDamage(10f, 1f, out _);
            Assert.IsTrue(hp.TryTakeDamage(10f, 1.9f, out _));
            Assert.AreEqual(80f, hp.Current);
        }

        [Test]
        public void DeadCannotBeDamagedOrHealed()
        {
            var hp = new HealthPool(10f, 0f);
            hp.TryTakeDamage(10f, 0f, out _);
            Assert.IsFalse(hp.TryTakeDamage(5f, 1f, out _));
            Assert.AreEqual(0f, hp.Heal(5f));
        }

        [Test]
        public void ZeroOrNegativeDamageIsIgnored()
        {
            var hp = new HealthPool(100f, 0.8f);
            Assert.IsFalse(hp.TryTakeDamage(0f, 0f, out _));
            Assert.IsFalse(hp.TryTakeDamage(-5f, 0f, out _));
            Assert.IsFalse(hp.IsInvulnerable(0f), "Nieudane trafienie nie powinno dawać nieśmiertelności.");
        }

        [Test]
        public void HealIsClampedToMax()
        {
            var hp = new HealthPool(100f, 0f);
            hp.TryTakeDamage(20f, 0f, out _);
            Assert.AreEqual(20f, hp.Heal(50f));
            Assert.AreEqual(100f, hp.Current);
        }

        [Test]
        public void RestoreRevivesAndClearsInvulnerability()
        {
            var hp = new HealthPool(100f, 5f);
            hp.TryTakeDamage(100f, 0f, out _);
            hp.Restore();
            Assert.AreEqual(100f, hp.Current);
            Assert.IsFalse(hp.IsInvulnerable(0f));
        }

        [Test]
        public void ConfigureLowerMaxClampsCurrent()
        {
            var hp = new HealthPool(100f, 0f);
            hp.Configure(60f, 0f);
            Assert.AreEqual(60f, hp.Current);
        }
    }
}
