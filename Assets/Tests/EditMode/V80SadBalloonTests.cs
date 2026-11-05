using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 27 della roadmap: The Sad Balloon (trascinare via il sasso per sciogliere il nodo).</summary>
    public class V80SadBalloonTests
    {
        static void AssertStillPlaying(SadBalloonLogic l)
        {
            Assert.AreEqual(SadBalloonStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.IsTrue(l.Tied, "il nodo è ancora legato");
            Assert.IsFalse(l.CanFly, "legato, il palloncino non vola");
            Assert.IsFalse(l.TryComplete());
        }

        static SadBalloonDrop PullAndLet(SadBalloonLogic l, float dx, float dy)
        {
            Assert.IsTrue(l.GrabBalloon());
            Assert.IsTrue(l.PullBalloon(dx, dy));
            Assert.LessOrEqual(l.BalloonY, SadBalloonLogic.MaxStretch, "lo spago trattiene il palloncino");
            return l.ReleaseBalloon();
        }

        static SadBalloonStone DragStone(SadBalloonLogic l, float dx, float dy, int steps = 10)
        {
            Assert.IsTrue(l.GrabStone());
            SadBalloonStone last = SadBalloonStone.None;
            for (int i = 1; i <= steps; i++)
            {
                last = l.MoveStone(dx * i / steps, dy * i / steps);
                if (last == SadBalloonStone.Untied) break;
            }
            return last;
        }

        [Test]
        public void StartsTiedAndSad()
        {
            var l = new SadBalloonLogic();
            AssertStillPlaying(l);
            Assert.IsFalse(l.BalloonHeld || l.StoneHeld || l.Pinching);
            Assert.AreEqual(1f, l.PinchScale, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void BalloonDragsNeverWinAndFallBack()
        {
            var l = new SadBalloonLogic();
            Assert.AreEqual(SadBalloonDrop.FellBack, PullAndLet(l, 0f, 3f));
            Assert.AreEqual(SadBalloonDrop.FellBack, PullAndLet(l, 0.5f, 50f));
            Assert.AreEqual(SadBalloonDrop.FellBack, PullAndLet(l, -5f, -5f));
            Assert.AreEqual(SadBalloonDrop.FellBack, PullAndLet(l, 2f, 0.4f));
            Assert.AreEqual(4, l.BalloonPulls);
            Assert.AreEqual(0f, l.BalloonX, 1e-5f);
            Assert.AreEqual(0f, l.BalloonY, 1e-5f);
            Assert.IsFalse(l.BalloonHeld);
            AssertStillPlaying(l);
        }

        [Test]
        public void BalloonNeverRisesBeyondTheStringWhileTied()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.GrabBalloon());
            for (float dy = -10f; dy <= 1000f; dy += 0.37f)
            {
                l.PullBalloon(0f, dy);
                Assert.Less(l.BalloonY, SadBalloonLogic.MaxStretch);
                Assert.GreaterOrEqual(l.BalloonY, -SadBalloonLogic.MaxDip);
            }
            l.PullBalloon(99f, float.MaxValue);
            Assert.LessOrEqual(l.BalloonX, SadBalloonLogic.MaxSway);
            Assert.Less(l.BalloonY, SadBalloonLogic.MaxStretch);
            l.PullBalloon(float.NaN, float.NaN);
            Assert.AreEqual(0f, l.BalloonX, 1e-5f);
            Assert.AreEqual(0f, l.BalloonY, 1e-5f);
            Assert.IsFalse(l.CanFly);
            // la tirata in su è monotona (più tiri, più sale, ma frenata)
            Assert.Less(SadBalloonLogic.Stretch(0.2f), SadBalloonLogic.Stretch(0.6f));
            Assert.Less(SadBalloonLogic.Stretch(0.6f), SadBalloonLogic.Stretch(5f));
        }

        [Test]
        public void ShortPullIsNotAnAttempt()
        {
            var l = new SadBalloonLogic();
            Assert.AreEqual(SadBalloonDrop.Unchanged, PullAndLet(l, 0.05f, 0.05f));
            Assert.AreEqual(0, l.BalloonPulls);
            AssertStillPlaying(l);
        }

        [Test]
        public void TapsMakeItSighAndNeverWin()
        {
            var l = new SadBalloonLogic();
            for (int i = 0; i < 50; i++) Assert.IsTrue(l.TapBalloon());
            for (int i = 0; i < 20; i++) Assert.IsTrue(l.TapStone());
            Assert.AreEqual(50, l.Sighs);
            Assert.AreEqual(20, l.StoneTaps);
            Assert.AreEqual(0, l.WrongMoves);
            AssertStillPlaying(l);
        }

        [Test]
        public void PinchGrowsALittleThenReverts()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.StartPinch());
            Assert.AreEqual(1.25f, l.Pinch(1.5f), 1e-5f);
            Assert.AreEqual(SadBalloonLogic.MaxPinchScale, l.Pinch(10f), 1e-5f);
            Assert.AreEqual(1f, l.Pinch(0.3f), 1e-5f, "stringere non lo rimpicciolisce sotto il normale");
            Assert.AreEqual(SadBalloonDrop.BiggerStuck, l.EndPinch());
            Assert.AreEqual(1f, l.PinchScale, 1e-5f, "si sgonfia");
            Assert.IsFalse(l.Pinching);
            Assert.AreEqual(1, l.Pinches);
            AssertStillPlaying(l);
        }

        [Test]
        public void TinyPinchDoesNotCount()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.StartPinch());
            l.Pinch(1.05f);
            Assert.AreEqual(SadBalloonDrop.Unchanged, l.EndPinch());
            Assert.AreEqual(0, l.Pinches);
            Assert.AreEqual(SadBalloonDrop.None, l.EndPinch(), "doppio rilascio: niente");
            AssertStillPlaying(l);
        }

        [Test]
        public void PinchTakesOverABalloonDragWithoutCountingIt()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.GrabBalloon());
            l.PullBalloon(0f, 0.6f);
            Assert.IsTrue(l.StartPinch());
            Assert.IsFalse(l.BalloonHeld);
            Assert.AreEqual(0f, l.BalloonY, 1e-5f);
            Assert.IsFalse(l.GrabBalloon(), "durante il pizzico non si tira");
            Assert.IsFalse(l.TapBalloon());
            Assert.AreEqual(SadBalloonDrop.None, l.ReleaseBalloon());
            Assert.AreEqual(0, l.BalloonPulls);
            l.Pinch(2f);
            Assert.AreEqual(SadBalloonDrop.BiggerStuck, l.EndPinch());
            AssertStillPlaying(l);
        }

        [Test]
        public void ShortStoneDragReturnsAndStaysTied()
        {
            var l = new SadBalloonLogic();
            Assert.AreEqual(SadBalloonStone.Moving, DragStone(l, 0.6f, 0f));
            Assert.AreEqual(0.6f, l.StoneDistance, 1e-4f);
            Assert.AreEqual(SadBalloonDrop.SlidBack, l.ReleaseStone());
            Assert.AreEqual(0f, l.StoneDistance, 1e-5f);
            Assert.AreEqual(SadBalloonStone.Moving, DragStone(l, -0.5f, 0.5f));
            Assert.AreEqual(SadBalloonDrop.SlidBack, l.ReleaseStone());
            Assert.AreEqual(SadBalloonStone.Moving, DragStone(l, 0f, -0.99f));
            Assert.AreEqual(SadBalloonDrop.SlidBack, l.ReleaseStone());
            Assert.AreEqual(3, l.ShortDrags);
            Assert.AreEqual(SadBalloonDrop.None, l.ReleaseStone(), "doppio rilascio: niente");
            AssertStillPlaying(l);
        }

        [Test]
        public void StoneBackAndForthBelowThresholdNeverUnties()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.GrabStone());
            for (int i = 0; i < 200; i++)
            {
                float a = i * 0.31f;
                Assert.AreEqual(SadBalloonStone.Moving, l.MoveStone(0.95f * (float)System.Math.Cos(a), 0.95f * (float)System.Math.Sin(a)));
            }
            Assert.AreEqual(SadBalloonStone.Moving, l.MoveStone(float.NaN, 0f));
            Assert.AreEqual(SadBalloonDrop.SlidBack, l.ReleaseStone());
            AssertStillPlaying(l);
        }

        [Test]
        public void StonePastThresholdUntiesDuringDragAndWinsOnce()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.GrabStone());
            Assert.AreEqual(SadBalloonStone.Moving, l.MoveStone(0.9f, 0f));
            Assert.IsTrue(l.Tied);
            Assert.AreEqual(SadBalloonStone.Untied, l.MoveStone(1.0f, 0.05f), "si scioglie durante il trascinamento");
            Assert.IsFalse(l.Tied);
            Assert.IsTrue(l.CanFly);
            Assert.AreEqual(SadBalloonStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.IsFalse(l.StoneHeld);
            // dopo: niente altro conta
            Assert.AreEqual(SadBalloonStone.None, l.MoveStone(3f, 0f));
            Assert.AreEqual(SadBalloonDrop.None, l.ReleaseStone());
            Assert.IsFalse(l.GrabStone());
            Assert.IsFalse(l.GrabBalloon());
            Assert.IsFalse(l.StartPinch());
            Assert.IsFalse(l.TapBalloon());
            Assert.IsFalse(l.Cancel());
            Assert.IsFalse(l.Tied, "Cancel non rilega il nodo");
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(SadBalloonStage.Done, l.Stage);
        }

        [Test]
        public void AnyDirectionAwayUnties()
        {
            foreach (var d in new[] { (1.2f, 0f), (-1.2f, 0f), (0f, 1.5f), (0.8f, -0.8f), (-0.75f, 0.75f) })
            {
                var l = new SadBalloonLogic();
                Assert.AreEqual(SadBalloonStone.Untied, DragStone(l, d.Item1, d.Item2), d.ToString());
                Assert.IsTrue(l.TryComplete());
            }
        }

        [Test]
        public void WrongAttemptsThenSolution()
        {
            var l = new SadBalloonLogic();
            PullAndLet(l, 0f, 2f);
            l.StartPinch(); l.Pinch(1.8f); l.EndPinch();
            DragStone(l, 0.4f, 0f); l.ReleaseStone();
            l.TapBalloon();
            Assert.AreEqual(3, l.WrongMoves);
            AssertStillPlaying(l);
            Assert.AreEqual(SadBalloonStone.Untied, DragStone(l, 1.5f, 0f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void UntieWhileOtherFingerHoldsTheBalloon()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.GrabBalloon());
            l.PullBalloon(0f, 1f);
            Assert.AreEqual(SadBalloonStone.Untied, DragStone(l, 0f, -1.1f));
            Assert.IsFalse(l.BalloonHeld, "la presa del palloncino si scioglie con la vittoria");
            Assert.AreEqual(SadBalloonDrop.None, l.ReleaseBalloon());
            Assert.AreEqual(0, l.BalloonPulls);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void UntieWhilePinchingEndsThePinch()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.StartPinch());
            l.Pinch(2f);
            Assert.AreEqual(SadBalloonStone.Untied, DragStone(l, 1.1f, 0f));
            Assert.IsFalse(l.Pinching);
            Assert.AreEqual(1f, l.PinchScale, 1e-5f);
            Assert.AreEqual(SadBalloonDrop.None, l.EndPinch());
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void CancelLetsGoOfEverythingAndKeepsTheKnot()
        {
            var l = new SadBalloonLogic();
            Assert.IsTrue(l.GrabBalloon());
            l.PullBalloon(0.3f, 0.8f);
            Assert.IsTrue(l.GrabStone());
            l.MoveStone(0.9f, 0f);
            Assert.IsTrue(l.Cancel());
            Assert.IsFalse(l.BalloonHeld || l.StoneHeld || l.Pinching);
            Assert.AreEqual(0f, l.StoneDistance, 1e-5f);
            Assert.AreEqual(0f, l.BalloonY, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves, "un dito perso non è un tentativo");
            AssertStillPlaying(l);
            Assert.IsFalse(l.Cancel(), "niente da annullare");
            Assert.IsTrue(l.StartPinch());
            l.Pinch(3f);
            Assert.IsTrue(l.Cancel());
            Assert.AreEqual(1f, l.PinchScale, 1e-5f);
            Assert.AreEqual(SadBalloonDrop.None, l.EndPinch());
            // dopo l'annullamento tutto funziona come prima
            Assert.AreEqual(SadBalloonStone.Untied, DragStone(l, 1.2f, 0f));
        }

        [Test]
        public void RapidRepeatedTouchesStayValid()
        {
            var l = new SadBalloonLogic();
            for (int i = 0; i < 300; i++)
            {
                switch (i % 6)
                {
                    case 0: l.TapBalloon(); break;
                    case 1: l.GrabBalloon(); l.GrabBalloon(); l.PullBalloon(0f, i * 0.01f); break;
                    case 2: l.ReleaseBalloon(); l.ReleaseBalloon(); break;
                    case 3: l.GrabStone(); l.GrabStone(); l.MoveStone(0.3f, 0.2f); break;
                    case 4: l.ReleaseStone(); l.TapStone(); break;
                    case 5: l.StartPinch(); l.Pinch(1.4f); l.EndPinch(); l.EndPinch(); break;
                }
                Assert.IsTrue(l.Tied);
                Assert.AreEqual(SadBalloonStage.Playing, l.Stage);
            }
            AssertStillPlaying(l);
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = new SadBalloonLogic();
            PullAndLet(l, 0f, 1f);
            l.StartPinch(); l.Pinch(2f);
            DragStone(l, 2f, 0f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertStillPlaying(l);
            Assert.IsFalse(l.BalloonHeld || l.StoneHeld || l.Pinching);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Sighs);
            Assert.AreEqual(1f, l.PinchScale, 1e-5f);
            // e si può rivincere (una volta)
            Assert.AreEqual(SadBalloonStone.Untied, DragStone(l, 0f, 1.3f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ResetMidDragAndMidPinch()
        {
            var l = new SadBalloonLogic();
            l.GrabStone(); l.MoveStone(0.99f, 0f);
            l.StartPinch(); l.Pinch(2f);
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(SadBalloonStone.None, l.MoveStone(5f, 0f), "il sasso non è più tenuto");
            Assert.IsTrue(l.Tied);
        }
    }
}
