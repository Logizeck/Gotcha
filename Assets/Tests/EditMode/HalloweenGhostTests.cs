using System;
using NUnit.Framework;
using Gotcha.Levels.Halloween;

namespace Gotcha.Tests
{
    /// <summary>Livello 247: il fantasma che non fa paura (lenzuolo dal fantasma all'uomo).</summary>
    public class HalloweenGhostTests
    {
        [Test]
        public void CorrectPath_Completes()
        {
            var l = new GhostLogic();
            Assert.IsFalse(l.GhostInvisible);
            Assert.IsTrue(l.TryPickSheet());
            Assert.IsTrue(l.GhostInvisible);
            Assert.IsTrue(l.TryDropSheet(GhostDropTarget.Man));
            Assert.AreEqual(GhostState.Ending, l.State);
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.AreEqual(GhostState.Done, l.State);
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void Works_WhateverAlarmAndLamp(bool alarm, bool lampToggled)
        {
            var l = new GhostLogic();
            if (alarm) Assert.IsTrue(l.TryRingAlarm());
            if (lampToggled) Assert.IsTrue(l.TryToggleLamp());
            l.TryPickSheet();
            Assert.IsTrue(l.TryDropSheet(GhostDropTarget.Man));
            Assert.IsTrue(l.TryCompleteEnding());
        }

        [Test]
        public void WrongPaths_DoNotWin()
        {
            var l = new GhostLogic();
            for (int i = 0; i < 5; i++) Assert.IsTrue(l.TryBoo());
            Assert.IsTrue(l.TryRingAlarm());
            Assert.IsTrue(l.ManAwake);
            Assert.IsTrue(l.TryToggleLamp());
            Assert.IsFalse(l.TryCompleteEnding());
            // lenzuolo lasciato per terra: resta disponibile
            l.TryPickSheet();
            Assert.IsFalse(l.TryDropSheet(GhostDropTarget.Nowhere));
            Assert.AreEqual(GhostState.SheetLoose, l.State);
            Assert.IsFalse(l.TryBoo());   // invisibile: niente BOO
            Assert.IsTrue(l.TryPickSheet());
            // rimesso sul fantasma
            Assert.IsFalse(l.TryDropSheet(GhostDropTarget.Ghost));
            Assert.AreEqual(GhostState.Covered, l.State);
            Assert.IsFalse(l.TryCompleteEnding());
        }

        [Test]
        public void SingleSheet_NoDoublePick()
        {
            var l = new GhostLogic();
            Assert.IsTrue(l.TryPickSheet());
            Assert.IsFalse(l.TryPickSheet());
            Assert.IsFalse(l.TryDropSheet(GhostDropTarget.Nowhere));
            Assert.IsFalse(l.TryDropSheet(GhostDropTarget.Man));   // non ce l'ha in mano
        }

        [Test]
        public void Completion_ReportedOnce_InputLocked()
        {
            var l = new GhostLogic();
            l.TryPickSheet();
            l.TryDropSheet(GhostDropTarget.Man);
            Assert.IsFalse(l.TryPickSheet());
            Assert.IsFalse(l.TryRingAlarm());
            Assert.IsFalse(l.TryToggleLamp());
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.IsFalse(l.TryCompleteEnding());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (GhostState s in Enum.GetValues(typeof(GhostState)))
            {
                var l = new GhostLogic();
                l.TryRingAlarm(); l.TryToggleLamp(); l.TryBoo();
                switch (s)
                {
                    case GhostState.SheetLoose: l.TryPickSheet(); l.TryDropSheet(GhostDropTarget.Nowhere); break;
                    case GhostState.HoldingSheet: l.TryPickSheet(); break;
                    case GhostState.Ending: l.TryPickSheet(); l.TryDropSheet(GhostDropTarget.Man); break;
                    case GhostState.Done: l.TryPickSheet(); l.TryDropSheet(GhostDropTarget.Man); l.TryCompleteEnding(); break;
                }
                Assert.AreEqual(s, l.State);
                l.Reset();
                Assert.AreEqual(GhostState.Covered, l.State);
                Assert.IsFalse(l.ManAwake);
                Assert.IsTrue(l.LampOn);
                Assert.AreEqual(0, l.Boos);
                Assert.IsFalse(l.CompletionReported);
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(21, 22, 23, 24, 25, 26)] int seed)
        {
            var rnd = new Random(seed);
            var l = new GhostLogic();
            int completions = 0;
            for (int step = 0; step < 500; step++)
            {
                GhostState before = l.State;
                switch (rnd.Next(8))
                {
                    case 0: l.TryBoo(); break;
                    case 1: l.TryRingAlarm(); break;
                    case 2: l.TryManSleeps(); break;
                    case 3: l.TryToggleLamp(); break;
                    case 4: l.TryPickSheet(); break;
                    case 5:
                        bool ending = l.TryDropSheet((GhostDropTarget)rnd.Next(3));
                        if (ending) Assert.AreEqual(GhostState.HoldingSheet, before);
                        break;
                    case 6: if (l.TryCompleteEnding()) completions++; break;
                    default: if (rnd.Next(60) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == GhostState.Done, l.CompletionReported);
                Assert.AreEqual(l.State != GhostState.Covered, l.GhostInvisible);
            }
        }
    }
}
