using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 273: il salvadanaio (aveva un tappo sotto la pancia, il martello non serviva).</summary>
    public class V74PiggyBankTests
    {
        static void AssertStillClosed(PiggyBankLogic l)
        {
            Assert.AreEqual(PiggyStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.PlugOut);
            Assert.IsTrue(l.PiggyIntact);
            Assert.Less(l.Cents, PiggyBankLogic.TotalCents);
            Assert.IsFalse(l.TryComplete());
        }

        static void Solve(PiggyBankLogic l)
        {
            if (!l.Flipped) Assert.AreNotEqual(PiggyTurn.Righted, l.Turn());
            Assert.IsTrue(l.Flipped);
            Assert.AreEqual(PiggyPlug.Pulled, l.PullPlug());
        }

        [Test]
        public void StartsUprightAndFull()
        {
            var l = new PiggyBankLogic();
            AssertStillClosed(l);
            Assert.AreEqual(PiggyPose.Upright, l.Pose);
            Assert.AreEqual(0, l.Cents);
            Assert.IsFalse(l.PlugSeen);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void EachWrongMove_KeepsStateAndCounts()
        {
            var l = new PiggyBankLogic();
            Assert.AreEqual(1, l.Hammer());
            Assert.AreEqual(PiggyTrickle.Coin, l.Shake());
            Assert.AreEqual(PiggyKnife.Coin, l.Knife());
            Assert.IsTrue(l.TapSlot());
            Assert.IsTrue(l.TapPiggy());
            Assert.IsTrue(l.TapKid());
            Assert.IsTrue(l.TapTeddy());
            Assert.IsTrue(l.TapWindow());
            l.ReadNote();
            AssertStillClosed(l);
            Assert.AreEqual(PiggyPose.Upright, l.Pose);
            Assert.IsTrue(l.NoteRead);
            Assert.AreEqual(2, l.Cents);
            Assert.AreEqual(4, l.WrongMoves);
        }

        [Test]
        public void HammerNeverBreaksIt()
        {
            var l = new PiggyBankLogic();
            for (int i = 1; i <= 50; i++) Assert.AreEqual(i, l.Hammer());
            AssertStillClosed(l);
            Assert.AreEqual(0, l.Cents);
            l.Turn();
            Assert.AreEqual(51, l.Hammer());
            Assert.IsTrue(l.Flipped);
            AssertStillClosed(l);
        }

        [Test]
        public void TrickleIsSlow_KeepsCoins_NeverEnough()
        {
            var l = new PiggyBankLogic();
            for (int i = 1; i <= PiggyBankLogic.TrickleMax; i++)
            {
                Assert.AreEqual(PiggyTrickle.Coin, l.Shake());
                Assert.AreEqual(i, l.Cents);
            }
            Assert.IsTrue(l.SlotJammed);
            for (int i = 0; i < 100; i++) Assert.AreEqual(PiggyTrickle.Jammed, l.Shake());
            Assert.AreEqual(PiggyKnife.Stuck, l.Knife());
            Assert.AreEqual(PiggyTurn.FlippedNoCoin, l.Turn());
            Assert.AreEqual(PiggyBankLogic.TrickleMax, l.Cents);
            AssertStillClosed(l);
        }

        [Test]
        public void KnifeGivesOneCoinPerTry_AndNothingWhenFlipped()
        {
            var l = new PiggyBankLogic();
            Assert.AreEqual(PiggyKnife.Coin, l.Knife());
            Assert.AreEqual(1, l.Cents);
            Assert.AreEqual(PiggyTurn.Flipped, l.Turn());
            Assert.AreEqual(2, l.Cents);
            Assert.AreEqual(PiggyKnife.SlotUnder, l.Knife());
            Assert.AreEqual(2, l.Cents);
            Assert.AreEqual(2, l.KnifeTries);
            AssertStillClosed(l);
        }

        [Test]
        public void PlugOnlyReachableWhenTurnedOver()
        {
            var l = new PiggyBankLogic();
            Assert.AreEqual(PiggyPlug.Hidden, l.PullPlug());
            Assert.IsFalse(l.TapPlug());
            AssertStillClosed(l);
            Assert.AreEqual(PiggyTurn.Flipped, l.Turn());
            Assert.IsTrue(l.PlugSeen);
            Assert.IsTrue(l.TapPlug());
            Assert.AreEqual(PiggyTurn.Righted, l.Turn());
            Assert.AreEqual(PiggyPose.Upright, l.Pose);
            Assert.AreEqual(PiggyPlug.Hidden, l.PullPlug());
            Assert.IsTrue(l.PlugSeen);
            AssertStillClosed(l);
            Assert.AreEqual(PiggyTurn.Flipped, l.Turn());
            Assert.AreEqual(PiggyPlug.Pulled, l.PullPlug());
            Assert.AreEqual(PiggyStage.Pouring, l.Stage);
            Assert.IsTrue(l.PlugOut);
        }

        [Test]
        public void TurningBackAndForth_IsAlwaysPossible()
        {
            var l = new PiggyBankLogic();
            for (int i = 0; i < 20; i++)
            {
                Assert.AreNotEqual(PiggyTurn.Righted, l.Turn());
                Assert.IsTrue(l.Flipped);
                Assert.AreEqual(PiggyTurn.Righted, l.Turn());
                Assert.IsFalse(l.Flipped);
            }
            Assert.AreEqual(20, l.Flips);
            Assert.AreEqual(20, l.Rights);
            Assert.AreEqual(PiggyBankLogic.TrickleMax, l.Cents);
            AssertStillClosed(l);
        }

        [Test]
        public void InputLockedWhilePouring()
        {
            var l = new PiggyBankLogic();
            l.Shake();
            Solve(l);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(0, l.Hammer());
            Assert.AreEqual(PiggyTrickle.None, l.Shake());
            Assert.AreEqual(PiggyKnife.None, l.Knife());
            Assert.AreEqual(PiggyTurn.None, l.Turn());
            Assert.AreEqual(PiggyPlug.None, l.PullPlug());
            Assert.IsFalse(l.TapSlot());
            Assert.IsFalse(l.TapPlug());
            Assert.IsFalse(l.TapKid());
            Assert.IsTrue(l.Flipped);
        }

        [Test]
        public void WinsOnlyOnce_WithAllTheMoney()
        {
            var l = new PiggyBankLogic();
            l.Shake();
            l.Knife();
            Solve(l);
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(PiggyBankLogic.TotalCents, l.Cents);
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(PiggyPlug.None, l.PullPlug());
            Assert.AreEqual(PiggyStage.Done, l.Stage);
            Assert.IsTrue(l.CompletionReported);
        }

        [Test]
        public void CentsLabel_Formats()
        {
            Assert.AreEqual("0 CENTS", PiggyBankLogic.CentsLabel(0));
            Assert.AreEqual("1 CENT", PiggyBankLogic.CentsLabel(1));
            Assert.AreEqual("12 CENTS", PiggyBankLogic.CentsLabel(12));
            Assert.AreEqual("$12.47", PiggyBankLogic.CentsLabel(1247));
            Assert.AreEqual("$1.05", PiggyBankLogic.CentsLabel(105));
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = new PiggyBankLogic();
            l.Hammer();
            l.Shake();
            l.Knife();
            l.TapSlot();
            l.ReadNote();
            Solve(l);
            l.TryComplete();
            l.Reset();
            AssertStillClosed(l);
            Assert.AreEqual(PiggyPose.Upright, l.Pose);
            Assert.AreEqual(0, l.Cents);
            Assert.AreEqual(0, l.Trickled);
            Assert.IsFalse(l.PlugSeen);
            Assert.IsFalse(l.NoteRead);
            Assert.IsFalse(l.CompletionReported);
            Assert.AreEqual(0, l.WrongMoves);
            Solve(l);
            Assert.IsTrue(l.TryComplete());
        }
    }
}
