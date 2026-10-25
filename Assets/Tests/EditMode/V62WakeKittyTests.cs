using System;
using NUnit.Framework;
using Gotcha.Levels.V62;

namespace Gotcha.Tests
{
    /// <summary>Livello 252: svegliare il gatto (sveglia, lampada, piuma, topo ladro, formaggio, pesce).</summary>
    public class V62WakeKittyTests
    {
        [Test]
        public void CorrectPath_Completes()
        {
            var l = new WakeKittyLogic();
            Assert.AreEqual(KittyReaction.MouseTakesCheese, l.TryGiveCheese());
            Assert.IsTrue(l.MouseBusy);
            Assert.IsTrue(l.TryPickFish());
            Assert.AreEqual(KittyReaction.Wakes, l.FishAtCat());
            Assert.AreEqual(WakeKittyState.Waking, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryCompleteWake());
            Assert.AreEqual(WakeKittyState.Done, l.State);
            Assert.AreEqual(0, l.Steals);
        }

        [Test]
        public void FishWithFreeMouse_IsStolen_ThenRespawns()
        {
            var l = new WakeKittyLogic();
            l.TryPickFish();
            Assert.AreEqual(KittyReaction.MouseSteals, l.FishAtCat());
            Assert.AreEqual(WakeKittyState.MouseStealing, l.State);
            Assert.IsFalse(l.TryPickFish());         // il pesce ce l'ha il topo
            Assert.IsTrue(l.TryRespawnFish());
            Assert.AreEqual(WakeKittyState.Sleeping, l.State);
            Assert.AreEqual(1, l.Steals);
            Assert.IsTrue(l.TryPickFish());
            Assert.AreEqual(KittyReaction.MouseSteals, l.FishAtCat());   // ancora: il topo è sempre libero
            Assert.AreEqual(2, l.Steals);
        }

        [Test]
        public void CheeseAfterATheft_StillWorks()
        {
            var l = new WakeKittyLogic();
            l.TryPickFish(); l.FishAtCat();
            Assert.AreEqual(KittyReaction.MouseTakesCheese, l.TryGiveCheese());   // anche mentre il pesce ricompare
            l.TryRespawnFish();
            l.TryPickFish();
            Assert.AreEqual(KittyReaction.Wakes, l.FishAtCat());
        }

        [Test]
        public void Alarm_Lamp_Feather_Cheese_DoNotWakeIt()
        {
            var l = new WakeKittyLogic();
            Assert.AreEqual(KittyReaction.SlapsAlarm, l.TapAlarm());
            Assert.AreEqual(2, l.Snore);
            l.TapAlarm(); l.TapAlarm();
            Assert.AreEqual(3, l.Snore);              // russa più forte, ma non oltre
            bool left = l.FacingLeft;
            Assert.AreEqual(KittyReaction.HidesFromLight, l.TapLamp());
            Assert.IsTrue(l.LampOn);
            Assert.AreNotEqual(left, l.FacingLeft);
            Assert.AreEqual(KittyReaction.None, l.TapLamp());   // spenta: niente
            Assert.IsFalse(l.LampOn);
            Assert.AreEqual(KittyReaction.Stretches, l.FeatherOnCat());
            Assert.AreEqual(KittyReaction.IgnoresCheese, l.CheeseOnCat());
            Assert.AreEqual(WakeKittyState.Sleeping, l.State);
            Assert.IsFalse(l.MouseBusy);
        }

        [Test]
        public void Cheese_OnlyOnce_AndNotWhileHoldingFish()
        {
            var l = new WakeKittyLogic();
            l.TryPickFish();
            Assert.AreEqual(KittyReaction.None, l.TryGiveCheese());
            l.DropFishElsewhere();
            Assert.AreEqual(KittyReaction.MouseTakesCheese, l.TryGiveCheese());
            Assert.AreEqual(KittyReaction.None, l.TryGiveCheese());
        }

