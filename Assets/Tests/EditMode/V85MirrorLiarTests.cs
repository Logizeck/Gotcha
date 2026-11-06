using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 91 della roadmap: Mirror Liar (due dita sui due personaggi, mosse in modo diverso).</summary>
    public class V85MirrorLiarTests
    {
        const int L = MirrorLiarLogic.LeftSide, R = MirrorLiarLogic.RightSide;
        const float Head = 1.2f;   // il dito si appoggia sul busto (piedi + 1.2)

        static MirrorLiarLogic Make(int real, int seed = 7)
        {
            var l = new MirrorLiarLogic(seed);
            Assert.IsTrue(l.SetRealSide(real));
            Assert.AreEqual(real, l.RealSide);
            return l;
        }

        static void AssertStillPlaying(MirrorLiarLogic l)
        {
            Assert.AreEqual(MirrorLiarStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static void AssertMirrored(MirrorLiarLogic l)
        {
            Assert.AreEqual(MirrorLiarLogic.Mirror(l.LeftX), l.RightX, 1e-4f, "i due personaggi sono sempre speculari");
            Assert.AreEqual(l.Y(L), l.Y(R), 1e-5f);
            Assert.LessOrEqual(l.LeftX, MirrorLiarLogic.MirrorX - MirrorLiarLogic.MinAbsX + 1e-4f);
            Assert.GreaterOrEqual(l.LeftX, MirrorLiarLogic.MirrorX - MirrorLiarLogic.MaxAbsX - 1e-4f);
            Assert.GreaterOrEqual(l.RightX, MirrorLiarLogic.MirrorX + MirrorLiarLogic.MinAbsX - 1e-4f);
            Assert.LessOrEqual(l.RightX, MirrorLiarLogic.MirrorX + MirrorLiarLogic.MaxAbsX + 1e-4f);
            Assert.GreaterOrEqual(l.FeetY, MirrorLiarLogic.MinY - 1e-5f);
            Assert.LessOrEqual(l.FeetY, MirrorLiarLogic.MaxY + 1e-5f);
        }

        static float GrabX(MirrorLiarLogic l, int side) => l.X(side);
        static float GrabY(MirrorLiarLogic l) => l.FeetY + Head;

        /// <summary>Trascina con un dito solo il personaggio <paramref name="side"/> di (dx, dy) in più passi.</summary>
        static MirrorLiarEvent SoloDrag(MirrorLiarLogic l, int side, float dx, float dy, int id = 1)
        {
            float x = GrabX(l, side), y = GrabY(l);
            Assert.AreEqual(side == L ? MirrorLiarPress.Left : MirrorLiarPress.Right, l.Press(id, x, y));
            for (int i = 1; i <= 8; i++)
            {
                l.Move(id, x + dx * i / 8f, y + dy * i / 8f);
                AssertMirrored(l);
            }
            return l.Release(id);
        }

        /// <summary>Due dita (id 1 a sinistra, id 2 a destra) mosse di (ldx, ldy) e (rdx, rdy); restituisce l'ultimo evento dei movimenti.</summary>
        static MirrorLiarEvent TwoDrag(MirrorLiarLogic l, float ldx, float ldy, float rdx, float rdy, bool rightFirst = false, int steps = 10)
        {
            float lx = GrabX(l, L), rx = GrabX(l, R), y = GrabY(l);
            if (rightFirst)
            {
                Assert.AreEqual(MirrorLiarPress.Right, l.Press(2, rx, y));
                Assert.AreEqual(MirrorLiarPress.Left, l.Press(1, lx, y));
            }
            else
            {
                Assert.AreEqual(MirrorLiarPress.Left, l.Press(1, lx, y));
                Assert.AreEqual(MirrorLiarPress.Right, l.Press(2, rx, y));
            }
            Assert.IsTrue(l.TwoHanded);
            MirrorLiarEvent last = MirrorLiarEvent.None;
            for (int i = 1; i <= steps; i++)
            {
                float k = i / (float)steps;
                MirrorLiarEvent a = l.Move(1, lx + ldx * k, y + ldy * k);
                if (a == MirrorLiarEvent.Revealed) return a;
                MirrorLiarEvent b = l.Move(2, rx + rdx * k, y + rdy * k);
                if (b == MirrorLiarEvent.Revealed) return b;
                AssertMirrored(l);
                last = b;
            }
            return last;
        }

        [Test]
        public void StartsMirroredAndPlaying()
        {
            var l = new MirrorLiarLogic(3);
            AssertStillPlaying(l);
            AssertMirrored(l);
            Assert.AreEqual(-MirrorLiarLogic.StartAbsX, l.LeftX, 1e-5f);
            Assert.AreEqual(MirrorLiarLogic.StartAbsX, l.RightX, 1e-5f);
            Assert.AreEqual(MirrorLiarLogic.StartY, l.FeetY, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.FingerCount);
            Assert.That(l.RealSide == L || l.RealSide == R);
            Assert.AreEqual(1 - l.RealSide, l.FakeSide);
        }

        [Test]
        public void RealSideIsRandomPerAttemptButSeeded()
        {
            var a = new MirrorLiarLogic(1234);
            var b = new MirrorLiarLogic(1234);
            bool sawL = false, sawR = false;
            for (int i = 0; i < 40; i++)
            {
                Assert.AreEqual(a.RealSide, b.RealSide, "stesso seme, stessa sequenza");
                if (a.RealSide == L) sawL = true; else sawR = true;
                a.Reset();
                b.Reset();
            }
            Assert.IsTrue(sawL && sawR, "a ogni tentativo il vero può essere a sinistra o a destra");
        }

        [TestCase(L, L)]
        [TestCase(L, R)]
        [TestCase(R, L)]
        [TestCase(R, R)]
        public void OneFingerDragsNeverWinAndMirror(int real, int grab)
        {
            var l = Make(real);
            float dirX = grab == L ? -1f : 1f;
            Assert.AreEqual(MirrorLiarEvent.Dropped, SoloDrag(l, grab, 0.4f * dirX, 0.5f));
            AssertStillPlaying(l);
            AssertMirrored(l);
            Assert.AreEqual(MirrorLiarLogic.StartAbsX + 0.4f, l.RightX, 1e-4f, "il preso segue il dito, l'altro lo specchia");
            Assert.AreEqual(MirrorLiarLogic.StartY + 0.5f, l.FeetY, 1e-4f);
            // avanti e indietro, su e giù, di qua e di là: mai una vittoria
            Assert.AreEqual(MirrorLiarEvent.Dropped, SoloDrag(l, 1 - grab, 3f, -3f));
            Assert.AreEqual(MirrorLiarEvent.Dropped, SoloDrag(l, grab, -3f, 3f));
            Assert.AreEqual(MirrorLiarEvent.Dropped, SoloDrag(l, 1 - grab, 0.7f, 0.2f));
            AssertStillPlaying(l);
            AssertMirrored(l);
            Assert.AreEqual(4, l.SoloDrags);
            Assert.AreEqual(4, l.WrongMoves);
        }

        [Test]
        public void GrabbedFigureFollowsFingerTowardsTheMirror()
        {
            var l = Make(R);
            // si prende il falso (a sinistra) e lo si porta verso lo specchio: il vero (a destra) viene incontro
            SoloDrag(l, L, 0.3f, 0f);
            Assert.AreEqual(-MirrorLiarLogic.StartAbsX + 0.3f, l.LeftX, 1e-4f);
            Assert.AreEqual(MirrorLiarLogic.StartAbsX - 0.3f, l.RightX, 1e-4f);
        }

        [TestCase(L)]
        [TestCase(R)]
        public void TapsAreMirroredAndNeverWin(int real)
        {
            var l = Make(real);
            for (int i = 0; i < 6; i++)
            {
                int side = i % 2;
                Assert.AreEqual(side == L ? MirrorLiarPress.Left : MirrorLiarPress.Right, l.Press(5, l.X(side), GrabY(l)));
                l.Move(5, l.X(side) + 0.05f, GrabY(l) - 0.05f);   // tremolio sotto la soglia del tocco
                Assert.AreEqual(MirrorLiarEvent.Tapped, l.Release(5));
                AssertMirrored(l);
            }
            Assert.AreEqual(-MirrorLiarLogic.StartAbsX, l.LeftX, 1e-5f, "un tocco non sposta");
            Assert.AreEqual(6, l.Taps);
            AssertStillPlaying(l);
        }

        [Test]
        public void MirrorTapsAndEmptyTouchesDoNothing()
        {
            var l = Make(L);
            Assert.AreEqual(MirrorLiarPress.Mirror, l.Press(1, 0f, 0f));
            Assert.AreEqual(MirrorLiarEvent.MirrorTapped, l.Release(1));
            Assert.AreEqual(MirrorLiarPress.None, l.Press(2, 2.5f, 3.5f));
            Assert.AreEqual(MirrorLiarEvent.None, l.Move(2, 1.2f, -1.5f));
            Assert.AreEqual(MirrorLiarEvent.None, l.Release(2));
            Assert.AreEqual(1, l.MirrorTaps);
            AssertStillPlaying(l);
            AssertMirrored(l);
            Assert.AreEqual(-MirrorLiarLogic.StartAbsX, l.LeftX, 1e-5f);
        }

        [TestCase(L)]
        [TestCase(R)]
        public void TwoFingersMovingMirroredDoNotReveal(int real)
        {
            var l = Make(real);
            // verso l'esterno, poi verso lo specchio, poi su e giù: sempre speculari
            Assert.AreEqual(MirrorLiarEvent.Moved, TwoDrag(l, -0.45f, 0.6f, 0.45f, 0.6f));
            AssertStillPlaying(l);
            Assert.Less(l.Separation, 1e-3f);
            Assert.AreEqual(MirrorLiarEvent.Synced, l.Release(1));
            Assert.AreEqual(MirrorLiarEvent.Dropped, l.Release(2));
            Assert.AreEqual(1, l.SyncTries);
            Assert.AreEqual(MirrorLiarEvent.Moved, TwoDrag(l, 0.4f, -0.8f, -0.4f, -0.8f));
            AssertStillPlaying(l);
            Assert.AreEqual(MirrorLiarEvent.Synced, l.Release(2));
            Assert.AreEqual(MirrorLiarEvent.Dropped, l.Release(1));
            AssertStillPlaying(l);
            AssertMirrored(l);
        }

        [Test]
        public void MirroredMotionPastTheClampsStillDoesNotReveal()
        {
            var l = Make(L);
            // le dita vanno molto oltre i bordi (i personaggi si fermano), ma restano speculari fra loro
            Assert.AreEqual(MirrorLiarEvent.Moved, TwoDrag(l, -3f, 4f, 3f, 4f, false, 30));
            AssertStillPlaying(l);
            AssertMirrored(l);
            Assert.AreEqual(-MirrorLiarLogic.MaxAbsX, l.LeftX, 1e-4f);
            Assert.AreEqual(MirrorLiarLogic.MaxY, l.FeetY, 1e-4f);
            Assert.AreEqual(MirrorLiarEvent.Moved, TwoDrag2Continue(l, 4f, -6f, -4f, -6f));
            AssertStillPlaying(l);
            Assert.AreEqual(-MirrorLiarLogic.MinAbsX, l.LeftX, 1e-4f, "verso lo specchio: si fermano a distanza");
            Assert.AreEqual(MirrorLiarLogic.MinY, l.FeetY, 1e-4f);
        }

        /// <summary>Continua a muovere le due dita già giù (id 1 e 2) dalle loro ultime posizioni.</summary>
        static MirrorLiarEvent TwoDrag2Continue(MirrorLiarLogic l, float ldx, float ldy, float rdx, float rdy)
        {
            // le dita ripartono da dove erano (le si ricava dai bersagli: bersaglio = dito + presa)
            Assert.IsTrue(l.FingerTarget(L, out float ltx, out float lty));
            Assert.IsTrue(l.FingerTarget(R, out float rtx, out float rty));
            float lx = ltx, ly = lty + Head, rx = rtx, ry = rty + Head;   // dita sul busto (la presa era a +Head)
            MirrorLiarEvent last = MirrorLiarEvent.None;
            for (int i = 1; i <= 30; i++)
            {
                float k = i / 30f;
                MirrorLiarEvent a = l.Move(1, lx + ldx * k, ly + ldy * k);
                if (a == MirrorLiarEvent.Revealed) return a;
                last = l.Move(2, rx + rdx * k, ry + rdy * k);
                if (last == MirrorLiarEvent.Revealed) return last;
                AssertMirrored(l);
            }
            return last;
        }

        [TestCase(L, false)]
        [TestCase(R, false)]
        [TestCase(L, true)]
        [TestCase(R, true)]
        public void OppositeDirectionsRevealAndWinOnce(int real, bool rightFirst)
        {
            var l = Make(real);
            // tutte e due le dita verso destra (direzioni "opposte" rispetto al riflesso)
            Assert.AreEqual(MirrorLiarEvent.Revealed, TwoDrag(l, 0.6f, 0f, 0.6f, 0f, rightFirst));
            Assert.AreEqual(MirrorLiarStage.Revealed, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(0, l.FingerCount, "le dita vengono lasciate");
            Assert.AreEqual(MirrorLiarEvent.None, l.Release(1));
            Assert.AreEqual(MirrorLiarEvent.None, l.Release(2));
            Assert.AreEqual(MirrorLiarPress.None, l.Press(3, l.X(L), GrabY(l)));
            AssertMirrored(l);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "la vittoria una volta sola");
            Assert.AreEqual(MirrorLiarStage.Done, l.Stage);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [TestCase(L)]
        [TestCase(R)]
        public void TheRealOneFollowsItsFingerTheFakeDoesNot(int real)
        {
            var l = Make(real);
            float lx = l.LeftX, rx = l.RightX, y = GrabY(l);
            l.Press(1, lx, y);
            l.Press(2, rx, y);
            // il dito di sinistra resta fermo, quello di destra si alza di 0.6 (sotto la soglia di rivelazione)
            for (int i = 1; i <= 6; i++) l.Move(2, rx, y + 0.1f * i);
            AssertStillPlaying(l);
            Assert.AreEqual(0.6f, l.Separation, 1e-3f);
            Assert.AreEqual(0.6f / MirrorLiarLogic.RevealDistance, l.Strain, 1e-3f);
            if (real == R) Assert.AreEqual(MirrorLiarLogic.StartY + 0.6f, l.FeetY, 1e-3f, "il vero (destra) segue il suo dito");
            else Assert.AreEqual(MirrorLiarLogic.StartY, l.FeetY, 1e-3f, "il vero (sinistra) resta col suo dito fermo; il falso non si alza");
            AssertMirrored(l);
            // ancora un po': il falso non segue più, si rivela
            Assert.AreEqual(MirrorLiarEvent.Revealed, l.Move(2, rx, y + 0.95f));
        }

        [Test]
        public void OneStillFingerAndOneMovingFingerReveal()
        {
            var l = Make(R);
            float lx = l.LeftX, rx = l.RightX, y = GrabY(l);
            Assert.AreEqual(MirrorLiarPress.Left, l.Press(1, lx, y));
            Assert.AreEqual(MirrorLiarPress.Right, l.Press(2, rx, y));
            MirrorLiarEvent e = MirrorLiarEvent.None;
            for (int i = 1; i <= 10 && e != MirrorLiarEvent.Revealed; i++) e = l.Move(1, lx - 0.15f * i, y);
            Assert.AreEqual(MirrorLiarEvent.Revealed, e, "anche muovendo solo il dito sul falso");
        }

        [Test]
        public void SmallDifferencesBelowTheThresholdDoNotReveal()
        {
            var l = Make(L);
            Assert.AreEqual(MirrorLiarEvent.Moved, TwoDrag(l, -0.4f, 0.3f, 0.4f + 0.5f, 0.3f));
            AssertStillPlaying(l);
            Assert.AreEqual(0.5f, l.Separation, 1e-3f);
            // tornando speculari la tensione sparisce
            l.Move(2, l.RightX + 0f, GrabY(l));
            Assert.IsFalse(l.InputLocked);
        }

        [TestCase(L, 1)]
        [TestCase(L, 2)]
        [TestCase(R, 1)]
        [TestCase(R, 2)]
        public void LiftingOneFingerGoesBackToMirroredModeWithoutJumps(int real, int lift)
        {
            var l = Make(real);
            float lx0 = l.LeftX, rx0 = l.RightX, y0 = GrabY(l);
            Assert.AreEqual(MirrorLiarEvent.Moved, TwoDrag(l, 0.35f, 0f, 0.35f, 0f));
            Assert.Greater(l.Separation, 0.6f);
            float beforeL = l.LeftX, beforeY = l.FeetY;
            Assert.AreEqual(MirrorLiarEvent.Synced, l.Release(lift));
            Assert.AreEqual(0f, l.Separation, 1e-5f);
            Assert.IsFalse(l.TwoHanded);
            Assert.AreEqual(1, l.FingerCount);
            Assert.AreEqual(beforeL, l.LeftX, 1e-5f, "alzare un dito non sposta niente");
            int keep = lift == 1 ? 2 : 1;
            int keptSide = keep == 1 ? L : R;
            Assert.IsTrue(l.FingerTarget(keptSide, out float tx, out float ty));
            Assert.AreEqual(l.X(keptSide), tx, 1e-4f, "riallineato: il bersaglio è dove sta il personaggio");
            Assert.AreEqual(beforeY, ty, 1e-4f);
            // il dito rimasto (dove l'ha lasciato TwoDrag) si muove ancora: il suo personaggio lo segue da dov'è, l'altro specchia
            float fingerX = (keep == 1 ? lx0 : rx0) + 0.35f;
            float keptX = l.X(keptSide);
            Assert.AreEqual(MirrorLiarEvent.Moved, l.Move(keep, fingerX, y0 + 0.2f));
            Assert.AreEqual(keptX, l.X(keptSide), 1e-4f, "nessun salto in orizzontale");
            Assert.AreEqual(beforeY + 0.2f, l.FeetY, 1e-4f, "segue il dito da dove era");
            AssertMirrored(l);
            MirrorLiarEvent e = MirrorLiarEvent.None;
            // si muove molto: a un dito non si rivela mai
            for (int i = 1; i <= 10; i++) e = l.Move(keep, fingerX - 0.3f * i, y0 + 0.2f + 0.08f * i);
            Assert.AreEqual(MirrorLiarEvent.Moved, e);
            AssertStillPlaying(l);
            AssertMirrored(l);
            Assert.AreEqual(MirrorLiarEvent.Dropped, l.Release(keep));
            Assert.AreEqual(0, l.FingerCount);
            // e si può riprovare subito a due dita
            Assert.AreEqual(MirrorLiarEvent.Revealed, TwoDrag(l, -0.5f, 0f, -0.5f, 0f));
        }

        [Test]
        public void SwitchingFromOneToTwoFingersDoesNotJumpOrReveal()
        {
            var l = Make(L);
            float lx = l.LeftX, y = GrabY(l);
            l.Press(1, lx, y);
            // il personaggio a sinistra spinto oltre il bordo: il dito va lontano, il personaggio si ferma
            for (int i = 1; i <= 10; i++) l.Move(1, lx - 0.2f * i, y);
            Assert.AreEqual(-MirrorLiarLogic.MaxAbsX, l.LeftX, 1e-4f);
            // il secondo dito arriva sull'altro: le prese si riallineano, nessuna rivelazione
            Assert.AreEqual(MirrorLiarPress.Right, l.Press(2, l.RightX, GrabY(l)));
            Assert.IsTrue(l.TwoHanded);
            Assert.AreEqual(0f, l.Separation, 1e-5f);
            Assert.AreEqual(MirrorLiarEvent.Moved, l.Move(1, lx - 2f, y));
            Assert.Less(l.Separation, 1e-3f, "il dito fermo dov'era: nessuna differenza");
            AssertStillPlaying(l);
        }

        [Test]
        public void ThirdFingerAndSecondFingerOnSameFigureAreIgnored()
        {
            var l = Make(R);
            float y = GrabY(l);
            Assert.AreEqual(MirrorLiarPress.Left, l.Press(1, l.LeftX, y));
            Assert.AreEqual(MirrorLiarPress.SameFigure, l.Press(2, l.LeftX + 0.3f, y + 0.4f));
            Assert.IsFalse(l.TwoHanded);
            Assert.AreEqual(1, l.SameFigureTries);
            Assert.AreEqual(MirrorLiarPress.None, l.Press(3, l.RightX, y), "al massimo due dita");
            Assert.AreEqual(MirrorLiarPress.None, l.Press(1, l.RightX, y), "lo stesso id non si appoggia due volte");
            // il dito "in più" che si muove non fa niente
            Assert.AreEqual(MirrorLiarEvent.None, l.Move(2, l.LeftX + 2f, y));
            Assert.AreEqual(-MirrorLiarLogic.StartAbsX, l.LeftX, 1e-5f);
            Assert.AreEqual(MirrorLiarEvent.None, l.Release(2));
            // ora c'è posto: il secondo dito sull'altro personaggio vale
            Assert.AreEqual(MirrorLiarPress.Right, l.Press(3, l.RightX, y));
            Assert.IsTrue(l.TwoHanded);
            AssertStillPlaying(l);
        }

        [Test]
        public void FiguresStayInsideTheirHalfAndOnTheFloor()
        {
            var l = Make(L);
            float[][] moves = { new[] { 5f, 5f }, new[] { -5f, -5f }, new[] { 5f, -5f }, new[] { -5f, 5f }, new[] { 0.6f, 0.1f } };
            foreach (int side in new[] { L, R })
                foreach (float[] m in moves)
                {
                    SoloDrag(l, side, m[0], m[1]);
                    AssertMirrored(l);
                    Assert.Less(l.LeftX, MirrorLiarLogic.MirrorX);
                    Assert.Greater(l.RightX, MirrorLiarLogic.MirrorX);
                    Assert.LessOrEqual(System.Math.Abs(l.LeftX) + 0.5f, 2.25f + 1e-4f, "il personaggio (largo 1) resta nella zona sicura");
                }
            AssertStillPlaying(l);
        }

        [Test]
        public void HitAreasAreGenerousAndOnTheirOwnSide()
        {
            var l = Make(L);
            Assert.AreEqual(L, l.FigureAt(l.LeftX - 0.75f, l.FeetY + 2.2f));
            Assert.AreEqual(R, l.FigureAt(l.RightX + 0.75f, l.FeetY - 0.25f));
            Assert.AreEqual(-1, l.FigureAt(l.LeftX, l.FeetY + 2.6f));
            // trascinati vicino allo specchio, le aree non passano dall'altra parte
            SoloDrag(l, L, 2f, 0f);
            Assert.AreEqual(-MirrorLiarLogic.MinAbsX, l.LeftX, 1e-4f);
            Assert.AreEqual(L, l.FigureAt(-0.01f, l.FeetY + 1f));
            Assert.AreEqual(R, l.FigureAt(0.01f, l.FeetY + 1f));
            Assert.IsTrue(MirrorLiarLogic.OnMirror(0f, 1.5f));
            Assert.IsFalse(MirrorLiarLogic.OnMirror(0.5f, 1.5f));
        }

        [Test]
        public void RapidTouchesNeverBreakTheState()
        {
            var l = Make(R);
            var px = new float[3];
            var py = new float[3];
            for (int i = 0; i < 200; i++)
            {
                int id = i % 3;
                float x = (i * 37 % 11 - 5) * 0.45f, y = (i * 13 % 9 - 4) * 0.6f - 1f;
                if (!l.Tracks(id)) { l.Press(id, x, y); px[id] = x; py[id] = y; }
                // tocchi rapidi: ogni dito trema appena attorno a dove si è appoggiato (sotto la soglia del tocco)
                l.Move(id, px[id] + 0.02f * (i % 5), py[id]);
                if (i % 2 == 0) l.Release(id);
                if (i % 7 == 0) l.Release((id + 1) % 3);
                AssertMirrored(l);
                Assert.LessOrEqual(l.FingerCount, MirrorLiarLogic.MaxFingers);
            }
            l.Cancel();
            Assert.AreEqual(0, l.FingerCount);
            AssertStillPlaying(l);
            // si vince ancora, una volta sola
            Assert.AreEqual(MirrorLiarEvent.Revealed, TwoDrag(l, 0f, 0.5f, 0f, -0.5f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void EarlyReleaseDuringDragIsJustADrop()
        {
            var l = Make(L);
            float x = l.LeftX, y = GrabY(l);
            l.Press(1, x, y);
            l.Move(1, x - 0.3f, y);
            Assert.AreEqual(MirrorLiarEvent.Dropped, l.Release(1));
            Assert.AreEqual(MirrorLiarEvent.None, l.Move(1, x - 1f, y), "dopo il rilascio il dito non conta più");
            Assert.AreEqual(-MirrorLiarLogic.StartAbsX - 0.3f, l.LeftX, 1e-4f);
            AssertStillPlaying(l);
        }

        [Test]
        public void CancelLetsGoOfEverythingAndNeverWins()
        {
            var l = Make(L);
            Assert.AreEqual(MirrorLiarEvent.Moved, TwoDrag(l, 0.4f, 0f, 0.4f, 0f));
            Assert.Greater(l.Separation, 0.7f);
            float x = l.LeftX;
            l.Cancel();   // app in background a un passo dalla rivelazione
            Assert.AreEqual(0, l.FingerCount);
            Assert.AreEqual(0f, l.Separation);
            Assert.IsFalse(l.TwoHanded);
            Assert.AreEqual(x, l.LeftX, 1e-5f, "i personaggi restano dove sono");
            Assert.AreEqual(MirrorLiarEvent.None, l.Move(1, 3f, 3f));
            Assert.AreEqual(MirrorLiarEvent.None, l.Release(2));
            AssertStillPlaying(l);
            AssertMirrored(l);
            // al ritorno si riprova: serve di nuovo tutta la distanza
            Assert.AreEqual(MirrorLiarEvent.Revealed, TwoDrag(l, 0.6f, 0f, 0.6f, 0f));
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = Make(L);
            SoloDrag(l, L, -0.4f, 0.4f);
            l.Press(1, 0f, 0f);
            l.Release(1);
            TwoDrag(l, 0.6f, 0f, 0.6f, 0f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.FingerCount);
            Assert.AreEqual(-MirrorLiarLogic.StartAbsX, l.LeftX, 1e-5f);
            Assert.AreEqual(MirrorLiarLogic.StartY, l.FeetY, 1e-5f);
            // si rigioca e si rivince
            Assert.AreEqual(MirrorLiarEvent.Revealed, TwoDrag(l, -0.6f, 0f, -0.6f, 0f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void SetRealSideOnlyWhenIdle()
        {
            var l = Make(L);
            float lx = l.LeftX;
            Assert.IsTrue(l.SetRealSide(R));
            Assert.AreEqual(lx, l.LeftX, 1e-5f, "la scena non cambia");
            l.Press(1, l.LeftX, GrabY(l));
            Assert.IsFalse(l.SetRealSide(L), "non con un dito giù");
            l.Release(1);
            Assert.IsFalse(l.SetRealSide(5));
        }

        [Test]
        public void ThreeWrongAttemptsInARowThenTheSolution()
        {
            var l = Make(R);
            Assert.AreEqual(MirrorLiarEvent.Dropped, SoloDrag(l, R, 0.3f, 0f));            // 1: trascinamento a un dito
            l.Press(1, l.LeftX, GrabY(l));
            Assert.AreEqual(MirrorLiarEvent.Tapped, l.Release(1));                         // 2: tocco
            Assert.AreEqual(MirrorLiarEvent.Moved, TwoDrag(l, -0.4f, 0f, 0.4f, 0f));       // 3: due dita speculari
            Assert.AreEqual(MirrorLiarEvent.Synced, l.Release(1));
            Assert.AreEqual(MirrorLiarEvent.Dropped, l.Release(2));
            l.Press(1, 0f, -1f);
            Assert.AreEqual(MirrorLiarEvent.MirrorTapped, l.Release(1));                   // 4: lo specchio
            Assert.AreEqual(5, l.WrongMoves);
            AssertStillPlaying(l);
            Assert.AreEqual(MirrorLiarEvent.Revealed, TwoDrag(l, 0.3f, 0.5f, 0.3f, -0.5f)); // soluzione
            Assert.IsTrue(l.TryComplete());
        }
    }
}
