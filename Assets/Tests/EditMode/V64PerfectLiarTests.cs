using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 257 "The perfect liar": presa a due dita, capovolgimento, spie verdi, TEST, suono.</summary>
    public class V64PerfectLiarTests
    {
        const float Dt = 0.02f;
        const float Upright = -0.95f, Flipped = 0.95f, Flat = 0f;

        /// <summary>Fa girare la logica per "seconds" con gravità e presa costanti; ritorna l'ultimo evento non nullo.</summary>
        static LiarEvent Run(PerfectLiarLogic l, float seconds, float gy, bool left, bool right)
        {
            LiarEvent last = LiarEvent.None;
            for (float t = 0f; t < seconds; t += Dt)
            {
                LiarEvent e = l.Tick(Dt, gy, left, right);
                if (e != LiarEvent.None) last = e;
            }
            return last;
        }

        static void Solve(PerfectLiarLogic l)
        {
            Run(l, 0.2f, Upright, false, false);
            Assert.AreEqual(LiarEvent.GripStarted, Run(l, 0.1f, Upright, true, true));
            Assert.AreEqual(LiarEvent.Corrected, Run(l, 1.0f, Flipped, true, true));
            Assert.AreEqual(LiarEvent.Calibrated, Run(l, 0.1f, Flipped, false, false));
        }

        [Test]
        public void CorrectPath_Completes_Once()
        {
            var l = new PerfectLiarLogic();
            Solve(l);
            Assert.AreEqual(LiarState.Calibrated, l.State);
            Assert.AreEqual(LiarTest.AwaitingStatement, l.PressTest());
            Assert.IsTrue(l.ListeningForStatement);
            Assert.AreEqual(LiarSound.PlayerIsLiar, l.Sound(false));
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(LiarState.Done, l.State);
        }

        [Test]
        public void TestBeforeCorrection_IsContradictory_AndChangesNothing()
        {
            var l = new PerfectLiarLogic();
            Assert.AreEqual(LiarTest.Contradictory, l.PressTest());
            Assert.AreEqual(LiarTest.Contradictory, l.PressTest());
            Assert.AreEqual(2, l.Tests);
            Assert.AreEqual(LiarState.Exploring, l.State);
        }

        [Test]
        public void SoundBeforeFinalTest_NeverWins()
        {
            var l = new PerfectLiarLogic();
            Assert.AreEqual(LiarSound.Ignored, l.Sound(false));          // rumore normale: niente
            Assert.AreEqual(LiarSound.Unstable, l.Sound(true));          // urlo: "UNSTABLE"
            Solve(l);
            Assert.AreEqual(LiarSound.Unstable, l.Sound(true));          // anche calibrata, finché non c'è il TEST
            Assert.AreEqual(LiarState.Calibrated, l.State);
            Assert.AreEqual(2, l.Shouts);
        }

        [Test]
        public void OneFinger_DoesNotGrip()
        {
            var l = new PerfectLiarLogic();
            Run(l, 1.5f, Flipped, true, false);
            Assert.AreEqual(LiarState.Exploring, l.State);
            Run(l, 1.5f, Flipped, false, true);
            Assert.AreEqual(LiarState.Exploring, l.State);
        }

        [Test]
        public void FlipWithoutGrip_DoesNothing_AndGripMustBeHeldDuringFlip()
        {
            var l = new PerfectLiarLogic();
            Run(l, 1f, Flipped, false, false);
            Run(l, 0.5f, Upright, false, false);
            Assert.AreEqual(LiarState.Exploring, l.State);
            Run(l, 0.2f, Upright, true, true);
            Assert.AreEqual(LiarState.Gripping, l.State);
            Run(l, 0.1f, Upright, false, true);                          // un dito si stacca: presa persa
            Assert.AreEqual(LiarState.Exploring, l.State);
            Assert.AreEqual(1, l.GripsLost);
            Run(l, 1f, Flipped, false, true);
            Assert.AreEqual(LiarState.Exploring, l.State);
        }

        [Test]
        public void ReleaseWrongOrder_BeforeFlip_CancelsSafely_ThenRetryWorks()
        {
            var l = new PerfectLiarLogic();
            Run(l, 0.1f, Upright, true, true);
            Run(l, 0.1f, Upright, true, false);
            Run(l, 0.1f, Upright, false, false);
            Assert.AreEqual(LiarState.Exploring, l.State);
            Solve(l);
            Assert.AreEqual(LiarState.Calibrated, l.State);
        }

        [Test]
        public void PartialRotation_AndJitter_DoNotCorrect()
        {
            var l = new PerfectLiarLogic();
            Run(l, 0.1f, Upright, true, true);
            Run(l, 2f, 0.35f, true, true);                               // a metà strada: non basta
            Assert.AreEqual(LiarState.Gripping, l.State);
            // rumore attorno al dritto: mai capovolto
            for (int i = 0; i < 200; i++) l.Tick(Dt, Upright + ((i % 2 == 0) ? 0.3f : -0.3f), true, true);
            Assert.AreEqual(LiarState.Gripping, l.State);
            // un picco brevissimo (un frame) non basta: il filtro lo assorbe
            l.Tick(Dt, 1f, true, true);
            l.Tick(Dt, Upright, true, true);
            Assert.AreEqual(LiarState.Gripping, l.State);
        }

        [Test]
        public void SlowRotation_FromFlat_Works()
        {
            var l = new PerfectLiarLogic();
            Run(l, 0.2f, Flat, true, true);                              // telefono appoggiato: non capovolto
            Assert.AreEqual(LiarState.Gripping, l.State);
            LiarEvent last = LiarEvent.None;
            for (float g = Flat; g <= 0.8f; g += 0.005f)
            {
                LiarEvent e = l.Tick(Dt, g, true, true);
                if (e != LiarEvent.None) last = e;
            }
            last = Run(l, 0.6f, 0.8f, true, true) == LiarEvent.Corrected ? LiarEvent.Corrected : last;
            Assert.AreEqual(LiarEvent.Corrected, last);
            Assert.IsTrue(l.MachineCorrected);
        }

        [Test]
        public void GripStartedAlreadyUpsideDown_NeedsAnOrientationChange()
        {
            var l = new PerfectLiarLogic();
            Run(l, 0.5f, Flipped, false, false);
            Run(l, 1.5f, Flipped, true, true);
            Assert.AreEqual(LiarState.Gripping, l.State);                // non è cambiato niente
            Run(l, 0.5f, Upright, true, true);
            Assert.AreEqual(LiarEvent.Corrected, Run(l, 1f, Flipped, true, true));
        }

        [Test]
        public void Correction_Persists_ThroughOrientationChanges_AndHoldingLonger()
        {
            var l = new PerfectLiarLogic();
            Run(l, 0.1f, Upright, true, true);
            Run(l, 1f, Flipped, true, true);
            Assert.AreEqual(LiarState.Corrected, l.State);
            Run(l, 1f, Upright, true, true);                             // rigirato ma ancora tenuto
            Assert.AreEqual(LiarState.Corrected, l.State);
            Assert.AreEqual(LiarTest.LetGoFirst, l.PressTest());
            Run(l, 0.1f, Upright, false, true);                          // uno solo staccato: aspetta l'altro
            Assert.AreEqual(LiarState.Corrected, l.State);
            Run(l, 0.1f, Upright, false, false);
            Assert.AreEqual(LiarState.Calibrated, l.State);
            Run(l, 1f, Flipped, true, true);                             // ri-afferrare non disfa niente
            Run(l, 1f, Upright, false, false);
            Assert.AreEqual(LiarState.Calibrated, l.State);
        }

        [Test]
        public void ZeroDeltaTime_Paused_DoesNotAdvance()
        {
            var l = new PerfectLiarLogic();
            l.Tick(Dt, Upright, true, true);
            for (int i = 0; i < 100; i++) l.Tick(0f, Flipped, true, true);
            Assert.AreEqual(LiarState.Gripping, l.State);
        }

        [Test]
        public void Suspects_GetAnnoyed_AndShakeOnlySpikes()
        {
            var l = new PerfectLiarLogic();
            Assert.AreEqual(1, l.TapSuspect(0));
            Assert.IsFalse(l.IsAnnoyed(0));
            l.TapSuspect(0); l.TapSuspect(0);
            Assert.IsTrue(l.IsAnnoyed(0));
            Assert.IsFalse(l.IsAnnoyed(1));
            Assert.AreEqual(0, l.TapSuspect(7));
            Assert.IsTrue(l.Shake());
            Assert.AreEqual(LiarState.Exploring, l.State);
        }

        [Test]
        public void AfterWin_EverythingIsIgnored()
        {
            var l = new PerfectLiarLogic();
            Solve(l);
            l.PressTest();
            l.Sound(true);
            Assert.AreEqual(LiarTest.Ignored, l.PressTest());
            Assert.AreEqual(LiarSound.Ignored, l.Sound(true));
            Assert.IsFalse(l.Shake());
            Assert.AreEqual(0, l.TapSuspect(1));
            Assert.AreEqual(LiarEvent.None, Run(l, 1f, Upright, true, true));
        }

        [Test]
        public void Reset_RestoresStart()
        {
            var l = new PerfectLiarLogic();
            Solve(l);
            l.PressTest();
            l.Reset();
            Assert.AreEqual(LiarState.Exploring, l.State);
            Assert.AreEqual(0, l.Tests);
            Assert.IsFalse(l.Flip.UpsideDown);
        }

        // ---------- microfono ----------

        static void Feed(LiarMicGate g, float seconds, float level, ref int sounds, ref int louds)
        {
            for (float t = 0f; t < seconds; t += Dt)
            {
                g.Feed(Dt, level, out bool s, out bool l);
                if (s) sounds++;
                if (l) louds++;
            }
        }

        [Test]
        public void Mic_Quiet_NoSound()
        {
            var g = new LiarMicGate();
            int s = 0, l = 0;
            Feed(g, 5f, 0.04f, ref s, ref l);
            Assert.AreEqual(0, s);
            Assert.AreEqual(0, l);
        }

        [Test]
        public void Mic_ModerateNoise_RaisesThreshold()
        {
            var g = new LiarMicGate();
            int s = 0, l = 0;
            Feed(g, 6f, 0.12f, ref s, ref l);                            // ventilatore acceso
            Assert.AreEqual(0, s);
            Assert.Greater(g.Threshold, 0.2f);
            Feed(g, 0.3f, 0.6f, ref s, ref l);                           // un "ehi!" vero
            Assert.AreEqual(1, s);
        }

        [Test]
        public void Mic_LongSound_CountsOnce_ThenRearms()
        {
            var g = new LiarMicGate();
            int s = 0, l = 0;
            Feed(g, 1f, 0.03f, ref s, ref l);
            Feed(g, 3f, 0.5f, ref s, ref l);
            Assert.AreEqual(1, s);
            Feed(g, 0.1f, 0.03f, ref s, ref l);                          // pausa troppo corta
            Feed(g, 0.5f, 0.5f, ref s, ref l);
            Assert.AreEqual(1, s);
            Feed(g, 0.6f, 0.03f, ref s, ref l);
            Feed(g, 0.5f, 0.5f, ref s, ref l);
            Assert.AreEqual(2, s);
        }

        [Test]
        public void Mic_Loud_IsReportedOncePerBurst()
        {
            var g = new LiarMicGate();
            int s = 0, l = 0;
            Feed(g, 1f, 0.03f, ref s, ref l);
            Feed(g, 1f, 1f, ref s, ref l);
            Assert.AreEqual(1, s);
            Assert.AreEqual(1, l);
            Feed(g, 0.1f, 0.03f, ref s, ref l);
            Feed(g, 0.1f, 1f, ref s, ref l);
            Assert.AreEqual(1, l);
        }

        [Test]
        public void Mic_TinyClick_IsIgnored_AndWarmupIgnoresEverything()
        {
            var g = new LiarMicGate();
            int s = 0, l = 0;
            Feed(g, 0.4f, 1f, ref s, ref l);                             // durante il riscaldamento
            Assert.AreEqual(0, s);
            Feed(g, 1f, 0.03f, ref s, ref l);
            g.Feed(Dt, 0.9f, out bool once, out _);                      // un solo frame: un clic
            Assert.IsFalse(once);
            Feed(g, 0.5f, 0.03f, ref s, ref l);
            Assert.AreEqual(0, s);
        }

        [Test]
        public void Mic_DeniedPermission_ZeroLevel_NeverTriggers()
        {
            var g = new LiarMicGate();
            int s = 0, l = 0;
            Feed(g, 10f, 0f, ref s, ref l);
            g.Feed(Dt, float.NaN, out bool nan, out _);
            Assert.AreEqual(0, s);
            Assert.IsFalse(nan);
        }
    }
}
