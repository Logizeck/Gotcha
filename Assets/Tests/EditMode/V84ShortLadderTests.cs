using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 72 della roadmap: The Short Ladder (allungare la scala col pizzico e metterla sotto la finestra).</summary>
    public class V84ShortLadderTests
    {
        const float Under = ShortLadderLogic.WindowX;
        const float Away = -1.5f;

        static void AssertStillPlaying(ShortLadderLogic l)
        {
            Assert.AreEqual(ShortLadderStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static ShortLadderDrop DragLadder(ShortLadderLogic l, float x, float bottomY = ShortLadderLogic.FloorY)
        {
            Assert.IsTrue(l.GrabLadder());
            for (int i = 1; i <= 8; i++)
                Assert.IsTrue(l.MoveLadder(l.LadderX + (x - l.LadderX) * i / 8f, bottomY));
            return l.ReleaseLadder();
        }

        static ShortLadderDrop DragLadderHeld(ShortLadderLogic l, float x)
        {
            Assert.IsTrue(l.MoveLadder(x, ShortLadderLogic.FloorY));
            return l.ReleaseLadder();
        }

        static ShortLadderDrop PinchTo(ShortLadderLogic l, float ratio, bool keepDragging = false)
        {
            Assert.IsTrue(l.StartPinch());
            for (int i = 1; i <= 6; i++) l.Pinch(1f + (ratio - 1f) * i / 6f);
            return l.EndPinch(keepDragging);
        }

        /// <summary>Allunga la scala lontano dalla finestra fino ad almeno i pioli necessari.</summary>
        static void MakeLong(ShortLadderLogic l)
        {
            PinchTo(l, 2f);
            Assert.IsTrue(l.LongEnough, "un pizzico che raddoppia la distanza basta");
            Assert.IsFalse(l.InColumn);
        }

        [Test]
        public void StartsShortAwayFromTheWindow()
        {
            var l = new ShortLadderLogic();
            AssertStillPlaying(l);
            Assert.AreEqual(ShortLadderLogic.MinSegments, l.Segments);
            Assert.IsFalse(l.LongEnough);
            Assert.IsFalse(l.InColumn);
            Assert.IsFalse(l.Reaches);
            Assert.AreEqual(ShortLadderLogic.FloorY, l.BottomY, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.Less(l.TopY, 1.4f - 2f, "la scala corta non arriva neanche vicino alla finestra");
        }

        [Test]
        public void ShortLadderUnderTheWindowNeverWins()
        {
            var l = new ShortLadderLogic();
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(ShortLadderDrop.TooShort, DragLadder(l, Under + (i - 1.5f) * 0.3f));
                Assert.IsTrue(l.ClimbAttempt, "l'omino prova a salire");
                Assert.IsFalse(l.GrabMan(), "mentre sale non si prende");
                Assert.IsFalse(l.TapMan());
                l.ClimbAttemptDone(Under);
                Assert.IsFalse(l.ClimbAttempt);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(4, l.ShortClimbs);
            Assert.AreEqual(Under, l.ManX, 1e-5f, "l'omino resta dove è sceso");
        }

        [Test]
        public void LiftingTheShortLadderDoesNotHelpItDropsToTheFloor()
        {
            var l = new ShortLadderLogic();
            Assert.IsTrue(l.GrabLadder());
            Assert.IsTrue(l.MoveLadder(Under, 1.5f));
            Assert.Greater(l.BottomY, ShortLadderLogic.FloorY, "tenuta si può sollevare");
            Assert.LessOrEqual(l.TopY, ShortLadderLogic.MaxTopY + 1e-4f);
            Assert.AreEqual(ShortLadderDrop.TooShort, l.ReleaseLadder());
            Assert.AreEqual(ShortLadderLogic.FloorY, l.BottomY, 1e-5f, "posata torna a terra");
            AssertStillPlaying(l);
        }

        [Test]
        public void ShortLadderElsewhereIsJustPlaced()
        {
            var l = new ShortLadderLogic();
            Assert.AreEqual(ShortLadderDrop.Placed, DragLadder(l, -1.8f));
            Assert.AreEqual(ShortLadderDrop.Placed, DragLadder(l, 1.8f));
            Assert.IsFalse(l.ClimbAttempt);
            AssertStillPlaying(l);
        }

        [Test]
        public void LongLadderElsewhereNeverWins()
        {
            var l = new ShortLadderLogic();
            MakeLong(l);
            foreach (float x in new[] { -1.85f, -0.6f, 0f, 1.85f, 99f, -99f })
            {
                ShortLadderDrop d = DragLadder(l, x);
                if (l.InColumn) continue;
                Assert.AreEqual(ShortLadderDrop.WrongSpot, d, "x " + x);
                AssertStillPlaying(l);
            }
            Assert.GreaterOrEqual(l.WrongSpots, 5);
            Assert.AreEqual(0, l.ShortClimbs);
        }

        [Test]
        public void ColumnEdgesAreRespected()
        {
            var l = new ShortLadderLogic();
            MakeLong(l);
            Assert.AreEqual(ShortLadderDrop.WrongSpot, DragLadder(l, Under - ShortLadderLogic.ColumnHalfWidth - 0.05f));
            Assert.AreEqual(ShortLadderDrop.Reached, DragLadder(l, Under - ShortLadderLogic.ColumnHalfWidth + 0.05f));
        }

        [Test]
        public void ExtensionClampsBetweenMinAndMax()
        {
            var l = new ShortLadderLogic();
            Assert.IsTrue(l.StartPinch());
            Assert.AreEqual(ShortLadderLogic.MaxSegments, l.Pinch(50f));
            Assert.AreEqual(ShortLadderLogic.MaxSegments, l.Pinch(float.PositiveInfinity));
            Assert.AreEqual(ShortLadderLogic.MinSegments, l.Pinch(0.01f));
            Assert.AreEqual(ShortLadderLogic.MinSegments, l.Pinch(-3f));
            int before = l.Segments;
            Assert.AreEqual(before, l.Pinch(float.NaN));
            for (float r = 0f; r < 6f; r += 0.05f)
            {
                int s = l.Pinch(r);
                Assert.GreaterOrEqual(s, ShortLadderLogic.MinSegments);
                Assert.LessOrEqual(s, ShortLadderLogic.MaxSegments);
                Assert.LessOrEqual(ShortLadderLogic.FloorY + s * ShortLadderLogic.SegmentHeight, ShortLadderLogic.MaxTopY + 1e-4f, "la cima resta nello schermo");
            }
            l.EndPinch(false);
            AssertStillPlaying(l);
        }

        [Test]
        public void LengthStaysAfterThePinchAndCanShrinkBack()
        {
            var l = new ShortLadderLogic();
            Assert.AreEqual(ShortLadderDrop.Stretched, PinchTo(l, 1.5f));
            Assert.AreEqual(ShortLadderLogic.MinSegments + 4, l.Segments);
            Assert.AreEqual(ShortLadderDrop.WrongSpot, PinchTo(l, 1.75f), "ora è lunga, ma lontano dalla finestra");
            Assert.AreEqual(ShortLadderLogic.MaxSegments, l.Segments, "si somma (fino al massimo)");
            Assert.AreEqual(ShortLadderDrop.WrongSpot, PinchTo(l, 0.75f), "accorciata ma ancora lunga, lontano dalla finestra");
            Assert.AreEqual(ShortLadderLogic.MaxSegments - 2, l.Segments);
            Assert.AreEqual(ShortLadderDrop.Stretched, PinchTo(l, 0.1f));
            Assert.AreEqual(ShortLadderLogic.MinSegments, l.Segments, "mai sotto il minimo");
            Assert.AreEqual(ShortLadderDrop.Unchanged, PinchTo(l, 1f));
            AssertStillPlaying(l);
        }

        [Test]
        public void LongLadderUnderTheWindowWinsOnce()
        {
            var l = new ShortLadderLogic();
            MakeLong(l);
            Assert.AreEqual(ShortLadderDrop.Reached, DragLadder(l, Under));
            Assert.AreEqual(ShortLadderStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.IsFalse(l.LadderHeld || l.Pinching || l.ManHeld || l.ClimbAttempt);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "la vittoria arriva una volta sola");
            Assert.AreEqual(ShortLadderStage.Done, l.Stage);
            // dopo la vittoria non succede più niente
            Assert.IsFalse(l.GrabLadder());
            Assert.IsFalse(l.StartPinch());
            Assert.IsFalse(l.GrabMan());
            Assert.AreEqual(ShortLadderDrop.None, l.TapLadder());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ShortUnderTheWindowThenPinchLongInPlaceWins()
        {
            var l = new ShortLadderLogic();
            Assert.AreEqual(ShortLadderDrop.TooShort, DragLadder(l, Under));
            Assert.AreEqual(ShortLadderDrop.TooShort, PinchTo(l, 1.25f), "ancora corta: l'omino riprova");
            Assert.IsTrue(l.ClimbAttempt);
            Assert.AreEqual(ShortLadderDrop.Reached, PinchTo(l, 1.5f), "il pizzico interrompe la prova e la scala arriva");
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void PinchThenDragWithTheRemainingFinger()
        {
            var l = new ShortLadderLogic();
            Assert.IsTrue(l.GrabLadder());
            Assert.IsTrue(l.MoveLadder(-1f, -2f));
            Assert.IsTrue(l.StartPinch(), "il secondo dito trasforma il trascinamento in pizzico");
            Assert.IsFalse(l.LadderHeld);
            Assert.AreEqual(ShortLadderLogic.FloorY, l.BottomY, 1e-5f, "nel pizzico i piedi sono a terra");
            l.Pinch(2f);
            Assert.AreEqual(ShortLadderDrop.Stretched, l.EndPinch(true));
            Assert.IsTrue(l.LadderHeld, "l'altro dito continua a trascinare: niente blocchi");
            Assert.IsFalse(l.Pinching);
            Assert.IsTrue(l.MoveLadder(Under + 0.2f, ShortLadderLogic.FloorY + 0.3f));
            AssertStillPlaying(l);
            Assert.AreEqual(ShortLadderDrop.Reached, l.ReleaseLadder());
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void PinchThenDragAgainSeveralTimes()
        {
            var l = new ShortLadderLogic();
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(l.StartPinch());
                l.Pinch(1.1f);
                l.EndPinch(true);
                Assert.IsTrue(l.LadderHeld);
                Assert.IsTrue(l.MoveLadder(Away, ShortLadderLogic.FloorY));
                Assert.IsTrue(l.StartPinch(), "si può ripizzicare mentre la si tiene");
                l.EndPinch(false);
                Assert.IsFalse(l.LadderHeld || l.Pinching);
            }
            AssertStillPlaying(l);
            Assert.IsTrue(l.GrabLadder());
        }

        [Test]
        public void NeverThroughTheFloorNorOffScreen()
        {
            var l = new ShortLadderLogic();
            Assert.IsTrue(l.GrabLadder());
            foreach (float y in new[] { -99f, -5f, ShortLadderLogic.FloorY - 0.01f, float.NaN, float.NegativeInfinity })
            {
                l.MoveLadder(0f, y);
                Assert.GreaterOrEqual(l.BottomY, ShortLadderLogic.FloorY);
            }
            l.MoveLadder(99f, 99f);
            Assert.LessOrEqual(l.LadderX, ShortLadderLogic.MaxX);
            Assert.LessOrEqual(l.TopY, ShortLadderLogic.MaxTopY + 1e-4f);
            l.MoveLadder(-99f, float.PositiveInfinity);
            Assert.GreaterOrEqual(l.LadderX, ShortLadderLogic.MinX);
            Assert.LessOrEqual(l.TopY, ShortLadderLogic.MaxTopY + 1e-4f);
            l.MoveLadder(float.NaN, 0f);
            Assert.GreaterOrEqual(l.LadderX, ShortLadderLogic.MinX);
            l.ReleaseLadder();
            // lunga al massimo: sollevata resta comunque nello schermo
            PinchTo(l, 10f);
            Assert.AreEqual(ShortLadderLogic.MaxSegments, l.Segments);
            Assert.IsTrue(l.GrabLadder());
            l.MoveLadder(-1.2f, 50f);
            Assert.GreaterOrEqual(l.BottomY, ShortLadderLogic.FloorY);
            Assert.LessOrEqual(l.TopY, ShortLadderLogic.MaxTopY + 1e-4f);
            l.ReleaseLadder();
            Assert.AreEqual(ShortLadderLogic.FloorY, l.BottomY, 1e-5f);
            AssertStillPlaying(l);
        }

        [Test]
        public void MovingTheManIsUseless()
        {
            var l = new ShortLadderLogic();
            foreach (var p in new[] { (0f, 3f), (2f, 0f), (-99f, 99f), (0.9f, 2.5f) })
            {
                Assert.IsTrue(l.GrabMan());
                Assert.IsTrue(l.MoveMan(l.ManX + p.Item1, ShortLadderLogic.FloorY + p.Item2));
                Assert.LessOrEqual(l.ManY, ShortLadderLogic.FloorY + ShortLadderLogic.ManMaxLift + 1e-4f);
                Assert.GreaterOrEqual(l.ManX, ShortLadderLogic.ManMinX);
                Assert.LessOrEqual(l.ManX, ShortLadderLogic.ManMaxX);
                Assert.AreEqual(ShortLadderDrop.Jumped, l.ReleaseMan());
                Assert.AreEqual(ShortLadderLogic.FloorY, l.ManY, 1e-5f, "ricade a terra");
            }
            Assert.IsTrue(l.GrabMan());
            Assert.AreEqual(ShortLadderDrop.Unchanged, l.ReleaseMan(), "presa senza spostarlo");
            Assert.IsTrue(l.TapMan());
            Assert.AreEqual(4, l.ManDrags);
            AssertStillPlaying(l);
        }

        [Test]
        public void ManHeldWhileTheShortLadderLandsDoesNotClimb()
        {
            var l = new ShortLadderLogic();
            Assert.IsTrue(l.GrabMan());
            Assert.AreEqual(ShortLadderDrop.TooShort, DragLadder(l, Under));
            Assert.IsFalse(l.ClimbAttempt);
            Assert.IsTrue(l.ManHeld);
            l.ReleaseMan();
            AssertStillPlaying(l);
        }

        [Test]
        public void ManHeldWhileTheLongLadderLandsStillWins()
        {
            var l = new ShortLadderLogic();
            MakeLong(l);
            Assert.IsTrue(l.GrabMan());
            l.MoveMan(0f, -2f);
            Assert.AreEqual(ShortLadderDrop.Reached, DragLadder(l, Under));
            Assert.IsFalse(l.ManHeld);
            Assert.AreEqual(ShortLadderLogic.FloorY, l.ManY, 1e-5f);
            Assert.AreEqual(ShortLadderDrop.None, l.ReleaseMan());
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void GrabbingTheLadderInterruptsTheClimbAttempt()
        {
            var l = new ShortLadderLogic();
            Assert.AreEqual(ShortLadderDrop.TooShort, DragLadder(l, Under));
            Assert.IsTrue(l.ClimbAttempt);
            Assert.IsTrue(l.GrabLadder());
            Assert.IsFalse(l.ClimbAttempt);
            l.PlaceMan(Under);
            Assert.AreEqual(Under, l.ManX, 1e-5f, "l'omino è sceso lì");
            Assert.IsTrue(l.GrabMan(), "con un dito sulla scala, l'altro può prendere l'omino");
            Assert.AreEqual(ShortLadderDrop.Placed, DragLadderHeld(l, Away));
            Assert.AreEqual(ShortLadderDrop.Unchanged, l.ReleaseMan());
            AssertStillPlaying(l);
        }

        [Test]
        public void RapidTouchesNeverWinNorBreak()
        {
            var l = new ShortLadderLogic();
            for (int i = 0; i < 200; i++)
            {
                l.TapLadder();
                l.TapMan();
                l.GrabLadder();
                l.ReleaseLadder();
                l.StartPinch();
                l.Pinch(1f);
                l.EndPinch(i % 2 == 0);
                l.ReleaseLadder();
                l.GrabMan();
                l.ReleaseMan();
                l.ClimbAttemptDone(l.ManX);
            }
            AssertStillPlaying(l);
            Assert.IsFalse(l.LadderHeld || l.Pinching || l.ManHeld);
            Assert.AreEqual(400, l.LadderTaps + l.ManTaps);
            Assert.AreEqual(ShortLadderLogic.MinSegments, l.Segments);
        }

        [Test]
        public void DoublePressesAreIgnored()
        {
            var l = new ShortLadderLogic();
            Assert.IsTrue(l.GrabLadder());
            Assert.IsFalse(l.GrabLadder());
            Assert.AreEqual(ShortLadderDrop.Placed, l.ReleaseLadder());
            Assert.AreEqual(ShortLadderDrop.None, l.ReleaseLadder());
            Assert.IsTrue(l.StartPinch());
            Assert.IsFalse(l.StartPinch());
            Assert.IsFalse(l.GrabLadder(), "durante il pizzico la scala non si trascina");
            Assert.AreEqual(ShortLadderDrop.Unchanged, l.EndPinch(false));
            Assert.AreEqual(ShortLadderDrop.None, l.EndPinch(false));
            Assert.IsFalse(l.MoveLadder(0f, 0f), "non tenuta non si muove");
            Assert.IsFalse(l.MoveMan(0f, 0f));
            Assert.AreEqual(ShortLadderDrop.None, l.ReleaseMan());
            AssertStillPlaying(l);
        }

        [Test]
        public void TapOnAPlacedLongLadderUnderTheWindowWins()
        {
            var l = new ShortLadderLogic();
            MakeLong(l);
            Assert.IsTrue(l.GrabLadder());
            l.MoveLadder(Under, ShortLadderLogic.FloorY + 1f);
            Assert.IsTrue(l.Cancel(), "pausa mentre la si porta: niente vittoria");
            AssertStillPlaying(l);
            Assert.IsTrue(l.InColumn && l.LongEnough);
            Assert.AreEqual(ShortLadderDrop.Reached, l.TapLadder(), "un tocco basta, mai bloccati");
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void CancelLeavesAValidState()
        {
            var l = new ShortLadderLogic();
            Assert.IsTrue(l.StartPinch());
            l.Pinch(1.5f);
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.LadderHeld || l.Pinching || l.ManHeld || l.ClimbAttempt);
            Assert.AreEqual(ShortLadderLogic.MinSegments + 4, l.Segments, "la lunghezza raggiunta resta");
            Assert.AreEqual(ShortLadderLogic.FloorY, l.BottomY, 1e-5f);
            Assert.AreEqual(ShortLadderDrop.None, l.EndPinch(false));
            AssertStillPlaying(l);

            Assert.IsTrue(l.GrabMan());
            l.MoveMan(0f, 0f);
            Assert.IsTrue(l.GrabLadder());
            l.MoveLadder(1f, 0f);
            Assert.IsTrue(l.Cancel());
            Assert.AreEqual(ShortLadderLogic.FloorY, l.ManY, 1e-5f);
            Assert.AreEqual(ShortLadderLogic.FloorY, l.BottomY, 1e-5f);
            Assert.IsFalse(l.Cancel(), "niente da lasciare");
            Assert.AreEqual(0, l.WrongMoves, "le dita perse non contano come tentativi");
            AssertStillPlaying(l);

            // dopo Cancel si riprende normalmente e si vince
            Assert.AreEqual(ShortLadderDrop.TooShort, DragLadder(l, Under));
            Assert.IsTrue(l.Cancel(), "la prova di salita si interrompe");
            Assert.IsFalse(l.ClimbAttempt);
            Assert.AreEqual(ShortLadderDrop.Reached, PinchTo(l, 1.5f));
        }

        [Test]
        public void CancelDuringTheCelebrationChangesNothing()
        {
            var l = new ShortLadderLogic();
            MakeLong(l);
            DragLadder(l, Under);
            Assert.IsFalse(l.Cancel());
            Assert.AreEqual(ShortLadderStage.Celebrating, l.Stage);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresTheStart()
        {
            var l = new ShortLadderLogic();
            MakeLong(l);
            DragLadder(l, -1f);
            l.GrabMan();
            l.MoveMan(1f, 0f);
            l.ReleaseMan();
            DragLadder(l, Under);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(ShortLadderLogic.MinSegments, l.Segments);
            Assert.AreEqual(ShortLadderLogic.LadderStartX, l.LadderX, 1e-5f);
            Assert.AreEqual(ShortLadderLogic.FloorY, l.BottomY, 1e-5f);
            Assert.AreEqual(ShortLadderLogic.ManStartX, l.ManX, 1e-5f);
            Assert.AreEqual(ShortLadderLogic.FloorY, l.ManY, 1e-5f);
            Assert.IsFalse(l.LadderHeld || l.Pinching || l.ManHeld || l.ClimbAttempt);
            Assert.AreEqual(0, l.WrongMoves + l.Stretches + l.LadderTaps + l.ManTaps);
            // e si rivince, una volta sola
            MakeLong(l);
            Assert.AreEqual(ShortLadderDrop.Reached, DragLadder(l, Under));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ThreeWrongAttemptsThenTheSolution()
        {
            var l = new ShortLadderLogic();
            Assert.AreEqual(ShortLadderDrop.TooShort, DragLadder(l, Under));   // 1: scala corta sotto la finestra
            l.ClimbAttemptDone(Under);
            Assert.IsTrue(l.GrabMan());                                         // 2: far saltare l'omino
            l.MoveMan(Under, ShortLadderLogic.FloorY + 2f);
            Assert.AreEqual(ShortLadderDrop.Jumped, l.ReleaseMan());
            Assert.AreEqual(ShortLadderDrop.TooShort, DragLadder(l, Under, 1f)); // 3: sollevarla
            l.ClimbAttemptDone(Under);
            Assert.AreEqual(ShortLadderDrop.Placed, DragLadder(l, Away));
            MakeLong(l);                                                        // allungata lontano: ancora niente
            Assert.AreEqual(ShortLadderDrop.WrongSpot, DragLadder(l, -1.2f));
            Assert.AreEqual(5, l.WrongMoves);
            AssertStillPlaying(l);
            Assert.AreEqual(ShortLadderDrop.Reached, DragLadder(l, Under));
            Assert.IsTrue(l.TryComplete());
        }
    }
}
