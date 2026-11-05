using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 19 della roadmap: The Shy Monster (la coccarda BEST MONSTER lasciata sul petto del mostro).</summary>
    public class V79ShyMonsterTests
    {
        // mostro coi piedi in (0, -3.3) come nel builder
        const float FeetX = 0f, FeetY = -3.3f;

        static void AssertStillPlaying(ShyMonsterLogic l)
        {
            Assert.AreEqual(ShyMonsterStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static ShyMonsterDrop Move(ShyMonsterLogic l, ShyMonsterItem item, bool inChest, bool onMonster)
        {
            Assert.IsTrue(l.Grab(item));
            Assert.AreEqual(item, l.Held);
            return l.Release(inChest, onMonster);
        }

        static void Run(ShyMonsterLogic l, float seconds, float step = 1f / 60f)
        {
            for (float t = 0f; t < seconds; t += step) l.Tick(step);
        }

        static bool Chest(float x, float y, float scale) => ShyMonsterLogic.InChest(x, y, FeetX, FeetY, scale);

        [Test]
        public void StartsNormal()
        {
            var l = new ShyMonsterLogic();
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.Growth);
            Assert.AreEqual(1f, l.Scale, 1e-5f);
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void TapsGrowUpToTheCapAndNeverWin()
        {
            var l = new ShyMonsterLogic();
            for (int i = 1; i <= ShyMonsterLogic.MaxGrowth; i++)
            {
                Assert.AreEqual(ShyMonsterHit.Grew, l.Hit());
                Assert.AreEqual(i, l.Growth);
            }
            for (int i = 0; i < 30; i++) Assert.AreEqual(ShyMonsterHit.Maxed, l.Hit());
            Assert.AreEqual(ShyMonsterLogic.MaxGrowth, l.Growth);
            Assert.AreEqual(1f + ShyMonsterLogic.MaxGrowth * ShyMonsterLogic.GrowthPerStep, l.Scale, 1e-5f);
            Assert.LessOrEqual(l.Scale, 1.35f, "resta dentro lo schermo");
            Assert.AreEqual(34, l.Hits);
            AssertStillPlaying(l);
        }

        [Test]
        public void GrowthDecaysBackAfterQuiet()
        {
            var l = new ShyMonsterLogic();
            for (int i = 0; i < 6; i++) l.Hit();
            Assert.AreEqual(ShyMonsterLogic.MaxGrowth, l.Growth);
            Run(l, ShyMonsterLogic.QuietSeconds - 0.2f);
            Assert.AreEqual(ShyMonsterLogic.MaxGrowth, l.Growth, "prima della calma non si sgonfia");
            Run(l, 0.3f);
            Assert.AreEqual(ShyMonsterLogic.MaxGrowth - 1, l.Growth);
            Run(l, ShyMonsterLogic.ShrinkStepSeconds * (ShyMonsterLogic.MaxGrowth - 1) + 0.2f);
            Assert.AreEqual(0, l.Growth);
            Assert.AreEqual(1f, l.Scale, 1e-5f);
            Run(l, 10f);
            Assert.AreEqual(0, l.Growth, "mai sotto il normale");
            AssertStillPlaying(l);
        }

        [Test]
        public void HitDuringShrinkRestartsTheQuiet()
        {
            var l = new ShyMonsterLogic();
            l.Hit(); l.Hit();
            Run(l, ShyMonsterLogic.QuietSeconds + 0.05f);
            Assert.AreEqual(1, l.Growth);
            Assert.AreEqual(ShyMonsterHit.Grew, l.Hit());
            Assert.AreEqual(2, l.Growth);
            Run(l, ShyMonsterLogic.QuietSeconds - 0.2f);
            Assert.AreEqual(2, l.Growth);
        }

        [Test]
        public void TickReturnsTrueOncePerStep()
        {
            var l = new ShyMonsterLogic();
            l.Hit();
            Assert.IsFalse(l.Tick(ShyMonsterLogic.QuietSeconds - 0.01f));
            Assert.IsTrue(l.Tick(0.02f));
            Assert.IsFalse(l.Tick(5f));
            Assert.IsFalse(l.Tick(0f));
            Assert.IsFalse(l.Tick(-1f));
        }

        [Test]
        public void MedalDroppedOnTheFloorReturns()
        {
            var l = new ShyMonsterLogic();
            Assert.AreEqual(ShyMonsterDrop.Floor, Move(l, ShyMonsterItem.Medal, false, false));
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(1, l.FloorDrops);
            AssertStillPlaying(l);
        }

        [Test]
        public void MedalOnHeadOrArmsIsIgnored()
        {
            var l = new ShyMonsterLogic();
            Assert.AreEqual(ShyMonsterDrop.Ignored, Move(l, ShyMonsterItem.Medal, false, true));
            Assert.AreEqual(ShyMonsterDrop.Ignored, Move(l, ShyMonsterItem.Medal, false, true));
            Assert.AreEqual(2, l.Ignored);
            Assert.AreEqual(0, l.Growth, "la medaglia non lo fa arrabbiare");
            AssertStillPlaying(l);
        }

        [Test]
        public void MedalPassingOverTheChestButReleasedElsewhereDoesNotWin()
        {
            var l = new ShyMonsterLogic();
            Assert.IsTrue(l.Grab(ShyMonsterItem.Medal));
            // il dito passa sopra il petto: nessun evento finché non si lascia
            Assert.IsTrue(Chest(0f, FeetY + ShyMonsterLogic.ChestY, 1f));
            AssertStillPlaying(l);
            Assert.AreEqual(ShyMonsterDrop.Floor, l.Release(Chest(2.2f, -4f, 1f), false));
            AssertStillPlaying(l);
        }

        [Test]
        public void MedalReleasedOnTheChestWinsOnce()
        {
            var l = new ShyMonsterLogic();
            Assert.AreEqual(ShyMonsterDrop.Pinned, Move(l, ShyMonsterItem.Medal, true, true));
            Assert.AreEqual(ShyMonsterStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(ShyMonsterHit.None, l.Hit());
            Assert.IsFalse(l.Grab(ShyMonsterItem.Medal));
            Assert.AreEqual(ShyMonsterDrop.None, l.Release(true, true));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(ShyMonsterStage.Done, l.Stage);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void MedalOnTheChestWinsAlsoWhenGrown()
        {
            var l = new ShyMonsterLogic();
            for (int i = 0; i < 10; i++) l.Hit();
            float s = l.Scale;
            float cy = FeetY + ShyMonsterLogic.ChestY * s;
            Assert.IsTrue(Chest(0f, cy, s));
            Assert.AreEqual(ShyMonsterDrop.Pinned, Move(l, ShyMonsterItem.Medal, Chest(0.3f, cy + 0.2f, s), true));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ChestZoneFollowsTheScaleAndIsDistinctFromTheHead()
        {
            // la faccia comincia a y 2.45 (scala 1): non è petto, a nessuna scala
            for (int g = 0; g <= ShyMonsterLogic.MaxGrowth; g++)
            {
                float s = 1f + g * ShyMonsterLogic.GrowthPerStep;
                Assert.IsTrue(Chest(0f, FeetY + ShyMonsterLogic.ChestY * s, s), "centro del petto, scala " + s);
                Assert.IsTrue(Chest(0.85f * s, FeetY + 1.25f * s, s), "angolo generoso del petto");
                Assert.IsFalse(Chest(0f, FeetY + 2.5f * s, s), "la faccia non è il petto");
                Assert.IsFalse(Chest(0f, FeetY + 3.3f * s, s), "la testa non è il petto");
                Assert.IsFalse(Chest(1.5f * s, FeetY + 1.8f * s, s), "il braccio non è il petto");
                Assert.IsFalse(Chest(0f, FeetY + 0.5f * s, s), "le gambe non sono il petto");
            }
            // un punto del petto a scala 1 che a scala massima è già sotto: la zona va calcolata dalla scala attuale
            float big = 1f + ShyMonsterLogic.MaxGrowth * ShyMonsterLogic.GrowthPerStep;
            Assert.IsTrue(Chest(0f, FeetY + 2.3f * big, big));
            Assert.IsFalse(Chest(0f, FeetY + 2.3f * big, 1f));
        }

        [Test]
        public void DecoysNeverWin()
        {
            var l = new ShyMonsterLogic();
            foreach (ShyMonsterItem d in new[] { ShyMonsterItem.Sword, ShyMonsterItem.Slingshot })
            {
                Assert.AreEqual(ShyMonsterDrop.Tickled, Move(l, d, true, true), "anche sul petto fa solo il solletico");
                Assert.AreEqual(ShyMonsterDrop.Tickled, Move(l, d, false, true));
                Assert.AreEqual(ShyMonsterDrop.Floor, Move(l, d, false, false));
                AssertStillPlaying(l);
            }
            Assert.AreEqual(4, l.Tickles);
            Assert.AreEqual(4, l.Growth, "il solletico lo fa crescere");
            Assert.AreEqual(ShyMonsterDrop.Tickled, Move(l, ShyMonsterItem.Sword, false, true));
            Assert.AreEqual(ShyMonsterLogic.MaxGrowth, l.Growth, "mai oltre il massimo");
            AssertStillPlaying(l);
        }

        [Test]
        public void AtLeastThreeWrongAttemptsThenTheSolution()
        {
            var l = new ShyMonsterLogic();
            l.Hit();
            Move(l, ShyMonsterItem.Sword, false, true);
            Move(l, ShyMonsterItem.Medal, false, true);
            Move(l, ShyMonsterItem.Medal, false, false);
            Move(l, ShyMonsterItem.Slingshot, false, false);
            Assert.AreEqual(5, l.WrongMoves);
            AssertStillPlaying(l);
            Assert.AreEqual(ShyMonsterDrop.Pinned, Move(l, ShyMonsterItem.Medal, true, true));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void OneThingAtATime()
        {
            var l = new ShyMonsterLogic();
            Assert.IsTrue(l.Grab(ShyMonsterItem.Sword));
            Assert.IsFalse(l.Grab(ShyMonsterItem.Medal));
            Assert.AreEqual(ShyMonsterHit.None, l.Hit(), "col dito occupato non si colpisce");
            Assert.IsFalse(l.TapItem(ShyMonsterItem.Medal));
            Assert.AreEqual(ShyMonsterDrop.Tickled, l.Release(true, true), "si lascia la spada, non la medaglia");
            Assert.AreEqual(ShyMonsterDrop.None, l.Release(true, true), "niente in mano");
            Assert.IsFalse(l.Grab(ShyMonsterItem.None));
            AssertStillPlaying(l);
        }

        [Test]
        public void RapidTouches()
        {
            var l = new ShyMonsterLogic();
            for (int i = 0; i < 500; i++)
            {
                l.Hit();
                l.TapItem(ShyMonsterItem.Medal);
                l.TapItem(ShyMonsterItem.Slingshot);
                l.Grab(ShyMonsterItem.Medal);
                l.Release(false, i % 2 == 0);
                l.Grab(ShyMonsterItem.Sword);
                l.Cancel();
                l.Tick(0.016f);
            }
            Assert.LessOrEqual(l.Growth, ShyMonsterLogic.MaxGrowth);
            Assert.GreaterOrEqual(l.Growth, 0);
            Assert.IsFalse(l.Holding);
            AssertStillPlaying(l);
        }

        [Test]
        public void EarlyReleaseAndCancelLeaveAValidState()
        {
            var l = new ShyMonsterLogic();
            Assert.IsFalse(l.Cancel(), "niente da annullare");
            Assert.IsTrue(l.Grab(ShyMonsterItem.Medal));
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(0, l.WrongMoves, "annullare non è un errore");
            Assert.AreEqual(ShyMonsterDrop.None, l.Release(true, true), "dopo l'annullo il rilascio non conta");
            AssertStillPlaying(l);
            // si riprende e si vince lo stesso
            Assert.AreEqual(ShyMonsterDrop.Pinned, Move(l, ShyMonsterItem.Medal, true, true));
            Assert.IsFalse(l.Cancel(), "durante la festa Cancel non cambia niente");
            Assert.AreEqual(ShyMonsterStage.Celebrating, l.Stage);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ShrinkStopsDuringTheCelebration()
        {
            var l = new ShyMonsterLogic();
            l.Hit(); l.Hit();
            Move(l, ShyMonsterItem.Medal, true, true);
            Assert.IsFalse(l.Tick(10f));
            Assert.AreEqual(2, l.Growth);
        }

        [Test]
        public void TapsOnItemsAreHarmless()
        {
            var l = new ShyMonsterLogic();
            Assert.IsTrue(l.TapItem(ShyMonsterItem.Medal));
            Assert.IsTrue(l.TapItem(ShyMonsterItem.Sword));
            Assert.IsTrue(l.TapItem(ShyMonsterItem.Slingshot));
            Assert.IsFalse(l.TapItem(ShyMonsterItem.None));
            Assert.AreEqual(1, l.MedalTaps);
            Assert.AreEqual(2, l.DecoyTaps);
            Assert.AreEqual(0, l.Growth);
            AssertStillPlaying(l);
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = new ShyMonsterLogic();
            for (int i = 0; i < 5; i++) l.Hit();
            l.Grab(ShyMonsterItem.Slingshot);
            l.Reset();
            Assert.AreEqual(0, l.Growth);
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(0, l.WrongMoves);
            AssertStillPlaying(l);
            // anche dopo una vittoria
            Move(l, ShyMonsterItem.Medal, true, true);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(ShyMonsterDrop.Pinned, Move(l, ShyMonsterItem.Medal, true, true));
            Assert.IsTrue(l.TryComplete(), "dopo il reset si può rivincere (rientro nel livello)");
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void InZoneMargin()
        {
            Assert.IsTrue(ShyMonsterLogic.InZone(1.1f, 0f, 0f, 0f, 1f, 1f, 0.15f));
            Assert.IsFalse(ShyMonsterLogic.InZone(1.2f, 0f, 0f, 0f, 1f, 1f, 0.15f));
            Assert.IsFalse(ShyMonsterLogic.InZone(1.1f, 0f, 0f, 0f, 1f, 1f, -1f), "margine negativo = 0");
            Assert.IsTrue(ShyMonsterLogic.InZone(0f, -0.9f, 0f, 0f, -1f, -1f, 0f), "mezze misure negative = positive");
        }
    }
}
