using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 265: la barchetta nella vasca (le false piste non costano niente, il tappo svuota il "mare").</summary>
    public class V69DontLetItSinkTests
    {
        const float Eps = 1e-4f;

        /// <summary>Fa passare il tempo a piccoli passi (come i fotogrammi), fermandosi al primo evento.</summary>
        static SinkEvent Run(DontLetItSinkLogic l, float seconds, float step = 0.05f)
        {
            for (float t = 0f; t < seconds - 1e-5f; t += step)
            {
                SinkEvent e = l.Tick(step);
                if (e != SinkEvent.None) return e;
            }
            return SinkEvent.None;
        }

        static void AssertStillFloating(DontLetItSinkLogic l)
        {
            Assert.AreEqual(SinkStage.Floating, l.Stage);
            Assert.IsFalse(l.Solved);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void StartsFloatingAndEmpty()
        {
            var l = new DontLetItSinkLogic();
            AssertStillFloating(l);
            Assert.AreEqual(0f, l.Level);
            Assert.IsTrue(l.Leaking);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0f, l.Pull);
        }

        [Test]
        public void FillsSlowly_BetweenSixtyAndNinetySeconds()
        {
            var l = new DontLetItSinkLogic();
            Assert.AreEqual(SinkEvent.None, Run(l, 60f));
            Assert.Less(l.Level, 1f);
            Assert.Greater(l.Level, 0.7f);
            Assert.AreEqual(SinkEvent.BoatFull, Run(l, 30f));
            Assert.AreEqual(SinkStage.Sinking, l.Stage);
        }

        [Test]
        public void LongPauseStep_IsClamped()
        {
            var l = new DontLetItSinkLogic();
            l.Tick(600f);   // il gioco è stato in pausa dieci minuti: un passo solo, corto
            Assert.AreEqual(DontLetItSinkLogic.MaxStep / DontLetItSinkLogic.FillSeconds, l.Level, Eps);
            l.Tick(float.NaN);
            l.Tick(-1f);
            l.Tick(0f);
            Assert.AreEqual(DontLetItSinkLogic.MaxStep / DontLetItSinkLogic.FillSeconds, l.Level, Eps);
            AssertStillFloating(l);
        }

        [Test]
        public void Bucket_LowersThenWaterRushesBack()
        {
            var l = new DontLetItSinkLogic();
            Run(l, 30f);
            float before = l.Level;
            float taken = l.Bail();
            Assert.AreEqual(DontLetItSinkLogic.BailAmount, taken, Eps);
            Assert.AreEqual(before - taken, l.Level, Eps);
            Assert.AreEqual(taken, l.RushBack, Eps);
            // dopo RushBackSeconds l'acqua è tornata tutta (più quella entrata dal buco nel frattempo)
            Run(l, DontLetItSinkLogic.RushBackSeconds + 0.1f);
            Assert.AreEqual(0f, l.RushBack, Eps);
            Assert.GreaterOrEqual(l.Level, before);
            Assert.AreEqual(1, l.Bails);
            AssertStillFloating(l);
        }

        [Test]
        public void Bucket_OnEmptyBoat_TakesNothing()
        {
            var l = new DontLetItSinkLogic();
            Assert.AreEqual(0f, l.Bail());
            Assert.AreEqual(0f, l.Level);
            Assert.AreEqual(1, l.Bails);
            AssertStillFloating(l);
        }

        [Test]
        public void Finger_StopsLeakOnlyWhileHeld()
        {
            var l = new DontLetItSinkLogic();
            Run(l, 10f);
            float before = l.Level;
            Assert.IsTrue(l.SetFinger(true));
            Assert.IsFalse(l.SetFinger(true));   // già lì
            Assert.IsFalse(l.Leaking);
            Run(l, 20f);
            Assert.AreEqual(before, l.Level, Eps);
            Assert.IsTrue(l.SetFinger(false));
            Assert.IsTrue(l.Leaking);
            Run(l, 5f);
            Assert.Greater(l.Level, before);
            Assert.AreEqual(1, l.FingerPlugs);
            AssertStillFloating(l);
        }

        [Test]
        public void Tape_HoldsForAFewSecondsThenPeels()
        {
            var l = new DontLetItSinkLogic();
            Run(l, 10f);
            float before = l.Level;
            Assert.AreEqual(1, l.ApplyPatch());
            Assert.AreEqual(0, l.ApplyPatch());   // c'è già
            Assert.IsTrue(l.Patched);
            Assert.IsFalse(l.Leaking);
            Assert.AreEqual(SinkEvent.PatchPeeled, Run(l, 10f));
            Assert.AreEqual(before, l.Level, 0.01f);
            Assert.IsFalse(l.Patched);
            Assert.IsTrue(l.Leaking);
            Assert.AreEqual(2, l.ApplyPatch());   // si può rimettere (la seconda toppa)
            Assert.AreEqual(2, l.Patches);
            AssertStillFloating(l);
        }

        [Test]
        public void DuckAndWaves_ChangeNothing()
        {
            var l = new DontLetItSinkLogic();
            Run(l, 20f);
            float before = l.Level;
            Assert.AreEqual(1, l.TapDuck());
            Assert.AreEqual(2, l.TapDuck());
            Assert.AreEqual(1, l.Rock());
            Assert.AreEqual(before, l.Level, Eps);
            Assert.AreEqual(3, l.WrongMoves);
            AssertStillFloating(l);
        }

        [Test]
        public void Tilt_HasHysteresis_AndFiresOncePerTilt()
        {
            var l = new DontLetItSinkLogic();
            int fired = 0;
            // telefono dritto con un po' di tremolio: niente onde
            for (int i = 0; i < 100; i++) if (l.FeedTilt(0.1f * (i % 2 == 0 ? 1f : -1f), -1f, 0.02f)) fired++;
            Assert.AreEqual(0, fired);
            // inclinato di lato e tenuto: una sola volta
            for (int i = 0; i < 100; i++) if (l.FeedTilt(0.7f, -0.7f, 0.02f)) fired++;
            Assert.AreEqual(1, fired);
            // torna appena sotto la soglia di accensione ma sopra quella di spegnimento: niente
            for (int i = 0; i < 100; i++) if (l.FeedTilt(0.35f, -0.9f, 0.02f)) fired++;
            Assert.AreEqual(1, fired);
            // dritto e di nuovo inclinato (dall'altra parte): un'altra volta
            for (int i = 0; i < 100; i++) if (l.FeedTilt(0f, -1f, 0.02f)) fired++;
            for (int i = 0; i < 100; i++) if (l.FeedTilt(-0.7f, -0.7f, 0.02f)) fired++;
            Assert.AreEqual(2, fired);
            AssertStillFloating(l);
        }

        [Test]
        public void FullBoat_SinksAndComesBackEmpty_WithoutFailing()
        {
            var l = new DontLetItSinkLogic();
            l.TapDuck();
            l.Bail();
            l.SetFinger(true);
            l.SetFinger(false);
            int wrong = l.WrongMoves;
            Assert.AreEqual(SinkEvent.BoatFull, Run(l, 200f));
            Assert.AreEqual(SinkStage.Sinking, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(1, l.Sinks);
            // mentre va a fondo non si fa niente (e il tempo non conta)
            Assert.AreEqual(PullResult.None, l.PullChain(1f));
            Assert.AreEqual(0f, l.Bail());
            Assert.AreEqual(0, l.ApplyPatch());
            Assert.IsFalse(l.SetFinger(true));
            Assert.AreEqual(SinkEvent.None, l.Tick(1f));
            Assert.IsFalse(l.TryComplete());
            // torna su vuota: si continua, le mosse fatte restano
            Assert.IsTrue(l.Resurface());
            Assert.IsFalse(l.Resurface());
            Assert.AreEqual(0f, l.Level);
            Assert.AreEqual(0f, l.RushBack);
            Assert.AreEqual(wrong, l.WrongMoves);
            AssertStillFloating(l);
            // e può riaffondare ancora (nessun limite, nessuna vita persa)
            Assert.AreEqual(SinkEvent.BoatFull, Run(l, 200f));
            Assert.IsTrue(l.Resurface());
            Assert.AreEqual(2, l.Sinks);
        }

        [Test]
        public void BailedWaterComingBack_NeverOverfills_ThenSinkingClearsTheHole()
        {
            var l = new DontLetItSinkLogic();
            l.AddWater(0.99f);
            Assert.Greater(l.Bail(), 0f);
            l.SetFinger(true);
            Assert.AreEqual(SinkEvent.None, Run(l, 10f));   // torna solo l'acqua tolta: col buco tappato non si riempie
            Assert.AreEqual(0.99f, l.Level, Eps);
            l.SetFinger(false);
            Assert.AreEqual(SinkEvent.BoatFull, Run(l, 2f));
            Assert.IsFalse(l.FingerOnHole);
            Assert.IsFalse(l.Patched);
            Assert.AreEqual(0f, l.Pull);
        }

        [Test]
        public void PartialPull_SnapsBack_NothingLost()
        {
            var l = new DontLetItSinkLogic();
            Run(l, 20f);
            float before = l.Level;
            Assert.AreEqual(PullResult.Partial, l.PullChain(0.3f));
            Assert.AreEqual(PullResult.Partial, l.PullChain(0.95f));
            Assert.AreEqual(0.95f, l.Pull, Eps);
            Assert.IsTrue(l.ReleaseChain());
            Assert.AreEqual(0f, l.Pull);
            Assert.IsFalse(l.ReleaseChain());   // già giù
            Assert.AreEqual(before, l.Level, 0.01f);
            Assert.AreEqual(1, l.PartialPulls);
            AssertStillFloating(l);
            // la catenella si può riprendere quando si vuole
            Assert.AreEqual(PullResult.Unplugged, l.PullChain(1f));
        }

        [Test]
        public void PullWeirdValues_AreClamped()
        {
            var l = new DontLetItSinkLogic();
            Assert.AreEqual(PullResult.Partial, l.PullChain(float.NaN));
            Assert.AreEqual(0f, l.Pull);
            Assert.AreEqual(PullResult.Partial, l.PullChain(-3f));
            Assert.AreEqual(0f, l.Pull);
            Assert.AreEqual(PullResult.Unplugged, l.PullChain(7f));
            Assert.AreEqual(1f, l.Pull);
        }

        [Test]
        public void FullPull_Drains_AndWinsOnce()
        {
            var l = new DontLetItSinkLogic();
            l.Bail();
            l.ApplyPatch();
            l.SetFinger(true);
            Run(l, 40f);
            Assert.AreEqual(PullResult.Unplugged, l.PullChain(1f));
            Assert.AreEqual(SinkStage.Draining, l.Stage);
            Assert.IsTrue(l.Solved);
            Assert.IsTrue(l.InputLocked);
            Assert.IsFalse(l.FingerOnHole);
            Assert.IsFalse(l.Patched);
            // svuotando: niente più acqua che entra, niente affondamento, niente mosse
            float lvl = l.Level;
            Assert.AreEqual(SinkEvent.None, Run(l, 300f));
            Assert.AreEqual(lvl, l.Level, Eps);
            Assert.AreEqual(PullResult.None, l.PullChain(1f));
            Assert.IsFalse(l.ReleaseChain());
            Assert.AreEqual(0, l.TapDuck());
            Assert.AreEqual(0, l.Rock());
            Assert.IsFalse(l.Resurface());
            Assert.IsFalse(l.TryComplete());   // prima deve posarsi sul fondo
            Assert.IsTrue(l.FinishDrain());
            Assert.IsFalse(l.FinishDrain());
            Assert.AreEqual(SinkStage.Landed, l.Stage);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(SinkStage.Done, l.Stage);
            Assert.IsTrue(l.CompletionReported);
        }

        [Test]
        public void WinsEvenRightAfterResurfacing()
        {
            var l = new DontLetItSinkLogic();
            Run(l, 200f);
            l.Resurface();
            Assert.AreEqual(PullResult.Unplugged, l.PullChain(1f));
            Assert.IsTrue(l.FinishDrain());
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void TiltWhileNotFloating_IsNotAWave()
        {
            var l = new DontLetItSinkLogic();
            l.PullChain(1f);
            bool fired = false;
            for (int i = 0; i < 50; i++) fired |= l.FeedTilt(0.9f, -0.4f, 0.02f);
            Assert.IsFalse(fired);
        }

        [Test]
        public void Reset_ClearsEverything()
        {
            var l = new DontLetItSinkLogic();
            l.TapDuck();
            l.Rock();
            l.Bail();
            l.ApplyPatch();
            l.SetFinger(true);
            l.PullChain(0.5f);
            l.ReleaseChain();
            l.SetFinger(false);
            Run(l, 200f);
            l.Resurface();
            l.PullChain(1f);
            l.FinishDrain();
            l.TryComplete();
            l.Reset();
            AssertStillFloating(l);
            Assert.AreEqual(0f, l.Level);
            Assert.AreEqual(0f, l.RushBack);
            Assert.AreEqual(0f, l.Pull);
            Assert.IsFalse(l.FingerOnHole);
            Assert.IsFalse(l.Patched);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Sinks);
            Assert.IsFalse(l.CompletionReported);
            Assert.AreEqual(0f, l.TiltX);
            Assert.AreEqual(PullResult.Unplugged, l.PullChain(1f));
            Assert.IsTrue(l.FinishDrain());
            Assert.IsTrue(l.TryComplete());
        }
    }
}
