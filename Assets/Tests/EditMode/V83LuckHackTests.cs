using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 54 della roadmap: Luck Hack (la moneta cade sempre su TAILS, tranne se il mondo si capovolge in volo).</summary>
    public class V83LuckHackTests
    {
        const float Upright = -1f, Upside = 1f, Flat = 0f, Frame = 1f / 60f;
        const float Window = LuckHackLogic.FlightTime + LuckHackLogic.GraceTime;

        /// <summary>Lettura costante dell'accelerometro per <paramref name="seconds"/>, a fotogrammi di 1/60 s; ultimo evento.</summary>
        static LuckHackEvent Feed(LuckHackLogic l, float y, float seconds, float x = 0f)
        {
            LuckHackEvent last = LuckHackEvent.None;
            float t = 0f;
            while (t < seconds - 1e-4f)
            {
                float dt = System.Math.Min(Frame, seconds - t);
                LuckHackEvent e = l.Tick(dt, x, y);
                if (e != LuckHackEvent.None) last = e;
                t += dt;
            }
            return last;
        }

        /// <summary>Come Feed ma conta quante volte arriva un certo evento.</summary>
        static int Count(LuckHackLogic l, float y, float seconds, LuckHackEvent what)
        {
            int n = 0;
            for (float t = 0f; t < seconds - 1e-4f; t += Frame) if (l.Tick(Frame, 0f, y) == what) n++;
            return n;
        }

        static LuckHackLogic Ready()
        {
            var l = new LuckHackLogic();
            Feed(l, Upright, 0.3f);
            return l;
        }

        /// <summary>Un lancio normale completo (telefono dritto): deve finire su TAILS.</summary>
        static void PlainFlip(LuckHackLogic l)
        {
            Assert.AreEqual(LuckHackEvent.Flipped, l.PressFlip());
            Assert.AreEqual(LuckHackEvent.Tails, Feed(l, Upright, Window + 0.1f));
            Assert.AreEqual(LuckHackState.Idle, l.State);
        }

        [Test]
        public void StartsIdleOnTails()
        {
            var l = new LuckHackLogic();
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.IsFalse(l.HeadsUp);
            Assert.IsFalse(l.Hacked);
            Assert.IsFalse(l.WindowOpen);
            Assert.IsFalse(l.InputLocked);
            Assert.AreEqual(0f, l.Flight01, 1e-5f);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SameThresholdsAsAristocraticCat()
        {
            Assert.AreEqual(AristocraticCatLogic.InvertOn, LuckHackLogic.InvertOn);
            Assert.AreEqual(AristocraticCatLogic.InvertOff, LuckHackLogic.InvertOff);
            Assert.AreEqual(AristocraticCatLogic.ArmBelow, LuckHackLogic.ArmBelow);
            Assert.AreEqual(AristocraticCatLogic.Smoothing, LuckHackLogic.Smoothing);
            Assert.That(LuckHackLogic.FlightTime, Is.InRange(1.6f, 2.0f));
            Assert.That(LuckHackLogic.GraceTime, Is.InRange(0.25f, 0.35f));
        }

        [Test]
        public void PlainFlipsAlwaysLandTails()
        {
            var l = Ready();
            for (int i = 0; i < 12; i++) PlainFlip(l);
            Assert.AreEqual(12, l.Flips);
            Assert.AreEqual(12, l.WrongMoves);
            Assert.IsFalse(l.HeadsUp);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void PlainFlipTimeline()
        {
            var l = Ready();
            l.PressFlip();
            Assert.AreEqual(LuckHackState.Flying, l.State);
            Assert.IsTrue(l.WindowOpen);
            Feed(l, Upright, LuckHackLogic.FlightTime * 0.5f);
            Assert.AreEqual(0.5f, l.Flight01, 0.02f);
            // atterra su TAILS e traballa per la tolleranza
            Assert.AreEqual(LuckHackEvent.TouchedDown, Feed(l, Upright, LuckHackLogic.FlightTime * 0.5f + 0.02f));
            Assert.AreEqual(LuckHackState.Settling, l.State);
            Assert.IsTrue(l.WindowOpen);
            Assert.AreEqual(LuckHackEvent.Tails, Feed(l, Upright, LuckHackLogic.GraceTime));
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.IsFalse(l.WindowOpen);
        }

        [Test]
        public void FlatPhoneFlipsTooAndStillTails()
        {
            var l = new LuckHackLogic();
            Feed(l, Flat, 0.5f);   // telefono appoggiato sul tavolo
            Assert.AreEqual(LuckHackEvent.Flipped, l.PressFlip());
            Assert.AreEqual(LuckHackEvent.Tails, Feed(l, Flat, Window + 0.1f));
        }

        [Test]
        public void InversionDuringFlightLandsHeadsOnce()
        {
            var l = Ready();
            l.PressFlip();
            Feed(l, Upright, 0.5f);
            Assert.AreEqual(1, Count(l, Upside, 0.6f, LuckHackEvent.Hacked));
            Assert.IsTrue(l.Hacked);
            Assert.AreEqual(LuckHackState.Flying, l.State);   // non vince prima di atterrare
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(1, Count(l, Upside, 2f, LuckHackEvent.LandedHeads));
            Assert.AreEqual(LuckHackState.Won, l.State);
            Assert.IsTrue(l.HeadsUp);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(LuckHackEvent.Ignored, l.PressFlip());
            Assert.AreEqual(LuckHackEvent.Ignored, l.TouchCoin());
            l.Cancel();
            Assert.AreEqual(LuckHackState.Won, l.State);
            Assert.AreEqual(LuckHackEvent.None, l.Tick(Frame, 0f, Upright));
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void InversionLatchesEvenIfStraightenedBeforeLanding()
        {
            var l = Ready();
            l.PressFlip();
            Feed(l, Upside, 0.6f);
            Assert.IsTrue(l.Hacked);
            Assert.AreEqual(LuckHackEvent.LandedHeads, Feed(l, Upright, 2f));
        }

        [Test]
        public void InversionDuringGraceFlipsOver()
        {
            var l = Ready();
            l.PressFlip();
            // capovolto all'ultimo istante del volo: il filtro (~0.28 s da dritto a capovolto) lo vede dopo l'atterraggio
            Feed(l, Upright, LuckHackLogic.FlightTime - 0.1f);
            Assert.AreEqual(LuckHackEvent.TouchedDown, Feed(l, Upside, 0.12f));
            Assert.AreEqual(LuckHackState.Settling, l.State);
            // nella tolleranza la moneta si rigira su HEADS
            Assert.AreEqual(1, Count(l, Upside, 0.25f, LuckHackEvent.FlippedOver));
            Assert.AreEqual(LuckHackState.Won, l.State);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void SlowInversionThatStartsDuringFlightStillCounts()
        {
            var l = Ready();
            l.PressFlip();
            Feed(l, Upright, 0.8f);
            // rotazione lenta, 1 s da dritto a capovolto (y da -1 a +1)
            for (int i = 0; i < 60; i++) l.Tick(Frame, 0f, -1f + 2f * (i + 1) / 60f);
            Feed(l, Upside, 0.15f);
            Assert.IsTrue(l.Hacked);
            Assert.AreEqual(LuckHackState.Flying, l.State);
            Assert.AreEqual(LuckHackEvent.LandedHeads, Feed(l, Upside, 1f));
        }

        [Test]
        public void InversionBeforeFlipIsRefused()
        {
            var l = Ready();
            Feed(l, Upside, 0.5f);
            Assert.IsTrue(l.Inverted);
            Assert.AreEqual(LuckHackEvent.NeedUpright, l.PressFlip());
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.AreEqual(1, l.Refusals);
            // tenuto capovolto: non parte mai e non vince mai
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(LuckHackEvent.NeedUpright, l.PressFlip());
                Assert.AreEqual(LuckHackEvent.None, Feed(l, Upside, 1f));
            }
            Assert.IsFalse(l.TryComplete());
            // a metà strada (inclinato oltre ~100°): ancora rifiutato
            Feed(l, 0.3f, 1f);
            Assert.AreEqual(LuckHackEvent.NeedUpright, l.PressFlip());
        }

        [Test]
        public void InversionBeforeFlipThenUprightIsAPlainTails()
        {
            var l = Ready();
            Feed(l, Upside, 0.5f);
            Feed(l, Upright, 0.5f);
            Assert.AreEqual(LuckHackEvent.Flipped, l.PressFlip());
            Assert.IsFalse(l.Hacked);
            Assert.AreEqual(LuckHackEvent.Tails, Feed(l, Upright, Window + 0.1f));
        }

        [Test]
        public void JustFlippedBackUprightFilterStillHighIsRefused()
        {
            var l = Ready();
            Feed(l, Upside, 0.5f);
            l.Tick(Frame, 0f, Upright);   // un solo fotogramma: il filtro è ancora in alto
            Assert.That(l.GravityY, Is.GreaterThan(LuckHackLogic.ArmBelow));
            Assert.AreEqual(LuckHackEvent.NeedUpright, l.PressFlip());
        }

        [Test]
        public void InversionAfterLandingDoesNotCount()
        {
            var l = Ready();
            PlainFlip(l);
            Assert.AreEqual(LuckHackEvent.None, Feed(l, Upside, 1f));
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.IsFalse(l.Hacked);
            Assert.IsFalse(l.TryComplete());
            // e poi un FLIP capovolto è rifiutato
            Assert.AreEqual(LuckHackEvent.NeedUpright, l.PressFlip());
        }

        [Test]
        public void InversionJustAfterWindowDoesNotCount()
        {
            var l = Ready();
            l.PressFlip();
            Feed(l, Upright, Window + 0.02f);
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.AreEqual(LuckHackEvent.None, Feed(l, Upside, 0.5f));
            Assert.IsFalse(l.HeadsUp);
        }

        [Test]
        public void WobbleNearThresholdDoesNotTrigger()
        {
            var l = Ready();
            l.PressFlip();
            // oscillazioni fra 0.3 e 0.58 (sotto InvertOn), anche rapide
            float t = 0f;
            while (l.State == LuckHackState.Flying || l.State == LuckHackState.Settling)
            {
                float y = 0.44f + 0.14f * (float)System.Math.Sin(t * 25f);
                Assert.AreNotEqual(LuckHackEvent.Hacked, l.Tick(Frame, 0f, y));
                t += Frame;
                Assert.That(t, Is.LessThan(5f));
            }
            Assert.IsFalse(l.Hacked);
            Assert.AreEqual(LuckHackState.Idle, l.State);
            // un solo picco sopra soglia (un sobbalzo) è smorzato dal filtro
            Feed(l, Upright, 0.5f);
            l.PressFlip();
            Feed(l, Upright, 0.4f);
            l.Tick(Frame, 0f, 1f);
            l.Tick(Frame, 0f, 1f);
            Feed(l, Upright, 0.3f);
            Assert.IsFalse(l.Hacked);
        }

        [Test]
        public void TouchingDuringFlightFallsOnTails()
        {
            var l = Ready();
            l.PressFlip();
            Feed(l, Upright, 0.7f);
            Assert.AreEqual(LuckHackEvent.Touched, l.TouchCoin());
            Assert.AreEqual(LuckHackState.Falling, l.State);
            Assert.IsFalse(l.WindowOpen);
            // capovolgere mentre cade non serve più
            Assert.AreEqual(LuckHackEvent.Fell, Feed(l, Upside, LuckHackLogic.DropTime + 0.05f));
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.IsFalse(l.HeadsUp);
            Assert.AreEqual(1, l.Touches);
        }

        [Test]
        public void TouchingAfterHackStillTails()
        {
            var l = Ready();
            l.PressFlip();
            Feed(l, Upside, 0.6f);
            Assert.IsTrue(l.Hacked);
            Assert.AreEqual(LuckHackEvent.Touched, l.TouchCoin());
            Assert.IsFalse(l.Hacked);
            Feed(l, Upside, 3f);
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void TouchingWhileSettlingFallsOnTails()
        {
            var l = Ready();
            l.PressFlip();
            Feed(l, Upright, LuckHackLogic.FlightTime + 0.05f);
            Assert.AreEqual(LuckHackState.Settling, l.State);
            Assert.AreEqual(LuckHackEvent.Touched, l.TouchCoin());
            Assert.AreEqual(LuckHackEvent.Fell, Feed(l, Upright, 0.5f));
            Assert.AreEqual(LuckHackState.Idle, l.State);
        }

        [Test]
        public void TapAtRestSpinsAndStaysTails()
        {
            var l = Ready();
            Assert.AreEqual(LuckHackEvent.Spun, l.TouchCoin());
            Assert.AreEqual(LuckHackState.Spinning, l.State);
            // tocchi a raffica mentre gira: ignorati
            for (int i = 0; i < 10; i++) Assert.AreEqual(LuckHackEvent.Ignored, l.TouchCoin());
            Assert.AreEqual(1, l.Spins);
            Assert.AreEqual(LuckHackEvent.SpinStopped, Feed(l, Upright, LuckHackLogic.SpinTime + 0.05f));
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.IsFalse(l.HeadsUp);
            // capovolto mentre gira: niente
            l.TouchCoin();
            Assert.AreEqual(LuckHackEvent.SpinStopped, Feed(l, Upside, LuckHackLogic.SpinTime + 0.05f));
            Assert.IsFalse(l.HeadsUp);
        }

        [Test]
        public void FlipDuringSpinIsAllowed()
        {
            var l = Ready();
            l.TouchCoin();
            Feed(l, Upright, 0.3f);
            Assert.AreEqual(LuckHackEvent.Flipped, l.PressFlip());
            Assert.AreEqual(LuckHackState.Flying, l.State);
        }

        [Test]
        public void RapidFlipPressesDuringFlightIgnored()
        {
            var l = Ready();
            Assert.AreEqual(LuckHackEvent.Flipped, l.PressFlip());
            for (int i = 0; i < 40; i++)
            {
                Assert.AreEqual(LuckHackEvent.Ignored, l.PressFlip());
                l.Tick(Frame, 0f, Upright);
            }
            Assert.AreEqual(LuckHackState.Flying, l.State);
            Assert.AreEqual(LuckHackEvent.Tails, Feed(l, Upright, Window));
            Assert.AreEqual(1, l.Flips);
            // anche durante la tolleranza e la caduta
            l.PressFlip();
            Feed(l, Upright, LuckHackLogic.FlightTime + 0.05f);
            Assert.AreEqual(LuckHackEvent.Ignored, l.PressFlip());
            l.TouchCoin();
            Assert.AreEqual(LuckHackEvent.Ignored, l.PressFlip());
            Assert.AreEqual(LuckHackEvent.Ignored, l.TouchCoin());
        }

        [Test]
        public void ThreeDifferentWrongAttemptsThenSolution()
        {
            var l = Ready();
            PlainFlip(l);                                         // 1) lancio normale
            l.PressFlip(); Feed(l, Upright, 0.4f); l.TouchCoin(); // 2) tocco in volo
            Feed(l, Upright, 0.5f);
            l.TouchCoin(); Feed(l, Upright, 1f);                  // 3) giravolta sul tavolo
            Feed(l, Upside, 0.5f); l.PressFlip();                 // 4) capovolto prima del lancio
            Feed(l, Upright, 0.5f);
            Assert.AreEqual(4, l.WrongMoves);
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.AreEqual(LuckHackEvent.Flipped, l.PressFlip());
            Feed(l, Upright, 0.3f);
            Feed(l, Upside, 3f);
            Assert.AreEqual(LuckHackState.Won, l.State);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void CancelDuringFlightRestsOnTails()
        {
            var l = Ready();
            l.PressFlip();
            Feed(l, Upside, 0.6f);
            Assert.IsTrue(l.Hacked);
            l.Cancel();
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.IsFalse(l.Hacked);
            Assert.IsFalse(l.WindowOpen);
            Assert.IsFalse(l.Inverted);
            Assert.AreEqual(0, l.Flips);
            // dopo la pausa il telefono è ancora capovolto: il filtro si riempie subito e FLIP è rifiutato
            l.Tick(Frame, 0f, Upside);
            Assert.AreEqual(LuckHackEvent.NeedUpright, l.PressFlip());
            // raddrizzato: si gioca di nuovo
            Feed(l, Upright, 0.5f);
            Assert.AreEqual(LuckHackEvent.Flipped, l.PressFlip());
            Feed(l, Upside, 3f);
            Assert.AreEqual(LuckHackState.Won, l.State);
        }

        [Test]
        public void CancelInEveryStateIsValid()
        {
            var l = Ready();
            l.Cancel();
            Assert.AreEqual(LuckHackState.Idle, l.State);
            l.TouchCoin(); l.Cancel();
            Assert.AreEqual(LuckHackState.Idle, l.State);
            l.Tick(Frame, 0f, Upright);
            l.PressFlip(); Feed(l, Upright, LuckHackLogic.FlightTime + 0.05f); l.Cancel();
            Assert.AreEqual(LuckHackState.Idle, l.State);
            l.Tick(Frame, 0f, Upright);
            l.PressFlip(); l.TouchCoin(); l.Cancel();
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.AreEqual(0, l.Flips);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = Ready();
            PlainFlip(l);
            l.TouchCoin();
            l.PressFlip();
            Feed(l, Upside, 3f);
            Assert.AreEqual(LuckHackState.Won, l.State);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(LuckHackState.Idle, l.State);
            Assert.AreEqual(0, l.Flips);
            Assert.AreEqual(0, l.Spins);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.IsFalse(l.Hacked);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
            // rientro: si vince di nuovo, una volta
            Feed(l, Upright, 0.3f);
            l.PressFlip();
            Feed(l, Upside, 3f);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void BadDeltaTimeAndNaNAreHarmless()
        {
            var l = Ready();
            l.PressFlip();
            Assert.AreEqual(LuckHackEvent.None, l.Tick(float.NaN, 0f, Upright));
            Assert.AreEqual(LuckHackEvent.None, l.Tick(-1f, 0f, Upright));
            Assert.AreEqual(LuckHackEvent.None, l.Tick(float.PositiveInfinity, 0f, Upright));
            Assert.AreEqual(LuckHackEvent.None, l.Tick(Frame, float.NaN, float.NaN));
            Assert.AreEqual(LuckHackState.Flying, l.State);
            Assert.AreEqual(LuckHackEvent.Tails, Feed(l, Upright, Window + 0.1f));
        }

        [Test]
        public void LongFrameAtLandingWithInversionWins()
        {
            // un fotogramma lunghissimo (telefono lento) che scavalca l'atterraggio mentre si capovolge: vale (è dentro il volo)
            var l = Ready();
            l.PressFlip();
            Feed(l, Upright, LuckHackLogic.FlightTime - 0.1f);
            Assert.AreEqual(LuckHackEvent.LandedHeads, l.Tick(0.5f, 0f, Upside));
            Assert.IsTrue(l.TryComplete());
        }
    }
}
