using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 258 "The rigged three-card game": nessuna Regina vera, la si fabbrica con l'inchiostro.</summary>
    public class V64RiggedCardsTests
    {
        const float Dt = 0.02f;

        static bool[] HeldExcept(RiggedCardsLogic l, int freeCard)
        {
            var h = new bool[3];
            for (int c = 0; c < 3; c++) h[c] = c != freeCard;
            return h;
        }

        static CardsSlide Run(RiggedCardsLogic l, float seconds, float tilt, bool[] held)
        {
            CardsSlide last = CardsSlide.None;
            for (float t = 0f; t < seconds; t += Dt)
            {
                CardsSlide e = l.Tick(Dt, tilt, held);
                if (e != CardsSlide.None) last = e;
            }
            return last;
        }

        static RiggedCardsLogic Started()
        {
            var l = new RiggedCardsLogic();
            Assert.IsTrue(l.FinishShuffle());
            return l;
        }

        [Test]
        public void Shuffle_IsDeterministic_QueenEndsInTheMiddle()
        {
            var a = Started();
            var b = Started();
            for (int s = 0; s < 3; s++) Assert.AreEqual(a.CardAt(s), b.CardAt(s));
            Assert.AreEqual(RiggedCardsLogic.ShownQueenCard, a.CardAt(RiggedCardsLogic.StainSlot));
            Assert.IsFalse(a.FinishShuffle());
        }

        [Test]
        public void InputLocked_DuringShuffle()
        {
            var l = new RiggedCardsLogic();
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(CardPick.Ignored, l.Pick(0));
            Assert.IsNull(l.TryRearrange());
            Assert.AreEqual(CardsSlide.None, Run(l, 1f, 1f, HeldExcept(l, 0)));
        }

        [Test]
        public void EveryGuess_IsTheTwoOfSpades_NoRandomWin()
        {
            var l = Started();
            for (int i = 0; i < 30; i++)
            {
                CardPick p = l.Pick(i % 3);
                Assert.AreNotEqual(CardPick.Queen, p);
                Assert.AreEqual(i % 3 == RiggedCardsLogic.ShownQueenCard ? CardPick.TwoOfSpadesRemembered : CardPick.TwoOfSpades, p);
            }
            Assert.AreEqual(30, l.Guesses);
            Assert.AreEqual(CardsState.Playing, l.State);
        }

        [Test]
        public void CorrectPath_Completes_Once()
        {
            var l = Started();
            int middle = l.CardAt(RiggedCardsLogic.StainSlot);
            Assert.AreEqual(CardsSlide.FellStain, Run(l, 3f, 0.6f, HeldExcept(l, middle)));
            Assert.IsTrue(l.StainVisible);
            Assert.IsTrue(l.DealerDistracted);
            Assert.IsTrue(l.IsGone(middle));
            int other = l.CardAt(0);
            Assert.IsFalse(l.TransferInk(middle));                       // la carta caduta non c'è più
            Assert.IsTrue(l.TransferInk(other));
            Assert.AreEqual(other, l.QueenCard);
            Assert.IsFalse(l.TransferInk(l.CardAt(2)));                  // l'inchiostro è finito
            Assert.AreEqual(CardPick.TwoOfSpades, l.Pick(l.CardAt(2)));
            Assert.AreEqual(CardPick.Queen, l.Pick(other));
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SlideLeft_AlsoWorks()
        {
            var l = Started();
            int middle = l.CardAt(RiggedCardsLogic.StainSlot);
            Assert.AreEqual(CardsSlide.FellStain, Run(l, 3f, -0.6f, HeldExcept(l, middle)));
        }

        [Test]
        public void WrongFreeCard_FallsWithNothingUnder_DealerReturnsIt()
        {
            var l = Started();
            int left = l.CardAt(0);
            Assert.AreEqual(CardsSlide.FellNothing, Run(l, 3f, -0.6f, HeldExcept(l, left)));
            Assert.AreEqual(CardsState.DealerFetching, l.State);
            Assert.AreEqual(CardPick.Ignored, l.Pick(left));
            Assert.IsTrue(l.ReturnFallenCard());
            Assert.AreEqual(CardsState.Playing, l.State);
            Assert.IsFalse(l.IsGone(left));
            Assert.AreEqual(left, l.CardAt(0));
            // e si può riprovare con quella giusta
            int middle = l.CardAt(RiggedCardsLogic.StainSlot);
            Assert.AreEqual(CardsSlide.FellStain, Run(l, 3f, 0.6f, HeldExcept(l, middle)));
        }

        [Test]
        public void OneOrThreeFingers_NoSlide()
        {
            var l = Started();
            Assert.AreEqual(CardsSlide.None, Run(l, 2f, 0.8f, new[] { true, false, false }));
            Assert.AreEqual(CardsSlide.None, Run(l, 2f, 0.8f, new[] { true, true, true }));
            Assert.AreEqual(CardsSlide.None, Run(l, 2f, 0.8f, new bool[3]));
            Assert.AreEqual(CardsState.Playing, l.State);
        }

        [Test]
        public void SmallTilt_AndJitter_DoNotSlide()
        {
            var l = Started();
            bool[] h = HeldExcept(l, l.CardAt(1));
            Assert.AreEqual(CardsSlide.None, Run(l, 2f, 0.2f, h));
            for (int i = 0; i < 200; i++) l.Tick(Dt, (i % 2 == 0) ? 0.45f : -0.45f, h);
            Assert.AreEqual(CardsState.Playing, l.State);
        }

        [Test]
        public void ReleasedEarly_CardGoesBack_NothingLost()
        {
            var l = Started();
            int middle = l.CardAt(1);
            bool[] h = HeldExcept(l, middle);
            Assert.AreEqual(CardsSlide.Started, Run(l, 0.4f, 0.6f, h));
            Assert.AreEqual(CardsState.Sliding, l.State);
            Assert.Less(System.Math.Abs(l.Offset), RiggedCardsLogic.Commit);
            Assert.AreEqual(CardsSlide.Cancelled, Run(l, 0.1f, 0.6f, new[] { true, false, false }));
            Assert.AreEqual(CardsState.Playing, l.State);
            Assert.AreEqual(0f, l.Offset);
            Assert.IsFalse(l.IsGone(middle));
        }

        [Test]
        public void Committed_CardFalls_EvenIfReleased()
        {
            var l = Started();
            int middle = l.CardAt(1);
            bool[] h = HeldExcept(l, middle);
            for (int i = 0; i < 500 && System.Math.Abs(l.Offset) < RiggedCardsLogic.Commit; i++) l.Tick(Dt, 0.6f, h);
            Assert.AreEqual(CardsSlide.FellStain, Run(l, 3f, 0f, new bool[3]));
        }

        [Test]
        public void TiltStopped_CardStopsWhereItIs()
        {
            var l = Started();
            int middle = l.CardAt(1);
            bool[] h = HeldExcept(l, middle);
            Run(l, 0.25f, 0.6f, h);
            Run(l, 1.5f, 0.05f, h);
            float at = l.Offset;
            Run(l, 1f, 0.05f, h);
            Assert.AreEqual(at, l.Offset, 0.02f);
            Assert.AreEqual(CardsState.Sliding, l.State);
        }

        [Test]
        public void Rearrange_OnlyWhilePlaying_IsDeterministic()
        {
            var l = Started();
            int[] s = l.TryRearrange();
            Assert.AreEqual(RiggedCardsLogic.RearrangeSwaps[0], s);
            Assert.AreEqual(1, l.Rearranges);
            var m = Started();
            m.TryRearrange();
            for (int i = 0; i < 3; i++) Assert.AreEqual(l.CardAt(i), m.CardAt(i));
            // dopo il riordino la macchia è ancora sotto la posizione di mezzo
            int middle = l.CardAt(RiggedCardsLogic.StainSlot);
            Assert.AreEqual(CardsSlide.FellStain, Run(l, 3f, 0.6f, HeldExcept(l, middle)));
            Assert.IsNull(l.TryRearrange());
        }

        [Test]
        public void Shake_SpillsCoins_NoStateChange()
        {
            var l = Started();
            Assert.IsTrue(l.Shake());
            Assert.AreEqual(1, l.Spills);
            Assert.AreEqual(CardsState.Playing, l.State);
        }

        [Test]
        public void InkBeforeExposure_AndPickQueenTooEarly_Fail()
        {
            var l = Started();
            Assert.IsFalse(l.TransferInk(0));
            Assert.AreNotEqual(CardPick.Queen, l.Pick(0));
        }

        [Test]
        public void QueenState_Persists_GuessesOnOtherCardDoNotUndo()
        {
            var l = Started();
            int middle = l.CardAt(1);
            Run(l, 3f, 0.6f, HeldExcept(l, middle));
            int q = l.CardAt(2);
            l.TransferInk(q);
            for (int i = 0; i < 5; i++) Assert.AreEqual(CardPick.TwoOfSpades, l.Pick(l.CardAt(0)));
            Assert.AreEqual(CardsSlide.None, Run(l, 2f, 0.9f, new[] { true, true, false }));
            Assert.AreEqual(CardsState.QueenMade, l.State);
            Assert.IsNull(l.TryRearrange());
            Assert.AreEqual(CardPick.Queen, l.Pick(q));
        }

        [Test]
        public void Paused_ZeroDt_NoMovement()
        {
            var l = Started();
            bool[] h = HeldExcept(l, l.CardAt(1));
            Run(l, 0.3f, 0.6f, h);
            float at = l.Offset;
            for (int i = 0; i < 100; i++) l.Tick(0f, 1f, h);
            Assert.AreEqual(at, l.Offset);
        }

        [Test]
        public void Reset_RestoresStart()
        {
            var l = Started();
            l.Pick(1);
            l.Reset();
            Assert.AreEqual(CardsState.Shuffling, l.State);
            Assert.AreEqual(0, l.Guesses);
            Assert.AreEqual(-1, l.QueenCard);
        }
    }
}
