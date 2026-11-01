using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 2 della roadmap: il pulsante timido (un dito lo blocca, il secondo lo preme).</summary>
    public class V76ShyButtonTests
    {
        const float MinX = -1.15f, MaxX = 1.15f, MinY = -3.3f, MaxY = 1.9f, StartX = 0f, StartY = -0.6f;
        const int A = 0, B = 1;

        static ShyButtonLogic New() => new ShyButtonLogic(MinX, MaxX, MinY, MaxY, StartX, StartY);

        static void Settle(ShyButtonLogic l)
        {
            if (l.State == ShyButtonState.Dodging) Assert.AreEqual(ShyButtonEvent.Landed, l.Tick(ShyButtonLogic.DodgeTime + 0.01f));
            Assert.AreEqual(ShyButtonState.Free, l.State);
        }

        /// <summary>Un dito appoggiato al centro e tenuto finché si blocca.</summary>
        static void Pin(ShyButtonLogic l, int finger)
        {
            Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(finger, l.X, l.Y));
            Assert.AreEqual(ShyButtonEvent.None, l.Tick(0.1f));
            Assert.AreEqual(ShyButtonEvent.Pinned, l.Tick(0.2f));
            Assert.AreEqual(ShyButtonState.Pinned, l.State);
            Assert.AreEqual(finger, l.Holder);
        }

        static void Tap(ShyButtonLogic l, int finger)
        {
            Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(finger, l.X, l.Y));
            l.Tick(0.08f);
            Assert.AreEqual(ShyButtonEvent.Dodged, l.Up(finger));
        }

        static void AssertInBounds(ShyButtonLogic l)
        {
            Assert.That(l.X, Is.InRange(MinX, MaxX));
            Assert.That(l.Y, Is.InRange(MinY, MaxY));
        }

        [Test]
        public void StartsFreeInTheMiddle()
        {
            var l = New();
            Assert.AreEqual(ShyButtonState.Free, l.State);
            Assert.AreEqual(StartX, l.X, 1e-5f);
            Assert.AreEqual(StartY, l.Y, 1e-5f);
            Assert.AreEqual(0, l.Dodges);
            Assert.AreEqual(0, l.Nervous);
            Assert.IsFalse(l.Sweating);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void CorrectSolutionWinsExactlyOnce()
        {
            var l = New();
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(B, l.X + 0.3f, l.Y - 0.2f));
            Assert.AreEqual(ShyButtonState.Won, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            // tutto il resto dopo la vittoria non cambia niente
            Assert.AreEqual(ShyButtonEvent.Ignored, l.Down(2, l.X, l.Y));
            Assert.AreEqual(ShyButtonEvent.None, l.Up(A));
            Assert.AreEqual(ShyButtonEvent.None, l.Up(B));
            Assert.AreEqual(ShyButtonEvent.None, l.Tick(5f));
            Assert.AreEqual(ShyButtonState.Won, l.State);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void PinNeedsHoldTime()
        {
            var l = New();
            Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(A, l.X, l.Y));
            Assert.AreEqual(ShyButtonEvent.None, l.Tick(ShyButtonLogic.PinTime - 0.05f));
            Assert.AreEqual(ShyButtonState.Holding, l.State);
            // secondo dito prima del blocco: non vince e non lo fa scappare
            Assert.AreEqual(ShyButtonEvent.None, l.Down(B, l.X, l.Y));
            Assert.AreEqual(ShyButtonEvent.None, l.Up(B));
            Assert.AreEqual(ShyButtonState.Holding, l.State);
            Assert.AreEqual(ShyButtonEvent.Pinned, l.Tick(0.06f));
            // ora un tocco NUOVO del secondo dito vince
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(B, l.X, l.Y));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void QuickTapDodgesOnReleaseNotInstantly()
        {
            var l = New();
            float x0 = l.X, y0 = l.Y;
            Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(A, x0, y0));
            Assert.AreEqual(x0, l.X, 1e-5f);   // il primo contatto non lo fa scappare
            Assert.AreEqual(y0, l.Y, 1e-5f);
            Assert.AreEqual(ShyButtonEvent.Dodged, l.Up(A));
            Assert.AreEqual(ShyButtonState.Dodging, l.State);
            Assert.AreEqual(1, l.Dodges);
            Assert.AreEqual(x0, l.FromX, 1e-5f);
            Assert.GreaterOrEqual(Dist(x0, y0, l.X, l.Y), ShyButtonLogic.MinJump - 1e-4f);
            AssertInBounds(l);
            Assert.AreEqual(ShyButtonEvent.None, l.Tick(0.1f));
            Assert.AreEqual(ShyButtonEvent.Landed, l.Tick(0.3f));
            Assert.AreEqual(ShyButtonState.Free, l.State);
        }

        [Test]
        public void SingleFingerNeverWins()
        {
            var l = New();
            for (int i = 0; i < 40; i++)
            {
                // tocchi, doppi tocchi, tenute lunghissime: sempre lo stesso dito
                Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(A, l.X, l.Y));
                l.Tick(i % 3 == 0 ? 0.05f : 2f);
                Assert.AreNotEqual(ShyButtonState.Won, l.State);
                Assert.AreEqual(ShyButtonEvent.None, l.Down(A, l.X, l.Y));   // lo stesso id "di nuovo giù" non conta
                Assert.AreEqual(ShyButtonEvent.Dodged, l.Up(A));
                Settle(l);
                AssertInBounds(l);
            }
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(40, l.Dodges);
        }

        [Test]
        public void ReleasingThePinFingerFirstLetsItEscape()
        {
            var l = New();
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.Dodged, l.Up(A));
            Assert.AreEqual(ShyButtonState.Dodging, l.State);
            Assert.AreEqual(ShyButtonLogic.NoFinger, l.Holder);
            // il secondo dito arriva tardi sul vecchio posto (o durante la fuga): niente
            Assert.AreEqual(ShyButtonEvent.Ignored, l.Down(B, l.FromX, l.FromY));
            Settle(l);
            Assert.AreEqual(ShyButtonEvent.None, l.Up(B));
            Assert.IsFalse(l.TryComplete());
            // e si può rifare tutto da capo
            Pin(l, B);
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(A, l.X, l.Y));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void SecondFingerFarAwayDoesNotCount()
        {
            var l = New();
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.Missed, l.Down(B, l.X + 2.2f, l.Y + 1.0f));
            Assert.AreEqual(ShyButtonState.Pinned, l.State);
            Assert.AreEqual(ShyButtonEvent.None, l.Up(B));
            Assert.AreEqual(ShyButtonState.Pinned, l.State);
            // appena fuori dal bordo vale (raggio generoso)
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(B, l.X + ShyButtonLogic.GrabRadius + 0.3f, l.Y));
        }

        [Test]
        public void DodgeTargetAlwaysInsideBounds()
        {
            foreach (int seed in new[] { 1, 7, 76, 1234 })
            {
                var l = new ShyButtonLogic(MinX, MaxX, MinY, MaxY, StartX, StartY, seed);
                for (int i = 0; i < 300; i++)
                {
                    float fx = i % 2 == 0 ? l.X : l.X + 0.5f;
                    Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(i % 2, fx, l.Y));
                    Assert.AreEqual(ShyButtonEvent.Dodged, i % 3 == 0 ? l.Move(i % 2, l.X + 3f, l.Y) : l.Up(i % 2));
                    AssertInBounds(l);
                    Settle(l);
                }
            }
        }

        [Test]
        public void StartPositionIsClampedIntoBounds()
        {
            var l = new ShyButtonLogic(MinX, MaxX, MinY, MaxY, 9f, -9f);
            AssertInBounds(l);
        }

        [Test]
        public void RapidTapsDuringTheDodgeAreIgnored()
        {
            var l = New();
            Tap(l, A);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(ShyButtonEvent.Ignored, l.Down(i % 2, l.X, l.Y));
                Assert.AreEqual(ShyButtonEvent.None, l.Up(i % 2));
                l.Tick(0.02f);
            }
            Assert.AreEqual(1, l.Dodges);
            Settle(l);
            // dopo la fuga un tocco veloce lo fa scappare di nuovo
            Tap(l, B);
            Assert.AreEqual(2, l.Dodges);
        }

        [Test]
        public void ThreeWrongAttemptsThenSweatAndHint()
        {
            var l = New();
            Assert.AreEqual(ShyButtonEvent.Flinch, l.Down(A, l.X + 1.5f, l.Y));   // vicino ma fuori: sobbalza
            Assert.AreEqual(ShyButtonState.Free, l.State);
            Assert.AreEqual(ShyButtonEvent.None, l.Up(A));
            Assert.AreEqual(ShyButtonEvent.None, l.Down(A, l.X + 3f, l.Y + 3f));  // lontano: niente
            Assert.AreEqual(ShyButtonEvent.None, l.Up(A));
            for (int i = 0; i < 3; i++) { Tap(l, A); Settle(l); }
            Assert.AreEqual(1, l.Nervous);
            Assert.IsTrue(l.Sweating);
            Assert.IsFalse(l.TakeHint());
            // dito che scivola via mentre lo tiene: scappa anche così
            Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(A, l.X, l.Y));
            Assert.AreEqual(ShyButtonEvent.None, l.Move(A, l.X + 0.5f, l.Y));
            Assert.AreEqual(ShyButtonEvent.Dodged, l.Move(A, l.X + 2f, l.Y));
            Assert.AreEqual(ShyButtonEvent.None, l.Up(A));
            Settle(l);
            Tap(l, B);
            Settle(l);
            Assert.AreEqual(5, l.Dodges);
            Assert.AreEqual(2, l.Nervous);
            Assert.IsTrue(l.TakeHint());
            Assert.IsFalse(l.TakeHint());
            // nessuno stato bloccato: si vince ancora
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(B, l.X, l.Y));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void PinnedFingerSlidingOffLetsItEscape()
        {
            var l = New();
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.None, l.Move(A, l.X + 1.0f, l.Y));
            Assert.AreEqual(ShyButtonState.Pinned, l.State);
            Assert.AreEqual(ShyButtonEvent.Dodged, l.Move(A, l.X + ShyButtonLogic.SlipRadius + 0.1f, l.Y));
            Assert.AreEqual(ShyButtonEvent.None, l.Move(A, l.X, l.Y));   // ormai è un dito qualsiasi
            Settle(l);
        }

        [Test]
        public void NudgeOncePerPin()
        {
            var l = New();
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.None, l.Tick(ShyButtonLogic.NudgeTime - 0.1f));
            Assert.AreEqual(ShyButtonEvent.Nudge, l.Tick(0.2f));
            Assert.AreEqual(ShyButtonEvent.None, l.Tick(10f));
            Assert.AreEqual(ShyButtonState.Pinned, l.State);
        }

        [Test]
        public void CancelReturnsToAValidFreeState()
        {
            var l = New();
            // a metà presa
            Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(A, l.X, l.Y));
            l.Cancel();
            Assert.AreEqual(ShyButtonState.Free, l.State);
            Assert.AreEqual(0, l.Dodges);
            Assert.AreEqual(ShyButtonEvent.None, l.Up(A));   // la risalita persa non fa niente
            // bloccato
            Pin(l, A);
            l.Cancel();
            Assert.AreEqual(ShyButtonState.Free, l.State);
            Assert.AreEqual(ShyButtonLogic.NoFinger, l.Holder);
            Assert.AreEqual(ShyButtonEvent.Grabbed, l.Down(B, l.X, l.Y));   // il secondo dito ora è solo un primo dito
            Assert.AreEqual(ShyButtonState.Holding, l.State);
            l.Cancel();
            // durante la fuga: atterra subito al nuovo posto
            Tap(l, A);
            l.Cancel();
            Assert.AreEqual(ShyButtonState.Free, l.State);
            AssertInBounds(l);
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(B, l.X, l.Y));
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = New();
            for (int i = 0; i < 6; i++) { Tap(l, A); Settle(l); }
            Assert.IsTrue(l.TakeHint());
            float firstX, firstY;
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(B, l.X, l.Y));
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(ShyButtonState.Free, l.State);
            Assert.AreEqual(0, l.Dodges);
            Assert.AreEqual(StartX, l.X, 1e-5f);
            Assert.AreEqual(StartY, l.Y, 1e-5f);
            Assert.IsFalse(l.TakeHint());
            Assert.IsFalse(l.TryComplete());
            // stessa sequenza di fughe dopo il reset (ripetibile)
            Tap(l, A);
            firstX = l.X; firstY = l.Y;
            l.Reset();
            Tap(l, A);
            Assert.AreEqual(firstX, l.X, 1e-5f);
            Assert.AreEqual(firstY, l.Y, 1e-5f);
            // e si vince di nuovo, una volta
            Settle(l);
            Pin(l, A);
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(B, l.X, l.Y));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void FingerOrderDoesNotMatterOnlyDistinctIds()
        {
            var l = New();
            Pin(l, B);   // il "secondo" id tiene, il "primo" preme
            Assert.AreEqual(ShyButtonEvent.Started, l.Down(A, l.X, l.Y));
            var m = New();
            Pin(m, 900);   // dito piantato dell'Editor
            Assert.AreEqual(ShyButtonEvent.Started, m.Down(-1, m.X, m.Y));   // mouse
        }

        static float Dist(float ax, float ay, float bx, float by) => (float)System.Math.Sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));
    }
}
