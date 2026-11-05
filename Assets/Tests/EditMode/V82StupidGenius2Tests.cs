using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 44 della roadmap: The Stupid Genius 2 (trascinare il 5 sul 4, poi premere il 4 per valore).</summary>
    public class V82StupidGenius2Tests
    {
        const int Res = StupidGenius2Logic.ResultSymbol;
        const int None = StupidGenius2Logic.NoTarget;
        static readonly int[] Values = { 3, 4, 5 };

        static void AssertNotWon(StupidGenius2Logic l)
        {
            Assert.AreNotEqual(StupidGenius2Stage.Celebrating, l.Stage);
            Assert.AreNotEqual(StupidGenius2Stage.Done, l.Stage);
            Assert.IsFalse(l.TryComplete());
        }

        /// <summary>Il dito prende il 5 in (0, 0), lo porta a (x, y) a passi e lo lascia sopra il pulsante di valore target.</summary>
        static StupidGenius2Drop DragFive(StupidGenius2Logic l, int finger, int target, float x = 1f, float y = -3f)
        {
            Assert.AreEqual(StupidGenius2Grab.Dragging, l.Grab(finger, Res, 0f, 0f));
            for (int i = 1; i <= 8; i++) Assert.IsTrue(l.Move(finger, x * i / 8f, y * i / 8f));
            return l.Drop(finger, target);
        }

        static StupidGenius2Logic FixedLogic()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Drop.Fixed, DragFive(l, 1, 4));
            Assert.AreEqual(StupidGenius2Stage.Fixing, l.Stage);
            Assert.IsTrue(l.FinishFix());
            Assert.AreEqual(StupidGenius2Stage.Fixed, l.Stage);
            return l;
        }

        [Test]
        public void StartsWithTheWrongEquation()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Stage.Wrong, l.Stage);
            Assert.AreEqual(5, l.BoardResult);
            Assert.AreEqual(4, StupidGenius2Logic.TrueSum);
            Assert.IsFalse(l.BoardFixed);
            Assert.IsFalse(l.InputLocked);
            Assert.AreEqual(-1, l.CorrectButton(Values));
            AssertNotWon(l);
        }

        [Test]
        public void EveryAnswerBeforeTheFixIsWrong()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Answer.BoardSaysFive, l.Answer(4));
            Assert.AreEqual(StupidGenius2Answer.AgreedWithBoard, l.Answer(5));
            Assert.AreEqual(StupidGenius2Answer.NotEvenClose, l.Answer(3));
            Assert.AreEqual(StupidGenius2Answer.NotEvenClose, l.Answer(7));
            Assert.AreEqual(4, l.WrongAnswers);
            Assert.AreEqual(StupidGenius2Stage.Wrong, l.Stage);
            AssertNotWon(l);
        }

        [Test]
        public void PressingFourBeforeTheFixNeverWins()
        {
            var l = new StupidGenius2Logic();
            for (int i = 0; i < 50; i++) Assert.AreEqual(StupidGenius2Answer.BoardSaysFive, l.Answer(4));
            AssertNotWon(l);
            Assert.AreEqual(5, l.BoardResult);
        }

        [Test]
        public void DragFiveOntoFourFixesTheEquation()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Drop.Fixed, DragFive(l, 7, 4));
            Assert.IsTrue(l.BoardFixed);
            Assert.AreEqual(4, l.BoardResult);
            Assert.IsTrue(l.InputLocked, "scambio in corso: input bloccato");
            Assert.AreEqual(StupidGenius2Answer.None, l.Answer(4), "durante lo scambio le pressioni non contano");
            Assert.AreEqual(StupidGenius2Grab.None, l.Grab(8, Res, 0f, 0f));
            Assert.IsTrue(l.FinishFix());
            Assert.IsFalse(l.FinishFix(), "una volta sola");
            Assert.AreEqual(StupidGenius2Stage.Fixed, l.Stage);
            Assert.IsFalse(l.InputLocked);
            AssertNotWon(l);
        }

        [Test]
        public void DragFiveOntoThreeOrFiveReturnsIt()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Drop.DoesntHelp, DragFive(l, 1, 3));
            Assert.AreEqual(StupidGenius2Drop.DoesntHelp, DragFive(l, 1, 5, -1f));
            Assert.AreEqual(StupidGenius2Drop.Returned, DragFive(l, 2, None, 2f, 1f));
            Assert.AreEqual(2, l.WrongDrops);
            Assert.AreEqual(1, l.Returns);
            Assert.AreEqual(StupidGenius2Stage.Wrong, l.Stage);
            Assert.AreEqual(5, l.BoardResult);
            Assert.IsFalse(l.Dragging);
            // si può sempre riprovare: la soluzione funziona ancora
            Assert.AreEqual(StupidGenius2Drop.Fixed, DragFive(l, 3, 4));
        }

        [Test]
        public void TapOnFiveIsNotADrag()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Grab.Dragging, l.Grab(1, Res, 0f, 0f));
            Assert.IsTrue(l.Move(1, 0.05f, 0.05f));
            Assert.IsFalse(l.DragMoved);
            Assert.AreEqual(StupidGenius2Drop.Tap, l.Drop(1, None));
            Assert.AreEqual(1, l.Taps);
            Assert.AreEqual(StupidGenius2Stage.Wrong, l.Stage);
        }

        [Test]
        public void OtherBoardSymbolsAreNotDraggable()
        {
            var l = new StupidGenius2Logic();
            for (int s = 0; s < Res; s++)
            {
                Assert.AreEqual(StupidGenius2Grab.NotDraggable, l.Grab(1, s, 0f, 0f));
                Assert.IsFalse(l.Dragging);
                Assert.AreEqual(StupidGenius2Drop.None, l.Drop(1, 4), "un simbolo fermo non si lascia su niente");
            }
            Assert.AreEqual(StupidGenius2Grab.None, l.Grab(1, -1, 0f, 0f));
            Assert.AreEqual(StupidGenius2Grab.None, l.Grab(1, 99, 0f, 0f));
            Assert.AreEqual(4, l.Wiggles);
            Assert.AreEqual(StupidGenius2Stage.Wrong, l.Stage);
        }

        [Test]
        public void AfterTheFixOnlyTheValueFourWins()
        {
            var l = FixedLogic();
            Assert.AreEqual(StupidGenius2Answer.BoardAgreesWrong, l.Answer(3));
            Assert.AreEqual(StupidGenius2Answer.BoardAgreesWrong, l.Answer(5));
            AssertNotWon(l);
            Assert.AreEqual(StupidGenius2Answer.Correct, l.Answer(4));
            Assert.AreEqual(StupidGenius2Stage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
        }

        [Test]
        public void TheWinnerIsFoundByValueNotByIndex()
        {
            // pulsanti in qualunque ordine: il giusto è quello che mostra 4
            foreach (int[] order in new[] { new[] { 3, 4, 5 }, new[] { 4, 5, 3 }, new[] { 5, 3, 4 } })
            {
                var l = FixedLogic();
                int correct = l.CorrectButton(order);
                Assert.AreEqual(4, order[correct]);
                for (int i = 0; i < order.Length; i++)
                {
                    if (i == correct) continue;
                    Assert.AreEqual(StupidGenius2Answer.BoardAgreesWrong, l.Answer(order[i]));
                }
                Assert.AreEqual(StupidGenius2Answer.Correct, l.Answer(order[correct]));
            }
            // indice 1 (la posizione del 4 nella disposizione standard) non vince se mostra un altro valore
            var m = FixedLogic();
            int[] shuffled = { 4, 5, 3 };
            Assert.AreEqual(StupidGenius2Answer.BoardAgreesWrong, m.Answer(shuffled[1]));
            Assert.AreEqual(-1, m.CorrectButton(new[] { 3, 5, 6 }), "nessun pulsante mostra 4");
        }

        [Test]
        public void WinFiresExactlyOnce()
        {
            var l = FixedLogic();
            Assert.AreEqual(StupidGenius2Answer.Correct, l.Answer(4));
            for (int i = 0; i < 20; i++) Assert.AreEqual(StupidGenius2Answer.None, l.Answer(4), "pressioni rapide durante la festa");
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(StupidGenius2Stage.Done, l.Stage);
            Assert.AreEqual(StupidGenius2Answer.None, l.Answer(4));
            Assert.AreEqual(StupidGenius2Grab.None, l.Grab(1, Res, 0f, 0f));
        }

        [Test]
        public void RapidTapsNeverWinBeforeTheFix()
        {
            var l = new StupidGenius2Logic();
            for (int i = 0; i < 200; i++)
            {
                l.Answer(3 + i % 3);
                l.Grab(i, i % 4, 0f, 0f);   // simboli fermi
            }
            AssertNotWon(l);
            Assert.AreEqual(StupidGenius2Stage.Wrong, l.Stage);
            Assert.AreEqual(200, l.WrongAnswers);
        }

        [Test]
        public void OnlyOneFingerDragsAndAnswersWaitForTheDrop()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Grab.Dragging, l.Grab(1, Res, 0f, 0f));
            Assert.AreEqual(StupidGenius2Grab.None, l.Grab(2, Res, 0f, 0f), "secondo dito ignorato");
            Assert.IsFalse(l.Move(2, 5f, 5f));
            Assert.AreEqual(StupidGenius2Drop.None, l.Drop(2, 4), "il secondo dito non lascia niente");
            Assert.AreEqual(StupidGenius2Answer.None, l.Answer(4), "nessuna risposta mentre il 5 è in mano");
            Assert.IsTrue(l.Dragging);
            Assert.AreEqual(1, l.DragFinger);
            Assert.IsTrue(l.Move(1, 1f, -3f));
            Assert.AreEqual(StupidGenius2Drop.Fixed, l.Drop(1, 4));
        }

        [Test]
        public void EarlyReleaseAndCancelLeaveAValidState()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Grab.Dragging, l.Grab(1, Res, 0f, 0f));
            Assert.IsTrue(l.Move(1, 0.5f, -0.5f));
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Cancel());
            Assert.IsFalse(l.Dragging);
            Assert.AreEqual(StupidGenius2Drop.None, l.Drop(1, 4), "il dito perso non conta più");
            Assert.AreEqual(StupidGenius2Stage.Wrong, l.Stage);
            Assert.AreEqual(0, l.WrongMoves);
            // rilascio a metà strada (nessun pulsante sotto): torna
            Assert.AreEqual(StupidGenius2Drop.Returned, DragFive(l, 2, None, 0.5f, -1f));
            // poi la soluzione funziona come sempre
            Assert.AreEqual(StupidGenius2Drop.Fixed, DragFive(l, 3, 4));
        }

        [Test]
        public void CancelDuringTheSwapKeepsTheFix()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Drop.Fixed, DragFive(l, 1, 4));
            Assert.IsFalse(l.Cancel());
            Assert.AreEqual(StupidGenius2Stage.Fixing, l.Stage);
            Assert.IsTrue(l.FinishFix());
            Assert.AreEqual(4, l.BoardResult);
        }

        [Test]
        public void AfterTheFixTheBoardIsNoLongerDraggable()
        {
            var l = FixedLogic();
            Assert.AreEqual(StupidGenius2Grab.NotDraggable, l.Grab(1, Res, 0f, 0f));
            Assert.IsFalse(l.Dragging);
            Assert.AreEqual(4, l.BoardResult);
        }

        [Test]
        public void ResetRestoresTheStartFromAnyState()
        {
            var l = FixedLogic();
            l.Answer(3);
            l.Answer(4);
            l.Reset();
            Assert.AreEqual(StupidGenius2Stage.Wrong, l.Stage);
            Assert.AreEqual(5, l.BoardResult);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(StupidGenius2Answer.BoardSaysFive, l.Answer(4));
            l.Reset();
            Assert.AreEqual(StupidGenius2Grab.Dragging, l.Grab(1, Res, 0f, 0f));
            l.Reset();
            Assert.IsFalse(l.Dragging);
            // la vittoria si può rifare dopo un reset (rientro nel livello)
            Assert.AreEqual(StupidGenius2Drop.Fixed, DragFive(l, 2, 4));
            l.FinishFix();
            Assert.AreEqual(StupidGenius2Answer.Correct, l.Answer(4));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void BadNumbersAreIgnored()
        {
            var l = new StupidGenius2Logic();
            Assert.AreEqual(StupidGenius2Grab.None, l.Grab(1, Res, float.NaN, 0f));
            Assert.AreEqual(StupidGenius2Grab.None, l.Grab(StupidGenius2Logic.NoFinger, Res, 0f, 0f));
            Assert.AreEqual(StupidGenius2Grab.Dragging, l.Grab(1, Res, 0f, 0f));
            Assert.IsFalse(l.Move(1, float.PositiveInfinity, 0f));
            Assert.AreEqual(StupidGenius2Answer.None, new StupidGenius2Logic().Answer(None));
        }
    }
}
