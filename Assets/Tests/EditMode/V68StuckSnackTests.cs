using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 263: la barretta incastrata (le false piste non costano niente, la A3 sopra la fa cadere).</summary>
    public class V68StuckSnackTests
    {
        const int A = 0, B = 1, C = 2;

        static void AllWrongMoves(StuckSnackLogic l)
        {
            l.Kick();
            l.Kick();
            l.Shake();
            Assert.AreEqual(FlapReaction.ArmStuck, l.ReachFlap());
            l.TapChange();
            Assert.AreNotEqual(VendResult.None, l.InsertCoin());
            Assert.AreEqual(VendResult.SoldOut, l.PressSlot(B, 2));
            Assert.IsTrue(l.CoinInWallet);
            l.InsertCoin();
            Assert.AreEqual(VendResult.Error, l.PressSlot(C, 0));
            Assert.IsTrue(l.CoinInWallet);
        }

        static void AssertStillStuckWithCoin(StuckSnackLogic l)
        {
            Assert.AreEqual(SnackStage.Stuck, l.Stage);
            Assert.IsTrue(l.CoinInWallet);
            Assert.IsFalse(l.CoinSpent);
            Assert.IsFalse(l.HasCredit);
            Assert.IsFalse(l.Solved);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void StartsStuckWithOneCoin()
        {
            var l = new StuckSnackLogic();
            AssertStillStuckWithCoin(l);
            Assert.IsFalse(l.HasPending);
            Assert.IsFalse(l.InputLocked);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual("A3", StuckSnackLogic.SlotName(StuckSnackLogic.SolutionRow, StuckSnackLogic.SolutionCol));
            Assert.AreEqual("B3", StuckSnackLogic.SlotName(StuckSnackLogic.StuckRow, StuckSnackLogic.StuckCol));
            // la soluzione è proprio sopra la barretta incastrata
            Assert.AreEqual(StuckSnackLogic.StuckCol, StuckSnackLogic.SolutionCol);
            Assert.AreEqual(StuckSnackLogic.StuckRow - 1, StuckSnackLogic.SolutionRow);
        }

        [Test]
        public void KickShakeArmAndChange_ChangeNothing()
        {
            var l = new StuckSnackLogic();
            Assert.AreEqual(1, l.Kick());
            Assert.AreEqual(2, l.Kick());
            Assert.AreEqual(1, l.Shake());
            Assert.AreEqual(FlapReaction.ArmStuck, l.ReachFlap());
            Assert.AreEqual(FlapReaction.ArmStuck, l.ReachFlap());
            Assert.AreEqual(1, l.TapChange());
            AssertStillStuckWithCoin(l);
            Assert.AreEqual(2, l.ArmAttempts);
            Assert.AreEqual(6, l.WrongMoves);
        }

        [Test]
        public void WrongMovesWithCreditInside_KeepTheCredit()
        {
            var l = new StuckSnackLogic();
            Assert.AreEqual(VendResult.NeedCoin, l.InsertCoin());
            Assert.IsTrue(l.HasCredit);
            l.Kick();
            l.Shake();
            l.ReachFlap();
            l.TapChange();
            Assert.IsTrue(l.HasCredit);
            Assert.AreEqual(VendResult.None, l.InsertCoin());   // non c'è una seconda moneta
            Assert.AreEqual(VendResult.Vend, l.PressSlot(A, 2));
        }

        [Test]
        public void B3Again_SoldOut_CoinReturned()
        {
            var l = new StuckSnackLogic();
            l.InsertCoin();
            Assert.AreEqual(VendResult.SoldOut, l.PressSlot(B, 2));
            AssertStillStuckWithCoin(l);
            Assert.AreEqual(1, l.SoldOuts);
            Assert.AreEqual(1, l.Refunds);
        }

        [Test]
        public void EveryOtherSlot_ErrorAndRefund()
        {
            var l = new StuckSnackLogic();
            for (int r = 0; r < StuckSnackLogic.Rows; r++)
                for (int c = 0; c < StuckSnackLogic.Cols; c++)
                {
                    if (StuckSnackLogic.IsSolutionSlot(r, c) || StuckSnackLogic.IsStuckSlot(r, c)) continue;
                    Assert.AreEqual(VendResult.NeedCoin, l.InsertCoin());
                    Assert.AreEqual(VendResult.Error, l.PressSlot(r, c), StuckSnackLogic.SlotName(r, c));
                    AssertStillStuckWithCoin(l);
                }
            Assert.AreEqual(7, l.Errors);
            Assert.AreEqual(7, l.Refunds);
        }

        [Test]
        public void PressWithoutCoin_RemembersChoice_NoLoss()
        {
            var l = new StuckSnackLogic();
            Assert.AreEqual(VendResult.NeedCoin, l.PressSlot(C, 1));
            Assert.IsTrue(l.HasPending);
            AssertStillStuckWithCoin(l);
            // la moneta compra la scelta in memoria: errore, rimborso
            Assert.AreEqual(VendResult.Error, l.InsertCoin());
            AssertStillStuckWithCoin(l);
            Assert.IsFalse(l.HasPending);
        }

        [Test]
        public void LastChoiceWins_AndPendingCanExpire()
        {
            var l = new StuckSnackLogic();
            l.PressSlot(C, 1);
            l.PressSlot(B, 2);
            Assert.AreEqual(VendResult.SoldOut, l.InsertCoin());
            l.PressSlot(C, 1);
            l.ClearPending();
            Assert.IsFalse(l.HasPending);
            Assert.AreEqual(VendResult.NeedCoin, l.InsertCoin());
            Assert.IsTrue(l.HasCredit);
        }

        [Test]
        public void InvalidSlots_DoNothing()
        {
            var l = new StuckSnackLogic();
            Assert.AreEqual(VendResult.None, l.PressSlot(-1, 0));
            Assert.AreEqual(VendResult.None, l.PressSlot(3, 0));
            Assert.AreEqual(VendResult.None, l.PressSlot(0, 3));
            Assert.IsFalse(l.HasPending);
            AssertStillStuckWithCoin(l);
            Assert.AreEqual("??", StuckSnackLogic.SlotName(5, 5));
        }

        [Test]
        public void CoinThenA3_Wins_Once()
        {
            var l = new StuckSnackLogic();
            Assert.AreEqual(VendResult.NeedCoin, l.InsertCoin());
            Assert.AreEqual(VendResult.Vend, l.PressSlot(A, 2));
            Assert.AreEqual(SnackStage.Dropping, l.Stage);
            Assert.IsTrue(l.CoinSpent);
            Assert.IsFalse(l.CoinInWallet);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.Solved);
            Assert.IsTrue(l.FinishDrop());
            Assert.IsFalse(l.FinishDrop());
            Assert.AreEqual(SnackStage.InFlap, l.Stage);
            Assert.IsFalse(l.TryComplete());                     // le barrette sono nello sportello ma non ancora prese
            Assert.AreEqual(FlapReaction.Take, l.ReachFlap());
            Assert.AreEqual(FlapReaction.None, l.ReachFlap());
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(SnackStage.Done, l.Stage);
        }

        [Test]
        public void A3ThenCoin_AlsoWins()
        {
            var l = new StuckSnackLogic();
            Assert.AreEqual(VendResult.NeedCoin, l.PressSlot(A, 2));
            Assert.AreEqual(VendResult.Vend, l.InsertCoin());
            Assert.AreEqual(SnackStage.Dropping, l.Stage);
            Assert.IsFalse(l.HasPending);
        }

        [Test]
        public void AfterAllWrongMoves_StillSolvable()
        {
            var l = new StuckSnackLogic();
            AllWrongMoves(l);
            AllWrongMoves(l);
            AssertStillStuckWithCoin(l);
            Assert.AreEqual(VendResult.NeedCoin, l.InsertCoin());
            Assert.AreEqual(VendResult.Vend, l.PressSlot(A, 2));
        }

        [Test]
        public void NothingWorksWhileDropping_OrAfterTheWin()
        {
            var l = new StuckSnackLogic();
            l.InsertCoin();
            l.PressSlot(A, 2);
            Assert.AreEqual(0, l.Kick());
            Assert.AreEqual(0, l.Shake());
            Assert.AreEqual(0, l.TapChange());
            Assert.AreEqual(FlapReaction.None, l.ReachFlap());
            Assert.AreEqual(VendResult.None, l.PressSlot(A, 2));
            Assert.AreEqual(VendResult.None, l.InsertCoin());
            l.FinishDrop();
            // nello sportello: le false piste non contano più, i tasti non fanno niente
            Assert.AreEqual(0, l.Kick());
            Assert.AreEqual(VendResult.None, l.PressSlot(B, 2));
            l.ReachFlap();
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(0, l.Kick());
            Assert.AreEqual(FlapReaction.None, l.ReachFlap());
            Assert.AreEqual(VendResult.None, l.PressSlot(A, 2));
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void CoinCanNeverBeLost()
        {
            var l = new StuckSnackLogic();
            // ogni sequenza di scelte sbagliate rimette la moneta nel portafoglio
            for (int i = 0; i < 50; i++)
            {
                int r = i % 3, c = (i * 7) % 3;
                if (StuckSnackLogic.IsSolutionSlot(r, c)) continue;
                if (i % 2 == 0) { l.InsertCoin(); l.PressSlot(r, c); }
                else { l.PressSlot(r, c); l.InsertCoin(); }
                Assert.IsTrue(l.CoinInWallet, "giro " + i);
                Assert.IsFalse(l.CoinSpent);
            }
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = new StuckSnackLogic();
            AllWrongMoves(l);
            l.PressSlot(C, 0);
            l.InsertCoin();
            l.InsertCoin();
            l.PressSlot(A, 2);
            l.FinishDrop();
            l.ReachFlap();
            l.TryComplete();
            l.Reset();
            AssertStillStuckWithCoin(l);
            Assert.IsFalse(l.HasPending);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Refunds);
            Assert.IsFalse(l.CompletionReported);
            // e si rivince
            l.InsertCoin();
            Assert.AreEqual(VendResult.Vend, l.PressSlot(A, 2));
            l.FinishDrop();
            Assert.AreEqual(FlapReaction.Take, l.ReachFlap());
            Assert.IsTrue(l.TryComplete());
        }
    }
}
