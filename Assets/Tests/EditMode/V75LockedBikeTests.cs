using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 276: la bici legata a un palo corto (si solleva sopra la cima del palo).</summary>
    public class V75LockedBikeTests
    {
        const float Home = 0.4f;

        static void AssertStillPlaying(LockedBikeLogic l)
        {
            Assert.AreEqual(LockedBikeStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static void AssertLockedAtHome(LockedBikeLogic l)
        {
            Assert.AreEqual(LockedBikeSpot.Locked, l.Spot);
            Assert.IsFalse(l.Free);
            Assert.AreEqual(Home, l.RestX, 1e-5f);
            Assert.IsTrue(l.LockClosed);
        }

        /// <summary>Tutte le false piste, una dopo l'altra.</summary>
        static void AllWrongMoves(LockedBikeLogic l)
        {
            for (int i = 0; i < 4; i++) Assert.AreEqual(LockedBikeKey.Wrong, l.TryKey(), "key " + i);
            Assert.AreEqual(LockedBikeKey.HouseKey, l.TryKey());
            Assert.AreEqual(LockedBikeKey.BottleOpener, l.TryKey());
            Assert.AreEqual(LockedBikeCode.Zeros, l.TryCode());
            Assert.AreEqual(LockedBikeCode.Wrong, l.TryCode());
            Assert.AreEqual(LockedBikeCode.Wrong, l.TryCode());
            Assert.AreEqual(LockedBikeCode.Obvious, l.TryCode());
            Assert.AreEqual(LockedBikeCut.Bounce, l.Cut());
            Assert.AreEqual(LockedBikeCut.Bend, l.Cut());
            Assert.AreEqual(LockedBikePole.WobblePigeonFlew, l.KickPole());
            Assert.AreEqual(LockedBikePole.Wobble, l.KickPole());   // il piccione è già via
            Assert.IsTrue(l.Yank());
            Assert.AreEqual(LockedBikeLift.Rising, l.Lift(0.5f));
            Assert.AreEqual(LockedBikeDrop.SnapBack, l.Drop(Home + 0.1f, 0.5f));
            Assert.IsTrue(l.TapBike());
            Assert.IsTrue(l.ReadSign());
            Assert.IsFalse(l.TapPigeon());   // già in volo
        }

        [Test]
        public void StartsLockedWithThePigeonOnThePole()
        {
            var l = new LockedBikeLogic(Home);
            AssertStillPlaying(l);
            AssertLockedAtHome(l);
            Assert.IsFalse(l.PigeonAway);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void EachWrongMove_KeepsStateAndCounts()
        {
            var l = new LockedBikeLogic(Home);
            AllWrongMoves(l);
            AssertStillPlaying(l);
            AssertLockedAtHome(l);
            Assert.AreEqual(6, l.KeyTries);
            Assert.AreEqual(4, l.CodeTries);
            Assert.AreEqual(2, l.CutTries);
            Assert.AreEqual(2, l.PoleKicks);
            Assert.AreEqual(1, l.Yanks);
            Assert.AreEqual(1, l.LowLifts);
            Assert.AreEqual(1, l.SignReads);
            Assert.AreEqual(6 + 4 + 2 + 2 + 1 + 1, l.WrongMoves);
        }

        [Test]
        public void Keys_NeverFit_AndCycle()
        {
            var l = new LockedBikeLogic(Home);
            for (int round = 0; round < 3; round++)
            {
                for (int i = 0; i < 4; i++) Assert.AreEqual(LockedBikeKey.Wrong, l.TryKey());
                Assert.AreEqual(LockedBikeKey.HouseKey, l.TryKey());
                Assert.AreEqual(LockedBikeKey.BottleOpener, l.TryKey());
            }
            AssertStillPlaying(l);
            AssertLockedAtHome(l);
        }

        [Test]
        public void Combination_NeverOpens()
        {
            var l = new LockedBikeLogic(Home);
            Assert.AreEqual(LockedBikeCode.Zeros, l.TryCode());
            Assert.AreEqual(0, l.LastCode);
            Assert.AreEqual("0000", LockedBikeLogic.CodeLabel(l.LastCode));
            for (int i = 1; i < 200; i++)
            {
                LockedBikeCode r = l.TryCode();
                Assert.AreNotEqual(LockedBikeCode.None, r);
                Assert.AreNotEqual(LockedBikeCode.Zeros, r, "only the first try is 0000");
                Assert.That(l.LastCode, Is.InRange(1, 9999));
                Assert.AreEqual(4, LockedBikeLogic.CodeLabel(l.LastCode).Length);
            }
            Assert.AreEqual(1234, LockedBikeLogic.CodeFor(3));
            Assert.AreEqual(LockedBikeLogic.CodeFor(7), LockedBikeLogic.CodeFor(7));   // ripetibile
            AssertStillPlaying(l);
            AssertLockedAtHome(l);
        }

        [Test]
        public void BoltCutters_ThreeStrikes_BringThePolice()
        {
            var l = new LockedBikeLogic(Home);
            Assert.IsFalse(l.WhistleHeard);
            Assert.IsFalse(l.OfficerWatching);
            Assert.AreEqual(3, l.CutsLeft);
            Assert.AreEqual(LockedBikeCut.Bounce, l.Cut());     // 1°: fischietto lontano
            Assert.IsTrue(l.WhistleHeard);
            Assert.IsFalse(l.OfficerWatching);
            AssertStillPlaying(l);
            Assert.IsFalse(l.TryFail());
            Assert.AreEqual(LockedBikeCut.Bend, l.Cut());       // 2°: il vigile si affaccia (avviso)
            Assert.IsTrue(l.OfficerWatching);
            Assert.AreEqual(1, l.CutsLeft);
            AssertStillPlaying(l);
            AssertLockedAtHome(l);
            Assert.IsFalse(l.TryFail());
            Assert.AreEqual(LockedBikeCut.Busted, l.Cut());     // 3°: arrestato
            Assert.IsTrue(l.Busted);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(0, l.CutsLeft);
            Assert.IsTrue(l.TryFail());
            Assert.IsFalse(l.TryFail());                        // una volta sola
        }

        [Test]
        public void AfterTheFail_NothingElseRuns_NoWin()
        {
            var l = new LockedBikeLogic(Home);
            l.Cut(); l.Cut(); l.Cut();
            Assert.IsTrue(l.TryFail());
            Assert.AreEqual(LockedBikeLift.None, l.Lift(LockedBikeLogic.MaxLift));
            Assert.IsFalse(l.Free);
            Assert.AreEqual(LockedBikeDrop.None, l.Drop(LockedBikeLogic.ExitX + 1f, 0f));
            Assert.AreEqual(LockedBikeCut.None, l.Cut());
            Assert.AreEqual(LockedBikeKey.None, l.TryKey());
            Assert.AreEqual(LockedBikeCode.None, l.TryCode());
            Assert.AreEqual(LockedBikePole.None, l.KickPole());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(LockedBikeStage.Busted, l.Stage);
            Assert.AreEqual(LockedBikeTick.None, l.Tick(10f));
            l.Reset();   // nuova vita: si riparte da capo, senza colpi
            AssertStillPlaying(l);
            Assert.AreEqual(3, l.CutsLeft);
            Assert.AreEqual(LockedBikeCut.Bounce, l.Cut());
        }

        [Test]
        public void FailAlsoWithTheBikeAlreadyFree_ButNotAfterTheWin()
        {
            var l = new LockedBikeLogic(Home);
            Assert.AreEqual(LockedBikeLift.Cleared, l.Lift(LockedBikeLogic.ClearLift));
            Assert.AreEqual(LockedBikeDrop.Parked, l.Drop(0f, 0f));
            l.Cut(); l.Cut();
            Assert.AreEqual(LockedBikeCut.Busted, l.Cut());
            Assert.IsTrue(l.TryFail());
            Assert.IsFalse(l.TryComplete());

            var w = new LockedBikeLogic(Home);
            w.Cut(); w.Cut();                                   // due colpi: avvisato ma in gioco
            Assert.AreEqual(LockedBikeLift.Cleared, w.Lift(LockedBikeLogic.ClearLift));
            Assert.AreEqual(LockedBikeDrop.Rolled, w.Drop(LockedBikeLogic.ExitX, 0f));
            Assert.AreEqual(LockedBikeCut.None, w.Cut());       // durante la festa niente terzo colpo
            Assert.IsFalse(w.TryFail());
            Assert.IsTrue(w.TryComplete());
            Assert.IsFalse(w.TryFail());                        // nessuna sconfitta dopo la vittoria
            Assert.IsFalse(w.Busted);
        }

        [Test]
        public void SolutionPath_NeverFails()
        {
            var l = new LockedBikeLogic(Home);
            for (int i = 0; i < 30; i++) { l.TryKey(); l.TryCode(); l.KickPole(); l.Yank(); l.Lift(0.5f); l.Drop(Home, 0.5f); l.Tick(1f); }
            AssertStillPlaying(l);
            Assert.IsFalse(l.TryFail());
            Assert.AreEqual(LockedBikeLift.Cleared, l.Lift(LockedBikeLogic.ClearLift + 0.1f));
            Assert.AreEqual(LockedBikeDrop.Rolled, l.Drop(LockedBikeLogic.ExitX + 0.3f, 0f));
            Assert.IsFalse(l.TryFail());
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void KickingThePole_PigeonFliesOffAndComesBack()
        {
            var l = new LockedBikeLogic(Home);
            Assert.AreEqual(LockedBikePole.WobblePigeonFlew, l.KickPole());
            Assert.IsTrue(l.PigeonAway);
            Assert.AreEqual(LockedBikeTick.None, l.Tick(1f));
            Assert.AreEqual(LockedBikePole.Wobble, l.KickPole());   // riparte il conto
            Assert.AreEqual(LockedBikeTick.None, l.Tick(LockedBikeLogic.PigeonAwaySeconds - 0.1f));
            Assert.AreEqual(LockedBikeTick.PigeonBack, l.Tick(0.2f));
            Assert.IsFalse(l.PigeonAway);
            Assert.AreEqual(LockedBikeTick.None, l.Tick(5f));
            Assert.IsTrue(l.TapPigeon());
            Assert.AreEqual(2, l.PigeonFlights);
            AssertStillPlaying(l);
            AssertLockedAtHome(l);
        }

        [Test]
        public void LiftingNotHighEnough_SnapsBack_NoSoftlock()
        {
            var l = new LockedBikeLogic(Home);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(LockedBikeLift.Rising, l.Lift(0.2f));
                Assert.AreEqual(LockedBikeLift.Rising, l.Lift(LockedBikeLogic.ClearLift - 0.01f));
                Assert.AreEqual(LockedBikeDrop.SnapBack, l.Drop(2.5f, LockedBikeLogic.ClearLift - 0.01f));   // anche "verso casa": legata
                AssertLockedAtHome(l);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(5, l.LowLifts);
            Assert.AreEqual(LockedBikeDrop.SnapBack, l.Drop(Home, 0.05f));   // un tocco tremolante non conta
            Assert.AreEqual(5, l.LowLifts);
            // dopo tutti i tentativi bassi si può ancora vincere
            Assert.AreEqual(LockedBikeLift.Cleared, l.Lift(LockedBikeLogic.ClearLift));
            Assert.AreEqual(LockedBikeDrop.Rolled, l.Drop(LockedBikeLogic.ExitX + 0.2f, 0f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void LiftingHighSpooksThePigeon()
        {
            var l = new LockedBikeLogic(Home);
            Assert.AreEqual(LockedBikeLift.Rising, l.Lift(LockedBikeLogic.PigeonSpook - 0.05f));
            Assert.IsFalse(l.PigeonAway);
            Assert.AreEqual(LockedBikeLift.Rising, l.Lift(LockedBikeLogic.PigeonSpook + 0.05f));
            Assert.IsTrue(l.PigeonAway);
        }

        [Test]
        public void LiftingOverTheTop_FreesTheBike_LockStaysClosed()
        {
            var l = new LockedBikeLogic(Home);
            Assert.AreEqual(LockedBikeLift.Cleared, l.Lift(LockedBikeLogic.ClearLift + 0.3f));
            Assert.IsTrue(l.Free);
            Assert.IsTrue(l.LockClosed);
            Assert.AreEqual(LockedBikeLift.None, l.Lift(2f));   // già libera
            Assert.IsFalse(l.Yank());                           // niente più catena attorno al palo
            AssertStillPlaying(l);
            // posata a metà strada: resta libera dov'è (progresso conservato)
            Assert.AreEqual(LockedBikeDrop.Parked, l.Drop(-0.5f, 0f));
            Assert.IsTrue(l.Free);
            Assert.AreEqual(-0.5f, l.RestX, 1e-5f);
            Assert.IsTrue(l.Park(-9f));
            Assert.AreEqual(LockedBikeLogic.ParkMinX, l.ParkX, 1e-5f);
            Assert.IsTrue(l.Park(9f));                          // la pausa non la fa mai partire
            Assert.Less(l.ParkX, LockedBikeLogic.ExitX);
            AssertStillPlaying(l);
            // le false piste funzionano ancora e non cambiano niente
            l.TryKey(); l.TryCode(); l.Cut(); l.Cut(); l.KickPole(); l.TapPigeon(); l.ReadSign(); l.TapBike();
            Assert.IsTrue(l.Free);
            AssertStillPlaying(l);
        }

        [Test]
        public void RollingHome_WinsOnce()
        {
            var l = new LockedBikeLogic(Home);
            Assert.AreEqual(LockedBikeLift.Cleared, l.Lift(LockedBikeLogic.ClearLift));
            Assert.AreEqual(LockedBikeDrop.Rolled, l.Drop(LockedBikeLogic.ExitX, 0f));
            Assert.AreEqual(LockedBikeStage.Rolling, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(LockedBikeKey.None, l.TryKey());
            Assert.AreEqual(LockedBikeCode.None, l.TryCode());
            Assert.AreEqual(LockedBikeCut.None, l.Cut());
            Assert.AreEqual(LockedBikePole.None, l.KickPole());
            Assert.AreEqual(LockedBikeDrop.None, l.Drop(0f, 0f));
            Assert.IsFalse(l.Park(0f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(LockedBikeStage.Done, l.Stage);
            Assert.IsTrue(l.LockClosed);   // il lucchetto non si è mai aperto
        }

        [Test]
        public void Reset_RestoresTheStart()
        {
            var l = new LockedBikeLogic(Home);
            AllWrongMoves(l);
            l.Lift(LockedBikeLogic.MaxLift);
            l.Drop(LockedBikeLogic.ExitX + 1f, 0f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.CutTries);
            Assert.IsFalse(l.OfficerWatching);
            Assert.IsFalse(l.FailReported);
            AssertLockedAtHome(l);
            Assert.IsFalse(l.PigeonAway);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.CodeTries);
            Assert.AreEqual(LockedBikeCode.Zeros, l.TryCode());   // si riparte da 0000
            Assert.AreEqual(LockedBikeLift.Cleared, l.Lift(LockedBikeLogic.ClearLift));
            Assert.AreEqual(LockedBikeDrop.Rolled, l.Drop(LockedBikeLogic.ExitX + 0.5f, 0f));
            Assert.IsTrue(l.TryComplete());
        }
    }
}
