using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 5 della roadmap: il leone fifone (la criniera è un costume da tirare via).</summary>
    public class V76CowardlyLionTests
    {
        const float Far = CowardlyLionLogic.RevealDistance + 0.1f;

        static void AssertStillPlaying(CowardlyLionLogic l)
        {
            Assert.AreEqual(CowardlyLionStage.Playing, l.Stage);
            Assert.AreEqual(CowardlyLionMane.On, l.Mane);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static CowardlyLionRelease Pull(CowardlyLionLogic l, float distance)
        {
            Assert.IsTrue(l.GrabMane());
            l.DragMane(distance);
            return l.ReleaseMane(distance);
        }

        [Test]
        public void StartsCalmWithTheManeOn()
        {
            var l = new CowardlyLionLogic();
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.RoarLevel);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void FarDrag_RevealsOnce_AndWinsOnce()
        {
            var l = new CowardlyLionLogic();
            Assert.AreEqual(CowardlyLionRelease.Off, Pull(l, Far));
            Assert.AreEqual(CowardlyLionStage.Revealing, l.Stage);
            Assert.AreEqual(CowardlyLionMane.Off, l.Mane);
            Assert.IsTrue(l.InputLocked);
            // a criniera tolta niente si ripete
            Assert.IsFalse(l.GrabMane());
            Assert.AreEqual(CowardlyLionRelease.None, l.ReleaseMane(Far));
            Assert.AreEqual(CowardlyLionRoar.None, l.TapLion());
            Assert.AreEqual(CowardlyLionButton.None, l.PressRoar());
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(CowardlyLionStage.Done, l.Stage);
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ExactlyAtThreshold_Counts()
        {
            var l = new CowardlyLionLogic();
            Assert.AreEqual(CowardlyLionRelease.Off, Pull(l, CowardlyLionLogic.RevealDistance));
        }

        [Test]
        public void SmallDrag_SnapsBack_AndCanBeRetried()
        {
            var l = new CowardlyLionLogic();
            foreach (float d in new[] { 0.3f, 0.8f, 1.2f, CowardlyLionLogic.RevealDistance - 0.01f })
            {
                Assert.AreEqual(CowardlyLionRelease.SnapBack, Pull(l, d), "d " + d);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(4, l.SnapBacks);
            Assert.AreEqual(CowardlyLionRelease.Off, Pull(l, Far));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void LightTap_OnTheMane_Wobbles()
        {
            var l = new CowardlyLionLogic();
            Assert.AreEqual(CowardlyLionRelease.Wobble, Pull(l, 0f));
            Assert.AreEqual(CowardlyLionRelease.Wobble, Pull(l, CowardlyLionLogic.WobbleDistance - 0.01f));
            Assert.AreEqual(2, l.ManeWobbles);
            AssertStillPlaying(l);
        }

        [Test]
        public void PastThresholdDuringDrag_ButReleasedClose_SnapsBack()
        {
            // documentato: conta la distanza al rilascio, non il massimo raggiunto
            var l = new CowardlyLionLogic();
            Assert.IsTrue(l.GrabMane());
            l.DragMane(Far + 1f);
            Assert.IsTrue(l.PastThreshold);
            Assert.AreEqual(CowardlyLionStage.Playing, l.Stage);   // nessuna rivelazione a dito ancora giù
            l.DragMane(0.5f);
            Assert.IsFalse(l.PastThreshold);
            Assert.AreEqual(CowardlyLionRelease.SnapBack, l.ReleaseMane(0.5f));
            AssertStillPlaying(l);
        }

        [Test]
        public void RevealFraction_IsProgressiveAndClamped()
        {
            Assert.AreEqual(0f, CowardlyLionLogic.RevealFraction(-1f), 1e-5f);
            Assert.AreEqual(0f, CowardlyLionLogic.RevealFraction(0f), 1e-5f);
            Assert.AreEqual(0.5f, CowardlyLionLogic.RevealFraction(CowardlyLionLogic.RevealDistance * 0.5f), 1e-5f);
            Assert.AreEqual(1f, CowardlyLionLogic.RevealFraction(CowardlyLionLogic.RevealDistance), 1e-5f);
            Assert.AreEqual(1f, CowardlyLionLogic.RevealFraction(10f), 1e-5f);
        }

        [Test]
        public void TapsAndRoarButton_NeverWin()
        {
            var l = new CowardlyLionLogic();
            for (int i = 0; i < 50; i++)
            {
                Assert.AreNotEqual(CowardlyLionRoar.None, l.TapLion());
                Assert.AreNotEqual(CowardlyLionButton.None, l.PressRoar());
                l.Tick(0.05f);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(50, l.LionTaps);
            Assert.AreEqual(50, l.ButtonPresses);
            Assert.AreEqual(100, l.WrongMoves);
        }

        [Test]
        public void RoarEscalation_IsCapped()
        {
            var l = new CowardlyLionLogic();
            for (int i = 1; i <= CowardlyLionLogic.MaxRoar; i++)
            {
                Assert.AreEqual(CowardlyLionRoar.Roar, l.TapLion());
                Assert.AreEqual(i, l.RoarLevel);
            }
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(CowardlyLionRoar.Capped, l.TapLion());
                Assert.AreEqual(CowardlyLionLogic.MaxRoar, l.RoarLevel);
            }
            for (int i = 0; i < 10; i++) l.PressRoar();
            Assert.AreEqual(CowardlyLionLogic.MaxRoar, l.RoarLevel);
        }

        [Test]
        public void RoarButton_AlternatesLines_AndGrowsTwice()
        {
            var l = new CowardlyLionLogic();
            Assert.AreEqual(CowardlyLionButton.RoarsBack, l.PressRoar());
            Assert.AreEqual(2, l.RoarLevel);
            Assert.AreEqual(CowardlyLionButton.Taunt, l.PressRoar());
            Assert.AreEqual(CowardlyLionButton.RoarsBack, l.PressRoar());
            Assert.AreEqual(CowardlyLionLogic.MaxRoar, l.RoarLevel);
        }

        [Test]
        public void RoarEscalation_IsReversible_TheLionCalmsDown()
        {
            var l = new CowardlyLionLogic();
            for (int i = 0; i < 6; i++) l.TapLion();
            Assert.AreEqual(CowardlyLionLogic.MaxRoar, l.RoarLevel);
            l.Tick(CowardlyLionLogic.CalmDelay - 0.1f);
            Assert.AreEqual(CowardlyLionLogic.MaxRoar, l.RoarLevel);
            l.Tick(0.2f);
            Assert.AreEqual(CowardlyLionLogic.MaxRoar - 1, l.RoarLevel);
            for (int i = 0; i < 100; i++) l.Tick(0.1f);
            Assert.AreEqual(0, l.RoarLevel);
            // un nuovo ruggito riparte da capo
            Assert.AreEqual(CowardlyLionRoar.Roar, l.TapLion());
            Assert.AreEqual(1, l.RoarLevel);
        }

        [Test]
        public void WhileTheManeIsHeld_TapsAndButtonDoNothing()
        {
            var l = new CowardlyLionLogic();
            l.PressRoar();
            Assert.IsTrue(l.GrabMane());
            Assert.AreEqual(2, l.RoarLevel);
            Assert.AreEqual(CowardlyLionMane.Held, l.Mane);
            // mentre la criniera è tenuta, tocchi e bottone non fanno niente
            Assert.AreEqual(CowardlyLionRoar.None, l.TapLion());
            Assert.AreEqual(CowardlyLionButton.None, l.PressRoar());
            Assert.IsFalse(l.GrabMane());   // un secondo dito non la riafferra
            Assert.AreEqual(CowardlyLionRelease.SnapBack, l.ReleaseMane(0.6f));
            Assert.AreEqual(1, l.ButtonPresses);   // solo la pressione prima della presa conta
            Assert.AreEqual(0, l.LionTaps);
        }

        [Test]
        public void TapOnTheFace_CancelThenTap_KeepsTheEscalation()
        {
            // il livello tratta un tocco sul muso (che sta sulla criniera) come presa annullata + tocco sul leone
            var l = new CowardlyLionLogic();
            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(l.GrabMane());
                Assert.IsTrue(l.CancelMane());
                Assert.AreEqual(CowardlyLionRoar.Roar, l.TapLion());
                Assert.AreEqual(i, l.RoarLevel);
            }
            Assert.AreEqual(0, l.ManeWobbles + l.SnapBacks);
        }

        [Test]
        public void CancelDuringDrag_PutsTheManeBack_NeverWins()
        {
            var l = new CowardlyLionLogic();
            Assert.IsTrue(l.GrabMane());
            l.DragMane(Far + 0.5f);
            Assert.IsTrue(l.CancelMane());   // pausa / app in background a dito giù oltre la soglia
            AssertStillPlaying(l);
            Assert.IsFalse(l.CancelMane());
            Assert.AreEqual(CowardlyLionRelease.None, l.ReleaseMane(Far));   // il rilascio tardivo non conta
            AssertStillPlaying(l);
            Assert.AreEqual(CowardlyLionRelease.Off, Pull(l, Far));
        }

        [Test]
        public void ReleaseWithoutGrab_DoesNothing()
        {
            var l = new CowardlyLionLogic();
            Assert.AreEqual(CowardlyLionRelease.None, l.ReleaseMane(Far));
            l.DragMane(Far);
            Assert.IsFalse(l.PastThreshold);
            AssertStillPlaying(l);
        }

        [Test]
        public void ThreeWrongAttemptsThenSolution()
        {
            var l = new CowardlyLionLogic();
            Assert.AreEqual(CowardlyLionRoar.Roar, l.TapLion());
            Assert.AreEqual(CowardlyLionButton.RoarsBack, l.PressRoar());
            Assert.AreEqual(CowardlyLionRelease.Wobble, Pull(l, 0.05f));
            Assert.AreEqual(CowardlyLionRelease.SnapBack, Pull(l, 1.0f));
            Assert.AreEqual(4, l.WrongMoves);
            AssertStillPlaying(l);
            Assert.AreEqual(CowardlyLionRelease.Off, Pull(l, 2.4f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void RapidRepeatedTouches_StayValid()
        {
            var l = new CowardlyLionLogic();
            for (int i = 0; i < 200; i++)
            {
                if (i % 3 == 0) l.TapLion();
                else if (i % 3 == 1) Pull(l, 0.05f * (i % 20));
                else l.PressRoar();
                Assert.IsTrue(l.RoarLevel >= 0 && l.RoarLevel <= CowardlyLionLogic.MaxRoar);
                Assert.AreEqual(CowardlyLionStage.Playing, l.Stage);
            }
            Assert.AreEqual(CowardlyLionMane.On, l.Mane);
        }

        [Test]
        public void Reset_RestoresEverything_EvenAfterTheWin()
        {
            var l = new CowardlyLionLogic();
            l.TapLion();
            l.PressRoar();
            Pull(l, 0.7f);
            Pull(l, Far);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.RoarLevel);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0f, l.PullDistance, 1e-5f);
            // dopo il reset si può rivincere (rientro nel livello), sempre una volta sola
            Assert.AreEqual(CowardlyLionRelease.Off, Pull(l, Far));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void Reset_DuringAHeldMane()
        {
            var l = new CowardlyLionLogic();
            Assert.IsTrue(l.GrabMane());
            l.DragMane(1.0f);
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(CowardlyLionRelease.None, l.ReleaseMane(Far));
        }
    }
}
