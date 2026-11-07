using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 138 della roadmap: The Broken Applause (due dita, una per mano, portate a incontrarsi).</summary>
    public class V88BrokenApplauseTests
    {
        const int L = BrokenApplauseLogic.Left, R = BrokenApplauseLogic.Right;
        const float H = BrokenApplauseLogic.HomeX;

        static void AssertStillPlaying(BrokenApplauseLogic l)
        {
            Assert.AreEqual(BrokenApplauseStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.AreEqual(0, l.OvationStarts);
            Assert.IsFalse(l.TryComplete());
        }

        static void AssertHome(BrokenApplauseLogic l)
        {
            Assert.AreEqual(-H, l.X(L), 1e-5f);
            Assert.AreEqual(H, l.X(R), 1e-5f);
            Assert.AreEqual(0f, l.Y(L), 1e-5f);
            Assert.AreEqual(0f, l.Y(R), 1e-5f);
            Assert.AreEqual(0, l.HeldCount);
        }

        static void AssertInBounds(BrokenApplauseLogic l)
        {
            foreach (int w in new[] { L, R })
            {
                Assert.LessOrEqual(System.Math.Abs(l.X(w)), BrokenApplauseLogic.MaxX + 1e-5f);
                Assert.GreaterOrEqual(l.Y(w), BrokenApplauseLogic.MinY - 1e-5f);
                Assert.LessOrEqual(l.Y(w), BrokenApplauseLogic.MaxY + 1e-5f);
                Assert.IsFalse(float.IsNaN(l.X(w)) || float.IsNaN(l.Y(w)));
            }
        }

        /// <summary>Un dito solo trascina una mano fino a (fx1, fy1) a passi, poi si alza.</summary>
        static BrokenApplauseDrop SoloDrag(BrokenApplauseLogic l, int finger, int who, float fx1, float fy1, int steps = 12)
        {
            float fx0 = l.X(who), fy0 = l.Y(who);
            Assert.IsTrue(l.Grab(finger, who, fx0, fy0));
            for (int i = 1; i <= steps; i++)
            {
                BrokenApplauseMove m = l.Move(finger, fx0 + (fx1 - fx0) * i / steps, fy0 + (fy1 - fy0) * i / steps);
                Assert.AreEqual(BrokenApplauseMove.Resisting, m);
                Assert.Greater(l.Gap, BrokenApplauseLogic.ClapGap, "da sola non arriva mai all'altra");
                Assert.AreEqual(l.HomeOf(1 - who), l.X(1 - who), 1e-5f, "l'altra resta a casa");
                AssertInBounds(l);
            }
            return l.Release(finger);
        }

        /// <summary>Due dita (a, b) prendono le due mani, nell'ordine dato, e le portano a (ax, ay) e (bx, by).</summary>
        static BrokenApplauseMove TwoFingers(BrokenApplauseLogic l, int fa, int whoA, int fb, int whoB, float ax, float ay, float bx, float by, int steps = 10)
        {
            float a0x = l.X(whoA), a0y = l.Y(whoA);
            Assert.IsTrue(l.Grab(fa, whoA, a0x, a0y));
            float b0x = l.X(whoB), b0y = l.Y(whoB);
            Assert.IsTrue(l.Grab(fb, whoB, b0x, b0y));
            Assert.IsTrue(l.Together);
            BrokenApplauseMove last = BrokenApplauseMove.None;
            for (int i = 1; i <= steps; i++)
            {
                float k = i / (float)steps;
                last = l.Move(fa, a0x + (ax - a0x) * k, a0y + (ay - a0y) * k);
                if (last == BrokenApplauseMove.Clap) return last;
                last = l.Move(fb, b0x + (bx - b0x) * k, b0y + (by - b0y) * k);
                if (last == BrokenApplauseMove.Clap) return last;
                AssertInBounds(l);
            }
            return last;
        }

        static void AssertClappedOnce(BrokenApplauseLogic l)
        {
            Assert.AreEqual(BrokenApplauseStage.Ovation, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(1, l.OvationStarts);
            Assert.AreEqual(0, l.HeldCount, "durante l'ovazione nessuna mano è più tenuta");
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(BrokenApplauseStage.Done, l.Stage);
            Assert.AreEqual(1, l.OvationStarts);
        }

        [Test]
        public void StartsApartAndBored()
        {
            var l = new BrokenApplauseLogic();
            AssertStillPlaying(l);
            AssertHome(l);
            Assert.IsFalse(l.Together || l.Near);
            Assert.AreEqual(2f * H, l.Gap, 1e-5f);
            Assert.AreEqual(1f, l.Apart, 1e-4f);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void OneHandDragsNeverWinAndSpringBack()
        {
            var l = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, SoloDrag(l, 0, L, 0f, 0f));      // sinistra verso il centro
            AssertHome(l);
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, SoloDrag(l, 0, R, 0f, 0f));      // destra verso il centro
            AssertHome(l);
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, SoloDrag(l, 5, L, H, 0f));       // fin sopra l'altra mano
            AssertHome(l);
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, SoloDrag(l, 5, R, -40f, -9f));   // lontanissimo
            AssertHome(l);
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, SoloDrag(l, 1, L, -5f, 4f));     // all'indietro: resta nello schermo
            AssertHome(l);
            Assert.AreEqual(5, l.SoloDrags);
            Assert.AreEqual(5, l.WrongMoves);
            AssertStillPlaying(l);
        }

        [Test]
        public void SoloDragResists()
        {
            var l = new BrokenApplauseLogic();
            Assert.IsTrue(l.Grab(3, L, -H, 0f));
            Assert.AreEqual(BrokenApplauseMove.Resisting, l.Move(3, 0f, 0f));
            float got = l.X(L) + H;
            Assert.Greater(got, 0f, "si muove un po'");
            Assert.Less(got, BrokenApplauseLogic.SoloReach, "ma fa resistenza");
            Assert.AreEqual(H, l.X(R), 1e-5f);
            for (int i = 0; i < 50; i++) l.Move(3, 100f * i, 0f);
            Assert.Less(l.X(L) + H, BrokenApplauseLogic.SoloReach);
            Assert.Greater(l.Gap, BrokenApplauseLogic.ClapGap);
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, l.Release(3));
            AssertHome(l);
        }

        [Test]
        public void TwoFingersMeetingClapOnce()
        {
            var l = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(l, 0, L, 1, R, -0.2f, 0f, 0.2f, 0f));
            // niente più input
            Assert.IsFalse(l.Grab(0, L, 0f, 0f));
            Assert.AreEqual(BrokenApplauseMove.None, l.Move(0, 0f, 0f));
            Assert.AreEqual(BrokenApplauseDrop.None, l.Release(0));
            Assert.AreEqual(-1, l.TapAudience(), "il pubblico non si tocca più durante l'ovazione");
            AssertClappedOnce(l);
        }

        [Test]
        public void ClapNeedsTheSameHeight()
        {
            var l = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseMove.Following, TwoFingers(l, 0, L, 1, R, -0.3f, 0.75f, 0.3f, -0.9f));
            Assert.LessOrEqual(l.Gap, BrokenApplauseLogic.ClapGap);
            Assert.Greater(l.DeltaY, BrokenApplauseLogic.ClapYTol, "una sopra e una sotto: si mancano");
            AssertStillPlaying(l);
            // si allineano: battito
            BrokenApplauseMove m = BrokenApplauseMove.None;
            for (int i = 1; i <= 10 && m != BrokenApplauseMove.Clap; i++) m = l.Move(1, 0.3f, -0.9f + 1.65f * i / 10f);
            Assert.AreEqual(BrokenApplauseMove.Clap, m);
            AssertClappedOnce(l);
        }

        [Test]
        public void OrderOfContactsDoesNotMatter()
        {
            var a = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(a, 0, L, 1, R, -0.3f, 0.1f, 0.3f, 0.1f));
            AssertClappedOnce(a);
            var b = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(b, 0, R, 1, L, 0.3f, 0.1f, -0.3f, 0.1f));
            AssertClappedOnce(b);
            var c = new BrokenApplauseLogic();   // id delle dita qualsiasi (mouse -1, dito fittizio dell'Editor 900)
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(c, 900, R, -1, L, 0.2f, -0.2f, -0.2f, -0.2f));
            AssertClappedOnce(c);
            // anche lontano dal centro: le mani si incontrano dove vogliono le dita
            var d = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(d, 4, L, 7, R, 0.3f, 0.5f, 1.1f, 0.5f));
            AssertClappedOnce(d);
        }

        [Test]
        public void SecondFingerAfterASoloDragStillWorksWithoutJumps()
        {
            var l = new BrokenApplauseLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            l.Move(0, -0.5f, 0f);   // fa resistenza
            float xl = l.X(L), xr = l.X(R);
            Assert.IsTrue(l.Grab(1, R, xr, 0f));   // il secondo dito prende l'altra dove si trova
            Assert.AreEqual(xl, l.X(L), 1e-5f, "nessun salto");
            Assert.AreEqual(xr, l.X(R), 1e-5f);
            Assert.IsTrue(l.Together);
            BrokenApplauseMove m = BrokenApplauseMove.None;
            for (int i = 1; i <= 20 && m != BrokenApplauseMove.Clap; i++)
            {
                m = l.Move(0, -0.5f + 0.8f * i / 20f, 0f);
                if (m == BrokenApplauseMove.Clap) break;
                m = l.Move(1, xr - (xr - 0.3f) * i / 20f, 0f);
            }
            Assert.AreEqual(BrokenApplauseMove.Clap, m);
            AssertClappedOnce(l);
        }

        [Test]
        public void OneFingerLiftedCancels()
        {
            var l = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseMove.Following, TwoFingers(l, 0, L, 1, R, -0.7f, 0f, 0.7f, 0f));
            Assert.IsTrue(l.Near, "vicine: fremono");
            Assert.Less(l.Apart, 0.4f);
            Assert.AreEqual(BrokenApplauseDrop.Separated, l.Release(1));
            Assert.AreEqual(H, l.X(R), 1e-5f, "quella lasciata torna a casa");
            Assert.Less(l.X(L) + H, BrokenApplauseLogic.SoloReach, "quella tenuta torna a fare resistenza");
            Assert.IsFalse(l.Together || l.Near);
            // il dito rimasto porta la sua mano fin sopra l'altra: niente battito
            Assert.AreEqual(BrokenApplauseMove.Resisting, l.Move(0, 1f, 0f));
            Assert.AreEqual(BrokenApplauseMove.Resisting, l.Move(0, H, 0f));
            AssertStillPlaying(l);
            // si riprende l'altra: di nuovo insieme, e ora il battito arriva
            float xr0 = l.X(R), xl0 = l.X(L);
            Assert.IsTrue(l.Grab(1, R, xr0, 0f));
            Assert.AreEqual(xl0, l.X(L), 1e-5f, "nessun salto");
            BrokenApplauseMove m = BrokenApplauseMove.None;
            for (int i = 1; i <= 20 && m != BrokenApplauseMove.Clap; i++) m = l.Move(1, xr0 + (-0.4f - xr0) * i / 20f, 0f);
            Assert.AreEqual(BrokenApplauseMove.Clap, m);
            Assert.AreEqual(1, l.Separations);
            AssertClappedOnce(l);
        }

        [Test]
        public void EarlyReleaseDuringATwoFingerDragNeverWins()
        {
            var l = new BrokenApplauseLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            Assert.IsTrue(l.Grab(1, R, H, 0f));
            l.Move(0, -0.6f, 0f);   // la sinistra è già quasi al centro, la destra no
            l.Move(1, 1.0f, 0f);
            Assert.AreEqual(BrokenApplauseStage.Playing, l.Stage);
            Assert.AreEqual(BrokenApplauseDrop.Separated, l.Release(1));
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, l.Release(0));
            AssertHome(l);
            AssertStillPlaying(l);
        }

        [Test]
        public void SameFingerCannotHoldBoth()
        {
            var l = new BrokenApplauseLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            Assert.IsFalse(l.Grab(0, R, H, 0f), "lo stesso dito non prende anche l'altra");
            Assert.AreEqual(1, l.RefusedGrabs);
            Assert.AreEqual(BrokenApplauseLogic.NoFinger, l.HolderOf(R));
            Assert.IsFalse(l.Together);
            for (int i = 0; i <= 30; i++) Assert.AreNotEqual(BrokenApplauseMove.Clap, l.Move(0, -H + 3f * i / 30f, 0f));
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, l.Release(0));
            AssertStillPlaying(l);
        }

        [Test]
        public void AHandHeldByOneFingerCannotBeTakenByAnother()
        {
            var l = new BrokenApplauseLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            Assert.IsFalse(l.Grab(1, L, -H, 0f));
            Assert.AreEqual(0, l.HolderOf(L));
            Assert.AreEqual(-1, l.HeldBy(1));
            Assert.IsTrue(l.Grab(1, R, H, 0f));
            Assert.IsFalse(l.Grab(2, L, -H, 0f), "terzo dito: niente");
            Assert.IsFalse(l.Grab(2, R, H, 0f));
            Assert.AreEqual(BrokenApplauseMove.None, l.Move(2, 0f, 0f));
            Assert.AreEqual(BrokenApplauseDrop.None, l.Release(2));
            Assert.AreEqual(2, l.HeldCount);
        }

        [Test]
        public void ClampedInBoundsWhateverTheFingersDo()
        {
            var l = new BrokenApplauseLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            Assert.IsTrue(l.Grab(1, R, H, 0f));
            float[] xs = { -99f, 99f, float.NaN, -3f, 3f, float.PositiveInfinity, -2.5f };
            float[] ys = { 50f, -50f, 0f, float.NaN, 3f, -3f, 9f };
            for (int i = 0; i < xs.Length; i++)
            {
                l.Move(0, xs[i], ys[i]);
                l.Move(1, -xs[i], -ys[i]);
                AssertInBounds(l);
            }
            if (l.Stage != BrokenApplauseStage.Playing) l.Reset();   // due dita che si incrociano possono anche battere: è lecito
            l.Release(0);
            l.Release(1);
            AssertHome(l);
            Assert.IsTrue(l.Grab(0, R, H, 0f));
            l.Move(0, 50f, 50f);
            AssertInBounds(l);
            Assert.AreEqual(BrokenApplauseLogic.MaxX, l.X(R), 1e-5f);
            Assert.LessOrEqual(l.Y(R), BrokenApplauseLogic.SoloLift);
            l.Release(0);
            AssertHome(l);
        }

        [Test]
        public void PatsAndRapidTouchesAreHarmless()
        {
            var l = new BrokenApplauseLogic();
            for (int i = 0; i < 40; i++)
            {
                int who = i % 2;
                Assert.IsTrue(l.Grab(i, who, l.X(who), 0f));
                l.Move(i, l.X(who) + 0.05f, 0.05f);   // sotto la soglia del tocco
                Assert.AreEqual(BrokenApplauseDrop.Pat, l.Release(i));
                Assert.AreEqual(BrokenApplauseDrop.None, l.Release(i), "rilascio doppio: niente");
            }
            Assert.AreEqual(40, l.Pats);
            Assert.AreEqual(0, l.WrongMoves);
            AssertHome(l);
            AssertStillPlaying(l);
            for (int i = 0; i < 30; i++)
            {
                l.Grab(100 + i, L, -H, 0f);
                l.Grab(200 + i, R, H, 0f);
                l.Move(100 + i, -1f, 0f);
                l.Release(200 + i);
                l.Release(100 + i);
            }
            AssertHome(l);
            AssertStillPlaying(l);
        }

        [Test]
        public void AudienceTapsOnlyYawn()
        {
            var l = new BrokenApplauseLogic();
            for (int i = 0; i < 25; i++) Assert.AreEqual(i, l.TapAudience());
            Assert.AreEqual(25, l.AudienceTaps);
            Assert.AreEqual(0, l.WrongMoves);
            AssertHome(l);
            AssertStillPlaying(l);
            // tocchi al pubblico mentre si tiene una mano: non cambiano niente
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            l.TapAudience();
            Assert.IsTrue(l.IsHeld(L));
            Assert.AreEqual(BrokenApplauseDrop.Pat, l.Release(0));
        }

        [Test]
        public void ThreeWrongAttemptsThenTheRightOne()
        {
            var l = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, SoloDrag(l, 0, L, 0f, 0f));
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, SoloDrag(l, 0, R, 0f, 0f));
            l.TapAudience();
            Assert.AreEqual(BrokenApplauseMove.Following, TwoFingers(l, 0, L, 1, R, -0.8f, 0f, 0.8f, 0f));
            Assert.AreEqual(BrokenApplauseDrop.Separated, l.Release(0));
            Assert.AreEqual(BrokenApplauseDrop.SprangBack, l.Release(1));
            Assert.AreEqual(4, l.WrongMoves);
            AssertHome(l);
            AssertStillPlaying(l);
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(l, 0, L, 1, R, -0.1f, 0f, 0.1f, 0f));
            AssertClappedOnce(l);
        }

        [Test]
        public void CancelLetsEverythingGoWithoutCounting()
        {
            var l = new BrokenApplauseLogic();
            Assert.AreEqual(BrokenApplauseMove.Following, TwoFingers(l, 0, L, 1, R, -0.8f, 0f, 0.8f, 0f));
            Assert.IsTrue(l.Cancel());
            AssertHome(l);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.IsFalse(l.Cancel());
            Assert.AreEqual(BrokenApplauseDrop.None, l.Release(0), "le dita perse non contano più");
            AssertStillPlaying(l);
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(l, 5, R, 6, L, 0.2f, 0f, -0.2f, 0f));
            // Cancel durante l'ovazione non rovina niente
            l.Cancel();
            Assert.AreEqual(BrokenApplauseStage.Ovation, l.Stage);
            AssertClappedOnce(l);
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = new BrokenApplauseLogic();
            SoloDrag(l, 0, L, 0f, 0f);
            l.TapAudience();
            Assert.IsTrue(l.Grab(1, R, H, 0f));
            l.Reset();
            AssertHome(l);
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.SoloDrags + l.Separations + l.Pats + l.RefusedGrabs + l.AudienceTaps);
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(l, 0, L, 1, R, -0.2f, 0f, 0.2f, 0f));
            l.Reset();   // uscita a metà ovazione e nuovo ingresso
            AssertStillPlaying(l);
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(l, 0, L, 1, R, -0.2f, 0f, 0.2f, 0f));
            AssertClappedOnce(l);
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(BrokenApplauseMove.Clap, TwoFingers(l, 0, L, 1, R, -0.2f, 0f, 0.2f, 0f));
            AssertClappedOnce(l);
        }

        [Test]
        public void OneFingerNeverClapsEvenWithManyFingerSwaps()
        {
            var l = new BrokenApplauseLogic();
            var rnd = new System.Random(138);
            for (int i = 0; i < 2000; i++)
            {
                int who = rnd.Next(2);
                if (l.HeldCount == 0) l.Grab(i, who, l.X(who), l.Y(who));
                int finger = l.HolderOf(L) != BrokenApplauseLogic.NoFinger ? l.HolderOf(L) : l.HolderOf(R);
                BrokenApplauseMove m = l.Move(finger, (float)(rnd.NextDouble() * 8 - 4), (float)(rnd.NextDouble() * 8 - 4));
                Assert.AreNotEqual(BrokenApplauseMove.Clap, m);
                Assert.Greater(l.Gap, BrokenApplauseLogic.ClapGap);
                AssertInBounds(l);
                if (rnd.Next(4) == 0) l.Release(finger);
                if (rnd.Next(7) == 0) l.TapAudience();
            }
            AssertStillPlaying(l);
        }
    }
}
