using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 8 della roadmap: Love the Bug (il tocco sdoppia l'insetto, la pressione lunga lo fa innamorare).</summary>
    public class V77LoveTheBugTests
    {
        const float MinX = -1.85f, MaxX = 1.85f, MinY = -2.6f, MaxY = 1.75f, StartX = -0.9f, StartY = -0.5f;
        const int A = 0, B = 1;

        static LoveTheBugLogic New() => new LoveTheBugLogic(MinX, MaxX, MinY, MaxY, StartX, StartY);

        static void Tap(LoveTheBugLogic l, int bug, int finger = A)
        {
            Assert.AreEqual(LoveTheBugEvent.Pressed, l.Down(finger, l.BugX(bug), l.BugY(bug)));
            l.Tick(0.05f);
            LoveTheBugEvent e = l.Up(finger);
            Assert.That(e == LoveTheBugEvent.Split || e == LoveTheBugEvent.Scatter, "un tocco breve sdoppia o sparpaglia: " + e);
        }

        /// <summary>Tiene premuto per <paramref name="seconds"/> a piccoli passi, come il gioco.</summary>
        static LoveTheBugEvent Hold(LoveTheBugLogic l, float seconds)
        {
            LoveTheBugEvent last = LoveTheBugEvent.None;
            float t = 0f;
            while (t < seconds - 1e-4f)
            {
                float dt = System.Math.Min(1f / 60f, seconds - t);
                LoveTheBugEvent e = l.Tick(dt);
                if (e != LoveTheBugEvent.None) last = e;
                t += dt;
                if (e == LoveTheBugEvent.Loved) break;
            }
            return last;
        }

        static void AssertInBounds(LoveTheBugLogic l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                Assert.That(l.BugX(i), Is.InRange(MinX, MaxX));
                Assert.That(l.BugY(i), Is.InRange(MinY, MaxY));
            }
        }

        [Test]
        public void StartsWithOneWanderingBug()
        {
            var l = New();
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(StartX, l.BugX(0), 1e-5f);
            Assert.AreEqual(StartY, l.BugY(0), 1e-5f);
            Assert.AreEqual(0f, l.Glitch, 1e-5f);
            Assert.AreEqual(LoveTheBugLogic.StartProgress, l.Progress, 1e-5f);
            Assert.AreEqual(-1, l.MeterBug);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void HoldTwoSecondsWinsExactlyOnce()
        {
            var l = New();
            Assert.AreEqual(LoveTheBugEvent.Pressed, l.Down(A, l.BugX(0), l.BugY(0)));
            Assert.AreEqual(LoveTheBugEvent.Loved, Hold(l, LoveTheBugLogic.HoldTime + 0.02f));
            Assert.AreEqual(LoveTheBugState.Won, l.State);
            Assert.AreEqual(0, l.PressedBug);
            Assert.AreEqual(1f, l.Progress, 1e-5f);
            Assert.AreEqual(0f, l.Glitch, 1e-5f);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            // tutto il resto dopo la vittoria non cambia niente
            Assert.AreEqual(LoveTheBugEvent.None, l.Up(A));
            Assert.AreEqual(LoveTheBugEvent.Ignored, l.Down(B, l.BugX(0), l.BugY(0)));
            Assert.AreEqual(LoveTheBugEvent.None, l.Tick(5f));
            l.Cancel();
            Assert.AreEqual(LoveTheBugState.Won, l.State);
            Assert.AreEqual(1, l.Count);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void JustUnderTwoSecondsIsNotEnough()
        {
            var l = New();
            l.Down(A, l.BugX(0), l.BugY(0));
            Assert.AreEqual(LoveTheBugEvent.None, Hold(l, LoveTheBugLogic.HoldTime - 0.05f));
            Assert.AreEqual(LoveTheBugState.Pressing, l.State);
            Assert.That(l.HoldProgress, Is.InRange(0.9f, 0.999f));
            Assert.AreEqual(LoveTheBugEvent.Loved, l.Tick(0.06f));
        }

        [Test]
        public void PressedBugStopsWhileOthersWalk()
        {
            var l = New();
            Tap(l, 0);
            Assert.AreEqual(2, l.Count);
            float x0 = l.BugX(0), y0 = l.BugY(0), x1 = l.BugX(1), y1 = l.BugY(1);
            l.Down(A, x0, y0);
            Hold(l, 1f);
            Assert.AreEqual(x0, l.BugX(0), 1e-5f);
            Assert.AreEqual(y0, l.BugY(0), 1e-5f);
            Assert.IsFalse(l.Walking(0));
            Assert.IsTrue(l.Walking(1));
            Assert.That(System.Math.Abs(l.BugX(1) - x1) + System.Math.Abs(l.BugY(1) - y1), Is.GreaterThan(0.1f));
        }

        [Test]
        public void TapsMultiplyUpToTheCapAndNeverWin()
        {
            var l = New();
            for (int n = 1; n < LoveTheBugLogic.MaxBugs; n++)
            {
                float glitchBefore = l.Glitch, barBefore = l.Progress;
                Tap(l, n - 1);
                Assert.AreEqual(n + 1, l.Count);
                Assert.Greater(l.Glitch, glitchBefore);
                Assert.Less(l.Progress, barBefore);
                l.Tick(0.3f);
                AssertInBounds(l);
            }
            Assert.AreEqual(1f, l.Glitch, 1e-5f);
            // al tetto: tocchi che sparpagliano soltanto
            for (int k = 0; k < 6; k++)
            {
                Assert.AreEqual(LoveTheBugEvent.Pressed, l.Down(A, l.BugX(k % l.Count), l.BugY(k % l.Count)));
                l.Tick(0.02f);
                Assert.AreEqual(LoveTheBugEvent.Scatter, l.Up(A));
                Assert.IsTrue(l.Scattering);
                Assert.AreEqual(LoveTheBugLogic.MaxBugs, l.Count);
                l.Tick(0.2f);
                AssertInBounds(l);
            }
            Assert.AreNotEqual(LoveTheBugState.Won, l.State);
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(LoveTheBugLogic.MaxBugs - 1 + 6, l.WrongTaps);
            Hold(l, LoveTheBugLogic.ScatterTime + 0.1f);
            Assert.IsFalse(l.Scattering);
        }

        [Test]
        public void RapidTapsInTheSameFrame()
        {
            var l = New();
            for (int k = 0; k < 20; k++)
            {
                int i = k % l.Count;
                // giù e su nello stesso fotogramma (tempo zero): è un tocco
                Assert.AreEqual(LoveTheBugEvent.Pressed, l.Down(A, l.BugX(i), l.BugY(i)));
                LoveTheBugEvent e = l.Up(A);
                Assert.That(e == LoveTheBugEvent.Split || e == LoveTheBugEvent.Scatter);
            }
            Assert.AreEqual(LoveTheBugLogic.MaxBugs, l.Count);
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
            AssertInBounds(l);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ShortHoldReleasedDoesNothing()
        {
            var l = New();
            foreach (float s in new[] { LoveTheBugLogic.TapTime + 0.01f, 0.6f, 1.2f, LoveTheBugLogic.HoldTime - 0.1f })
            {
                Assert.AreEqual(LoveTheBugEvent.Pressed, l.Down(A, l.BugX(0), l.BugY(0)));
                Hold(l, s);
                Assert.AreEqual(LoveTheBugEvent.LetGo, l.Up(A), "pressione di " + s + " s");
                Assert.AreEqual(1, l.Count, "una pressione lunga non sdoppia mai");
                Assert.AreEqual(LoveTheBugState.Wandering, l.State);
                Assert.AreEqual(0, l.WrongTaps);
                Assert.AreEqual(0f, l.HoldProgress, 1e-5f);
                Assert.IsTrue(l.Walking(0));
            }
            Assert.AreEqual(4, l.LetGos);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void TapThresholdIsExact()
        {
            var l = New();
            l.Down(A, l.BugX(0), l.BugY(0));
            l.Tick(LoveTheBugLogic.TapTime - 0.01f);
            Assert.AreEqual(LoveTheBugEvent.Split, l.Up(A));
            Assert.AreEqual(2, l.Count);
            l.Down(A, l.BugX(0), l.BugY(0));
            l.Tick(LoveTheBugLogic.TapTime + 0.001f);
            Assert.AreEqual(LoveTheBugEvent.LetGo, l.Up(A));
            Assert.AreEqual(2, l.Count);
        }

        [Test]
        public void MeterDrainsAfterLetGo()
        {
            var l = New();
            l.Down(A, l.BugX(0), l.BugY(0));
            Hold(l, 1f);
            Assert.AreEqual(0.5f, l.MeterFill, 0.02f);
            Assert.AreEqual(0.5f, l.Blush(0), 0.02f);
            l.Up(A);
            Assert.AreEqual(0, l.MeterBug);
            l.Tick(0.1f);
            Assert.Less(l.MeterFill, 0.5f);
            Hold(l, LoveTheBugLogic.DrainTime + 0.1f);
            Assert.AreEqual(0f, l.Blush(0), 1e-5f);
            Assert.AreEqual(-1, l.MeterBug);
            // nuova pressione: il tempo riparte da zero (niente credito dalla volta prima)
            l.Down(A, l.BugX(0), l.BugY(0));
            Assert.AreEqual(LoveTheBugEvent.None, Hold(l, LoveTheBugLogic.HoldTime - 0.1f));
            Assert.AreEqual(LoveTheBugEvent.Loved, Hold(l, 0.2f));
        }

        [Test]
        public void MovingOffCancels()
        {
            var l = New();
            float x = l.BugX(0), y = l.BugY(0);
            l.Down(A, x, y);
            Hold(l, 1f);
            // piccoli spostamenti dentro il raggio: si continua
            Assert.AreEqual(LoveTheBugEvent.None, l.Move(A, x + 0.5f, y - 0.4f));
            Assert.AreEqual(LoveTheBugState.Pressing, l.State);
            // oltre il raggio: annullato, nessun sdoppiamento
            Assert.AreEqual(LoveTheBugEvent.LetGo, l.Move(A, x + LoveTheBugLogic.SlipRadius + 0.1f, y));
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(LoveTheBugEvent.None, l.Up(A));
            Assert.AreEqual(1, l.Count);
            Hold(l, 3f);
            Assert.AreNotEqual(LoveTheBugState.Won, l.State);
            // scivolato via subito (meno di un tocco): silenzio, niente sdoppiamento
            l.Down(A, l.BugX(0), l.BugY(0));
            Assert.AreEqual(LoveTheBugEvent.Slipped, l.Move(A, l.BugX(0) - 2f, l.BugY(0)));
            Assert.AreEqual(LoveTheBugEvent.None, l.Up(A));
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(0, l.WrongTaps);
        }

        [Test]
        public void HoldWorksOnAnyBugWithManyBugs()
        {
            for (int target = 0; target < LoveTheBugLogic.MaxBugs; target++)
            {
                var l = New();
                for (int n = 1; n < LoveTheBugLogic.MaxBugs; n++) { Tap(l, n - 1); l.Tick(0.4f); }
                Assert.AreEqual(LoveTheBugLogic.MaxBugs, l.Count);
                // si preme proprio quell'insetto (bersaglio unico anche se sovrapposto: si prende il più vicino)
                Assert.AreEqual(LoveTheBugEvent.Pressed, l.Down(A, l.BugX(target), l.BugY(target)));
                int got = l.PressedBug;
                Assert.AreEqual(LoveTheBugEvent.Loved, Hold(l, LoveTheBugLogic.HoldTime + 0.05f));
                Assert.AreEqual(got, l.PressedBug);
                Assert.IsTrue(l.TryComplete());
                Assert.IsFalse(l.TryComplete());
            }
        }

        [Test]
        public void GenerousHitRadiusOnAMovingBug()
        {
            var l = New();
            l.Tick(0.5f);
            float x = l.BugX(0), y = l.BugY(0);
            Assert.AreEqual(LoveTheBugEvent.Missed, l.Down(A, x + LoveTheBugLogic.HitRadius + 0.05f, y));
            Assert.AreEqual(LoveTheBugEvent.Pressed, l.Down(A, x + LoveTheBugLogic.HitRadius - 0.05f, y));
            // si ferma subito
            l.Tick(0.5f);
            Assert.AreEqual(x, l.BugX(0), 1e-5f);
        }

        [Test]
        public void SecondFingerIsIgnored()
        {
            var l = New();
            Tap(l, 0);
            l.Down(A, l.BugX(0), l.BugY(0));
            Hold(l, 0.5f);
            Assert.AreEqual(LoveTheBugEvent.Ignored, l.Down(B, l.BugX(1), l.BugY(1)));
            Assert.AreEqual(LoveTheBugEvent.None, l.Move(B, 5f, 5f));
            Assert.AreEqual(LoveTheBugEvent.None, l.Up(B));
            Assert.AreEqual(2, l.Count);
            Assert.AreEqual(A, l.Holder);
            Assert.AreEqual(LoveTheBugEvent.Loved, Hold(l, LoveTheBugLogic.HoldTime));
        }

        [Test]
        public void SecondFingerTapDuringHoldNeverSplits()
        {
            var l = New();
            l.Down(A, l.BugX(0), l.BugY(0));
            for (int k = 0; k < 10; k++)
            {
                l.Down(B, l.BugX(0), l.BugY(0));
                l.Tick(0.05f);
                l.Up(B);
            }
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(0, l.WrongTaps);
            Assert.AreEqual(LoveTheBugEvent.Loved, Hold(l, LoveTheBugLogic.HoldTime));
        }

        [Test]
        public void MissedTouchesDoNothing()
        {
            var l = New();
            Assert.AreEqual(LoveTheBugEvent.Missed, l.Down(A, 1.8f, 1.7f));
            Assert.AreEqual(LoveTheBugEvent.None, l.Up(A));
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(0, l.WrongTaps);
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
        }

        [Test]
        public void HugHintAfterFiveWrongTapsOnce()
        {
            var l = New();
            for (int k = 0; k < LoveTheBugLogic.HintAfter - 1; k++) { Tap(l, 0); Assert.IsFalse(l.TakeHint()); }
            Tap(l, 0);
            Assert.IsTrue(l.TakeHint());
            Tap(l, 0);
            Assert.IsFalse(l.TakeHint());
        }

        [Test]
        public void CancelIsValidInEveryState()
        {
            // a riposo
            var l = New();
            l.Cancel();
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
            // durante un tocco (meno di TapTime): niente sdoppiamento
            l.Down(A, l.BugX(0), l.BugY(0));
            l.Cancel();
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(LoveTheBugEvent.None, l.Up(A));
            Assert.AreEqual(1, l.Count);
            // durante una pressione lunga: annullata, nessuna battuta, nessun conteggio
            l.Down(A, l.BugX(0), l.BugY(0));
            Hold(l, 1.5f);
            l.Cancel();
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
            Assert.AreEqual(0, l.LetGos);
            Assert.AreEqual(LoveTheBugEvent.None, l.Up(A));
            Hold(l, 3f);
            Assert.AreNotEqual(LoveTheBugState.Won, l.State);
            // durante lo sparpagliamento
            for (int n = 1; n < LoveTheBugLogic.MaxBugs + 1; n++) Tap(l, 0);
            Assert.IsTrue(l.Scattering);
            l.Cancel();
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
            // dopo la pausa si vince normalmente
            l.Down(A, l.BugX(3), l.BugY(3));
            Assert.AreEqual(LoveTheBugEvent.Loved, Hold(l, LoveTheBugLogic.HoldTime));
            l.Cancel();
            Assert.AreEqual(LoveTheBugState.Won, l.State);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = New();
            for (int k = 0; k < 9; k++) Tap(l, 0);
            l.Down(A, l.BugX(2), l.BugY(2));
            Hold(l, 1f);
            l.Reset();
            Assert.AreEqual(LoveTheBugState.Wandering, l.State);
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(0, l.WrongTaps);
            Assert.AreEqual(LoveTheBugLogic.NoFinger, l.Holder);
            Assert.AreEqual(-1, l.PressedBug);
            Assert.AreEqual(StartX, l.BugX(0), 1e-5f);
            Assert.AreEqual(StartY, l.BugY(0), 1e-5f);
            Assert.IsFalse(l.Scattering);
            Assert.IsFalse(l.TakeHint());
            // vinto e ricominciato: si può rivincere (una volta)
            l.Down(A, l.BugX(0), l.BugY(0));
            Hold(l, LoveTheBugLogic.HoldTime);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.IsFalse(l.TryComplete());
            l.Down(A, l.BugX(0), l.BugY(0));
            Hold(l, LoveTheBugLogic.HoldTime);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void PathsAreDeterministicAndInBounds()
        {
            var a = New();
            var b = New();
            for (int k = 0; k < 7; k++) { Tap(a, 0); Tap(b, 0); }
            for (int s = 0; s < 1200; s++)
            {
                a.Tick(1f / 60f);
                b.Tick(1f / 60f);
                AssertInBounds(a);
            }
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a.BugX(i), b.BugX(i), 1e-5f);
                Assert.AreEqual(a.BugY(i), b.BugY(i), 1e-5f);
            }
            // un passo enorme (fotogramma lunghissimo) non li fa uscire
            a.Tick(10f);
            AssertInBounds(a);
        }

        [Test]
        public void NegativeTimeIsHarmless()
        {
            var l = New();
            l.Down(A, l.BugX(0), l.BugY(0));
            Assert.AreEqual(LoveTheBugEvent.None, l.Tick(-5f));
            Assert.AreEqual(0f, l.HoldProgress, 1e-5f);
            Assert.AreEqual(LoveTheBugEvent.Split, l.Up(A));
        }
    }
}
