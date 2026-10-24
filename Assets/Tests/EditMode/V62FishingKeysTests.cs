using System;
using NUnit.Framework;
using Gotcha.Levels.V62;

namespace Gotcha.Tests
{
    /// <summary>Livello 251: la chiave nello scarico (calamita sull'amo, stivale, piranha).</summary>
    public class V62FishingKeysTests
    {
        [Test]
        public void CorrectPath_Completes()
        {
            var l = new FishingKeysLogic();
            Assert.IsTrue(l.TryAttach(HookBait.Magnet));
            Assert.AreEqual(CastResult.Key, l.TryCast());
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryFinishCast());
            Assert.AreEqual(FishingKeysState.KeyOut, l.State);
            Assert.IsFalse(l.KeyInDrain);
            Assert.IsTrue(l.TryPickKey());
            Assert.IsTrue(l.TryDropKey(true));
            Assert.AreEqual(FishingKeysState.Opening, l.State);
            Assert.IsTrue(l.TryCompleteOpening());
            Assert.AreEqual(FishingKeysState.Done, l.State);
            Assert.IsFalse(l.EggFound);
        }

        [Test]
        public void EmptyHook_CatchesBoot_AndRetries()
        {
            var l = new FishingKeysLogic();
            Assert.AreEqual(CastResult.Boot, l.TryCast());
            Assert.IsTrue(l.TryFinishCast());
            Assert.AreEqual(FishingKeysState.Idle, l.State);
            Assert.AreEqual(1, l.Boots);
            Assert.IsTrue(l.KeyInDrain);
            Assert.AreEqual(CastResult.Boot, l.TryCast());   // nessun fallimento: si riprova
            l.TryFinishCast();
            Assert.AreEqual(2, l.Boots);
        }

        [Test]
        public void Finger_DoesNotFit()
        {
            var l = new FishingKeysLogic();
            Assert.IsTrue(l.TryFinger());
            Assert.IsTrue(l.TryFinger());
            Assert.AreEqual(2, l.FingerTries);
            Assert.AreEqual(FishingKeysState.Idle, l.State);
            Assert.IsFalse(l.TryPickKey());
        }

        [Test]
        public void WrongKeyDrop_ReturnsToHook()
        {
            var l = new FishingKeysLogic();
            l.TryAttach(HookBait.Magnet); l.TryCast(); l.TryFinishCast();
            l.TryPickKey();
            Assert.IsFalse(l.TryDropKey(false));
            Assert.AreEqual(FishingKeysState.KeyOut, l.State);
            Assert.IsTrue(l.TryPickKey());
        }

        [Test]
        public void Hook_HoldsOneThing_AndCanBeEmptied()
        {
            var l = new FishingKeysLogic();
            Assert.IsTrue(l.TryAttach(HookBait.Fish));
            Assert.IsFalse(l.TryAttach(HookBait.Magnet));
            Assert.AreEqual(HookBait.Fish, l.TryDetach());
            Assert.AreEqual(HookBait.None, l.Bait);
            Assert.AreEqual(HookBait.None, l.TryDetach());
            Assert.IsTrue(l.TryAttach(HookBait.Magnet));
            Assert.IsFalse(l.TryAttach(HookBait.None));
        }

        [Test]
        public void Egg_FishOnHook_Piranha_ThenContinues()
        {
            var l = new FishingKeysLogic();
            l.TryAttach(HookBait.Fish);
            Assert.AreEqual(CastResult.Piranha, l.TryCast());
            Assert.IsFalse(l.EggFound);
            Assert.IsTrue(l.TryFinishCast());
            Assert.IsTrue(l.EggFound);
            Assert.AreEqual(HookBait.None, l.Bait);           // il pesce se l'è mangiato il piranha
            Assert.AreEqual(FishingKeysState.Idle, l.State);
            Assert.IsTrue(l.TryAttach(HookBait.Magnet));      // si continua normalmente
            Assert.AreEqual(CastResult.Key, l.TryCast());
            l.TryFinishCast(); l.TryPickKey(); l.TryDropKey(true);
            Assert.IsTrue(l.TryCompleteOpening());
            Assert.IsTrue(l.EggFound);
        }

        [Test]
        public void LockedStates_IgnoreInput()
        {
            var l = new FishingKeysLogic();
            l.TryAttach(HookBait.Magnet);
            l.TryCast();
            Assert.IsFalse(l.TryFinger());
            Assert.AreEqual(HookBait.None, l.TryDetach());
            Assert.AreEqual(CastResult.Nothing, l.TryCast());
            Assert.IsFalse(l.TryPickKey());
            Assert.IsFalse(l.TryCompleteOpening());
        }

        [Test]
        public void Completion_ReportedOnce()
        {
            var l = new FishingKeysLogic();
            l.TryAttach(HookBait.Magnet); l.TryCast(); l.TryFinishCast(); l.TryPickKey(); l.TryDropKey(true);
            Assert.IsTrue(l.TryCompleteOpening());
            Assert.IsFalse(l.TryCompleteOpening());
            Assert.IsFalse(l.TryPickKey());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (FishingKeysState target in Enum.GetValues(typeof(FishingKeysState)))
            {
                var l = new FishingKeysLogic();
                l.TryAttach(HookBait.Fish); l.TryCast(); l.TryFinishCast();   // con l'Easter egg
                if (target >= FishingKeysState.Casting) { l.TryAttach(HookBait.Magnet); l.TryCast(); }
                if (target >= FishingKeysState.KeyOut) l.TryFinishCast();
                if (target >= FishingKeysState.HoldingKey) l.TryPickKey();
                if (target >= FishingKeysState.Opening) l.TryDropKey(true);
                if (target >= FishingKeysState.Done) l.TryCompleteOpening();
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(FishingKeysState.Idle, l.State);
                Assert.AreEqual(HookBait.None, l.Bait);
                Assert.IsFalse(l.EggFound);
                Assert.IsFalse(l.CompletionReported);
                Assert.AreEqual(0, l.Boots);
                Assert.IsTrue(l.TryAttach(HookBait.Magnet));
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(1, 2, 3, 4, 5, 6)] int seed)
        {
            var rnd = new Random(seed);
            var l = new FishingKeysLogic();
            int completions = 0;
            for (int step = 0; step < 500; step++)
            {
                switch (rnd.Next(9))
                {
                    case 0: l.TryAttach((HookBait)rnd.Next(3)); break;
                    case 1: l.TryDetach(); break;
                    case 2: l.TryCast(); break;
                    case 3: l.TryFinishCast(); break;
                    case 4: l.TryPickKey(); break;
                    case 5: l.TryDropKey(rnd.Next(3) == 0); break;
                    case 6: if (l.TryCompleteOpening()) completions++; break;
                    case 7: l.TryFinger(); break;
                    default: if (rnd.Next(60) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == FishingKeysState.Done, l.CompletionReported);
                if (l.State == FishingKeysState.KeyOut || l.State == FishingKeysState.HoldingKey) Assert.AreEqual(HookBait.Magnet, l.Bait);
            }
        }
    }
}
