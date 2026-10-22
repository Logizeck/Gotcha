using System;
using NUnit.Framework;
using Gotcha.Levels.Halloween;

namespace Gotcha.Tests
{
    /// <summary>Livello 245: logica del lupo-carlino (scatola, biscotto unico, luna coperta).</summary>
    public class HalloweenWerepugTests
    {
        [Test]
        public void CorrectPath_Completes()
        {
            var l = new WerepugLogic();
            Assert.IsTrue(l.TryOpenBox());
            Assert.IsTrue(l.TryPickBiscuit());
            Assert.IsTrue(l.TryDropBiscuit(true));
            Assert.AreEqual(WerepugState.Transforming, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryCompleteTransform());
            Assert.AreEqual(WerepugState.Done, l.State);
        }

        [Test]
        public void WrongDrop_ReturnsBiscuit()
        {
            var l = new WerepugLogic();
            l.TryOpenBox();
            l.TryPickBiscuit();
            Assert.IsFalse(l.TryDropBiscuit(false));
            Assert.AreEqual(WerepugState.BiscuitReady, l.State);
            Assert.IsTrue(l.TryPickBiscuit());   // si può riprovare
        }

        [Test]
        public void OnlyOneBiscuit_Ever()
        {
            var l = new WerepugLogic();
            Assert.IsTrue(l.TryOpenBox());
            Assert.IsFalse(l.TryOpenBox());
            l.TryPickBiscuit();
            l.TryDropBiscuit(false);
            Assert.IsFalse(l.TryOpenBox());
            Assert.AreEqual(1, l.BiscuitsOut);
        }

        [Test]
        public void PrematureCompletion_Impossible()
        {
            var l = new WerepugLogic();
            Assert.IsFalse(l.TryCompleteTransform());
            Assert.IsFalse(l.TryPickBiscuit());           // scatola chiusa
            Assert.IsFalse(l.TryDropBiscuit(true));       // niente in mano
            l.TryOpenBox();
            Assert.IsFalse(l.TryCompleteTransform());
            Assert.IsFalse(l.TryDropBiscuit(true));
        }

        [Test]
        public void Completion_ReportedOnce()
        {
            var l = new WerepugLogic();
            l.TryOpenBox(); l.TryPickBiscuit(); l.TryDropBiscuit(true);
            Assert.IsTrue(l.TryCompleteTransform());
            Assert.IsFalse(l.TryCompleteTransform());
            Assert.IsFalse(l.TryPickBiscuit());
            Assert.IsFalse(l.CanGrowl);
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (WerepugState target in Enum.GetValues(typeof(WerepugState)))
            {
                var l = new WerepugLogic();
                if (target >= WerepugState.BiscuitReady) l.TryOpenBox();
                if (target >= WerepugState.Holding) l.TryPickBiscuit();
                if (target >= WerepugState.Transforming) l.TryDropBiscuit(true);
                if (target >= WerepugState.Done) l.TryCompleteTransform();
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(WerepugState.BoxClosed, l.State);
                Assert.AreEqual(0, l.BiscuitsOut);
                Assert.IsFalse(l.CompletionReported);
                Assert.IsTrue(l.TryOpenBox());
            }
        }

        [Test]
        public void Covers_GeometryIsTolerantButNotTrivial()
        {
            Assert.IsTrue(WerepugLogic.Covers(1f, 3f, 0.8f, 1f, 3f, 0.8f));
            Assert.IsTrue(WerepugLogic.Covers(1.4f, 3.2f, 0.8f, 1f, 3f, 0.8f));     // un po' spostato
            Assert.IsFalse(WerepugLogic.Covers(2.2f, 3f, 0.8f, 1f, 3f, 0.8f));      // troppo lontano
            Assert.IsFalse(WerepugLogic.Covers(1f, 3f, 0.3f, 1f, 3f, 0.8f));        // troppo piccolo
            Assert.IsFalse(WerepugLogic.Covers(1f, 3f, 0.8f, 1f, 3f, 0f));
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(1, 2, 3, 4, 5, 6, 7, 8)] int seed)
        {
            var rnd = new Random(seed);
            var l = new WerepugLogic();
            int completions = 0;
            for (int step = 0; step < 400; step++)
            {
                switch (rnd.Next(6))
                {
                    case 0: l.TryOpenBox(); break;
                    case 1: l.TryPickBiscuit(); break;
                    case 2: l.TryDropBiscuit(rnd.Next(4) == 0); break;
                    case 3: if (l.TryCompleteTransform()) completions++; break;
                    case 4: if (rnd.Next(50) == 0) { l.Reset(); completions = 0; } break;
                    default: _ = l.CanGrowl; break;
                }
                Assert.LessOrEqual(l.BiscuitsOut, 1);
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == WerepugState.Done, l.CompletionReported);
                if (l.State == WerepugState.BoxClosed) Assert.AreEqual(0, l.BiscuitsOut);
                else Assert.AreEqual(1, l.BiscuitsOut);
            }
        }
    }
}
