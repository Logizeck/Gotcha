using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 272: il calzino perso (era nella guarnizione dell'oblò della lavatrice).</summary>
    public class V73MissingSockTests
    {
        static void AssertStillPlaying(MissingSockLogic l)
        {
            Assert.AreEqual(SockStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.Paired);
            Assert.IsFalse(l.TryComplete());
        }

        /// <summary>Tutte le false piste, una dopo l'altra.</summary>
        static void AllWrongMoves(MissingSockLogic l)
        {
            for (int i = 0; i < MissingSockLogic.BasketSocks; i++) Assert.AreEqual(SockPairResult.Wrong, l.TryPair(i));
            Assert.AreEqual(SockDryerResult.OpenedLint, l.ToggleDryer());
            Assert.IsTrue(l.TapLint());
            Assert.AreEqual(SockDryerResult.Closed, l.ToggleDryer());
            Assert.IsFalse(l.TapLint());   // asciugatrice chiusa: il coniglietto non si vede
            Assert.AreEqual(SockPetResult.RevealsSock, l.TapPet());
            Assert.AreEqual(SockPetResult.Sleeps, l.TapPet());
            Assert.AreEqual(SockPairResult.Close, l.TryPair(MissingSockLogic.PetSock));
            Assert.IsTrue(l.Shake());
            Assert.IsTrue(l.TapRackSock());
            Assert.IsTrue(l.TapDetergent());
        }

        [Test]
        public void StartsClosedAndUnsolved()
        {
            var l = new MissingSockLogic();
            AssertStillPlaying(l);
            Assert.IsFalse(l.WasherOpen);
            Assert.IsFalse(l.DryerOpen);
            Assert.IsFalse(l.PetRevealed);
            Assert.IsFalse(l.SockFound);
            Assert.IsTrue(l.PeekVisible);
            Assert.IsFalse(l.SealReachable);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void EachWrongMove_KeepsStateAndCounts()
        {
            var l = new MissingSockLogic();
            AllWrongMoves(l);
            AssertStillPlaying(l);
            Assert.IsFalse(l.SockFound);
            Assert.IsTrue(l.PeekVisible);
            Assert.IsTrue(l.PetRevealed);
            Assert.AreEqual(MissingSockLogic.BasketSocks, l.WrongPairs);
            Assert.AreEqual(1, l.ClosePairs);
            Assert.AreEqual(1, l.DryerOpens);
            Assert.AreEqual(1, l.LintTaps);
            Assert.AreEqual(2, l.PetTaps);
            Assert.AreEqual(1, l.Shakes);
            Assert.AreEqual(MissingSockLogic.BasketSocks + 1 + 1 + 1 + 2 + 1 + 1 + 1, l.WrongMoves);
        }

        [Test]
        public void OpeningTheWasher_LooksEmpty_AndKeepsState()
        {
            var l = new MissingSockLogic();
            Assert.AreEqual(SockDrumResult.None, l.TapDrum());   // sportello chiuso: il cestello non si tocca
            Assert.AreEqual(SockWasherResult.OpenedEmpty, l.ToggleWasher());
            Assert.IsTrue(l.WasherOpen);
            Assert.AreEqual(SockDrumResult.Empty, l.TapDrum());
            Assert.AreEqual(SockDrumResult.Empty, l.TapDrum());
            Assert.AreEqual(SockWasherResult.Closed, l.ToggleWasher());
            Assert.AreEqual(SockWasherResult.OpenedEmpty, l.ToggleWasher());
            AssertStillPlaying(l);
            Assert.IsFalse(l.SockFound);
            Assert.AreEqual(2, l.WasherOpens);
            Assert.AreEqual(1, l.WasherCloses);
            Assert.AreEqual(2, l.DrumTaps);
        }

        [Test]
        public void Seal_OnlyReachableWithTheDoorOpen()
        {
            var l = new MissingSockLogic();
            for (int i = 0; i < 5; i++) Assert.AreEqual(SockSealResult.Blocked, l.PullSeal());
            Assert.IsFalse(l.SockFound);
            Assert.IsTrue(l.PeekVisible);
            Assert.AreEqual(5, l.BlockedSealTries);
            Assert.IsFalse(l.CanHold(MissingSockLogic.MatchSock));
            Assert.AreEqual(SockPairResult.None, l.TryPair(MissingSockLogic.MatchSock));
            l.ToggleWasher();
            Assert.IsTrue(l.SealReachable);
            Assert.AreEqual(SockSealResult.Found, l.PullSeal());
            Assert.IsTrue(l.SockFound);
            Assert.IsFalse(l.PeekVisible);
            Assert.AreEqual(SockSealResult.Nothing, l.PullSeal());
            // richiudere lo sportello non rimette il calzino nella guarnizione
            l.ToggleWasher();
            Assert.IsTrue(l.SockFound);
            Assert.IsTrue(l.CanHold(MissingSockLogic.MatchSock));
            AssertStillPlaying(l);
        }

        [Test]
        public void PetSock_OnlyAfterThePetMoves_AndNeverMatches()
        {
            var l = new MissingSockLogic();
            Assert.IsFalse(l.CanHold(MissingSockLogic.PetSock));
            Assert.AreEqual(SockPairResult.None, l.TryPair(MissingSockLogic.PetSock));
            l.TapPet();
            for (int i = 0; i < 10; i++) Assert.AreEqual(SockPairResult.Close, l.TryPair(MissingSockLogic.PetSock));
            AssertStillPlaying(l);
        }

        [Test]
        public void Matching_OnlyWithTheRightSock()
        {
            var l = new MissingSockLogic();
            l.ToggleWasher();
            l.PullSeal();
            l.TapPet();
            for (int i = 0; i < MissingSockLogic.BasketSocks; i++) Assert.AreEqual(SockPairResult.Wrong, l.TryPair(i));
            Assert.AreEqual(SockPairResult.Close, l.TryPair(MissingSockLogic.PetSock));
            AssertStillPlaying(l);
            Assert.AreEqual(SockPairResult.Match, l.TryPair(MissingSockLogic.MatchSock));
            Assert.AreEqual(SockStage.Matching, l.Stage);
            Assert.IsTrue(l.Paired);
            Assert.IsTrue(l.InputLocked);
        }

        [Test]
        public void InvalidSocks_AreIgnored()
        {
            var l = new MissingSockLogic();
            Assert.AreEqual(SockPairResult.None, l.TryPair(-1));
            Assert.AreEqual(SockPairResult.None, l.TryPair(MissingSockLogic.MatchSock + 1));
            Assert.AreEqual(SockPairResult.None, l.TryPair(99));
            Assert.IsFalse(l.CanHold(-3));
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void FullSolution_AfterWrongMoves_KeepsPartialProgress()
        {
            var l = new MissingSockLogic();
            AllWrongMoves(l);
            Assert.AreEqual(SockSealResult.Blocked, l.PullSeal());
            l.ToggleWasher();
            l.TapDrum();
            Assert.AreEqual(SockSealResult.Found, l.PullSeal());
            // altre false piste dopo il ritrovamento: il calzino resta trovato
            l.ToggleWasher();
            l.ToggleDryer();
            l.Shake();
            l.TryPair(0);
            Assert.IsTrue(l.SockFound);
            Assert.IsTrue(l.PetRevealed);
            Assert.IsTrue(l.DryerOpen);
            AssertStillPlaying(l);
            Assert.AreEqual(SockPairResult.Match, l.TryPair(MissingSockLogic.MatchSock));
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(SockStage.Done, l.Stage);
        }

        [Test]
        public void Win_OnlyOnce_AndInputLockedDuringTheParty()
        {
            var l = new MissingSockLogic();
            Assert.IsFalse(l.TryComplete());
            l.ToggleWasher();
            l.PullSeal();
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(SockPairResult.Match, l.TryPair(MissingSockLogic.MatchSock));
            // durante la festa niente fa niente
            Assert.AreEqual(SockWasherResult.None, l.ToggleWasher());
            Assert.AreEqual(SockSealResult.None, l.PullSeal());
            Assert.AreEqual(SockDryerResult.None, l.ToggleDryer());
            Assert.AreEqual(SockPetResult.None, l.TapPet());
            Assert.IsFalse(l.Shake());
            Assert.IsFalse(l.TapRackSock());
            Assert.IsFalse(l.TapDetergent());
            Assert.AreEqual(SockPairResult.None, l.TryPair(0));
            Assert.AreEqual(SockPairResult.None, l.TryPair(MissingSockLogic.MatchSock));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(SockStage.Done, l.Stage);
            Assert.AreEqual(SockPairResult.None, l.TryPair(MissingSockLogic.MatchSock));
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = new MissingSockLogic();
            AllWrongMoves(l);
            l.ToggleWasher();
            l.TapDrum();
            l.PullSeal();
            l.ToggleDryer();
            l.TryPair(MissingSockLogic.MatchSock);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.IsFalse(l.WasherOpen);
            Assert.IsFalse(l.DryerOpen);
            Assert.IsFalse(l.PetRevealed);
            Assert.IsFalse(l.SockFound);
            Assert.IsTrue(l.PeekVisible);
            Assert.IsFalse(l.CompletionReported);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.SealTries);
            Assert.AreEqual(0, l.WasherCloses);
            Assert.AreEqual(SockSealResult.Blocked, l.PullSeal());
            l.ToggleWasher();
            Assert.AreEqual(SockSealResult.Found, l.PullSeal());
            Assert.AreEqual(SockPairResult.Match, l.TryPair(MissingSockLogic.MatchSock));
            Assert.IsTrue(l.TryComplete());
        }
    }
}
