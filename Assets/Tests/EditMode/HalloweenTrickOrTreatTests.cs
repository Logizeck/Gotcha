using System;
using NUnit.Framework;
using Gotcha.Levels.Halloween;

namespace Gotcha.Tests
{
    /// <summary>Livello 250: dolcetto o scherzetto (travestimento a cinque e Easter egg di Toni).</summary>
    public class HalloweenTrickOrTreatTests
    {
        static void BuildDisguise(TrickOrTreatLogic l, bool pumpkinFirst = false)
        {
            Assert.IsTrue(l.TryZombieToDoor());
            if (pumpkinFirst) { Assert.IsTrue(l.TryPumpkinHead()); Assert.IsTrue(l.TryHideToni()); }
            else { Assert.IsTrue(l.TryHideToni()); Assert.IsTrue(l.TryPumpkinHead()); }
            Assert.IsTrue(l.TryGhostWrap());
            Assert.IsTrue(l.TryWitchSpell());
        }

        static void PrepareEgg(TrickOrTreatLogic l)
        {
            Assert.IsTrue(l.TryDetachArm());
            Assert.IsTrue(l.TryGiveArmToToni());
            Assert.IsTrue(l.TrySetToniAtDoor(true));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NormalPath_EitherOrderForToniAndPumpkin(bool pumpkinFirst)
        {
            var l = new TrickOrTreatLogic();
            BuildDisguise(l, pumpkinFirst);
            Assert.IsTrue(l.DisguiseComplete);
            Assert.AreEqual(TrickRingResult.Scared, l.TryRingBell());
            Assert.AreEqual(TrickState.NormalEnding, l.State);
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.IsFalse(l.EndedWithEgg);
        }

        [Test]
        public void Dependencies_AreEnforced()
        {
            var l = new TrickOrTreatLogic();
            Assert.IsFalse(l.TryHideToni());       // serve il corpo alla porta
            Assert.IsFalse(l.TryPumpkinHead());
            Assert.IsFalse(l.TryGhostWrap());
            Assert.IsFalse(l.TryWitchSpell());
            l.TryZombieToDoor();
            Assert.IsFalse(l.TryGhostWrap());      // mancano Toni e la zucca
            l.TryHideToni();
            Assert.IsFalse(l.TryGhostWrap());
            Assert.IsFalse(l.TryWitchSpell());
            l.TryPumpkinHead();
            Assert.IsFalse(l.TryWitchSpell());     // prima il fantasma
            Assert.IsTrue(l.TryGhostWrap());
            Assert.IsTrue(l.TryWitchSpell());
        }

        [Test]
        public void Bell_OneChocolate_ThenAlreadySeen()
        {
            var l = new TrickOrTreatLogic();
            Assert.AreEqual(TrickRingResult.GaveChocolate, l.TryRingBell());
            Assert.AreEqual(TrickRingResult.Ignored, l.TryRingBell());   // la signora è ancora alla porta
            Assert.IsTrue(l.TryLadyLeaves());
            Assert.AreEqual(TrickRingResult.AlreadySeen, l.TryRingBell());
            Assert.IsFalse(l.ChangedSinceLastRing);
            l.TryLadyLeaves();
            l.TryZombieToDoor();
            Assert.IsTrue(l.ChangedSinceLastRing);
            Assert.AreEqual(TrickRingResult.AlreadySeen, l.TryRingBell());   // travestimento a metà: riconosciuti
            Assert.AreEqual(1, l.Chocolates);
        }

        [Test]
        public void EggPath_Completes()
        {
            var l = new TrickOrTreatLogic();
            PrepareEgg(l);
            Assert.AreEqual(TrickRingResult.Egg, l.TryRingWithArm());
            Assert.AreEqual(TrickState.EggEnding, l.State);
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.IsTrue(l.EndedWithEgg);
            Assert.IsFalse(l.TryCompleteEnding());
        }

        [Test]
        public void EggPath_WorksWithZombieAndPumpkinAtDoor()
        {
            var l = new TrickOrTreatLogic();
            l.TryZombieToDoor();
            l.TryPumpkinHead();
            Assert.IsTrue(l.EggAvailable);
            PrepareEgg(l);
            Assert.AreEqual(TrickRingResult.Egg, l.TryRingWithArm());
        }

        [Test]
        public void EggRequirements_Missing_GiveNormalRing()
        {
            var l = new TrickOrTreatLogic();
            l.TryDetachArm();
            Assert.AreEqual(TrickRingResult.GaveChocolate, l.TryRingWithArm());   // Toni non ha il braccio
            l.TryLadyLeaves();
            l.TryGiveArmToToni();
            Assert.AreEqual(TrickRingResult.AlreadySeen, l.TryRingWithArm());     // Toni non è alla porta
            l.TryLadyLeaves();
            l.TrySetToniAtDoor(true);
            Assert.AreEqual(TrickRingResult.Egg, l.TryRingWithArm());
        }

        [Test]
        public void ArmAttached_CannotRingWithIt()
        {
            var l = new TrickOrTreatLogic();
            Assert.AreEqual(TrickRingResult.Ignored, l.TryRingWithArm());
        }

        [Test]
        public void Egg_NotAvailable_OnceDisguiseConflicts()
        {
            var l = new TrickOrTreatLogic();
            l.TryZombieToDoor();
            l.TryHideToni();
            Assert.IsFalse(l.EggAvailable);
            Assert.IsFalse(l.CanGrabArm);
            Assert.IsFalse(l.TryDetachArm());
            Assert.IsFalse(l.TrySetToniAtDoor(true));
        }

        [Test]
        public void ArmDetached_BlocksDisguise_UntilReattached()
        {
            var l = new TrickOrTreatLogic();
            l.TryZombieToDoor();
            l.TryDetachArm();
            Assert.IsFalse(l.TryHideToni());
            l.TryGiveArmToToni();
            Assert.IsFalse(l.TryHideToni());
            Assert.IsTrue(l.TryAttachArm());   // dal muso di Toni di nuovo allo zombie
            Assert.AreEqual(ArmPlace.OnZombie, l.Arm);
            Assert.IsTrue(l.TryHideToni());
            l.TryPumpkinHead();
            Assert.IsTrue(l.TryGhostWrap());
            Assert.IsTrue(l.TryWitchSpell());
            Assert.AreEqual(TrickRingResult.Scared, l.TryRingBell());
        }

        [Test]
        public void NormalAndEgg_AreMutuallyExclusive()
        {
            var l = new TrickOrTreatLogic();
            BuildDisguise(l);
            Assert.IsFalse(l.EggAvailable);
            Assert.AreEqual(TrickRingResult.Scared, l.TryRingBell());
            Assert.AreEqual(TrickRingResult.Ignored, l.TryRingWithArm());
            Assert.IsTrue(l.TryCompleteEnding());
            Assert.IsFalse(l.EndedWithEgg);

            var e = new TrickOrTreatLogic();
            PrepareEgg(e);
            Assert.AreEqual(TrickRingResult.Egg, e.TryRingWithArm());
            Assert.AreEqual(TrickRingResult.Ignored, e.TryRingBell());
            Assert.IsFalse(e.TryZombieToDoor());
            Assert.IsTrue(e.TryCompleteEnding());
            Assert.IsTrue(e.EndedWithEgg);
            Assert.IsFalse(e.TryCompleteEnding());
        }

        [Test]
        public void NoDuplication_StepsOnlyOnce()
        {
            var l = new TrickOrTreatLogic();
            Assert.IsTrue(l.TryZombieToDoor());
            Assert.IsFalse(l.TryZombieToDoor());
            Assert.IsTrue(l.TryPumpkinHead());
            Assert.IsFalse(l.TryPumpkinHead());
            Assert.IsTrue(l.TryDetachArm());
            Assert.IsFalse(l.TryDetachArm());
            Assert.IsTrue(l.TryGiveArmToToni());
            Assert.IsFalse(l.TryGiveArmToToni());
        }

        [Test]
        public void PrematureCompletion_Impossible()
        {
            var l = new TrickOrTreatLogic();
            Assert.IsFalse(l.TryCompleteEnding());
            l.TryRingBell();
            Assert.IsFalse(l.TryCompleteEnding());
            l.TryLadyLeaves();
            l.TryZombieToDoor(); l.TryHideToni(); l.TryPumpkinHead(); l.TryGhostWrap();
            Assert.AreEqual(TrickRingResult.AlreadySeen, l.TryRingBell());
            Assert.IsFalse(l.TryCompleteEnding());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (TrickState s in Enum.GetValues(typeof(TrickState)))
            {
                var l = new TrickOrTreatLogic();
                switch (s)
                {
                    case TrickState.LadyAtDoor: l.TryZombieToDoor(); l.TryRingBell(); break;
                    case TrickState.NormalEnding: BuildDisguise(l); l.TryRingBell(); break;
                    case TrickState.EggEnding: PrepareEgg(l); l.TryRingWithArm(); break;
                    case TrickState.Done: PrepareEgg(l); l.TryRingWithArm(); l.TryCompleteEnding(); break;
                }
                Assert.AreEqual(s, l.State);
                l.Reset();
                Assert.AreEqual(TrickState.Building, l.State);
                Assert.AreEqual(0, l.DisguiseSteps);
                Assert.AreEqual(ArmPlace.OnZombie, l.Arm);
                Assert.AreEqual(0, l.Chocolates);
                Assert.IsFalse(l.ToniAtDoor || l.CompletionReported || l.EndedWithEgg);
                Assert.IsTrue(l.EggAvailable);
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(51, 52, 53, 54, 55, 56, 57, 58, 59, 60)] int seed)
        {
            var rnd = new Random(seed);
            var l = new TrickOrTreatLogic();
            int completions = 0;
            for (int step = 0; step < 1000; step++)
            {
                bool eggWasAvailable = l.EggAvailable, complete = l.DisguiseComplete;
                switch (rnd.Next(14))
                {
                    case 0: l.TryZombieToDoor(); break;
                    case 1: l.TryHideToni(); break;
                    case 2: l.TryPumpkinHead(); break;
                    case 3: l.TryGhostWrap(); break;
                    case 4: l.TryWitchSpell(); break;
                    case 5: l.TryDetachArm(); break;
                    case 6: l.TryAttachArm(); break;
                    case 7: l.TryGiveArmToToni(); break;
                    case 8: l.TrySetToniAtDoor(rnd.Next(2) == 0); break;
                    case 9:
                        TrickRingResult r = l.TryRingBell();
                        if (r == TrickRingResult.Scared) Assert.IsTrue(complete);
                        Assert.AreNotEqual(TrickRingResult.Egg, r);
                        break;
                    case 10:
                        if (l.TryRingWithArm() == TrickRingResult.Egg) Assert.IsTrue(eggWasAvailable);
                        break;
                    case 11: l.TryLadyLeaves(); break;
                    case 12: if (l.TryCompleteEnding()) completions++; break;
                    default: if (rnd.Next(100) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.LessOrEqual(l.Chocolates, 1);
                Assert.AreEqual(l.State == TrickState.Done, l.CompletionReported);
                if (l.ToniInside) { Assert.AreEqual(ArmPlace.OnZombie, l.Arm); Assert.IsFalse(l.ToniAtDoor); }
                if (l.GhostWrapped) Assert.IsTrue(l.ToniInside && l.PumpkinHead && l.ZombieAtDoor);
                if (l.WitchDone) Assert.IsTrue(l.GhostWrapped);
                if (l.State == TrickState.EggEnding) Assert.IsFalse(l.DisguiseComplete);
                if (l.State == TrickState.NormalEnding) Assert.IsTrue(l.DisguiseComplete);
                if (l.State == TrickState.Done) Assert.AreEqual(!l.DisguiseComplete, l.EndedWithEgg);
            }
        }
    }
}
