using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 271: il cassetto bloccato (era push-to-open, la maniglia era finta).</summary>
    public class V73StuckDrawerTests
    {
        static void AssertStillStuck(StuckDrawerLogic l)
        {
            Assert.AreEqual(DrawerStage.Closed, l.Stage);
            Assert.IsFalse(l.DrawerOpen);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TakeKeys());
            Assert.IsFalse(l.TryComplete());
        }

        static void Solve(StuckDrawerLogic l)
        {
            Assert.AreEqual(DrawerPush.Click, l.Press(StuckDrawerLogic.Target, 0.5f));
            Assert.IsTrue(l.FinishOpening());
            Assert.IsTrue(l.TakeKeys());
        }

        [Test]
        public void StartsClosedWithHandle()
        {
            var l = new StuckDrawerLogic();
            AssertStillStuck(l);
            Assert.IsTrue(l.HandleOn);
            Assert.IsFalse(l.Lubed);
            Assert.IsFalse(l.ScrewdriverBent);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void EachWrongMove_KeepsStateAndCounts()
        {
            var l = new StuckDrawerLogic();
            Assert.AreEqual(DrawerPull.Budge, l.PullHandle(0.2f));
            Assert.AreEqual(DrawerToolResult.Squeak, l.UseTool(DrawerTool.Lube));
            Assert.AreEqual(DrawerToolResult.Shinier, l.UseTool(DrawerTool.Lube));
            Assert.AreEqual(DrawerToolResult.Bends, l.UseTool(DrawerTool.Screwdriver));
            Assert.AreEqual(DrawerToolResult.AlreadyBent, l.UseTool(DrawerTool.Screwdriver));
            Assert.AreEqual(DrawerToolResult.Blows, l.UseTool(DrawerTool.Hairdryer));
            Assert.IsTrue(l.Bump());
            Assert.IsTrue(l.Scratch());
            Assert.AreEqual(DrawerPush.Nudge, l.Press(StuckDrawerLogic.Target, 0.1f));
            Assert.AreEqual(DrawerPush.OtherPops, l.Press(2, 1f));
            Assert.AreEqual(DrawerPush.Nudge, l.Press(1, 0f));
            l.ReadNote();
            Assert.IsTrue(l.TapLamp());
            Assert.IsFalse(l.TapLamp());
            Assert.IsTrue(l.TapRug());
            AssertStillStuck(l);
            Assert.IsTrue(l.HandleOn);
            Assert.IsTrue(l.Lubed);
            Assert.IsTrue(l.ScrewdriverBent);
            Assert.IsTrue(l.NoteRead);
            Assert.AreEqual(2, l.Sprays);
            Assert.AreEqual(2, l.Pries);
            Assert.AreEqual(1, l.Blows);
            Assert.AreEqual(2, l.Nudges);
            Assert.AreEqual(1, l.OtherOpens);
            Assert.AreEqual(1 + 2 + 2 + 1 + 1 + 2 + 1 + 1, l.WrongMoves);
        }

        [Test]
        public void HardPull_HandleComesOff_AndGoesBack()
        {
            var l = new StuckDrawerLogic();
            Assert.AreEqual(DrawerPull.HandleOff, l.PullHandle(StuckDrawerLogic.HardPull));
            Assert.IsFalse(l.HandleOn);
            Assert.AreEqual(DrawerPull.NoHandle, l.PullHandle(5f));
            AssertStillStuck(l);
            Assert.IsTrue(l.PutHandleBack());
            Assert.IsFalse(l.PutHandleBack());
            Assert.IsTrue(l.HandleOn);
            Assert.AreEqual(1, l.HandleOffs);
            Assert.AreEqual(1, l.HandleBacks);
            Assert.AreEqual(DrawerPull.Budge, l.PullHandle(0.1f));
        }

        [Test]
        public void ManyWeakPulls_EventuallySnapTheHandle()
        {
            var l = new StuckDrawerLogic();
            for (int i = 0; i < StuckDrawerLogic.PullsBeforeSnap; i++) Assert.AreEqual(DrawerPull.Budge, l.PullHandle(0.1f));
            Assert.AreEqual(DrawerPull.HandleOff, l.PullHandle(0.1f));
            Assert.IsTrue(l.PutHandleBack());
            // dopo averla rimessa si ricomincia a contare
            Assert.AreEqual(DrawerPull.Budge, l.PullHandle(0.1f));
            AssertStillStuck(l);
        }

        [Test]
        public void NothingButAFirmPushOnTheTargetOpensIt()
        {
            var l = new StuckDrawerLogic();
            for (int i = 0; i < 10; i++)
            {
                l.PullHandle(0.3f);
                l.PutHandleBack();
                l.UseTool(DrawerTool.Lube);
                l.UseTool(DrawerTool.Screwdriver);
                l.UseTool(DrawerTool.Hairdryer);
                l.Bump();
                l.Scratch();
                l.Press(StuckDrawerLogic.Target, StuckDrawerLogic.FirmPress * 0.5f);
                l.Press(1, 1f);
                l.Press(2, 1f);
            }
            AssertStillStuck(l);
            Assert.AreEqual(DrawerPush.Click, l.Press(StuckDrawerLogic.Target, StuckDrawerLogic.FirmPress));
            Assert.AreEqual(DrawerStage.Opening, l.Stage);
        }

        [Test]
        public void PushOpens_EvenWithoutHandle()
        {
            var l = new StuckDrawerLogic();
            l.PullHandle(10f);
            Assert.IsFalse(l.HandleOn);
            Solve(l);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void InputLockedWhileOpening_AndOpenWaitsForKeys()
        {
            var l = new StuckDrawerLogic();
            Assert.AreEqual(DrawerPush.Click, l.Press(0, 1f));
            Assert.IsTrue(l.InputLocked);
            Assert.IsFalse(l.TakeKeys());   // ancora in apertura
            Assert.AreEqual(DrawerPull.None, l.PullHandle(5f));
            Assert.AreEqual(DrawerToolResult.None, l.UseTool(DrawerTool.Lube));
            Assert.AreEqual(DrawerPush.None, l.Press(0, 1f));
            Assert.IsFalse(l.Bump());
            Assert.IsFalse(l.TryComplete());
            Assert.IsTrue(l.FinishOpening());
            Assert.IsFalse(l.FinishOpening());
            Assert.AreEqual(DrawerStage.Open, l.Stage);
            Assert.IsTrue(l.DrawerOpen);
            Assert.IsFalse(l.KeysTaken);
            Assert.IsFalse(l.TryComplete());   // le chiavi non sono ancora prese
            Assert.IsTrue(l.TakeKeys());
            Assert.IsTrue(l.KeysTaken);
        }

        [Test]
        public void WinsOnlyOnce()
        {
            var l = new StuckDrawerLogic();
            Solve(l);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TakeKeys());
            Assert.AreEqual(DrawerStage.Done, l.Stage);
            Assert.IsTrue(l.CompletionReported);
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = new StuckDrawerLogic();
            l.PullHandle(5f);
            l.UseTool(DrawerTool.Lube);
            l.UseTool(DrawerTool.Screwdriver);
            l.ReadNote();
            l.TapLamp();
            Solve(l);
            l.TryComplete();
            l.Reset();
            AssertStillStuck(l);
            Assert.IsTrue(l.HandleOn);
            Assert.IsFalse(l.Lubed);
            Assert.IsFalse(l.ScrewdriverBent);
            Assert.IsFalse(l.NoteRead);
            Assert.IsFalse(l.LampOn);
            Assert.IsFalse(l.CompletionReported);
            Assert.AreEqual(0, l.WrongMoves);
            Solve(l);
            Assert.IsTrue(l.TryComplete());
        }
    }
}
