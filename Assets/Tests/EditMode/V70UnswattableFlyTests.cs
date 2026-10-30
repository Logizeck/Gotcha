using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 267: la mosca che non si lascia schiacciare (le false piste non costano niente, la finestra aperta la fa uscire).</summary>
    public class V70UnswattableFlyTests
    {
        const float Eps = 1e-4f;

        /// <summary>Fa passare il tempo a piccoli passi (come i fotogrammi), fermandosi al primo evento uguale a "until".</summary>
        static bool RunUntil(UnswattableFlyLogic l, FlyEvent until, float seconds, float step = 0.05f)
        {
            for (float t = 0f; t < seconds - 1e-5f; t += step)
                if (l.Tick(step) == until) return true;
            return false;
        }

        static void Run(UnswattableFlyLogic l, float seconds, float step = 0.05f)
        {
            for (float t = 0f; t < seconds - 1e-5f; t += step) l.Tick(step);
        }

        static void AssertStillBuzzing(UnswattableFlyLogic l)
        {
            Assert.AreEqual(FlyStage.Buzzing, l.Stage);
            Assert.IsFalse(l.Solved);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static void OpenWindow(UnswattableFlyLogic l)
        {
            if (l.Latched) Assert.AreEqual(LatchResult.Unlatched, l.ToggleLatch());
            l.SetOpening(1f);
        }

        [Test]
        public void StartsBuzzing_WindowClosedAndLatched()
        {
            var l = new UnswattableFlyLogic();
            AssertStillBuzzing(l);
            Assert.IsTrue(l.Latched);
            Assert.AreEqual(0f, l.Opening);
            Assert.IsFalse(l.WindowOpen);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0f, l.LeaveProgress);
            Assert.IsFalse(l.Masked || l.Foggy || l.OnScreen);
        }

        [Test]
        public void Dodge_AlwaysEscapesTheTouch_AndStaysInsideTheRoom()
        {
            // griglia di mosche e di dita, compresi gli angoli e il dito proprio sopra la mosca
            for (float fx = UnswattableFlyLogic.MinX; fx <= UnswattableFlyLogic.MaxX + 1e-3f; fx += 0.55f)
            for (float fy = UnswattableFlyLogic.MinY; fy <= UnswattableFlyLogic.MaxY + 1e-3f; fy += 0.6f)
            for (float ox = -1.2f; ox <= 1.21f; ox += 0.4f)
            for (float oy = -1.2f; oy <= 1.21f; oy += 0.4f)
            {
                float tx = fx + ox, ty = fy + oy;
                UnswattableFlyLogic.Dodge(tx, ty, fx, fy, out float nx, out float ny);
                float d = (float)System.Math.Sqrt((nx - tx) * (nx - tx) + (ny - ty) * (ny - ty));
                Assert.GreaterOrEqual(d, UnswattableFlyLogic.MinDodge - Eps, $"fly ({fx},{fy}) touch ({tx},{ty}) -> ({nx},{ny})");
                Assert.IsFalse(UnswattableFlyLogic.Threatens(tx, ty, nx, ny, UnswattableFlyLogic.MinDodge - 0.01f));
                Assert.That(nx, Is.InRange(UnswattableFlyLogic.MinX, UnswattableFlyLogic.MaxX));
                Assert.That(ny, Is.InRange(UnswattableFlyLogic.MinY, UnswattableFlyLogic.MaxY));
            }
        }

        [Test]
        public void Dodge_HandlesNaN()
        {
            UnswattableFlyLogic.Dodge(float.NaN, float.NaN, 0f, 0f, out float nx, out float ny);
            Assert.IsFalse(float.IsNaN(nx) || float.IsNaN(ny));
            UnswattableFlyLogic.Dodge(0f, 0f, float.NaN, 1f, out nx, out ny);
            Assert.IsFalse(float.IsNaN(nx) || float.IsNaN(ny));
        }

        [Test]
        public void Threatens_IsGenerous()
        {
            Assert.IsTrue(UnswattableFlyLogic.Threatens(0f, 0f, 1.0f, 0.5f));
            Assert.IsFalse(UnswattableFlyLogic.Threatens(0f, 0f, 2.0f, 0f));
        }

        [Test]
        public void TapFly_NeverWins_KeepsState()
        {
            var l = new UnswattableFlyLogic();
            for (int i = 1; i <= 10; i++) Assert.AreEqual(i, l.TapFly());
            Assert.AreEqual(10, l.Taps);
            Assert.AreEqual(10, l.WrongMoves);
            AssertStillBuzzing(l);
            Assert.IsTrue(l.Latched);
        }

        [Test]
        public void Swatter_AlwaysMisses_OnCakeSmashesIt()
        {
            var l = new UnswattableFlyLogic();
            Assert.AreEqual(SwatResult.Miss, l.Swat(false));
            Assert.AreEqual(SwatResult.SmashedCake, l.Swat(true));
            Assert.AreEqual(SwatResult.SmashedCake, l.Swat(true));
            Assert.AreEqual(3, l.Swats);
            Assert.AreEqual(2, l.CakeSmashes);
            Assert.AreEqual(2, l.CakeSquash);
            for (int i = 0; i < 5; i++) l.Swat(true);
            Assert.AreEqual(3, l.CakeSquash);   // la torta più spiaccicata di così non diventa
            AssertStillBuzzing(l);
        }

        [Test]
        public void Spray_MaskAndFog_WearOff_WithoutChangingProgress()
        {
            var l = new UnswattableFlyLogic();
            Assert.AreEqual(1, l.Spray());
            Assert.IsTrue(l.Masked);
            Assert.IsTrue(l.Foggy);
            Assert.IsTrue(RunUntil(l, FlyEvent.MaskOff, UnswattableFlyLogic.MaskSeconds + 0.2f));
            Assert.IsFalse(l.Masked);
            Assert.IsFalse(l.Foggy);
            Assert.AreEqual(2, l.Spray());
            AssertStillBuzzing(l);
            Assert.AreEqual(2, l.WrongMoves);
        }

        [Test]
        public void CoverCake_FlyWalksOnTheScreen_ThenComesBack()
        {
            var l = new UnswattableFlyLogic();
            Assert.AreEqual(1, l.CoverCake());
            Assert.IsTrue(l.OnScreen);
            Assert.AreEqual(0, l.CoverCake());   // c'è già
            Assert.IsTrue(RunUntil(l, FlyEvent.ScreenWalkOver, UnswattableFlyLogic.ScreenSeconds + 0.2f));
            Assert.IsFalse(l.OnScreen);
            // toccarla sul vetro la fa tornare subito in cucina
            l.CoverCake();
            l.TapFly();
            Assert.IsFalse(l.OnScreen);
            Assert.AreEqual(FlyEvent.ScreenWalkOver, l.Tick(0.01f));
            AssertStillBuzzing(l);
        }

        [Test]
        public void Bonks_AtIntervals_WhileTheWindowIsClosed()
        {
            var l = new UnswattableFlyLogic();
            Assert.IsTrue(RunUntil(l, FlyEvent.Bonk, UnswattableFlyLogic.FirstBonk + 0.1f));
            Assert.IsTrue(RunUntil(l, FlyEvent.Bonk, UnswattableFlyLogic.BonkPeriod + 0.1f));
            Assert.AreEqual(2, l.Bonks);
            // bussando sul vetro arriva subito
            l.KnockGlass();
            Assert.IsTrue(RunUntil(l, FlyEvent.Bonk, UnswattableFlyLogic.KnockBonk + 0.1f));
            AssertStillBuzzing(l);
        }

        [Test]
        public void NoBonks_WhileWalkingOnTheScreen()
        {
            var l = new UnswattableFlyLogic();
            l.CoverCake();
            Assert.IsFalse(RunUntil(l, FlyEvent.Bonk, UnswattableFlyLogic.ScreenSeconds - 0.1f));
        }

        [Test]
        public void LatchedWindow_DoesNotOpen_AndRattles()
        {
            var l = new UnswattableFlyLogic();
            Assert.AreEqual(WindowEvent.Blocked, l.SetOpening(1f));
            Assert.AreEqual(0f, l.Opening);
            Assert.IsTrue(l.Rattle());
            Assert.AreEqual(1, l.Rattles);
            AssertStillBuzzing(l);
        }

        [Test]
        public void Latch_TogglesOnlyWhenClosed()
        {
            var l = new UnswattableFlyLogic();
            Assert.AreEqual(LatchResult.Unlatched, l.ToggleLatch());
            Assert.AreEqual(LatchResult.Latched, l.ToggleLatch());
            Assert.AreEqual(LatchResult.Unlatched, l.ToggleLatch());
            Assert.IsFalse(l.Rattle());   // sganciata: niente da sbatacchiare
            l.SetOpening(0.2f);
            Assert.AreEqual(LatchResult.Blocked, l.ToggleLatch());
            Assert.IsFalse(l.Latched);
            l.SetOpening(0f);
            Assert.AreEqual(LatchResult.Latched, l.ToggleLatch());
        }

        [Test]
        public void SmallOpening_IsNotEnough()
        {
            var l = new UnswattableFlyLogic();
            l.ToggleLatch();
            Assert.AreEqual(WindowEvent.None, l.SetOpening(UnswattableFlyLogic.OpenEnough - 0.05f));
            Run(l, 10f);
            AssertStillBuzzing(l);
        }

        [Test]
        public void OpenWindow_FlyLeavesByItself_WinsOnce()
        {
            var l = new UnswattableFlyLogic();
            l.ToggleLatch();
            Assert.AreEqual(WindowEvent.StartedLeaving, l.SetOpening(1f));
            Assert.AreEqual(FlyStage.Leaving, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
            Assert.IsTrue(RunUntil(l, FlyEvent.NoticedWindow, UnswattableFlyLogic.NoticeSeconds + 0.1f));
            Assert.IsTrue(l.Noticed);
            Assert.IsTrue(RunUntil(l, FlyEvent.FlewOut, UnswattableFlyLogic.TravelSeconds + 0.1f));
            Assert.AreEqual(FlyStage.Outside, l.Stage);
            Assert.IsTrue(l.Solved);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(1f, l.LeaveProgress);
            // fuori: niente più eventi né mosse
            Assert.AreEqual(FlyEvent.None, l.Tick(1f));
            Assert.AreEqual(0, l.TapFly());
            Assert.AreEqual(SwatResult.None, l.Swat(true));
            Assert.AreEqual(WindowEvent.None, l.SetOpening(0f));
            Assert.AreEqual(LatchResult.None, l.ToggleLatch());
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(FlyStage.Done, l.Stage);
        }

        [Test]
        public void ClosingBeforeItLeaves_FlyStays_NoSoftlock()
        {
            var l = new UnswattableFlyLogic();
            OpenWindow(l);
            Run(l, UnswattableFlyLogic.LeaveSeconds * 0.7f);
            Assert.Greater(l.LeaveProgress, 0.5f);
            Assert.AreEqual(WindowEvent.LeaveCancelled, l.SetOpening(0f));
            AssertStillBuzzing(l);
            Assert.AreEqual(0f, l.LeaveProgress);
            Assert.AreEqual(1, l.LeaveCancels);
            Assert.IsFalse(l.Latched);   // resta sganciata: riaprirla è un attimo
            Run(l, 20f);
            AssertStillBuzzing(l);
            // ...e riaprendo si riparte da capo, fino in fondo
            Assert.AreEqual(WindowEvent.StartedLeaving, l.SetOpening(1f));
            Assert.IsTrue(RunUntil(l, FlyEvent.FlewOut, UnswattableFlyLogic.LeaveSeconds + 0.1f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ClosingAndRelatching_ThenReopening_StillWorks()
        {
            var l = new UnswattableFlyLogic();
            OpenWindow(l);
            l.SetOpening(0f);
            Assert.AreEqual(LatchResult.Latched, l.ToggleLatch());
            Assert.AreEqual(WindowEvent.Blocked, l.SetOpening(1f));
            OpenWindow(l);
            Assert.AreEqual(FlyStage.Leaving, l.Stage);
        }

        [Test]
        public void PartiallyClosing_ButStillOpenEnough_KeepsLeaving()
        {
            var l = new UnswattableFlyLogic();
            OpenWindow(l);
            Assert.AreEqual(WindowEvent.None, l.SetOpening(UnswattableFlyLogic.OpenEnough + 0.05f));
            Assert.AreEqual(FlyStage.Leaving, l.Stage);
            Assert.IsTrue(RunUntil(l, FlyEvent.FlewOut, UnswattableFlyLogic.LeaveSeconds + 0.1f));
        }

        [Test]
        public void WrongMovesBeforeOpening_DoNotMatter()
        {
            var l = new UnswattableFlyLogic();
            l.TapFly(); l.Swat(true); l.Spray(); l.CoverCake(); l.Rattle(); l.KnockGlass();
            Assert.AreEqual(5, l.WrongMoves);
            Run(l, 4f);
            AssertStillBuzzing(l);
            Assert.AreEqual(2, l.CoverCake());
            OpenWindow(l);
            Assert.IsFalse(l.OnScreen);   // dal vetro del telefono va dritta alla finestra
            Assert.IsTrue(RunUntil(l, FlyEvent.FlewOut, UnswattableFlyLogic.LeaveSeconds + 0.1f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void WhileLeaving_ItemsAreIgnored_ButTapsStillDodge()
        {
            var l = new UnswattableFlyLogic();
            OpenWindow(l);
            Assert.AreEqual(SwatResult.None, l.Swat(true));
            Assert.AreEqual(0, l.Spray());
            Assert.AreEqual(0, l.CoverCake());
            Assert.AreEqual(0, l.KnockGlass());
            Assert.AreEqual(1, l.TapFly());
            Assert.AreEqual(FlyStage.Leaving, l.Stage);
            Assert.AreEqual(0, l.CakeSmashes);
        }

        [Test]
        public void NoBonks_WhileLeaving()
        {
            var l = new UnswattableFlyLogic();
            Run(l, UnswattableFlyLogic.FirstBonk - 0.2f);
            OpenWindow(l);
            for (float t = 0f; t < UnswattableFlyLogic.LeaveSeconds - 0.1f; t += 0.05f)
                Assert.AreNotEqual(FlyEvent.Bonk, l.Tick(0.05f));
        }

        [Test]
        public void LongPauseStep_IsClamped_AndBadSteps_Ignored()
        {
            var l = new UnswattableFlyLogic();
            OpenWindow(l);
            l.Tick(600f);   // dieci minuti di pausa: un passo solo, corto
            Assert.AreEqual(UnswattableFlyLogic.MaxStep / UnswattableFlyLogic.LeaveSeconds, l.LeaveProgress, Eps);
            l.Tick(float.NaN);
            l.Tick(-1f);
            Assert.AreEqual(UnswattableFlyLogic.MaxStep / UnswattableFlyLogic.LeaveSeconds, l.LeaveProgress, Eps);
            Assert.AreEqual(FlyStage.Leaving, l.Stage);
        }

        [Test]
        public void SetOpening_Clamps_AndHandlesNaN()
        {
            var l = new UnswattableFlyLogic();
            l.ToggleLatch();
            l.SetOpening(5f);
            Assert.AreEqual(1f, l.Opening);
            l.SetOpening(float.NaN);
            Assert.AreEqual(0f, l.Opening);
            AssertStillBuzzing(l);
        }

        [Test]
        public void Reset_RestoresTheStart()
        {
            var l = new UnswattableFlyLogic();
            l.TapFly(); l.Swat(true); l.Spray(); l.CoverCake(); l.Rattle();
            OpenWindow(l);
            RunUntil(l, FlyEvent.FlewOut, 5f);
            l.TryComplete();
            l.Reset();
            AssertStillBuzzing(l);
            Assert.IsTrue(l.Latched);
            Assert.AreEqual(0f, l.Opening);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.CakeSquash);
            Assert.IsFalse(l.CompletionReported);
            Assert.IsFalse(l.Masked || l.Foggy || l.OnScreen);
            Assert.IsTrue(RunUntil(l, FlyEvent.Bonk, UnswattableFlyLogic.FirstBonk + 0.1f));
        }
    }
}
