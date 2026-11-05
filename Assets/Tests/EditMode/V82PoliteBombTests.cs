using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 42 della roadmap: The Polite Bomb (STOP accelera, i fili danno errori, si stacca la batteria).</summary>
    public class V82PoliteBombTests
    {
        const float D = PoliteBombLogic.RemoveDistance;

        static void AssertStillTicking(PoliteBombLogic l)
        {
            Assert.AreEqual(PoliteBombStage.Ticking, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.BatteryRemoved);
            Assert.IsFalse(l.TryComplete());
        }

        static void AssertBatteryHome(PoliteBombLogic l)
        {
            Assert.IsFalse(l.BatteryHeld);
            Assert.AreEqual(0f, l.BatteryX, 1e-6f);
            Assert.AreEqual(0f, l.BatteryY, 1e-6f);
        }

        /// <summary>Trascina la batteria dal punto (x0, y0) di (dx, dy) a passi; restituisce l'ultimo risultato.</summary>
        static PoliteBombMove Drag(PoliteBombLogic l, int id, float x0, float y0, float dx, float dy, int steps = 10)
        {
            Assert.IsTrue(l.GrabBattery(id, x0, y0));
            PoliteBombMove m = PoliteBombMove.None;
            for (int i = 1; i <= steps; i++)
            {
                m = l.MoveBattery(id, x0 + dx * i / steps, y0 + dy * i / steps);
                if (m == PoliteBombMove.Unplugged) break;
            }
            return m;
        }

        static void Run(PoliteBombLogic l, float seconds, float step = 0.05f)
        {
            for (float t = 0f; t < seconds; t += step) l.Tick(step);
        }

        // ---------- Avvio ----------

        [Test]
        public void StartsAtTenSecondsTicking()
        {
            var l = new PoliteBombLogic();
            AssertStillTicking(l);
            AssertBatteryHome(l);
            Assert.AreEqual("00:10", l.Display);
            Assert.AreEqual(1f, l.Speed);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void TimerCountsDownAndShowsSeconds()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombTick.None, l.Tick(0.5f));
            Assert.AreEqual("00:10", l.Display);
            Assert.AreEqual(PoliteBombTick.Second, l.Tick(0.6f));
            Assert.AreEqual("00:09", l.Display);
            Run(l, 3f);
            Assert.AreEqual("00:06", l.Display);
            AssertStillTicking(l);
        }

        [Test]
        public void BadOrZeroTimeIsIgnored()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombTick.None, l.Tick(0f));
            Assert.AreEqual(PoliteBombTick.None, l.Tick(-1f));
            Assert.AreEqual(PoliteBombTick.None, l.Tick(float.NaN));
            Assert.AreEqual(PoliteBombTick.None, l.Tick(float.PositiveInfinity));
            Assert.AreEqual(PoliteBombLogic.StartSeconds, l.Remaining);
        }

        // ---------- STOP accelera e non vince mai ----------

        [Test]
        public void StopSpeedsUpTheTimerAndNeverWins()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombStop.Faster, l.PressStop());
            Assert.AreEqual(2f, l.Speed);
            Assert.AreEqual(PoliteBombStop.Faster, l.PressStop());
            Assert.AreEqual(PoliteBombStop.Faster, l.PressStop());
            Assert.AreEqual(PoliteBombLogic.MaxSpeed, l.Speed);
            Assert.AreEqual(PoliteBombStop.TopSpeed, l.PressStop());
            Assert.AreEqual(PoliteBombLogic.MaxSpeed, l.Speed);
            Assert.AreEqual(4, l.Stops);
            AssertStillTicking(l);
            // a velocità 4 un secondo vero consuma 4 secondi del display
            l.Tick(1f);
            Assert.AreEqual("00:06", l.Display);
        }

        [Test]
        public void RapidStopSpamNeverWinsAndStaysValid()
        {
            var l = new PoliteBombLogic();
            for (int i = 0; i < 300; i++)
            {
                PoliteBombStop s = l.PressStop();
                Assert.AreNotEqual(PoliteBombStop.Ignored, s);
                l.Tick(0.016f);
                Assert.LessOrEqual(l.Speed, PoliteBombLogic.MaxSpeed);
                Assert.GreaterOrEqual(l.Remaining, 0f);
                Assert.IsFalse(l.TryComplete());
            }
            Assert.AreEqual(300, l.Stops);
            AssertStillTicking(l);
        }

        [Test]
        public void StopDuringPuffIsRestingAndDoesNotCarryOver()
        {
            var l = new PoliteBombLogic();
            l.PressStop();
            Assert.AreEqual(PoliteBombTick.Puff, l.Tick(20f));
            Assert.AreEqual(PoliteBombStop.Resting, l.PressStop());
            Assert.AreEqual(PoliteBombTick.Restart, l.Tick(PoliteBombLogic.PuffPause + 0.01f));
            Assert.AreEqual(1f, l.Speed, "dopo lo sbuffo si riparte a velocità normale");
            Assert.AreEqual("00:10", l.Display);
        }

        // ---------- I fili e i colpetti non vincono mai ----------

        [Test]
        public void WiresOnlyGiveErrorsAndNeverWin()
        {
            var l = new PoliteBombLogic();
            int n = 0;
            for (int round = 0; round < 20; round++)
                for (int w = 0; w < PoliteBombLogic.WireCount; w++)
                {
                    Assert.AreEqual(n++, l.TouchWire(w));
                    AssertStillTicking(l);
                }
            Assert.AreEqual(-1, l.TouchWire(-1));
            Assert.AreEqual(-1, l.TouchWire(PoliteBombLogic.WireCount));
            Assert.AreEqual(60, l.WireTouches);
            Assert.AreEqual(1f, l.Speed, "i fili non cambiano il timer");
        }

        [Test]
        public void PokesOnlyProtest()
        {
            var l = new PoliteBombLogic();
            for (int i = 0; i < 10; i++) Assert.AreEqual(i, l.Poke());
            AssertStillTicking(l);
            Assert.AreEqual(10, l.WrongMoves);
        }

        // ---------- Lo zero è una gag: riparte, mai sconfitta ----------

        [Test]
        public void ReachingZeroPuffsAndRestartsRepeatedly()
        {
            var l = new PoliteBombLogic();
            for (int round = 1; round <= 5; round++)
            {
                int puffs = 0, restarts = 0;
                float total = 0f;
                while (restarts == 0 && total < 30f)
                {
                    PoliteBombTick t = l.Tick(0.05f);
                    total += 0.05f;
                    if (t == PoliteBombTick.Puff) { puffs++; Assert.AreEqual("00:00", l.Display); Assert.IsTrue(l.Puffing); }
                    if (t == PoliteBombTick.Restart) restarts++;
                    AssertStillTicking(l);
                }
                Assert.AreEqual(1, puffs);
                Assert.AreEqual(1, restarts);
                Assert.AreEqual(round, l.Puffs);
                Assert.AreEqual("00:10", l.Display);
                Assert.IsFalse(l.Puffing);
                Assert.AreEqual(PoliteBombLogic.StartSeconds + PoliteBombLogic.PuffPause, total, 0.11f);
            }
        }

        [Test]
        public void HugeTimeStepGivesOnePuffOnly()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombTick.Puff, l.Tick(1000f));
            Assert.AreEqual(1, l.Puffs);
            Assert.AreEqual(PoliteBombTick.Restart, l.Tick(1000f));
            Assert.AreEqual("00:10", l.Display);
            AssertStillTicking(l);
        }

        // ---------- La batteria: poco e lasciata torna ----------

        [Test]
        public void ShortDragReturnsIntoCompartment()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombMove.Pulling, Drag(l, 1, 1.5f, -0.4f, 0.5f, -0.2f));
            Assert.IsTrue(l.Worried);
            Assert.Greater(l.Pull, 0.3f);
            Assert.AreEqual(PoliteBombDrop.SnapBack, l.ReleaseBattery(1));
            AssertBatteryHome(l);
            AssertStillTicking(l);
            Assert.AreEqual(1, l.SnapBacks);
        }

        [Test]
        public void EarlyReleaseAtEveryDistanceBelowThresholdReturns()
        {
            var l = new PoliteBombLogic();
            for (float d = 0.2f; d < D - 0.01f; d += 0.1f)
            {
                Assert.AreEqual(PoliteBombMove.Pulling, Drag(l, 3, 0f, 0f, 0f, -d));
                Assert.AreEqual(PoliteBombDrop.SnapBack, l.ReleaseBattery(3));
                AssertBatteryHome(l);
            }
            AssertStillTicking(l);
        }

        [Test]
        public void TapOnBatteryDoesNothing()
        {
            var l = new PoliteBombLogic();
            Assert.IsTrue(l.GrabBattery(1, 1f, 1f));
            l.MoveBattery(1, 1.05f, 1.02f);
            Assert.AreEqual(PoliteBombDrop.Tap, l.ReleaseBattery(1));
            Assert.AreEqual(1, l.BatteryTaps);
            Assert.AreEqual(0, l.SnapBacks);
            AssertBatteryHome(l);
            AssertStillTicking(l);
        }

        [Test]
        public void PushingIntoTheBombNeverUnplugs()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombMove.Pulling, Drag(l, 1, 2f, 0f, -3f, 0f));
            Assert.AreEqual(-PoliteBombLogic.PushIn, l.BatteryX, 1e-5f);
            Assert.AreEqual(PoliteBombDrop.SnapBack, l.ReleaseBattery(1));
            AssertStillTicking(l);
        }

        // ---------- La soluzione ----------

        [Test]
        public void PullingTheBatteryOutWinsOnce()
        {
            var l = new PoliteBombLogic();
            l.Tick(3.2f);
            Assert.AreEqual(PoliteBombMove.Unplugged, Drag(l, 7, 1.8f, -0.4f, 1.0f, 0f));
            Assert.IsTrue(l.BatteryRemoved);
            Assert.AreEqual(PoliteBombStage.Defused, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual("", l.Display, "display spento");
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(PoliteBombStage.Done, l.Stage);
        }

        [TestCase(1f, 0f)]
        [TestCase(0f, -1f)]
        [TestCase(0f, 1f)]
        [TestCase(0.7f, -0.7f)]
        [TestCase(-0.04f, -1f)]
        public void AnyOutwardDirectionUnplugs(float ux, float uy)
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombMove.Unplugged, Drag(l, 1, 0f, 0f, ux * (D + 0.05f), uy * (D + 0.05f), 20));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void TimerStopsForeverOnceRemoved()
        {
            var l = new PoliteBombLogic();
            l.Tick(4.1f);
            string before = l.DisplaySeconds.ToString();
            Assert.AreEqual(PoliteBombMove.Unplugged, Drag(l, 1, 0f, 0f, 0f, -2f));
            float rem = l.Remaining;
            for (int i = 0; i < 100; i++) Assert.AreEqual(PoliteBombTick.None, l.Tick(0.5f));
            Assert.AreEqual(rem, l.Remaining);
            Assert.AreEqual(0, l.Puffs);
            Assert.AreEqual("6", before);
        }

        [Test]
        public void SolutionWorksDuringThePuff()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombTick.Puff, l.Tick(11f));
            Assert.AreEqual(PoliteBombMove.Unplugged, Drag(l, 1, 0f, 0f, 1.2f, 0f));
            Assert.AreEqual(PoliteBombTick.None, l.Tick(PoliteBombLogic.PuffPause + 1f), "niente ripartenza dopo la vittoria");
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void BatteryCannotReturnOrBeGrabbedAfterRemoval()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombMove.Unplugged, Drag(l, 1, 0f, 0f, 1.5f, 0f));
            // il dito che continua a muoversi o si alza non fa niente
            Assert.AreEqual(PoliteBombMove.None, l.MoveBattery(1, 0f, 0f));
            Assert.AreEqual(PoliteBombDrop.None, l.ReleaseBattery(1));
            Assert.IsTrue(l.BatteryRemoved);
            Assert.IsFalse(l.GrabBattery(1, 0f, 0f));
            Assert.IsFalse(l.GrabBattery(2, 0f, 0f));
            l.Cancel();
            Assert.IsTrue(l.BatteryRemoved, "Cancel non rimette la batteria");
            Assert.AreEqual(PoliteBombStage.Defused, l.Stage);
            // le false piste dopo la vittoria non contano
            Assert.AreEqual(PoliteBombStop.Ignored, l.PressStop());
            Assert.AreEqual(-1, l.TouchWire(0));
            Assert.AreEqual(-1, l.Poke());
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.GrabBattery(3, 0f, 0f));
            Assert.AreEqual(PoliteBombStop.Ignored, l.PressStop());
        }

        // ---------- Dita ----------

        [Test]
        public void OnlyOneFingerHoldsTheBattery()
        {
            var l = new PoliteBombLogic();
            Assert.IsTrue(l.GrabBattery(1, 0f, 0f));
            Assert.IsFalse(l.GrabBattery(2, 0f, 0f));
            Assert.IsFalse(l.GrabBattery(1, 0f, 0f));
            // l'altro dito non la muove né la lascia
            Assert.AreEqual(PoliteBombMove.None, l.MoveBattery(2, 0f, -3f));
            Assert.AreEqual(PoliteBombDrop.None, l.ReleaseBattery(2));
            Assert.IsTrue(l.BatteryHeld);
            Assert.IsFalse(l.BatteryRemoved);
            // e intanto STOP e fili con un secondo dito funzionano come sempre
            Assert.AreEqual(PoliteBombStop.Faster, l.PressStop());
            Assert.AreEqual(0, l.TouchWire(1));
            Assert.AreEqual(PoliteBombMove.Unplugged, l.MoveBattery(1, 0f, -1f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void InvalidFingersAndPositionsAreRejected()
        {
            var l = new PoliteBombLogic();
            Assert.IsFalse(l.GrabBattery(PoliteBombLogic.NoFinger, 0f, 0f));
            Assert.IsFalse(l.GrabBattery(1, float.NaN, 0f));
            Assert.IsFalse(l.GrabBattery(1, 0f, float.PositiveInfinity));
            Assert.IsTrue(l.GrabBattery(1, 0f, 0f));
            Assert.AreEqual(PoliteBombMove.None, l.MoveBattery(1, float.NaN, -5f));
            Assert.IsFalse(l.BatteryRemoved);
            Assert.AreEqual(PoliteBombMove.None, l.MoveBattery(PoliteBombLogic.NoFinger, 0f, -5f));
            Assert.AreEqual(PoliteBombDrop.None, l.ReleaseBattery(PoliteBombLogic.NoFinger));
            Assert.IsTrue(l.BatteryHeld);
        }

        [Test]
        public void RapidGrabReleaseNeverWins()
        {
            var l = new PoliteBombLogic();
            for (int i = 0; i < 200; i++)
            {
                Assert.IsTrue(l.GrabBattery(i, 0f, 0f));
                l.MoveBattery(i, 0f, -(i % 8) * 0.1f);
                PoliteBombDrop d = l.ReleaseBattery(i);
                Assert.AreNotEqual(PoliteBombDrop.None, d);
                AssertBatteryHome(l);
                l.Tick(0.02f);
            }
            AssertStillTicking(l);
        }

        // ---------- Pausa, uscita, rientro ----------

        [Test]
        public void CancelReturnsBatteryWithoutCounting()
        {
            var l = new PoliteBombLogic();
            Drag(l, 1, 0f, 0f, 0f, -0.6f);
            Assert.IsTrue(l.Worried);
            l.Cancel();
            AssertBatteryHome(l);
            Assert.IsFalse(l.Worried);
            Assert.AreEqual(0, l.SnapBacks);
            Assert.AreEqual(PoliteBombDrop.None, l.ReleaseBattery(1), "il dito perso non conta più");
            // dopo la pausa si riprende normalmente
            Assert.AreEqual(PoliteBombMove.Unplugged, Drag(l, 2, 0f, 0f, 0f, -1f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = new PoliteBombLogic();
            l.PressStop(); l.PressStop();
            l.TouchWire(2); l.Poke();
            l.Tick(20f);
            Drag(l, 1, 0f, 0f, 0f, -0.5f);
            l.Reset();
            AssertStillTicking(l);
            AssertBatteryHome(l);
            Assert.AreEqual("00:10", l.Display);
            Assert.AreEqual(1f, l.Speed);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Puffs);

            // anche dopo una vittoria: si rigioca da capo e si vince di nuovo una volta
            Assert.AreEqual(PoliteBombMove.Unplugged, Drag(l, 1, 0f, 0f, 1f, 0f));
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillTicking(l);
            Assert.IsTrue(l.GrabBattery(1, 0f, 0f));
            Assert.AreEqual(PoliteBombMove.Unplugged, l.MoveBattery(1, 1f, 0f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ThreeDifferentWrongAttemptsThenSolution()
        {
            var l = new PoliteBombLogic();
            Assert.AreEqual(PoliteBombStop.Faster, l.PressStop());
            Assert.AreEqual(0, l.TouchWire(0));
            Assert.AreEqual(PoliteBombMove.Pulling, Drag(l, 1, 0f, 0f, 0.4f, 0f));
            Assert.AreEqual(PoliteBombDrop.SnapBack, l.ReleaseBattery(1));
            Run(l, 6f);
            Assert.AreEqual(3, l.WrongMoves);
            AssertStillTicking(l);
            Assert.AreEqual(PoliteBombMove.Unplugged, Drag(l, 1, 0f, 0f, 0.6f, -0.8f));
            Assert.IsTrue(l.TryComplete());
        }
    }
}
