using System;
using System.Collections.Generic;
using NUnit.Framework;
using Gotcha.Levels.Halloween;

namespace Gotcha.Tests
{
    /// <summary>Livello 249: la parola BRAAAINS → BRAIN e lo zombie vegetariano.</summary>
    public class HalloweenZombieTests
    {
        // B R A A A I N S
        // 0 1 2 3 4 5 6 7

        [Test]
        public void Word_StartsAsBraaains()
        {
            var w = new ZombieWord();
            Assert.AreEqual("BRAAAINS", w.Text);
            Assert.IsFalse(w.IsTarget);
            Assert.AreEqual(8, w.VisibleCount);
        }

        [TestCase(2, 3)]
        [TestCase(3, 4)]
        [TestCase(2, 4)]
        public void Word_AnyTwoAsPlusS_GiveBrain(int a, int b)
        {
            var w = new ZombieWord();
            Assert.IsTrue(w.TryRemove(a));
            Assert.IsTrue(w.TryRemove(b));
            Assert.IsFalse(w.IsTarget);
            Assert.IsTrue(w.TryRemove(7));
            Assert.AreEqual("BRAIN", w.Text);
            Assert.IsTrue(w.IsTarget);
        }

        [Test]
        public void Word_WrongRemovals_AreNotBrain_AndUndoRecovers()
        {
            var w = new ZombieWord();
            w.TryRemove(0);   // RAAAINS
            w.TryRemove(2);
            w.TryRemove(7);
            Assert.AreEqual("RAAIN", w.Text);
            Assert.IsFalse(w.IsTarget);
            Assert.AreEqual(7, w.Undo());
            Assert.AreEqual(2, w.Undo());
            Assert.AreEqual(0, w.Undo());
            Assert.AreEqual(-1, w.Undo());
            Assert.AreEqual("BRAAAINS", w.Text);
        }

        [Test]
        public void Word_NoDuplicateRemoval_NeverEmpty()
        {
            var w = new ZombieWord();
            Assert.IsTrue(w.TryRemove(5));
            Assert.IsFalse(w.TryRemove(5));
            Assert.IsFalse(w.TryRemove(-1));
            Assert.IsFalse(w.TryRemove(8));
            for (int i = 0; i < 8; i++) w.TryRemove(i);
            Assert.AreEqual(1, w.VisibleCount);
            Assert.AreEqual(7, w.RemovedCount);
        }

        [Test]
        public void Word_IsCaseAndOrderExact()
        {
            var w = new ZombieWord("BRAAAINS", "BRAIN");
            w.TryRemove(1); w.TryRemove(3); w.TryRemove(4);   // BAINS
            Assert.IsFalse(w.IsTarget);
            CollectionAssert.AreEqual(new List<int> { 0, 2, 5, 6, 7 }, w.VisibleIndices());
        }

        static ZombieLogic MakeBrain()
        {
            var l = new ZombieLogic();
            Assert.IsTrue(l.TryTapLetter(3));
            Assert.IsTrue(l.TryTapLetter(4));
            Assert.IsTrue(l.TryTapLetter(7));
            Assert.IsTrue(l.BalloonReady);
            return l;
        }

        [Test]
        public void CorrectPath_Completes()
        {
            ZombieLogic l = MakeBrain();
            Assert.IsTrue(l.TryPickBalloon());
            Assert.IsTrue(l.TryDropBalloon(true));
            Assert.AreEqual(ZombieState.Devouring, l.State);
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.AreEqual(ZombieState.Done, l.State);
        }

        [Test]
        public void Foods_RefusedOrEaten_NoWin()
        {
            var l = new ZombieLogic();
            Assert.AreEqual(ZombieFeedResult.Refused, l.TryFeed(ZombieFood.Brain));
            Assert.AreEqual(ZombieFeedResult.Ignored, l.TryFeed(ZombieFood.Burger));   // sta ancora reagendo
            l.TryEndReaction();
            Assert.AreEqual(ZombieFeedResult.Refused, l.TryFeed(ZombieFood.Burger));
            l.TryEndReaction();
            Assert.AreEqual(ZombieFeedResult.Eaten, l.TryFeed(ZombieFood.Broccoli));
            l.TryEndReaction();
            Assert.AreEqual(ZombieState.Hungry, l.State);
            Assert.IsFalse(l.TryCompleteEnding());
        }

        [Test]
        public void Broccoli_NeverRunsOut()
        {
            var l = new ZombieLogic();
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(ZombieFeedResult.Eaten, l.TryFeed(ZombieFood.Broccoli));
                Assert.GreaterOrEqual(l.Broccoli, 1);
                l.TryEndReaction();
            }
            Assert.AreEqual(10, l.BroccoliEaten);
            // il percorso giusto resta possibile
            l.TryTapLetter(2); l.TryTapLetter(3); l.TryTapLetter(7);
            Assert.IsTrue(l.TryPickBalloon());
            Assert.IsTrue(l.TryDropBalloon(true));
        }

