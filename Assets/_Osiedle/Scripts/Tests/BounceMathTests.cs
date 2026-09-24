using NUnit.Framework;
using Osiedle.Weapons;
using UnityEngine;

namespace Osiedle.Tests
{
    public class BounceMathTests
    {
        const float Epsilon = 0.0001f;

        [Test]
        public void HeadOnWallReversesDirection()
        {
            Assert.IsTrue(BounceMath.TryReflectFlat(Vector3.forward, Vector3.back, out Vector3 result));
            Assert.That(Vector3.Distance(result, Vector3.back), Is.LessThan(Epsilon));
        }

        [Test]
        public void AngledHitMirrorsAcrossWall()
        {
            Vector3 incoming = new Vector3(1f, 0f, 1f).normalized;
            Assert.IsTrue(BounceMath.TryReflectFlat(incoming, Vector3.back, out Vector3 result));
            Assert.That(Vector3.Distance(result, new Vector3(1f, 0f, -1f).normalized), Is.LessThan(Epsilon));
        }

        [Test]
        public void ResultIsFlatAndNormalized()
        {
            // Lekko pochyła ściana nie może posłać pocisku w górę ani w dół.
            Vector3 slantedNormal = new Vector3(0f, 0.3f, -1f).normalized;
            Assert.IsTrue(BounceMath.TryReflectFlat(Vector3.forward, slantedNormal, out Vector3 result));
            Assert.AreEqual(0f, result.y, Epsilon);
            Assert.AreEqual(1f, result.magnitude, Epsilon);
        }

        [Test]
        public void FloorDoesNotBounce()
        {
            Assert.IsFalse(BounceMath.TryReflectFlat(Vector3.forward, Vector3.up, out _));
        }
    }
}
