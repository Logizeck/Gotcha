using System;
using NUnit.Framework;
using Gotcha.Levels.V62;

namespace Gotcha.Tests
{
    /// <summary>Livello 255: l'ombrello rovesciato (aprire, girare, riempire, versare).</summary>
    public class V62UmbrellaTests
    {
        static void Fill(UmbrellaLogic l)
        {
            for (int i = 0; i < 50; i++) l.Rain(0.1f);
        }

        [Test]
        public void CorrectPath_Completes()
        {
            var l = new UmbrellaLogic();
            Assert.IsTrue(l.TryOpen());
            Assert.IsTrue(l.TryFlip(false, out PourResult p));
            Assert.AreEqual(PourResult.Nothing, p);
            Assert.AreEqual(UmbrellaPose.Inverted, l.Pose);
            Assert.IsTrue(l.Rain(2f));
            Assert.AreEqual(0.5f, l.Water, 1e-5f);
            Assert.IsFalse(l.Full);
            Assert.IsTrue(l.Rain(2f));
            Assert.IsTrue(l.Full);
            Assert.IsFalse(l.Rain(1f));                     // già pieno
            Assert.AreEqual(PourResult.Bloom, l.TryPour(true));
            Assert.AreEqual(UmbrellaState.Blooming, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryCompleteBloom());
            Assert.AreEqual(UmbrellaState.Done, l.State);
                    }

        [Test]
        public void OpenUpright_InRain_StaysDry()
        {
            var l = new UmbrellaLogic();
            Assert.IsFalse(l.Rain(1f));                     // chiuso
            l.TryOpen();
            Assert.IsFalse(l.Rain(10f));                    // dritto: ripara e basta
            Assert.AreEqual(0f, l.Water);
            Assert.AreEqual(PourResult.Nothing, l.TryPour(true));
            Assert.IsFalse(l.TryOpen());
        }

        [Test]
        public void ClosedUmbrella_CannotFlip()
        {
            var l = new UmbrellaLogic();
            Assert.IsFalse(l.TryFlip(true, out PourResult p));
            Assert.AreEqual(PourResult.Nothing, p);
            Assert.AreEqual(UmbrellaPose.Closed, l.Pose);
        }

        [Test]
        public void PouringAway_WastesWater_CanRefill()
        {
            var l = new UmbrellaLogic();
            l.TryOpen(); l.TryFlip(false, out _);
            Fill(l);
            Assert.AreEqual(PourResult.Wasted, l.TryPour(false));
            Assert.AreEqual(0f, l.Water);
            Assert.AreEqual(UmbrellaState.Playing, l.State);
            Assert.AreEqual(1, l.Wasted);
            Fill(l);
            Assert.AreEqual(PourResult.Bloom, l.TryPour(true));
        }

        [Test]
        public void HalfFull_OverPlant_NotEnough()
        {
            var l = new UmbrellaLogic();
            l.TryOpen(); l.TryFlip(false, out _);
            l.Rain(1f);
            Assert.AreEqual(PourResult.NotEnough, l.TryPour(true));
            Assert.AreEqual(UmbrellaState.Playing, l.State);
            Assert.AreEqual(0f, l.Water);
        }

        [Test]
        public void FlippingBackFull_PoursWhereItIs()
        {
            var l = new UmbrellaLogic();
            l.TryOpen(); l.TryFlip(false, out _);
            Fill(l);
            Assert.IsTrue(l.TryFlip(false, out PourResult p));
            Assert.AreEqual(PourResult.Wasted, p);
            Assert.AreEqual(UmbrellaPose.Open, l.Pose);
            l.TryFlip(false, out _);
            Fill(l);
            Assert.IsTrue(l.TryFlip(true, out p));
            Assert.AreEqual(PourResult.Bloom, p);
            Assert.AreEqual(UmbrellaState.Blooming, l.State);
        }

        [Test]
        public void LockedStates_IgnoreInput()
        {
            var l = new UmbrellaLogic();
            l.TryOpen(); l.TryFlip(false, out _); Fill(l); l.TryPour(true);
            Assert.IsFalse(l.TryFlip(true, out _));
            Assert.IsFalse(l.Rain(1f));
            Assert.AreEqual(PourResult.Nothing, l.TryPour(true));
            Assert.IsTrue(l.TryCompleteBloom());
            Assert.IsFalse(l.TryCompleteBloom());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (UmbrellaState target in Enum.GetValues(typeof(UmbrellaState)))
            {
                var l = new UmbrellaLogic();
                l.TryOpen(); l.TryFlip(false, out _); l.Rain(1f); l.TryPour(false);
                if (target >= UmbrellaState.Blooming) { Fill(l); l.TryPour(true); }
                if (target >= UmbrellaState.Done) l.TryCompleteBloom();
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(UmbrellaState.Playing, l.State);
                Assert.AreEqual(UmbrellaPose.Closed, l.Pose);
                Assert.AreEqual(0f, l.Water);
                Assert.AreEqual(0, l.Wasted);
                                Assert.IsFalse(l.CompletionReported);
                Assert.IsTrue(l.TryOpen());
            }
        }

        [Test]
        public void RandomSequences_KeepInvariants([Values(1, 2, 3, 4, 5, 6)] int seed)
        {
            var rnd = new Random(seed);
            var l = new UmbrellaLogic();
            int completions = 0;
            for (int step = 0; step < 600; step++)
            {
                bool wasFull = l.Full;
                PourResult r = PourResult.Nothing;
                switch (rnd.Next(8))
                {
                    case 0: l.TryOpen(); break;
                    case 1: l.TryFlip(rnd.Next(2) == 0, out r); break;
                    case 2: l.Rain((float)rnd.NextDouble()); break;
                    case 3: r = l.TryPour(rnd.Next(2) == 0); break;
                    case 5: if (l.TryCompleteBloom()) completions++; break;
                    case 6: if (rnd.Next(40) == 0) { l.Reset(); completions = 0; } break;
                    default: l.Rain(0.5f); break;
                }
                if (r == PourResult.Bloom) Assert.IsTrue(wasFull);
                Assert.That(l.Water, Is.InRange(0f, 1f));
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == UmbrellaState.Done, l.CompletionReported);
                if (l.Pose != UmbrellaPose.Inverted && l.State == UmbrellaState.Playing) Assert.AreEqual(0f, l.Water);
            }
        }
    }
}