        [Test]
        public void OriginalBalloon_CannotBeDragged()
        {
            var l = new ZombieLogic();
            Assert.IsFalse(l.TryPickBalloon());
            Assert.IsFalse(l.TryDropBalloon(true));
            l.TryTapLetter(7);   // BRAAAIN: non ancora
            Assert.IsFalse(l.TryPickBalloon());
        }

        [Test]
        public void BalloonDroppedElsewhere_ReturnsAndStaysReady()
        {
            ZombieLogic l = MakeBrain();
            l.TryPickBalloon();
            Assert.IsFalse(l.TryDropBalloon(false));
            Assert.AreEqual(ZombieState.Hungry, l.State);
            Assert.IsTrue(l.BalloonReady);
        }

        [Test]
        public void LettersLocked_WhenBrain_UndoUnlocks()
        {
            ZombieLogic l = MakeBrain();
            Assert.IsFalse(l.TryTapLetter(0));   // non si rovina toccando mentre si trascina
            Assert.AreEqual(7, l.TryUndo());
            Assert.IsFalse(l.BalloonReady);
            Assert.IsTrue(l.TryTapLetter(0));
        }

        [Test]
        public void NoSoftlock_UndoAlwaysRecovers()
        {
            var l = new ZombieLogic();
            for (int i = 0; i < 8; i++) l.TryTapLetter(i);
            while (l.TryUndo() >= 0) { }
            Assert.AreEqual("BRAAAINS", l.Word.Text);
            l.TryTapLetter(2); l.TryTapLetter(4); l.TryTapLetter(7);
            Assert.IsTrue(l.BalloonReady);
        }

        [Test]
        public void LockedDuringReactionAndEnding()
        {
            var l = new ZombieLogic();
            l.TryFeed(ZombieFood.Brain);
            Assert.IsFalse(l.TryTapLetter(0));
            Assert.AreEqual(-1, l.TryUndo());
            l.TryEndReaction();
            l = MakeBrain();
            l.TryPickBalloon();
            l.TryDropBalloon(true);
            Assert.AreEqual(ZombieFeedResult.Ignored, l.TryFeed(ZombieFood.Broccoli));
            Assert.AreEqual(-1, l.TryUndo());
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.IsFalse(l.TryCompleteEnding());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (ZombieState s in Enum.GetValues(typeof(ZombieState)))
            {
                var l = new ZombieLogic();
                switch (s)
                {
                    case ZombieState.Reacting: l.TryFeed(ZombieFood.Broccoli); break;
                    case ZombieState.HoldingBalloon: l = MakeBrain(); l.TryPickBalloon(); break;
                    case ZombieState.Devouring: l = MakeBrain(); l.TryPickBalloon(); l.TryDropBalloon(true); break;
                    case ZombieState.Done: l = MakeBrain(); l.TryPickBalloon(); l.TryDropBalloon(true); l.TryCompleteEnding(); break;
                }
                Assert.AreEqual(s, l.State);
                l.Reset();
                Assert.AreEqual(ZombieState.Hungry, l.State);
                Assert.AreEqual("BRAAAINS", l.Word.Text);
                Assert.AreEqual(ZombieLogic.StartBroccoli, l.Broccoli);
                Assert.IsFalse(l.CompletionReported);
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(41, 42, 43, 44, 45, 46, 47, 48)] int seed)
        {
            var rnd = new Random(seed);
            var l = new ZombieLogic();
            int completions = 0;
            for (int step = 0; step < 800; step++)
            {
                string before = l.Word.Text;
                bool wasTarget = l.Word.IsTarget;
                switch (rnd.Next(8))
                {
                    case 0: case 1: l.TryTapLetter(rnd.Next(-1, 9)); break;
                    case 2: l.TryUndo(); break;
                    case 3: l.TryFeed((ZombieFood)rnd.Next(3)); break;
                    case 4: l.TryEndReaction(); break;
                    case 5: if (l.TryPickBalloon()) Assert.IsTrue(wasTarget); break;
                    case 6:
                        if (l.TryDropBalloon(rnd.Next(2) == 0)) Assert.AreEqual("BRAIN", before);
                        if (l.TryCompleteEnding()) completions++;
                        break;
                    default: if (rnd.Next(80) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.GreaterOrEqual(l.Broccoli, 1);
                Assert.GreaterOrEqual(l.Word.VisibleCount, 1);
                Assert.AreEqual(8, l.Word.VisibleCount + l.Word.RemovedCount);
                Assert.AreEqual(l.State == ZombieState.Done, l.CompletionReported);
                if (l.State == ZombieState.HoldingBalloon || l.State == ZombieState.Devouring || l.State == ZombieState.Done)
                    Assert.AreEqual("BRAIN", l.Word.Text);
            }
        }
    }
}
