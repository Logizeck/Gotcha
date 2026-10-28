using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 261: la stanza buia (le false piste non cambiano niente, le tende aprono il giorno).</summary>
    public class V67DarkRoomTests
    {
        static bool Drag(DarkRoomLogic l, CurtainSide side, float total, int steps = 20)
        {
            bool opened = false;
            for (int i = 0; i < steps; i++) opened |= l.DragCurtain(side, total / steps);
            return opened;
        }

        static void AllWrongMoves(DarkRoomLogic l)
        {
            l.FlipSwitch();
            l.FlipSwitch();
            l.FlipSwitch();
            l.TapLamp();
            l.TryPlug();
            l.TryMatches();
            l.TapRadio();
            l.TapRadio();
            l.TapClock();
            l.TapMouse();
            l.TapCurtain();
        }

        [Test]
        public void StartsDarkAndClosed()
        {
            var l = new DarkRoomLogic();
            Assert.AreEqual(DarkStage.Dark, l.Stage);
            Assert.AreEqual(0f, l.Gap);
            Assert.AreEqual(0f, l.OpenFraction);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SwitchClicksThenRevealsMissingBulb()
        {
            var l = new DarkRoomLogic();
            Assert.AreEqual(SwitchReaction.Nothing, l.FlipSwitch());
            Assert.IsTrue(l.SwitchOn);
            Assert.AreEqual(SwitchReaction.BulbGone, l.FlipSwitch());
            Assert.IsTrue(l.BulbSeen);
            Assert.AreEqual(SwitchReaction.StillNothing, l.FlipSwitch());
            Assert.AreEqual(SwitchReaction.StillNothing, l.FlipSwitch());
            Assert.AreEqual(DarkStage.Dark, l.Stage);
        }

        [Test]
        public void EveryWrongMove_KeepsStateAndProgress()
        {
            var l = new DarkRoomLogic();
            Drag(l, CurtainSide.Right, 0.3f);
            float gap = l.Gap;
            Assert.Greater(gap, 0f);
            AllWrongMoves(l);
            AllWrongMoves(l);
            Assert.AreEqual(DarkStage.Dark, l.Stage);
            Assert.AreEqual(gap, l.Gap, 1e-5);                // la tenda resta dov'era
            Assert.IsTrue(l.CableSeen);
            Assert.AreEqual(2, l.PlugAttempts);
            Assert.AreEqual(2, l.MatchAttempts);
            Assert.AreEqual(4, l.RadioTaps);
            Assert.AreEqual(2, l.ClockTaps);
            Assert.AreEqual(2, l.LampTaps);
            Assert.AreEqual(6, l.SwitchFlips);
            Assert.AreEqual(18, l.WrongMoves);
            Assert.IsFalse(l.TryComplete());
            // e poi si vince lo stesso
            Assert.IsTrue(Drag(l, CurtainSide.Right, 1f));
        }

        [Test]
        public void OneSideIsEnough_Right()
        {
            var l = new DarkRoomLogic();
            Assert.IsTrue(Drag(l, CurtainSide.Right, 1f));
            Assert.AreEqual(DarkStage.Sunlit, l.Stage);
            Assert.AreEqual(0f, l.LeftOpen);
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(DarkStage.Done, l.Stage);
        }

        [Test]
        public void OneSideIsEnough_Left()
        {
            var l = new DarkRoomLogic();
            Assert.IsTrue(Drag(l, CurtainSide.Left, -1f));
            Assert.AreEqual(DarkStage.Sunlit, l.Stage);
            Assert.AreEqual(0f, l.RightOpen);
        }

        [Test]
        public void TwoFingersPullingApart_Win()
        {
            var l = new DarkRoomLogic();
            bool opened = false;
            for (int i = 0; i < 20 && !opened; i++)
            {
                opened |= l.DragCurtain(CurtainSide.Left, -0.03f);
                opened |= l.DragCurtain(CurtainSide.Right, 0.03f);
            }
            Assert.IsTrue(opened);
            Assert.Greater(l.LeftOpen, 0f);
            Assert.Greater(l.RightOpen, 0f);
            Assert.AreEqual(DarkStage.Sunlit, l.Stage);
        }

        [Test]
        public void DraggingInward_DoesNothing()
        {
            var l = new DarkRoomLogic();
            Assert.IsFalse(Drag(l, CurtainSide.Right, -2f));
            Assert.IsFalse(Drag(l, CurtainSide.Left, 2f));
            Assert.AreEqual(0f, l.Gap);
            Assert.AreEqual(DarkStage.Dark, l.Stage);
        }

        [Test]
        public void PartialDrag_Released_StaysAndCanContinue()
        {
            var l = new DarkRoomLogic();
            Assert.IsFalse(Drag(l, CurtainSide.Right, 0.5f));
            Assert.AreEqual(0.5f, l.RightOpen, 1e-4);
            Assert.Less(l.OpenFraction, 1f);
            // il dito si alza: nessuna chiamata, la tenda resta lì (nessun blocco)
            Assert.AreEqual(DarkStage.Dark, l.Stage);
            Assert.IsFalse(l.InputLocked);
            // la si può richiudere un po' e poi riaprire
            Assert.IsFalse(Drag(l, CurtainSide.Right, -0.3f));
            Assert.AreEqual(0.2f, l.RightOpen, 1e-4);
            Assert.IsTrue(Drag(l, CurtainSide.Left, -0.6f));
            Assert.AreEqual(DarkStage.Sunlit, l.Stage);
        }

        [Test]
        public void SlideIsClamped()
        {
            var l = new DarkRoomLogic();
            l.DragCurtain(CurtainSide.Right, 0.69f);
            Assert.AreEqual(DarkStage.Dark, l.Stage);
            l.DragCurtain(CurtainSide.Right, 50f);
            Assert.AreEqual(DarkRoomLogic.MaxSlide, l.RightOpen, 1e-5);
            Assert.AreEqual(1f, l.OpenFraction);
        }

        [Test]
        public void NaNDrag_IsIgnored()
        {
            var l = new DarkRoomLogic();
            Assert.IsFalse(l.DragCurtain(CurtainSide.Left, float.NaN));
            Assert.IsFalse(l.PullCord(float.NaN));
            Assert.AreEqual(0f, l.Gap);
        }

        [Test]
        public void CordPulledDown_OpensBothSides()
        {
            var l = new DarkRoomLogic();
            Assert.IsFalse(l.PullCord(0.5f));                 // in su: niente
            Assert.AreEqual(0f, l.Gap);
            Assert.IsFalse(l.PullCord(-0.2f));
            Assert.AreEqual(l.LeftOpen, l.RightOpen, 1e-5);
            Assert.Greater(l.Gap, 0f);
            bool opened = false;
            for (int i = 0; i < 20 && !opened; i++) opened = l.PullCord(-0.05f);
            Assert.IsTrue(opened);
            Assert.AreEqual(DarkStage.Sunlit, l.Stage);
        }

        [Test]
        public void WinsOnce_AndLocksInput()
        {
            var l = new DarkRoomLogic();
            Assert.IsTrue(Drag(l, CurtainSide.Right, 1f));
            Assert.IsFalse(l.DragCurtain(CurtainSide.Right, 0.1f));   // già aperta: non scatta di nuovo
            Assert.IsFalse(l.DragCurtain(CurtainSide.Left, -0.5f));
            Assert.IsFalse(l.PullCord(-1f));
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(SwitchReaction.None, l.FlipSwitch());
            Assert.AreEqual(0, l.TryPlug());
            Assert.AreEqual(0, l.TryMatches());
            Assert.AreEqual(0, l.TapRadio());
            Assert.AreEqual(0, l.TapClock());
            Assert.AreEqual(0, l.TapLamp());
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.DragCurtain(CurtainSide.Right, -1f));
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = new DarkRoomLogic();
            AllWrongMoves(l);
            Drag(l, CurtainSide.Left, -1f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(DarkStage.Dark, l.Stage);
            Assert.AreEqual(0f, l.Gap);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.IsFalse(l.SwitchOn);
            Assert.IsFalse(l.BulbSeen);
            Assert.IsFalse(l.CableSeen);
            Assert.IsFalse(l.CompletionReported);
            Assert.AreEqual(SwitchReaction.Nothing, l.FlipSwitch());
            Assert.IsTrue(Drag(l, CurtainSide.Right, 1f));            // si può rigiocare
            Assert.IsTrue(l.TryComplete());
        }
    }
}
