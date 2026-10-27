using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 256: occhi del mago coperti, sussurro, botola, coniglio che sparisce.</summary>
    public class V64IncompetentMagicianTests
    {
        static MagicianReaction Run(IncompetentMagicianLogic l, float seconds, bool sound, bool mic = true)
        {
            MagicianReaction last = MagicianReaction.None;
            for (float t = 0f; t < seconds; t += 0.02f)
            {
                MagicianReaction r = l.Tick(0.02f, sound, mic);
                if (r != MagicianReaction.None) last = r;
            }
            return last;
        }

        [Test]
        public void FullSolution_Completes_Once()
        {
            var l = new IncompetentMagicianLogic();
            Assert.AreEqual(MagicianReaction.EyesClosed, l.SetEyesCovered(true));
            Assert.IsTrue(l.EyesClosed);
            Assert.AreEqual(MagicianReaction.RabbitPoints, Run(l, 0.5f, true));
            Assert.IsTrue(l.TrapdoorFound);
            Assert.IsFalse(l.NeedsMic, "mic released once the trapdoor is found");
            Assert.AreEqual(MagicianReaction.RubsEyes, l.SetEyesCovered(false));
            Assert.AreEqual(MagicianState.RubbingEyes, l.State);
            Assert.AreEqual(MagicianReaction.TrapdoorOpens, l.TryOpenTrapdoor());
            Run(l, 1.5f, false);
            Assert.AreEqual(MagicianReaction.RabbitVanishes, l.DropRabbitInTrapdoor());
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ReleasingOneFingerBeforeWhisper_ReopensEyes()
        {
            var l = new IncompetentMagicianLogic();
            l.SetEyesCovered(true);
            Run(l, 0.1f, true);
            Assert.AreEqual(MagicianReaction.EyesOpen, l.SetEyesCovered(false));
            Assert.AreEqual(MagicianState.Performing, l.State);
            Assert.AreEqual(0f, l.Whisper, "whisper must be done in one blindfold");
            l.SetEyesCovered(true);
            Run(l, 0.1f, true);
            Assert.IsFalse(l.TrapdoorFound);
        }

        [Test]
        public void SoundWithEyesOpen_IsAFailedSpell_WithCooldown()
        {
            var l = new IncompetentMagicianLogic();
            Assert.AreEqual(MagicianReaction.FailedSpell, l.Tick(0.02f, true, true));
            Assert.AreEqual(MagicianReaction.None, l.Tick(0.02f, true, true));
            Run(l, 3f, false);
            Assert.AreEqual(MagicianReaction.FailedSpell, l.Tick(0.02f, true, true));
            Assert.IsFalse(l.TrapdoorFound, "sound before blindfolding reveals nothing");
        }

        [Test]
        public void Trapdoor_BeforeDiscovery_IsJustHollowFloor()
        {
            var l = new IncompetentMagicianLogic();
            Assert.AreEqual(MagicianReaction.HollowFloor, l.TryOpenTrapdoor());
            Assert.IsFalse(l.TrapdoorOpen);
            Assert.AreEqual(MagicianReaction.None, l.DropRabbitInTrapdoor(), "rabbit can't vanish through a closed floor");
        }

        [Test]
        public void GraceExpires_MagicianSlamsTrapdoor_DiscoveryKept()
        {
            var l = new IncompetentMagicianLogic();
            l.SetEyesCovered(true);
            Run(l, 0.5f, true);
            l.SetEyesCovered(false);
            l.TryOpenTrapdoor();
            Assert.AreEqual(MagicianReaction.MagicianSlamsShut, Run(l, IncompetentMagicianLogic.GraceSeconds + 0.5f, false));
            Assert.IsFalse(l.TrapdoorOpen);
            Assert.IsTrue(l.TrapdoorFound);
            Assert.AreEqual(MagicianReaction.MagicianStopsYou, l.TryOpenTrapdoor());
            // copre di nuovo gli occhi: niente sussurro, basta togliere le dita
            l.SetEyesCovered(true);
            Assert.AreEqual(MagicianReaction.RubsEyes, l.SetEyesCovered(false));
            Assert.AreEqual(MagicianReaction.TrapdoorOpens, l.TryOpenTrapdoor());
            Assert.AreEqual(MagicianReaction.RabbitVanishes, l.DropRabbitInTrapdoor());
        }

        [Test]
        public void GraceExpires_WithClosedTrapdoor_JustOpensEyes()
        {
            var l = new IncompetentMagicianLogic();
            l.SetEyesCovered(true);
            Run(l, 0.5f, true);
            l.SetEyesCovered(false);
            Assert.AreEqual(MagicianReaction.EyesOpen, Run(l, IncompetentMagicianLogic.GraceSeconds + 0.5f, false));
            Assert.IsTrue(l.Watching);
        }

        [Test]
        public void NoMic_FallbackByHoldingEyes()
        {
            var l = new IncompetentMagicianLogic();
            Assert.IsTrue(l.FallbackReady(false));
            l.SetEyesCovered(true);
            Assert.AreEqual(MagicianReaction.RabbitPoints, Run(l, IncompetentMagicianLogic.FallbackHold + 0.2f, false, false));
        }

        [Test]
        public void MicButSilentPlayer_FallbackAppearsAfterAWhile()
        {
            var l = new IncompetentMagicianLogic();
            Assert.IsFalse(l.FallbackReady(true));
            l.SetEyesCovered(true);
            Run(l, IncompetentMagicianLogic.FallbackAfter - 0.5f, false);
            Assert.IsFalse(l.TrapdoorFound);
            Run(l, 0.6f, false);
            Assert.IsTrue(l.FallbackReady(true));
            Assert.AreEqual(MagicianReaction.RabbitPoints, Run(l, IncompetentMagicianLogic.FallbackHold + 0.2f, false));
        }

        [Test]
        public void WrongInteractions_DoNotBreakTheLevel()
        {
            var l = new IncompetentMagicianLogic();
            Assert.AreEqual(MagicianReaction.RabbitWearsHat, l.RabbitIntoHat());
            Assert.AreEqual(MagicianReaction.RabbitReappears, l.RabbitIntoBox());
            Assert.AreEqual(MagicianReaction.Confetti, l.TapWand());
            Assert.AreEqual(MagicianReaction.Bows, l.TapMagician());
            Assert.AreEqual(1, l.Bows);
            Assert.AreEqual(MagicianState.Performing, l.State);
            l.SetEyesCovered(true);
            Assert.AreEqual(MagicianReaction.None, l.TapMagician(), "he can't bow while blindfolded");
            Assert.AreEqual(MagicianReaction.RabbitPoints, Run(l, 0.5f, true));
        }

        [Test]
        public void RepeatedCoverEvents_AreIdempotent()
        {
            var l = new IncompetentMagicianLogic();
            l.SetEyesCovered(true);
            Assert.AreEqual(MagicianReaction.None, l.SetEyesCovered(true));
            l.SetEyesCovered(false);
            Assert.AreEqual(MagicianReaction.None, l.SetEyesCovered(false));
        }

        [Test]
        public void Reset_RestoresStart()
        {
            var l = new IncompetentMagicianLogic();
            l.SetEyesCovered(true);
            Run(l, 0.5f, true);
            l.Reset();
            Assert.IsFalse(l.TrapdoorFound);
            Assert.AreEqual(MagicianState.Performing, l.State);
            Assert.IsTrue(l.NeedsMic);
        }

        [Test]
        public void MicGate_CalibratesToAmbient()
        {
            var g = new MagicianMicGate();
            // stanza rumorosa costante a 0.12: dopo la calibrazione non conta come suono
            bool any = false;
            for (int i = 0; i < 200; i++) any |= g.Feed(0.12f, 0.02f);
            Assert.IsFalse(any);
            Assert.Greater(g.Threshold, 0.12f);
            // un suono sopra il fondo, tenuto un attimo, conta
            bool heard = false;
            for (int i = 0; i < 10; i++) heard |= g.Feed(g.Threshold + 0.1f, 0.02f);
            Assert.IsTrue(heard);
        }

        [Test]
        public void MicGate_IgnoresSingleSpike_AndNaN()
        {
            var g = new MagicianMicGate();
            for (int i = 0; i < 50; i++) g.Feed(0.01f, 0.02f);
            Assert.IsFalse(g.Feed(0.9f, 0.02f));
            Assert.IsFalse(g.Feed(0.01f, 0.02f));
            Assert.IsFalse(g.Feed(float.NaN, 0.02f));
        }

        [Test]
        public void MicGate_NothingCountsWhileCalibrating()
        {
            var g = new MagicianMicGate();
            Assert.IsTrue(g.Calibrating);
            bool any = false;
            for (int i = 0; i < 20; i++) any |= g.Feed(0.9f, 0.02f);
            Assert.IsFalse(any, "input before calibration is ignored");
        }

        [Test]
        public void MicGate_QuietRoom_DetectsQuietSound()
        {
            var g = new MagicianMicGate();
            for (int i = 0; i < 100; i++) g.Feed(0.01f, 0.02f);
            Assert.LessOrEqual(g.Threshold, 0.08f, "a whisper must be enough in a quiet room");
        }
    }
}
