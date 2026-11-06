using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 47 della roadmap: The Wrong Prize (trascinare il NUMERO 1 sul gradino del corridore).</summary>
    public class V83WrongPrizeTests
    {
        const int L = WrongPrizeLogic.LeftStep, C = WrongPrizeLogic.CentreStep, R = WrongPrizeLogic.RightStep;
        const int None = WrongPrizeLogic.NoStep;

        static void AssertNotWon(WrongPrizeLogic l)
        {
            Assert.AreEqual(WrongPrizeStage.Playing, l.Stage);
            Assert.IsFalse(l.Won);
            Assert.IsFalse(l.TryComplete());
        }

        static void AssertStart(WrongPrizeLogic l)
        {
            Assert.AreEqual(2, l.NumberOn(L));
            Assert.AreEqual(1, l.NumberOn(C));
            Assert.AreEqual(3, l.NumberOn(R));
            Assert.AreEqual(3, l.RunnerPlace);
            Assert.AreEqual(C, l.FirstStep);
        }

        /// <summary>Il dito prende il numero del gradino <paramref name="from"/>, lo trascina a passi e lo lascia su <paramref name="onto"/>.</summary>
        static WrongPrizeDrop DragDigit(WrongPrizeLogic l, int finger, int from, int onto)
        {
            Assert.AreEqual(WrongPrizeGrab.Digit, l.Grab(finger, WrongPrizePart.Digit, from, 0f, 0f));
            for (int i = 1; i <= 8; i++) Assert.IsTrue(l.Move(finger, 1.5f * i / 8f, 0.5f * i / 8f));
            return l.Drop(finger, onto);
        }

        static WrongPrizeDrop DragRunner(WrongPrizeLogic l, int finger, int onto)
        {
            Assert.AreEqual(WrongPrizeGrab.Runner, l.Grab(finger, WrongPrizePart.Runner, None, 1.4f, 0f));
            for (int i = 1; i <= 8; i++) Assert.IsTrue(l.Move(finger, 1.4f - 1.4f * i / 8f, 0.6f * i / 8f));
            return l.Drop(finger, onto);
        }

        [Test]
        public void StartsWithTheClassicPodium()
        {
            var l = new WrongPrizeLogic();
            AssertStart(l);
            Assert.AreEqual(R, WrongPrizeLogic.RunnerStep);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.Dragging);
            Assert.AreEqual(0, l.WrongMoves);
            AssertNotWon(l);
        }

        [Test]
        public void RunnerOnFirstStepIsSentBackAndNeverWins()
        {
            var l = new WrongPrizeLogic();
            for (int i = 0; i < 30; i++)
            {
                Assert.AreEqual(WrongPrizeDrop.RunnerSentBack, DragRunner(l, 3, C));
                AssertNotWon(l);
                AssertStart(l);
            }
            Assert.AreEqual(30, l.SentBack);
            Assert.IsFalse(l.Dragging);
        }

        [Test]
        public void RunnerDroppedElsewhereReturns()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeDrop.RunnerReturned, DragRunner(l, 1, None));
            Assert.AreEqual(WrongPrizeDrop.RunnerReturned, DragRunner(l, 1, L));
            Assert.AreEqual(WrongPrizeDrop.RunnerReturned, DragRunner(l, 1, R));
            Assert.AreEqual(WrongPrizeDrop.RunnerReturned, DragRunner(l, 1, 99));
            Assert.AreEqual(4, l.RunnerReturns);
            AssertStart(l);
            AssertNotWon(l);
        }

        [Test]
        public void RunnerTapJustHops()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeGrab.Runner, l.Grab(2, WrongPrizePart.Runner, None, 1.4f, 0f));
            Assert.IsTrue(l.Move(2, 1.45f, 0.05f));   // sotto la soglia del tocco
            Assert.IsFalse(l.DragMoved);
            Assert.AreEqual(WrongPrizeDrop.RunnerTap, l.Drop(2, R));
            Assert.AreEqual(1, l.RunnerTaps);
            Assert.AreEqual(0, l.WrongMoves);
            AssertNotWon(l);
        }

        [Test]
        public void TrophyIsBoltedAndNeverMoves()
        {
            var l = new WrongPrizeLogic();
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(WrongPrizeGrab.Bolted, l.Grab(i, WrongPrizePart.Trophy, C, 0f, 0.5f));
                Assert.IsFalse(l.Dragging, "la coppa non si trascina mai");
                Assert.AreEqual(WrongPrizeDrop.None, l.Drop(i, R));
            }
            Assert.AreEqual(20, l.TrophyTries);
            AssertStart(l);
            AssertNotWon(l);
        }

        [Test]
        public void DigitOneDroppedElsewhereReturnsHome()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeDrop.DigitReturned, DragDigit(l, 1, C, None));
            Assert.AreEqual(WrongPrizeDrop.DigitReturned, DragDigit(l, 1, C, C));
            Assert.AreEqual(WrongPrizeDrop.NotYourStep, DragDigit(l, 1, C, L));
            Assert.AreEqual(WrongPrizeDrop.DigitReturned, DragDigit(l, 1, C, -7));
            Assert.AreEqual(3, l.DigitReturns);
            Assert.AreEqual(1, l.WrongSteps);
            AssertStart(l);
            AssertNotWon(l);
        }

        [Test]
        public void DigitTapDoesNothing()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeGrab.Digit, l.Grab(4, WrongPrizePart.Digit, C, 0f, 0f));
            Assert.AreEqual(1, l.DragNumber);
            Assert.AreEqual(C, l.DragStep);
            Assert.AreEqual(WrongPrizeDrop.DigitTap, l.Drop(4, None));
            Assert.AreEqual(1, l.DigitTaps);
            AssertStart(l);
            AssertNotWon(l);
        }

        [Test]
        public void DigitOneOnRunnerStepWinsOnce()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 5, C, R));
            Assert.AreEqual(1, l.RunnerPlace);
            Assert.AreEqual(3, l.NumberOn(C), "il 3 va sul vecchio primo gradino");
            Assert.AreEqual(2, l.NumberOn(L));
            Assert.AreEqual(R, l.FirstStep);
            Assert.AreEqual(WrongPrizeStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(WrongPrizeStage.Done, l.Stage);
        }

        [Test]
        public void NothingCountsAfterTheWin()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 5, C, R));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(6, WrongPrizePart.Digit, C, 0f, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(6, WrongPrizePart.Runner, None, 0f, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(6, WrongPrizePart.Trophy, C, 0f, 0f));
            Assert.AreEqual(WrongPrizeDrop.None, l.Drop(6, R));
            Assert.AreEqual(1, l.RunnerPlace);
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(6, WrongPrizePart.Digit, R, 0f, 0f));
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void DigitTwoSwapsWithThreeAndIsReversible()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeDrop.SecondPlace, DragDigit(l, 1, L, R));
            Assert.AreEqual(2, l.RunnerPlace);
            Assert.AreEqual(3, l.NumberOn(L));
            Assert.AreEqual(1, l.NumberOn(C));
            AssertNotWon(l);
            // il 3 riportato sul gradino del corridore: di nuovo terzo
            Assert.AreEqual(WrongPrizeDrop.ThirdAgain, DragDigit(l, 1, L, R));
            AssertStart(l);
            AssertNotWon(l);
            Assert.AreEqual(2, l.Swaps);
        }

        [Test]
        public void OneStillWinsAfterTheTwoSwap()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeDrop.SecondPlace, DragDigit(l, 1, L, R));
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 1, C, R));
            Assert.AreEqual(1, l.RunnerPlace);
            Assert.AreEqual(2, l.NumberOn(C));
            Assert.AreEqual(3, l.NumberOn(L));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void RunnerStepDigitDraggedAwayReturns()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeDrop.NotYourStep, DragDigit(l, 1, R, C));
            Assert.AreEqual(WrongPrizeDrop.NotYourStep, DragDigit(l, 1, R, L));
            Assert.AreEqual(WrongPrizeDrop.DigitReturned, DragDigit(l, 1, R, R));
            AssertStart(l);
            AssertNotWon(l);
        }

        [Test]
        public void ThreeWrongAttemptsThenTheSolution()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeDrop.RunnerSentBack, DragRunner(l, 1, C));
            Assert.AreEqual(WrongPrizeGrab.Bolted, l.Grab(1, WrongPrizePart.Trophy, C, 0f, 0f));
            Assert.AreEqual(WrongPrizeDrop.DigitReturned, DragDigit(l, 1, C, None));
            Assert.AreEqual(WrongPrizeDrop.NotYourStep, DragDigit(l, 1, C, L));
            Assert.AreEqual(4, l.WrongMoves);
            AssertNotWon(l);
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 1, C, R));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void OneFingerAtATime()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeGrab.Digit, l.Grab(1, WrongPrizePart.Digit, C, 0f, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(2, WrongPrizePart.Runner, None, 0f, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(2, WrongPrizePart.Trophy, C, 0f, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(2, WrongPrizePart.Digit, L, 0f, 0f));
            Assert.IsFalse(l.Move(2, 5f, 5f), "il secondo dito non muove niente");
            Assert.AreEqual(WrongPrizeDrop.None, l.Drop(2, R), "il secondo dito che si alza non lascia niente");
            Assert.IsTrue(l.Dragging);
            Assert.AreEqual(1, l.DragFinger);
            Assert.AreEqual(0, l.TrophyTries);
            // il primo dito lascia l'1 sul gradino del corridore: vale
            Assert.IsTrue(l.Move(1, 1.5f, 0f));
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, l.Drop(1, R));
        }

        [Test]
        public void RapidTapsNeverWinOrBreak()
        {
            var l = new WrongPrizeLogic();
            for (int i = 0; i < 200; i++)
            {
                int f = i % 3;
                WrongPrizePart part = (WrongPrizePart)(i % 3);
                l.Grab(f, part, i % 4 - 1, 0f, 0f);
                l.Drop(f, (i % 5) - 1);   // tocchi rapidissimi: nessuno spostamento
            }
            AssertStart(l);
            AssertNotWon(l);
            Assert.AreEqual(0, l.Swaps);
            l.Cancel();
            Assert.IsFalse(l.Dragging);
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 9, C, R));
        }

        [Test]
        public void EarlyReleaseWithoutMovingIsATap()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeGrab.Digit, l.Grab(1, WrongPrizePart.Digit, C, 0f, 0f));
            Assert.AreEqual(WrongPrizeDrop.DigitTap, l.Drop(1, C));
            AssertStart(l);
            AssertNotWon(l);
        }

        [Test]
        public void ReleaseWithoutMovingNeverSwaps()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeGrab.Digit, l.Grab(1, WrongPrizePart.Digit, C, 0f, 0f));
            Assert.AreEqual(WrongPrizeDrop.DigitTap, l.Drop(1, R), "un dito fermo non porta l'1 da nessuna parte");
            Assert.AreEqual(WrongPrizeGrab.Runner, l.Grab(1, WrongPrizePart.Runner, None, 0f, 0f));
            Assert.AreEqual(WrongPrizeDrop.RunnerTap, l.Drop(1, C));
            Assert.AreEqual(0, l.Swaps);
            AssertStart(l);
            AssertNotWon(l);
        }

        [Test]
        public void CancelPutsEverythingBackAndCountsNothing()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeGrab.Digit, l.Grab(1, WrongPrizePart.Digit, C, 0f, 0f));
            Assert.IsTrue(l.Move(1, 1.5f, 0f));
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Dragging);
            Assert.AreEqual(WrongPrizeDrop.None, l.Drop(1, R), "dopo la pausa il dito che si alza non conta");
            Assert.AreEqual(0, l.DragNumber);
            Assert.AreEqual(WrongPrizeGrab.Runner, l.Grab(2, WrongPrizePart.Runner, None, 0f, 0f));
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Cancel());
            Assert.AreEqual(0, l.WrongMoves);
            AssertStart(l);
            AssertNotWon(l);
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 3, C, R));
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = new WrongPrizeLogic();
            DragDigit(l, 1, L, R);
            DragRunner(l, 1, C);
            l.Grab(1, WrongPrizePart.Trophy, C, 0f, 0f);
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 1, C, R));
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStart(l);
            AssertNotWon(l);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Swaps);
            Assert.IsFalse(l.Dragging);
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 1, C, R));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetWhileDraggingDropsNothing()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeGrab.Digit, l.Grab(1, WrongPrizePart.Digit, C, 0f, 0f));
            l.Reset();
            Assert.IsFalse(l.Dragging);
            Assert.AreEqual(WrongPrizeDrop.None, l.Drop(1, R));
            AssertStart(l);
        }

        [Test]
        public void BadInputIsIgnored()
        {
            var l = new WrongPrizeLogic();
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(WrongPrizeLogic.NoFinger, WrongPrizePart.Digit, C, 0f, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(1, WrongPrizePart.Digit, 3, 0f, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(1, WrongPrizePart.Digit, -1, 0f, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(1, WrongPrizePart.Digit, C, float.NaN, 0f));
            Assert.AreEqual(WrongPrizeGrab.None, l.Grab(1, WrongPrizePart.Runner, None, 0f, float.PositiveInfinity));
            Assert.IsFalse(l.Dragging);
            Assert.AreEqual(WrongPrizeGrab.Digit, l.Grab(1, WrongPrizePart.Digit, C, 0f, 0f));
            Assert.IsFalse(l.Move(1, float.NaN, 0f));
            Assert.AreEqual(WrongPrizeDrop.DigitTap, l.Drop(1, None));
            Assert.AreEqual(0, l.NumberOn(-1));
            Assert.AreEqual(0, l.NumberOn(3));
            Assert.AreEqual(None, l.StepOf(7));
        }

        [Test]
        public void NumbersAreAlwaysAPermutation()
        {
            var l = new WrongPrizeLogic();
            var rnd = new System.Random(47);
            for (int i = 0; i < 500 && !l.Won; i++)
            {
                int from = rnd.Next(3), onto = rnd.Next(5) - 1;
                if (from == C && onto == R) continue;   // la soluzione la si tiene per la fine
                if (l.NumberOn(from) == 1 && onto == R) continue;
                DragDigit(l, 1, from, onto);
                Assert.AreEqual(6, l.NumberOn(L) + l.NumberOn(C) + l.NumberOn(R));
                Assert.AreNotEqual(None, l.StepOf(1));
                Assert.AreNotEqual(None, l.StepOf(2));
                Assert.AreNotEqual(None, l.StepOf(3));
                Assert.AreEqual(C, l.FirstStep, "l'1 lascia il centro solo per il gradino del corridore");
            }
            AssertNotWon(l);
            Assert.AreEqual(WrongPrizeDrop.FirstPlace, DragDigit(l, 1, l.FirstStep, R));
        }
    }
}
