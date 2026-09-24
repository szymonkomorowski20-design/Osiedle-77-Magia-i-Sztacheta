using NUnit.Framework;
using Osiedle.Weapons;

namespace Osiedle.Tests
{
    public class FireTimerTests
    {
        [Test]
        public void FirstShotIsAllowed()
        {
            var timer = new FireTimer(0.2f);
            Assert.IsTrue(timer.CanFire(0f));
        }

        [Test]
        public void BlocksUntilIntervalPasses()
        {
            var timer = new FireTimer(0.2f);
            timer.Fire(1f);
            Assert.IsFalse(timer.CanFire(1.1f));
            Assert.IsTrue(timer.CanFire(1.2f));
        }

        [Test]
        public void ContinuousFireKeepsSteadyRhythm()
        {
            // Klatki co 0,05 s, strzał co 0,2 s: po 1 s ciągłego ognia ma być 5 strzałów, bez gubienia czasu.
            var timer = new FireTimer(0.2f);
            int shots = 0;
            for (int frame = 0; frame < 20; frame++)
            {
                float now = frame * 0.05f + 0.013f;
                if (!timer.CanFire(now)) continue;
                timer.Fire(now);
                shots++;
            }
            Assert.AreEqual(5, shots);
        }

        [Test]
        public void AfterPauseCountsFromNow()
        {
            var timer = new FireTimer(0.2f);
            timer.Fire(0f);
            timer.Fire(5f);
            Assert.IsFalse(timer.CanFire(5.1f), "Po przerwie następny strzał dopiero po pełnym odstępie.");
            Assert.IsTrue(timer.CanFire(5.2f));
        }

        [Test]
        public void ResetAllowsImmediateShot()
        {
            var timer = new FireTimer(0.2f);
            timer.Fire(1f);
            timer.Reset();
            Assert.IsTrue(timer.CanFire(1f));
        }
    }
}
