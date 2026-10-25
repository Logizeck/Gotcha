using System;
using NUnit.Framework;
using Gotcha.Levels.V63;

namespace Gotcha.Tests
{
    /// <summary>Livello 257: gli orologi bugiardi (tempo, cucù, tastierino, errori, sconfitta).</summary>
    public class V63LyingClocksTests
    {
        /// <summary>Imposta sul tastierino l'ora (minuti 0-719) passando dalle frecce, come il giocatore.</summary>
        static void Dial(LyingClocksLogic l, int minutes)
        {
            int h = LyingClocksLogic.HourOf(minutes), m = LyingClocksLogic.MinuteOf(minutes);
            while (l.Hour != h) l.StepHour(1);
            while (l.Minute != m) l.StepMinute(1);
        }

        /// <summary>Fa scorrere il tempo a piccoli passi finché canta il cucù; restituisce i secondi passati.</summary>
        static float RunToCuckoo(LyingClocksLogic l, out int calls)
        {
            float t = 0f;
            calls = 0;
            while (calls == 0 && t < 400f) { calls = l.Tick(0.1f); t += 0.1f; }
            return t;
        }

        [Test]
        public void FirstCuckoo_Within25To40Seconds([Range(0, 59)] int seed)
        {
            var l = new LyingClocksLogic();
            l.Reset(seed);
            float t = RunToCuckoo(l, out int calls);
            Assert.That(t, Is.InRange(25f, 40f));
            Assert.That(calls, Is.InRange(1, 12));
            Assert.AreEqual(calls, LyingClocksLogic.HourOf(l.TrueTime));
            // in quel momento l'ora vera è HH:00 e il cucù segna xx:(60 - ritardo)
            Assert.That(LyingClocksLogic.MinuteOf(l.TrueTime), Is.EqualTo(0).Or.EqualTo(59));
            Assert.AreEqual(Wrap60(60 - l.CuckooDelay), Wrap60(LyingClocksLogic.MinuteOf(l.CuckooTime + 0.5f)));
            Assert.That(l.CuckooDelay, Is.InRange(LyingClocksLogic.MinDelay, LyingClocksLogic.MaxDelay));
        }

        static int Wrap60(int m) => ((m % 60) + 60) % 60;

        [Test]
        public void Clocks_Lie_InTheirOwnWays()
        {
            var l = new LyingClocksLogic();
            l.Reset(7);
            float stopped = l.StoppedTime, cuckoo0 = l.CuckooTime, fast0 = l.FastTime, true0 = l.TrueTime;
            l.Tick(30f);   // 10 minuti di gioco
            Assert.AreEqual(10f, LyingClocksLogic.Diff(l.TrueTime, true0), 1e-3f);
            Assert.AreEqual(10f, LyingClocksLogic.Diff(l.CuckooTime, cuckoo0), 1e-3f);   // giusto di velocità
            Assert.AreEqual(20f, LyingClocksLogic.Diff(l.FastTime, fast0), 1e-3f);       // il doppio
            Assert.AreEqual(stopped, l.StoppedTime);                                     // fermo
            Assert.AreEqual(l.CuckooDelay, LyingClocksLogic.Diff(l.TrueTime, l.CuckooTime), 1e-3f);
            Assert.Greater(LyingClocksLogic.Diff(l.TrueTime, l.StoppedTime), 5f);
            Assert.Greater(LyingClocksLogic.Diff(l.TrueTime, l.FastTime), 5f);
        }

        [Test]
        public void Solution_ReadTheCuckoo_AddTheDelay([Range(0, 19)] int seed)
        {
            var l = new LyingClocksLogic();
            l.Reset(seed);
            RunToCuckoo(l, out _);
            // il giocatore vede il cucù segnare xx:(60 - d) quando l'ora vera è HH:00: ricava d
            int delay = 60 - LyingClocksLogic.MinuteOf(l.CuckooTime + 0.5f);
            Assert.AreEqual(l.CuckooDelay, delay);
            l.Tick(20f);
            Dial(l, (int)Math.Floor(l.CuckooTime) + delay);
            Assert.AreEqual(ClockAnswer.Correct, l.TryOk());
            Assert.AreEqual(ClockState.Opening, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryCompleteOpen());
            Assert.IsFalse(l.TryCompleteOpen());
            Assert.AreEqual(ClockState.Done, l.State);
            Assert.AreEqual(0, l.Mistakes);
        }

        [Test]
        public void Tolerance_IsOneMinute()
        {
            var l = new LyingClocksLogic();
            l.Reset(3);
            l.Tick(4.5f);   // 1.5 minuti dopo la partenza
            int now = (int)Math.Floor(l.TrueTime);
            Dial(l, now + 1);
            Assert.AreEqual(ClockAnswer.Correct, l.TryOk());
            l.Reset(3);
            l.Tick(4.5f);
            Dial(l, now - 1);
            Assert.AreEqual(ClockAnswer.Correct, l.TryOk());
            l.Reset(3);
            l.Tick(4.5f);
            Dial(l, now + 3);
            Assert.AreNotEqual(ClockAnswer.Correct, l.TryOk());
        }

        [Test]
        public void CopiedFromALiar_GetsTheRightJoke()
        {
            var l = new LyingClocksLogic();
            l.Reset(11);
            l.Tick(6f);
            Dial(l, (int)Math.Floor(l.StoppedTime));
            Assert.AreEqual(ClockAnswer.CopiedStopped, l.TryOk());
            Dial(l, (int)Math.Floor(l.FastTime));
            Assert.AreEqual(ClockAnswer.CopiedFast, l.TryOk());
            Assert.AreEqual(2, l.Mistakes);
            Assert.AreEqual(ClockState.Running, l.State);
            Assert.AreEqual("That one died yesterday.", LyingClocksLogic.Answer(ClockAnswer.CopiedStopped));
            Assert.AreEqual("That one drinks coffee.", LyingClocksLogic.Answer(ClockAnswer.CopiedFast));
        }

