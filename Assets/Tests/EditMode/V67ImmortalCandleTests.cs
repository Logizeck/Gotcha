using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 262: la candela immortale (soffio, ventilatore, bicchiere, solletico, presa a due dita).</summary>
    public class V67ImmortalCandleTests
    {
        const float Dt = 1f / 60f;
        const float HX = -0.3f, HY = 0.5f;   // centro della fiamma sullo stoppino, come nel livello

        static ImmortalCandleLogic New() => new ImmortalCandleLogic(HX, HY);

        static CandleEvent Ticks(ImmortalCandleLogic l, float seconds)
        {
            CandleEvent last = CandleEvent.None;
            for (float t = 0f; t < seconds; t += Dt)
            {
                CandleEvent e = l.Tick(Dt);
                if (e != CandleEvent.None) last = e;
            }
            return last;
        }

        static CandleEvent Blow(ImmortalCandleLogic l, float seconds, bool loud, ref int grows, ref int taunts)
        {
            CandleEvent last = CandleEvent.None;
            for (float t = 0f; t < seconds; t += Dt)
            {
                CandleEvent e = l.BlowFeed(loud, Dt);
                if (e == CandleEvent.BlowGrow) grows++;
                if (e == CandleEvent.BlowTaunt) taunts++;
                if (e != CandleEvent.None) last = e;
                l.Tick(Dt);
            }
            return last;
        }

        /// <summary>Due dita (id 1 e 2) che salgono insieme a passetti; restituisce l'ultimo evento.</summary>
        static CandleEvent Lift(ImmortalCandleLogic l, float ax, float ay, float bx, float by, float dy, int steps = 20)
        {
            CandleEvent last = CandleEvent.None;
            for (int i = 1; i <= steps; i++)
            {
                float k = dy * i / steps;
                CandleEvent e = l.FingerMove(1, ax, ay + k);
                if (e != CandleEvent.None) last = e;
                e = l.FingerMove(2, bx, by + k);
                if (e != CandleEvent.None) last = e;
                if (l.InputLocked) break;
            }
            return last;
        }

        static void GrabAndDetach(ImmortalCandleLogic l)
        {
            Assert.AreEqual(CandleEvent.Tickled, l.FingerDown(1, HX - 0.2f, HY));
            Assert.AreEqual(CandleEvent.Grabbed, l.FingerDown(2, HX + 0.2f, HY));
            Assert.AreEqual(CandleEvent.Detached, Lift(l, HX - 0.2f, HY, HX + 0.2f, HY, 0.8f));
            Assert.AreEqual(CandleState.Carried, l.State);
            Assert.IsFalse(l.CandleLit);
        }

        // ---------- Soluzione ----------

        [Test]
        public void TwoFingers_LiftDetach_CarryToTop_WinsOnce()
        {
            var l = New();
            Assert.AreEqual(CandleState.Lit, l.State);
            Assert.IsTrue(l.CandleLit);
            GrabAndDetach(l);
            Assert.AreEqual(CandleEvent.Escaped, Lift(l, HX - 0.2f, HY + 0.8f, HX + 0.2f, HY + 0.8f, 3f, 40));
            Assert.AreEqual(CandleState.Escaping, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(CandleState.Done, l.State);
        }

        [Test]
        public void TwoFingers_EitherOrder_Grabs()
        {
            foreach (int first in new[] { 1, 2 })
            {
                var l = New();
                int second = first == 1 ? 2 : 1;
                Assert.AreEqual(CandleEvent.Tickled, l.FingerDown(first, HX + 0.3f, HY - 0.2f));
                Assert.AreEqual(CandleEvent.Grabbed, l.FingerDown(second, HX - 0.3f, HY + 0.2f));
                Assert.AreEqual(CandleState.Grabbed, l.State);
                Assert.AreEqual(CandleEvent.Detached, Lift(l, HX - 0.3f, HY, HX + 0.3f, HY, 0.9f));
            }
        }

        [Test]
        public void FlameFollowsFingers_AfterDetach()
        {
            var l = New();
            GrabAndDetach(l);
            float y0 = l.FlameY;
            l.FingerMove(1, HX + 0.3f, HY + 1.3f);
            l.FingerMove(2, HX + 0.7f, HY + 1.3f);
            Assert.AreEqual(CandleState.Carried, l.State);
            Assert.Greater(l.FlameY, y0 + 0.4f);
            Assert.Greater(l.FlameX, HX + 0.4f);
        }

        // ---------- Un dito solo ----------

        [Test]
        public void OneFinger_NeverDetaches_EvenDraggedFarUp()
        {
            var l = New();
            Assert.AreEqual(CandleEvent.Tickled, l.FingerDown(7, HX, HY));
            for (int i = 0; i < 60; i++) Assert.AreEqual(CandleEvent.None, l.FingerMove(7, HX, HY + i * 0.1f));
            Assert.AreEqual(CandleState.Lit, l.State);
            Assert.AreEqual(HY, l.FlameY, 1e-4f);
            Assert.AreEqual(CandleEvent.None, l.FingerUp(7));
            Assert.AreEqual(1, l.Tickles);
            Assert.AreEqual(CandleEvent.Tickled, l.FingerDown(7, HX, HY));
            Assert.AreEqual(2, l.Tickles);
        }

        [Test]
        public void FingersOutsideFlame_AreNotTaken()
        {
            var l = New();
            Assert.AreEqual(CandleEvent.None, l.FingerDown(1, HX + 2f, HY));
            Assert.AreEqual(CandleEvent.None, l.FingerDown(2, HX, HY - 2f));
            Assert.IsFalse(l.IsFlameFinger(1));
            Assert.AreEqual(0, l.FlameFingerCount);
            Assert.AreEqual(CandleState.Lit, l.State);
        }

        [Test]
        public void ThirdFinger_Ignored()
        {
            var l = New();
            l.FingerDown(1, HX, HY);
            l.FingerDown(2, HX, HY);
            Assert.AreEqual(CandleEvent.None, l.FingerDown(3, HX, HY));
            Assert.AreEqual(2, l.FlameFingerCount);
        }

        // ---------- Interruzioni ----------

        [Test]
        public void ReleasingOneFinger_BeforeDetach_SnapsBack_AndCanRegrab()
        {
            var l = New();
            l.FingerDown(1, HX, HY);
            l.FingerDown(2, HX + 0.2f, HY);
            Lift(l, HX, HY, HX + 0.2f, HY, 0.4f);   // a metà
            Assert.AreEqual(CandleState.Grabbed, l.State);
            Assert.Greater(l.Pull, 0.5f);
            Assert.AreEqual(CandleEvent.SnapBack, l.FingerUp(2));
            Assert.AreEqual(CandleState.Lit, l.State);
            Assert.IsTrue(l.CandleLit);
            Assert.AreEqual(HY, l.FlameY, 1e-4f);
            Assert.AreEqual(0f, l.Pull);
            // il dito rimasto + uno nuovo: nuova presa che parte da dove sono adesso
            Assert.AreEqual(CandleEvent.Grabbed, l.FingerDown(3, HX + 0.1f, HY + 0.2f));
            Assert.AreEqual(CandleEvent.Detached, Lift2(l, 1, HX, HY + 0.4f, 3, HX + 0.1f, HY + 0.2f, 0.7f));
        }

        static CandleEvent Lift2(ImmortalCandleLogic l, int a, float ax, float ay, int b, float bx, float by, float dy)
        {
            CandleEvent last = CandleEvent.None;
            for (int i = 1; i <= 20; i++)
            {
                float k = dy * i / 20f;
                CandleEvent e = l.FingerMove(a, ax, ay + k);
                if (e != CandleEvent.None) last = e;
                e = l.FingerMove(b, bx, by + k);
                if (e != CandleEvent.None) last = e;
            }
            return last;
        }

        [Test]
        public void IncompleteDrag_ReleasedBoth_BackToLit()
        {
            var l = New();
            l.FingerDown(1, HX, HY);
            l.FingerDown(2, HX, HY);
            Lift(l, HX, HY, HX, HY, 0.3f);
            l.FingerUp(1);
            l.FingerUp(2);
            Assert.AreEqual(CandleState.Lit, l.State);
            Assert.AreEqual(1, l.SnapBacks);
            Assert.AreEqual(0, l.FlameFingerCount);
            // draggare in giù o di lato non stacca
            l.FingerDown(1, HX, HY);
            l.FingerDown(2, HX, HY);
            for (int i = 0; i < 20; i++) { l.FingerMove(1, HX + i * 0.1f, HY - i * 0.1f); l.FingerMove(2, HX - i * 0.1f, HY - i * 0.1f); }
            Assert.AreEqual(CandleState.Grabbed, l.State);
            Assert.AreEqual(0f, l.Pull);
        }

        [Test]
        public void ReleaseAfterDetach_Hovers_ThenFloatsBack_AndRelights()
        {
            var l = New();
            GrabAndDetach(l);
            l.FingerMove(1, 1.5f, -1f);
            l.FingerMove(2, 1.7f, -1f);
            Assert.AreEqual(CandleEvent.Dropped, l.FingerUp(1));
            Assert.AreEqual(CandleState.Hovering, l.State);
            Assert.AreEqual(CandleEvent.None, l.FingerUp(2));
            Assert.IsFalse(l.CandleLit);
            Assert.AreEqual(CandleEvent.Returning, Ticks(l, l.HoverSeconds + 0.05f));
            Assert.AreEqual(CandleEvent.Relit, Ticks(l, l.ReturnSeconds + 0.1f));
            Assert.AreEqual(CandleState.Lit, l.State);
            Assert.IsTrue(l.CandleLit);
            Assert.AreEqual(HX, l.FlameX, 1e-4f);
            Assert.AreEqual(HY, l.FlameY, 1e-4f);
            Assert.AreEqual(0, l.FlameFingerCount);
            // e si può rifare tutto
            GrabAndDetach(l);
        }

        [Test]
        public void HoveringFlame_CanBeRegrabbed_AndStillWins()
        {
            var l = New();
            GrabAndDetach(l);
            l.FingerUp(1);
            l.FingerUp(2);
            Assert.AreEqual(CandleState.Hovering, l.State);
            float fx = l.FlameX, fy = l.FlameY;
            Assert.AreEqual(CandleEvent.None, l.FingerDown(1, fx, fy));   // un dito: niente
            Assert.AreEqual(CandleEvent.Regrabbed, l.FingerDown(2, fx + 0.1f, fy));
            Assert.AreEqual(CandleState.Carried, l.State);
            Assert.AreEqual(CandleEvent.Escaped, Lift(l, fx, fy, fx + 0.1f, fy, 4f, 40));
        }

        [Test]
        public void ReturningFlame_CanBeRegrabbed()
        {
            var l = New();
            GrabAndDetach(l);
            l.CancelFingers();
            Ticks(l, l.HoverSeconds + 0.2f);
            Assert.AreEqual(CandleState.Returning, l.State);
            l.FingerDown(1, l.FlameX, l.FlameY);
            Assert.AreEqual(CandleEvent.Regrabbed, l.FingerDown(2, l.FlameX, l.FlameY));
            Assert.AreEqual(CandleState.Carried, l.State);
        }

        [Test]
        public void PauseDuringGrab_And_DuringCarry_LeavesValidState()
        {
            var l = New();
            l.FingerDown(1, HX, HY);
            l.FingerDown(2, HX, HY);
            Assert.AreEqual(CandleEvent.SnapBack, l.CancelFingers());
            Assert.AreEqual(CandleState.Lit, l.State);
            Assert.AreEqual(CandleEvent.None, l.CancelFingers());   // idempotente
            GrabAndDetach(l);
            Assert.AreEqual(CandleEvent.Dropped, l.CancelFingers());
            Assert.AreEqual(CandleState.Hovering, l.State);
            Assert.AreEqual(0, l.FlameFingerCount);
            Ticks(l, 3f);
            Assert.AreEqual(CandleState.Lit, l.State);
        }

        [Test]
        public void FingerEvents_ForUnknownIds_AreIgnored()
        {
            var l = New();
            Assert.AreEqual(CandleEvent.None, l.FingerMove(42, 0f, 5f));
            Assert.AreEqual(CandleEvent.None, l.FingerUp(42));
            Assert.AreEqual(CandleState.Lit, l.State);
        }

        // ---------- La fiamma non esce per sbaglio ----------

        [Test]
        public void CarriedFlame_IsClampedInsideScene_OnlyTopEdgeWins()
        {
            var l = New();
            GrabAndDetach(l);
            l.FingerMove(1, -9f, -9f);
            l.FingerMove(2, -9f, -9f);
            Assert.AreEqual(CandleState.Carried, l.State);
            Assert.GreaterOrEqual(l.FlameX, l.MinX);
            Assert.GreaterOrEqual(l.FlameY, l.MinY);
            l.FingerMove(1, 9f, -9f);
            l.FingerMove(2, 9f, -9f);
            Assert.LessOrEqual(l.FlameX, l.MaxX);
            Assert.AreEqual(CandleState.Carried, l.State);
            Assert.Greater(l.MaxY, l.TopY);   // la cima si raggiunge sempre
            l.FingerUp(1);
            Ticks(l, 3f);
            Assert.AreEqual(CandleState.Lit, l.State);   // torna a casa anche dall'angolo
        }

        // ---------- Mosse sbagliate ----------

        [Test]
        public void Blowing_GrowsFlame_NeverWins_TauntEveryFewAttempts()
        {
            var l = New();
            int grows = 0, taunts = 0;
            for (int i = 0; i < 6; i++)
            {
                Blow(l, 0.8f, true, ref grows, ref taunts);
                Assert.IsTrue(l.Grow > 0.5f);
                Assert.AreEqual(CandleState.Lit, l.State);
                Assert.IsTrue(l.CandleLit);
                Blow(l, 1.2f, false, ref grows, ref taunts);
                Assert.AreEqual(0f, l.Grow, 1e-3f);   // torna normale in circa un secondo
            }
            Assert.AreEqual(6, l.BlowAttempts);
            Assert.AreEqual(4, grows);
            Assert.AreEqual(2, taunts);   // al 3° e al 6° soffio, non a ogni rilevamento
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void Blowing_ShortClicks_DoNotCount()
        {
            var l = New();
            int grows = 0, taunts = 0;
            for (int i = 0; i < 20; i++)
            {
                Blow(l, 0.1f, true, ref grows, ref taunts);
                Blow(l, 0.3f, false, ref grows, ref taunts);
            }
            Assert.AreEqual(0, l.BlowAttempts);
            Assert.AreEqual(0f, l.Grow);
        }

        [Test]
        public void ContinuousLoudNoise_ReactsOnce_ThenStops()
        {
            var l = New();
            int grows = 0, taunts = 0;
            Blow(l, 10f, true, ref grows, ref taunts);
            Assert.AreEqual(1, l.BlowAttempts);
            Assert.IsFalse(l.Blowing);
            Assert.AreEqual(0f, l.Grow, 1e-3f);   // niente animazione continua
        }

        [Test]
        public void AmbientNoise_BelowCalibratedThreshold_NoReaction()
        {
            var l = New();
            var gate = new V64AMicGate();
            var rnd = new System.Random(5);
            for (int i = 0; i < 600; i++)
            {
                float level = 0.12f + (float)rnd.NextDouble() * 0.06f;   // stanza rumorosa ma costante
                l.BlowFeed(gate.Feed(level, Dt), Dt);
            }
            Assert.AreEqual(0, l.BlowAttempts);
            // un soffio vero sopra il fondo invece conta
            CandleEvent ev = CandleEvent.None;
            for (int i = 0; i < 40; i++) { CandleEvent e = l.BlowFeed(gate.Feed(0.8f, Dt), Dt); if (e != CandleEvent.None) ev = e; }
            Assert.AreEqual(CandleEvent.BlowGrow, ev);
        }

        [Test]
        public void MicDenied_NoInput_LevelStillSolvable()
        {
            var l = New();
            for (int i = 0; i < 300; i++) Assert.AreEqual(CandleEvent.None, l.BlowFeed(false, Dt));
            GrabAndDetach(l);
            Assert.AreEqual(CandleEvent.Escaped, Lift(l, HX - 0.2f, HY + 0.8f, HX + 0.2f, HY + 0.8f, 3f, 40));
        }

        [Test]
        public void Blowing_WhileCarried_DoesNothing()
        {
            var l = New();
            GrabAndDetach(l);
            int grows = 0, taunts = 0;
            Blow(l, 1f, true, ref grows, ref taunts);
            Assert.AreEqual(0, l.BlowAttempts);
            Assert.AreEqual(CandleState.Carried, l.State);
        }

        [Test]
        public void Fan_SpinsBendsAndStopsByItself_FlameStaysLit()
        {
            var l = New();
            Assert.AreEqual(CandleEvent.FanOn, l.TapFan());
            Assert.IsTrue(l.FanOn);
            Assert.IsTrue(l.FanBend);
            Ticks(l, 2f);
            Assert.AreEqual(CandleEvent.FanAgain, l.TapFan());   // ricomincia il conto
            Ticks(l, l.FanSeconds - 0.3f);
            Assert.IsTrue(l.FanOn);
            Ticks(l, 0.5f);
            Assert.IsFalse(l.FanOn);
            Assert.IsFalse(l.FanBend);
            Assert.AreEqual(CandleState.Lit, l.State);
            Assert.AreEqual(2, l.FanTaps);
        }

        [Test]
        public void Glass_PoursSteam_FlameStaysLit_GlassComesBack_Unlimited()
        {
            var l = New();
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(l.CanPour);
                Assert.AreEqual(CandleEvent.Poured, l.Pour());
                Assert.AreEqual(CandleEvent.None, l.Pour());   // già in corso
                Assert.IsTrue(l.GlassBusy);
                Ticks(l, l.PourSeconds + 0.05f);
                Assert.IsFalse(l.GlassBusy);                   // niente copre la fiamma per sempre
                Assert.AreEqual(CandleState.Lit, l.State);
            }
            Assert.AreEqual(5, l.Pours);
        }

        [Test]
        public void WrongMoves_KeepPartialProgress()
        {
            var l = New();
            GrabAndDetach(l);
            l.FingerUp(1);
            l.FingerUp(2);
            Assert.AreEqual(CandleEvent.PouredNothing, l.Pour());
            l.TapFan();
            Assert.IsFalse(l.FanBend);   // la fiamma non è sullo stoppino
            Assert.AreEqual(CandleState.Hovering, l.State);
            l.FingerDown(1, l.FlameX, l.FlameY);
            Assert.AreEqual(CandleEvent.Regrabbed, l.FingerDown(2, l.FlameX, l.FlameY));
        }

        [Test]
        public void GrabWorks_WhileFanSpinsOrGlassPours()
        {
            var l = New();
            l.TapFan();
            l.Pour();
            GrabAndDetach(l);
        }

        // ---------- Dopo la vittoria ----------

        [Test]
        public void NoRelight_AfterWin()
        {
            var l = New();
            GrabAndDetach(l);
            Lift(l, HX - 0.2f, HY + 0.8f, HX + 0.2f, HY + 0.8f, 3f, 40);
            Assert.AreEqual(CandleState.Escaping, l.State);
            Assert.AreEqual(0, l.FlameFingerCount);
            int g = 0, t = 0;
            Blow(l, 1f, true, ref g, ref t);
            Assert.AreEqual(CandleEvent.None, l.TapFan());
            Assert.AreEqual(CandleEvent.None, l.Pour());
            Assert.AreEqual(CandleEvent.None, l.FingerDown(1, HX, HY));
            Assert.AreEqual(CandleEvent.None, l.CancelFingers());
            Assert.AreEqual(CandleEvent.None, Ticks(l, 10f));
            Assert.IsFalse(l.CandleLit);
            Assert.IsTrue(l.TryComplete());
            Ticks(l, 10f);
            Assert.AreEqual(CandleState.Done, l.State);
            Assert.IsFalse(l.CandleLit);
        }

        [Test]
        public void Reset_ReplaysFromScratch()
        {
            var l = New();
            l.TapFan();
            l.Pour();
            int g = 0, t = 0;
            Blow(l, 0.6f, true, ref g, ref t);
            GrabAndDetach(l);
            Lift(l, HX - 0.2f, HY + 0.8f, HX + 0.2f, HY + 0.8f, 3f, 40);
            l.TryComplete();
            l.Reset();
            Assert.AreEqual(CandleState.Lit, l.State);
            Assert.IsTrue(l.CandleLit);
            Assert.AreEqual(HX, l.FlameX);
            Assert.AreEqual(HY, l.FlameY);
            Assert.IsFalse(l.FanOn);
            Assert.IsFalse(l.GlassBusy);
            Assert.AreEqual(0, l.BlowAttempts + l.Pours + l.FanTaps + l.Tickles + l.FlameFingerCount);
            Assert.IsFalse(l.CompletionReported);
            GrabAndDetach(l);
            Lift(l, HX - 0.2f, HY + 0.8f, HX + 0.2f, HY + 0.8f, 3f, 40);
            Assert.IsTrue(l.TryComplete());
        }
    }
}
