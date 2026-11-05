using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 15 della roadmap: The Honest Thief (cartellino FREE sulla teca, poi il diamante al ladro).</summary>
    public class V78HonestThiefTests
    {
        static void AssertStillPlaying(HonestThiefLogic l)
        {
            Assert.AreEqual(HonestThiefStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.AreNotEqual(HonestThiefDiamondAt.Thief, l.DiamondAt);
            Assert.IsFalse(l.TryComplete());
        }

        static HonestThiefSignDrop MoveSign(HonestThiefLogic l, bool onCase)
        {
            Assert.IsTrue(l.GrabSign());
            Assert.AreEqual(HonestThiefSignAt.Held, l.SignAt);
            return l.ReleaseSign(onCase);
        }

        static HonestThiefDiamondDrop MoveDiamond(HonestThiefLogic l, bool onThief)
        {
            Assert.AreEqual(HonestThiefGrab.Grabbed, l.GrabDiamond());
            Assert.AreEqual(HonestThiefDiamondAt.Held, l.DiamondAt);
            return l.ReleaseDiamond(onThief);
        }

        static void Run(HonestThiefLogic l, float seconds, float step = 1f / 60f)
        {
            for (float t = 0f; t < seconds; t += step) l.Tick(step);
        }

        [Test]
        public void StartsProtected()
        {
            var l = new HonestThiefLogic();
            AssertStillPlaying(l);
            Assert.AreEqual(HonestThiefSignAt.Home, l.SignAt);
            Assert.AreEqual(HonestThiefDiamondAt.Case, l.DiamondAt);
            Assert.IsFalse(l.CaseIsFree);
            Assert.IsFalse(l.AlarmOn);
            Assert.IsFalse(l.GuardOn);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void TapBeforeSignRingsTheAlarmThatStopsByItself()
        {
            var l = new HonestThiefLogic();
            Assert.AreEqual(HonestThiefTap.Alarm, l.TapCase());
            Assert.IsTrue(l.AlarmOn);
            AssertStillPlaying(l);
            Assert.AreEqual(HonestThiefTick.None, l.Tick(HonestThiefLogic.AlarmSeconds - 0.1f));
            Assert.IsTrue(l.AlarmOn);
            Assert.AreEqual(HonestThiefTick.AlarmOff, l.Tick(0.2f));
            Assert.IsFalse(l.AlarmOn);
            Assert.AreEqual(HonestThiefTick.None, l.Tick(1f), "si spegne una volta sola");
            AssertStillPlaying(l);
        }

        [Test]
        public void RapidTapsJustRestartTheAlarm()
        {
            var l = new HonestThiefLogic();
            Assert.AreEqual(HonestThiefTap.Alarm, l.TapCase());
            for (int i = 0; i < 25; i++)
            {
                l.Tick(0.02f);
                Assert.AreEqual(HonestThiefTap.AlarmAgain, l.TapCase());
                Assert.AreEqual(HonestThiefLogic.AlarmSeconds, l.AlarmLeft, 1e-5f);
            }
            Assert.AreEqual(26, l.Alarms);
            AssertStillPlaying(l);
            Run(l, HonestThiefLogic.AlarmSeconds + 0.1f);
            Assert.IsFalse(l.AlarmOn);
            Assert.AreEqual(HonestThiefTap.Alarm, l.TapCase(), "dopo il silenzio riparte come la prima volta");
        }

        [Test]
        public void DraggingTheDiamondBeforeTheSignCallsTheGuard()
        {
            var l = new HonestThiefLogic();
            Assert.AreEqual(HonestThiefGrab.Guard, l.GrabDiamond());
            Assert.AreEqual(HonestThiefDiamondAt.Case, l.DiamondAt, "il diamante torna subito al suo posto");
            Assert.IsFalse(l.Holding);
            Assert.IsTrue(l.GuardOn);
            Assert.AreEqual(HonestThiefDiamondDrop.None, l.ReleaseDiamond(true), "non era tenuto: niente vittoria");
            AssertStillPlaying(l);
            Assert.AreEqual(HonestThiefTick.None, l.Tick(HonestThiefLogic.GuardSeconds - 0.1f));
            Assert.AreEqual(HonestThiefTick.GuardLeft, l.Tick(0.2f));
            Assert.IsFalse(l.GuardOn);
        }

        [Test]
        public void RepeatedDragAttemptsKeepTheGuardAroundButNeverWin()
        {
            var l = new HonestThiefLogic();
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(HonestThiefGrab.Guard, l.GrabDiamond());
                Assert.AreEqual(HonestThiefDiamondDrop.None, l.ReleaseDiamond(true));
                l.Tick(0.5f);
                Assert.IsTrue(l.GuardOn, "ogni tentativo la trattiene");
            }
            Assert.AreEqual(10, l.GuardCalls);
            AssertStillPlaying(l);
        }

        [Test]
        public void AlarmAndGuardCanEndInTheSameTick()
        {
            var l = new HonestThiefLogic();
            l.TapCase();
            l.GrabDiamond();
            Assert.AreEqual(HonestThiefTick.AlarmOff | HonestThiefTick.GuardLeft, l.Tick(5f));
            Assert.IsFalse(l.AlarmOn);
            Assert.IsFalse(l.GuardOn);
        }

        [Test]
        public void SignDroppedElsewhereReturnsHome()
        {
            var l = new HonestThiefLogic();
            Assert.AreEqual(HonestThiefSignDrop.Returned, MoveSign(l, false));
            Assert.AreEqual(HonestThiefSignAt.Home, l.SignAt);
            Assert.IsFalse(l.CaseIsFree);
            Assert.AreEqual(1, l.SignReturns);
            Assert.AreEqual(HonestThiefGrab.Guard, l.GrabDiamond(), "ancora protetto");
            Assert.IsTrue(l.GrabSign(), "si può riprendere");
            Assert.AreEqual(HonestThiefSignDrop.Returned, l.ReleaseSign(false));
            AssertStillPlaying(l);
        }

        [Test]
        public void SignOnCaseFreesTheDiamondAndLocks()
        {
            var l = new HonestThiefLogic();
            l.TapCase();
            Assert.IsTrue(l.AlarmOn);
            Assert.AreEqual(HonestThiefSignDrop.Placed, MoveSign(l, true));
            Assert.AreEqual(HonestThiefSignAt.Case, l.SignAt);
            Assert.IsTrue(l.CaseIsFree);
            Assert.IsFalse(l.AlarmOn, "la teca FREE fa tacere la sirena");
            Assert.IsFalse(l.GrabSign(), "blocco al posto: il cartellino resta sulla teca");
            Assert.AreEqual(HonestThiefSignDrop.None, l.ReleaseSign(false));
            Assert.AreEqual(HonestThiefSignAt.Case, l.SignAt);
            Assert.AreEqual(HonestThiefTap.Free, l.TapCase(), "niente più allarme");
            Assert.IsFalse(l.AlarmOn);
            AssertStillPlaying(l);
        }

        [Test]
        public void DiamondToThiefWinsExactlyOnce()
        {
            var l = new HonestThiefLogic();
            MoveSign(l, true);
            Assert.AreEqual(HonestThiefDiamondDrop.Taken, MoveDiamond(l, true));
            Assert.AreEqual(HonestThiefDiamondAt.Thief, l.DiamondAt);
            Assert.AreEqual(HonestThiefStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(HonestThiefStage.Done, l.Stage);
        }

        [Test]
        public void DiamondReleasedShortReturnsToTheCase()
        {
            var l = new HonestThiefLogic();
            MoveSign(l, true);
            Assert.AreEqual(HonestThiefDiamondDrop.Returned, MoveDiamond(l, false));
            Assert.AreEqual(HonestThiefDiamondAt.Case, l.DiamondAt);
            Assert.AreEqual(1, l.DiamondReturns);
            AssertStillPlaying(l);
            Assert.AreEqual(HonestThiefDiamondDrop.Taken, MoveDiamond(l, true), "si può riprovare");
        }

        [Test]
        public void ThreeOrMoreWrongAttemptsThenTheSolution()
        {
            var l = new HonestThiefLogic();
            Assert.AreEqual(HonestThiefTap.Alarm, l.TapCase());                    // 1 tocco: allarme
            Assert.AreEqual(HonestThiefGrab.Guard, l.GrabDiamond());               // 2 trascinare: guardia
            Assert.IsTrue(l.TapThief());                                           //   il ladro: "Psst."
            Assert.AreEqual(HonestThiefSignDrop.Returned, MoveSign(l, false));     // 3 cartellino altrove
            Assert.AreEqual(HonestThiefTap.AlarmAgain, l.TapCase());               // 4 ancora allarme
            Assert.IsTrue(l.TapBasket());
            Assert.IsTrue(l.TapSign());
            Assert.AreEqual(4, l.WrongMoves);
            AssertStillPlaying(l);
            Run(l, 3f);
            Assert.AreEqual(HonestThiefSignDrop.Placed, MoveSign(l, true));
            Assert.AreEqual(HonestThiefDiamondDrop.Returned, MoveDiamond(l, false)); // 5 lasciato corto
            Assert.AreEqual(HonestThiefDiamondDrop.Taken, MoveDiamond(l, true));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void OnlyOneThingAtATime()
        {
            var l = new HonestThiefLogic();
            Assert.IsTrue(l.GrabSign());
            Assert.AreEqual(HonestThiefGrab.None, l.GrabDiamond(), "il dito tiene già il cartellino");
            Assert.AreEqual(HonestThiefTap.None, l.TapCase());
            Assert.IsFalse(l.TapSign());
            Assert.IsFalse(l.GrabSign());
            Assert.AreEqual(0, l.GuardCalls);
            Assert.AreEqual(0, l.Alarms);
            l.ReleaseSign(true);
            Assert.AreEqual(HonestThiefGrab.Grabbed, l.GrabDiamond());
            Assert.AreEqual(HonestThiefGrab.None, l.GrabDiamond(), "già in mano");
            Assert.AreEqual(HonestThiefTap.None, l.TapCase());
        }

        [Test]
        public void CancelPutsBackWhatWasHeld()
        {
            var l = new HonestThiefLogic();
            Assert.IsFalse(l.Cancel(), "niente in mano: niente da annullare");
            l.GrabSign();
            Assert.IsTrue(l.Cancel());
            Assert.AreEqual(HonestThiefSignAt.Home, l.SignAt);
            Assert.AreEqual(0, l.SignReturns, "una pausa non è un tentativo sbagliato");
            Assert.AreEqual(HonestThiefSignDrop.None, l.ReleaseSign(true), "rilascio tardivo dopo la pausa: ignorato");
            Assert.AreEqual(HonestThiefSignAt.Home, l.SignAt);

            MoveSign(l, true);
            l.GrabDiamond();
            Assert.IsTrue(l.Cancel());
            Assert.AreEqual(HonestThiefDiamondAt.Case, l.DiamondAt);
            Assert.AreEqual(HonestThiefDiamondDrop.None, l.ReleaseDiamond(true), "rilascio tardivo dopo la pausa: niente vittoria");
            Assert.IsTrue(l.CaseIsFree, "il cartellino resta agganciato");
            AssertStillPlaying(l);
            Assert.AreEqual(HonestThiefDiamondDrop.Taken, MoveDiamond(l, true));
        }

        [Test]
        public void CancelAfterTheWinChangesNothing()
        {
            var l = new HonestThiefLogic();
            MoveSign(l, true);
            MoveDiamond(l, true);
            Assert.IsFalse(l.Cancel());
            Assert.AreEqual(HonestThiefDiamondAt.Thief, l.DiamondAt);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void InputIsIgnoredAfterTheWin()
        {
            var l = new HonestThiefLogic();
            MoveSign(l, true);
            MoveDiamond(l, true);
            Assert.AreEqual(HonestThiefTap.None, l.TapCase());
            Assert.AreEqual(HonestThiefGrab.None, l.GrabDiamond());
            Assert.IsFalse(l.GrabSign());
            Assert.IsFalse(l.TapThief());
            Assert.IsFalse(l.TapBasket());
            Assert.IsFalse(l.TapSign());
            Assert.AreEqual(HonestThiefDiamondDrop.None, l.ReleaseDiamond(true));
            Assert.IsFalse(l.AlarmOn);
        }

        [Test]
        public void ResetRestoresTheStart()
        {
            var l = new HonestThiefLogic();
            l.TapCase();
            l.GrabDiamond();
            MoveSign(l, true);
            MoveDiamond(l, true);
            l.TryComplete();
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(HonestThiefSignAt.Home, l.SignAt);
            Assert.AreEqual(HonestThiefDiamondAt.Case, l.DiamondAt);
            Assert.IsFalse(l.AlarmOn);
            Assert.IsFalse(l.GuardOn);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(HonestThiefGrab.Guard, l.GrabDiamond(), "di nuovo protetto");
            // e si può rivincere (una volta sola anche dopo il reset)
            MoveSign(l, true);
            MoveDiamond(l, true);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ResetWhileHoldingIsClean()
        {
            var l = new HonestThiefLogic();
            l.GrabSign();
            l.Reset();
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(HonestThiefSignDrop.None, l.ReleaseSign(true));
            Assert.IsFalse(l.CaseIsFree);
        }

        [Test]
        public void TickIgnoresZeroAndNegativeTime()
        {
            var l = new HonestThiefLogic();
            l.TapCase();
            Assert.AreEqual(HonestThiefTick.None, l.Tick(0f));
            Assert.AreEqual(HonestThiefTick.None, l.Tick(-5f));
            Assert.AreEqual(HonestThiefLogic.AlarmSeconds, l.AlarmLeft, 1e-5f);
        }

        [Test]
        public void ZonesIncludeTheMarginAndNothingMore()
        {
            // teca: centro (0, -1.8), mezze misure 0.68 x 1.8, margine CaseMargin
            float m = HonestThiefLogic.CaseMargin;
            Assert.IsTrue(HonestThiefLogic.InZone(0f, -1.8f, 0f, -1.8f, 0.68f, 1.8f, m));
            Assert.IsTrue(HonestThiefLogic.InZone(0.68f + m - 0.01f, -1.8f, 0f, -1.8f, 0.68f, 1.8f, m));
            Assert.IsFalse(HonestThiefLogic.InZone(0.68f + m + 0.01f, -1.8f, 0f, -1.8f, 0.68f, 1.8f, m));
            Assert.IsTrue(HonestThiefLogic.InZone(0f, 0f + m - 0.01f, 0f, -1.8f, 0.68f, 1.8f, m));
            Assert.IsFalse(HonestThiefLogic.InZone(0f, 0f + m + 0.01f, 0f, -1.8f, 0.68f, 1.8f, m));
            // mezze misure negative = come positive; margine negativo ignorato
            Assert.IsTrue(HonestThiefLogic.InZone(0.5f, 0f, 0f, 0f, -0.6f, -0.6f, -1f));
            Assert.IsFalse(HonestThiefLogic.InZone(0.65f, 0f, 0f, 0f, -0.6f, -0.6f, -1f));
        }

        [Test]
        public void SceneZonesDoNotOverlap()
        {
            // misure della scena (GotchaLevels78B / V78HonestThiefLevel): il diamante sul cuscino NON è già sul ladro,
            // e il cartellino a casa NON è già sulla teca
            const float caseX = 0f, caseY = -1.8f, caseHW = 0.7f, caseHH = 1.85f;
            const float thiefX = 1.7f, thiefY = -2.15f, thiefHW = 0.55f, thiefHH = 1.5f;
            const float diamondX = 0f, diamondY = -1.3f, signX = -1.85f, signY = -2.1f;
            Assert.IsFalse(HonestThiefLogic.InZone(diamondX, diamondY, thiefX, thiefY, thiefHW, thiefHH, HonestThiefLogic.ThiefMargin));
            Assert.IsFalse(HonestThiefLogic.InZone(caseX + caseHW, diamondY, thiefX, thiefY, thiefHW, thiefHH, HonestThiefLogic.ThiefMargin),
                "il bordo del vetro è ancora 'corto'");
            Assert.IsFalse(HonestThiefLogic.InZone(signX, signY, caseX, caseY, caseHW, caseHH, HonestThiefLogic.CaseMargin));
            Assert.IsTrue(HonestThiefLogic.InZone(thiefX, thiefY, thiefX, thiefY, thiefHW, thiefHH, HonestThiefLogic.ThiefMargin));
        }
    }
}
