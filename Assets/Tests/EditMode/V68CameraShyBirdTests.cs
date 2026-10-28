using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 264: l'uccellino che odia le foto (le false piste non cambiano niente, il selfie col fotobomba vince).</summary>
    public class V68CameraShyBirdTests
    {
        static ShyEvent Run(CameraShyBirdLogic l, float seconds, ShyEvent stopAt = ShyEvent.None, float dt = 0.05f)
        {
            for (float t = 0f; t < seconds; t += dt)
            {
                ShyEvent e = l.Tick(dt);
                if (stopAt != ShyEvent.None && e == stopAt) return e;
            }
            return ShyEvent.None;
        }

        static void AimAtBird(CameraShyBirdLogic l) => l.PanBy(l.BirdX - l.Pan);

        static CameraShyBirdLogic SelfieWithBirdPosing()
        {
            var l = new CameraShyBirdLogic();
            Assert.IsTrue(l.Flip());
            Assert.AreEqual(ShyEvent.SelfieLand, Run(l, 3f, ShyEvent.SelfieLand));
            Assert.AreEqual(SelfieBird.Posing, l.Selfie);
            return l;
        }

        [Test]
        public void StartsLookingAtTheBench_BirdPeeksFromTheEdge()
        {
            var l = new CameraShyBirdLogic();
            Assert.AreEqual(ShyStage.Hunting, l.Stage);
            Assert.AreEqual(ShyFacing.Rear, l.Facing);
            Assert.AreEqual(BirdMood.Peeking, l.Mood);
            Assert.AreEqual(1f, l.Zoom);
            Assert.IsFalse(l.FlashOn);
            Assert.IsTrue(l.BirdInFrame);          // si vede sul bordo...
            Assert.IsFalse(l.Aimed);               // ...ma non è puntato
            Assert.Greater(System.Math.Abs(l.BirdFrame), l.AimRadius);
            Assert.AreEqual(ShyEvent.None, Run(l, 2f, ShyEvent.Noticed));
            Assert.AreEqual(BirdMood.Peeking, l.Mood);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void PanningOntoTheBird_ItStaresThenFliesAwayAndHides()
        {
            var l = new CameraShyBirdLogic();
            AimAtBird(l);
            Assert.IsTrue(l.Aimed);
            Assert.AreEqual(ShyEvent.Noticed, l.Tick(0.02f));
            Assert.AreEqual(BirdMood.Staring, l.Mood);
            Assert.AreEqual(ShyEvent.Fled, Run(l, 1f, ShyEvent.Fled));
            Assert.AreEqual(ShyEvent.Hid, Run(l, 2f, ShyEvent.Hid));
            Assert.AreEqual(BirdMood.Hidden, l.Mood);
            Assert.GreaterOrEqual(System.Math.Abs(l.BirdFrame), l.OutEdge);   // fuori campo, tra le foglie
            Assert.IsFalse(l.BirdInFrame);
            Assert.AreEqual(ShyEvent.Reappeared, Run(l, 2f, ShyEvent.Reappeared));
            Assert.AreEqual(1, l.Notices);
            Assert.AreEqual(ShyStage.Hunting, l.Stage);
        }

        [Test]
        public void SneakingUpSlowly_StillNoticed()
        {
            var l = new CameraShyBirdLogic();
            bool noticed = false;
            for (int i = 0; i < 400 && !noticed; i++)
            {
                l.PanBy(0.004f);                    // pochissimo alla volta
                noticed = l.Tick(0.02f) == ShyEvent.Noticed;
            }
            Assert.IsTrue(noticed);
            Assert.AreEqual(BirdMood.Staring, l.Mood);
        }

        [Test]
        public void PointingAway_BirdFollowsTheCameraToTheEdge()
        {
            var l = new CameraShyBirdLogic();
            Assert.IsTrue(l.PanBy(-3f));            // verso il sole, lontano dall'uccellino
            Assert.Greater(System.Math.Abs(l.BirdFrame), l.OutEdge);
            Assert.AreEqual(ShyEvent.Followed, Run(l, 2f, ShyEvent.Followed));
            Assert.AreEqual(ShyEvent.Landed, Run(l, 1f, ShyEvent.Landed));
            Assert.AreEqual(BirdMood.Peeking, l.Mood);
            Assert.IsTrue(l.BirdInFrame);           // sbircia dal bordo
            Assert.IsFalse(l.Aimed);
            Assert.AreEqual(l.PeekEdge, System.Math.Abs(l.BirdFrame), 0.02f);
            Assert.AreEqual(1, l.Follows);
            Assert.AreEqual(0, l.Notices);
        }

        [Test]
        public void ZoomOnIt_CoversFace_ThenUncovers()
        {
            var l = new CameraShyBirdLogic();
            // si ingrandisce e poi si va verso l'uccellino: appena entra in campo si copre
            Assert.IsTrue(l.SetZoom(2f));
            Assert.AreEqual(ShyEvent.None, l.Tick(0.02f));   // a 2x il bordo è fuori campo
            l.PanBy(1.0f);
            Assert.IsTrue(l.BirdInFrame);
            Assert.IsFalse(l.Aimed);
            Assert.AreEqual(ShyEvent.CoveredFace, l.Tick(0.02f));
            Assert.AreEqual(BirdMood.Covering, l.Mood);
            Assert.AreEqual(ShyPhoto.Wing, l.Shutter());
            Assert.AreEqual(BirdMood.Covering, l.Mood);
            // isteresi: a 1.6 resta coperto, sotto 1.5 abbassa l'ala
            l.SetZoom(1.6f);
            Assert.AreEqual(ShyEvent.None, l.Tick(0.02f));
            l.SetZoom(1f);
            Assert.AreEqual(ShyEvent.Uncovered, l.Tick(0.02f));
            Assert.AreEqual(BirdMood.Peeking, l.Mood);
            Assert.AreEqual(1, l.Covers);
        }

        [Test]
        public void CoveredTooLong_FliesAway()
        {
            var l = new CameraShyBirdLogic();
            l.SetZoom(2.5f);
            l.PanBy(1.4f);
            Assert.AreEqual(ShyEvent.CoveredFace, l.Tick(0.02f));
            Assert.AreEqual(ShyEvent.Fled, Run(l, 4f, ShyEvent.Fled));
        }

        [Test]
        public void ZoomWhileItStares_CoversFace()
        {
            var l = new CameraShyBirdLogic();
            AimAtBird(l);
            Assert.AreEqual(ShyEvent.Noticed, l.Tick(0.02f));
            l.SetZoom(2f);
            Assert.AreEqual(ShyEvent.CoveredFace, l.Tick(0.02f));
            Assert.AreEqual(BirdMood.Covering, l.Mood);
        }

        [Test]
        public void ZoomButtons_StepAndClamp_AndSelfieIsAlways1x()
        {
            var l = new CameraShyBirdLogic();
            Assert.IsTrue(l.ZoomStep(+1));
            Assert.AreEqual(1.5f, l.Zoom, 1e-5);
            for (int i = 0; i < 10; i++) l.ZoomStep(+1);
            Assert.AreEqual(l.ZoomMax, l.Zoom, 1e-5);
            for (int i = 0; i < 10; i++) l.ZoomStep(-1);
            Assert.AreEqual(l.ZoomMin, l.Zoom, 1e-5);
            Assert.IsFalse(l.ZoomStep(-1));
            l.SetZoom(2.5f);
            l.Flip();
            Assert.AreEqual(1f, l.Zoom);
            Assert.IsFalse(l.ZoomStep(+1));
            Assert.IsFalse(l.SetZoom(2f));
            Assert.IsFalse(l.PanBy(1f));
            Assert.AreEqual(1f, l.Zoom);
        }

        [Test]
        public void PanIsClamped_AndNaNIgnored()
        {
            var l = new CameraShyBirdLogic();
            l.PanBy(100f);
            Assert.AreEqual(l.PanMax, l.Pan, 1e-5);
            l.PanBy(-100f);
            Assert.AreEqual(-l.PanMax, l.Pan, 1e-5);
            Assert.IsFalse(l.PanBy(float.NaN));
            Assert.IsFalse(l.SetZoom(float.NaN));
            Assert.AreEqual(ShyEvent.None, l.Tick(float.NaN));
            Assert.AreEqual(ShyEvent.None, l.Tick(0f));
            Assert.AreEqual(-l.PanMax, l.Pan, 1e-5);
        }

        [Test]
        public void FlashGivesGlare_BirdKeepsItsShadesAndPlace()
        {
            var l = new CameraShyBirdLogic();
            Assert.IsTrue(l.ToggleFlash());
            Assert.AreEqual(ShyPhoto.Glare, l.Shutter());
            Assert.AreEqual(BirdMood.Peeking, l.Mood);
            Assert.AreEqual(ShyStage.Hunting, l.Stage);
            Assert.IsFalse(l.ToggleFlash());
            Assert.AreEqual(2, l.FlashToggles);
            Assert.AreEqual(1, l.Glares);
        }

        [Test]
        public void ShutterWithBirdInFrame_ItDodges()
        {
            var l = new CameraShyBirdLogic();
            Assert.IsTrue(l.BirdInFrame);
            Assert.AreEqual(ShyPhoto.Dodged, l.Shutter());
            Assert.AreEqual(BirdMood.Fleeing, l.Mood);
            // scattare di nuovo mentre vola in campo: ancora mossa, niente di rotto
            Assert.AreEqual(ShyPhoto.Dodged, l.Shutter());
            Assert.AreEqual(ShyEvent.Hid, Run(l, 2f, ShyEvent.Hid));
        }

        [Test]
        public void ShutterOnEmptyScenery_LovelyBranch()
        {
            var l = new CameraShyBirdLogic();
            AimAtBird(l);
            Run(l, 2f, ShyEvent.Hid);               // è scappato: il ramo è vuoto
            Assert.IsFalse(l.BirdInFrame);
            Assert.AreEqual(ShyPhoto.Scenery, l.Shutter());
            Assert.AreEqual(1, l.Sceneries);
            Assert.AreEqual(ShyStage.Hunting, l.Stage);
        }

        [Test]
        public void FocusTapOnTheBird_ItNotices()
        {
            var l = new CameraShyBirdLogic();
            Assert.IsTrue(l.FocusOnBird());
            Assert.AreEqual(BirdMood.Staring, l.Mood);
            Assert.IsFalse(l.FocusOnBird());        // già se n'è accorto
        }

        [Test]
        public void EveryWrongMove_KeepsStateAndProgress_ThenSelfieStillWins()
        {
            var l = new CameraShyBirdLogic();
            l.ToggleFlash();
            l.Shutter();
            l.ToggleFlash();
            l.Shutter();                            // dodged
            Run(l, 3f);
            AimAtBird(l);
            Run(l, 3f);
            l.SetZoom(3f);
            l.Shutter();
            l.SetZoom(1f);
            l.PanBy(-2f);
            Run(l, 3f);
            l.FocusOnBird();
            l.Shutter();
            Run(l, 3f);
            Assert.AreEqual(ShyStage.Hunting, l.Stage);
            Assert.Greater(l.WrongMoves, 2);
            Assert.IsFalse(l.TryComplete());
            l.Flip();
            Assert.AreEqual(ShyEvent.SelfieLand, Run(l, 3f, ShyEvent.SelfieLand));
            Assert.AreEqual(ShyPhoto.Photobomb, l.Shutter());
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void Selfie_BirdArrivesPosesLeavesAndComesBack()
        {
            var l = new CameraShyBirdLogic();
            l.Flip();
            Assert.AreEqual(ShyFacing.Selfie, l.Facing);
            Assert.AreEqual(SelfieBird.Away, l.Selfie);
            Assert.IsFalse(l.BirdInSelfie);
            Assert.AreEqual(ShyEvent.SelfieArrive, Run(l, 2f, ShyEvent.SelfieArrive));
            Assert.AreEqual(ShyEvent.SelfieLand, Run(l, 2f, ShyEvent.SelfieLand));
            Assert.IsTrue(l.BirdInSelfie);
            Assert.AreEqual(ShyEvent.SelfieLeave, Run(l, 5f, ShyEvent.SelfieLeave));
            Assert.AreEqual(ShyEvent.SelfieGone, Run(l, 2f, ShyEvent.SelfieGone));
            Assert.IsFalse(l.BirdInSelfie);
            Assert.AreEqual(ShyEvent.SelfieArrive, Run(l, 3f, ShyEvent.SelfieArrive));
            Assert.AreEqual(2, l.Arrivals);
        }

        [Test]
        public void Selfie_PosesForAFewSecondsAtLeast()
        {
            var l = SelfieWithBirdPosing();
            Run(l, l.PoseSeconds - 0.3f);
            Assert.AreEqual(SelfieBird.Posing, l.Selfie);
            Assert.GreaterOrEqual(l.PoseSeconds, 3f);
        }

        [Test]
        public void SelfieShutterTiming_Tolerance()
        {
            // troppo presto: non è ancora arrivato
            var l = new CameraShyBirdLogic();
            l.Flip();
            Assert.AreEqual(ShyPhoto.SelfieAlone, l.Shutter());
            Assert.AreEqual(ShyStage.Hunting, l.Stage);
            // a metà dell'arrivo vale già
            Run(l, 2f, ShyEvent.SelfieArrive);
            l.Tick(l.ArriveSeconds * 0.3f);
            Assert.IsFalse(l.BirdInSelfie);
            l.Tick(l.ArriveSeconds * 0.3f);
            Assert.IsTrue(l.BirdInSelfie);
            Assert.AreEqual(ShyPhoto.Photobomb, l.Shutter());

            // appena ripartito vale ancora; un po' dopo no; al giro successivo sì
            var m = SelfieWithBirdPosing();
            Run(m, 6f, ShyEvent.SelfieLeave);
            m.Tick(0.2f);
            Assert.IsTrue(m.BirdInSelfie);
            m.Tick(0.3f);
            Assert.IsFalse(m.BirdInSelfie);
            Assert.AreEqual(ShyPhoto.SelfieAlone, m.Shutter());
            Assert.AreEqual(ShyEvent.SelfieLand, Run(m, 5f, ShyEvent.SelfieLand));
            Assert.AreEqual(ShyPhoto.Photobomb, m.Shutter());
        }

        [Test]
        public void FlashInSelfie_DoesNotSpoilThePhotobomb()
        {
            var l = SelfieWithBirdPosing();
            Assert.IsTrue(l.ToggleFlash());
            Assert.AreEqual(ShyPhoto.Photobomb, l.Shutter());
        }

        [Test]
        public void FlippingBackAndForth_IsAlwaysValid()
        {
            var l = new CameraShyBirdLogic();
            float pan0 = l.Pan;
            for (int i = 0; i < 7; i++)
            {
                Assert.IsTrue(l.Flip());
                Run(l, 0.4f);
            }
            Assert.AreEqual(ShyFacing.Selfie, l.Facing);   // dispari: selfie
            Assert.AreEqual(pan0, l.Pan, 1e-5);
            Assert.AreEqual(ShyEvent.SelfieLand, Run(l, 3f, ShyEvent.SelfieLand));
            // indietro mentre posa: torna timido, nascosto fuori campo, poi sbircia di nuovo
            l.Flip();
            Assert.AreEqual(ShyFacing.Rear, l.Facing);
            Assert.AreEqual(BirdMood.Hidden, l.Mood);
            Assert.IsFalse(l.BirdInFrame);
            Assert.GreaterOrEqual(System.Math.Abs(l.BirdFrame), l.OutEdge);
            Assert.AreEqual(ShyEvent.Reappeared, Run(l, 2f, ShyEvent.Reappeared));
            // e ancora selfie: il conto riparte, l'uccellino torna
            l.Flip();
            Assert.AreEqual(SelfieBird.Away, l.Selfie);
            Assert.AreEqual(ShyEvent.SelfieLand, Run(l, 3f, ShyEvent.SelfieLand));
            Assert.AreEqual(ShyPhoto.Photobomb, l.Shutter());
            Assert.AreEqual(8 + 1, l.Flips);
        }

        [Test]
        public void FlipAtTheEndOfThePark_BirdStillHidesOffFrame()
        {
            var l = new CameraShyBirdLogic();
            l.PanBy(100f);
            l.Flip();
            l.Flip();
            Assert.AreEqual(BirdMood.Hidden, l.Mood);
            Assert.GreaterOrEqual(System.Math.Abs(l.BirdFrame), l.OutEdge);
            Assert.LessOrEqual(l.BirdX, l.PerchMax);
            Assert.GreaterOrEqual(l.BirdX, l.PerchMin);
        }

        [Test]
        public void WinsOnlyOnce_AndLocksInput()
        {
            var l = SelfieWithBirdPosing();
            Assert.AreEqual(ShyPhoto.Photobomb, l.Shutter());
            Assert.AreEqual(ShyStage.Snapped, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(ShyPhoto.None, l.Shutter());
            Assert.IsFalse(l.Flip());
            Assert.IsFalse(l.FocusOnBird());
            bool flash = l.FlashOn;
            Assert.AreEqual(flash, l.ToggleFlash());
            Assert.AreEqual(ShyEvent.None, l.Tick(0.1f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(ShyStage.Done, l.Stage);
        }

        [Test]
        public void LongFrame_DoesNotSkipStates()
        {
            var l = new CameraShyBirdLogic();
            AimAtBird(l);
            Assert.AreEqual(ShyEvent.Noticed, l.Tick(10f));
            Assert.AreEqual(ShyEvent.None, l.Tick(0.2f));   // un fotogramma enorme vale al massimo 0.25 s
            Assert.AreEqual(BirdMood.Staring, l.Mood);
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = SelfieWithBirdPosing();
            l.ToggleFlash();
            l.Shutter();
            l.TryComplete();
            l.Reset();
            Assert.AreEqual(ShyStage.Hunting, l.Stage);
            Assert.AreEqual(ShyFacing.Rear, l.Facing);
            Assert.AreEqual(BirdMood.Peeking, l.Mood);
            Assert.AreEqual(SelfieBird.Away, l.Selfie);
            Assert.AreEqual(CameraShyBirdLogic.StartPan, l.Pan);
            Assert.AreEqual(CameraShyBirdLogic.StartBirdX, l.BirdX);
            Assert.AreEqual(1f, l.Zoom);
            Assert.IsFalse(l.FlashOn);
            Assert.AreEqual(0, l.Photos + l.Flips + l.FlashToggles + l.WrongMoves + l.Arrivals);
            Assert.IsFalse(l.CompletionReported);
            Assert.IsFalse(l.TryComplete());
            // e si rigioca da capo
            l.Flip();
            Run(l, 3f, ShyEvent.SelfieLand);
            Assert.AreEqual(ShyPhoto.Photobomb, l.Shutter());
            Assert.IsTrue(l.TryComplete());
        }
    }
}
