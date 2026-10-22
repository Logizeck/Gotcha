using System;
using System.Collections.Generic;
using NUnit.Framework;
using Gotcha.Levels.Halloween;

namespace Gotcha.Tests
{
    /// <summary>Livello 246: riconoscimento della faccia intagliata e condizioni di vittoria della zucca.</summary>
    public class HalloweenPumpkinTests
    {
        static List<HwPoint> Circle(float cx, float cy, float r, int n = 12)
        {
            var pts = new List<HwPoint>();
            for (int i = 0; i <= n; i++)
            {
                double a = i / (double)n * Math.PI * 2;
                pts.Add(new HwPoint(cx + (float)Math.Cos(a) * r, cy + (float)Math.Sin(a) * r));
            }
            return pts;
        }

        static List<HwPoint> Segment(float x0, float y0, float x1, float y1, int n = 8)
        {
            var pts = new List<HwPoint>();
            for (int i = 0; i <= n; i++) pts.Add(new HwPoint(x0 + (x1 - x0) * i / n, y0 + (y1 - y0) * i / n));
            return pts;
        }

        static List<HwPoint> Smile(float cy = -0.45f, float half = 0.4f)
        {
            var pts = new List<HwPoint>();
            for (int i = 0; i <= 10; i++)
            {
                float x = -half + 2f * half * i / 10f;
                pts.Add(new HwPoint(x, cy - 0.15f * (1f - x * x / (half * half))));
            }
            return pts;
        }

        static void CarveFace(PumpkinLogic l)
        {
            Assert.IsTrue(l.TryCarve(Circle(-0.4f, 0.35f, 0.12f), out _));
            Assert.IsTrue(l.TryCarve(Circle(0.4f, 0.35f, 0.12f), out _));
            Assert.IsTrue(l.TryCarve(Segment(-0.08f, 0.0f, 0.08f, 0.0f), out _));
            Assert.IsTrue(l.TryCarve(Smile(), out _));
        }

        static void Prepare(PumpkinLogic l)
        {
            CarveFace(l);
            Assert.IsTrue(l.TryUseLighter(PumpkinTarget.Candle));
            Assert.IsTrue(l.TrySetCandleInside(true));
        }

        [Test]
        public void Validator_RecognisesFullFace()
        {
            var l = new PumpkinLogic();
            CarveFace(l);
            FaceReport r = l.LastReport;
            Assert.IsTrue(r.LeftEye && r.RightEye && r.Nose && r.Mouth);
            Assert.AreEqual(FacePart.LeftEye, r.StrokeParts[0]);
            Assert.AreEqual(FacePart.RightEye, r.StrokeParts[1]);
            Assert.AreEqual(FacePart.Nose, r.StrokeParts[2]);
            Assert.AreEqual(FacePart.Mouth, r.StrokeParts[3]);
        }

        [Test]
        public void Validator_EyeMadeOfSeveralStrokes_CountsOnce()
        {
            var v = new PumpkinFaceValidator();
            v.AddStroke(Segment(-0.55f, 0.3f, -0.3f, 0.3f), out _);
            v.AddStroke(Segment(-0.42f, 0.2f, -0.42f, 0.42f), out _);   // croce: stesso occhio
            FaceReport r = v.Evaluate();
            Assert.IsTrue(r.LeftEye);
            Assert.IsFalse(r.RightEye);
            Assert.AreEqual(1, r.PartsFound);
        }

