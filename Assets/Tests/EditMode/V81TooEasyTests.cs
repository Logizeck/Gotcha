using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 34 della roadmap: Too Easy! (il pulsante WIN mente; si vince toccando la parola LEVEL).</summary>
    public class V81TooEasyTests
    {
        // gli stessi limiti del livello (centro del pulsante) e la frase "COMPLETE THIS LEVEL ."
        const float MinX = -0.9f, MaxX = 0.9f, MinY = -2.9f, MaxY = 1.4f, StartX = 0f, StartY = -0.9f;
        const int Complete = 0, This = 1, Level = 2, Dot = 3;

        static TooEasyLogic Make() => new TooEasyLogic(MinX, MaxX, MinY, MaxY, StartX, StartY, 4, Level);

        static void Land(TooEasyLogic l)
        {
            for (int i = 0; i < 20 && l.Jumping; i++) l.Tick(0.05f);
            Assert.IsFalse(l.Jumping);
        }

        static void AssertStillPlaying(TooEasyLogic l)
        {
            Assert.AreEqual(TooEasyStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static void AssertInBounds(TooEasyLogic l)
        {
            Assert.GreaterOrEqual(l.X, MinX);
            Assert.LessOrEqual(l.X, MaxX);
            Assert.GreaterOrEqual(l.Y, MinY);
            Assert.LessOrEqual(l.Y, MaxY);
        }

        [Test]
        public void StartsInTheMiddleAndPlaying()
        {
            var l = Make();
            AssertStillPlaying(l);
            Assert.AreEqual(StartX, l.X, 1e-5f);
            Assert.AreEqual(StartY, l.Y, 1e-5f);
            Assert.AreEqual(0, l.Presses);
            Assert.AreEqual(0, l.LabelStage);
            Assert.IsFalse(l.Tongue);
            Assert.IsFalse(l.Jumping);
            Assert.AreEqual(1f, l.JumpProgress, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void WinButtonNeverWinsIn100Presses()
        {
            var l = Make();
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(TooEasyPress.Jumped, l.PressWin(), "press " + i);
                AssertStillPlaying(l);
                AssertInBounds(l);
                Land(l);
            }
            Assert.AreEqual(100, l.Presses);
            Assert.AreEqual(100, l.WrongMoves);
            AssertStillPlaying(l);
        }

        [Test]
        public void JumpPositionsAlwaysInBoundsAndFarApart()
        {
            var l = Make();
            float px = l.X, py = l.Y;
            for (int i = 0; i < 3 * TooEasyLogic.SpotCount + 1; i++)
            {
                l.PressWin();
                AssertInBounds(l);
                Assert.AreEqual(px, l.FromX, 1e-5f);
                Assert.AreEqual(py, l.FromY, 1e-5f);
                float d = (float)System.Math.Sqrt((l.X - px) * (l.X - px) + (l.Y - py) * (l.Y - py));
                Assert.GreaterOrEqual(d, TooEasyLogic.MinJump, "jump " + i + " too short");
                px = l.X;
                py = l.Y;
                Land(l);
            }
        }

        [Test]
        public void JumpSequenceIsDeterministicAndRestartsOnReset()
        {
            var a = Make();
            var b = Make();
            var xs = new float[25];
            var ys = new float[25];
            for (int i = 0; i < 25; i++)
            {
                a.PressWin(); b.PressWin();
                Assert.AreEqual(a.X, b.X, 1e-6f);
                Assert.AreEqual(a.Y, b.Y, 1e-6f);
                xs[i] = a.X; ys[i] = a.Y;
                Land(a); Land(b);
            }
            a.Reset();
            Assert.AreEqual(StartX, a.X, 1e-6f);
            Assert.AreEqual(StartY, a.Y, 1e-6f);
            for (int i = 0; i < 25; i++)
            {
                a.PressWin();
                Assert.AreEqual(xs[i], a.X, 1e-6f);
                Assert.AreEqual(ys[i], a.Y, 1e-6f);
                Land(a);
            }
        }

        [Test]
        public void SpotsStayInsideEvenWithOddBounds()
        {
            // limiti rovesciati, degeneri o NaN: i posti restano dentro, niente NaN
            var l = new TooEasyLogic(1f, -1f, 2f, -2f, 99f, -99f);
            Assert.AreEqual(1f, l.X, 1e-6f);
            Assert.AreEqual(-2f, l.Y, 1e-6f);
            for (int i = 0; i < 30; i++)
            {
                l.PressWin();
                Assert.That(l.X >= -1f && l.X <= 1f && l.Y >= -2f && l.Y <= 2f);
                l.Cancel();
            }
            var z = new TooEasyLogic(0.5f, 0.5f, float.NaN, 1f, 0f, 0f);
            for (int i = 0; i < 12; i++)
            {
                z.PressWin();
                Assert.IsFalse(float.IsNaN(z.X) || float.IsNaN(z.Y));
                Assert.AreEqual(0.5f, z.X, 1e-6f);
                z.Cancel();
            }
            for (int n = -25; n < 25; n++)
            {
                l.SpotAt(n, out float x, out float y);
                Assert.That(x >= -1f && x <= 1f && y >= -2f && y <= 2f);
            }
        }

        [Test]
        public void MockeryEscalates()
        {
            var l = Make();
            for (int i = 1; i <= TooEasyLogic.NudgeAfter + 2; i++)
            {
                l.PressWin();
                Assert.AreEqual(i >= TooEasyLogic.TongueAfter, l.Tongue, "tongue at " + i);
                int label = i >= TooEasyLogic.ShoutAfter ? 2 : i >= TooEasyLogic.UnsureAfter ? 1 : 0;
                Assert.AreEqual(label, l.LabelStage, "label at " + i);
                Assert.AreEqual(i == TooEasyLogic.NudgeAfter, l.TakeNudge(), "nudge at " + i);
                Land(l);
            }
            Assert.IsFalse(l.TakeNudge(), "the nudge is said once");
            l.Reset();
            Assert.AreEqual(0, l.LabelStage);
            Assert.IsFalse(l.Tongue);
            for (int i = 0; i < TooEasyLogic.NudgeAfter; i++) { l.PressWin(); Land(l); }
            Assert.IsTrue(l.TakeNudge(), "reset gives the nudge back");
        }

        [Test]
        public void RapidPressesDuringTheJumpAreIgnored()
        {
            var l = Make();
            Assert.AreEqual(TooEasyPress.Jumped, l.PressWin());
            float x = l.X, y = l.Y;
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(TooEasyPress.Ignored, l.PressWin());
                l.Tick(TooEasyLogic.JumpTime / 200f);
            }
            Assert.AreEqual(1, l.Presses);
            Assert.AreEqual(x, l.X, 1e-6f);
            Assert.AreEqual(y, l.Y, 1e-6f);
            Assert.IsTrue(l.Jumping);
            Assert.Greater(l.JumpProgress, 0f);
            Assert.Less(l.JumpProgress, 1f);
            bool landed = false;
            for (int i = 0; i < 20 && !landed; i++) landed = l.Tick(0.05f);
            Assert.IsTrue(landed);
            Assert.AreEqual(TooEasyPress.Jumped, l.PressWin());
            Assert.AreEqual(2, l.Presses);
            // passi di tempo strani: niente salti fantasma
            l.Tick(-1f); l.Tick(float.NaN); l.Tick(0f);
            Assert.IsTrue(l.Jumping);
            AssertStillPlaying(l);
        }

        [Test]
        public void OtherWordsNeverWin()
        {
            var l = Make();
            for (int i = 0; i < 40; i++)
            {
                Assert.AreEqual(TooEasyTap.Wrong, l.TapWord(Complete));
                Assert.AreEqual(TooEasyTap.Wrong, l.TapWord(This));
                Assert.AreEqual(TooEasyTap.Wrong, l.TapWord(Dot));
                Assert.AreEqual(TooEasyTap.Ignored, l.TapWord(-1));
                Assert.AreEqual(TooEasyTap.Ignored, l.TapWord(4));
                AssertStillPlaying(l);
            }
            Assert.AreEqual(40, l.WordTaps(Complete));
            Assert.AreEqual(40, l.WordTaps(This));
            Assert.AreEqual(40, l.WordTaps(Dot));
            Assert.AreEqual(0, l.WordTaps(Level));
            Assert.AreEqual(120, l.WrongWordTaps);
        }

        [Test]
        public void LevelTapWinsExactlyOnce()
        {
            var l = Make();
            Assert.AreEqual(TooEasyTap.Solved, l.TapWord(Level));
            Assert.AreEqual(TooEasyStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            // tutto il resto è chiuso dopo la vittoria
            Assert.AreEqual(TooEasyTap.Ignored, l.TapWord(Level));
            Assert.AreEqual(TooEasyTap.Ignored, l.TapWord(Complete));
            Assert.AreEqual(TooEasyPress.Ignored, l.PressWin());
            Assert.IsFalse(l.TakeNudge());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(1, l.WordTaps(Level));
        }

        [Test]
        public void LevelTapWinsAfterManyWrongAttemptsAndMidJump()
        {
            var l = Make();
            l.PressWin(); Land(l);
            l.TapWord(Complete);
            l.PressWin(); Land(l);
            l.TapWord(This);
            l.TapWord(Dot);
            l.PressWin();   // il pulsante sta ancora saltando
            Assert.IsTrue(l.Jumping);
            Assert.AreEqual(TooEasyTap.Solved, l.TapWord(Level));
            Assert.IsFalse(l.Jumping, "the win stops the jump");
            Assert.AreEqual(3, l.Presses);
            Assert.AreEqual(6, l.WrongMoves);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void TheTargetWordComesFromTheData()
        {
            // una traduzione con un'altra frase: la parola giusta è solo un indice
            var l = new TooEasyLogic(MinX, MaxX, MinY, MaxY, 0f, 0f, 5, 3);
            Assert.AreEqual(3, l.Target);
            Assert.AreEqual(TooEasyTap.Wrong, l.TapWord(2));
            Assert.AreEqual(TooEasyTap.Wrong, l.TapWord(4));
            Assert.AreEqual(TooEasyTap.Solved, l.TapWord(3));
            var clamped = new TooEasyLogic(MinX, MaxX, MinY, MaxY, 0f, 0f, 2, 9);
            Assert.AreEqual(1, clamped.Target);
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = Make();
            for (int i = 0; i < 7; i++) { l.PressWin(); Land(l); }
            l.TapWord(Complete);
            l.PressWin();
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.Presses);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.IsFalse(l.Jumping);
            Assert.AreEqual(StartX, l.X, 1e-6f);
            Assert.AreEqual(StartY, l.Y, 1e-6f);
            // anche dopo una vittoria: si rigioca da capo e si rivince una volta sola
            l.TapWord(Level);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(TooEasyTap.Solved, l.TapWord(Level));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void CancelLeavesAValidState()
        {
            var l = Make();
            l.PressWin();
            Assert.IsTrue(l.Jumping);
            float x = l.X, y = l.Y;
            l.Cancel();
            Assert.IsFalse(l.Jumping);
            Assert.AreEqual(x, l.X, 1e-6f);
            Assert.AreEqual(y, l.Y, 1e-6f);
            AssertStillPlaying(l);
            Assert.AreEqual(TooEasyPress.Jumped, l.PressWin(), "after cancel the button works again");
            l.Cancel(); l.Cancel();
            Assert.AreEqual(TooEasyTap.Solved, l.TapWord(Level));
            l.Cancel();
            Assert.AreEqual(TooEasyStage.Celebrating, l.Stage, "cancel never undoes the win");
            Assert.IsTrue(l.TryComplete());
        }

        // ---------- Impaginazione dalle larghezze ----------

        static void AssertRow(float[] widths, float gap)
        {
            var gaps = new float[widths.Length];
            for (int i = 1; i < gaps.Length; i++) gaps[i] = gap;
            float[] c = TooEasyLogic.Row(widths, gaps, out float total);
            float sum = 0f;
            foreach (float w in widths) sum += w;
            Assert.AreEqual(sum + gap * (widths.Length - 1), total, 1e-4f);
            // centrata e senza sovrapposizioni, spazio esatto fra le parole
            Assert.AreEqual(-total * 0.5f, c[0] - widths[0] * 0.5f, 1e-4f);
            Assert.AreEqual(total * 0.5f, c[c.Length - 1] + widths[widths.Length - 1] * 0.5f, 1e-4f);
            for (int i = 1; i < c.Length; i++)
                Assert.AreEqual(gap, (c[i] - widths[i] * 0.5f) - (c[i - 1] + widths[i - 1] * 0.5f), 1e-4f);
        }

        [Test]
        public void RowLayoutFollowsMeasuredWidths()
        {
            AssertRow(new[] { 1.9f, 0.85f, 1.15f, 0.12f }, 0.16f);        // COMPLETE THIS LEVEL .
            AssertRow(new[] { 0.4f, 0.3f, 0.35f, 0.1f }, 0.16f);          // parole cortissime
            AssertRow(new[] { 3.6f, 1.8f, 2.9f, 0.12f }, 0.16f);          // traduzione lunghissima
            AssertRow(new[] { 2.2f }, 0.16f);                             // una parola sola
            float[] none = TooEasyLogic.Row(new float[0], null, out float t0);
            Assert.AreEqual(0, none.Length);
            Assert.AreEqual(0f, t0);
            float[] bad = TooEasyLogic.Row(new[] { float.NaN, -1f, 1f }, new[] { 0f, float.NaN, 0.2f }, out float tb);
            Assert.AreEqual(1.2f, tb, 1e-5f);
            foreach (float v in bad) Assert.IsFalse(float.IsNaN(v));
        }

        [Test]
        public void LongRowsShrinkToFitAndShortOnesDont()
        {
            Assert.AreEqual(1f, TooEasyLogic.FitScale(4.2f, 5.0f), 1e-6f);
            Assert.AreEqual(1f, TooEasyLogic.FitScale(5.0f, 5.0f), 1e-6f);
            float k = TooEasyLogic.FitScale(8.75f, 5.0f);
            Assert.AreEqual(5.0f / 8.75f, k, 1e-6f);
            Assert.LessOrEqual(8.75f * k, 5.0f + 1e-5f);
            Assert.AreEqual(1f, TooEasyLogic.FitScale(3f, 0f), 1e-6f);
            Assert.AreEqual(1f, TooEasyLogic.FitScale(float.NaN, 5f), 1e-6f);
            Assert.Greater(TooEasyLogic.FitScale(float.PositiveInfinity, 5f), 0f);
        }

        static void AssertSpans(float[] widths, float gap, float pad, float minW)
        {
            var gaps = new float[widths.Length];
            for (int i = 1; i < gaps.Length; i++) gaps[i] = gap;
            float[] c = TooEasyLogic.Row(widths, gaps, out float total);
            TooEasyLogic.HitSpans(c, widths, pad, minW, out float[] l, out float[] r);
            for (int i = 0; i < c.Length; i++)
            {
                Assert.LessOrEqual(l[i], c[i] - widths[i] * 0.5f + 1e-5f, "span " + i + " covers its word (left)");
                Assert.GreaterOrEqual(r[i], c[i] + widths[i] * 0.5f - 1e-5f, "span " + i + " covers its word (right)");
                // generosa: almeno minW, oppure (parola interna cortissima) fino alle parole vicine
                float room = i > 0 && i < c.Length - 1 ? (c[i + 1] - widths[i + 1] * 0.5f) - (c[i - 1] + widths[i - 1] * 0.5f) : float.MaxValue;
                Assert.GreaterOrEqual(r[i] - l[i], System.Math.Min(minW, room) - 1e-4f, "span " + i + " is generous");
                Assert.AreEqual(i, TooEasyLogic.SpanAt(l, r, c[i]), "the centre of word " + i + " hits it");
                if (i > 0) Assert.AreEqual(r[i - 1], l[i], 1e-5f, "no gap and no overlap between " + (i - 1) + " and " + i);
            }
            Assert.AreEqual(-total * 0.5f - pad, l[0], 1e-4f);
            Assert.GreaterOrEqual(r[c.Length - 1], total * 0.5f + pad - 1e-4f);
            Assert.AreEqual(-1, TooEasyLogic.SpanAt(l, r, l[0] - 0.01f));
            Assert.AreEqual(-1, TooEasyLogic.SpanAt(l, r, r[c.Length - 1] + 0.01f));
        }

        [Test]
        public void HitAreasAreGenerousContiguousAndNeverOverlap()
        {
            AssertSpans(new[] { 1.9f, 0.85f, 1.15f, 0.12f }, 0.16f, 0.3f, 0.6f);
            AssertSpans(new[] { 0.4f, 0.3f, 0.35f, 0.1f }, 0.16f, 0.3f, 0.45f);
            AssertSpans(new[] { 3.6f, 1.8f, 2.9f, 0.12f }, 0.3f, 0.3f, 0.6f);
            AssertSpans(new[] { 0.9f, 0.05f, 0.9f }, 0.1f, 0.2f, 0.6f);    // parola cortissima in mezzo: arriva alle vicine
            float[] mw = { 0.9f, 0.05f, 0.9f };
            float[] mc = TooEasyLogic.Row(mw, new[] { 0f, 0.1f, 0.1f }, out _);
            TooEasyLogic.HitSpans(mc, mw, 0.2f, 0.6f, out float[] ml, out float[] mr);
            Assert.AreEqual(mc[0] + 0.45f, ml[1], 1e-5f);
            Assert.AreEqual(mc[2] - 0.45f, mr[1], 1e-5f);
            AssertSpans(new[] { 1f }, 0f, 0.3f, 0.6f);
            Assert.AreEqual(-1, TooEasyLogic.SpanAt(null, null, 0f));
            Assert.AreEqual(-1, TooEasyLogic.SpanAt(new[] { -1f }, new[] { 1f }, float.NaN));
        }

        [Test]
        public void TheGapBetweenThisAndLevelIsSplitFairly()
        {
            float[] w = { 1.9f, 0.85f, 1.15f, 0.12f };
            float[] c = TooEasyLogic.Row(w, new[] { 0f, 0.2f, 0.2f, 0.02f }, out _);
            TooEasyLogic.HitSpans(c, w, 0.3f, 0.6f, out float[] l, out float[] r);
            float thisRight = c[1] + w[1] * 0.5f, levelLeft = c[2] - w[2] * 0.5f;
            Assert.AreEqual(1, TooEasyLogic.SpanAt(l, r, thisRight + 0.09f));
            Assert.AreEqual(2, TooEasyLogic.SpanAt(l, r, levelLeft - 0.09f));
            // il punto (cortissimo) ha comunque un'area larga almeno 0.6, che si allarga verso l'esterno
            Assert.GreaterOrEqual(r[3] - l[3], 0.6f - 1e-5f);
            Assert.GreaterOrEqual(r[2] - l[2], w[2]);
        }
    }
}
