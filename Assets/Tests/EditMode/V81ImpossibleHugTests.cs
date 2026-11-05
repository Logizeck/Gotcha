using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 38 della roadmap: The Impossible Hug (due dita, una per amico, trascinati insieme al centro).</summary>
    public class V81ImpossibleHugTests
    {
        const int L = ImpossibleHugLogic.Left, R = ImpossibleHugLogic.Right;
        const float H = ImpossibleHugLogic.HomeX;

        static void AssertStillPlaying(ImpossibleHugLogic l)
        {
            Assert.AreEqual(ImpossibleHugStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static void AssertHome(ImpossibleHugLogic l)
        {
            Assert.AreEqual(-H, l.X(L), 1e-5f);
            Assert.AreEqual(H, l.X(R), 1e-5f);
            Assert.AreEqual(0f, l.Y(L), 1e-5f);
            Assert.AreEqual(0f, l.Y(R), 1e-5f);
            Assert.AreEqual(0, l.HeldCount);
        }

        static void AssertInBounds(ImpossibleHugLogic l)
        {
            foreach (int w in new[] { L, R })
            {
                Assert.LessOrEqual(System.Math.Abs(l.X(w)), ImpossibleHugLogic.MaxX + 1e-5f);
                Assert.GreaterOrEqual(l.Y(w), ImpossibleHugLogic.MinY - 1e-5f);
                Assert.LessOrEqual(l.Y(w), ImpossibleHugLogic.MaxY + 1e-5f);
                Assert.IsFalse(float.IsNaN(l.X(w)) || float.IsNaN(l.Y(w)));
            }
        }

        /// <summary>Un dito solo trascina un amico da (fx0, 0) a (fx1, fy1) a passi, poi si alza.</summary>
        static ImpossibleHugDrop SoloDrag(ImpossibleHugLogic l, int finger, int who, float fx1, float fy1, int steps = 12)
        {
            float fx0 = l.X(who), fy0 = l.Y(who);
            Assert.IsTrue(l.Grab(finger, who, fx0, fy0));
            for (int i = 1; i <= steps; i++)
            {
                ImpossibleHugMove m = l.Move(finger, fx0 + (fx1 - fx0) * i / steps, fy0 + (fy1 - fy0) * i / steps);
                Assert.AreEqual(ImpossibleHugMove.Resisting, m);
                Assert.Greater(l.Dist(who), ImpossibleHugLogic.HugRadius, "da solo non arriva mai al centro");
                Assert.Greater(l.Dist(1 - who), ImpossibleHugLogic.HugRadius);
                AssertInBounds(l);
            }
            return l.Release(finger);
        }

        /// <summary>Due dita (a, b) prendono i due amici, nell'ordine dato, e li portano a (lx, ly) e (rx, ry).</summary>
        static ImpossibleHugMove TwoFingers(ImpossibleHugLogic l, int fa, int whoA, int fb, int whoB, float ax, float ay, float bx, float by, int steps = 10)
        {
            float a0x = l.X(whoA), a0y = l.Y(whoA);
            Assert.IsTrue(l.Grab(fa, whoA, a0x, a0y));
            float b0x = l.X(whoB), b0y = l.Y(whoB);
            Assert.IsTrue(l.Grab(fb, whoB, b0x, b0y));
            Assert.IsTrue(l.Together);
            ImpossibleHugMove last = ImpossibleHugMove.None;
            for (int i = 1; i <= steps; i++)
            {
                float k = i / (float)steps;
                last = l.Move(fa, a0x + (ax - a0x) * k, a0y + (ay - a0y) * k);
                if (last == ImpossibleHugMove.Hug) return last;
                last = l.Move(fb, b0x + (bx - b0x) * k, b0y + (by - b0y) * k);
                if (last == ImpossibleHugMove.Hug) return last;
                AssertInBounds(l);
            }
            return last;
        }

        [Test]
        public void StartsApartAndAngry()
        {
            var l = new ImpossibleHugLogic();
            AssertStillPlaying(l);
            AssertHome(l);
            Assert.IsFalse(l.Together || l.Shy);
            Assert.AreEqual(1f, l.Apart, 1e-4f);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void SingleFingerDragsNeverWinAndBothGoBack()
        {
            var l = new ImpossibleHugLogic();
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, SoloDrag(l, 0, L, 0f, 0f));      // sinistro verso il centro
            AssertHome(l);
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, SoloDrag(l, 0, R, 0f, 0f));      // destro verso il centro
            AssertHome(l);
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, SoloDrag(l, 5, L, 3f, 0.5f));    // oltre, fino all'altro lato
            AssertHome(l);
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, SoloDrag(l, 5, R, -40f, -9f));   // lontanissimo
            AssertHome(l);
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, SoloDrag(l, 1, L, -5f, 4f));     // all'indietro: resta nello schermo
            AssertHome(l);
            Assert.AreEqual(5, l.SoloDrags);
            Assert.AreEqual(5, l.WrongMoves);
            AssertStillPlaying(l);
        }

        [Test]
        public void SoloDragResistsAndTheOtherBacksAway()
        {
            var l = new ImpossibleHugLogic();
            Assert.IsTrue(l.Grab(3, L, -H, 0f));
            Assert.AreEqual(ImpossibleHugMove.Resisting, l.Move(3, 0f, 0f));
            float gotL = l.X(L) + H;
            Assert.Greater(gotL, 0f, "si muove un po'");
            Assert.Less(gotL, ImpossibleHugLogic.SoloReach, "ma punta i piedi");
            Assert.Greater(l.X(R), H, "l'altro si allontana");
            Assert.LessOrEqual(l.X(R), ImpossibleHugLogic.MaxX + 1e-5f);
            // anche tirando sempre di più non arriva
            for (int i = 0; i < 50; i++) l.Move(3, 100f * i, 0f);
            Assert.Less(l.X(L) + H, ImpossibleHugLogic.SoloReach);
            Assert.Greater(l.Dist(L), ImpossibleHugLogic.HugRadius);
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, l.Release(3));
            AssertHome(l);
        }

        [Test]
        public void TwoFingersInTheCentreHugOnce()
        {
            var l = new ImpossibleHugLogic();
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(l, 0, L, 1, R, -0.2f, 0f, 0.2f, 0f));
            Assert.AreEqual(ImpossibleHugStage.Hugging, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(0, l.HeldCount, "durante la festa nessuno è più tenuto");
            // niente più input
            Assert.IsFalse(l.Grab(0, L, 0f, 0f));
            Assert.AreEqual(ImpossibleHugMove.None, l.Move(0, 0f, 0f));
            Assert.AreEqual(ImpossibleHugDrop.None, l.Release(0));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(ImpossibleHugStage.Done, l.Stage);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void OrderOfContactsDoesNotMatter()
        {
            var a = new ImpossibleHugLogic();
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(a, 0, L, 1, R, -0.3f, 0.1f, 0.3f, 0.1f));
            var b = new ImpossibleHugLogic();
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(b, 0, R, 1, L, 0.3f, 0.1f, -0.3f, 0.1f));
            var c = new ImpossibleHugLogic();   // id delle dita qualsiasi (mouse -1, dito fittizio dell'Editor 900)
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(c, 900, R, -1, L, 0.2f, -0.2f, -0.2f, -0.2f));
            // anche "incrociati": ognuno dalla parte dell'altro, purché vicini al centro
            var d = new ImpossibleHugLogic();
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(d, 4, L, 7, R, 0.4f, 0f, -0.4f, 0f));
        }

        [Test]
        public void SecondFingerAfterASoloDragStillWorksWithoutJumps()
        {
            var l = new ImpossibleHugLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            l.Move(0, -0.5f, 0f);   // punta i piedi
            float xl = l.X(L), xr = l.X(R);
            Assert.IsTrue(l.Grab(1, R, xr, 0f));   // il secondo dito prende l'altro dove si trova
            Assert.AreEqual(xl, l.X(L), 1e-5f, "nessun salto");
            Assert.AreEqual(xr, l.X(R), 1e-5f);
            Assert.IsTrue(l.Together);
            // ora seguono le dita: si portano al centro
            ImpossibleHugMove m = ImpossibleHugMove.None;
            for (int i = 1; i <= 20 && m != ImpossibleHugMove.Hug; i++)
            {
                m = l.Move(0, -0.5f + 0.8f * i / 20f, 0f);
                if (m == ImpossibleHugMove.Hug) break;
                m = l.Move(1, xr - (xr - 0.3f) * i / 20f, 0f);
            }
            Assert.AreEqual(ImpossibleHugMove.Hug, m);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void OneFingerLiftedCancelsTheHug()
        {
            var l = new ImpossibleHugLogic();
            Assert.AreEqual(ImpossibleHugMove.Following, TwoFingers(l, 0, L, 1, R, -0.9f, 0f, 0.9f, 0f));
            Assert.IsTrue(l.Shy, "vicini: si guardano");
            Assert.Less(l.Apart, 0.4f);
            Assert.AreEqual(ImpossibleHugDrop.Separated, l.Release(1));
            Assert.GreaterOrEqual(l.X(R), H - 1e-5f, "quello lasciato torna indietro (anche un po' oltre: l'altro tira ancora)");
            Assert.Greater(l.Dist(L), ImpossibleHugLogic.HugRadius, "quello tenuto torna a puntare i piedi");
            Assert.IsFalse(l.Together || l.Shy);
            // il dito rimasto porta il suo amico in pieno centro: niente abbraccio
            Assert.AreEqual(ImpossibleHugMove.Resisting, l.Move(0, 0f, 0f));
            Assert.AreEqual(ImpossibleHugMove.Resisting, l.Move(0, 0.1f, 0f));
            AssertStillPlaying(l);
            // si riprende l'altro: di nuovo insieme, e ora l'abbraccio arriva
            float xr0 = l.X(R), xl0 = l.X(L);
            Assert.IsTrue(l.Grab(1, R, xr0, 0f));
            Assert.AreEqual(xl0, l.X(L), 1e-5f, "nessun salto");
            ImpossibleHugMove m = ImpossibleHugMove.None;
            for (int i = 1; i <= 20 && m != ImpossibleHugMove.Hug; i++)
            {
                m = l.Move(1, xr0 + (0.2f - xr0) * i / 20f, 0f);
                if (m == ImpossibleHugMove.Hug) break;
                m = l.Move(0, 0.1f + (-0.2f - xl0) * i / 20f, 0f);   // il sinistro va da xl0 a -0.2
            }
            Assert.AreEqual(ImpossibleHugMove.Hug, m);
            Assert.AreEqual(1, l.Separations);
        }

        [Test]
        public void LiftingWhileBothInTheZoneButBeforeTheSecondArrivesNeverWins()
        {
            var l = new ImpossibleHugLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            Assert.IsTrue(l.Grab(1, R, H, 0f));
            l.Move(0, -0.2f, 0f);   // il sinistro è già nella zona, il destro no
            Assert.AreEqual(ImpossibleHugStage.Playing, l.Stage);
            Assert.AreEqual(ImpossibleHugDrop.Separated, l.Release(1));
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, l.Release(0));
            AssertHome(l);
            AssertStillPlaying(l);
        }

        [Test]
        public void SameFingerCannotHoldBoth()
        {
            var l = new ImpossibleHugLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            Assert.IsFalse(l.Grab(0, R, H, 0f), "lo stesso dito non prende anche l'altro");
            Assert.AreEqual(1, l.RefusedGrabs);
            Assert.AreEqual(0, l.HolderOf(R) == ImpossibleHugLogic.NoFinger ? 0 : 1);
            Assert.IsFalse(l.Together);
            // e spostandolo ovunque non vince
            for (int i = 0; i <= 30; i++) Assert.AreNotEqual(ImpossibleHugMove.Hug, l.Move(0, -H + 3f * i / 30f, 0f));
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, l.Release(0));
            AssertStillPlaying(l);
        }

        [Test]
        public void AFriendHeldByOneFingerCannotBeTakenByAnother()
        {
            var l = new ImpossibleHugLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            Assert.IsFalse(l.Grab(1, L, -H, 0f));
            Assert.AreEqual(0, l.HolderOf(L));
            Assert.AreEqual(-1, l.HeldBy(1));
            Assert.IsTrue(l.Grab(1, R, H, 0f));
            Assert.IsFalse(l.Grab(2, L, -H, 0f), "terza dita: niente");
            Assert.IsFalse(l.Grab(2, R, H, 0f));
            Assert.AreEqual(ImpossibleHugMove.None, l.Move(2, 0f, 0f));
            Assert.AreEqual(ImpossibleHugDrop.None, l.Release(2));
            Assert.AreEqual(2, l.HeldCount);
        }

        [Test]
        public void ClampedInBoundsWhateverTheFingersDo()
        {
            var l = new ImpossibleHugLogic();
            Assert.IsTrue(l.Grab(0, L, -H, 0f));
            Assert.IsTrue(l.Grab(1, R, H, 0f));
            float[] xs = { -99f, 99f, float.NaN, -3f, 3f, float.PositiveInfinity, 0.6f };
            float[] ys = { 50f, -50f, 0f, float.NaN, 3f, -3f, 9f };
            for (int i = 0; i < xs.Length; i++)
            {
                l.Move(0, xs[i], ys[i]);
                l.Move(1, -xs[i], -ys[i]);
                AssertInBounds(l);
            }
            l.Release(0);
            l.Release(1);
            AssertHome(l);
            // da solo, all'indietro e in alto: dentro i limiti
            Assert.IsTrue(l.Grab(0, R, H, 0f));
            l.Move(0, 50f, 50f);
            AssertInBounds(l);
            Assert.AreEqual(ImpossibleHugLogic.MaxX, l.X(R), 1e-5f);
            Assert.LessOrEqual(l.Y(R), ImpossibleHugLogic.SoloLift);
            l.Release(0);
            AssertHome(l);
        }

        [Test]
        public void TapsAndRapidTouchesAreHarmless()
        {
            var l = new ImpossibleHugLogic();
            for (int i = 0; i < 40; i++)
            {
                int who = i % 2;
                Assert.IsTrue(l.Grab(i, who, l.X(who), 0f));
                l.Move(i, l.X(who) + 0.05f, 0.05f);   // sotto la soglia del tocco
                Assert.AreEqual(ImpossibleHugDrop.Tap, l.Release(i));
                Assert.AreEqual(ImpossibleHugDrop.None, l.Release(i), "rilascio doppio: niente");
            }
            Assert.AreEqual(40, l.Taps);
            Assert.AreEqual(0, l.WrongMoves);
            AssertHome(l);
            AssertStillPlaying(l);
            // due dita che entrano ed escono velocissime
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
        public void ThreeWrongAttemptsThenTheRightOne()
        {
            var l = new ImpossibleHugLogic();
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, SoloDrag(l, 0, L, 0f, 0f));
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, SoloDrag(l, 0, R, 0f, 0f));
            Assert.AreEqual(ImpossibleHugMove.Following, TwoFingers(l, 0, L, 1, R, -1f, 0f, 1f, 0f));
            Assert.AreEqual(ImpossibleHugDrop.Separated, l.Release(0));
            Assert.AreEqual(ImpossibleHugDrop.SteppedBack, l.Release(1));
            Assert.AreEqual(4, l.WrongMoves);
            AssertHome(l);
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(l, 0, L, 1, R, -0.1f, 0f, 0.1f, 0f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void CancelLetsEverythingGoWithoutCounting()
        {
            var l = new ImpossibleHugLogic();
            Assert.AreEqual(ImpossibleHugMove.Following, TwoFingers(l, 0, L, 1, R, -0.9f, 0f, 0.9f, 0f));
            Assert.IsTrue(l.Cancel());
            AssertHome(l);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.IsFalse(l.Cancel());
            Assert.AreEqual(ImpossibleHugDrop.None, l.Release(0), "le dita perse non contano più");
            AssertStillPlaying(l);
            // dopo la pausa si gioca normalmente
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(l, 5, R, 6, L, 0.2f, 0f, -0.2f, 0f));
            // Cancel durante la festa non rovina niente
            l.Cancel();
            Assert.AreEqual(ImpossibleHugStage.Hugging, l.Stage);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = new ImpossibleHugLogic();
            SoloDrag(l, 0, L, 0f, 0f);
            Assert.IsTrue(l.Grab(1, R, H, 0f));
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(new ImpossibleHugLogic(), 0, L, 1, R, 0f, 0f, 0f, 0f));
            l.Reset();
            AssertHome(l);
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.SoloDrags + l.Separations + l.Taps + l.RefusedGrabs);
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(l, 0, L, 1, R, -0.2f, 0f, 0.2f, 0f));
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(ImpossibleHugMove.Hug, TwoFingers(l, 0, L, 1, R, -0.2f, 0f, 0.2f, 0f));
            Assert.IsTrue(l.TryComplete(), "dopo un nuovo ingresso nel livello si vince di nuovo (una volta)");
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SoloNeverReachesTheZoneEvenWithManyFingerSwaps()
        {
            var l = new ImpossibleHugLogic();
            var rnd = new System.Random(38);
            for (int i = 0; i < 2000; i++)
            {
                int who = rnd.Next(2);
                if (l.HeldCount == 0) l.Grab(i, who, l.X(who), l.Y(who));
                int finger = l.HolderOf(L) != ImpossibleHugLogic.NoFinger ? l.HolderOf(L) : l.HolderOf(R);
                ImpossibleHugMove m = l.Move(finger, (float)(rnd.NextDouble() * 8 - 4), (float)(rnd.NextDouble() * 8 - 4));
                Assert.AreNotEqual(ImpossibleHugMove.Hug, m);
                Assert.Greater(l.Dist(L), ImpossibleHugLogic.HugRadius);
                Assert.Greater(l.Dist(R), ImpossibleHugLogic.HugRadius);
                AssertInBounds(l);
                if (rnd.Next(4) == 0) l.Release(finger);
            }
            AssertStillPlaying(l);
        }
    }
}
