using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 299 "The Impossible Print": la stampante senza carta e il foglio portato da PAPERWORK NIGHTMARE.</summary>
    public class V89LostSheetTests
    {
        static readonly LostSheetTarget[] AllTargets =
            { LostSheetTarget.PrintButton, LostSheetTarget.Tray, LostSheetTarget.Bin, LostSheetTarget.Stapler, LostSheetTarget.Printer };

        static LostSheetLogic WithSheetOnDesk()
        {
            var l = new LostSheetLogic();
            l.Start(true);
            Assert.AreEqual(LostSheetState.SheetArriving, l.State);
            Assert.AreEqual(LostSheetEvent.SheetLanded, l.Tick(LostSheetLogic.ArriveTime + 0.01f));
            Assert.AreEqual(LostSheetState.SheetOnDesk, l.State);
            return l;
        }

        static void PrintAll(LostSheetLogic l)
        {
            Assert.AreEqual(LostSheetEvent.PrintStarted, l.Tick(LostSheetLogic.FeedTime + 0.01f));
            Assert.AreEqual(LostSheetEvent.PageOut, l.Tick(LostSheetLogic.PrintTime + 0.01f));
            Assert.AreEqual(LostSheetState.PageOut, l.State);
        }

        [Test]
        public void WithoutTheFlagThereIsNoPaperAndNothingWins()
        {
            var l = new LostSheetLogic();
            l.Start(false);
            Assert.AreEqual(LostSheetState.NoPaper, l.State);
            Assert.IsFalse(l.HasSheet);
            for (int i = 0; i < 40; i++)
            {
                Assert.AreNotEqual(LostSheetEvent.Ignored, l.Tap(AllTargets[i % AllTargets.Length]));
                l.Shake(30f);
                l.Tick(0.4f);
                Assert.AreEqual(LostSheetEvent.Ignored, l.Grab(), "no sheet to grab");
                Assert.AreEqual(LostSheetEvent.Ignored, l.Release(true));
                Assert.IsFalse(l.TryComplete());
                Assert.AreEqual(LostSheetState.NoPaper, l.State);
            }
        }

        [Test]
        public void EachWrongAttemptHasItsOwnFunnyReaction()
        {
            var l = new LostSheetLogic();
            Assert.AreEqual(LostSheetEvent.PrintNoPaper, l.Tap(LostSheetTarget.PrintButton));
            Assert.AreEqual(LostSheetEvent.TrayEmpty, l.Tap(LostSheetTarget.Tray));
            Assert.AreEqual(LostSheetEvent.BinNothing, l.Tap(LostSheetTarget.Bin));
            Assert.AreEqual(LostSheetEvent.StaplerNothing, l.Tap(LostSheetTarget.Stapler));
            Assert.AreEqual(LostSheetEvent.PrinterBlink, l.Tap(LostSheetTarget.Printer));
            Assert.AreEqual(LostSheetEvent.Rattle, l.Shake(20f));
            Assert.AreEqual("PAPER JAM. Just kidding. NO PAPER.", l.LineFor(LostSheetEvent.PrintNoPaper));
            Assert.AreEqual("Nothing useful in here.", l.LineFor(LostSheetEvent.BinNothing));
            Assert.AreEqual(6, l.Misses);
        }

        [Test]
        public void LinesRotateAndStayShort()
        {
            var l = new LostSheetLogic();
            string a = l.LineFor(LostSheetEvent.PrintNoPaper), b = l.LineFor(LostSheetEvent.PrintNoPaper);
            Assert.AreNotEqual(a, b);
            for (int i = 2; i < LostSheetLogic.PrintLines.Length; i++) l.LineFor(LostSheetEvent.PrintNoPaper);
            Assert.AreEqual(a, l.LineFor(LostSheetEvent.PrintNoPaper), "wraps around");
            foreach (string[] set in new[] { LostSheetLogic.PrintLines, LostSheetLogic.NeedsFeedLines, LostSheetLogic.TrayLines, LostSheetLogic.BinLines,
                                              LostSheetLogic.StaplerLines, LostSheetLogic.PrinterLines, LostSheetLogic.RattleLines })
                foreach (string s in set) Assert.LessOrEqual(s.Length, 40, s);
            Assert.IsNull(l.LineFor(LostSheetEvent.None));
        }

        [Test]
        public void ShakeNeedsStrengthAndHasACooldown()
        {
            var l = new LostSheetLogic();
            Assert.AreEqual(LostSheetEvent.None, l.Shake(LostSheetLogic.ShakeLevel - 1f));
            Assert.AreEqual(LostSheetEvent.Rattle, l.Shake(25f));
            Assert.AreEqual(LostSheetEvent.None, l.Shake(25f));
            l.Tick(LostSheetLogic.ShakeCooldown + 0.01f);
            Assert.AreEqual(LostSheetEvent.Rattle, l.Shake(25f));
        }

        [Test]
        public void NudgeComesOnceAfterEnoughMissesAndOnlyWithoutTheSheet()
        {
            var l = new LostSheetLogic();
            for (int i = 0; i < LostSheetLogic.NudgeAfter - 1; i++) { l.Tap(LostSheetTarget.Tray); Assert.IsFalse(l.TakeNudge()); }
            l.Tap(LostSheetTarget.Bin);
            Assert.IsTrue(l.TakeNudge());
            Assert.IsFalse(l.TakeNudge());
            var s = WithSheetOnDesk();
            for (int i = 0; i < 10; i++) s.Tap(LostSheetTarget.Tray);
            Assert.IsFalse(s.TakeNudge());
        }

        [Test]
        public void SheetArrivesThenCanBeGrabbedOnlyAfterLanding()
        {
            var l = new LostSheetLogic();
            l.Start(true);
            Assert.IsTrue(l.HasSheet);
            Assert.AreEqual(LostSheetEvent.Ignored, l.Grab());
            Assert.AreEqual(LostSheetEvent.None, l.Tick(LostSheetLogic.ArriveTime * 0.5f));
            Assert.AreEqual(0.5f, l.Progress, 0.01f);
            Assert.AreEqual(LostSheetEvent.SheetLanded, l.Tick(LostSheetLogic.ArriveTime));
            Assert.AreEqual(LostSheetLogic.ArriveLine, l.LineFor(LostSheetEvent.SheetLanded));
            Assert.AreEqual(LostSheetEvent.Grabbed, l.Grab());
        }

        [Test]
        public void SheetDroppedElsewhereReturnsAndCanBeRetried()
        {
            var l = WithSheetOnDesk();
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(LostSheetEvent.Grabbed, l.Grab());
                Assert.AreEqual(LostSheetEvent.SheetReturned, l.Release(false));
                Assert.AreEqual(LostSheetState.SheetOnDesk, l.State);
            }
            Assert.AreEqual(LostSheetEvent.PrintNeedsFeed, l.Tap(LostSheetTarget.PrintButton), "PRINT with paper on the desk only teaches");
            Assert.AreEqual(LostSheetState.SheetOnDesk, l.State);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void TapsWhileDraggingAreIgnoredAndTheSheetStaysHeld()
        {
            var l = WithSheetOnDesk();
            l.Grab();
            foreach (LostSheetTarget t in AllTargets) Assert.AreEqual(LostSheetEvent.Ignored, l.Tap(t));
            Assert.AreEqual(LostSheetEvent.Ignored, l.Grab(), "a second grab does nothing");
            Assert.AreEqual(LostSheetState.Dragging, l.State);
        }

        [Test]
        public void SheetIntoThePrinterWinsExactlyOnce()
        {
            var l = WithSheetOnDesk();
            Assert.AreEqual(LostSheetEvent.Grabbed, l.Grab());
            Assert.AreEqual(LostSheetEvent.Fed, l.Release(true));
            Assert.IsTrue(l.InputLocked);
            Assert.IsFalse(l.TryComplete(), "not before the page is out");
            Assert.AreEqual(LostSheetEvent.None, l.Tick(LostSheetLogic.FeedTime * 0.5f));
            Assert.AreEqual(LostSheetEvent.PrintStarted, l.Tick(LostSheetLogic.FeedTime));
            Assert.AreEqual(LostSheetState.Printing, l.State);
            Assert.AreEqual(LostSheetEvent.PageOut, l.Tick(LostSheetLogic.PrintTime + 0.01f));
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(LostSheetState.Won, l.State);
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual("you_rock", LostSheetLogic.EggId);
        }

        [Test]
        public void EverythingIsIgnoredDuringTheFinale()
        {
            var l = WithSheetOnDesk();
            l.Grab();
            l.Release(true);
            foreach (LostSheetTarget t in AllTargets) Assert.AreEqual(LostSheetEvent.Ignored, l.Tap(t));
            Assert.AreEqual(LostSheetEvent.Ignored, l.Shake(40f));
            Assert.AreEqual(LostSheetEvent.Ignored, l.Grab());
            Assert.AreEqual(LostSheetEvent.Ignored, l.Release(false));
            Assert.AreEqual(LostSheetEvent.None, l.Cancel());
            PrintAll(l);
            Assert.AreEqual(LostSheetEvent.Ignored, l.Tap(LostSheetTarget.PrintButton));
            Assert.AreEqual(LostSheetEvent.None, l.Tick(10f), "page out waits for the level");
            Assert.AreEqual(LostSheetState.PageOut, l.State);
        }

        [Test]
        public void RapidGrabReleaseNeverBreaksTheState()
        {
            var l = WithSheetOnDesk();
            for (int i = 0; i < 200; i++)
            {
                l.Grab();
                l.Grab();
                l.Release(false);
                l.Release(false);
                l.Tap(LostSheetTarget.PrintButton);
                Assert.AreEqual(LostSheetState.SheetOnDesk, l.State);
            }
            l.Grab();
            Assert.AreEqual(LostSheetEvent.Fed, l.Release(true));
        }

        [Test]
        public void CancelDuringDragPutsTheSheetBackAndIsHarmlessOtherwise()
        {
            var l = WithSheetOnDesk();
            Assert.AreEqual(LostSheetEvent.None, l.Cancel());
            l.Grab();
            Assert.AreEqual(LostSheetEvent.SheetReturned, l.Cancel());
            Assert.AreEqual(LostSheetState.SheetOnDesk, l.State);
            Assert.AreEqual(LostSheetEvent.Ignored, l.Release(true), "a late release after the cancel does nothing");
            Assert.AreEqual(LostSheetEvent.Grabbed, l.Grab());

            var n = new LostSheetLogic();
            Assert.AreEqual(LostSheetEvent.None, n.Cancel());
            Assert.AreEqual(LostSheetState.NoPaper, n.State);
        }

        [Test]
        public void CancelDuringArrivalKeepsArriving()
        {
            var l = new LostSheetLogic();
            l.Start(true);
            l.Tick(0.3f);
            Assert.AreEqual(LostSheetEvent.None, l.Cancel());
            Assert.AreEqual(LostSheetState.SheetArriving, l.State);
            Assert.AreEqual(LostSheetEvent.SheetLanded, l.Tick(LostSheetLogic.ArriveTime));
        }

        [Test]
        public void RestartResetsEverythingAndReplaysWorkWithTheKeptFlag()
        {
            var l = WithSheetOnDesk();
            for (int i = 0; i < 8; i++) l.Tap(LostSheetTarget.Bin);
            l.Grab();
            l.Release(true);
            PrintAll(l);
            Assert.IsTrue(l.TryComplete());

            l.Start(true);   // la preferenza resta: di nuovo il foglio
            Assert.AreEqual(LostSheetState.SheetArriving, l.State);
            Assert.AreEqual(0, l.Misses);
            Assert.IsFalse(l.Completed);
            l.Tick(LostSheetLogic.ArriveTime + 0.01f);
            l.Grab();
            l.Release(true);
            PrintAll(l);
            Assert.IsTrue(l.TryComplete(), "a replay wins again (once)");
            Assert.IsFalse(l.TryComplete());

            l.Start(false);
            Assert.AreEqual(LostSheetState.NoPaper, l.State);
            Assert.AreEqual("PAPER JAM. Just kidding. NO PAPER.", l.LineFor(LostSheetEvent.PrintNoPaper), "line rotation restarts");
        }

        [Test]
        public void RestartInTheMiddleOfTheFinaleIsValid()
        {
            var l = WithSheetOnDesk();
            l.Grab();
            l.Release(true);
            l.Tick(0.2f);
            l.Start(true);
            Assert.AreEqual(LostSheetState.SheetArriving, l.State);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void NegativeTimeIsIgnored()
        {
            var l = new LostSheetLogic();
            l.Start(true);
            Assert.AreEqual(LostSheetEvent.None, l.Tick(-5f));
            Assert.AreEqual(0f, l.Progress, 1e-5f);
        }

        // ---------- Il gancio in PAPERWORK NIGHTMARE ----------

        const float Edge = 2.8125f, BinX = 2.2f, BinY = -3.05f, BinR = 0.85f;

        [Test]
        public void SheetLeavesOnlyNearTheRightEdge()
        {
            Assert.IsFalse(LostSheetLogic.ShouldLeaveRight(0f, 0f, Edge, BinX, BinY, BinR));
            Assert.IsFalse(LostSheetLogic.ShouldLeaveRight(2.4f, 0f, Edge, BinX, BinY, BinR));
            Assert.IsTrue(LostSheetLogic.ShouldLeaveRight(Edge - LostSheetLogic.EdgeMargin, 0f, Edge, BinX, BinY, BinR));
            Assert.IsTrue(LostSheetLogic.ShouldLeaveRight(3.5f, 1.5f, Edge, BinX, BinY, BinR), "beyond the edge (unclamped finger position)");
            Assert.IsFalse(LostSheetLogic.ShouldLeaveRight(-3.5f, 0f, Edge, BinX, BinY, BinR), "the left edge does nothing");
        }

        [Test]
        public void TheBinWinsOverTheEdgeOnNarrowScreens()
        {
            const float narrow = 2.25f;   // 9:20
            Assert.IsFalse(LostSheetLogic.ShouldLeaveRight(BinX, BinY, narrow, BinX, BinY, BinR), "over the bin mouth: it's a toss");
            Assert.IsFalse(LostSheetLogic.ShouldLeaveRight(2.3f, -2.6f, narrow, BinX, BinY, BinR));
            Assert.IsTrue(LostSheetLogic.ShouldLeaveRight(2.0f, 0.5f, narrow, BinX, BinY, BinR), "higher up on the same edge it leaves");
        }
    }
}
