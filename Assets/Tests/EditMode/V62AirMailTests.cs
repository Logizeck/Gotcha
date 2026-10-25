using System;
using NUnit.Framework;
using Gotcha.Levels.V62;

namespace Gotcha.Tests
{
    /// <summary>Livello 253: la lettera in volo (aeroplanino, palloncino, candela).</summary>
    public class V62AirMailTests
    {
        [Test]
        public void CorrectPath_Completes()
        {
            var l = new AirMailLogic();
            Assert.IsTrue(l.TryTieLetter());
            Assert.IsTrue(l.LetterOnBalloon);
            Assert.IsTrue(l.TryReleaseKnot());
            Assert.AreEqual(AirMailState.Flying, l.State);
            Assert.IsFalse(l.BalloonTied);
            Assert.IsTrue(l.TryReachRoof());
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryCompleteDelivery());
            Assert.AreEqual(AirMailState.Done, l.State);
                    }

        [Test]
        public void Plane_FallsShort_AndComesBack()
        {
            var l = new AirMailLogic();
            Assert.IsTrue(l.TryThrowPlane());
            Assert.IsTrue(l.InputLocked);
            Assert.IsFalse(l.TryThrowPlane());
            Assert.IsTrue(l.TryLandPlane());
            Assert.AreEqual(AirMailState.Ready, l.State);
            l.TryTieLetter();
            Assert.IsTrue(l.TryThrowPlane());
            Assert.IsTrue(l.TryLandPlane());
            Assert.AreEqual(AirMailState.LetterTied, l.State);   // la lettera resta legata
            Assert.AreEqual(2, l.Throws);
        }

        [Test]
        public void Knot_WithoutLetter_DoesNothing()
        {
            var l = new AirMailLogic();
            Assert.IsFalse(l.TryReleaseKnot());
            Assert.AreEqual(AirMailState.Ready, l.State);
            Assert.IsFalse(l.TryReachRoof());
        }

        [Test]
        public void Candle_PopsBalloon_LetterFalls_BalloonBack()
        {
            var l = new AirMailLogic();
            l.TryTieLetter(); l.TryReleaseKnot();
            Assert.IsTrue(l.TryPop());
            Assert.AreEqual(AirMailState.Ready, l.State);
            Assert.IsTrue(l.BalloonTied);
            Assert.IsFalse(l.LetterOnBalloon);
            Assert.AreEqual(1, l.Pops);
            Assert.IsFalse(l.TryPop());
            Assert.IsTrue(l.TryTieLetter());                 // si riprova, nessun fallimento
        }

        [Test]
        public void Letter_CanBeUntied()
        {
            var l = new AirMailLogic();
            l.TryTieLetter();
            Assert.IsFalse(l.TryTieLetter());
            Assert.IsTrue(l.TryUntieLetter());
            Assert.AreEqual(AirMailState.Ready, l.State);
            Assert.IsFalse(l.TryUntieLetter());
        }

        [Test]
        public void FlameGeometry()
        {
            // palloncino col nodo in (0, 0), raggio 0.5, alto 1.2
            Assert.IsTrue(AirMailLogic.FlameTouches(0f, 0f, 0.5f, 1.2f, 0.1f, 0.5f));    // fiamma dentro la cupola
            Assert.IsTrue(AirMailLogic.FlameTouches(0f, 0f, 0.5f, 1.2f, 0.05f, -0.3f));  // proprio sotto il nodo
            Assert.IsFalse(AirMailLogic.FlameTouches(0f, 0f, 0.5f, 1.2f, 0.05f, -1.2f)); // troppo sotto
            Assert.IsFalse(AirMailLogic.FlameTouches(0f, 0f, 0.5f, 1.2f, 1.2f, 0.5f));   // di lato
            Assert.IsFalse(AirMailLogic.FlameTouches(0f, 0f, 0f, 1.2f, 0f, 0.5f));
            Assert.IsTrue(AirMailLogic.OverRoof(1.6f, 2.0f, 1.2f, 1.8f));
            Assert.IsFalse(AirMailLogic.OverRoof(0.6f, 2.0f, 1.2f, 1.8f));
            Assert.IsFalse(AirMailLogic.OverRoof(1.6f, 1.0f, 1.2f, 1.8f));
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (AirMailState target in Enum.GetValues(typeof(AirMailState)))
            {
                var l = new AirMailLogic();
                switch (target)
                {
                    case AirMailState.LetterTied: l.TryTieLetter(); break;
                    case AirMailState.Flying: l.TryTieLetter(); l.TryReleaseKnot(); break;
                    case AirMailState.PlaneFlying: l.TryThrowPlane(); break;
                    case AirMailState.Delivering: l.TryTieLetter(); l.TryReleaseKnot(); l.TryReachRoof(); break;
                    case AirMailState.Done: l.TryTieLetter(); l.TryReleaseKnot(); l.TryReachRoof(); l.TryCompleteDelivery(); break;
                }
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(AirMailState.Ready, l.State);
                                Assert.IsFalse(l.CompletionReported);
                Assert.AreEqual(0, l.Throws);
                Assert.IsTrue(l.TryThrowPlane());
                Assert.IsTrue(l.TryLandPlane());
                Assert.AreEqual(AirMailState.Ready, l.State);
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(1, 2, 3, 4, 5, 6)] int seed)
        {
            var rnd = new Random(seed);
            var l = new AirMailLogic();
            int completions = 0;
            for (int step = 0; step < 500; step++)
            {
                switch (rnd.Next(12))
                {
                    case 0: l.TryThrowPlane(); break;
                    case 1: l.TryLandPlane(); break;
                    case 2: l.TryTieLetter(); break;
                    case 3: l.TryUntieLetter(); break;
                    case 4: l.TryReleaseKnot(); break;
                    case 5: l.TryPop(); break;
                    case 6: l.TryReachRoof(); break;
                    case 7: l.TryPop(); break;
                    case 8: l.TryLandPlane(); break;
                    case 9: if (l.TryCompleteDelivery()) completions++; break;
                    default: if (rnd.Next(60) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == AirMailState.Done, l.CompletionReported);
                Assert.IsFalse(l.BalloonTied && l.State == AirMailState.Flying);
            }
        }
    }
}
