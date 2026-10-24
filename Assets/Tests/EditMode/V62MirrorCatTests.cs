using System;
using NUnit.Framework;
using Gotcha.Levels.V62;

namespace Gotcha.Tests
{
    /// <summary>Livello 254: il gatto che spinge il vaso (tocchi, pesce, campanello, specchio).</summary>
    public class V62MirrorCatTests
    {
        [Test]
        public void CorrectPath_Completes()
        {
            var l = new MirrorCatLogic();
            l.Tick(5f);
            Assert.AreEqual(5, l.Pushes);
            Assert.IsTrue(l.TryDropMirror(MirrorTarget.InFrontOfCat));
            Assert.AreEqual(MirrorCatState.Scared, l.State);
            Assert.AreEqual(0, l.Tick(30f));                 // spaventato: non spinge più
            Assert.IsTrue(l.TryCompleteSave());
            Assert.AreEqual(MirrorCatState.Done, l.State);
            Assert.IsFalse(l.EggFound);
        }

        [Test]
        public void DoingNothing_VaseFallsAfterAbout25Seconds()
        {
            var l = new MirrorCatLogic();
            Assert.AreEqual(24, l.Tick(24.5f));
            Assert.AreEqual(MirrorCatState.Pushing, l.State);
            Assert.AreEqual(1, l.Tick(1f));
            Assert.AreEqual(MirrorCatState.Fallen, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsFalse(l.TryDropMirror(MirrorTarget.InFrontOfCat));   // troppo tardi
            Assert.IsFalse(l.TryCompleteSave());
        }

        [Test]
        public void TouchingCat_PausesThenFaster()
        {
            var l = new MirrorCatLogic();
            Assert.IsTrue(l.TryTouchCat());
            Assert.AreEqual(MirrorCatState.Paused, l.State);
            Assert.AreEqual(0, l.Tick(MirrorCatLogic.TouchPause - 0.1f));
            Assert.Less(l.Interval, MirrorCatLogic.BaseInterval);
            l.Tick(0.2f);
            Assert.AreEqual(MirrorCatState.Pushing, l.State);
            for (int i = 0; i < 20; i++) l.TryTouchCat();
            Assert.AreEqual(MirrorCatLogic.MinInterval, l.Interval, 1e-5f);
        }

        [Test]
        public void Fish_PausesButDoesNotSave()
        {
            var l = new MirrorCatLogic();
            Assert.IsTrue(l.TryFeedFish());
            Assert.AreEqual(0, l.Tick(MirrorCatLogic.EatPause - 0.05f));
            Assert.AreEqual(MirrorCatLogic.BaseInterval, l.Interval, 1e-5f);
            l.Tick(60f);
            Assert.AreEqual(MirrorCatState.Fallen, l.State);
            Assert.AreEqual(1, l.FishEaten);
        }

        [Test]
        public void Bell_And_Vase_DoNothing()
        {
            var l = new MirrorCatLogic();
            l.Tick(3f);
            Assert.IsTrue(l.TryRingBell());
            Assert.IsTrue(l.TryDragVase());
            Assert.AreEqual(MirrorCatState.Pushing, l.State);
            Assert.AreEqual(1, l.Tick(1f));
            Assert.AreEqual(4, l.Pushes);
        }

        [Test]
        public void MirrorNowhere_NoEffect()
        {
            var l = new MirrorCatLogic();
            Assert.IsFalse(l.TryDropMirror(MirrorTarget.Nowhere));
            Assert.AreEqual(MirrorCatState.Pushing, l.State);
            Assert.IsFalse(l.EggFound);
        }

        [Test]
        public void Egg_MirrorInFrontOfToni_ThenContinues()
        {
            var l = new MirrorCatLogic();
            Assert.IsFalse(l.TryDropMirror(MirrorTarget.InFrontOfToni));
            Assert.IsTrue(l.EggFound);
            Assert.AreEqual(MirrorCatState.Pushing, l.State);
            Assert.IsTrue(l.TryDropMirror(MirrorTarget.InFrontOfCat));
            Assert.IsTrue(l.TryCompleteSave());
            Assert.IsTrue(l.EggFound);
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (MirrorCatState target in Enum.GetValues(typeof(MirrorCatState)))
            {
                var l = new MirrorCatLogic();
                l.TryDropMirror(MirrorTarget.InFrontOfToni);
                l.TryTouchCat(); l.Tick(5f);
                switch (target)
                {
                    case MirrorCatState.Paused: l.TryFeedFish(); break;
                    case MirrorCatState.Scared: l.TryDropMirror(MirrorTarget.InFrontOfCat); break;
                    case MirrorCatState.Fallen: l.Tick(100f); break;
                    case MirrorCatState.Done: l.TryDropMirror(MirrorTarget.InFrontOfCat); l.TryCompleteSave(); break;
                }
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(MirrorCatState.Pushing, l.State);
                Assert.AreEqual(0, l.Pushes);
                Assert.AreEqual(MirrorCatLogic.BaseInterval, l.Interval, 1e-5f);
                Assert.IsFalse(l.EggFound);
                Assert.IsFalse(l.CompletionReported);
                Assert.AreEqual(1, l.Tick(1.01f));
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(1, 2, 3, 4, 5, 6)] int seed)
        {
            var rnd = new Random(seed);
            var l = new MirrorCatLogic();
            int completions = 0;
            for (int step = 0; step < 600; step++)
            {
                switch (rnd.Next(8))
                {
                    case 0: l.TryTouchCat(); break;
                    case 1: l.TryFeedFish(); break;
                    case 2: l.TryRingBell(); break;
                    case 3: l.TryDropMirror((MirrorTarget)rnd.Next(3)); break;
                    case 4: if (l.TryCompleteSave()) completions++; break;
                    case 5: if (rnd.Next(40) == 0) { l.Reset(); completions = 0; } break;
                    default: l.Tick((float)rnd.NextDouble() * 1.5f); break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.That(l.Pushes, Is.InRange(0, MirrorCatLogic.PushesToFall));
                Assert.AreEqual(l.Pushes >= MirrorCatLogic.PushesToFall, l.State == MirrorCatState.Fallen);
                Assert.AreEqual(l.State == MirrorCatState.Done, l.CompletionReported);
                Assert.That(l.Interval, Is.InRange(MirrorCatLogic.MinInterval, MirrorCatLogic.BaseInterval));
            }
        }
    }
}
