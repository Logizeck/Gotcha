using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 24 della roadmap: The Lazy Sun (abbassare la collina per scoprire il sole che dorme).</summary>
    public class V79LazySunTests
    {
        const float Travel = 3.1f, TopY = 0.35f;

        static void AssertStillPlaying(LazySunLogic l)
        {
            Assert.AreEqual(LazySunStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        /// <summary>Prende la collina, la porta a <paramref name="to"/> e la lascia.</summary>
        static LazySunRelease Lower(LazySunLogic l, float to)
        {
            Assert.IsTrue(l.GrabHill());
            l.DragHill(to);
            return l.ReleaseHill();
        }

        static void Run(LazySunLogic l, float seconds, float step = 1f / 60f)
        {
            for (float t = 0f; t < seconds; t += step) l.Tick(step);
        }

        [Test]
        public void StartsAsleepAndCovered()
        {
            var l = new LazySunLogic();
            AssertStillPlaying(l);
            Assert.AreEqual(0f, l.Lowered);
            Assert.IsFalse(l.Holding);
            Assert.IsFalse(l.SunPeeking);
            Assert.IsFalse(l.PastThreshold);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void TapsNeverWin()
        {
            var l = new LazySunLogic();
            foreach (LazySunSpot s in System.Enum.GetValues(typeof(LazySunSpot)))
                for (int i = 0; i < 5; i++) Assert.IsTrue(l.Tap(s));
            AssertStillPlaying(l);
            Assert.AreEqual(0f, l.Lowered);
            Assert.AreEqual(5, l.Taps(LazySunSpot.Sky));
            Assert.AreEqual(5, l.Taps(LazySunSpot.Moon));
            Assert.AreEqual(5, l.Taps(LazySunSpot.Rooster));
            Assert.AreEqual(0, l.Taps(LazySunSpot.Sun), "sole ancora coperto: il tocco va alla collina");
            Assert.AreEqual(10, l.Taps(LazySunSpot.Hill));
            Assert.AreEqual(30, l.WrongMoves);
        }

        [Test]
        public void RapidTapsJustCount()
        {
            var l = new LazySunLogic();
            for (int i = 0; i < 200; i++)
            {
                l.Tap((LazySunSpot)(i % 6));
                l.Tick(0.005f);
            }
            AssertStillPlaying(l);
            Assert.AreEqual(0f, l.Lowered);
            Assert.AreEqual(0, l.BigSnores, "chi tocca non lascia il tempo di russare forte");
        }

        [Test]
        public void WaitingOnlySnoresLouderNeverWins()
        {
            var l = new LazySunLogic();
            Assert.IsFalse(l.Tick(LazySunLogic.BigSnoreSeconds - 0.1f));
            Assert.IsTrue(l.Tick(0.2f), "dopo ~10 s il russare forte");
            Assert.AreEqual(1, l.BigSnores);
            Assert.IsFalse(l.Tick(1f), "poi si ricomincia a contare");
            Run(l, 65f);
            Assert.AreEqual(7, l.BigSnores);
            AssertStillPlaying(l);
            Assert.AreEqual(0f, l.Lowered);
        }

        [Test]
        public void AnyGestureRestartsTheSnoreClock()
        {
            var l = new LazySunLogic();
            l.Tick(9f);
            l.Tap(LazySunSpot.Moon);
            Assert.IsFalse(l.Tick(9f));
            l.GrabSun();
            Assert.IsFalse(l.Tick(9f));
            Assert.IsTrue(l.GrabHill());
            Assert.IsFalse(l.Tick(30f), "mentre si tiene la collina il sole non fa il russare forte");
            Assert.AreEqual(LazySunRelease.Stays, l.ReleaseHill());
            Assert.IsFalse(l.Tick(9f));
            Assert.IsTrue(l.Tick(1.1f));
        }

        [Test]
        public void DraggingTheSunNeverWins()
        {
            var l = new LazySunLogic();
            for (int i = 0; i < 10; i++) Assert.IsTrue(l.GrabSun());
            Assert.AreEqual(10, l.SunGrabs);
            Assert.AreEqual(0f, l.Lowered, "il sole non sposta la collina");
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(LazySunRelease.None, l.ReleaseHill());
            AssertStillPlaying(l);
            // anche con il sole già in vista
            Assert.AreEqual(LazySunRelease.Stays, Lower(l, 0.5f));
            Assert.IsTrue(l.SunPeeking);
            Assert.IsTrue(l.GrabSun());
            Assert.IsTrue(l.Tap(LazySunSpot.Sun));
            Assert.AreEqual(1, l.Taps(LazySunSpot.Sun));
            AssertStillPlaying(l);
            Assert.AreEqual(0.5f, l.Lowered, 1e-6f);
        }

        [Test]
        public void HillIsClampedBothWays()
        {
            var l = new LazySunLogic();
            Assert.IsTrue(l.GrabHill());
            Assert.AreEqual(0f, l.DragHill(-0.6f), "verso l'alto oltre l'inizio: fermo all'inizio");
            Assert.AreEqual(1f, l.DragHill(3f), "giù oltre la profondità massima: fermo in fondo");
            Assert.AreEqual(0.4f, l.DragHill(0.4f), 1e-6f);
            Assert.AreEqual(0f, l.DragHill(float.NegativeInfinity));
            Assert.AreEqual(LazySunRelease.Stays, l.ReleaseHill());
            AssertStillPlaying(l);
        }

        [Test]
        public void LoweredFromDragFollowsTheFingerAndClamps()
        {
            Assert.AreEqual(0f, LazySunLogic.LoweredFromDrag(0f, 0f, 0f, Travel));
            Assert.AreEqual(0.5f, LazySunLogic.LoweredFromDrag(0f, 0.2f, 0.2f - Travel * 0.5f, Travel), 1e-5f);
            Assert.AreEqual(0f, LazySunLogic.LoweredFromDrag(0f, 0.2f, 2.5f, Travel), "dito verso l'alto: limitato");
            Assert.AreEqual(1f, LazySunLogic.LoweredFromDrag(0.3f, 0.2f, -9f, Travel), "dito molto in basso: limitato");
            Assert.AreEqual(0.3f + 1f / Travel, LazySunLogic.LoweredFromDrag(0.3f, 1f, 0f, Travel), 1e-5f, "riprende da dove era rimasta");
            Assert.AreEqual(0.2f, LazySunLogic.LoweredFromDrag(0.6f, 0f, 0.4f * Travel, Travel), 1e-5f, "si può anche rialzare");
            Assert.AreEqual(0.7f, LazySunLogic.LoweredFromDrag(0.7f, 0f, -5f, 0f), "corsa nulla: niente divisioni per zero");
        }

        [Test]
        public void PartialLowerReleasedStaysWhereItIs()
        {
            var l = new LazySunLogic();
            Assert.AreEqual(LazySunRelease.Stays, Lower(l, 0.2f));
            Assert.AreEqual(0.2f, l.Lowered, 1e-6f);
            Assert.IsFalse(l.SunPeeking);
            Assert.AreEqual(LazySunRelease.Stays, Lower(l, 0.6f));
            Assert.AreEqual(0.6f, l.Lowered, 1e-6f);
            Assert.IsTrue(l.SunPeeking, "sole in parte scoperto: continua a dormire");
            Run(l, 25f);
            Assert.AreEqual(0.6f, l.Lowered, 1e-6f, "nessun ritorno automatico");
            Assert.AreEqual(LazySunRelease.Stays, Lower(l, LazySunLogic.Threshold - 0.01f));
            Assert.AreEqual(3, l.ShortReleases);
            AssertStillPlaying(l);
        }

        [Test]
        public void PartialStateIsKeptOnTheNextGrab()
        {
            var l = new LazySunLogic();
            Assert.AreEqual(LazySunRelease.Stays, Lower(l, 0.5f));
            Assert.IsTrue(l.GrabHill());
            Assert.AreEqual(0.5f, l.Lowered, 1e-6f, "riprendendola non salta");
            l.DragHill(LazySunLogic.LoweredFromDrag(0.5f, 0f, -0.3f * Travel, Travel));
            Assert.AreEqual(0.8f, l.Lowered, 1e-5f);
            Assert.AreEqual(LazySunRelease.Rises, l.ReleaseHill());
        }

        [Test]
        public void LoweringPastThresholdWinsOnce()
        {
            var l = new LazySunLogic();
            Assert.AreEqual(LazySunRelease.Rises, Lower(l, LazySunLogic.Threshold));
            Assert.AreEqual(LazySunStage.Celebrating, l.Stage);
            Assert.AreEqual(1f, l.Lowered, "scivola fino in fondo");
            Assert.IsTrue(l.InputLocked);
            // durante la festa niente input
            Assert.IsFalse(l.Tap(LazySunSpot.Moon));
            Assert.IsFalse(l.GrabSun());
            Assert.IsFalse(l.GrabHill());
            Assert.AreEqual(LazySunRelease.None, l.ReleaseHill());
            Assert.IsFalse(l.Tick(30f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "la vittoria una volta sola");
            Assert.AreEqual(LazySunStage.Done, l.Stage);
        }

        [Test]
        public void PassingTheThresholdDuringTheDragIsNotEnough()
        {
            var l = new LazySunLogic();
            Assert.IsTrue(l.GrabHill());
            l.DragHill(1f);
            Assert.IsTrue(l.PastThreshold);
            Assert.AreEqual(LazySunStage.Playing, l.Stage, "vince solo al rilascio");
            l.DragHill(0.5f);
            Assert.AreEqual(LazySunRelease.Stays, l.ReleaseHill(), "rialzata prima di lasciarla: niente");
            AssertStillPlaying(l);
        }

        [Test]
        public void ThreeWrongAttemptsThenSolution()
        {
            var l = new LazySunLogic();
            l.Tap(LazySunSpot.Sky);
            l.Tap(LazySunSpot.Moon);
            l.Tap(LazySunSpot.Rooster);
            l.GrabSun();
            Lower(l, 0.4f);
            Run(l, 12f);
            AssertStillPlaying(l);
            Assert.AreEqual(5, l.WrongMoves);
            Assert.AreEqual(LazySunRelease.Rises, Lower(l, 0.9f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void CancelKeepsThePositionAndNeverWins()
        {
            var l = new LazySunLogic();
            Assert.IsFalse(l.Cancel(), "niente da annullare");
            Assert.IsTrue(l.GrabHill());
            l.DragHill(0.95f);
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(0.95f, l.Lowered, 1e-6f, "la collina resta dov'era");
            AssertStillPlaying(l);
            Assert.AreEqual(LazySunRelease.None, l.ReleaseHill(), "un rilascio dopo l'annullamento non conta");
            Assert.AreEqual(0.95f, l.DragHill(0.1f), 1e-6f, "senza presa non si muove");
            // riprendendola e lasciandola vince
            Assert.AreEqual(LazySunRelease.Rises, Lower(l, 0.95f));
        }

        [Test]
        public void DoubleGrabAndTapsWhileHoldingAreIgnored()
        {
            var l = new LazySunLogic();
            Assert.IsTrue(l.GrabHill());
            Assert.IsFalse(l.GrabHill());
            Assert.IsFalse(l.GrabSun());
            Assert.IsFalse(l.Tap(LazySunSpot.Moon));
            Assert.AreEqual(1, l.HillGrabs);
            Assert.AreEqual(LazySunRelease.Stays, l.ReleaseHill());
            Assert.AreEqual(LazySunRelease.None, l.ReleaseHill(), "doppio rilascio ignorato");
        }

        [Test]
        public void RapidGrabReleaseNeverWinsBelowThreshold()
        {
            var l = new LazySunLogic();
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(l.GrabHill());
                l.DragHill(l.Lowered + 0.005f);
                Assert.AreEqual(LazySunRelease.Stays, l.ReleaseHill());
            }
            Assert.AreEqual(0.5f, l.Lowered, 1e-4f);
            AssertStillPlaying(l);
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = new LazySunLogic();
            l.Tap(LazySunSpot.Moon);
            l.GrabSun();
            Lower(l, 0.6f);
            l.Tick(10.5f);
            Assert.AreEqual(LazySunRelease.Rises, Lower(l, 0.9f));
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(LazySunStage.Playing, l.Stage);
            Assert.AreEqual(0f, l.Lowered);
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(0f, l.Idle);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.BigSnores);
            Assert.AreEqual(0, l.HillGrabs);
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(LazySunRelease.Rises, Lower(l, 1f), "si può rigiocare");
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetWhileHoldingLeavesNoGrip()
        {
            var l = new LazySunLogic();
            Assert.IsTrue(l.GrabHill());
            l.DragHill(0.9f);
            l.Reset();
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(LazySunRelease.None, l.ReleaseHill());
            Assert.AreEqual(0f, l.Lowered);
        }

        [Test]
        public void HillEdgeAndGrabZone()
        {
            const float a = 4.9f, b = 4.6f;
            Assert.AreEqual(0f, LazySunLogic.EdgeDrop(0f, a, b), 1e-6f);
            Assert.Greater(LazySunLogic.EdgeDrop(1f, a, b), 0f);
            Assert.Greater(LazySunLogic.EdgeDrop(2f, a, b), LazySunLogic.EdgeDrop(1f, a, b));
            Assert.AreEqual(LazySunLogic.EdgeDrop(1.5f, a, b), LazySunLogic.EdgeDrop(-1.5f, a, b), 1e-6f);
            Assert.AreEqual(b, LazySunLogic.EdgeDrop(9f, a, b));
            Assert.AreEqual(b, LazySunLogic.EdgeDrop(1f, 0f, b));
            float edge = TopY;
            Assert.IsTrue(LazySunLogic.InHillGrab(edge, edge, 0.45f, 2f));
            Assert.IsTrue(LazySunLogic.InHillGrab(edge + 0.4f, edge, 0.45f, 2f), "poco sopra il bordo: presa generosa");
            Assert.IsTrue(LazySunLogic.InHillGrab(edge - 1.9f, edge, 0.45f, 2f));
            Assert.IsFalse(LazySunLogic.InHillGrab(edge + 0.6f, edge, 0.45f, 2f), "in pieno cielo no");
            Assert.IsFalse(LazySunLogic.InHillGrab(edge - 2.1f, edge, 0.45f, 2f));
        }

        [Test]
        public void SunIsHiddenAtStartAndTouchableOnlyWhereVisible()
        {
            // misure del livello: cima della collina in TopY, sole (raggio con i raggi 1.05) centrato in (-0.45, -1.25)
            const float a = 4.9f, b = 4.6f, sx = -0.45f, sy = -1.25f, r = 1.05f;
            for (int i = 0; i <= 36; i++)
            {
                double ang = i * System.Math.PI * 2 / 36;
                float px = sx + r * (float)System.Math.Cos(ang), py = sy + r * (float)System.Math.Sin(ang);
                float edge = TopY - LazySunLogic.EdgeDrop(px, a, b);
                Assert.Less(py, edge - 0.2f, "all'inizio il sole è tutto dietro la collina");
                Assert.IsFalse(LazySunLogic.OnVisibleSun(px, py, sx, sy, r + 0.01f, edge, 0.25f));
            }
            // abbassata del tutto: il centro del sole si vede ed è toccabile
            float low = TopY - Travel - LazySunLogic.EdgeDrop(sx, a, b);
            Assert.IsTrue(LazySunLogic.OnVisibleSun(sx, sy, sx, sy, r, low, 0.25f));
            Assert.IsFalse(LazySunLogic.OnVisibleSun(sx + 2f, sy, sx, sy, r, low, 0.25f));
            // alla soglia il corpo del sole (raggio 0.72) è già quasi tutto fuori
            float atThreshold = TopY - Travel * LazySunLogic.Threshold;
            Assert.Less(atThreshold, sy - 0.6f);
        }
    }
}
