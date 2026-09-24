using NUnit.Framework;
using Osiedle.Player;

namespace Osiedle.Tests
{
    public class DashChargesTests
    {
        [Test]
        public void StartsWithFullCharges()
        {
            var charges = new DashCharges(2, 0.8f);
            Assert.AreEqual(2, charges.Charges);
            Assert.AreEqual(1f, charges.RechargeProgress);
        }

        [Test]
        public void ConsumeFailsWhenEmpty()
        {
            var charges = new DashCharges(1, 0.8f);
            Assert.IsTrue(charges.TryConsume());
            Assert.IsFalse(charges.TryConsume());
            Assert.AreEqual(0, charges.Charges);
        }

        [Test]
        public void RechargesAfterCooldown()
        {
            var charges = new DashCharges(1, 0.8f);
            charges.TryConsume();

            charges.Tick(0.79f);
            Assert.AreEqual(0, charges.Charges);

            charges.Tick(0.02f);
            Assert.AreEqual(1, charges.Charges);
        }

        [Test]
        public void RechargesOneChargeAtATime()
        {
            var charges = new DashCharges(2, 1f);
            charges.TryConsume();
            charges.TryConsume();

            charges.Tick(1.1f);
            Assert.AreEqual(1, charges.Charges);

            charges.Tick(1f);
            Assert.AreEqual(2, charges.Charges);
        }

        [Test]
        public void LongTickDoesNotExceedMax()
        {
            var charges = new DashCharges(2, 0.5f);
            charges.TryConsume();
            charges.Tick(10f);
            Assert.AreEqual(2, charges.Charges);
        }

        [Test]
        public void ZeroCooldownRefillsImmediately()
        {
            var charges = new DashCharges(1, 0f);
            charges.TryConsume();
            charges.Tick(0f);
            Assert.AreEqual(1, charges.Charges);
        }

        [Test]
        public void ConfigureClampsChargesToNewMax()
        {
            var charges = new DashCharges(2, 0.8f);
            charges.Configure(1, 0.8f);
            Assert.AreEqual(1, charges.MaxCharges);
            Assert.AreEqual(1, charges.Charges);
        }

        [Test]
        public void ConfigureToMoreChargesRechargesTheNewOne()
        {
            var charges = new DashCharges(1, 0.8f);
            charges.Configure(2, 0.8f);
            Assert.AreEqual(1, charges.Charges);

            charges.Tick(0.8f);
            Assert.AreEqual(2, charges.Charges);
        }

        [Test]
        public void MaxChargesIsAtLeastOne()
        {
            var charges = new DashCharges(0, 0.8f);
            Assert.AreEqual(1, charges.MaxCharges);
        }
    }
}