        [Test]
        public void Validator_WrongFaces_AreNotComplete()
        {
            // due occhi dalla stessa parte
            var v = new PumpkinFaceValidator();
            v.AddStroke(Circle(-0.5f, 0.35f, 0.1f), out _);
            v.AddStroke(Circle(-0.2f, 0.6f, 0.1f), out _);
            v.AddStroke(Segment(-0.05f, 0f, 0.05f, 0f), out _);
            v.AddStroke(Smile(), out _);
            Assert.IsFalse(v.Evaluate().IsComplete);

            // bocca troppo stretta
            v.Reset();
            v.AddStroke(Circle(-0.4f, 0.35f, 0.1f), out _);
            v.AddStroke(Circle(0.4f, 0.35f, 0.1f), out _);
            v.AddStroke(Segment(-0.05f, 0f, 0.05f, 0f), out _);
            v.AddStroke(Segment(-0.1f, -0.5f, 0.1f, -0.5f), out _);
            FaceReport r = v.Evaluate();
            Assert.IsFalse(r.Mouth);
            Assert.IsFalse(r.IsComplete);

            // un unico scarabocchio che copre tutta la faccia non vale niente
            v.Reset();
            v.AddStroke(new List<HwPoint> { new HwPoint(-0.8f, 0.6f), new HwPoint(0.8f, -0.6f), new HwPoint(-0.8f, -0.6f), new HwPoint(0.8f, 0.6f) }, out _);
            Assert.IsFalse(v.Evaluate().IsComplete);

            // niente naso
            v.Reset();
            v.AddStroke(Circle(-0.4f, 0.35f, 0.1f), out _);
            v.AddStroke(Circle(0.4f, 0.35f, 0.1f), out _);
            v.AddStroke(Smile(), out _);
            Assert.IsFalse(v.Evaluate().IsComplete);
        }

        [Test]
        public void Validator_IgnoresTinyStrokes_AndEvictsOldest()
        {
            var v = new PumpkinFaceValidator(new PumpkinFaceConfig { MaxStrokes = 3 });
            Assert.IsFalse(v.AddStroke(new List<HwPoint> { new HwPoint(0, 0), new HwPoint(0.01f, 0) }, out _));
            Assert.IsFalse(v.AddStroke(null, out _));
            for (int i = 0; i < 3; i++) { Assert.IsTrue(v.AddStroke(Segment(0, 0, 0.3f, 0), out bool ev)); Assert.IsFalse(ev); }
            Assert.IsTrue(v.AddStroke(Segment(0, 0, 0.3f, 0), out bool evicted));
            Assert.IsTrue(evicted);
            Assert.AreEqual(3, v.StrokeCount);
        }

        [Test]
        public void CorrectPath_SwitchOffIsTheWinningMove()
        {
            var l = new PumpkinLogic();
            Prepare(l);
            Assert.IsTrue(l.ReadyForDark);
            Assert.AreEqual(PumpkinSwitchResult.Win, l.ToggleLight());
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.AreEqual(PumpkinPhase.Done, l.Phase);
        }

        [Test]
        public void LightOffTooEarly_ThenBackOn_ThenOffAgainWins()
        {
            var l = new PumpkinLogic();
            Assert.AreEqual(PumpkinSwitchResult.TurnedOff, l.ToggleLight());
            Prepare(l);   // si può preparare anche al buio
            Assert.AreEqual(PumpkinPhase.Preparing, l.Phase);
            Assert.AreEqual(PumpkinSwitchResult.TurnedOn, l.ToggleLight());
            Assert.AreEqual(PumpkinSwitchResult.Win, l.ToggleLight());
        }

        [Test]
        public void MissingAnyCondition_NoWin()
        {
            for (int missing = 0; missing < 3; missing++)
            {
                var l = new PumpkinLogic();
                if (missing != 0) CarveFace(l);
                if (missing != 1) l.TryUseLighter(PumpkinTarget.Candle);
                if (missing != 2) l.TrySetCandleInside(true);
                Assert.AreEqual(PumpkinSwitchResult.TurnedOff, l.ToggleLight(), "missing " + missing);
                Assert.IsFalse(l.TryCompleteEnding());
            }
        }

        [Test]
        public void Lighter_WorksOnlyOnCandle_Once()
        {
            var l = new PumpkinLogic();
            foreach (PumpkinTarget t in new[] { PumpkinTarget.Nothing, PumpkinTarget.Pumpkin, PumpkinTarget.Knife, PumpkinTarget.Lamp, PumpkinTarget.Switch })
                Assert.IsFalse(l.TryUseLighter(t));
            Assert.IsFalse(l.CandleLit);
            Assert.IsTrue(l.TryUseLighter(PumpkinTarget.Candle));
            Assert.IsFalse(l.TryUseLighter(PumpkinTarget.Candle));
        }

