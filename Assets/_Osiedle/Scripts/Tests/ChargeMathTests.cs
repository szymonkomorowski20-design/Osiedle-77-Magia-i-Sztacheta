using NUnit.Framework;
using Osiedle.Enemies;
using UnityEngine;

namespace Osiedle.Tests
{
    public class ChargeMathTests
    {
        [Test]
        public void TargetOnRightIsPushedRight()
        {
            Vector3 dir = ChargeMath.KnockbackDirection(Vector3.forward, new Vector3(0.3f, 0f, 1f));
            Assert.Greater(dir.x, 0.5f);
            Assert.Greater(dir.z, 0f, "Lekkie pchnięcie do przodu.");
        }

        [Test]
        public void TargetOnLeftIsPushedLeft()
        {
            Vector3 dir = ChargeMath.KnockbackDirection(Vector3.forward, new Vector3(-0.3f, 0f, 1f));
            Assert.Less(dir.x, -0.5f);
        }

        [Test]
        public void TargetOnAxisGoesRightAndIsNormalized()
        {
            Vector3 dir = ChargeMath.KnockbackDirection(Vector3.forward, Vector3.forward);
            Assert.Greater(dir.x, 0f);
            Assert.AreEqual(1f, dir.magnitude, 0.0001f);
            Assert.AreEqual(0f, dir.y, 0.0001f);
        }

        [Test]
        public void TouchingUsesRadiiAndPadding()
        {
            Assert.IsTrue(ChargeMath.Touching(Vector3.zero, 0.45f, new Vector3(0.9f, 0f, 0f), 0.35f, 0.15f));
            Assert.IsFalse(ChargeMath.Touching(Vector3.zero, 0.45f, new Vector3(1.0f, 0f, 0f), 0.35f, 0.15f));
        }

        [Test]
        public void TouchingIgnoresHeight()
        {
            Assert.IsTrue(ChargeMath.Touching(Vector3.zero, 0.5f, new Vector3(0.5f, 3f, 0f), 0.5f, 0f));
        }
    }
}
