using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 275: la poltrona di Toni (ti siedi sull'altra, lui la vuole, la tua si libera).</summary>
    public class V75ToniArmchairTests
    {
        static void AssertToniStillOnYourSeat(ToniArmchairLogic l)
        {
            Assert.AreEqual(ToniArmchairStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.AreEqual(ToniArmchairSpot.Left, l.ToniAt);
            Assert.AreEqual(ToniArmchairSpot.Floor, l.YouAt);
            Assert.IsFalse(l.LeftFree);
            Assert.IsFalse(l.ToniMoving);
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryReportFail());
        }

        static void SwitchSeats(ToniArmchairLogic l)
        {
            Assert.AreEqual(ToniArmchairSit.ToniTakesIt, l.Sit(ToniArmchairSpot.Right));
            Assert.IsTrue(l.ToniArrived());
        }

        [Test]
        public void StartsWithToniAsleepOnYourArmchair()
        {
            var l = new ToniArmchairLogic();
            AssertToniStillOnYourSeat(l);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(ToniArmchairRules.MatchSeconds, l.TimeLeft);
            Assert.AreEqual(60, l.SecondsShown);
            Assert.IsFalse(l.Urgent);
            Assert.AreEqual(ToniArmchairFail.None, l.FailReason);
        }

        [Test]
        public void EachWrongMove_KeepsStateAndCounts()
        {
            var l = new ToniArmchairLogic();
            Assert.AreEqual(1, l.Push());
            Assert.AreEqual(2, l.Push());
            Assert.AreEqual(1, l.GiveTreat());
            Assert.AreEqual(1, l.Squeak());
            Assert.AreEqual(ToniArmchairVacuum.Growl, l.Vacuum());
            Assert.AreEqual(1, l.TapToni());
            Assert.AreEqual(1, l.CoverToni());
            Assert.IsTrue(l.TapRemote());
            AssertToniStillOnYourSeat(l);
            Assert.AreEqual(7, l.WrongMoves);   // il telecomando non è una mossa contro Toni
        }

        [Test]
        public void SittingOnYourOwnSeat_IsOccupied_NothingChanges()
        {
            var l = new ToniArmchairLogic();
            Assert.AreEqual(ToniArmchairSit.Occupied, l.Sit(ToniArmchairSpot.Left));
            Assert.AreEqual(ToniArmchairSit.Occupied, l.Sit(ToniArmchairSpot.Left));
            Assert.AreEqual(2, l.OccupiedTries);
            Assert.AreEqual(ToniArmchairSit.None, l.Sit(ToniArmchairSpot.Floor));
            AssertToniStillOnYourSeat(l);
        }

        [Test]
        public void ItemsOnTheOtherArmchair_AreTheClue_TheyDoNotMoveToni()
        {
            var l = new ToniArmchairLogic();
            foreach (ToniArmchairItem it in new[] { ToniArmchairItem.Blanket, ToniArmchairItem.Mug, ToniArmchairItem.Remote, ToniArmchairItem.Treats, ToniArmchairItem.Squeaky, ToniArmchairItem.Vacuum })
                Assert.AreEqual(ToniArmchairPlace.Suspicious, l.PlaceOnRight(it));
            Assert.AreEqual(6, l.Stares);
            AssertToniStillOnYourSeat(l);
        }

        [Test]
        public void SittingInTheOtherArmchair_MakesToniMove()
        {
            var l = new ToniArmchairLogic();
            Assert.AreEqual(ToniArmchairSit.ToniTakesIt, l.Sit(ToniArmchairSpot.Right));
            Assert.IsTrue(l.ToniMoving);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(ToniArmchairSpot.Right, l.YouAt);
            Assert.AreEqual(0, l.Push());   // mentre salta non si fa altro
            Assert.IsTrue(l.ToniArrived());
            Assert.IsFalse(l.ToniArrived());
            Assert.IsTrue(l.ToniSwitched);
            Assert.IsTrue(l.LeftFree);
            Assert.AreEqual(ToniArmchairSpot.Floor, l.YouAt);   // ti ha buttato giù
            Assert.IsFalse(l.InputLocked);
            Assert.AreEqual(ToniArmchairPlace.Taken, l.PlaceOnRight(ToniArmchairItem.Mug));
            Assert.AreEqual(ToniArmchairSit.Occupied, l.Sit(ToniArmchairSpot.Right));
        }

        [Test]
        public void GoingBackBeforeToniMoved_DoesNothing_NoSoftlock()
        {
            var l = new ToniArmchairLogic();
            l.Sit(ToniArmchairSpot.Right);
            Assert.AreEqual(ToniArmchairSit.Wait, l.Sit(ToniArmchairSpot.Left));
            Assert.AreEqual(ToniArmchairSit.Wait, l.Sit(ToniArmchairSpot.Right));
            Assert.AreEqual(ToniArmchairStage.Playing, l.Stage);
            Assert.IsFalse(l.TryComplete());
            Assert.IsTrue(l.ToniArrived());
            Assert.AreEqual(ToniArmchairSit.Won, l.Sit(ToniArmchairSpot.Left));
        }

        [Test]
        public void GoingBackToYourArmchair_WinsOnce()
        {
            var l = new ToniArmchairLogic();
            l.Push();
            l.Vacuum();
            SwitchSeats(l);
            // le mosse sbagliate dopo il cambio non tolgono il progresso
            l.GiveTreat();
            l.TapToni();
            Assert.IsTrue(l.ToniSwitched);
            Assert.AreEqual(ToniArmchairSit.Won, l.Sit(ToniArmchairSpot.Left));
            Assert.AreEqual(ToniArmchairStage.Settling, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(ToniArmchairSit.None, l.Sit(ToniArmchairSpot.Left));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(ToniArmchairStage.Done, l.Stage);
        }

        [Test]
        public void Countdown_RunsOutAfter60s_FailsOnce()
        {
            var l = new ToniArmchairLogic();
            for (int i = 0; i < 49; i++) Assert.IsFalse(l.Tick(1f));
            Assert.AreEqual(11, l.SecondsShown);
            Assert.IsFalse(l.Urgent);
            Assert.IsFalse(l.Tick(1f));
            Assert.IsTrue(l.Urgent);
            Assert.AreEqual(10, l.SecondsShown);
            for (int i = 0; i < 9; i++) Assert.IsFalse(l.Tick(1f));
            Assert.IsTrue(l.Tick(1.5f));
            Assert.AreEqual(0, l.SecondsShown);
            Assert.IsTrue(l.IsFailed);
            Assert.AreEqual(ToniArmchairFail.Match, l.FailReason);
            Assert.IsFalse(l.Tick(1f));
            Assert.IsTrue(l.TryReportFail());
            Assert.IsFalse(l.TryReportFail());
            Assert.AreEqual(ToniArmchairSit.None, l.Sit(ToniArmchairSpot.Right));
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void Countdown_FailsEvenWhileToniIsMoving_NoWinAfterFail()
        {
            var l = new ToniArmchairLogic();
            l.Tick(58f);
            l.Sit(ToniArmchairSpot.Right);
            Assert.IsTrue(l.Tick(3f));
            Assert.IsFalse(l.ToniArrived());
            Assert.AreEqual(ToniArmchairSit.None, l.Sit(ToniArmchairSpot.Left));
            Assert.IsFalse(l.TryComplete());
            Assert.IsTrue(l.TryReportFail());
        }

        [Test]
        public void SolutionInTime_StopsTheClock_NoFailAfterWin()
        {
            var l = new ToniArmchairLogic();
            l.Tick(50f);
            SwitchSeats(l);
            l.Tick(5f);
            Assert.AreEqual(ToniArmchairSit.Won, l.Sit(ToniArmchairSpot.Left));
            Assert.IsFalse(l.ClockRunning);
            Assert.IsFalse(l.Tick(100f));
            Assert.AreEqual(5f, l.TimeLeft, 1e-4f);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.Tick(100f));
            Assert.AreEqual(ToniArmchairVacuum.None, l.Vacuum());
            Assert.IsFalse(l.TryReportFail());
            Assert.AreEqual(ToniArmchairFail.None, l.FailReason);
        }

        [Test]
        public void Vacuum_IsAThreeStrikeAction()
        {
            var l = new ToniArmchairLogic();
            Assert.AreEqual(ToniArmchairVacuum.Growl, l.Vacuum());
            AssertToniStillOnYourSeat(l);
            Assert.AreEqual(ToniArmchairVacuum.Grumpy, l.Vacuum());
            AssertToniStillOnYourSeat(l);
            Assert.AreEqual(ToniArmchairVacuum.Furious, l.Vacuum());
            Assert.IsTrue(l.IsFailed);
            Assert.AreEqual(ToniArmchairFail.Vacuum, l.FailReason);
            Assert.AreEqual(ToniArmchairVacuum.None, l.Vacuum());
            Assert.IsFalse(l.Tick(100f));   // niente seconda sconfitta
            Assert.AreEqual(ToniArmchairFail.Vacuum, l.FailReason);
            Assert.IsTrue(l.TryReportFail());
            Assert.IsFalse(l.TryReportFail());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SolutionPath_NeverFails()
        {
            var l = new ToniArmchairLogic();
            l.PlaceOnRight(ToniArmchairItem.Blanket);
            l.Tick(20f);
            SwitchSeats(l);
            l.Tick(20f);
            Assert.AreEqual(ToniArmchairSit.Won, l.Sit(ToniArmchairSpot.Left));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.IsFailed);
            Assert.IsFalse(l.TryReportFail());
        }

        [Test]
        public void Reset_RestoresTheStart()
        {
            var l = new ToniArmchairLogic();
            l.Vacuum();
            l.Vacuum();
            l.Tick(30f);
            SwitchSeats(l);
            l.Sit(ToniArmchairSpot.Left);
            l.TryComplete();
            l.Reset();
            AssertToniStillOnYourSeat(l);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Stares);
            Assert.AreEqual(ToniArmchairRules.MatchSeconds, l.TimeLeft);
            Assert.AreEqual(ToniArmchairVacuum.Growl, l.Vacuum());
            Assert.IsFalse(l.CompletionReported);
            Assert.IsFalse(l.FailReported);
        }
    }
}
