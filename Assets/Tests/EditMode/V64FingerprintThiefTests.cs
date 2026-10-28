using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 251: il ladro d'impronte (PIN finti, lettore, foto del proprietario, inclinazione).</summary>
    public class V64FingerprintThiefTests
    {
        const float Dt = 1f / 60f;

        static void Run(FingerprintThiefLogic l, float seconds, float gx, float gy, bool step = true)
        {
            for (float t = 0f; t < seconds; t += Dt)
            {
                l.Tick(Dt);
                l.Tilt(gx, gy, Dt);
                if (step) l.Step(Dt);
            }
        }

        /// <summary>Foto agganciata e premuta col telefono dritto, poi inclinato un po' verso sinistra.</summary>
        static FingerprintThiefLogic Solve(float g0x = 0f, float g0y = -0.7f)
        {
            var l = new FingerprintThiefLogic();
            Assert.IsTrue(l.Dock());
            l.SetContact(true, g0x, g0y);
            Assert.AreEqual(FpLed.Yellow, l.Led);
            Run(l, 2f, g0x - 0.19f, g0y);
            return l;
        }

        [Test]
        public void CorrectPath_DockHoldTilt_Unlocks()
        {
            FingerprintThiefLogic l = Solve();
            Assert.IsTrue(l.Unlocked);
            Assert.AreEqual(FpLed.Green, l.Led);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());   // la vittoria una volta sola
        }

        [Test]
        public void WorksFromAnyStartingAngle()
        {
            Assert.IsTrue(Solve(0f, -0.95f).Unlocked);   // telefono quasi verticale
            Assert.IsTrue(Solve(0.1f, -0.1f).Unlocked);  // quasi piatto sul tavolo
            Assert.IsTrue(Solve(0.3f, -0.6f).Unlocked);  // già un po' storto
        }

        [Test]
        public void TiltTolerance_IsGenerous()
        {
            // inclinazioni diverse attorno a quella giusta funzionano tutte
            foreach (float dx in new[] { -0.12f, -0.19f, -0.25f })
                foreach (float dy in new[] { -0.05f, 0f, 0.05f })
                {
                    var l = new FingerprintThiefLogic();
                    l.Dock();
                    l.SetContact(true, 0f, -0.7f);
                    Run(l, 2.5f, dx, -0.7f + dy);
                    Assert.IsTrue(l.Unlocked, "dx " + dx + " dy " + dy);
                }
        }

        [Test]
        public void PhotoDroppedOnScanner_OnlyFlashesYellow()
        {
            var l = new FingerprintThiefLogic();
            l.Dock();
            l.SetContact(false, 0f, -1f);
            Run(l, 3f, -0.3f, -1f);
            Assert.IsFalse(l.Unlocked);
            l.SetContact(true, 0f, -1f);
            l.SetContact(false, 0f, -1f);
            Assert.AreEqual(FpLed.Yellow, l.Led);    // lasciata sul lettore: giallo per un attimo
            Run(l, 1f, 0f, -1f);
            Assert.AreEqual(FpLed.Off, l.Led);
            Assert.AreEqual(1, l.Drops);
        }

        [Test]
        public void NoTilt_NoUnlock_EvenHeldForever()
        {
            var l = new FingerprintThiefLogic();
            l.Dock();
            l.SetContact(true, 0f, -0.7f);
            Run(l, 10f, 0f, -0.7f);
            Assert.IsFalse(l.Unlocked);
            Assert.Greater(l.TipDistance, FingerprintThiefLogic.AlignTolerance);
        }

        [Test]
        public void WrongDirection_NoUnlock_AndPhotoStaysInTray()
        {
            var l = new FingerprintThiefLogic();
            l.Dock();
            l.SetContact(true, 0f, -0.7f);
            Run(l, 3f, 1f, 0.3f);
            Assert.IsFalse(l.Unlocked);
            float r = (float)System.Math.Sqrt(l.OffsetX * l.OffsetX + l.OffsetY * l.OffsetY);
            Assert.LessOrEqual(r, FingerprintThiefLogic.TrayRadius + 0.001f);
        }

        [Test]
        public void ReleasingFinger_StopsTimer_ButKeepsPosition()
        {
            var l = new FingerprintThiefLogic();
            l.Dock();
            l.SetContact(true, 0f, -0.7f);
            Run(l, 0.9f, -0.19f, -0.7f, false);
            float ox = l.OffsetX, oy = l.OffsetY;
            l.SetContact(false, -0.19f, -0.7f);
            Run(l, 1f, 0f, -0.7f);                 // senza dito l'inclinazione non conta
            Assert.AreEqual(ox, l.OffsetX, 1e-5);
            Assert.AreEqual(oy, l.OffsetY, 1e-5);
            Assert.IsFalse(l.Unlocked);
            // di nuovo premuta: si riparte da dov'era, rispetto alla nuova posizione del telefono
            l.SetContact(true, 0f, -0.7f);
            Run(l, 2f, 0f, -0.7f);
            Assert.IsTrue(l.Unlocked);
        }

        [Test]
        public void BriefAlignment_IsNotEnough()
        {
            var l = new FingerprintThiefLogic();
            l.Dock();
            l.SetContact(true, 0f, -0.7f);
            Run(l, 0.8f, -0.19f, -0.7f, false);
            Assert.IsTrue(l.Aligned);
            for (int i = 0; i < 30; i++) { l.Tilt(-0.19f, -0.7f, Dt); l.Step(Dt); }   // mezzo secondo
            Assert.IsFalse(l.Unlocked);
            l.SetContact(false, 0f, -1f);
            l.Step(Dt);
            Assert.AreEqual(0f, l.AlignTimer);
        }

        [Test]
        public void Undock_ResetsPhotoOnly()
        {
            var l = new FingerprintThiefLogic();
            l.PressDigit(1);
            l.Dock();
            l.SetContact(true, 0f, -0.7f);
            l.Undock();
            Assert.IsFalse(l.Docked);
            Assert.IsFalse(l.Contact);
            Assert.AreEqual(1, l.Entered);
            l.SetContact(true, 0f, -0.7f);         // senza aggancio il dito non conta
            Assert.IsFalse(l.Contact);
        }

        [Test]
        public void Pins_AllFail_ThenFakeLockout()
        {
            var l = new FingerprintThiefLogic();
            int[] pins = { 1, 2, 3, 4, 0, 0, 0, 0, 1, 2, 3, 4 };
            FpPinResult last = FpPinResult.Ignored;
            int wrong = 0;
            foreach (int d in pins) { last = l.PressDigit(d); if (last == FpPinResult.Wrong) wrong++; }
            Assert.AreEqual(2, wrong);
            Assert.AreEqual(FpPinResult.LockedOut, last);
            Assert.IsTrue(l.IsLockedOut);
            Assert.AreEqual(30, l.LockoutSecondsShown);
            Assert.AreEqual(FpPinResult.StillLocked, l.PressDigit(5));
        }

        [Test]
        public void Lockout_DoesNotBlockThePhoto_AndEnds()
        {
            var l = new FingerprintThiefLogic();
            for (int i = 0; i < 12; i++) l.PressDigit(i % 10);
            Assert.IsTrue(l.IsLockedOut);
            Assert.IsTrue(l.Dock());
            l.SetContact(true, 0f, -0.7f);
            Run(l, 2f, -0.19f, -0.7f);
            Assert.IsTrue(l.Unlocked);              // il blocco è finto

            var m = new FingerprintThiefLogic();
            for (int i = 0; i < 12; i++) m.PressDigit(i % 10);
            bool ended = false;
            for (float t = 0f; t < 31f; t += 0.5f) ended |= m.Tick(0.5f);
            Assert.IsTrue(ended);
            Assert.AreEqual(FingerprintThiefLogic.MaxAttempts, m.AttemptsLeft);
            Assert.AreEqual(FpPinResult.Digit, m.PressDigit(1));
        }

        [Test]
        public void Scanner_GetsMoreRidiculous_AndNeverRepeatsTheFirst()
        {
            var l = new FingerprintThiefLogic();
            Assert.AreEqual("UNKNOWN FINGERPRINT", l.TapScanner());
            Assert.AreEqual(FpLed.Red, l.Led);
            Assert.AreEqual("Still not your finger.", l.TapScanner());
            Assert.AreEqual("Stop trying. We know it's you.", l.TapScanner());
            Assert.AreEqual("Nice finger. Wrong owner.", l.TapScanner());
            for (int i = 0; i < 10; i++) Assert.AreNotEqual("UNKNOWN FINGERPRINT", l.TapScanner());
        }

        [Test]
        public void Shake_HasCooldown_AndDoesNotUnlock()
        {
            var l = new FingerprintThiefLogic();
            Assert.IsTrue(l.Shake());
            Assert.IsFalse(l.Shake());
            l.Tick(FingerprintThiefLogic.ShakeCooldown + 0.1f);
            Assert.IsTrue(l.Shake());
            Assert.IsFalse(l.Unlocked);
        }

        [Test]
        public void AfterUnlock_EverythingIsIgnored()
        {
            FingerprintThiefLogic l = Solve();
            Assert.AreEqual(FpPinResult.Ignored, l.PressDigit(1));
            Assert.IsNull(l.TapScanner());
            Assert.IsFalse(l.Shake());
            Assert.IsFalse(l.Step(Dt));
            l.Undock();
            Assert.IsTrue(l.Docked);
        }

        [Test]
        public void Reset_RestoresInitialState()
        {
            FingerprintThiefLogic l = Solve();
            l.TryComplete();
            l.Reset();
            Assert.IsFalse(l.Unlocked);
            Assert.IsFalse(l.Docked);
            Assert.AreEqual(FingerprintThiefLogic.MaxAttempts, l.AttemptsLeft);
            Assert.AreEqual(FpLed.Off, l.Led);
            Assert.IsFalse(l.CompletionReported);
        }

        [Test]
        public void Gravity_InvertsWithHysteresis_AndFiltersJitter()
        {
            var g = new V64AGravity();
            g.Reset();
            for (int i = 0; i < 120; i++) g.Feed(0f, 0.45f + ((i % 2) == 0 ? 0.04f : -0.04f), Dt);   // rumore sotto soglia
            Assert.IsFalse(g.Inverted);
            for (int i = 0; i < 120; i++) g.Feed(0f, 0.9f, Dt);
            Assert.IsTrue(g.Inverted);
            for (int i = 0; i < 120; i++) g.Feed(0f, 0.3f, Dt);                                         // dentro l'isteresi
            Assert.IsTrue(g.Inverted);
            for (int i = 0; i < 120; i++) g.Feed(0f, -0.9f, Dt);
            Assert.IsFalse(g.Inverted);
            // un picco di un fotogramma non capovolge niente
            g.Feed(0f, 1f, Dt);
            Assert.IsFalse(g.Inverted);
            // telefono piatto: l'angolo resta quello di prima
            for (int i = 0; i < 120; i++) g.Feed(0.9f, 0f, Dt);
            float a = g.AngleDeg;
            Assert.AreEqual(90f, a, 1f);
            for (int i = 0; i < 120; i++) g.Feed(0.05f, 0.02f, Dt);
            Assert.AreEqual(a, g.AngleDeg, 5f);
        }
    }
}