        [Test]
        public void FishDroppedElsewhere_GoesBack()
        {
            var l = new WakeKittyLogic();
            l.TryPickFish();
            Assert.AreEqual(KittyReaction.FishBack, l.DropFishElsewhere());
            Assert.AreEqual(WakeKittyState.Sleeping, l.State);
            Assert.AreEqual(KittyReaction.None, l.DropFishElsewhere());
            Assert.AreEqual(KittyReaction.None, l.FishAtCat());   // senza pesce in mano
        }

        [Test]
        public void LockedStates_IgnoreInput()
        {
            var l = new WakeKittyLogic();
            l.TryGiveCheese(); l.TryPickFish(); l.FishAtCat();
            Assert.AreEqual(KittyReaction.None, l.TapAlarm());
            Assert.AreEqual(KittyReaction.None, l.TapLamp());
            Assert.AreEqual(KittyReaction.None, l.FeatherOnCat());
            Assert.AreEqual(KittyReaction.None, l.CheeseOnCat());
            Assert.IsFalse(l.TryPickFish());
            Assert.IsFalse(l.TryRespawnFish());
            Assert.IsTrue(l.TryCompleteWake());
            Assert.IsFalse(l.TryCompleteWake());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (WakeKittyState target in Enum.GetValues(typeof(WakeKittyState)))
            {
                var l = new WakeKittyLogic();
                l.TapAlarm(); l.TapLamp();
                switch (target)
                {
                    case WakeKittyState.HoldingFish: l.TryPickFish(); break;
                    case WakeKittyState.MouseStealing: l.TryPickFish(); l.FishAtCat(); break;
                    case WakeKittyState.Waking: l.TryGiveCheese(); l.TryPickFish(); l.FishAtCat(); break;
                    case WakeKittyState.Done: l.TryGiveCheese(); l.TryPickFish(); l.FishAtCat(); l.TryCompleteWake(); break;
                }
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(WakeKittyState.Sleeping, l.State);
                Assert.IsFalse(l.MouseBusy);
                Assert.IsFalse(l.LampOn);
                Assert.AreEqual(1, l.Snore);
                Assert.AreEqual(0, l.Steals);
                Assert.IsFalse(l.CompletionReported);
                Assert.IsTrue(l.TryPickFish());
            }
        }

        [Test]
        public void RandomSequences_NeverBlock([Values(1, 2, 3, 4, 5, 6)] int seed, [Values(7, 60, 250, 600)] int steps)
        {
            var rnd = new Random(seed);
            var l = new WakeKittyLogic();
            int completions = 0;
            for (int step = 0; step < steps; step++)
            {
                switch (rnd.Next(11))
                {
                    case 0: l.TapAlarm(); break;
                    case 1: l.TapLamp(); break;
                    case 2: l.FeatherOnCat(); break;
                    case 3: l.CheeseOnCat(); break;
                    case 4: if (rnd.Next(4) == 0) l.TryGiveCheese(); break;
                    case 5: l.TryPickFish(); break;
                    case 6: l.FishAtCat(); break;
                    case 7: l.DropFishElsewhere(); break;
                    case 8: l.TryRespawnFish(); break;
                    case 9: if (l.TryCompleteWake()) completions++; break;
                    default: if (rnd.Next(80) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == WakeKittyState.Done, l.CompletionReported);
                Assert.That(l.Snore, Is.InRange(1, 3));
                if (l.State == WakeKittyState.Waking || l.State == WakeKittyState.Done) Assert.IsTrue(l.MouseBusy);
            }
            // nessun blocco: da qualunque punto la soluzione porta ancora alla vittoria
            if (l.State == WakeKittyState.Done) return;
            if (l.State != WakeKittyState.Waking)
            {
                l.DropFishElsewhere();
                l.TryRespawnFish();
                l.TryGiveCheese();
                Assert.IsTrue(l.MouseBusy);
                Assert.IsTrue(l.TryPickFish());
                Assert.AreEqual(KittyReaction.Wakes, l.FishAtCat());
            }
            Assert.IsTrue(l.TryCompleteWake());
        }
    }
}
