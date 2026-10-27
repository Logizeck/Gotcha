using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 253: il pesce che ha paura dell'acqua (valvola, capovolgere, soffiare).</summary>
    public class V64PlasticFishTests
    {
        const float Dt = 1f / 60f;

        static FishEvent Run(PlasticFishLogic l, float seconds, bool inverted, bool sideways, bool blowing)
        {
            FishEvent last = FishEvent.None;
            for (float t = 0f; t < seconds; t += Dt)
            {
                FishEvent e = l.Tick(Dt, inverted, sideways, blowing);
                if (e != FishEvent.None) last = e;
                if (e == FishEvent.Inflated) break;
            }
            return last;
        }

        [Test]
        public void CorrectPath_ValveInvertBlow_Inflates()
        {
            var l = new PlasticFishLogic();
            Assert.IsTrue(l.OpenValve());
            Assert.IsFalse(l.OpenValve());                     // resta aperta
            Assert.AreEqual(FishEvent.FloatedUp, Run(l, 1f, true, false, false));
            Assert.IsTrue(l.ValveExposed);
            Assert.AreEqual(FishEvent.Inflated, Run(l, 2.2f, true, false, true));
            Assert.AreEqual(FishState.Inflating, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(FishState.Done, l.State);
        }

        [Test]
        public void Upright_FullBowl_ValveIsUnderwater()
        {
            var l = new PlasticFishLogic();
            l.OpenValve();
            Assert.IsFalse(l.ValveExposed);
            Assert.AreEqual(FishEvent.BlowUnderwater, Run(l, 5f, false, false, true));
            Assert.AreEqual(0f, l.BlowProgress);
        }

        [Test]
        public void BlowingWithClosedValve_OnlyBubbles_AndDrainsOxygenFaster()
        {
            var a = new PlasticFishLogic();
            var b = new PlasticFishLogic();
            Assert.AreEqual(FishEvent.BlowBubbles, Run(a, 3f, true, false, true));
            Run(b, 3f, true, false, false);
            Assert.Less(a.Oxygen, b.Oxygen);
            Assert.AreEqual(0f, a.BlowProgress);
            Assert.AreEqual(FishState.Swimming, a.State);
        }

        [Test]
        public void BlowProgress_Accumulates_AcrossInterruptions()
        {
            var l = new PlasticFishLogic();
            l.OpenValve();
            Run(l, 1f, true, false, false);
            Run(l, 0.8f, true, false, true);
            Run(l, 0.5f, true, false, false);    // pausa per respirare
            Run(l, 1f, false, false, false);     // rigirato dritto: il pesce torna sott'acqua
            float kept = l.BlowProgress;
            Assert.Greater(kept, 0.7f);
            Run(l, 2f, false, false, true);
            Assert.AreEqual(kept, l.BlowProgress, 1e-4);   // sott'acqua non conta
            Assert.AreEqual(FishEvent.Inflated, Run(l, 2f, true, false, true));
        }

        [Test]
        public void Geometry_SurfaceAndValve()
        {
            Assert.AreEqual(0.5f, FishBowlGeometry.FractionBelow(0f), 1e-4);
            Assert.AreEqual(0f, FishBowlGeometry.SurfaceHeight(0.5f), 1e-3);
            Assert.Greater(FishBowlGeometry.SurfaceHeight(1f), 1f);      // piena: l'acqua arriva nel collo
            Assert.IsFalse(FishBowlGeometry.ValveExposed(1f, false));
            Assert.IsTrue(FishBowlGeometry.ValveExposed(PlasticFishLogic.InvertedFill, false));
            Assert.IsTrue(FishBowlGeometry.ValveExposed(PlasticFishLogic.DrownFill, true));
            for (float f = 0.05f; f < 1f; f += 0.05f)
                Assert.AreEqual(f, FishBowlGeometry.FractionBelow(FishBowlGeometry.SurfaceHeight(f)), 1e-3);
        }

        [Test]
        public void PouringTooMuch_Drowning_ThenWaterComesBack()
        {
            var l = new PlasticFishLogic();
            FishEvent e = FishEvent.None;
            for (float t = 0f; t < 4f && e != FishEvent.Drowning; t += Dt) e = l.Tick(Dt, false, true, false);
            Assert.AreEqual(FishEvent.Drowning, e);
            Assert.AreEqual(FishState.Drained, l.State);
            Assert.IsTrue(l.StuckMidBowl);
            Assert.AreEqual(1, l.Drownings);
            Run(l, PlasticFishLogic.DrainedSeconds + 0.3f, false, true, false);     // anche continuando a versare...
            Assert.AreEqual(FishState.Refilling, l.State);                          // ...l'acqua torna
            Assert.Greater(l.Water, PlasticFishLogic.DrownFill);
            Assert.AreEqual(FishEvent.Refilled, Run(l, 4f, false, false, false));
            Assert.AreEqual(FishState.Swimming, l.State);
            Assert.AreEqual(PlasticFishLogic.FullFill, l.Water, 1e-4);
        }

        [Test]
        public void Inverted_DoesNotCountAsPouring()
        {
            var l = new PlasticFishLogic();
            Run(l, 10f, true, false, false);
            Assert.AreEqual(FishState.Swimming, l.State);
            Assert.AreEqual(PlasticFishLogic.InvertedFill, l.Water, 1e-4);
            Run(l, 3f, false, false, false);
            Assert.AreEqual(PlasticFishLogic.FullFill, l.Water, 1e-4);
        }

        [Test]
        public void Pump_AddsWater_NotOxygen()
        {
            var l = new PlasticFishLogic();
            float o2 = l.Oxygen;
            for (int i = 0; i < 5; i++) Assert.IsTrue(l.Pump());
            Assert.Greater(l.Water, PlasticFishLogic.FullFill);
            Assert.LessOrEqual(l.Water, PlasticFishLogic.MaxFill);
            Assert.LessOrEqual(l.Oxygen, o2);
            Assert.Greater(l.Distress, 0.5f);
            Run(l, 4f, false, false, false);
            Assert.AreEqual(PlasticFishLogic.FullFill, l.Water, 1e-4);   // trabocca e torna piena
        }

        [Test]
        public void OxygenRunsOut_FishIsFine_NoFailure()
        {
            var l = new PlasticFishLogic();
            Assert.AreEqual(FishEvent.OxygenEmpty, Run(l, 160f, false, false, false));
            Assert.AreEqual(FishState.Swimming, l.State);
            Assert.Greater(l.Oxygen, 50f);
        }

        [Test]
        public void Squeaks_AreCounted()
        {
            var l = new PlasticFishLogic();
            Assert.AreEqual(1, l.TapFish());
            Assert.AreEqual(2, l.TapFish());
        }

        [Test]
        public void BlowEvents_AreNotSpammed()
        {
            var l = new PlasticFishLogic();
            int bubbles = 0;
            for (float t = 0f; t < 3f; t += Dt) if (l.Tick(Dt, false, false, true) == FishEvent.BlowBubbles) bubbles++;
            Assert.LessOrEqual(bubbles, 3);
            Assert.GreaterOrEqual(bubbles, 1);
        }

        [Test]
        public void InputLocked_AfterInflation()
        {
            var l = new PlasticFishLogic();
            l.OpenValve();
            Run(l, 1f, true, false, false);
            Run(l, 3f, true, false, true);
            Assert.IsFalse(l.Pump());
            Assert.AreEqual(FishEvent.None, l.Tick(Dt, true, false, true));
            l.Reset();
            Assert.AreEqual(FishState.Swimming, l.State);
            Assert.IsFalse(l.ValveOpen);
        }

        [Test]
        public void MicGate_CalibratesToAmbient()
        {
            var g = new V64AMicGate();
            // stanza rumorosa: il rumore di fondo alza la soglia e non conta come soffio
            int loud = 0;
            for (int i = 0; i < 600; i++) g.Feed(0.2f + ((i % 3) * 0.02f), Dt);
            for (int i = 0; i < 600; i++) if (g.Feed(0.2f + ((i % 3) * 0.02f), Dt)) loud++;
            Assert.AreEqual(0, loud);
            Assert.Greater(g.Threshold, 0.3f);
            Assert.IsTrue(g.Feed(0.9f, Dt));            // il soffio vero sì
            // stanza silenziosa: un soffio moderato basta
            var q = new V64AMicGate();
            for (int i = 0; i < 300; i++) q.Feed(0.01f, Dt);
            Assert.IsTrue(q.Feed(0.35f, Dt));
            Assert.IsFalse(q.Feed(0.1f, Dt));
            // un soffio lungo non si "mangia" la soglia
            float th = q.Threshold;
            for (int i = 0; i < 600; i++) q.Feed(0.6f, Dt);
            Assert.AreEqual(th, q.Threshold, 1e-4);
        }
    }
}
