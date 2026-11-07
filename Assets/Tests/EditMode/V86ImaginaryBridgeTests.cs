using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 110 della roadmap: The Imaginary Bridge (allungare l'asse col pizzico e posarla sul canyon).</summary>
    public class V86ImaginaryBridgeTests
    {
        const float Gap = ImaginaryBridgeLogic.BridgeX;
        const float Y = ImaginaryBridgeLogic.CliffY;

        static void AssertStillPlaying(ImaginaryBridgeLogic l)
        {
            Assert.AreEqual(ImaginaryBridgeStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        /// <summary>Trascina l'asse a passi fino a (x, y) e la lascia.</summary>
        static ImaginaryBridgeDrop DragPlank(ImaginaryBridgeLogic l, float x, float y = Y + 0.6f)
        {
            Assert.IsTrue(l.GrabPlank());
            float x0 = l.PlankX, y0 = l.PlankY;
            for (int i = 1; i <= 8; i++)
                Assert.IsTrue(l.MovePlank(x0 + (x - x0) * i / 8f, y0 + (y - y0) * i / 8f));
            return l.ReleasePlank();
        }

        static ImaginaryBridgeDrop PinchTo(ImaginaryBridgeLogic l, float ratio, bool keepDragging = false)
        {
            Assert.IsTrue(l.StartPinch());
            for (int i = 1; i <= 6; i++) l.Pinch(1f + (ratio - 1f) * i / 6f);
            return l.EndPinch(keepDragging);
        }

        /// <summary>Allunga l'asse al suo posto (sulla piattaforma di sinistra) fino a una lunghezza che basta.</summary>
        static void MakeLong(ImaginaryBridgeLogic l, float ratio = 2.6f)
        {
            Assert.AreEqual(ImaginaryBridgeDrop.WrongSpot, PinchTo(l, ratio));
            Assert.IsTrue(l.LongEnough);
            Assert.IsFalse(l.OverGap);
            AssertStillPlaying(l);
        }

        [Test]
        public void StartsShortOnTheLeftPlatform()
        {
            var l = new ImaginaryBridgeLogic();
            AssertStillPlaying(l);
            Assert.AreEqual(ImaginaryBridgeLogic.MinLength, l.Length, 1e-5f);
            Assert.IsFalse(l.LongEnough);
            Assert.IsFalse(l.OverGap);
            Assert.IsFalse(l.Spans);
            Assert.AreEqual(Y, l.PlankY, 1e-5f);
            Assert.Less(l.RightEnd, ImaginaryBridgeLogic.LeftEdgeX, "l'asse è tutta sulla roccia");
            Assert.AreEqual(ImaginaryBridgeLogic.ManStartX, l.ManX, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.Less(ImaginaryBridgeLogic.MinLength, ImaginaryBridgeLogic.RightEdgeX - ImaginaryBridgeLogic.LeftEdgeX, "l'asse corta è più corta del canyon");
        }

        [Test]
        public void ShortPlankAcrossTheGapFallsBackHome()
        {
            var l = new ImaginaryBridgeLogic();
            float[] xs = { Gap, -0.5f, 0.5f, 0.2f };
            for (int i = 0; i < xs.Length; i++)
            {
                Assert.AreEqual(ImaginaryBridgeDrop.TooShort, DragPlank(l, xs[i]));
                Assert.IsTrue(l.Falling, "cade nel canyon");
                Assert.AreEqual(xs[i], l.FallFromX, 1e-4f);
                Assert.AreEqual(ImaginaryBridgeLogic.PlankStartX, l.PlankX, 1e-5f, "ricompare al suo posto");
                Assert.AreEqual(Y, l.PlankY, 1e-5f);
                Assert.IsFalse(l.GrabPlank(), "mentre cade non si prende");
                Assert.IsFalse(l.StartPinch());
                Assert.IsFalse(l.TapPlank());
                l.FallDone();
                Assert.IsFalse(l.Falling);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(4, l.ShortFalls);
            Assert.AreEqual(4, l.WrongMoves);
            Assert.AreEqual(ImaginaryBridgeLogic.MinLength, l.Length, 1e-5f);
        }

        [Test]
        public void ShortPlankDroppedHighAboveTheGapStillFalls()
        {
            var l = new ImaginaryBridgeLogic();
            Assert.AreEqual(ImaginaryBridgeDrop.TooShort, DragPlank(l, Gap, Y + 5f));
            Assert.AreEqual(Y + ImaginaryBridgeLogic.MaxLift, l.FallFromY, 1e-4f, "sollevata al massimo, non oltre");
            l.FallDone();
            AssertStillPlaying(l);
        }

        [Test]
        public void ShortPlankOnAPlatformJustRests()
        {
            var l = new ImaginaryBridgeLogic();
            Assert.AreEqual(ImaginaryBridgeDrop.Placed, DragPlank(l, 1.6f));
            Assert.AreEqual(1.6f, l.PlankX, 1e-4f);
            Assert.AreEqual(Y, l.PlankY, 1e-5f);
            Assert.IsFalse(l.Falling);
            Assert.AreEqual(ImaginaryBridgeDrop.Placed, DragPlank(l, -1.9f));
            Assert.AreEqual(ImaginaryBridgeDrop.Placed, DragPlank(l, -0.85f), "sporge sul vuoto ma il centro è sulla roccia");
            Assert.IsFalse(l.Falling);
            Assert.AreEqual(0, l.WrongMoves);
            AssertStillPlaying(l);
        }

        [Test]
        public void LongPlankElsewhereDoesNotWin()
        {
            var l = new ImaginaryBridgeLogic();
            MakeLong(l);
            // sulla piattaforma di destra, su quella di sinistra
            Assert.AreEqual(ImaginaryBridgeDrop.WrongSpot, DragPlank(l, 1.3f));
            Assert.AreEqual(ImaginaryBridgeDrop.WrongSpot, DragPlank(l, -1.3f));
            AssertStillPlaying(l);
            // sul canyon ma con un capo nel vuoto: cade e torna al suo posto, lunga com'era
            float len = l.Length;
            Assert.AreEqual(ImaginaryBridgeDrop.Slipped, DragPlank(l, 0.75f));
            Assert.IsTrue(l.Falling);
            Assert.IsFalse(ImaginaryBridgeLogic.SpansAt(l.PlankX, l.Length));
            l.FallDone();
            Assert.AreEqual(len, l.Length, 1e-5f, "la lunghezza resta");
            Assert.AreEqual(ImaginaryBridgeDrop.Slipped, DragPlank(l, -0.7f));
            l.FallDone();
            Assert.AreEqual(3, l.WrongSpots + 0, "una dal pizzico, due posate");
            Assert.AreEqual(2, l.Slips);
            AssertStillPlaying(l);
        }

        [Test]
        public void LengthClampsAndPlankStaysOnScreen()
        {
            var l = new ImaginaryBridgeLogic();
            PinchTo(l, 50f);
            Assert.AreEqual(ImaginaryBridgeLogic.MaxLength, l.Length, 1e-5f);
            Assert.GreaterOrEqual(l.LeftEnd, -ImaginaryBridgeLogic.ScreenHalf - 1e-4f);
            Assert.LessOrEqual(l.RightEnd, ImaginaryBridgeLogic.ScreenHalf + 1e-4f);
            Assert.IsFalse(l.OverGap, "allungata al suo posto resta con il centro sulla roccia");
            AssertStillPlaying(l);
            PinchTo(l, 0.01f);
            Assert.AreEqual(ImaginaryBridgeLogic.MinLength, l.Length, 1e-5f);
            PinchTo(l, float.NaN);
            PinchTo(l, float.PositiveInfinity);
            Assert.AreEqual(ImaginaryBridgeLogic.MinLength, l.Length, 1e-5f);
            // trascinata oltre i bordi: i capi restano nello schermo, mai sotto il piano delle rocce
            PinchTo(l, 3f);
            Assert.IsTrue(l.GrabPlank());
            l.MovePlank(10f, -10f);
            Assert.LessOrEqual(l.RightEnd, ImaginaryBridgeLogic.ScreenHalf + 1e-4f);
            Assert.AreEqual(Y, l.PlankY, 1e-5f);
            l.MovePlank(-10f, 10f);
            Assert.GreaterOrEqual(l.LeftEnd, -ImaginaryBridgeLogic.ScreenHalf - 1e-4f);
            Assert.AreEqual(Y + ImaginaryBridgeLogic.MaxLift, l.PlankY, 1e-5f);
            l.MovePlank(float.NaN, float.NaN);
            Assert.IsFalse(float.IsNaN(l.PlankX));
            l.ReleasePlank();
            AssertStillPlaying(l);
        }

        [Test]
        public void HomeNeverSpansNorHangsOverTheGap()
        {
            for (float len = ImaginaryBridgeLogic.MinLength; len <= ImaginaryBridgeLogic.MaxLength + 1e-4f; len += 0.05f)
            {
                float h = ImaginaryBridgeLogic.HomeX(len);
                Assert.IsFalse(ImaginaryBridgeLogic.SpansAt(h, len), "len " + len);
                Assert.Less(h, ImaginaryBridgeLogic.LeftEdgeX, "len " + len);
                Assert.GreaterOrEqual(h - len * 0.5f, -ImaginaryBridgeLogic.ScreenHalf - 1e-4f, "len " + len);
            }
        }

        [Test]
        public void LongPlankAcrossTheGapWinsOnce()
        {
            var l = new ImaginaryBridgeLogic();
            MakeLong(l);
            Assert.AreEqual(ImaginaryBridgeDrop.Bridged, DragPlank(l, 0.1f));
            Assert.AreEqual(ImaginaryBridgeStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(ImaginaryBridgeLogic.BridgeX, l.PlankX, 1e-5f, "scatta nello stato stabile, centrata");
            Assert.AreEqual(Y, l.PlankY, 1e-5f);
            Assert.IsTrue(l.Spans);
            // dopo la vittoria niente risponde
            Assert.IsFalse(l.GrabPlank());
            Assert.IsFalse(l.StartPinch());
            Assert.IsFalse(l.GrabMan());
            Assert.IsFalse(l.TapMan());
            Assert.IsFalse(l.TapPlank());
            Assert.AreEqual(ImaginaryBridgeDrop.None, l.ReleasePlank());
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "la vittoria una volta sola");
            Assert.AreEqual(ImaginaryBridgeStage.Done, l.Stage);
            l.Cancel();
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SnapZonesAreExact()
        {
            float need = ImaginaryBridgeLogic.NeedLength;
            Assert.IsTrue(ImaginaryBridgeLogic.SpansAt(0f, need));
            Assert.IsFalse(ImaginaryBridgeLogic.SpansAt(0f, need - 0.02f));
            Assert.IsFalse(ImaginaryBridgeLogic.SpansAt(0.02f, need), "capo sinistro fuori dalla sua zona");
            Assert.IsFalse(ImaginaryBridgeLogic.SpansAt(-0.02f, need), "capo destro fuori dalla sua zona");
            float len = 3f, tol = (len - (ImaginaryBridgeLogic.RightEdgeX - ImaginaryBridgeLogic.LeftEdgeX)) * 0.5f - ImaginaryBridgeLogic.Support;
            Assert.IsTrue(ImaginaryBridgeLogic.SpansAt(tol - 0.01f, len));
            Assert.IsTrue(ImaginaryBridgeLogic.SpansAt(-tol + 0.01f, len));
            Assert.IsFalse(ImaginaryBridgeLogic.SpansAt(tol + 0.02f, len));
            Assert.IsFalse(ImaginaryBridgeLogic.SpansAt(-tol - 0.02f, len));
        }

        [Test]
        public void PinchRightOverTheGapWins()
        {
            // asse corta portata sul canyon e allungata lì (il primo dito la tiene, il secondo pizzica)
            var l = new ImaginaryBridgeLogic();
            Assert.IsTrue(l.GrabPlank());
            Assert.IsTrue(l.MovePlank(0f, Y + 0.5f));
            Assert.IsTrue(l.StartPinch());
            Assert.IsFalse(l.PlankHeld);
            Assert.IsTrue(l.Pinching);
            l.Pinch(1.5f);
            Assert.AreEqual(0f, l.PlankX, 1e-5f, "il pizzico non sposta l'asse");
            l.Pinch(2.5f);
            Assert.AreEqual(ImaginaryBridgeDrop.Bridged, l.EndPinch(false));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void PinchThenDragWithoutLock()
        {
            var l = new ImaginaryBridgeLogic();
            Assert.AreEqual(ImaginaryBridgeDrop.Stretched, PinchTo(l, 2.6f, true));
            Assert.IsTrue(l.PlankHeld, "il dito rimasto tiene l'asse");
            Assert.IsFalse(l.Pinching);
            Assert.IsTrue(l.MovePlank(-0.3f, Y + 0.8f));
            Assert.IsTrue(l.MovePlank(0.05f, Y + 0.4f));
            Assert.AreEqual(ImaginaryBridgeDrop.Bridged, l.ReleasePlank());
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void DragThenPinchThenDragAgain()
        {
            var l = new ImaginaryBridgeLogic();
            Assert.IsTrue(l.GrabPlank());
            l.MovePlank(-1.0f, Y + 0.3f);
            Assert.IsTrue(l.StartPinch(), "il secondo dito trasforma il trascinamento in pizzico");
            l.Pinch(1.4f);
            Assert.AreEqual(ImaginaryBridgeDrop.Stretched, l.EndPinch(true));
            Assert.IsTrue(l.StartPinch(), "di nuovo due dita: altro pizzico, niente blocchi");
            l.Pinch(1.6f);
            Assert.AreEqual(1.4f * 1.6f, l.Length, 1e-4f, "il pizzico riparte dalla lunghezza raggiunta");
            l.EndPinch(true);
            Assert.IsTrue(l.MovePlank(0f, Y + 0.3f));
            Assert.AreEqual(ImaginaryBridgeDrop.Bridged, l.ReleasePlank());
        }

        [Test]
        public void ShrinkingBelowNeedOverTheGapFalls()
        {
            var l = new ImaginaryBridgeLogic();
            MakeLong(l, 3f);
            Assert.IsTrue(l.GrabPlank());
            l.MovePlank(0f, Y + 0.5f);
            Assert.IsTrue(l.StartPinch());
            l.Pinch(0.5f);
            Assert.IsFalse(l.LongEnough);
            Assert.AreEqual(ImaginaryBridgeDrop.TooShort, l.EndPinch(false));
            Assert.IsTrue(l.Falling);
            l.FallDone();
            AssertStillPlaying(l);
        }

        [Test]
        public void ManCannotCrossBefore()
        {
            var l = new ImaginaryBridgeLogic();
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(l.GrabMan());
                for (int k = 0; k < 10; k++) l.MoveMan(-2f + k * 0.8f);
                Assert.LessOrEqual(l.ManX, ImaginaryBridgeLogic.ManEdgeX + 1e-5f, "al bordo, non oltre");
                Assert.Less(l.ManX, ImaginaryBridgeLogic.LeftEdgeX);
                Assert.AreEqual(ImaginaryBridgeDrop.Refused, l.ReleaseMan());
                Assert.AreEqual(ImaginaryBridgeLogic.ManEdgeX - ImaginaryBridgeLogic.StepBack, l.ManX, 1e-5f, "passo indietro");
                AssertStillPlaying(l);
            }
            Assert.AreEqual(3, l.Refusals);
            // spostato sulla sua piattaforma: niente di grave
            Assert.IsTrue(l.GrabMan());
            l.MoveMan(-2.2f);
            Assert.AreEqual(ImaginaryBridgeDrop.Walked, l.ReleaseMan());
            Assert.IsTrue(l.GrabMan());
            l.MoveMan(-10f);
            Assert.AreEqual(ImaginaryBridgeLogic.ManMinX, l.ManX, 1e-5f);
            Assert.AreEqual(ImaginaryBridgeDrop.Unchanged, l.ReleaseMan());
            Assert.IsTrue(l.TapMan());
            Assert.AreEqual(3, l.WrongMoves);
            AssertStillPlaying(l);
        }

        [Test]
        public void ManAndPlankHeldByTwoFingers()
        {
            var l = new ImaginaryBridgeLogic();
            Assert.IsTrue(l.GrabMan());
            Assert.IsTrue(l.GrabPlank());
            Assert.IsFalse(l.GrabMan());
            Assert.IsFalse(l.GrabPlank());
            l.MoveMan(-1.5f);
            l.MovePlank(0f, Y + 0.4f);
            Assert.AreEqual(ImaginaryBridgeDrop.TooShort, l.ReleasePlank());
            Assert.AreEqual(ImaginaryBridgeDrop.Walked, l.ReleaseMan());
            l.FallDone();
            AssertStillPlaying(l);
        }

        [Test]
        public void RapidTapsAndGrabsNeverWin()
        {
            var l = new ImaginaryBridgeLogic();
            for (int i = 0; i < 200; i++)
            {
                l.TapPlank();
                l.TapMan();
                if (l.GrabPlank()) l.ReleasePlank();
                if (l.GrabMan()) l.ReleaseMan();
                if (l.StartPinch()) l.EndPinch(i % 2 == 0);
                if (l.PlankHeld) l.ReleasePlank();
            }
            Assert.AreEqual(ImaginaryBridgeLogic.MinLength, l.Length, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
            AssertStillPlaying(l);
            Assert.AreEqual(ImaginaryBridgeDrop.None, l.ReleasePlank());
            Assert.AreEqual(ImaginaryBridgeDrop.None, l.ReleaseMan());
            Assert.AreEqual(ImaginaryBridgeDrop.None, l.EndPinch(false));
        }

        [Test]
        public void EarlyReleaseDuringDragIsSafe()
        {
            var l = new ImaginaryBridgeLogic();
            Assert.IsTrue(l.GrabPlank());
            l.MovePlank(-1.3f, Y + 0.05f);
            Assert.AreEqual(ImaginaryBridgeDrop.Unchanged, l.ReleasePlank(), "lasciata quasi subito: nessun effetto");
            Assert.AreEqual(Y, l.PlankY, 1e-5f);
            AssertStillPlaying(l);
        }

        [Test]
        public void CancelKeepsAValidStateWithoutWinning()
        {
            var l = new ImaginaryBridgeLogic();
            // asse lunga tenuta proprio sopra il canyon: la pausa non fa vincere, l'asse torna al suo posto
            MakeLong(l);
            Assert.IsTrue(l.GrabPlank());
            l.MovePlank(0f, Y + 0.6f);
            Assert.IsTrue(l.Cancel());
            AssertStillPlaying(l);
            Assert.IsFalse(l.PlankHeld);
            Assert.IsFalse(l.OverGap);
            Assert.AreEqual(ImaginaryBridgeLogic.HomeX(l.Length), l.PlankX, 1e-5f);
            Assert.AreEqual(Y, l.PlankY, 1e-5f);
            Assert.IsTrue(l.LongEnough, "la lunghezza resta");
            // a metà pizzico
            Assert.IsTrue(l.StartPinch());
            l.Pinch(0.6f);
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Pinching);
            Assert.IsTrue(l.GrabPlank(), "dopo la pausa si riprende");
            l.ReleasePlank();
            // asse tenuta sopra una piattaforma: resta lì
            Assert.IsTrue(l.GrabPlank());
            l.MovePlank(1.7f, Y + 1f);
            l.Cancel();
            Assert.AreEqual(1.7f, l.PlankX, 1e-4f);
            Assert.AreEqual(Y, l.PlankY, 1e-5f);
            // durante la caduta e con l'omino in mano
            DragPlank(l, 0f);
            Assert.IsTrue(l.Falling || l.Spans);
            Assert.IsTrue(l.GrabMan());
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Falling);
            Assert.IsFalse(l.ManHeld);
            Assert.IsFalse(l.Cancel(), "niente da lasciare");
            AssertStillPlaying(l);
            // e alla fine si vince ancora normalmente
            PinchTo(l, 3f);
            Assert.AreEqual(ImaginaryBridgeDrop.Bridged, DragPlank(l, 0f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresTheStart()
        {
            var l = new ImaginaryBridgeLogic();
            MakeLong(l);
            DragPlank(l, 0.75f);
            l.GrabMan();
            l.MoveMan(0f);
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(ImaginaryBridgeLogic.MinLength, l.Length, 1e-5f);
            Assert.AreEqual(ImaginaryBridgeLogic.PlankStartX, l.PlankX, 1e-5f);
            Assert.AreEqual(Y, l.PlankY, 1e-5f);
            Assert.AreEqual(ImaginaryBridgeLogic.ManStartX, l.ManX, 1e-5f);
            Assert.IsFalse(l.Falling || l.PlankHeld || l.Pinching || l.ManHeld);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Stretches);
            // vinto, resettato, rivinto: di nuovo una volta sola
            PinchTo(l, 3f);
            DragPlank(l, 0f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            PinchTo(l, 3f);
            Assert.AreEqual(ImaginaryBridgeDrop.Bridged, DragPlank(l, 0f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ThreeWrongAttemptsThenTheSolution()
        {
            var l = new ImaginaryBridgeLogic();
            Assert.AreEqual(ImaginaryBridgeDrop.TooShort, DragPlank(l, 0f));   // 1: asse corta sul canyon
            l.FallDone();
            l.GrabMan(); l.MoveMan(0f);
            Assert.AreEqual(ImaginaryBridgeDrop.Refused, l.ReleaseMan());       // 2: omino al bordo
            Assert.AreEqual(ImaginaryBridgeDrop.WrongSpot, PinchTo(l, 2.2f));   // 3: lunga ma sulla roccia
            Assert.AreEqual(ImaginaryBridgeDrop.Slipped, DragPlank(l, 0.6f));   // 4: lunga ma storta
            l.FallDone();
            Assert.AreEqual(4, l.WrongMoves);
            AssertStillPlaying(l);
            Assert.AreEqual(ImaginaryBridgeDrop.Bridged, DragPlank(l, -0.05f));
            Assert.IsTrue(l.TryComplete());
        }
    }
}