        [Test]
        public void ThirdMistake_Fails()
        {
            var l = new LyingClocksLogic();
            l.Reset(5);
            l.Tick(3f);
            Dial(l, (int)Math.Floor(l.CuckooTime));
            Assert.AreEqual(ClockAnswer.CopiedCuckoo, l.TryOk());
            Dial(l, (int)Math.Floor(l.TrueTime) + 300);
            Assert.AreEqual(ClockAnswer.Wrong, l.TryOk());
            Assert.AreEqual(ClockState.Running, l.State);
            Assert.AreNotEqual(ClockAnswer.Correct, l.TryOk());   // terzo errore (stessa ora sbagliata)
            Assert.AreEqual(3, l.Mistakes);
            Assert.AreEqual(ClockState.Failed, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(ClockAnswer.None, l.TryOk());
            Assert.IsFalse(l.StepHour(1));
            Assert.AreEqual(0, l.Tick(400f));                       // il tempo si ferma
            Assert.IsFalse(l.TryCompleteOpen());
        }

        [Test]
        public void Keypad_Wraps()
        {
            var l = new LyingClocksLogic();
            Assert.AreEqual(12, l.Hour);
            Assert.AreEqual(0, l.Minute);
            l.StepHour(1);
            Assert.AreEqual(1, l.Hour);
            l.StepHour(-2);
            Assert.AreEqual(11, l.Hour);
            l.StepMinute(-1);
            Assert.AreEqual(59, l.Minute);
            Assert.AreEqual(11, l.Hour);        // i minuti non trascinano le ore
            l.StepMinute(2);
            Assert.AreEqual(1, l.Minute);
            Assert.AreEqual(11 * 60 + 1, l.Entered);
            Assert.IsFalse(l.StepMinute(0));
            Assert.AreEqual("04:07", LyingClocksLogic.Format(4, 7));
        }

        [Test]
        public void Radio_IsUseless_ButAlwaysTalks()
        {
            for (int i = -3; i < 20; i++) Assert.IsFalse(string.IsNullOrEmpty(LyingClocksLogic.RadioLine(i)));
            Assert.AreEqual("It's... time for music!", LyingClocksLogic.RadioLine(0));
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (ClockState target in Enum.GetValues(typeof(ClockState)))
            {
                var l = new LyingClocksLogic();
                l.Reset(2);
                l.Tick(10f);
                l.StepHour(3);
                if (target == ClockState.Opening || target == ClockState.Done) { Dial(l, (int)Math.Floor(l.TrueTime)); l.TryOk(); }
                if (target == ClockState.Done) l.TryCompleteOpen();
                if (target == ClockState.Failed) { Dial(l, (int)Math.Floor(l.TrueTime) + 200); l.TryOk(); l.TryOk(); l.TryOk(); }
                Assert.AreEqual(target, l.State);
                l.Reset(2);
                Assert.AreEqual(ClockState.Running, l.State);
                Assert.AreEqual(0f, l.Elapsed);
                Assert.AreEqual(12, l.Hour);
                Assert.AreEqual(0, l.Minute);
                Assert.AreEqual(0, l.Mistakes);
                Assert.IsFalse(l.CompletionReported);
                Assert.IsTrue(l.StepHour(1));
            }
        }

        [Test]
        public void RandomSequences_NeverBlock([Values(1, 2, 3, 4, 5, 6)] int seed, [Values(10, 120, 500)] int steps)
        {
            var rnd = new Random(seed);
            var l = new LyingClocksLogic();
            l.Reset(seed);
            int completions = 0;
            for (int step = 0; step < steps; step++)
            {
                int calls = 0;
                switch (rnd.Next(8))
                {
                    case 0: l.StepHour(rnd.Next(-1, 2)); break;
                    case 1:
                    case 2: l.StepMinute(rnd.Next(-3, 4)); break;
                    case 3: if (rnd.Next(6) == 0) l.TryOk(); break;
                    case 4: if (l.TryCompleteOpen()) completions++; break;
                    case 5: calls = l.Tick((float)rnd.NextDouble() * 5f); break;
                    default: if (rnd.Next(70) == 0) { l.Reset(rnd.Next()); completions = 0; } break;
                }
                Assert.That(calls, Is.InRange(0, 12));
                Assert.LessOrEqual(completions, 1);
                Assert.LessOrEqual(l.Mistakes, LyingClocksLogic.MaxMistakes);
                Assert.AreEqual(l.Mistakes >= LyingClocksLogic.MaxMistakes, l.State == ClockState.Failed);
                Assert.AreEqual(l.State == ClockState.Done, l.CompletionReported);
                Assert.That(l.Hour, Is.InRange(1, 12));
                Assert.That(l.Minute, Is.InRange(0, 59));
                Assert.That(l.TrueTime, Is.InRange(0f, 720f));
            }
            // nessun blocco: si vince (finché non si è perso), oppure si riparte
            if (l.State == ClockState.Failed) { l.Reset(seed); Assert.AreEqual(ClockState.Running, l.State); }
            if (l.State == ClockState.Running)
            {
                Dial(l, (int)Math.Floor(l.TrueTime));
                Assert.AreEqual(ClockAnswer.Correct, l.TryOk());
            }
            if (l.State == ClockState.Opening) Assert.IsTrue(l.TryCompleteOpen());
            Assert.AreEqual(ClockState.Done, l.State);
        }
    }
}
