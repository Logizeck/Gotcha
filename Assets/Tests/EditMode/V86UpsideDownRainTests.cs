using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 101 della roadmap: The Upside-Down Rain (l'ombrello bucato non serve; capovolgere il telefono rimanda su la pioggia).</summary>
    public class V86UpsideDownRainTests
    {
        const float Upright = -1f, Upside = 1f, Frame = 1f / 60f;
        const float Dry = UpsideDownRainLogic.DryTime;

        /// <summary>Lettura costante dell'accelerometro per <paramref name="seconds"/>, a fotogrammi di 1/60 s; ultimo evento non nullo.</summary>
        static UpsideDownRainEvent Feed(UpsideDownRainLogic l, float y, float seconds, float x = 0f)
        {
            UpsideDownRainEvent last = UpsideDownRainEvent.None;
            float t = 0f;
            while (t < seconds - 1e-4f)
            {
                float dt = System.Math.Min(Frame, seconds - t);
                UpsideDownRainEvent e = l.Tick(dt, x, y);
                if (e != UpsideDownRainEvent.None) last = e;
                t += dt;
            }
            return last;
        }

        static int Count(UpsideDownRainLogic l, float y, float seconds, UpsideDownRainEvent what)
        {
            int n = 0;
            for (float t = 0f; t < seconds - 1e-4f; t += Frame) if (l.Tick(Frame, 0f, y) == what) n++;
            return n;
        }

        /// <summary>Secondi fino a un certo evento (al massimo maxSeconds), o -1 se non arriva.</summary>
        static float Until(UpsideDownRainLogic l, float y, UpsideDownRainEvent what, float maxSeconds)
        {
            for (float t = 0f; t < maxSeconds; t += Frame) if (l.Tick(Frame, 0f, y) == what) return t + Frame;
            return -1f;
        }

        static UpsideDownRainLogic Ready()
        {
            var l = new UpsideDownRainLogic();
            Feed(l, Upright, 0.2f);
            return l;
        }

        static void AssertNotWon(UpsideDownRainLogic l)
        {
            Assert.AreNotEqual(UpsideDownRainState.Won, l.State);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void StartsRainingSlightlyWet()
        {
            var l = new UpsideDownRainLogic();
            Assert.AreEqual(UpsideDownRainState.Raining, l.State);
            Assert.AreEqual(UpsideDownRainLogic.StartWet, l.Wetness, 1e-5f);
            Assert.IsFalse(l.Armed);
            Assert.IsFalse(l.DripInFlight);
            AssertNotWon(l);
        }

        [Test]
        public void DropsGetThroughTheHolesAndWetHim()
        {
            var l = Ready();
            float t = Until(l, Upright, UpsideDownRainEvent.Dripped, 2f);
            Assert.Greater(t, 0f);
            Assert.IsTrue(l.DripInFlight);
            float w = l.Wetness;
            float hit = Until(l, Upright, UpsideDownRainEvent.Hit, 1f);
            Assert.AreEqual(UpsideDownRainLogic.DripFall, hit, 2 * Frame);
            Assert.Greater(l.Wetness, w + UpsideDownRainLogic.HitWet * 0.8f);
            Assert.AreEqual(1, l.Hits);
            Assert.IsFalse(l.DripInFlight);
        }

        [Test]
        public void SoaksThenShakesAndDriesABitNeverFails()
        {
            var l = Ready();
            float t = Until(l, Upright, UpsideDownRainEvent.Soaked, 20f);
            Assert.Greater(t, 0f, "a forza di gocce arriva a bagnato pieno");
            Assert.AreEqual(UpsideDownRainState.Soaked, l.State);
            Assert.AreEqual(1f, l.Wetness, 1e-5f);
            Assert.AreEqual(0, Count(l, Upright, 1.0f, UpsideDownRainEvent.Dripped), "niente gocce mentre si scrolla");
            Assert.Less(l.Wetness, 1f);
            float s = Until(l, Upright, UpsideDownRainEvent.Shook, UpsideDownRainLogic.SoakTime);
            Assert.Greater(s, 0f);
            Assert.AreEqual(UpsideDownRainState.Raining, l.State);
            Assert.AreEqual(UpsideDownRainLogic.SoakDryTo, l.Wetness, 1e-4f);
            Assert.Greater(Until(l, Upright, UpsideDownRainEvent.Dripped, 2f), 0f, "poi riprende a piovere");
            AssertNotWon(l);
        }

        [Test]
        public void UmbrellaMovesAndHoleTapsNeverWin()
        {
            var l = Ready();
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(UpsideDownRainEvent.UmbrellaMoved, l.MoveUmbrella());
                Assert.AreEqual(UpsideDownRainEvent.HolePlugged, l.PlugHole());
                Feed(l, Upright, 0.5f);
            }
            Assert.AreEqual(UpsideDownRainEvent.ManTapped, l.TapMan());
            Assert.AreEqual(UpsideDownRainEvent.CloudTapped, l.TapCloud());
            Feed(l, Upright, 10f);
            Assert.AreEqual(20, l.UmbrellaMoves);
            Assert.AreEqual(20, l.HolePlugs);
            Assert.AreEqual(42, l.WrongMoves);
            Assert.Greater(l.Hits, 5, "piove dentro lo stesso");
            AssertNotWon(l);
        }

        [Test]
        public void InvertedForDryIntervalWinsExactlyOnce()
        {
            var l = Ready();
            Assert.AreEqual(UpsideDownRainEvent.Reversing, Feed(l, Upside, 0.6f));
            Assert.IsTrue(l.Reversing);
            int hits = l.Hits;
            float t = Until(l, Upside, UpsideDownRainEvent.Fixed, 3f);
            Assert.Greater(t, 0f);
            Assert.AreEqual(hits, l.Hits, "nessuna goccia lo colpisce mentre la pioggia torna su");
            Assert.AreEqual(UpsideDownRainState.Won, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(1f, l.DryProgress, 1e-5f);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(0, Count(l, Upside, 3f, UpsideDownRainEvent.Fixed));
            Assert.AreEqual(UpsideDownRainEvent.Ignored, l.MoveUmbrella());
            Assert.AreEqual(UpsideDownRainEvent.Ignored, l.PlugHole());
            Assert.AreEqual(UpsideDownRainEvent.None, l.Tick(Frame, 0f, Upright));
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void TotalTimeFromFlipToWinIsAboutFilterPlusDryTime()
        {
            var l = Ready();
            float t = Until(l, Upside, UpsideDownRainEvent.Fixed, 5f);
            Assert.Greater(t, Dry);
            Assert.Less(t, Dry + 0.6f);
        }

        [Test]
        public void ReversingCancelsTheDropInFlight()
        {
            var l = Ready();
            Until(l, Upright, UpsideDownRainEvent.Dripped, 2f);
            Assert.IsTrue(l.DripInFlight);
            int hits = l.Hits;
            Assert.AreEqual(UpsideDownRainEvent.Reversing, Feed(l, Upside, 0.3f));
            Assert.IsFalse(l.DripInFlight);
            Feed(l, Upside, 0.5f);
            Assert.AreEqual(hits, l.Hits);
        }

        [Test]
        public void BrieflyInvertedThenUprightDoesNotWin()
        {
            var l = Ready();
            Until(l, Upside, UpsideDownRainEvent.Reversing, 1f);
            Feed(l, Upside, Dry * 0.5f);
            Assert.AreEqual(UpsideDownRainEvent.ReverseStopped, Feed(l, Upright, 0.6f));
            Assert.AreEqual(UpsideDownRainState.Raining, l.State);
            Assert.AreEqual(1, l.ReverseStops);
            Assert.AreEqual(0f, l.DryProgress, 1e-5f);
            Feed(l, Upright, 5f);
            Assert.Greater(Count(l, Upright, 3f, UpsideDownRainEvent.Hit), 0, "riprende a piovere");
            AssertNotWon(l);
            // e il tentativo dopo vince lo stesso (l'intervallo asciutto riparte da zero)
            Assert.Greater(Until(l, Upside, UpsideDownRainEvent.Fixed, 3f), Dry);
        }

        [Test]
        public void AlreadyInvertedAtStartDoesNotWinUntilUpright()
        {
            var l = new UpsideDownRainLogic();
            Feed(l, Upside, 6f);
            Assert.IsFalse(l.Armed);
            Assert.IsTrue(l.Inverted);
            Assert.AreNotEqual(UpsideDownRainState.Reversing, l.State);
            Assert.IsFalse(l.Reversing);
            AssertNotWon(l);
            Feed(l, Upright, 1f);
            Assert.IsTrue(l.Armed);
            Assert.Greater(Until(l, Upside, UpsideDownRainEvent.Fixed, 4f), 0f);
        }

        [Test]
        public void WobbleAroundThresholdNeverWins()
        {
            var l = Ready();
            // mezza inclinazione: non è capovolto
            Feed(l, 0.45f, 5f);
            Assert.IsFalse(l.Reversing);
            AssertNotWon(l);
            // su e giù ogni 0.4 s: mai DryTime di fila
            for (int i = 0; i < 20; i++)
            {
                Feed(l, Upside, 0.4f);
                Feed(l, Upright, 0.4f);
            }
            AssertNotWon(l);
            // tremolio di mezza inclinazione (sotto InvertOn): non è mai capovolto
            var w = Ready();
            for (int i = 0; i < 6; i++) { Feed(w, 0.2f, 0.2f); Feed(w, 0.3f, 0.2f); }
            AssertNotWon(w);
        }

        [Test]
        public void SideTiltDoesNotCount()
        {
            var l = Ready();
            Feed(l, 0f, 5f, 1f);   // telefono in orizzontale (gravità di lato)
            Assert.IsFalse(l.Reversing);
            AssertNotWon(l);
        }

        [Test]
        public void RapidTouchesAreHarmless()
        {
            var l = Ready();
            for (int i = 0; i < 300; i++)
            {
                l.PlugHole();
                l.TapMan();
                l.TapCloud();
                if (i % 3 == 0) l.MoveUmbrella();
                l.Tick(Frame, 0f, Upright);
            }
            Assert.AreEqual(1000, l.WrongMoves);
            Assert.That(l.Wetness, Is.InRange(0f, 1f));
            AssertNotWon(l);
            Assert.Greater(Until(l, Upside, UpsideDownRainEvent.Fixed, 4f), 0f);
        }

        [Test]
        public void WinsFromSoakedToo()
        {
            var l = Ready();
            Until(l, Upright, UpsideDownRainEvent.Soaked, 20f);
            Assert.AreEqual(UpsideDownRainState.Soaked, l.State);
            Assert.AreEqual(UpsideDownRainEvent.Reversing, Feed(l, Upside, 0.5f));
            Assert.Greater(Until(l, Upside, UpsideDownRainEvent.Fixed, 3f), 0f);
        }

        [Test]
        public void ReversingDriesHimVisually()
        {
            var l = Ready();
            Until(l, Upright, UpsideDownRainEvent.Hit, 3f);
            float w = l.Wetness;
            Until(l, Upside, UpsideDownRainEvent.Reversing, 1f);
            Feed(l, Upside, 0.5f);
            Assert.Less(l.Wetness, w);
        }

        [Test]
        public void CancelWhileReversingKeepsValidStateAndNeedsUprightAgain()
        {
            var l = Ready();
            Until(l, Upside, UpsideDownRainEvent.Reversing, 1f);
            Feed(l, Upside, Dry * 0.6f);
            float w = l.Wetness;
            l.Cancel();
            Assert.AreEqual(UpsideDownRainState.Raining, l.State);
            Assert.IsFalse(l.Armed);
            Assert.AreEqual(w, l.Wetness, 1e-5f);
            Assert.AreEqual(0f, l.DryProgress);
            // ancora capovolto dopo la pausa: non vince finché non lo si raddrizza
            Feed(l, Upside, 4f);
            AssertNotWon(l);
            Feed(l, Upright, 1f);
            Assert.Greater(Until(l, Upside, UpsideDownRainEvent.Fixed, 4f), Dry);
        }

        [Test]
        public void CancelAfterWinChangesNothing()
        {
            var l = Ready();
            Until(l, Upside, UpsideDownRainEvent.Fixed, 4f);
            l.Cancel();
            Assert.AreEqual(UpsideDownRainState.Won, l.State);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = Ready();
            l.MoveUmbrella();
            l.PlugHole();
            Until(l, Upright, UpsideDownRainEvent.Soaked, 20f);
            Until(l, Upside, UpsideDownRainEvent.Fixed, 4f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(UpsideDownRainState.Raining, l.State);
            Assert.AreEqual(UpsideDownRainLogic.StartWet, l.Wetness, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Hits);
            Assert.AreEqual(0, l.Soaks);
            Assert.IsFalse(l.Armed);
            Assert.IsFalse(l.DripInFlight);
            AssertNotWon(l);
            // dopo il reset si vince di nuovo, una volta sola
            Feed(l, Upright, 0.2f);
            Assert.Greater(Until(l, Upside, UpsideDownRainEvent.Fixed, 4f), 0f);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void BadTimeStepsAreSafe()
        {
            var l = Ready();
            Assert.DoesNotThrow(() => l.Tick(float.NaN, 0f, Upright));
            Assert.DoesNotThrow(() => l.Tick(-1f, 0f, Upright));
            Assert.DoesNotThrow(() => l.Tick(float.PositiveInfinity, 0f, Upright));
            Assert.DoesNotThrow(() => l.Tick(Frame, float.NaN, float.NaN));
            // un fotogramma lunghissimo capovolto non vince da solo (passo massimo)
            l.Tick(10f, 0f, Upside);
            AssertNotWon(l);
            Assert.That(l.Wetness, Is.InRange(0f, 1f));
        }
    }
}
