using System;
using NUnit.Framework;
using Gotcha.Levels.Halloween;

namespace Gotcha.Tests
{
    /// <summary>Livello 248: la strega e la pozione impossibile (la parola BEAUTIFUL nel calderone).</summary>
    public class HalloweenWitchTests
    {
        [Test]
        public void CorrectPath_Completes()
        {
            var l = new WitchLogic();
            Assert.IsTrue(l.TryPickWord());
            Assert.IsFalse(l.WordInSentence);
            Assert.IsTrue(l.TryDropWord(true));
            Assert.AreEqual(WitchState.Brewing, l.State);
            Assert.IsTrue(l.TryCompleteBrew());
            Assert.AreEqual(WitchState.Done, l.State);
        }

        [Test]
        public void NoIngredientCombination_Wins()
        {
            var l = new WitchLogic();
            foreach (WitchIngredient a in Enum.GetValues(typeof(WitchIngredient)))
                foreach (WitchIngredient b in Enum.GetValues(typeof(WitchIngredient)))
                {
                    Assert.IsTrue(l.TryAddIngredient(a));
                    Assert.AreEqual(a, l.Reaction);
                    Assert.IsFalse(l.TryCompleteBrew());
                    Assert.IsTrue(l.TryEndReaction());
                    Assert.IsTrue(l.TryAddIngredient(b));
                    Assert.IsTrue(l.TryEndReaction());
                    Assert.AreEqual(WitchState.Idle, l.State);
                }
            Assert.AreEqual(50, l.IngredientsTried);
        }

        [Test]
        public void IngredientDuringReaction_IsRejected_NotBlocking()
        {
            var l = new WitchLogic();
            Assert.IsTrue(l.TryAddIngredient(WitchIngredient.Toad));
            Assert.IsFalse(l.TryAddIngredient(WitchIngredient.Apple));
            Assert.IsTrue(l.TryEndReaction());
            Assert.IsTrue(l.TryAddIngredient(WitchIngredient.Apple));
        }

        [Test]
        public void WordDroppedOutside_ReturnsToSentence()
        {
            var l = new WitchLogic();
            l.TryPickWord();
            Assert.IsFalse(l.TryDropWord(false));
            Assert.IsTrue(l.WordInSentence);
            Assert.AreEqual(WitchState.Idle, l.State);
        }

        [Test]
        public void WordDuringReaction_CannotComplete()
        {
            var l = new WitchLogic();
            l.TryAddIngredient(WitchIngredient.Eye);
            Assert.IsTrue(l.TryPickWord());
            Assert.IsFalse(l.TryDropWord(true));
            Assert.IsTrue(l.WordInSentence);
            Assert.AreEqual(WitchState.Reacting, l.State);
            l.TryEndReaction();
            l.TryPickWord();
            Assert.IsTrue(l.TryDropWord(true));
        }

        [Test]
        public void SingleWord_NoDuplicatePick()
        {
            var l = new WitchLogic();
            Assert.IsTrue(l.TryPickWord());
            Assert.IsFalse(l.TryPickWord());
            Assert.IsFalse(l.TryDropWord(false));
            Assert.IsFalse(l.TryDropWord(true));   // già tornata nella frase
        }

        [Test]
        public void PrematureCompletion_Impossible_AndOnce()
        {
            var l = new WitchLogic();
            Assert.IsFalse(l.TryCompleteBrew());
            Assert.IsFalse(l.TryDropWord(true));
            l.TryPickWord();
            l.TryDropWord(true);
            Assert.IsFalse(l.TryAddIngredient(WitchIngredient.Heart));
            Assert.IsFalse(l.TryPickWord());
            Assert.IsTrue(l.TryCompleteBrew());
            Assert.IsFalse(l.TryCompleteBrew());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (WitchState s in Enum.GetValues(typeof(WitchState)))
            {
                var l = new WitchLogic();
                switch (s)
                {
                    case WitchState.Reacting: l.TryAddIngredient(WitchIngredient.Mushroom); l.TryPickWord(); break;
                    case WitchState.Brewing: l.TryPickWord(); l.TryDropWord(true); break;
                    case WitchState.Done: l.TryPickWord(); l.TryDropWord(true); l.TryCompleteBrew(); break;
                }
                Assert.AreEqual(s, l.State);
                l.Reset();
                Assert.AreEqual(WitchState.Idle, l.State);
                Assert.IsTrue(l.WordInSentence);
                Assert.IsNull(l.Reaction);
                Assert.IsFalse(l.CompletionReported);
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(31, 32, 33, 34, 35, 36)] int seed)
        {
            var rnd = new Random(seed);
            var l = new WitchLogic();
            int completions = 0;
            for (int step = 0; step < 500; step++)
            {
                WitchState before = l.State;
                switch (rnd.Next(6))
                {
                    case 0: l.TryAddIngredient((WitchIngredient)rnd.Next(5)); break;
                    case 1: l.TryEndReaction(); break;
                    case 2: l.TryPickWord(); break;
                    case 3: if (l.TryDropWord(rnd.Next(2) == 0)) Assert.AreEqual(WitchState.Idle, before); break;
                    case 4: if (l.TryCompleteBrew()) completions++; break;
                    default: if (rnd.Next(60) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == WitchState.Reacting, l.Reaction.HasValue);
                Assert.AreEqual(l.State == WitchState.Done, l.CompletionReported);
            }
        }
    }
}
