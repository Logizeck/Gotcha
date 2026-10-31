using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 270: il telecomando perso (i tasti erano sul fianco del televisore).</summary>
    public class V72LostRemoteTests
    {
        static void AssertStillPaint(LostRemoteLogic l)
        {
            Assert.AreEqual(RemoteStage.Playing, l.Stage);
            Assert.IsTrue(l.PaintDrying);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        /// <summary>Tutte le false piste che non toccano spina e canali.</summary>
        static void SofaAndRemote(LostRemoteLogic l)
        {
            Assert.AreEqual(CushionFind.Coins, l.LiftCushion(0));
            Assert.AreEqual(CushionFind.Sock, l.LiftCushion(1));
            Assert.AreEqual(CushionFind.Mouse, l.LiftCushion(2));
            Assert.AreEqual(CushionFind.Mouse, l.LiftCushion(2));
            Assert.AreEqual(ClockResult.Wrong, l.TapClock());
            Assert.AreEqual(RemoteResult.None, l.PressRemote());   // non ancora trovato
            Assert.AreEqual(CatResult.RollsOver, l.TapCat());
            Assert.AreEqual(CatResult.Sleeps, l.TapCat());
            Assert.AreEqual(RemoteResult.Empty, l.PressRemote());
            Assert.AreEqual(ClockResult.BatteriesTaken, l.TapClock());
            Assert.AreEqual(RemoteResult.Dead, l.PressRemote());
            Assert.AreEqual(ClockResult.Stopped, l.TapClock());
            l.ReadManual();
        }

        [Test]
        public void StartsOnPaintDryingLive()
        {
            var l = new LostRemoteLogic();
            AssertStillPaint(l);
            Assert.AreEqual(LostRemoteLogic.LiveChannel, l.Channel);
            Assert.IsTrue(l.Plugged);
            Assert.IsFalse(l.RemoteFound);
            Assert.IsTrue(l.ClockRunning);
            Assert.AreEqual("PAINT DRYING LIVE — HOUR 37", l.ScreenTitle);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void EachWrongMove_KeepsStateAndCounts()
        {
            var l = new LostRemoteLogic();
            SofaAndRemote(l);
            AssertStillPaint(l);
            Assert.AreEqual(LostRemoteLogic.LiveChannel, l.Channel);
            Assert.AreEqual(1, l.LiftsOf(0));
            Assert.AreEqual(2, l.LiftsOf(2));
            Assert.IsTrue(l.AllCushionsChecked);
            Assert.AreEqual(4, l.CushionLifts);
            Assert.AreEqual(2, l.RemotePresses);   // quello prima di trovarlo non conta
            Assert.AreEqual(3, l.ClockTaps);
            Assert.AreEqual(2, l.CatTaps);
            Assert.IsTrue(l.ManualRead);
            Assert.IsTrue(l.RemoteFound);
            Assert.IsFalse(l.ClockRunning);
        }

        [Test]
        public void RemoteNeverChangesTheChannel()
        {
            var l = new LostRemoteLogic();
            Assert.AreEqual(CatResult.RollsOver, l.TapCat());
            for (int i = 0; i < 20; i++) Assert.AreEqual(RemoteResult.Empty, l.PressRemote());
            l.TapClock();
            for (int i = 0; i < 20; i++) Assert.AreEqual(RemoteResult.Dead, l.PressRemote());
            AssertStillPaint(l);
            Assert.AreEqual(LostRemoteLogic.LiveChannel, l.Channel);
            Assert.AreEqual(RemoteStage.Playing, l.Stage);
        }

        [Test]
        public void ShoutingOrTappingTheScreen_OnlyAddsAnHour()
        {
            var l = new LostRemoteLogic();
            Assert.AreEqual(ShoutResult.NextHour, l.Shout());
            Assert.AreEqual(38, l.LiveHour);
            Assert.AreEqual(ShoutResult.NextHour, l.TapScreen());
            Assert.AreEqual("PAINT DRYING LIVE — HOUR 39", l.ScreenTitle);
            Assert.IsTrue(l.Smack());
            AssertStillPaint(l);
            Assert.AreEqual(1, l.Shouts);
            Assert.AreEqual(1, l.ScreenTaps);
            Assert.AreEqual(1, l.Smacks);
        }

        [Test]
        public void Unplug_BlackScreen_NotAWin_AndReplugRestoresTheSameShow()
        {
            var l = new LostRemoteLogic();
            l.Shout();
            Assert.IsTrue(l.Unplug());
            Assert.IsFalse(l.Unplug());
            Assert.IsFalse(l.Plugged);
            Assert.AreEqual("", l.ScreenTitle);
            Assert.AreEqual(ShoutResult.Off, l.Shout());
            Assert.AreEqual(ShoutResult.Off, l.TapScreen());
            Assert.IsFalse(l.Smack());
            // i tastini senza corrente non fanno niente (né vincono)
            Assert.AreEqual(ChannelResult.NoPower, l.PressChannel(true));
            Assert.AreEqual(ChannelResult.NoPower, l.PressChannel(false));
            Assert.AreEqual(LostRemoteLogic.LiveChannel, l.Channel);
            AssertStillPaint(l);
            Assert.IsTrue(l.Replug());
            Assert.IsFalse(l.Replug());
            Assert.AreEqual("PAINT DRYING LIVE — HOUR 38", l.ScreenTitle);
            Assert.AreEqual(1, l.Unplugs);
            Assert.AreEqual(1, l.Replugs);
        }

        [Test]
        public void ChannelDown_GivesWorseReruns_NeverWins()
        {
            var l = new LostRemoteLogic();
            Assert.AreEqual(ChannelResult.Down, l.PressChannel(false));
            Assert.IsTrue(l.OnRerun);
            Assert.AreEqual("PAINT DRYING — HOUR 36 (RERUN)", l.ScreenTitle);
            Assert.AreEqual(ShoutResult.Rerun, l.Shout());   // una replica non sente
            Assert.AreEqual(37, l.LiveHour);
            Assert.AreEqual(ChannelResult.Down, l.PressChannel(false));
            Assert.AreEqual("PAINT DRYING — HOUR 35 (RERUN)", l.ScreenTitle);
            for (int i = 0; i < 5; i++) Assert.AreEqual(ChannelResult.Bottom, l.PressChannel(false));
            Assert.AreEqual(LostRemoteLogic.LowestChannel, l.Channel);
            AssertStillPaint(l);
            Assert.AreEqual(7, l.ChannelDowns);
        }

        [Test]
        public void ChannelUp_ClimbsBackFromReruns_ThenReachesTheGoodChannel()
        {
            var l = new LostRemoteLogic();
            l.PressChannel(false);
            l.PressChannel(false);
            Assert.AreEqual(ChannelResult.Up, l.PressChannel(true));
            Assert.AreEqual(2, l.Channel);
            Assert.AreEqual(ChannelResult.Up, l.PressChannel(true));
            Assert.AreEqual("PAINT DRYING LIVE — HOUR 37", l.ScreenTitle);
            AssertStillPaint(l);
            Assert.AreEqual(ChannelResult.Good, l.PressChannel(true));
            Assert.IsFalse(l.PaintDrying);
            Assert.AreEqual("CAT DISCO — 24/7", l.ScreenTitle);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ChannelUp_FromTheStart_Wins_Once()
        {
            var l = new LostRemoteLogic();
            Assert.AreEqual(ChannelResult.Good, l.PressChannel(true));
            Assert.AreEqual(RemoteStage.Switching, l.Stage);
            Assert.IsTrue(l.InputLocked);
            // durante il cambio canale niente altro conta
            Assert.AreEqual(ChannelResult.None, l.PressChannel(true));
            Assert.AreEqual(ChannelResult.None, l.PressChannel(false));
            Assert.AreEqual(CushionFind.None, l.LiftCushion(0));
            Assert.AreEqual(CatResult.None, l.TapCat());
            Assert.IsFalse(l.Unplug());
            Assert.AreEqual(ShoutResult.None, l.Shout());
            Assert.AreEqual(LostRemoteLogic.GoodChannel, l.Channel);
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(RemoteStage.Done, l.Stage);
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SolvableAfterEveryWrongMove_AndProgressIsKept()
        {
            var l = new LostRemoteLogic();
            SofaAndRemote(l);
            l.Shout();
            l.Unplug();
            Assert.AreEqual(ChannelResult.NoPower, l.PressChannel(true));
            l.Replug();
            l.PressChannel(false);
            // il gatto resta girato, le pile restano nel telecomando, il libretto resta letto
            Assert.IsTrue(l.RemoteFound);
            Assert.IsTrue(l.ClockBatteriesInRemote);
            Assert.IsTrue(l.ManualRead);
            Assert.AreEqual(ChannelResult.Up, l.PressChannel(true));
            Assert.AreEqual(ChannelResult.Good, l.PressChannel(true));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void TryComplete_OnlyAfterTheGoodChannel()
        {
            var l = new LostRemoteLogic();
            Assert.IsFalse(l.TryComplete());
            l.PressChannel(false);
            l.Unplug();
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(RemoteStage.Playing, l.Stage);
        }

        [Test]
        public void InvalidCushion_IsIgnored()
        {
            var l = new LostRemoteLogic();
            Assert.AreEqual(CushionFind.None, l.LiftCushion(-1));
            Assert.AreEqual(CushionFind.None, l.LiftCushion(3));
            Assert.AreEqual(0, l.CushionLifts);
            Assert.AreEqual(0, l.LiftsOf(7));
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = new LostRemoteLogic();
            SofaAndRemote(l);
            l.Shout();
            l.PressChannel(false);
            l.Unplug();
            l.Replug();
            l.PressChannel(true);
            l.PressChannel(true);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(RemoteStage.Playing, l.Stage);
            Assert.AreEqual(LostRemoteLogic.LiveChannel, l.Channel);
            Assert.AreEqual(LostRemoteLogic.StartHour, l.LiveHour);
            Assert.IsTrue(l.Plugged);
            Assert.IsFalse(l.RemoteFound);
            Assert.IsTrue(l.ClockRunning);
            Assert.IsFalse(l.ManualRead);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Replugs);
            Assert.AreEqual(0, l.ManualReads);
            Assert.IsFalse(l.CompletionReported);
            Assert.AreEqual(ChannelResult.Good, l.PressChannel(true));
            Assert.IsTrue(l.TryComplete());
        }
    }
}