        [Test]
        public void CandleInsideOut_NoDuplicateChanges()
        {
            var l = new PumpkinLogic();
            Assert.IsFalse(l.TrySetCandleInside(false));
            Assert.IsTrue(l.TrySetCandleInside(true));
            Assert.IsFalse(l.TrySetCandleInside(true));
            Assert.IsTrue(l.TrySetCandleInside(false));
        }

        [Test]
        public void ClearCarving_LosesFace()
        {
            var l = new PumpkinLogic();
            CarveFace(l);
            Assert.IsTrue(l.FaceComplete);
            Assert.IsTrue(l.TryClearCarving());
            Assert.IsFalse(l.FaceComplete);
            Assert.AreEqual(0, l.Face.StrokeCount);
        }

        [Test]
        public void Completion_ReportedOnce_AndLocksEverything()
        {
            var l = new PumpkinLogic();
            Prepare(l);
            l.ToggleLight();
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.IsFalse(l.TryCompleteEnding());
            Assert.AreEqual(PumpkinSwitchResult.Ignored, l.ToggleLight());
            Assert.IsFalse(l.TrySetCandleInside(false));
            Assert.IsFalse(l.TryClearCarving());
            Assert.IsFalse(l.TryCarve(Smile(), out _));
        }

        [Test]
        public void Reset_FromEveryPhase()
        {
            foreach (PumpkinPhase p in Enum.GetValues(typeof(PumpkinPhase)))
            {
                var l = new PumpkinLogic();
                if (p != PumpkinPhase.Preparing) { Prepare(l); l.ToggleLight(); }
                if (p == PumpkinPhase.Done) l.TryCompleteEnding();
                Assert.AreEqual(p, l.Phase);
                l.Reset();
                Assert.AreEqual(PumpkinPhase.Preparing, l.Phase);
                Assert.IsTrue(l.LightOn);
                Assert.IsFalse(l.CandleLit || l.CandleInside || l.FaceComplete || l.CompletionReported);
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(11, 12, 13, 14, 15, 16)] int seed)
        {
            var rnd = new Random(seed);
            var l = new PumpkinLogic();
            int wins = 0, completions = 0;
            for (int step = 0; step < 600; step++)
            {
                bool wasReady = l.ReadyForDark, wasOn = l.LightOn, locked = l.InputLocked;
                switch (rnd.Next(8))
                {
                    case 0:
                        var pts = new List<HwPoint>();
                        float cx = (float)rnd.NextDouble() * 2f - 1f, cy = (float)rnd.NextDouble() * 2f - 1f;
                        for (int i = 0; i < 5; i++) pts.Add(new HwPoint(cx + (float)rnd.NextDouble() * 0.3f, cy + (float)rnd.NextDouble() * 0.3f));
                        l.TryCarve(pts, out _);
                        break;
                    case 1: l.TryUseLighter((PumpkinTarget)rnd.Next(6)); break;
                    case 2: l.TrySetCandleInside(rnd.Next(2) == 0); break;
                    case 3:
                        PumpkinSwitchResult r = l.ToggleLight();
                        if (r == PumpkinSwitchResult.Win)
                        {
                            wins++;
                            Assert.IsTrue(wasReady && wasOn && !locked, "win only by switching off when ready");
                        }
                        break;
                    case 4: if (l.TryCompleteEnding()) completions++; break;
                    case 5: if (rnd.Next(10) == 0) l.TryClearCarving(); break;
                    case 6: if (rnd.Next(80) == 0) { l.Reset(); wins = completions = 0; } break;
                    default: CarveFace(l.InputLocked ? new PumpkinLogic() : l); break;
                }
                Assert.LessOrEqual(wins, 1);
                Assert.LessOrEqual(completions, 1);
                Assert.LessOrEqual(completions, wins);
                if (l.Phase != PumpkinPhase.Preparing) Assert.IsFalse(l.LightOn);
                Assert.AreEqual(l.Phase == PumpkinPhase.Done, l.CompletionReported);
            }
        }
    }
}
