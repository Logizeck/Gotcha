using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 274: la cipolla che fa piangere (va messa in frigo prima di tagliarla).</summary>
    public class V74CryingOnionTests
    {
        static void AssertStillPlaying(CryingOnionLogic l)
        {
            Assert.AreEqual(OnionStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        /// <summary>Mette la cipolla in frigo, chiude e aspetta tutto il tempo: torna fredda e col frigo chiuso.</summary>
        static void ChillFully(CryingOnionLogic l)
        {
            Assert.AreEqual(OnionFridgeResult.OpenedEmpty, l.TapFridge());
            Assert.AreEqual(OnionDropResult.InFridge, l.DropOnion(true));
            Assert.AreEqual(OnionFridgeResult.ClosedChilling, l.TapFridge());
            for (int i = 0; i < 4; i++) Assert.AreEqual(OnionTick.None, l.Tick(1f));
            Assert.AreEqual(OnionTick.Chilled, l.Tick(1.01f));
            Assert.IsTrue(l.Cold);
        }

        /// <summary>Tutte le false piste, una dopo l'altra (la cipolla resta tiepida sul tagliere).</summary>
        static void AllWrongMoves(CryingOnionLogic l)
        {
            Assert.AreEqual(OnionChopResult.Tears, l.Chop());
            Assert.AreEqual(OnionGogglesResult.Fogged, l.Goggles());
            Assert.IsTrue(l.ToggleFan());
            Assert.AreEqual(OnionChopResult.Tears, l.Chop());   // lacrime di lato, ma lacrime
            Assert.AreEqual(OnionBreathResult.Gasp, l.HoldBreath(0.5f));
            Assert.AreEqual(OnionBreathResult.Blue, l.HoldBreath(CryingOnionLogic.BlueAfter + 0.1f));
            Assert.AreEqual(OnionRemedyResult.Applied, l.LightCandle());
            Assert.AreEqual(OnionRemedyResult.Already, l.LightCandle());
            Assert.AreEqual(OnionRemedyResult.Applied, l.Bread());
            Assert.AreEqual(OnionRemedyResult.Already, l.Bread());
            Assert.AreEqual(OnionChopResult.Tears, l.Chop());   // "Grandma lied."
            Assert.IsTrue(l.TapCook());
            Assert.IsTrue(l.TapKnife());
            Assert.IsTrue(l.ReadNote());
        }

        [Test]
        public void StartsWarmOnTheBoard()
        {
            var l = new CryingOnionLogic();
            AssertStillPlaying(l);
            Assert.AreEqual(OnionSpot.Board, l.Spot);
            Assert.IsFalse(l.Cold);
            Assert.IsFalse(l.FridgeOpen);
            Assert.IsFalse(l.FanOn);
            Assert.IsFalse(l.RemedyActive);
            Assert.IsTrue(l.CanGrabOnion);
            Assert.IsTrue(l.EyesWatery);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void EachWrongMove_KeepsStateAndCounts()
        {
            var l = new CryingOnionLogic();
            AllWrongMoves(l);
            AssertStillPlaying(l);
            Assert.AreEqual(OnionSpot.Board, l.Spot);
            Assert.IsFalse(l.Cold);
            Assert.IsTrue(l.FanOn);
            Assert.IsTrue(l.CandleLit);
            Assert.IsTrue(l.BreadInMouth);
            Assert.AreEqual(3, l.WarmChops);
            Assert.AreEqual(2, l.SidewaysChops);
            Assert.AreEqual(1, l.GrandmaChops);
            Assert.AreEqual(1, l.GogglesTries);
            Assert.AreEqual(2, l.BreathHolds);
            Assert.AreEqual(1, l.BlueFaces);
            Assert.AreEqual(1, l.NoteReads);
            Assert.AreEqual(3 + 1 + 1 + 2 + 2 + 2 + 1 + 1, l.WrongMoves);
        }

        [Test]
        public void WarmOnion_AlwaysMakesTears_WhateverTheRemedies()
        {
            var l = new CryingOnionLogic();
            for (int mask = 0; mask < 16; mask++)
            {
                l.Reset();
                if ((mask & 1) != 0) l.ToggleFan();
                if ((mask & 2) != 0) l.LightCandle();
                if ((mask & 4) != 0) l.Bread();
                if ((mask & 8) != 0) { l.Goggles(); l.HoldBreath(5f); }
                for (int i = 0; i < 3; i++) Assert.AreEqual(OnionChopResult.Tears, l.Chop(), "mask " + mask);
                AssertStillPlaying(l);
            }
        }

        [Test]
        public void ChillingNeedsTheDoorClosed_AndOpeningEarlyKeepsTheChill()
        {
            var l = new CryingOnionLogic();
            Assert.AreEqual(OnionDropResult.OnBoard, l.DropOnion(true));   // frigo chiuso: resta sul tagliere
            Assert.AreEqual(OnionSpot.Board, l.Spot);
            Assert.AreEqual(OnionFridgeResult.OpenedEmpty, l.TapFridge());
            Assert.AreEqual(OnionDropResult.InFridge, l.DropOnion(true));
            Assert.IsFalse(l.EyesWatery);   // la cipolla è via: occhi asciutti
            Assert.AreEqual(OnionTick.None, l.Tick(10f));   // sportello aperto: niente freddo
            Assert.AreEqual(0f, l.Chill);
            Assert.AreEqual(OnionFridgeResult.ClosedChilling, l.TapFridge());
            Assert.IsTrue(l.Chilling);
            Assert.IsFalse(l.CanGrabOnion);   // chiusa nel frigo
            Assert.AreEqual(OnionTick.None, l.Tick(2f));
            Assert.AreEqual(OnionFridgeResult.OpenedWarm, l.TapFridge());   // troppo presto
            Assert.AreEqual(1, l.EarlyOpens);
            Assert.IsFalse(l.Cold);
            Assert.IsTrue(l.CanGrabOnion);
            Assert.AreEqual(OnionTick.None, l.Tick(10f));   // aperto: il freddo non cresce ma resta
            Assert.AreEqual(2f, l.Chill, 1e-4f);
            Assert.AreEqual(0.4f, l.ChillProgress, 1e-4f);
            Assert.AreEqual(OnionFridgeResult.ClosedChilling, l.TapFridge());   // richiuso: riprende da 2 s
            Assert.AreEqual(OnionTick.None, l.Tick(2.9f));
            Assert.AreEqual(OnionTick.Chilled, l.Tick(0.2f));
            AssertStillPlaying(l);
            Assert.IsTrue(l.Cold);
        }

        [Test]
        public void TakingTheOnionOutEarly_KeepsTheChillForNextTime()
        {
            var l = new CryingOnionLogic();
            l.TapFridge();
            l.DropOnion(true);
            l.TapFridge();
            l.Tick(1.5f);
            Assert.AreEqual(OnionFridgeResult.OpenedWarm, l.TapFridge());
            Assert.AreEqual(OnionDropResult.OnBoard, l.DropOnion(false));
            Assert.AreEqual(OnionChopResult.Tears, l.Chop());   // ancora tiepida
            Assert.AreEqual(OnionTick.None, l.Tick(60f));
            Assert.AreEqual(1.5f, l.Chill, 1e-4f);
            Assert.AreEqual(OnionDropResult.InFridge, l.DropOnion(true));   // il frigo è rimasto aperto
            Assert.AreEqual(OnionFridgeResult.ClosedChilling, l.TapFridge());
            l.Tick(1.5f);
            Assert.AreEqual(OnionFridgeResult.Rushed, l.TapFridge());   // 1.5 + 1.5 = 3 s: basta
            Assert.IsTrue(l.Cold);
            AssertStillPlaying(l);
        }

        [Test]
        public void WaitingFiveSeconds_MakesItCold()
        {
            var l = new CryingOnionLogic();
            ChillFully(l);
            Assert.AreEqual(1f, l.ChillProgress);
            Assert.AreEqual(OnionFridgeResult.OpenedCold, l.TapFridge());
            Assert.IsTrue(l.CanGrabOnion);
            Assert.AreEqual(OnionDropResult.OnBoard, l.DropOnion(false));
            Assert.AreEqual(0f, CryingOnionLogic.FumeStrength(l.Cold, false, 10f));
            Assert.IsFalse(l.EyesWatery);
            AssertStillPlaying(l);
        }

        [Test]
        public void OpeningTheFridgeAfterThreeSeconds_FastForwards()
        {
            var l = new CryingOnionLogic();
            l.TapFridge();
            l.DropOnion(true);
            l.TapFridge();
            l.Tick(CryingOnionLogic.RushAfter + 0.01f);
            Assert.IsFalse(l.Cold);
            Assert.AreEqual(OnionFridgeResult.Rushed, l.TapFridge());
            Assert.IsTrue(l.Cold);
            Assert.IsTrue(l.FridgeOpen);
            Assert.AreEqual(1, l.TimesChilled);
            Assert.AreEqual(0, l.EarlyOpens);
            Assert.AreEqual(OnionDropResult.OnBoard, l.DropOnion(false));
            Assert.AreEqual(OnionChopResult.Clean, l.Chop());
        }

        [Test]
        public void ColdOnionInTheFridge_ClosingAndReopening()
        {
            var l = new CryingOnionLogic();
            ChillFully(l);
            Assert.AreEqual(OnionFridgeResult.OpenedCold, l.TapFridge());
            Assert.AreEqual(OnionFridgeResult.Closed, l.TapFridge());
            Assert.IsFalse(l.Chilling);
            Assert.AreEqual(OnionFridgeResult.OpenedCold, l.TapFridge());
            Assert.AreEqual(OnionDropResult.InFridge, l.DropOnion(true));   // rimessa dentro: resta fredda
            Assert.IsTrue(l.Cold);
            AssertStillPlaying(l);
        }

        [Test]
        public void WarmChopTakesNothingAway_AfterAChilledOnionWarmsUp()
        {
            var l = new CryingOnionLogic();
            ChillFully(l);
            l.TapFridge();
            l.DropOnion(false);
            Assert.IsTrue(l.Cold);
            Assert.AreEqual(OnionTick.None, l.Tick(CryingOnionLogic.ColdLasts - 1f));
            Assert.IsTrue(l.Cold);
            Assert.AreEqual(OnionTick.WarmedUp, l.Tick(1.5f));
            Assert.IsFalse(l.Cold);
            Assert.AreEqual(0f, l.Chill);
            Assert.IsTrue(l.EyesWatery);
            Assert.AreEqual(1, l.TimesWarmed);
            Assert.AreEqual(OnionChopResult.Tears, l.Chop());   // tiepida di nuovo: lacrime
            AssertStillPlaying(l);
            // si può rifare: il frigo è ancora aperto
            Assert.AreEqual(OnionDropResult.InFridge, l.DropOnion(true));
            Assert.AreEqual(OnionFridgeResult.ClosedChilling, l.TapFridge());
            Assert.AreEqual(OnionTick.Chilled, l.Tick(CryingOnionLogic.ChillSeconds));
            Assert.AreEqual(2, l.TimesChilled);
            Assert.AreEqual(OnionFridgeResult.OpenedCold, l.TapFridge());
            l.DropOnion(false);
            Assert.AreEqual(OnionChopResult.Clean, l.Chop());
        }

        [Test]
        public void ColdOnionStaysColdInsideTheFridge()
        {
            var l = new CryingOnionLogic();
            ChillFully(l);
            Assert.AreEqual(OnionTick.None, l.Tick(CryingOnionLogic.ColdLasts * 3f));
            Assert.IsTrue(l.Cold);
            Assert.AreEqual(OnionFridgeResult.OpenedCold, l.TapFridge());
            Assert.AreEqual(OnionTick.None, l.Tick(CryingOnionLogic.ColdLasts * 3f));   // anche a sportello aperto
            Assert.IsTrue(l.Cold);
        }

        [Test]
        public void ChoppingWhileTheOnionIsInTheFridge_CutsAir()
        {
            var l = new CryingOnionLogic();
            l.TapFridge();
            l.DropOnion(true);
            Assert.AreEqual(OnionChopResult.NoOnion, l.Chop());
            Assert.AreEqual(1, l.AirChops);
            AssertStillPlaying(l);
            Assert.AreEqual(OnionSpot.Fridge, l.Spot);
        }

        [Test]
        public void FumesShrinkNearTheFridge_AndVanishWhenColdOrHidden()
        {
            Assert.AreEqual(1f, CryingOnionLogic.FumeStrength(false, false, 5f));
            float mid = CryingOnionLogic.FumeStrength(false, false, CryingOnionLogic.FumeNear * 0.5f);
            Assert.Less(mid, 1f);
            Assert.Greater(mid, CryingOnionLogic.FumeMin);
            Assert.AreEqual(CryingOnionLogic.FumeMin, CryingOnionLogic.FumeStrength(false, false, 0f), 1e-5f);
            Assert.AreEqual(0f, CryingOnionLogic.FumeStrength(true, false, 5f));
            Assert.AreEqual(0f, CryingOnionLogic.FumeStrength(false, true, 5f));
        }

        [Test]
        public void ColdChop_WinsOnce()
        {
            var l = new CryingOnionLogic();
            AllWrongMoves(l);
            ChillFully(l);
            l.TapFridge();
            l.DropOnion(false);
            Assert.AreEqual(OnionChopResult.Clean, l.Chop());
            Assert.AreEqual(OnionStage.Chopping, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(OnionChopResult.None, l.Chop());
            Assert.AreEqual(OnionFridgeResult.None, l.TapFridge());
            Assert.AreEqual(OnionGogglesResult.None, l.Goggles());
            Assert.AreEqual(OnionTick.None, l.Tick(100f));
            Assert.IsTrue(l.Cold);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(OnionStage.Done, l.Stage);
            Assert.IsFalse(l.ToggleFan());
            Assert.AreEqual(OnionDropResult.None, l.DropOnion(true));
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void Reset_StartsOver()
        {
            var l = new CryingOnionLogic();
            AllWrongMoves(l);
            ChillFully(l);
            l.TapFridge();
            l.DropOnion(false);
            l.Chop();
            l.TryComplete();
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(OnionSpot.Board, l.Spot);
            Assert.IsFalse(l.Cold);
            Assert.IsFalse(l.FridgeOpen);
            Assert.IsFalse(l.FanOn);
            Assert.IsFalse(l.RemedyActive);
            Assert.AreEqual(0f, l.Chill);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.TimesChilled);
            Assert.IsFalse(l.CompletionReported);
            Assert.AreEqual(OnionChopResult.Tears, l.Chop());
        }
    }
}
