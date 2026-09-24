using NUnit.Framework;
using Osiedle.Player;

namespace Osiedle.Tests
{
    public class InputBufferTests
    {
        [Test]
        public void NothingPendingBeforePress()
        {
            var buffer = new InputBuffer(0.15f);
            Assert.IsFalse(buffer.IsPending(0f));
            Assert.IsFalse(buffer.TryConsume(0f));
        }

        [Test]
        public void PressIsValidInsideWindow()
        {
            var buffer = new InputBuffer(0.15f);
            buffer.Press(1f);
            Assert.IsTrue(buffer.TryConsume(1.1f));
        }

        [Test]
        public void PressExpiresAfterWindow()
        {
            var buffer = new InputBuffer(0.15f);
            buffer.Press(1f);
            Assert.IsFalse(buffer.TryConsume(1.2f));
        }

        [Test]
        public void ConsumeClearsBuffer()
        {
            var buffer = new InputBuffer(0.15f);
            buffer.Press(1f);
            Assert.IsTrue(buffer.TryConsume(1f));
            Assert.IsFalse(buffer.TryConsume(1f));
        }

        [Test]
        public void LaterPressRefreshesWindow()
        {
            var buffer = new InputBuffer(0.15f);
            buffer.Press(1f);
            buffer.Press(1.1f);
            Assert.IsTrue(buffer.TryConsume(1.2f));
        }

        [Test]
        public void ClearDropsPress()
        {
            var buffer = new InputBuffer(0.15f);
            buffer.Press(1f);
            buffer.Clear();
            Assert.IsFalse(buffer.IsPending(1f));
        }
    }
}
