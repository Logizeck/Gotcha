using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 266: il quadro storto (è la stanza a essere storta: si vince inclinando il telefono, rispetto
    /// all'angolo di partenza, con tolleranza, isteresi e niente vittorie a telefono piatto).</summary>
    public class V69CrookedPictureTests
    {
        const float Dt = 0.02f;

        /// <summary>Tiene il telefono a un rollio (assoluto) e una pendenza per un po'; si ferma al primo evento cercato.</summary>
        static CrookedEvent Hold(CrookedPictureLogic l, float roll, float seconds, float pitch = 20f, CrookedEvent stopAt = CrookedEvent.None,
            float jitter = 0f, int seed = 1)
        {
            var rnd = new System.Random(seed);
            for (float t = 0f; t < seconds; t += Dt)
            {
                float r = roll + (jitter > 0f ? (float)(rnd.NextDouble() * 2.0 - 1.0) * jitter : 0f);
                CrookedPictureLogic.GravityFor(r, pitch, out float gx, out float gy);
                if (jitter > 0f) { gx += (float)(rnd.NextDouble() * 2.0 - 1.0) * 0.04f; gy += (float)(rnd.NextDouble() * 2.0 - 1.0) * 0.04f; }
                CrookedEvent e = l.Feed(gx, gy, Dt);
                if (stopAt != CrookedEvent.None && e == stopAt) return e;
            }
            return CrookedEvent.None;
        }

        static CrookedPictureLogic Calibrated(float startRoll = 0f, float pitch = 20f)
        {
            var l = new CrookedPictureLogic();
            Assert.AreEqual(CrookedEvent.Calibrated, Hold(l, startRoll, 1f, pitch, CrookedEvent.Calibrated));
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
            return l;
        }

        [Test]
        public void RollComputation_UprightIsZero_ClockwiseIsPositive()
        {
            var l = new CrookedPictureLogic();
            Hold(l, 0f, 0.6f);
            Assert.AreEqual(0f, l.Roll, 0.01f);
            l = new CrookedPictureLogic();
            Hold(l, 15f, 0.6f);
            Assert.AreEqual(15f, l.Roll, 0.05f);
            l = new CrookedPictureLogic();
            Hold(l, -30f, 0.6f, 50f);   // anche telefono inclinato all'indietro: il rollio non cambia
            Assert.AreEqual(-30f, l.Roll, 0.05f);
        }

        [Test]
        public void StartsCalibrating_ThenPlaying_PictureLooksCrooked_NoWin()
        {
            var l = new CrookedPictureLogic();
            Assert.AreEqual(CrookedStage.Calibrating, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.AreEqual(CrookedEvent.Calibrated, Hold(l, 0f, 1f, 20f, CrookedEvent.Calibrated));
            Assert.AreEqual(CrookedEvent.None, Hold(l, 0f, 5f, 20f, CrookedEvent.Straightened));
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
            Assert.AreEqual(-1f, l.Bubble, 0.01f);                         // bolla tutta verso il lato alto (sinistra)
            Assert.AreEqual(-CrookedPictureLogic.RoomAngle, l.WaterTilt, 0.01f);   // acqua orizzontale nello schermo
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void TiltingByTheRoomAngle_HeldOneSecond_Wins()
        {
            var l = Calibrated();
            Assert.AreEqual(CrookedEvent.Aligning, Hold(l, CrookedPictureLogic.RoomAngle, 0.6f, 20f, CrookedEvent.Aligning));
            Assert.IsTrue(l.Aligned);
            Hold(l, CrookedPictureLogic.RoomAngle, 0.5f);
            Assert.IsTrue(l.Aligned);
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
            Assert.AreEqual(0f, l.Bubble, 0.15f);                          // la bolla va verso il centro
            Assert.AreEqual(0f, l.WaterTilt, 2f);                          // l'acqua si allinea al bicchiere
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle, 2f, 20f, CrookedEvent.Straightened));
            Assert.AreEqual(CrookedStage.Straightened, l.Stage);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(0f, l.Bubble);
        }

        [Test]
        public void GenerousTolerance_ThreeDegreesOffStillWins()
        {
            var l = Calibrated();
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle + 3f, 3f, 20f, CrookedEvent.Straightened));
            l = Calibrated();
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle - 3f, 3f, 20f, CrookedEvent.Straightened));
        }

        [Test]
        public void TooFarOrTooLittle_NoWin()
        {
            var l = Calibrated();
            Assert.AreEqual(CrookedEvent.None, Hold(l, CrookedPictureLogic.RoomAngle * 0.5f, 4f, 20f, CrookedEvent.Straightened));
            Assert.AreEqual(CrookedEvent.None, Hold(l, CrookedPictureLogic.RoomAngle * 2.2f, 4f, 20f, CrookedEvent.Straightened));
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
        }

        [Test]
        public void JitterAroundTheTarget_HysteresisKeepsCounting()
        {
            // rumore forte dell'accelerometro (±5 gradi, più rumore sui due assi) attorno a un angolo vicino al bordo
            var l = Calibrated();
            Assert.AreEqual(CrookedEvent.Straightened,
                Hold(l, CrookedPictureLogic.RoomAngle + 3.5f, 4f, 25f, CrookedEvent.Straightened, jitter: 5f, seed: 7));
        }

        [Test]
        public void Hysteresis_EnterAtFour_LeaveOnlyBeyondExitTolerance()
        {
            var l = Calibrated();
            float target = CrookedPictureLogic.RoomAngle;
            Hold(l, target + 5.5f, 1.5f);                 // tra EnterTol ed ExitTol, arrivando da fuori: non entra
            Assert.IsFalse(l.Aligned);
            Assert.AreEqual(0f, l.Hold, 0.001f);
            Hold(l, target + 2f, 0.5f);                   // dentro
            Assert.IsTrue(l.Aligned);
            Hold(l, target + 5.5f, 0.3f);                 // esce un po' ma entro ExitTol: resta allineato
            Assert.IsTrue(l.Aligned);
            Assert.AreEqual(CrookedEvent.LostAlign, Hold(l, target + 9f, 1f, 20f, CrookedEvent.LostAlign));
            Assert.IsFalse(l.Aligned);
        }

        [Test]
        public void LeavingEarly_ProgressDecaysSlowly_NotLostAtOnce()
        {
            var l = Calibrated();
            float target = CrookedPictureLogic.RoomAngle;
            Hold(l, target, 0.75f);
            Assert.IsTrue(l.Aligned);
            float before = l.Hold;
            Assert.Greater(before, 0.3f);
            Hold(l, 0f, 0.08f);                            // scatto brevissimo fuori (il filtro lo attenua)
            Assert.Greater(l.Hold, 0f);
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, target, 2f, 20f, CrookedEvent.Straightened));
        }

        [Test]
        public void WorksFromASlantedStart_RelativeToStartingAngle()
        {
            // il giocatore tiene il telefono già girato di 10 gradi in senso orario: conta il rollio relativo
            var l = Calibrated(10f);
            Assert.AreEqual(10f, l.Baseline, 0.1f);
            Assert.AreEqual(CrookedEvent.None, Hold(l, 10f, 3f, 20f, CrookedEvent.Straightened));   // fermo com'era: niente
            Assert.AreEqual(CrookedEvent.None, Hold(l, CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));   // -22 relativi: troppo
            Assert.IsFalse(l.InputLocked);
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, 10f + CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));

            // e dall'altra parte, con il telefono molto inclinato all'indietro
            l = Calibrated(-8f, 55f);
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, -8f + CrookedPictureLogic.RoomAngle, 3f, 55f, CrookedEvent.Straightened));
        }

        [Test]
        public void StartingAlreadyTilted_DoesNotWinByItself()
        {
            var l = new CrookedPictureLogic();
            Assert.AreEqual(CrookedEvent.None, Hold(l, CrookedPictureLogic.RoomAngle, 5f, 20f, CrookedEvent.Straightened));
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
            Assert.AreEqual(0f, l.Relative, 0.1f);
        }

        [Test]
        public void FlatPhone_RollUnreliable_Ignored_NoFalseWin()
        {
            // piatto sul tavolo (gravità quasi tutta su z): anche se x/y "puntano" all'angolo giusto non conta
            var l = Calibrated();
            Assert.AreEqual(CrookedEvent.Flat, Hold(l, CrookedPictureLogic.RoomAngle, 1f, 85f, CrookedEvent.Flat));
            Assert.IsFalse(l.Reliable);
            Assert.AreEqual(CrookedEvent.None, Hold(l, CrookedPictureLogic.RoomAngle, 1f, 85f, CrookedEvent.Straightened));
            Assert.IsFalse(l.Aligned);
            Assert.AreEqual(0f, l.Hold);
            // anche rumore puro a bassa intensità
            var noise = new System.Random(3);
            for (int i = 0; i < 300; i++)
                Assert.AreNotEqual(CrookedEvent.Straightened, l.Feed((float)noise.NextDouble() * 0.3f - 0.15f, (float)noise.NextDouble() * 0.3f - 0.15f, Dt));
            Assert.AreNotEqual(CrookedStage.Straightened, l.Stage);
        }

        [Test]
        public void AlignedThenLaidFlat_StopsCounting()
        {
            var l = Calibrated();
            Hold(l, CrookedPictureLogic.RoomAngle, 0.6f);
            Assert.IsTrue(l.Aligned);
            Assert.AreEqual(CrookedEvent.None, Hold(l, CrookedPictureLogic.RoomAngle, 3f, 88f, CrookedEvent.Straightened));
            Assert.IsFalse(l.Aligned);
            Assert.AreEqual(0f, l.Hold, 0.001f);
        }

        [Test]
        public void FlatForLong_RecalibratesWhenPickedUp()
        {
            var l = Calibrated(0f);
            Hold(l, 0f, 2f, 88f);                          // posato sul tavolo
            Assert.AreEqual(CrookedStage.Calibrating, l.Stage);
            Assert.AreEqual(CrookedEvent.Calibrated, Hold(l, 6f, 1f, 20f, CrookedEvent.Calibrated));   // ripreso un po' storto
            Assert.AreEqual(6f, l.Baseline, 0.5f);
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, 6f + CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));
        }

        [Test]
        public void WrongDirection_DoesNothing_JustOneRemark()
        {
            var l = Calibrated();
            Assert.AreEqual(CrookedEvent.WrongWay, Hold(l, -CrookedPictureLogic.RoomAngle, 2f, 20f, CrookedEvent.WrongWay));
            Assert.AreEqual(CrookedEvent.None, Hold(l, -CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.WrongWay));   // non ripete
            Assert.AreEqual(1, l.WrongWays);
            Assert.IsFalse(l.Aligned);
            Assert.AreEqual(0f, l.Hold);
            Assert.AreEqual(-1f, l.Bubble, 0.01f);        // la bolla resta (anzi va) dalla parte alta
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
            Hold(l, 0f, 1f);                               // tornando indietro si riarma
            Assert.AreEqual(CrookedEvent.WrongWay, Hold(l, -CrookedPictureLogic.RoomAngle, 2f, 20f, CrookedEvent.WrongWay));
            Assert.AreEqual(2, l.WrongWays);
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));
        }

        [Test]
        public void WrongMoves_KeepTheState()
        {
            var l = Calibrated();
            Hold(l, CrookedPictureLogic.RoomAngle, 0.5f);
            float hold = l.Hold;
            Assert.IsTrue(l.Aligned);
            Assert.AreEqual(1, l.TwistPicture());
            Assert.AreEqual(2, l.TwistPicture());
            Assert.AreEqual(1, l.Tape());
            Assert.AreEqual(1, l.Hammer());
            Assert.AreEqual(1, l.TapLevel());
            Assert.AreEqual(1, l.TapGlass());
            Assert.AreEqual(1, l.TapMarble());
            Assert.IsTrue(l.Aligned);
            Assert.AreEqual(hold, l.Hold);
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle, 2f, 20f, CrookedEvent.Straightened));
        }

        [Test]
        public void Marble_RollsToTheLowSide_AndBackAfterATap()
        {
            var l = Calibrated();
            Hold(l, 0f, 3f);
            Assert.Greater(l.MarbleX, 0.9f);               // lato basso = destra (stanza girata in senso orario)
            l.TapMarble();
            Hold(l, 0f, 0.35f);
            Assert.Less(l.MarbleX, 0.6f);                  // spinta verso il lato alto...
            Hold(l, 0f, 4f);
            Assert.Greater(l.MarbleX, 0.9f);               // ...e torna giù
            Hold(l, CrookedPictureLogic.RoomAngle, 0.5f);  // stanza dritta per chi guarda: la biglia si ferma
            float x = l.MarbleX;
            Hold(l, CrookedPictureLogic.RoomAngle, 0.3f);
            Assert.AreEqual(x, l.MarbleX, 0.1f);
        }

        [Test]
        public void PauseResume_ResetsHold_StaysValid()
        {
            var l = Calibrated();
            Hold(l, CrookedPictureLogic.RoomAngle, 0.6f);
            Assert.Greater(l.Hold, 0f);
            l.OnResume();
            Assert.AreEqual(0f, l.Hold);
            Assert.IsFalse(l.Aligned);
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));
        }

        [Test]
        public void NaNAndBadTimeSteps_AreHarmless()
        {
            var l = Calibrated();
            Assert.AreEqual(CrookedEvent.None, l.Feed(float.NaN, float.NaN, Dt));
            Assert.AreEqual(CrookedEvent.None, l.Feed(0f, -1f, -1f));
            Assert.AreEqual(CrookedEvent.None, l.Feed(0f, -1f, float.NaN));
            Assert.AreEqual(CrookedStage.Playing, l.Stage);
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));
        }

        [Test]
        public void WinsOnlyOnce_InputLockedAfterwards()
        {
            var l = Calibrated();
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));
            Assert.AreEqual(CrookedEvent.None, Hold(l, CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));
            Assert.AreEqual(0, l.TwistPicture());
            Assert.AreEqual(0, l.TapMarble());
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(CrookedStage.Done, l.Stage);
            Assert.AreEqual(CrookedEvent.None, l.Feed(0f, -1f, Dt));
        }

        [Test]
        public void Reset_StartsOver()
        {
            var l = Calibrated(5f);
            Hold(l, -CrookedPictureLogic.RoomAngle + 5f, 2f);
            l.TwistPicture();
            l.TapMarble();
            Hold(l, 5f + CrookedPictureLogic.RoomAngle, 3f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(CrookedStage.Calibrating, l.Stage);
            Assert.AreEqual(0f, l.Hold);
            Assert.AreEqual(0f, l.Baseline);
            Assert.AreEqual(0, l.Twists);
            Assert.AreEqual(0, l.MarbleTaps);
            Assert.AreEqual(0, l.WrongWays);
            Assert.IsFalse(l.Aligned);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(CrookedEvent.Calibrated, Hold(l, 0f, 1f, 20f, CrookedEvent.Calibrated));
            Assert.AreEqual(CrookedEvent.Straightened, Hold(l, CrookedPictureLogic.RoomAngle, 3f, 20f, CrookedEvent.Straightened));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void DeltaAngle_WrapsAround()
        {
            Assert.AreEqual(-20f, CrookedPictureLogic.DeltaAngle(170f, 150f), 0.001f);
            Assert.AreEqual(20f, CrookedPictureLogic.DeltaAngle(170f, -170f), 0.001f);
            Assert.AreEqual(-20f, CrookedPictureLogic.DeltaAngle(-170f, 170f), 0.001f);
        }
    }
}
