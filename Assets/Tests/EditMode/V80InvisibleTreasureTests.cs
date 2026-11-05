using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 30 della roadmap: The Invisible Treasure (spostare la X dalla mappa alla sabbia e scavare lì).</summary>
    public class V80InvisibleTreasureTests
    {
        // misure di default della logica (le stesse del builder)
        const float SpotX = 0.5f, SpotY = -0.75f;

        static void AssertStillPlaying(InvisibleTreasureLogic l)
        {
            Assert.AreEqual(InvisibleTreasureStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static InvisibleTreasureDig Dig(InvisibleTreasureLogic l, float x, float y)
        {
            Assert.IsTrue(l.GrabShovel());
            return l.DropShovel(x, y);
        }

        static InvisibleTreasureDrop MoveX(InvisibleTreasureLogic l, float x, float y)
        {
            Assert.IsTrue(l.GrabX());
            return l.DropX(x, y);
        }

        [Test]
        public void StartsWithXOnTheMap()
        {
            var l = new InvisibleTreasureLogic();
            AssertStillPlaying(l);
            Assert.IsFalse(l.XOnGround);
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void GeometryMapAndGroundAreDistinct()
        {
            var l = new InvisibleTreasureLogic();
            Assert.IsTrue(l.IsOnMap(l.MapX, l.MapY));
            Assert.IsFalse(l.IsSand(l.MapX, l.MapY), "la mappa non è mai sabbia");
            Assert.IsTrue(l.IsSand(l.IslandX, l.IslandY));
            Assert.IsTrue(l.IsSand(SpotX, SpotY), "il posto della mappa è sull'isola");
            Assert.IsFalse(l.IsSand(0f, -4.6f), "mare sotto l'isola");
            Assert.IsFalse(l.IsSand(2.6f, -1.45f), "mare a destra");
            Assert.IsFalse(l.IsSand(1.5f, 2.5f), "cielo");
            Assert.IsFalse(l.IsSand(l.IslandX + l.IslandRx - 0.05f, l.IslandY), "il bordino non conta");
            Assert.IsTrue(l.IsSand(l.IslandX + l.IslandRx - 0.2f, l.IslandY), "ma poco più dentro sì (generoso)");
        }

        [Test]
        public void MapDrawingPointsToTheMapSpot()
        {
            // il builder disegna la X della mappa in: isola della mappa + (posto - isola) * scala
            const float mapIslandX = 0.02f, mapIslandY = -0.08f, scale = 0.27f;
            float mx = mapIslandX + (SpotX - 0f) * scale, my = mapIslandY + (SpotY + 1.45f) * scale;
            InvisibleTreasureLogic.MapToGround(mx, my, mapIslandX, mapIslandY, scale, 0f, -1.45f, out float gx, out float gy);
            Assert.AreEqual(SpotX, gx, 1e-4f);
            Assert.AreEqual(SpotY, gy, 1e-4f);
        }

        [Test]
        public void DiggingAtTheMapSpotNeverWins()
        {
            var l = new InvisibleTreasureLogic();
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(InvisibleTreasureDig.MapSpot, Dig(l, SpotX + 0.3f * (i % 2), SpotY - 0.2f * (i % 3)));
                AssertStillPlaying(l);
            }
            Assert.AreEqual(6, l.MapSpotDigs);
            Assert.AreEqual(0, l.JunkDigs);
        }

        [Test]
        public void DiggingElsewhereRotatesJunkAndNeverWins()
        {
            var l = new InvisibleTreasureLogic();
            var expected = InvisibleTreasureLogic.JunkCycle;
            for (int i = 0; i < 12; i++)
            {
                Assert.AreEqual(expected[i % expected.Length], Dig(l, -1.2f, -2.2f + 0.1f * (i % 4)));
                AssertStillPlaying(l);
            }
            Assert.AreEqual(12, l.JunkDigs);
            Assert.AreEqual(12, l.Digs);
        }

        [Test]
        public void ShovelOffTheSandDoesNotDig()
        {
            var l = new InvisibleTreasureLogic();
            Assert.AreEqual(InvisibleTreasureDig.NotSand, Dig(l, 0f, -4.6f));   // mare
            Assert.AreEqual(InvisibleTreasureDig.NotSand, Dig(l, 1.5f, 2.5f));  // cielo
            Assert.AreEqual(InvisibleTreasureDig.NotSand, Dig(l, l.MapX, l.MapY));   // sulla mappa
            Assert.AreEqual(3, l.NotSandDrops);
            Assert.AreEqual(0, l.Digs);
            AssertStillPlaying(l);
        }

        [Test]
        public void XDroppedOffSandReturnsToTheMap()
        {
            var l = new InvisibleTreasureLogic();
            Assert.AreEqual(InvisibleTreasureDrop.OffIsland, MoveX(l, 0f, -4.6f));
            Assert.IsFalse(l.XOnGround);
            Assert.AreEqual(InvisibleTreasureDrop.OffIsland, MoveX(l, 2.0f, 2.2f));
            Assert.IsFalse(l.XOnGround);
            Assert.AreEqual(InvisibleTreasureDrop.OnMap, MoveX(l, l.MapX + 0.5f, l.MapY - 0.3f));
            Assert.IsFalse(l.XOnGround);
            Assert.AreEqual(2, l.XReturns);
            // la X sulla mappa non fa vincere scavando da nessuna parte
            Assert.AreEqual(InvisibleTreasureDig.MapSpot, Dig(l, SpotX, SpotY));
            AssertStillPlaying(l);
        }

        [Test]
        public void XOnSandStaysAndDiggingThereWinsOnce()
        {
            var l = new InvisibleTreasureLogic();
            Assert.AreEqual(InvisibleTreasureDrop.OnSand, MoveX(l, -1.0f, -2.0f));
            Assert.IsTrue(l.XOnGround);
            Assert.AreEqual(-1.0f, l.GroundX);
            Assert.AreEqual(-2.0f, l.GroundY);
            // tolleranza generosa (non millimetrica)
            Assert.AreEqual(InvisibleTreasureDig.Treasure, Dig(l, -1.0f + 0.55f, -2.0f - 0.5f));
            Assert.AreEqual(InvisibleTreasureStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.IsFalse(l.GrabShovel());
            Assert.IsFalse(l.GrabX());
            Assert.IsFalse(l.Tap(InvisibleTreasureSpot.Sand));
            Assert.AreEqual(InvisibleTreasureDig.None, l.DropShovel(-1f, -2f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "una volta sola");
            Assert.AreEqual(InvisibleTreasureStage.Done, l.Stage);
        }

        [Test]
        public void DigJustOutsideTheXToleranceIsNotTreasure()
        {
            var l = new InvisibleTreasureLogic();
            MoveX(l, -1.0f, -2.0f);
            Assert.AreNotEqual(InvisibleTreasureDig.Treasure, Dig(l, -1.0f + InvisibleTreasureLogic.XRadius + 0.05f, -2.0f));
            AssertStillPlaying(l);
            Assert.AreEqual(InvisibleTreasureDig.Treasure, Dig(l, -1.0f + InvisibleTreasureLogic.XRadius - 0.05f, -2.0f));
        }

        [Test]
        public void XMovedAgainOnlyTheNewSpotWins()
        {
            var l = new InvisibleTreasureLogic();
            Assert.AreEqual(InvisibleTreasureDrop.OnSand, MoveX(l, -1.2f, -2.4f));
            Assert.AreEqual(InvisibleTreasureDrop.OnSand, MoveX(l, 1.2f, -2.2f));   // ripresa da terra e spostata
            Assert.AreEqual(2, l.XPlacements);
            Assert.AreNotEqual(InvisibleTreasureDig.Treasure, Dig(l, -1.2f, -2.4f), "il vecchio posto non vale più");
            AssertStillPlaying(l);
            Assert.AreEqual(InvisibleTreasureDig.Treasure, Dig(l, 1.2f, -2.2f));
        }

        [Test]
        public void XFromGroundBackToMapOrSeaUndoesThePlacement()
        {
            var l = new InvisibleTreasureLogic();
            MoveX(l, -1.0f, -2.0f);
            Assert.AreEqual(InvisibleTreasureDrop.OnMap, MoveX(l, l.MapX, l.MapY));
            Assert.IsFalse(l.XOnGround);
            Assert.AreNotEqual(InvisibleTreasureDig.Treasure, Dig(l, -1.0f, -2.0f));
            MoveX(l, -1.0f, -2.0f);
            Assert.AreEqual(InvisibleTreasureDrop.OffIsland, MoveX(l, 2.7f, -3.5f));
            Assert.IsFalse(l.XOnGround);
            Assert.AreNotEqual(InvisibleTreasureDig.Treasure, Dig(l, -1.0f, -2.0f));
            AssertStillPlaying(l);
            // e si può sempre rifare
            MoveX(l, 0f, -1.5f);
            Assert.AreEqual(InvisibleTreasureDig.Treasure, Dig(l, 0f, -1.5f));
        }

        [Test]
        public void XOnTheMapSpotMakesTheMapSpotTheTreasure()
        {
            // chi mette la X proprio dove indicava la mappa ha comunque "deciso" lui: lì ora c'è il tesoro
            var l = new InvisibleTreasureLogic();
            Assert.AreEqual(InvisibleTreasureDig.MapSpot, Dig(l, SpotX, SpotY));
            MoveX(l, SpotX, SpotY);
            Assert.AreEqual(InvisibleTreasureDig.Treasure, Dig(l, SpotX, SpotY));
        }

        [Test]
        public void ThreeWrongAttemptsThenTheSolution()
        {
            var l = new InvisibleTreasureLogic();
            Assert.AreEqual(InvisibleTreasureDig.MapSpot, Dig(l, SpotX, SpotY));        // 1: la mappa mente
            Assert.AreEqual(InvisibleTreasureDig.Sand, Dig(l, -1.5f, -2.5f));            // 2: altrove solo sabbia
            Assert.AreEqual(InvisibleTreasureDrop.OffIsland, MoveX(l, 0f, -4.7f));      // 3: X in mare -> torna
            Assert.IsTrue(l.Tap(InvisibleTreasureSpot.Palm));                             // 4: tocchi a caso
            Assert.IsTrue(l.Tap(InvisibleTreasureSpot.Shovel));
            AssertStillPlaying(l);
            Assert.AreEqual(5, l.WrongMoves);
            Assert.AreEqual(InvisibleTreasureDrop.OnSand, MoveX(l, -0.8f, -1.2f));
            Assert.AreEqual(InvisibleTreasureDig.Treasure, Dig(l, -0.7f, -1.3f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void TapsNeverWinAndGroundXTapWithoutXCountsAsMapX()
        {
            var l = new InvisibleTreasureLogic();
            foreach (InvisibleTreasureSpot s in System.Enum.GetValues(typeof(InvisibleTreasureSpot)))
                for (int i = 0; i < 3; i++) Assert.IsTrue(l.Tap(s));
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.Taps(InvisibleTreasureSpot.GroundX));
            Assert.AreEqual(6, l.Taps(InvisibleTreasureSpot.MapX));
            Assert.AreEqual(21, l.WrongMoves);
            Assert.IsFalse(l.XOnGround);
        }

        [Test]
        public void RapidDigsAndTapsJustCount()
        {
            var l = new InvisibleTreasureLogic();
            for (int i = 0; i < 300; i++)
            {
                l.Tap((InvisibleTreasureSpot)(i % 7));
                Dig(l, (i % 2 == 0) ? SpotX : -1.3f, (i % 2 == 0) ? SpotY : -2.3f);
            }
            AssertStillPlaying(l);
            Assert.AreEqual(150, l.MapSpotDigs);
            Assert.AreEqual(150, l.JunkDigs);
            Assert.IsFalse(l.Holding);
        }

        [Test]
        public void OnlyOneThingHeldAtATime()
        {
            var l = new InvisibleTreasureLogic();
            Assert.IsTrue(l.GrabX());
            Assert.IsFalse(l.GrabShovel(), "un dito alla volta");
            Assert.IsFalse(l.GrabX());
            Assert.IsFalse(l.Tap(InvisibleTreasureSpot.Sand), "niente tocchi mentre si trascina");
            Assert.AreEqual(InvisibleTreasureDig.None, l.DropShovel(0f, -1.5f), "la pala non è tenuta");
            Assert.AreEqual(InvisibleTreasureDrop.OnSand, l.DropX(0f, -1.5f));
            Assert.AreEqual(InvisibleTreasureDrop.None, l.DropX(0f, -1.5f), "doppio rilascio ignorato");
            Assert.IsTrue(l.GrabShovel());
            Assert.AreEqual(InvisibleTreasureDrop.None, l.DropX(1f, -1.5f), "la X non è tenuta");
            Assert.AreEqual(InvisibleTreasureDig.Treasure, l.DropShovel(0f, -1.5f));
        }

        [Test]
        public void CancelLeavesAValidStateAndNeverWins()
        {
            var l = new InvisibleTreasureLogic();
            Assert.IsFalse(l.CancelHold(), "niente da annullare");
            // X presa dalla mappa e persa a metà: resta sulla mappa
            Assert.IsTrue(l.GrabX());
            Assert.IsTrue(l.CancelHold());
            Assert.IsFalse(l.XOnGround);
            Assert.IsFalse(l.Holding);
            // X a terra, ripresa e persa: resta dov'era
            MoveX(l, -1f, -2f);
            Assert.IsTrue(l.GrabX());
            Assert.IsTrue(l.CancelHold());
            Assert.IsTrue(l.XOnGround);
            Assert.AreEqual(-1f, l.GroundX);
            // pala persa proprio sopra la X: non scava e non vince
            Assert.IsTrue(l.GrabShovel());
            Assert.IsTrue(l.CancelHold());
            Assert.AreEqual(InvisibleTreasureDig.None, l.DropShovel(-1f, -2f));
            Assert.AreEqual(0, l.Digs);
            AssertStillPlaying(l);
            // e poi si vince normalmente
            Assert.AreEqual(InvisibleTreasureDig.Treasure, Dig(l, -1f, -2f));
        }

        [Test]
        public void ResetRestoresTheStart()
        {
            var l = new InvisibleTreasureLogic();
            Dig(l, SpotX, SpotY);
            Dig(l, -1.3f, -2.3f);
            MoveX(l, -1f, -2f);
            l.Tap(InvisibleTreasureSpot.Palm);
            Dig(l, -1f, -2f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.IsFalse(l.XOnGround);
            Assert.IsFalse(l.Holding);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Digs);
            // dopo il reset il vecchio posto della X non vale
            Assert.AreEqual(InvisibleTreasureDig.Sand, Dig(l, -1f, -2f), "la rotazione delle cianfrusaglie ricomincia");
            Assert.AreEqual(InvisibleTreasureDig.Boot, Dig(l, -1.3f, -2.3f));
            // e si vince di nuovo una volta sola
            MoveX(l, 0.8f, -2.5f);
            Assert.AreEqual(InvisibleTreasureDig.Treasure, Dig(l, 0.8f, -2.5f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ConfigureMovesTheGeometry()
        {
            var l = new InvisibleTreasureLogic();
            l.Configure(0f, 0f, 1f, 1f, 0f, 3f, 0.5f, 0.5f, 0.3f, 0.3f);
            l.Reset();
            Assert.IsTrue(l.IsSand(0f, 0f));
            Assert.IsFalse(l.IsSand(0f, -1.5f));
            Assert.IsTrue(l.IsOnMap(0.2f, 3.2f));
            Assert.AreEqual(InvisibleTreasureDig.MapSpot, Dig(l, 0.3f, 0.3f));
            Assert.AreEqual(InvisibleTreasureDrop.OffIsland, MoveX(l, 0f, -1.5f));
        }
    }
}
