using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 63 della roadmap: The Tiny Crown (allargare la corona col pizzico, poi metterla sulla testa).</summary>
    public class V84TinyCrownTests
    {
        const float HomeX = 1.65f, HomeY = -1.28f, TopX = -0.5f, TopY = 1.8f;

        static TinyCrownLogic New() => new TinyCrownLogic(HomeX, HomeY, TopX, TopY);

        static void AssertStillPlaying(TinyCrownLogic l)
        {
            Assert.AreEqual(TinyCrownStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.Crowned);
            Assert.IsFalse(l.TryComplete());
        }

        /// <summary>Prende la corona dov'è e la porta col dito fino a (x, y) in qualche passo, poi la lascia.</summary>
        static TinyCrownDrop DragTo(TinyCrownLogic l, float x, float y, int steps = 8)
        {
            float fx = l.X + 0.05f, fy = l.Y - 0.05f;   // il dito non è proprio al centro
            Assert.IsTrue(l.Grab(fx, fy));
            float tx = x + 0.05f, ty = y - 0.05f;
            for (int i = 1; i <= steps; i++) l.Drag(fx + (tx - fx) * i / steps, fy + (ty - fy) * i / steps);
            Assert.AreEqual(x, l.X, 1e-3f);
            Assert.AreEqual(y, l.Y, 1e-3f);
            return l.Release();
        }

        /// <summary>Pizzico da d0 a d1 e due dita alzate insieme.</summary>
        static TinyCrownDrop PinchTo(TinyCrownLogic l, float d0, float d1)
        {
            Assert.IsTrue(l.StartPinch(d0));
            for (int i = 1; i <= 6; i++) l.Pinch(d0 + (d1 - d0) * i / 6f);
            return l.EndPinch(false);
        }

        [Test]
        public void StartsTinyOnTheCushion()
        {
            var l = New();
            AssertStillPlaying(l);
            Assert.AreEqual(HomeX, l.X, 1e-5f);
            Assert.AreEqual(HomeY, l.Y, 1e-5f);
            Assert.AreEqual(TinyCrownLogic.MinScale, l.Scale, 1e-5f);
            Assert.IsFalse(l.BigEnough);
            Assert.IsFalse(l.Held || l.Pinching);
            Assert.IsFalse(l.OverHead);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void SmallCrownOnHeadFallsBackToCushion()
        {
            var l = New();
            // tre tentativi diversi nella zona della testa, sempre troppo piccola
            Assert.AreEqual(TinyCrownDrop.TooSmall, DragTo(l, TopX, TopY));
            Assert.AreEqual(TinyCrownDrop.TooSmall, DragTo(l, TopX + 1f, TopY - 1.4f));
            Assert.AreEqual(TinyCrownDrop.TooSmall, DragTo(l, TopX - 1.1f, TopY + 0.6f));
            Assert.AreEqual(3, l.SmallTries);
            Assert.AreEqual(HomeX, l.X, 1e-5f);
            Assert.AreEqual(HomeY, l.Y, 1e-5f);
            AssertStillPlaying(l);
        }

        [Test]
        public void AlmostBigEnoughStillFallsAndKeepsItsSize()
        {
            var l = New();
            PinchTo(l, 1f, 2f);   // scala 2 < 2.2
            Assert.AreEqual(2f, l.Scale, 1e-4f);
            Assert.IsFalse(l.BigEnough);
            Assert.AreEqual(TinyCrownDrop.TooSmall, DragTo(l, TopX, TopY));
            Assert.AreEqual(2f, l.LastTryScale, 1e-4f);
            Assert.AreEqual(2f, l.Scale, 1e-4f, "torna sul cuscino ma tiene la misura (niente lavoro perso)");
            Assert.AreEqual(HomeX, l.X, 1e-5f);
            AssertStillPlaying(l);
        }

        [Test]
        public void PinchClampsBetweenMinAndMax()
        {
            var l = New();
            Assert.IsTrue(l.StartPinch(1f));
            Assert.AreEqual(TinyCrownLogic.MaxScale, l.Pinch(50f), 1e-5f);
            Assert.AreEqual(TinyCrownLogic.MinScale, l.Pinch(0.01f), 1e-5f);
            Assert.AreEqual(TinyCrownLogic.MinScale, l.Pinch(0f), 1e-5f);
            Assert.AreEqual(1.5f, l.Pinch(1.5f), 1e-5f);
            Assert.AreEqual(1.5f, l.Pinch(float.NaN), 1e-5f, "valori assurdi ignorati");
            Assert.AreEqual(TinyCrownLogic.MaxScale, l.Pinch(float.MaxValue), 1e-5f);
            l.EndPinch(false);
            // un secondo pizzico parte dalla scala attuale (3): stringere la rimpicciolisce, mai sotto 1
            Assert.IsTrue(l.StartPinch(2f));
            Assert.AreEqual(1.5f, l.Pinch(1f), 1e-4f);
            Assert.AreEqual(TinyCrownLogic.MinScale, l.Pinch(0.1f), 1e-5f);
            l.EndPinch(false);
            // dita vicinissime all'inizio: la distanza iniziale è limitata, niente salti enormi
            Assert.IsTrue(l.StartPinch(0.01f));
            Assert.AreEqual(TinyCrownLogic.MinScale * 0.5f / TinyCrownLogic.MinPinchDistance, l.Pinch(0.5f), 1e-4f);
            l.EndPinch(false);
            AssertStillPlaying(l);
        }

        [Test]
        public void BigCrownOnHeadWinsOnceAndSnaps()
        {
            var l = New();
            Assert.AreEqual(TinyCrownDrop.Placed, PinchTo(l, 1f, 2.5f));
            Assert.IsTrue(l.BigEnough);
            Assert.AreEqual(TinyCrownDrop.Crowned, DragTo(l, TopX + 0.8f, TopY - 1.0f));   // un po' storta: si aggancia
            Assert.IsTrue(l.Crowned);
            Assert.AreEqual(TopX, l.X, 1e-5f);
            Assert.AreEqual(TopY, l.Y, 1e-5f);
            Assert.AreEqual(TinyCrownLogic.CrownedScale, l.Scale, 1e-5f);
            Assert.AreEqual(TinyCrownStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            // input bloccato durante la festa
            Assert.IsFalse(l.Grab(TopX, TopY));
            Assert.IsFalse(l.StartPinch(1f));
            Assert.IsFalse(l.TapKing());
            Assert.IsFalse(l.TapCrown());
            Assert.IsFalse(l.Cancel());
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "la vittoria arriva una volta sola");
            Assert.AreEqual(TinyCrownStage.Done, l.Stage);
        }

        [Test]
        public void MaxSizeCrownAlsoWins()
        {
            var l = New();
            PinchTo(l, 1f, 9f);
            Assert.AreEqual(TinyCrownLogic.MaxScale, l.Scale, 1e-5f);
            Assert.AreEqual(TinyCrownDrop.Crowned, DragTo(l, TopX, TopY + 0.5f));
            Assert.AreEqual(TinyCrownLogic.CrownedScale, l.Scale, 1e-5f, "si aggancia con la misura giusta");
        }

        [Test]
        public void BigCrownOffHeadStaysWhereReleasedNoWin()
        {
            var l = New();
            PinchTo(l, 1f, 3f);
            Assert.AreEqual(TinyCrownDrop.Placed, DragTo(l, -1.8f, -2.5f));
            Assert.AreEqual(TinyCrownDrop.Placed, DragTo(l, 1.9f, 2.0f));
            Assert.AreEqual(TinyCrownDrop.Placed, DragTo(l, TopX + 1.3f, TopY));   // appena fuori dalla zona
            Assert.AreEqual(3, l.OffHeadDrops);
            Assert.AreEqual(TopX + 1.3f, l.X, 1e-4f);
            Assert.AreEqual(TopY, l.Y, 1e-4f);
            Assert.AreEqual(TinyCrownLogic.MaxScale, l.Scale, 1e-5f);
            AssertStillPlaying(l);
            // e da lì si riprende e si mette sulla testa
            Assert.AreEqual(TinyCrownDrop.Crowned, DragTo(l, TopX, TopY));
        }

        [Test]
        public void SnapZoneEdges()
        {
            var l = New();
            Assert.IsTrue(l.OnHead(TopX, TopY));
            Assert.IsTrue(l.OnHead(TopX + TinyCrownLogic.SnapHalfWidth, TopY));
            Assert.IsTrue(l.OnHead(TopX - TinyCrownLogic.SnapHalfWidth, TopY - TinyCrownLogic.SnapBelow));
            Assert.IsTrue(l.OnHead(TopX, TopY + TinyCrownLogic.SnapAbove));
            Assert.IsFalse(l.OnHead(TopX + TinyCrownLogic.SnapHalfWidth + 0.01f, TopY));
            Assert.IsFalse(l.OnHead(TopX, TopY - TinyCrownLogic.SnapBelow - 0.01f));
            Assert.IsFalse(l.OnHead(TopX, TopY + TinyCrownLogic.SnapAbove + 0.01f));
            Assert.IsFalse(l.OnHead(HomeX, HomeY), "il cuscino non è nella zona della testa");
        }

        [Test]
        public void DragIsClampedInsideTheScreen()
        {
            var l = New();
            Assert.IsTrue(l.Grab(l.X, l.Y));
            l.Drag(-40f, 40f);
            Assert.AreEqual(TinyCrownLogic.MinX, l.X, 1e-5f);
            Assert.AreEqual(TinyCrownLogic.MaxY, l.Y, 1e-5f);
            l.Drag(40f, -40f);
            Assert.AreEqual(TinyCrownLogic.MaxX, l.X, 1e-5f);
            Assert.AreEqual(TinyCrownLogic.MinY, l.Y, 1e-5f);
            Assert.IsFalse(l.Drag(float.NaN, 0f));
            Assert.AreEqual(TinyCrownDrop.Placed, l.Release());
            AssertStillPlaying(l);
        }

        [Test]
        public void PinchThenDragWithRemainingFinger()
        {
            var l = New();
            // un dito trascina un po' la corona, il secondo arriva: pizzico (il trascinamento si interrompe senza valutarlo)
            Assert.IsTrue(l.Grab(HomeX, HomeY));
            l.Drag(HomeX - 0.5f, HomeY);
            Assert.IsTrue(l.StartPinch(0.8f));
            Assert.IsFalse(l.Held);
            Assert.AreEqual(0, l.WrongMoves);
            l.Pinch(2.0f);   // 0.8 -> 2.0 = scala 2.5
            Assert.IsTrue(l.BigEnough);
            float cx = l.X, cy = l.Y;
            // si alza un dito: l'altro (in un punto qualunque della corona) continua a trascinare, senza salti
            Assert.AreEqual(TinyCrownDrop.None, l.EndPinch(true, cx + 0.3f, cy - 0.2f));
            Assert.IsTrue(l.Held);
            Assert.IsFalse(l.Pinching);
            Assert.AreEqual(1, l.Pinches);
            l.Drag(cx + 0.3f, cy - 0.2f);
            Assert.AreEqual(cx, l.X, 1e-4f, "nessun salto al passaggio");
            Assert.AreEqual(cy, l.Y, 1e-4f);
            l.Drag(TopX + 0.3f, TopY - 0.2f);
            Assert.AreEqual(TinyCrownDrop.Crowned, l.Release());
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void PinchEndedOverHeadIsADrop()
        {
            var l = New();
            PinchTo(l, 1f, 3f);
            DragTo(l, TopX + 1.4f, TopY);   // fuori di poco: resta lì
            AssertStillPlaying(l);
            // rimpicciolita col pizzico lì vicino e trascinata sulla testa: cade
            PinchTo(l, 1f, 0.5f);
            Assert.AreEqual(1.5f, l.Scale, 1e-4f);
            Assert.AreEqual(TinyCrownDrop.TooSmall, DragTo(l, TopX, TopY));
            // pizzico che finisce con la corona grande già sopra la testa (senza trascinare): vale come rilascio
            var m = New();
            PinchTo(m, 1f, 3f);
            Assert.IsTrue(m.Grab(m.X, m.Y));
            m.Drag(TopX, TopY);
            Assert.IsTrue(m.StartPinch(1f));
            m.Pinch(1f);
            Assert.AreEqual(TinyCrownDrop.Crowned, m.EndPinch(false));
        }

        [Test]
        public void RapidTouchesNeverWin()
        {
            var l = New();
            for (int i = 0; i < 60; i++)
            {
                Assert.IsTrue(l.TapKing());
                Assert.IsTrue(l.TapCrown());
                Assert.IsTrue(l.Grab(l.X, l.Y));
                Assert.IsFalse(l.Grab(l.X, l.Y), "un solo dito alla volta trascina");
                Assert.AreEqual(TinyCrownDrop.Placed, l.Release());   // tocco senza movimento: non conta
                Assert.AreEqual(TinyCrownDrop.None, l.Release());
                Assert.AreEqual(TinyCrownDrop.None, l.EndPinch(false));
            }
            Assert.AreEqual(60, l.KingTaps);
            Assert.AreEqual(60, l.CrownTaps);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(HomeX, l.X, 1e-5f);
            AssertStillPlaying(l);
        }

        [Test]
        public void EarlyReleaseAndCancelAreValid()
        {
            var l = New();
            // rilascio anticipato a metà strada: resta lì, niente vittoria
            Assert.IsTrue(l.Grab(l.X, l.Y));
            l.Drag(0.5f, 0f);
            Assert.AreEqual(TinyCrownDrop.Placed, l.Release());
            AssertStillPlaying(l);
            // pausa durante il trascinamento sopra la testa con la corona grande: niente vittoria
            PinchTo(l, 1f, 3f);
            Assert.IsTrue(l.Grab(l.X, l.Y));
            l.Drag(TopX, TopY);
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Held);
            Assert.AreEqual(TinyCrownDrop.None, l.Release());
            Assert.AreEqual(HomeX, l.X, 1e-5f, "annullata sopra la testa: torna sul cuscino");
            Assert.AreEqual(HomeY, l.Y, 1e-5f);
            Assert.AreEqual(TinyCrownLogic.MaxScale, l.Scale, 1e-5f, "e tiene la sua misura");
            Assert.AreEqual(0, l.WrongMoves);
            AssertStillPlaying(l);
            // pausa durante il pizzico
            Assert.IsTrue(l.StartPinch(1f));
            l.Pinch(0.5f);
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.Pinching);
            Assert.AreEqual(TinyCrownDrop.None, l.EndPinch(true, 0f, 0f));
            Assert.IsFalse(l.Cancel(), "niente da annullare");
            AssertStillPlaying(l);
            // dopo la pausa tutto funziona ancora
            Assert.IsTrue(l.Grab(l.X, l.Y));
            Assert.IsTrue(l.Drag(l.X, l.Y));
            l.Release();
            PinchTo(l, 1f, 3f);
            Assert.AreEqual(TinyCrownDrop.Crowned, DragTo(l, TopX, TopY));
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = New();
            PinchTo(l, 1f, 3f);
            DragTo(l, 2f, 2f);
            l.TapKing();
            Assert.IsTrue(l.Grab(l.X, l.Y));
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(HomeX, l.X, 1e-5f);
            Assert.AreEqual(TinyCrownLogic.MinScale, l.Scale, 1e-5f);
            Assert.IsFalse(l.Held || l.Pinching);
            Assert.AreEqual(0, l.KingTaps + l.WrongMoves + l.Pinches + l.CrownTaps);
            // anche dopo una vittoria
            PinchTo(l, 1f, 3f);
            Assert.AreEqual(TinyCrownDrop.Crowned, DragTo(l, TopX, TopY));
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            PinchTo(l, 1f, 3f);
            Assert.AreEqual(TinyCrownDrop.Crowned, DragTo(l, TopX, TopY));
            Assert.IsTrue(l.TryComplete(), "dopo il reset si può vincere di nuovo (una volta)");
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SmallCrownOffHeadJustStays()
        {
            var l = New();
            Assert.AreEqual(TinyCrownDrop.Placed, DragTo(l, -2f, -3f));
            Assert.AreEqual(0, l.WrongMoves, "spostare la corona piccola altrove non è uno sbaglio");
            Assert.AreEqual(-2f, l.X, 1e-5f);
            AssertStillPlaying(l);
        }
    }
}
