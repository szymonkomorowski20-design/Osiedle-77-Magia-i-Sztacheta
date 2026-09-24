using NUnit.Framework;
using Osiedle.Weapons;

namespace Osiedle.Tests
{
    public class ComboCounterTests
    {
        [Test]
        public void GoesThroughStepsInOrder()
        {
            var combo = new ComboCounter(3, 0.5f);
            Assert.AreEqual(0, combo.Next(0f));
            combo.EndStep(0.4f);
            Assert.AreEqual(1, combo.Next(0.5f));
            combo.EndStep(0.9f);
            Assert.AreEqual(2, combo.Next(1f));
        }

        [Test]
        public void WrapsAfterLastStep()
        {
            var combo = new ComboCounter(3, 0.5f);
            // Ciosy co 0,3 s — w oknie serii (0,5 s).
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, combo.Next(i * 0.3f));
                combo.EndStep(i * 0.3f);
            }
            Assert.AreEqual(0, combo.Next(0.7f));
        }

        [Test]
        public void ResetsAfterPause()
        {
            var combo = new ComboCounter(3, 0.5f);
            combo.Next(0f);
            combo.EndStep(0.4f);
            Assert.AreEqual(0, combo.Next(1.0f), "Po przerwie dłuższej niż okno seria zaczyna się od nowa.");
        }

        [Test]
        public void SetLengthShorterKeepsIndexValid()
        {
            var combo = new ComboCounter(3, 0.5f);
            combo.Next(0f);
            combo.EndStep(0f);
            combo.Next(0.1f);
            combo.EndStep(0.1f);
            combo.SetLength(2);
            Assert.AreEqual(0, combo.Next(0.2f));
        }

        [Test]
        public void ResetStartsOver()
        {
            var combo = new ComboCounter(3, 10f);
            combo.Next(0f);
            combo.EndStep(0f);
            combo.Reset();
            Assert.AreEqual(0, combo.Next(0.1f));
        }
    }
}
