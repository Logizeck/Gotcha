using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 82 della roadmap: The Dishonest Hourglass (ADD TIME toglie tempo; capovolgere il telefono dà +10 s).</summary>
    public class V85DishonestHourglassTests
    {
        const float Upright = -1f, Upside = 1f, Frame = 1f / 60f;
        const float T = DishonestHourglassLogic.StartSeconds;

        /// <summary>Lettura costante dell'accelerometro per <paramref name="seconds"/>, a fotogrammi di 1/60 s; ultimo evento.</summary>
        static DishonestHourglassEvent Feed(DishonestHourglassLogic l, float y, float seconds, float x = 0f)
        {
            DishonestHourglassEvent last = DishonestHourglassEvent.None;
            float t = 0f;
            while (t < seconds - 1e-4f)
            {
                float dt = System.Math.Min(Frame, seconds - t);
                DishonestHourglassEvent e = l.Tick(dt, x, y);
                if (e != DishonestHourglassEvent.None) last = e;
                t += dt;
            }
            return last;
        }

        static int Count(DishonestHourglassLogic l, float y, float seconds, DishonestHourglassEvent what)
        {
            int n = 0;
            for (float t = 0f; t < seconds - 1e-4f; t += Frame) if (l.Tick(Frame, 0f, y) == what) n++;
            return n;
        }

        /// <summary>Fotogrammi fino a un certo evento (al massimo maxSeconds); i secondi passati, o -1 se non arriva.</summary>
        static float Until(DishonestHourglassLogic l, float y, DishonestHourglassEvent what, float maxSeconds)
        {
            for (float t = 0f; t < maxSeconds; t += Frame) if (l.Tick(Frame, 0f, y) == what) return t + Frame;
            return -1f;
        }

        static DishonestHourglassLogic Ready()
        {
            var l = new DishonestHourglassLogic();
            Feed(l, Upright, 0.2f);
            return l;
        }

        [Test]
        public void StartsRunningFromEightSeconds()
        {
            var l = new DishonestHourglassLogic();
            Assert.AreEqual(DishonestHourglassState.Running, l.State);
            Assert.AreEqual(8, l.DisplaySeconds);
            Assert.AreEqual("00:08", DishonestHourglassLogic.Format(l.DisplaySeconds));
            Assert.AreEqual(1f, l.Speed, 1e-5f);
            Assert.IsFalse(l.Armed);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void CountsDownInRealSeconds()
        {
            var l = Ready();
            Feed(l, Upright, 1.0f);
            Assert.AreEqual(T - 1.2f, l.Remaining, 0.02f);
            Assert.AreEqual(7, l.DisplaySeconds);
        }

        [Test]
        public void AddTimeSubtractsAndSpeedsUpAndNeverWins()
        {
            var l = Ready();
            float before = l.Remaining;
            Assert.AreEqual(DishonestHourglassEvent.Subtracted, l.PressAdd());
            Assert.AreEqual(before - DishonestHourglassLogic.PressPenalty, l.Remaining, 1e-4f);
            Assert.Greater(l.Speed, 1f);
            float r = l.Remaining;
            Feed(l, Upright, 0.5f);
            Assert.AreEqual(r - 0.5f * l.Speed, l.Remaining, 0.02f);   // la sabbia scende più in fretta
            Assert.AreNotEqual(DishonestHourglassState.Won, l.State);
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(1, l.Presses);
            Assert.AreEqual(1, l.WrongMoves);
        }

        [Test]
        public void SpeedIsCapped()
        {
            var l = Ready();
            for (int i = 0; i < 3; i++) l.PressAdd();
            Assert.LessOrEqual(l.Speed, DishonestHourglassLogic.MaxSpeed + 1e-5f);
            l.PressAdd();
            Assert.LessOrEqual(l.Speed, DishonestHourglassLogic.MaxSpeed + 1e-5f);
        }

        [Test]
        public void RapidPressesRunOutThenAreHarmless()
        {
            var l = Ready();
            DishonestHourglassEvent last = DishonestHourglassEvent.None;
            int ranOut = 0;
            for (int i = 0; i < 20; i++)
            {
                last = l.PressAdd();
                if (last == DishonestHourglassEvent.RanOut) ranOut++;
            }
            Assert.AreEqual(1, ranOut);
            Assert.AreEqual(DishonestHourglassEvent.NothingToTake, last);
            Assert.AreEqual(DishonestHourglassState.Empty, l.State);
            Assert.AreEqual(0, l.DisplaySeconds);
            Assert.AreEqual(0f, l.Remaining, 1e-5f);
            Assert.AreEqual(DishonestHourglassEvent.Refilled, Feed(l, Upright, DishonestHourglassLogic.EmptyTime + 0.05f));
            Assert.AreEqual(8, l.DisplaySeconds);
            Assert.AreEqual(1f, l.Speed, 1e-5f);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void WaitingRunsOutAndRefillsRepeatedlyWithoutFail()
        {
            var l = Ready();
            for (int round = 0; round < 3; round++)
            {
                float took = Until(l, Upright, DishonestHourglassEvent.RanOut, 10f);
                Assert.Greater(took, 7.5f, "round " + round);   // aspettare: 8 s veri (meno il primo 0.2 s)
                Assert.AreEqual(DishonestHourglassState.Empty, l.State);
                Assert.AreEqual(0, l.DisplaySeconds);
                float empty = Until(l, Upright, DishonestHourglassEvent.Refilled, 3f);
                Assert.AreEqual(DishonestHourglassLogic.EmptyTime, empty, 0.05f);
                Assert.AreEqual(DishonestHourglassState.Running, l.State);
                Assert.AreEqual(8, l.DisplaySeconds);
                Assert.AreEqual(1f, l.Speed, 1e-5f);
            }
            Assert.AreEqual(3, l.RunOuts);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void EmptyLastsAboutOneAndAHalfSeconds()
        {
            var l = Ready();
            Assert.Greater(Until(l, Upright, DishonestHourglassEvent.RanOut, 10f), 0f);
            Feed(l, Upright, 1.3f);
            Assert.AreEqual(DishonestHourglassState.Empty, l.State);
            Assert.AreEqual("00:00", DishonestHourglassLogic.Format(l.DisplaySeconds));
            Feed(l, Upright, 0.3f);
            Assert.AreEqual(DishonestHourglassState.Running, l.State);
        }

        [Test]
        public void InversionWinsOnceWithTenMoreSeconds()
        {
            var l = Ready();
            Feed(l, Upright, 2.6f);   // display a 00:06 (circa 5.2 s rimasti)
            int before = l.DisplaySeconds;
            Assert.AreEqual(1, Count(l, Upside, 1.5f, DishonestHourglassEvent.Turned));
            Assert.AreEqual(DishonestHourglassState.Won, l.State);
            int shown = l.GainedFrom;
            Assert.That(shown, Is.InRange(before - 1, before));   // il filtro impiega ~0.3 s, poi il tempo si ferma
            Assert.AreEqual(shown + 10, l.GainedTo);
            Assert.AreEqual(shown + 10, l.DisplaySeconds);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            // dopo: niente cambia, niente vittorie in più
            Assert.AreEqual(0, Count(l, Upright, 1f, DishonestHourglassEvent.Turned));
            Assert.AreEqual(0, Count(l, Upside, 1f, DishonestHourglassEvent.Turned));
            Assert.AreEqual(DishonestHourglassEvent.Ignored, l.PressAdd());
            Assert.AreEqual(DishonestHourglassEvent.Ignored, l.TapGlass());
            Assert.AreEqual(DishonestHourglassEvent.Ignored, l.TapDisplay());
            Assert.AreEqual(shown + 10, l.DisplaySeconds);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void InversionAtZeroAlsoWins()
        {
            var l = Ready();
            Assert.Greater(Until(l, Upright, DishonestHourglassEvent.RanOut, 10f), 0f);
            Assert.AreEqual(DishonestHourglassState.Empty, l.State);
            Assert.AreEqual(DishonestHourglassEvent.Turned, Feed(l, Upside, 0.9f));
            Assert.AreEqual(0, l.GainedFrom);
            Assert.AreEqual(10, l.GainedTo);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void InversionAfterAddTimeStillWins()
        {
            var l = Ready();
            l.PressAdd();
            l.PressAdd();
            Assert.AreEqual(DishonestHourglassEvent.Turned, Feed(l, Upside, 0.9f));
            Assert.AreEqual(1f, l.Speed, 1e-5f);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void InvertedAtStartDoesNotWinUntilUprightFirst()
        {
            var l = new DishonestHourglassLogic();
            Assert.AreEqual(0, Count(l, Upside, 3f, DishonestHourglassEvent.Turned));
            Assert.IsFalse(l.Armed);
            Assert.AreNotEqual(DishonestHourglassState.Won, l.State);
            // raddrizzato: si arma; poi capovolto di nuovo: vince
            Feed(l, Upright, 0.8f);
            Assert.IsTrue(l.Armed);
            Assert.AreEqual(DishonestHourglassEvent.Turned, Feed(l, Upside, 0.9f));
        }

        [Test]
        public void WobbleAndHalfTiltDoNotTrigger()
        {
            var l = Ready();
            // traballii brevi verso il capovolto
            for (int i = 0; i < 20; i++)
            {
                Feed(l, Upside, 0.1f);
                Feed(l, Upright, 0.25f);
            }
            Assert.AreNotEqual(DishonestHourglassState.Won, l.State);
            // telefono sdraiato o inclinato a metà (sotto la soglia)
            Assert.AreEqual(0, Count(l, 0f, 2f, DishonestHourglassEvent.Turned));
            Assert.AreEqual(0, Count(l, 0.45f, 2f, DishonestHourglassEvent.Turned));
            // di lato (x = ±1) non è capovolto
            for (float t = 0f; t < 2f; t += Frame) Assert.AreNotEqual(DishonestHourglassEvent.Turned, l.Tick(Frame, 1f, 0f));
            Assert.AreNotEqual(DishonestHourglassState.Won, l.State);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void TippingBackBeforeHoldCancels()
        {
            var l = Ready();
            // capovolto quanto basta a passare la soglia ma raddrizzato prima della tenuta
            DishonestHourglassEvent e = DishonestHourglassEvent.None;
            float t = 0f;
            while (e != DishonestHourglassEvent.Tipping && t < 2f) { e = l.Tick(Frame, 0f, Upside); t += Frame; }
            Assert.AreEqual(DishonestHourglassEvent.Tipping, e);
            Assert.IsTrue(l.Tipping);
            Feed(l, Upside, 0.1f);
            Assert.AreEqual(DishonestHourglassEvent.TippedBack, Feed(l, Upright, 0.6f));
            Assert.IsFalse(l.Tipping);
            Assert.AreEqual(0f, l.HoldProgress, 1e-5f);
            Assert.AreEqual(1, l.TipBacks);
            Assert.AreEqual(DishonestHourglassState.Running, l.State);
            // la volta dopo, tenuto: vince
            Assert.AreEqual(DishonestHourglassEvent.Turned, Feed(l, Upside, 0.9f));
        }

        [Test]
        public void HysteresisHoldsNearThreshold()
        {
            var l = Ready();
            Feed(l, Upside, 0.4f);   // supera InvertOn
            Assert.IsTrue(l.Inverted);
            // scende tra InvertOff e InvertOn: resta capovolto
            Feed(l, 0.45f, 0.5f);
            Assert.IsTrue(l.Inverted);
        }

        [Test]
        public void TimerFreezesWhileTipping()
        {
            var l = Ready();
            float t = 0f;
            DishonestHourglassEvent e = DishonestHourglassEvent.None;
            while (e != DishonestHourglassEvent.Tipping && t < 2f) { e = l.Tick(Frame, 0f, Upside); t += Frame; }
            float r = l.Remaining;
            l.Tick(0.1f, 0f, Upside);
            Assert.AreEqual(r, l.Remaining, 1e-5f);
        }

        [Test]
        public void TapsAreHarmlessWrongMoves()
        {
            var l = Ready();
            Assert.AreEqual(DishonestHourglassEvent.GlassTapped, l.TapGlass());
            Assert.AreEqual(DishonestHourglassEvent.DisplayTapped, l.TapDisplay());
            for (int i = 0; i < 30; i++) { l.TapGlass(); l.TapDisplay(); }
            Assert.AreEqual(DishonestHourglassState.Running, l.State);
            Assert.AreEqual(62, l.WrongMoves);
            Assert.AreEqual(8, l.DisplaySeconds);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ThreeDifferentWrongAttemptsThenSolution()
        {
            var l = Ready();
            l.PressAdd();                               // 1: ADD TIME
            l.TapGlass();                               // 2: scuotere la clessidra
            Assert.Greater(Until(l, Upright, DishonestHourglassEvent.RanOut, 10f), 0f);   // 3: aspettare fino a zero
            Assert.AreEqual(DishonestHourglassState.Empty, l.State);
            Assert.Greater(Until(l, Upright, DishonestHourglassEvent.Refilled, 3f), 0f);  // si riempie da sola
            Assert.AreEqual(DishonestHourglassState.Running, l.State);
            Assert.GreaterOrEqual(l.WrongMoves, 3);
            Assert.AreEqual(DishonestHourglassEvent.Turned, Feed(l, Upside, 0.9f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void CancelAbortsTippingAndNeedsUprightAgain()
        {
            var l = Ready();
            float t = 0f;
            DishonestHourglassEvent e = DishonestHourglassEvent.None;
            while (e != DishonestHourglassEvent.Tipping && t < 2f) { e = l.Tick(Frame, 0f, Upside); t += Frame; }
            int shown = l.DisplaySeconds;
            l.Cancel();
            Assert.IsFalse(l.Tipping);
            Assert.IsFalse(l.Armed);
            Assert.IsFalse(l.Inverted);
            Assert.AreEqual(DishonestHourglassState.Running, l.State);
            Assert.AreEqual(shown, l.DisplaySeconds);   // il tempo resta valido
            // al ritorno ancora capovolto: non vince finché non lo si raddrizza
            Assert.AreEqual(0, Count(l, Upside, 2f, DishonestHourglassEvent.Turned));
            Feed(l, Upright, 0.8f);
            Assert.AreEqual(DishonestHourglassEvent.Turned, Feed(l, Upside, 0.9f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void CancelAfterWinChangesNothing()
        {
            var l = Ready();
            Feed(l, Upside, 0.9f);
            Assert.AreEqual(DishonestHourglassState.Won, l.State);
            l.Cancel();
            Assert.AreEqual(DishonestHourglassState.Won, l.State);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = Ready();
            l.PressAdd(); l.PressAdd(); l.TapGlass(); l.TapDisplay();
            Feed(l, Upside, 0.9f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(DishonestHourglassState.Running, l.State);
            Assert.AreEqual(8, l.DisplaySeconds);
            Assert.AreEqual(1f, l.Speed, 1e-5f);
            Assert.IsFalse(l.Armed);
            Assert.IsFalse(l.Tipping);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Presses);
            Assert.AreEqual(0, l.GainedTo);
            Assert.IsFalse(l.TryComplete());
            // si rigioca e si vince di nuovo (una volta)
            Feed(l, Upright, 0.3f);
            Assert.AreEqual(DishonestHourglassEvent.Turned, Feed(l, Upside, 0.9f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void BadTimeStepsAreSafe()
        {
            var l = Ready();
            l.Tick(float.NaN, 0f, Upright);
            l.Tick(-1f, 0f, Upright);
            l.Tick(float.PositiveInfinity, 0f, Upright);
            l.Tick(Frame, float.NaN, float.NaN);
            Assert.AreEqual(DishonestHourglassState.Running, l.State);
            Assert.AreEqual(8, l.DisplaySeconds);
        }

        [Test]
        public void SandAndFormat()
        {
            var l = Ready();
            Assert.AreEqual(l.Remaining / DishonestHourglassLogic.Capacity, l.TopSand01, 1e-5f);
            Assert.AreEqual("00:00", DishonestHourglassLogic.Format(0));
            Assert.AreEqual("00:18", DishonestHourglassLogic.Format(18));
            Assert.AreEqual("01:05", DishonestHourglassLogic.Format(65));
            Assert.AreEqual("00:00", DishonestHourglassLogic.Format(-3));
            Feed(l, Upside, 0.9f);
            Assert.LessOrEqual(l.TopSand01, 1f);
        }
    }
}
