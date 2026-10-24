using System;
using NUnit.Framework;
using Gotcha.Levels.V62;

namespace Gotcha.Tests
{
    /// <summary>Livello 252: svegliare Toni (sveglia, lampada, gatto ladro, piuma, biscotto).</summary>
    public class V62WakeToniTests
    {
        [Test]
        public void CorrectPath_Completes()
        {
            var l = new WakeToniLogic();
            Assert.IsTrue(l.TryGiveFeatherToCat());
            Assert.IsTrue(l.CatBusy);
            Assert.IsTrue(l.TryPickCookie());
            Assert.AreEqual(WakeReaction.Wakes, l.CookieAtToni());
            Assert.AreEqual(WakeToniState.Waking, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryCompleteWake());
            Assert.AreEqual(WakeToniState.Done, l.State);
            Assert.IsFalse(l.EggFound);
        }

        [Test]
        public void CookieWithFreeCat_IsStolen_ThenRespawns()
        {
            var l = new WakeToniLogic();
            l.TryPickCookie();
            Assert.AreEqual(WakeReaction.CatSteals, l.CookieAtToni());
            Assert.AreEqual(WakeToniState.CatStealing, l.State);
            Assert.IsFalse(l.TryPickCookie());     // il biscotto ce l'ha il gatto
            Assert.IsTrue(l.TryRespawnCookie());
            Assert.AreEqual(WakeToniState.Sleeping, l.State);
            Assert.AreEqual(1, l.Steals);
            Assert.IsTrue(l.TryPickCookie());
        }

        [Test]
        public void Alarm_And_Lamp_DoNotWakeHim()
        {
            var l = new WakeToniLogic();
            Assert.AreEqual(WakeReaction.SlapsAlarm, l.TapAlarm());
            Assert.AreEqual(2, l.Snore);
            l.TapAlarm(); l.TapAlarm();
            Assert.AreEqual(3, l.Snore);              // russa più forte, ma non oltre
            bool left = l.FacingLeft;
            Assert.AreEqual(WakeReaction.TurnsOver, l.TapLamp());
            Assert.IsTrue(l.LampOn);
            Assert.AreNotEqual(left, l.FacingLeft);
            Assert.AreEqual(WakeReaction.None, l.TapLamp());   // spenta: niente
            Assert.IsFalse(l.LampOn);
            Assert.AreEqual(WakeToniState.Sleeping, l.State);
        }

        [Test]
        public void CookieDroppedElsewhere_GoesBack()
        {
            var l = new WakeToniLogic();
            l.TryPickCookie();
            Assert.AreEqual(WakeReaction.CookieBack, l.DropCookieElsewhere());
            Assert.AreEqual(WakeToniState.Sleeping, l.State);
            Assert.AreEqual(WakeReaction.None, l.DropCookieElsewhere());
        }

        [Test]
        public void FeatherBackFromCat_FreesTheCat()
        {
            var l = new WakeToniLogic();
            l.TryGiveFeatherToCat();
            Assert.IsFalse(l.TryGiveFeatherToCat());
            Assert.IsTrue(l.TryTakeFeatherFromCat());
            Assert.IsFalse(l.CatBusy);
            l.TryPickCookie();
            Assert.AreEqual(WakeReaction.CatSteals, l.CookieAtToni());
        }

        [Test]
        public void Egg_ThreeRubs_Sneeze_StillAsleep()
        {
            var l = new WakeToniLogic();
            Assert.IsFalse(l.TryRub());
            Assert.IsFalse(l.TryRub());
            l.EndRub();                                 // la piuma si è allontanata: si riparte
            Assert.IsFalse(l.TryRub());
            Assert.IsFalse(l.TryRub());
            Assert.IsFalse(l.EggFound);
            Assert.IsTrue(l.TryRub());
            Assert.IsTrue(l.EggFound);
            Assert.AreEqual(WakeToniState.Sleeping, l.State);
            // poi si continua normalmente
            l.TryGiveFeatherToCat();
            Assert.IsFalse(l.TryRub());                 // la piuma ce l'ha il gatto
            l.TryPickCookie();
            Assert.AreEqual(WakeReaction.Wakes, l.CookieAtToni());
            Assert.IsTrue(l.TryCompleteWake());
            Assert.IsTrue(l.EggFound);
        }

        [Test]
        public void LockedStates_IgnoreInput()
        {
            var l = new WakeToniLogic();
            l.TryGiveFeatherToCat(); l.TryPickCookie(); l.CookieAtToni();
            Assert.AreEqual(WakeReaction.None, l.TapAlarm());
            Assert.AreEqual(WakeReaction.None, l.TapLamp());
            Assert.IsFalse(l.TryTakeFeatherFromCat());
            Assert.IsFalse(l.TryRub());
            Assert.IsFalse(l.TryPickCookie());
            Assert.IsTrue(l.TryCompleteWake());
            Assert.IsFalse(l.TryCompleteWake());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (WakeToniState target in Enum.GetValues(typeof(WakeToniState)))
            {
                var l = new WakeToniLogic();
                l.TapAlarm(); l.TapLamp(); l.TryRub(); l.TryRub(); l.TryRub();
                if (target == WakeToniState.HoldingCookie) l.TryPickCookie();
                if (target == WakeToniState.CatStealing) { l.TryPickCookie(); l.CookieAtToni(); }
                if (target >= WakeToniState.Waking) { l.TryGiveFeatherToCat(); l.TryPickCookie(); l.CookieAtToni(); }
                if (target == WakeToniState.Done) l.TryCompleteWake();
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(WakeToniState.Sleeping, l.State);
                Assert.IsFalse(l.CatBusy);
                Assert.IsFalse(l.LampOn);
                Assert.AreEqual(1, l.Snore);
                Assert.AreEqual(0, l.Rubs);
                Assert.IsFalse(l.EggFound);
                Assert.IsFalse(l.CompletionReported);
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(1, 2, 3, 4, 5, 6)] int seed)
        {
            var rnd = new Random(seed);
            var l = new WakeToniLogic();
            int completions = 0;
            for (int step = 0; step < 500; step++)
            {
                WakeToniState before = l.State;
                bool busy = l.CatBusy;
                WakeReaction r = WakeReaction.None;
                switch (rnd.Next(11))
                {
                    case 0: l.TapAlarm(); break;
                    case 1: l.TapLamp(); break;
                    case 2: l.TryGiveFeatherToCat(); break;
                    case 3: l.TryTakeFeatherFromCat(); break;
                    case 4: l.TryRub(); break;
                    case 5: l.EndRub(); break;
                    case 6: l.TryPickCookie(); break;
                    case 7: r = l.CookieAtToni(); break;
                    case 8: l.DropCookieElsewhere(); l.TryRespawnCookie(); break;
                    case 9: if (l.TryCompleteWake()) completions++; break;
                    default: if (rnd.Next(60) == 0) { l.Reset(); completions = 0; } break;
                }
                if (r == WakeReaction.Wakes) Assert.IsTrue(busy);          // si sveglia solo col gatto occupato
                if (r == WakeReaction.CatSteals) Assert.IsFalse(busy);
                if (r != WakeReaction.None) Assert.AreEqual(WakeToniState.HoldingCookie, before);
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == WakeToniState.Done, l.CompletionReported);
                Assert.That(l.Snore, Is.InRange(1, 3));
            }
        }
    }
}
