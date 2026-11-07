using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 129 della roadmap: The Fake Trophy (pressione lunga su WIN, poi trascinarla sulla coppa).</summary>
    public class V87FakeTrophyTests
    {
        const float Dt = 1f / 60f;

        static void AssertStillPlaying(FakeTrophyLogic l)
        {
            Assert.AreEqual(FakeTrophyStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TrophyGold);
            Assert.IsFalse(l.WordOnTrophy);
            Assert.IsFalse(l.TryComplete());
        }

        static void AssertWordHome(FakeTrophyLogic l)
        {
            Assert.IsFalse(l.WordHeld);
            Assert.IsFalse(l.Pressing);
            Assert.AreEqual(l.WordHomeX, l.WordX, 1e-5f);
            Assert.AreEqual(l.WordHomeY, l.WordY, 1e-5f);
        }

        /// <summary>Tiene il dito su WIN per "seconds" secondi (a fotogrammi di 1/60) e restituisce l'ultimo risultato.</summary>
        static FakeTrophyResult Hold(FakeTrophyLogic l, float seconds, float moved = 0f)
        {
            FakeTrophyResult r = FakeTrophyResult.None;
            for (float t = 0f; t < seconds - 1e-4f; t += Dt)
            {
                r = l.HoldWord(Dt, moved);
                if (r != FakeTrophyResult.Pressing) return r;
            }
            return r;
        }

        static void Detach(FakeTrophyLogic l)
        {
            Assert.IsTrue(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.Detached, Hold(l, FakeTrophyLogic.HoldSeconds + 0.1f));
            Assert.IsTrue(l.WordHeld);
        }

        /// <summary>Trascina WIN a passi fino a (x, y): restituisce l'ultimo risultato (Snapped se è scattata per strada).</summary>
        static FakeTrophyResult DragWord(FakeTrophyLogic l, float x, float y)
        {
            float x0 = l.WordX, y0 = l.WordY;
            FakeTrophyResult r = FakeTrophyResult.None;
            for (int i = 1; i <= 10; i++)
            {
                r = l.MoveWord(x0 + (x - x0) * i / 10f, y0 + (y - y0) * i / 10f);
                if (r == FakeTrophyResult.Snapped) return r;
            }
            return r;
        }

        static FakeTrophyResult DragTrophy(FakeTrophyLogic l, float x, float y)
        {
            Assert.IsTrue(l.GrabTrophy());
            float x0 = l.TrophyX, y0 = l.TrophyY;
            for (int i = 1; i <= 8; i++) Assert.AreEqual(FakeTrophyResult.TrophyMoved, l.MoveTrophy(x0 + (x - x0) * i / 8f, y0 + (y - y0) * i / 8f));
            return l.ReleaseTrophy();
        }

        static void Solve(FakeTrophyLogic l)
        {
            Detach(l);
            Assert.AreEqual(FakeTrophyResult.Snapped, DragWord(l, l.ZoneX, l.ZoneY));
        }

        [Test]
        public void StartsWithAGreyTrophyAndWinOnTheSign()
        {
            var l = new FakeTrophyLogic();
            AssertStillPlaying(l);
            AssertWordHome(l);
            Assert.IsFalse(l.TrophyHeld);
            Assert.AreEqual(l.TrophyHomeX, l.TrophyX, 1e-5f);
            Assert.AreEqual(l.TrophyHomeY, l.TrophyY, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.IsFalse(l.InZone(l.WordHomeX, l.WordHomeY), "WIN sull'insegna non è già sulla coppa");
            Assert.IsFalse(l.OnGuy(l.TrophyHomeX, l.TrophyHomeY), "la coppa sul piedistallo non è sull'omino");
            Assert.IsTrue(l.InZone(FakeTrophyLogic.DefaultZoneX, FakeTrophyLogic.DefaultZoneY));
        }

        [Test]
        public void TrophyTapsOnlyClink()
        {
            var l = new FakeTrophyLogic();
            for (int i = 0; i < 25; i++) Assert.AreEqual(FakeTrophyResult.Clink, l.TapTrophy());
            Assert.AreEqual(25, l.Clinks);
            AssertStillPlaying(l);
        }

        [Test]
        public void TrophyDraggedOntoTheGuyIsRejectedAndReturns()
        {
            var l = new FakeTrophyLogic();
            float[][] spots = { new[] { l.GuyX, l.GuyY }, new[] { l.GuyX + 0.3f, l.GuyY - 0.6f }, new[] { l.GuyX - 0.4f, l.GuyY + 0.8f } };
            foreach (float[] s in spots)
            {
                Assert.AreEqual(FakeTrophyResult.TrophyRejected, DragTrophy(l, s[0], s[1]));
                Assert.IsFalse(l.TrophyHeld);
                Assert.AreEqual(l.TrophyHomeX, l.TrophyX, 1e-5f, "torna sul piedistallo");
                Assert.AreEqual(l.TrophyHomeY, l.TrophyY, 1e-5f);
                Assert.AreEqual(s[0], l.DropX, 1e-4f);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(3, l.Rejections);
        }

        [Test]
        public void TrophyDroppedElsewhereReturnsAndNeverWins()
        {
            var l = new FakeTrophyLogic();
            float[][] spots = { new[] { 2.2f, 2.5f }, new[] { 0f, -4f }, new[] { -0.2f, 1.75f }, new[] { 9f, -9f } };
            foreach (float[] s in spots)
            {
                Assert.AreEqual(FakeTrophyResult.TrophyReturned, DragTrophy(l, s[0], s[1]));
                Assert.AreEqual(l.TrophyHomeX, l.TrophyX, 1e-5f);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(4, l.TrophyReturns);
            Assert.LessOrEqual(l.DropX, FakeTrophyLogic.ScreenHalf, "la coppa resta nello schermo");
            Assert.GreaterOrEqual(l.DropY, FakeTrophyLogic.MinY);
        }

        [Test]
        public void TrophyBarelyMovedDoesNotCountAsAttempt()
        {
            var l = new FakeTrophyLogic();
            Assert.IsTrue(l.GrabTrophy());
            l.MoveTrophy(l.TrophyHomeX + 0.05f, l.TrophyHomeY);
            Assert.AreEqual(FakeTrophyResult.TrophyReturned, l.ReleaseTrophy());
            Assert.AreEqual(0, l.TrophyReturns);
        }

        [Test]
        public void SignTapsOnlyFlash()
        {
            var l = new FakeTrophyLogic();
            for (int i = 0; i < 12; i++) Assert.AreEqual(FakeTrophyResult.SignFlash, l.TapSign());
            Assert.AreEqual(12, l.SignFlashes);
            AssertWordHome(l);
            AssertStillPlaying(l);
        }

        [Test]
        public void ShortPressesOnWinDoNotDetach()
        {
            var l = new FakeTrophyLogic();
            foreach (float s in new[] { 0.05f, 0.2f, 0.4f, 0.6f, FakeTrophyLogic.HoldSeconds - 0.05f })
            {
                Assert.IsTrue(l.PressWord());
                Assert.AreEqual(FakeTrophyResult.Pressing, Hold(l, s));
                Assert.Greater(l.PressProgress, 0f);
                Assert.Less(l.PressProgress, 1f);
                Assert.AreEqual(FakeTrophyResult.SignFlash, l.ReleasePress());
                Assert.AreEqual(0f, l.PressProgress);
                AssertWordHome(l);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(5, l.ShortPresses);
            Assert.AreEqual(0, l.Detaches);
        }

        [Test]
        public void SlidingFingerCancelsThePress()
        {
            var l = new FakeTrophyLogic();
            Assert.IsTrue(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.Pressing, Hold(l, 0.3f, 0.1f));
            Assert.AreEqual(FakeTrophyResult.SignFlash, l.HoldWord(Dt, FakeTrophyLogic.PressSlop + 0.05f));
            AssertWordHome(l);
            Assert.AreEqual(FakeTrophyResult.None, l.HoldWord(1f, 0f), "annullata: niente distacco più tardi");
            Assert.IsFalse(l.WordHeld);
            AssertStillPlaying(l);
            // un piccolo tremolio del dito invece non annulla
            Assert.IsTrue(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.Detached, Hold(l, 1f, FakeTrophyLogic.PressSlop - 0.05f));
        }

        [Test]
        public void OneHugeFrameDoesNotDetachAlone()
        {
            var l = new FakeTrophyLogic();
            Assert.IsTrue(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.Pressing, l.HoldWord(5f, 0f), "un fotogramma enorme (ritorno dal background) non basta");
            Assert.AreEqual(FakeTrophyResult.Pressing, l.HoldWord(float.NaN, float.NaN));
            Assert.AreEqual(FakeTrophyResult.Pressing, l.HoldWord(-1f, 0f));
        }

        [Test]
        public void LongPressDetachesWin()
        {
            var l = new FakeTrophyLogic();
            Assert.IsTrue(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.Pressing, Hold(l, 0.5f));
            Assert.AreEqual(FakeTrophyResult.Detached, Hold(l, 0.3f));
            Assert.IsTrue(l.WordHeld);
            Assert.IsFalse(l.Pressing);
            Assert.AreEqual(1, l.Detaches);
            Assert.AreEqual(l.WordHomeX, l.WordX, 1e-5f, "si stacca dove era");
            AssertStillPlaying(l);
        }

        [Test]
        public void WinDroppedElsewhereGoesBackToTheSign()
        {
            var l = new FakeTrophyLogic();
            float[][] spots = { new[] { -2f, 0f }, new[] { l.GuyX, l.GuyY }, new[] { -2.2f, -3.8f }, new[] { -0.3f, l.ZoneY } };
            foreach (float[] s in spots)
            {
                Detach(l);
                Assert.AreEqual(FakeTrophyResult.Moved, DragWord(l, s[0], s[1]));
                Assert.AreEqual(FakeTrophyResult.ReturnedToSign, l.ReleaseWord());
                AssertWordHome(l);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(4, l.Returns);
            // dopo il ritorno ci vuole un'altra pressione lunga: un tocco breve non la stacca
            Assert.IsTrue(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.SignFlash, l.ReleasePress());
            Assert.IsFalse(l.WordHeld);
        }

        [Test]
        public void WinStaysOnScreenWhileDragged()
        {
            var l = new FakeTrophyLogic();
            Detach(l);
            l.MoveWord(-50f, 50f);
            Assert.AreEqual(-FakeTrophyLogic.ScreenHalf, l.WordX, 1e-5f);
            Assert.AreEqual(FakeTrophyLogic.MaxY, l.WordY, 1e-5f);
            l.MoveWord(50f, -50f);
            Assert.AreEqual(FakeTrophyLogic.ScreenHalf, l.WordX, 1e-5f);
            Assert.AreEqual(FakeTrophyLogic.MinY, l.WordY, 1e-5f);
            Assert.AreEqual(FakeTrophyResult.Moved, l.MoveWord(float.NaN, 0f));
            AssertStillPlaying(l);
        }

        [Test]
        public void WinOnTheTrophySnapsAndWinsOnce()
        {
            var l = new FakeTrophyLogic();
            Solve(l);
            Assert.AreEqual(FakeTrophyStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.WordOnTrophy);
            Assert.IsTrue(l.TrophyGold);
            Assert.IsFalse(l.WordHeld);
            Assert.AreEqual(l.ZoneX, l.WordX, 1e-5f, "scatta al centro della coppa");
            Assert.AreEqual(l.ZoneY, l.WordY, 1e-5f);
            // dopo lo scatto niente più gesti
            Assert.AreEqual(FakeTrophyResult.None, l.ReleaseWord());
            Assert.AreEqual(FakeTrophyResult.None, l.MoveWord(0f, 0f));
            Assert.IsFalse(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.None, l.TapTrophy());
            Assert.AreEqual(FakeTrophyResult.None, l.TapSign());
            Assert.AreEqual(FakeTrophyResult.None, l.TapGuy());
            Assert.IsFalse(l.GrabTrophy());
            l.Cancel();
            Assert.IsTrue(l.TrophyGold, "Cancel non annulla la trasformazione");
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(FakeTrophyStage.Done, l.Stage);
        }

        [Test]
        public void SnapAlsoOnReleaseInsideTheZoneAndAtTheZoneEdges()
        {
            foreach (float[] o in new[] { new[] { 0f, 0f }, new[] { 0.55f, 0f }, new[] { -0.55f, 0.5f }, new[] { 0.3f, -0.5f } })
            {
                var l = new FakeTrophyLogic();
                Detach(l);
                Assert.AreEqual(FakeTrophyResult.Snapped, l.MoveWord(l.ZoneX + o[0], l.ZoneY + o[1]));
                Assert.IsTrue(l.TryComplete());
            }
        }

        [Test]
        public void AtLeastThreeWrongAttemptsThenTheSolution()
        {
            var l = new FakeTrophyLogic();
            Assert.AreEqual(FakeTrophyResult.Clink, l.TapTrophy());
            Assert.AreEqual(FakeTrophyResult.TrophyRejected, DragTrophy(l, l.GuyX, l.GuyY));
            Assert.AreEqual(FakeTrophyResult.SignFlash, l.TapSign());
            Assert.IsTrue(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.SignFlash, l.ReleasePress());
            Detach(l);
            DragWord(l, -2f, -3f);
            Assert.AreEqual(FakeTrophyResult.ReturnedToSign, l.ReleaseWord());
            Assert.AreEqual(FakeTrophyResult.GuySigh, l.TapGuy());
            Assert.GreaterOrEqual(l.WrongMoves, 5);
            AssertStillPlaying(l);
            Solve(l);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void OneGestureAtATime()
        {
            var l = new FakeTrophyLogic();
            Assert.IsTrue(l.PressWord());
            Assert.IsFalse(l.PressWord(), "seconda pressione mentre la prima è in corso");
            Assert.IsFalse(l.GrabTrophy(), "la coppa non si prende mentre si preme WIN");
            Assert.AreEqual(FakeTrophyResult.SignFlash, l.ReleasePress());
            Assert.IsTrue(l.GrabTrophy());
            Assert.IsFalse(l.PressWord(), "WIN non si preme mentre si tiene la coppa");
            Assert.AreEqual(FakeTrophyResult.None, l.TapTrophy());
            Assert.AreEqual(FakeTrophyResult.TrophyReturned, l.ReleaseTrophy());
            Detach(l);
            Assert.IsFalse(l.GrabTrophy(), "la coppa non si prende mentre si tiene WIN");
            Assert.IsFalse(l.PressWord());
            Assert.AreEqual(FakeTrophyResult.None, l.ReleasePress());
            Assert.AreEqual(FakeTrophyResult.None, l.ReleaseTrophy());
            Assert.AreEqual(FakeTrophyResult.Snapped, DragWord(l, l.ZoneX, l.ZoneY));
        }

        [Test]
        public void RapidRepeatedTouchesNeverWinOrBreak()
        {
            var l = new FakeTrophyLogic();
            for (int i = 0; i < 200; i++)
            {
                switch (i % 6)
                {
                    case 0: l.TapTrophy(); break;
                    case 1: l.TapSign(); break;
                    case 2: if (l.PressWord()) { l.HoldWord(Dt, 0f); l.ReleasePress(); } break;
                    case 3: l.TapGuy(); break;
                    case 4: l.ReleaseWord(); l.ReleasePress(); l.ReleaseTrophy(); break;
                    case 5: if (l.GrabTrophy()) l.ReleaseTrophy(); break;
                }
                AssertStillPlaying(l);
                Assert.IsFalse(l.Busy);
            }
            Solve(l);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void CancelDuringEveryGestureLeavesAValidState()
        {
            var l = new FakeTrophyLogic();
            Assert.IsTrue(l.PressWord());
            Hold(l, 0.5f);
            l.Cancel();
            AssertWordHome(l);
            Assert.AreEqual(0f, l.PressProgress);
            Assert.AreEqual(FakeTrophyResult.None, l.HoldWord(1f, 0f), "la pressione annullata non si stacca più");
            AssertStillPlaying(l);

            Detach(l);
            DragWord(l, 0.2f, 0.3f);
            l.Cancel();
            AssertWordHome(l);
            Assert.AreEqual(FakeTrophyResult.None, l.ReleaseWord());
            AssertStillPlaying(l);

            Assert.IsTrue(l.GrabTrophy());
            l.MoveTrophy(l.GuyX, l.GuyY);
            l.Cancel();
            Assert.IsFalse(l.TrophyHeld);
            Assert.AreEqual(l.TrophyHomeX, l.TrophyX, 1e-5f);
            Assert.AreEqual(FakeTrophyResult.None, l.ReleaseTrophy());
            Assert.AreEqual(0, l.Rejections, "un dito perso non conta come tentativo");
            AssertStillPlaying(l);
            Assert.IsFalse(l.Busy);

            Solve(l);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = new FakeTrophyLogic();
            l.TapTrophy();
            DragTrophy(l, l.GuyX, l.GuyY);
            Detach(l);
            DragWord(l, -1f, -1f);
            l.Reset();
            AssertStillPlaying(l);
            AssertWordHome(l);
            Assert.IsFalse(l.TrophyHeld);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Detaches);
            // anche dopo una vittoria
            Solve(l);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            AssertWordHome(l);
            Solve(l);
            Assert.IsTrue(l.TryComplete(), "dopo Reset si vince di nuovo (una volta)");
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void CustomLayoutWorks()
        {
            // posizioni diverse (es. insegna tradotta o disposizione diversa): la logica usa solo quelle date
            var l = new FakeTrophyLogic(-1f, 2f, -0.5f, -2f, -0.5f, -1.2f, 0.4f, 0.4f, 1.5f, -1.5f, 0.5f, 0.8f);
            AssertWordHome(l);
            Assert.AreEqual(-1f, l.WordX, 1e-5f);
            Assert.AreEqual(FakeTrophyResult.TrophyRejected, DragTrophy(l, 1.5f, -1.5f));
            Detach(l);
            Assert.AreEqual(FakeTrophyResult.Moved, DragWord(l, 0.95f, -0.55f), "la zona di base non conta più");
            Assert.AreEqual(FakeTrophyResult.Snapped, DragWord(l, -0.5f, -1.2f));
            Assert.IsTrue(l.TryComplete());
            var tiny = new FakeTrophyLogic(0f, 0f, 0f, 0f, 1f, 1f, -0f, 0f, 0f, 0f, 0f, 0f);
            Assert.Greater(tiny.ZoneHalfW, 0f, "misure nulle diventano un minimo");
        }
    }
}
