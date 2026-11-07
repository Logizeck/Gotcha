using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 119 della roadmap: The Fireless Dragon (tocchi e scossoni no, la fiammella del falò in bocca sì).</summary>
    public class V87FirelessDragonTests
    {
        // stessa geometria del gioco (GotchaLevels87A / DoodleSpritesV87A)
        const float MouthX = 0.33f, MouthY = -0.48f, MouthR = 0.7f, FireX = 1.5f, FireY = -2.0f, FireR = 0.6f;
        const float Frame = 1f / 60f;

        static FirelessDragonLogic New() => new FirelessDragonLogic(MouthX, MouthY, MouthR, FireX, FireY, FireR);

        static void Run(FirelessDragonLogic l, float seconds)
        {
            for (float t = 0f; t < seconds - 1e-4f; t += Frame) l.Tick(Frame);
        }

        [Test]
        public void StartsPlayingWithFlameOnFire()
        {
            var l = New();
            Assert.AreEqual(FirelessDragonStage.Playing, l.Stage);
            Assert.AreEqual(FirelessDragonFlame.OnFire, l.Flame);
            Assert.IsTrue(l.CanPick);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.InMouth(FireX, FireY));
            Assert.IsTrue(l.NearFire(FireX, FireY));
        }

        [Test]
        public void TapsNeverWin()
        {
            var l = New();
            for (int i = 0; i < 200; i++)
            {
                Assert.AreEqual(FirelessDragonEvent.Bubbles, l.TapDragon());
                Assert.AreEqual(FirelessDragonEvent.Popped, l.PopBubble());
            }
            Assert.AreEqual(FirelessDragonStage.Playing, l.Stage);
            Assert.AreEqual(200, l.Taps);
            Assert.AreEqual(200, l.Pops);
            Assert.AreEqual(200, l.WrongMoves);   // le bolle scoppiate non contano
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ShakeCoughsWithCooldownAndNeverWins()
        {
            var l = New();
            Assert.AreEqual(FirelessDragonEvent.Cough, l.Shake());
            // scossone lungo: molti fotogrammi sopra la soglia, una tosse sola finché dura la pausa
            int coughs = 0;
            for (int i = 0; i < 50; i++) { l.Tick(Frame); if (l.Shake() == FirelessDragonEvent.Cough) coughs++; }
            Assert.AreEqual(0, coughs);
            Run(l, FirelessDragonLogic.CoughCooldown);
            Assert.AreEqual(FirelessDragonEvent.Cough, l.Shake());
            for (int i = 0; i < 20; i++) { Run(l, 1.05f); l.Shake(); }
            Assert.AreEqual(22, l.Coughs);
            Assert.AreEqual(FirelessDragonStage.Playing, l.Stage);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void FlameIntoMouthWinsOnce()
        {
            var l = New();
            Assert.AreEqual(FirelessDragonEvent.Picked, l.Pick());
            Assert.AreEqual(FirelessDragonFlame.Held, l.Flame);
            Assert.AreEqual(FirelessDragonEvent.None, l.Move(1.2f, -1.5f));
            Assert.AreEqual(FirelessDragonEvent.Absorbed, l.Release(MouthX + 0.2f, MouthY - 0.1f, MouthX + 0.2f, MouthY - 0.1f, true));
            Assert.AreEqual(FirelessDragonStage.Celebrating, l.Stage);
            Assert.AreEqual(FirelessDragonFlame.Eaten, l.Flame);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(FirelessDragonStage.Done, l.Stage);
            // dopo la vittoria niente cambia
            Assert.AreEqual(FirelessDragonEvent.Ignored, l.TapDragon());
            Assert.AreEqual(FirelessDragonEvent.Ignored, l.Shake());
            Assert.AreEqual(FirelessDragonEvent.None, l.Pick());
            Assert.AreEqual(FirelessDragonEvent.None, l.Cancel());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void DraggingIntoMouthAbsorbsImmediately()
        {
            var l = New();
            l.Pick();
            Assert.AreEqual(FirelessDragonEvent.Absorbed, l.Move(MouthX, MouthY));
            Assert.AreEqual(FirelessDragonEvent.None, l.Move(MouthX, MouthY));   // una volta sola
            Assert.AreEqual(FirelessDragonEvent.Ignored, l.Release(MouthX, MouthY, MouthX, MouthY, false));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void FingerInMouthIsEnough()
        {
            var l = New();
            l.Pick();
            Assert.AreEqual(FirelessDragonEvent.Absorbed, l.Release(MouthX + 1.2f, MouthY, MouthX + 0.3f, MouthY, false));
        }

        [Test]
        public void FlameElsewhereFizzlesAndRespawns()
        {
            var l = New();
            l.Pick();
            Assert.AreEqual(FirelessDragonEvent.Fizzled, l.Release(-2f, 2f, -2f, 2f, false));
            Assert.AreEqual(FirelessDragonFlame.Fizzling, l.Flame);
            Assert.IsFalse(l.CanPick);
            Assert.AreEqual(FirelessDragonEvent.None, l.Pick());
            l.Tick(FirelessDragonLogic.RespawnTime * 0.5f);
            Assert.AreEqual(0.5f, l.FizzleProgress, 0.01f);
            Assert.AreEqual(FirelessDragonEvent.Respawned, l.Tick(FirelessDragonLogic.RespawnTime * 0.5f));
            Assert.AreEqual(FirelessDragonFlame.OnFire, l.Flame);
            Assert.IsTrue(l.CanPick);
            Assert.AreEqual(1, l.Fizzles);
            Assert.AreEqual(FirelessDragonStage.Playing, l.Stage);
        }

        [Test]
        public void FlameOnDragonBodySinges()
        {
            var l = New();
            l.Pick();
            Assert.AreEqual(FirelessDragonEvent.Singed, l.Release(-1.2f, -1.6f, -1.2f, -1.6f, true));
            Assert.AreEqual(FirelessDragonFlame.Fizzling, l.Flame);
            Run(l, FirelessDragonLogic.RespawnTime + 0.05f);
            Assert.IsTrue(l.CanPick);
        }

        [Test]
        public void FlameDroppedNearFireJustReturns()
        {
            var l = New();
            l.Pick();
            Assert.AreEqual(FirelessDragonEvent.Returned, l.Release(FireX + 0.2f, FireY - 0.3f, FireX, FireY, false));
            Assert.AreEqual(FirelessDragonFlame.OnFire, l.Flame);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void ThreeWrongAttemptsThenSolutionStillWorks()
        {
            var l = New();
            l.TapDragon();
            l.Shake();
            l.Pick();
            l.Release(-2f, -4f, -2f, -4f, false);
            Run(l, 1f);
            l.Pick();
            l.Release(-1f, -1.5f, -1f, -1.5f, true);
            Run(l, 1f);
            Assert.AreEqual(4, l.WrongMoves);
            Assert.AreEqual(FirelessDragonEvent.Picked, l.Pick());
            Assert.AreEqual(FirelessDragonEvent.Absorbed, l.Move(MouthX, MouthY + 0.5f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void RapidRepeatedTouchesAreSafe()
        {
            var l = New();
            for (int i = 0; i < 100; i++)
            {
                l.Pick();
                l.Release(FireX, FireY, FireX, FireY, false);   // tocco sulla fiammella: torna su
                l.TapDragon();
                l.Pick();
                l.Release(2f, 1f, 2f, 1f, false);               // la seconda volta si spegne
                Assert.AreEqual(FirelessDragonEvent.None, l.Pick());   // spenta: niente da afferrare
                l.Tick(Frame);
            }
            Assert.AreEqual(FirelessDragonStage.Playing, l.Stage);
            Assert.AreEqual(FirelessDragonFlame.Fizzling, l.Flame);
            Run(l, FirelessDragonLogic.RespawnTime + 0.05f);
            Assert.IsTrue(l.CanPick);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ReleaseWithoutHoldingDoesNothing()
        {
            var l = New();
            Assert.AreEqual(FirelessDragonEvent.None, l.Release(MouthX, MouthY, MouthX, MouthY, true));
            Assert.AreEqual(FirelessDragonEvent.None, l.Move(MouthX, MouthY));
            Assert.AreEqual(FirelessDragonStage.Playing, l.Stage);
        }

        [Test]
        public void CancelReturnsFlameToFire()
        {
            var l = New();
            Assert.AreEqual(FirelessDragonEvent.None, l.Cancel());
            l.Pick();
            Assert.AreEqual(FirelessDragonEvent.Returned, l.Cancel());   // pausa a metà trascinamento
            Assert.AreEqual(FirelessDragonFlame.OnFire, l.Flame);
            l.Pick();
            l.Release(-2f, 2f, -2f, 2f, false);
            Assert.AreEqual(FirelessDragonEvent.Returned, l.Cancel());   // pausa durante lo spegnimento
            Assert.IsTrue(l.CanPick);
            Assert.AreEqual(FirelessDragonEvent.None, l.Tick(5f));        // nessuna ricomparsa doppia
            Assert.AreEqual(FirelessDragonStage.Playing, l.Stage);
        }

        [Test]
        public void CancelDuringCelebrationKeepsWin()
        {
            var l = New();
            l.Pick();
            l.Move(MouthX, MouthY);
            Assert.AreEqual(FirelessDragonEvent.None, l.Cancel());
            Assert.AreEqual(FirelessDragonStage.Celebrating, l.Stage);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = New();
            for (int i = 0; i < 6; i++) l.TapDragon();
            Assert.IsTrue(l.TakeHint());
            l.Shake();
            l.Pick();
            l.Move(MouthX, MouthY);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(FirelessDragonStage.Playing, l.Stage);
            Assert.AreEqual(FirelessDragonFlame.OnFire, l.Flame);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Taps);
            Assert.AreEqual(0, l.Coughs);
            Assert.AreEqual(FirelessDragonEvent.Cough, l.Shake());   // la pausa della tosse riparte da zero
            Assert.IsFalse(l.TakeHint());
            l.Pick();
            Assert.AreEqual(FirelessDragonEvent.Absorbed, l.Release(MouthX, MouthY, MouthX, MouthY, false));
            Assert.IsTrue(l.TryComplete());   // si può rivincere dopo il rientro
        }

        [Test]
        public void HintComesOnceAfterWrongMoves()
        {
            var l = New();
            for (int i = 0; i < FirelessDragonLogic.HintAfter - 1; i++) { l.TapDragon(); Assert.IsFalse(l.TakeHint()); }
            l.TapDragon();
            Assert.IsTrue(l.TakeHint());
            for (int i = 0; i < 10; i++) { l.TapDragon(); Assert.IsFalse(l.TakeHint()); }
        }

        [Test]
        public void BadTimeStepsAreIgnored()
        {
            var l = New();
            l.Pick();
            l.Release(-2f, 2f, -2f, 2f, false);
            Assert.AreEqual(FirelessDragonEvent.None, l.Tick(float.NaN));
            Assert.AreEqual(FirelessDragonEvent.None, l.Tick(-1f));
            Assert.AreEqual(FirelessDragonEvent.None, l.Tick(float.PositiveInfinity));
            Assert.AreEqual(FirelessDragonFlame.Fizzling, l.Flame);
            Assert.AreEqual(FirelessDragonEvent.Respawned, l.Tick(1f));
        }
    }
}
