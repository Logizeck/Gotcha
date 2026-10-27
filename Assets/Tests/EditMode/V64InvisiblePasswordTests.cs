using System;
using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 255: tastiera, cardini con due dita, porta staccata e inclinazione, 0000 finto e 8008 nello specchio.</summary>
    public class V64InvisiblePasswordTests
    {
        static PasswordReaction Type(InvisiblePasswordLogic l, string code)
        {
            PasswordReaction r = PasswordReaction.None;
            foreach (char c in code) r = l.PressDigit(c - '0');
            return r;
        }

        static PasswordReaction PullBoth(InvisiblePasswordLogic l, float amount, int steps = 10)
        {
            PasswordReaction last = PasswordReaction.None;
            for (int i = 1; i <= steps; i++)
            {
                PasswordReaction r = l.UpdatePins(true, amount * i / steps, true, amount * i / steps);
                if (r != PasswordReaction.None) last = r;
                if (r == PasswordReaction.PinsOut) break;
            }
            return last;
        }

        static PasswordReaction Tilt(InvisiblePasswordLogic l, float x, float seconds)
        {
            PasswordReaction last = PasswordReaction.None;
            for (float t = 0f; t < seconds; t += 0.02f)
            {
                PasswordReaction r = l.UpdateTilt(x, 0.02f);
                if (r != PasswordReaction.None) last = r;
            }
            return last;
        }

        [Test]
        public void FullSolution_Completes_Once()
        {
            var l = new InvisiblePasswordLogic();
            Assert.AreEqual(PasswordReaction.PinsOut, PullBoth(l, 0.8f));
            Assert.IsTrue(l.Detached);
            Assert.AreEqual(PasswordDoorState.Detached, l.State);
            Tilt(l, 0.45f, 1.5f);
            Assert.IsTrue(l.RearSeen, "a mid tilt shows the back of the door (0000)");
            Assert.IsFalse(l.MirrorSeen);
            Assert.AreEqual(PasswordReaction.NiceTry, Type(l, "0000"), "the decoy still fails");
            Tilt(l, 0.7f, 1.5f);
            Assert.IsTrue(l.MirrorSeen, "a strong tilt puts the back in the mirror");
            Assert.AreEqual(PasswordReaction.Unlocks, Type(l, "8008"));
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "success cannot trigger twice");
            Assert.AreEqual(PasswordReaction.None, l.PressDigit(1));
        }

        [Test]
        public void CorrectCode_WorksImmediately_EvenWithHinges()
        {
            var l = new InvisiblePasswordLogic();
            Assert.AreEqual(PasswordReaction.Unlocks, Type(l, "8008"));
            Assert.IsFalse(l.Detached);
        }

        [Test]
        public void WrongCodes_NeverLock_AndVaryMessages()
        {
            var l = new InvisiblePasswordLogic();
            string prev = null;
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(PasswordReaction.Wrong, Type(l, "5678"));
                Assert.AreNotEqual(prev, l.LastMessage);
                prev = l.LastMessage;
                Assert.AreEqual("", l.Entry);
            }
            Assert.AreEqual(PasswordReaction.Unlocks, Type(l, "8008"), "still solvable after many errors");
        }

        [Test]
        public void SpecialMessages()
        {
            var l = new InvisiblePasswordLogic();
            Assert.AreEqual(PasswordReaction.NiceTry, Type(l, "0000"));
            Assert.AreEqual("NICE TRY.", l.LastMessage);
            Type(l, "0000");
            Assert.AreNotEqual("NICE TRY.", l.LastMessage);
            Assert.AreEqual(PasswordReaction.SoDidWe, l.ForgotPassword());
            Assert.AreEqual("SO DID WE.", l.LastMessage);
            Type(l, "1234");
            Assert.AreEqual(InvisiblePasswordLogic.ObviousLine, l.LastMessage);
        }

        [Test]
        public void PartialEntry_Clear_And_InvalidDigits()
        {
            var l = new InvisiblePasswordLogic();
            Assert.AreEqual(PasswordReaction.Typed, l.PressDigit(8));
            Assert.AreEqual(PasswordReaction.None, l.PressDigit(12));
            Assert.AreEqual("8", l.Entry);
            Assert.AreEqual(PasswordReaction.Cleared, l.Clear());
            Assert.AreEqual(PasswordReaction.None, l.Clear());
            Assert.AreEqual(PasswordReaction.Unlocks, Type(l, "8008"));
        }

        [Test]
        public void DoorTaps_LoosenHingeOnce()
        {
            var l = new InvisiblePasswordLogic();
            Assert.AreEqual(PasswordReaction.DoorShakes, l.TapDoor());
            Assert.AreEqual(PasswordReaction.DoorShakes, l.TapDoor());
            Assert.AreEqual(PasswordReaction.HingeLoosens, l.TapDoor());
            Assert.IsTrue(l.HingeLoose);
            Assert.AreEqual(PasswordReaction.DoorShakes, l.TapDoor());
            Assert.IsFalse(l.Detached, "tapping alone never detaches the door");
        }

        [Test]
        public void OneFinger_OnlyStrains_AndNeverDetaches()
        {
            var l = new InvisiblePasswordLogic();
            Assert.AreEqual(PasswordReaction.Straining, l.UpdatePins(true, 2f, false, 0f));
            Assert.AreEqual(PasswordReaction.None, l.UpdatePins(true, 3f, false, 0f), "message only once per grab");
            Assert.AreEqual(PasswordReaction.None, l.UpdatePins(false, 0f, true, 5f));
            Assert.IsFalse(l.Detached);
            Assert.AreEqual(0f, l.PinProgress);
        }

        [Test]
        public void ReleasingEarly_SlidesPinsBack_ButKeepsOtherProgress()
        {
            var l = new InvisiblePasswordLogic();
            l.TapDoor(); l.TapDoor(); l.TapDoor();
            PullBoth(l, 0.4f);
            Assert.Greater(l.PinProgress, 0.5f);
            Assert.AreEqual(PasswordReaction.PinsSlideBack, l.UpdatePins(true, 0.4f, false, 0f), "one finger released first");
            Assert.AreEqual(0f, l.PinProgress);
            Assert.IsTrue(l.HingeLoose);
            Assert.AreEqual(PasswordReaction.PinsOut, PullBoth(l, 0.7f));
        }

        [Test]
        public void PullInward_DoesNothing()
        {
            var l = new InvisiblePasswordLogic();
            for (int i = 0; i < 20; i++) l.UpdatePins(true, -1f, true, -1f);
            Assert.IsFalse(l.Detached);
            Assert.AreEqual(0f, l.PinProgress);
        }

        [Test]
        public void Tilt_BeforeDetaching_DoesNothing()
        {
            var l = new InvisiblePasswordLogic();
            Tilt(l, 1f, 2f);
            Assert.AreEqual(0f, l.TargetDoorDeg);
            Assert.IsFalse(l.RearSeen);
        }

        [Test]
        public void Tilt_FilteredAgainstJitter_AndHysteresis()
        {
            var l = new InvisiblePasswordLogic();
            PullBoth(l, 1f);
            // telefono quasi dritto con rumore: la porta oscilla appena, niente retro
            var rnd = new Random(3);
            for (int i = 0; i < 300; i++) l.UpdateTilt((float)(rnd.NextDouble() - 0.5) * 0.2f, 0.016f);
            Assert.IsFalse(l.RearSeen);
            Assert.Less(l.TargetDoorDeg, 40f);
            // un picco singolo (scossone) non apre la porta del tutto
            l.UpdateTilt(1.5f, 0.016f);
            Assert.IsFalse(l.MirrorAligned);
            // forte inclinazione: specchio; tornando appena indietro resta allineato (isteresi)
            Tilt(l, 0.75f, 2f);
            Assert.IsTrue(l.MirrorAligned);
            Tilt(l, 0.53f, 2f);
            Assert.IsTrue(l.MirrorAligned, "hysteresis keeps the reflection near the threshold");
            Tilt(l, 0.1f, 2f);
            Assert.IsFalse(l.MirrorAligned);
            Assert.IsFalse(l.RearVisible);
            Assert.IsTrue(l.MirrorSeen, "discoveries persist");
        }

        [Test]
        public void TiltLeft_KeepsDoorShut()
        {
            var l = new InvisiblePasswordLogic();
            PullBoth(l, 1f);
            Tilt(l, -0.9f, 2f);
            Assert.AreEqual(0f, l.TargetDoorDeg);
        }

        [Test]
        public void SlowRotation_RevealsRearBeforeMirror()
        {
            var l = new InvisiblePasswordLogic();
            PullBoth(l, 1f);
            bool rearFirst = false;
            for (float x = 0f; x < 0.8f; x += 0.002f)
            {
                PasswordReaction r = l.UpdateTilt(x, 0.02f);
                if (r == PasswordReaction.RearRevealed) rearFirst = !l.MirrorSeen;
            }
            Assert.IsTrue(rearFirst);
            Assert.IsTrue(l.MirrorSeen);
        }

        [Test]
        public void NaNTilt_IsIgnored()
        {
            var l = new InvisiblePasswordLogic();
            PullBoth(l, 1f);
            l.UpdateTilt(float.NaN, 0.02f);
            Assert.IsFalse(float.IsNaN(l.SmoothedTilt));
        }

        [Test]
        public void Reset_RestoresStart()
        {
            var l = new InvisiblePasswordLogic();
            PullBoth(l, 1f);
            Tilt(l, 0.8f, 2f);
            Type(l, "8008");
            l.TryComplete();
            l.Reset();
            Assert.AreEqual(PasswordDoorState.Hinged, l.State);
            Assert.IsFalse(l.Detached);
            Assert.IsFalse(l.MirrorSeen);
            Assert.IsFalse(l.CompletionReported);
            Assert.AreEqual(PasswordReaction.Typed, l.PressDigit(1));
        }

        [Test]
        public void TheRealCodeIsMirrorSymmetric()
        {
            // l'8 e lo 0 sono simmetrici e "8008" è palindromo: allo specchio si legge uguale
            char[] c = InvisiblePasswordLogic.Code.ToCharArray();
            Array.Reverse(c);
            Assert.AreEqual(InvisiblePasswordLogic.Code, new string(c));
            Assert.AreNotEqual(InvisiblePasswordLogic.Decoy, InvisiblePasswordLogic.Code);
        }
    }
}
