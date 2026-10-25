using System;
using NUnit.Framework;
using Gotcha.Levels.V63;

namespace Gotcha.Tests
{
    /// <summary>Livello 256: la macchina alla Rube Goldberg (slot, oggetti, GO, anelli rotti, palloncino).</summary>
    public class V63ChainReactionTests
    {
        static readonly ChainItem[] Movable = { ChainItem.Ball, ChainItem.Book, ChainItem.Domino, ChainItem.Candle, ChainItem.Magnet };

        static ChainReactionLogic Solved()
        {
            var l = new ChainReactionLogic();
            l.TryPlace(ChainItem.Ball, 0, out _);
            l.TryPlace(ChainItem.Book, 1, out _);
            l.TryPlace(ChainItem.Domino, 2, out _);
            l.TryPlace(ChainItem.Candle, 3, out _);
            return l;
        }

        [Test]
        public void CorrectChain_RingsTheBell()
        {
            var l = Solved();
            Assert.IsTrue(l.TryGo(out ChainRun run));
            Assert.IsTrue(run.Success);
            Assert.AreEqual(4, run.Links);
            Assert.AreEqual(-1, run.BrokenSlot);
            Assert.AreEqual(ChainState.Running, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.IsTrue(l.TryFinishRun());
            Assert.AreEqual(ChainState.Ringing, l.State);
            Assert.IsTrue(l.TryCompleteRing());
            Assert.AreEqual(ChainState.Done, l.State);
            Assert.IsFalse(l.TryCompleteRing());
        }

        [Test]
        public void EveryWrongLink_HasItsOwnJoke()
        {
            Assert.AreEqual(ChainOutcome.NothingToPush, ChainReactionLogic.Evaluate(ChainItem.None, ChainItem.Book, ChainItem.Domino, ChainItem.Candle).Outcome);
            Assert.AreEqual(ChainOutcome.StartDomino, ChainReactionLogic.Evaluate(ChainItem.Domino, ChainItem.Book, ChainItem.Ball, ChainItem.Candle).Outcome);
            Assert.AreEqual(ChainOutcome.StartMagnet, ChainReactionLogic.Evaluate(ChainItem.Magnet, ChainItem.Book, ChainItem.Domino, ChainItem.Candle).Outcome);
            Assert.AreEqual(ChainOutcome.NoRamp, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.None, ChainItem.Domino, ChainItem.Candle).Outcome);
            Assert.AreEqual(ChainOutcome.RampDomino, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Domino, ChainItem.Book, ChainItem.Candle).Outcome);
            Assert.AreEqual(ChainOutcome.RampCandle, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Candle, ChainItem.Domino, ChainItem.None).Outcome);
            Assert.AreEqual(ChainOutcome.MagnetGrabsBall, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Magnet, ChainItem.Domino, ChainItem.Candle).Outcome);
            Assert.AreEqual(ChainOutcome.MagnetGrabsBall, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Book, ChainItem.Magnet, ChainItem.Candle).Outcome);
            Assert.AreEqual(ChainOutcome.BallRollsAway, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Book, ChainItem.None, ChainItem.Candle).Outcome);
            Assert.AreEqual(ChainOutcome.ToastedDominoes, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Book, ChainItem.Candle, ChainItem.Domino).Outcome);
            Assert.AreEqual(ChainOutcome.HitsCandle, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Book, ChainItem.Candle, ChainItem.Magnet).Outcome);
            Assert.AreEqual(ChainOutcome.NoCandle, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Book, ChainItem.Domino, ChainItem.None).Outcome);
            Assert.AreEqual(ChainOutcome.DominoOnMagnet, ChainReactionLogic.Evaluate(ChainItem.Ball, ChainItem.Book, ChainItem.Domino, ChainItem.Magnet).Outcome);
            foreach (ChainOutcome o in Enum.GetValues(typeof(ChainOutcome)))
                Assert.IsFalse(string.IsNullOrEmpty(ChainReactionLogic.Joke(o)));
        }

        [Test]
        public void OnlyOneArrangementOfTheFive_Works()
        {
            // tutte le disposizioni possibili (ogni slot vuoto o con un oggetto diverso): una sola suona
            int wins = 0;
            var opts = new ChainItem[Movable.Length + 1];
            opts[0] = ChainItem.None;
            Array.Copy(Movable, 0, opts, 1, Movable.Length);
            foreach (ChainItem a in opts)
            foreach (ChainItem b in opts)
            foreach (ChainItem c in opts)
            foreach (ChainItem d in opts)
            {
                if (Dup(a, b, c, d)) continue;
                ChainRun r = ChainReactionLogic.Evaluate(a, b, c, d);
                Assert.That(r.Links, Is.InRange(0, 4));
                Assert.AreEqual(r.Success, r.Links == 4);
                if (r.Success) wins++;
            }
            Assert.AreEqual(1, wins);
        }

        static bool Dup(params ChainItem[] s)
        {
            for (int i = 0; i < s.Length; i++)
                for (int j = i + 1; j < s.Length; j++)
                    if (s[i] != ChainItem.None && s[i] == s[j]) return true;
            return false;
        }

        [Test]
        public void FailedRun_KeepsTheSlots_AndAllowsRetry()
        {
            var l = new ChainReactionLogic();
            l.TryPlace(ChainItem.Ball, 0, out _);
            l.TryPlace(ChainItem.Domino, 2, out _);
            Assert.IsTrue(l.TryGo(out ChainRun run));
            Assert.AreEqual(ChainOutcome.NoRamp, run.Outcome);
            Assert.AreEqual(1, run.BrokenSlot);
            Assert.IsFalse(l.TryPlace(ChainItem.Book, 1, out _));   // durante la corsa no
            Assert.IsTrue(l.TryFinishRun());
            Assert.AreEqual(ChainState.Building, l.State);
            Assert.AreEqual(ChainItem.Ball, l.SlotItem(0));
            Assert.AreEqual(ChainItem.Domino, l.SlotItem(2));
            Assert.IsTrue(l.TryPlace(ChainItem.Book, 1, out _));
            Assert.IsTrue(l.TryPlace(ChainItem.Candle, 3, out _));
            Assert.IsTrue(l.TryGo(out run));
            Assert.IsTrue(run.Success);
            Assert.AreEqual(2, l.Attempts);
        }

        [Test]
        public void Placing_MovesAndSwaps()
        {
            var l = new ChainReactionLogic();
            Assert.IsTrue(l.TryPlace(ChainItem.Magnet, 2, out ChainItem d));
            Assert.AreEqual(ChainItem.None, d);
            Assert.IsTrue(l.TryPlace(ChainItem.Domino, 2, out d));
            Assert.AreEqual(ChainItem.Magnet, d);                 // la calamita torna sul tavolo
            Assert.AreEqual(-1, l.SlotOf(ChainItem.Magnet));
            Assert.IsTrue(l.TryPlace(ChainItem.Domino, 3, out d)); // spostata da uno slot all'altro
            Assert.AreEqual(ChainItem.None, l.SlotItem(2));
            Assert.AreEqual(3, l.SlotOf(ChainItem.Domino));
            Assert.IsTrue(l.TryRemove(ChainItem.Domino));
            Assert.IsFalse(l.TryRemove(ChainItem.Domino));
            Assert.IsFalse(l.TryPlace(ChainItem.None, 0, out _));
            Assert.IsFalse(l.TryPlace(ChainItem.Ball, 4, out _));
            Assert.IsFalse(l.TryPlace(ChainItem.Ball, -1, out _));
        }

        [Test]
        public void Balloon_FliesAway_NeverStays()
        {
            var l = new ChainReactionLogic();
            Assert.IsFalse(l.TryPlace(ChainItem.Balloon, 1, out _));
            Assert.IsTrue(l.TryBalloonInSlot());
            Assert.IsTrue(l.TryBalloonInSlot());
            Assert.AreEqual(2, l.BalloonEscapes);
            Assert.AreEqual(-1, l.SlotOf(ChainItem.Balloon));
        }

        [Test]
        public void LockedStates_IgnoreInput()
        {
            var l = Solved();
            l.TryGo(out _);
            Assert.IsFalse(l.TryGo(out _));
            Assert.IsFalse(l.TryRemove(ChainItem.Ball));
            Assert.IsFalse(l.TryBalloonInSlot());
            Assert.IsFalse(l.TryCompleteRing());
            l.TryFinishRun();
            Assert.IsFalse(l.TryFinishRun());
            Assert.IsFalse(l.TryPlace(ChainItem.Magnet, 0, out _));
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (ChainState target in Enum.GetValues(typeof(ChainState)))
            {
                var l = Solved();
                l.TryBalloonInSlot();
                if (target >= ChainState.Running) l.TryGo(out _);
                if (target >= ChainState.Ringing) l.TryFinishRun();
                if (target >= ChainState.Done) l.TryCompleteRing();
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(ChainState.Building, l.State);
                for (int i = 0; i < ChainReactionLogic.Slots; i++) Assert.AreEqual(ChainItem.None, l.SlotItem(i));
                Assert.AreEqual(0, l.Attempts);
                Assert.AreEqual(0, l.BalloonEscapes);
                Assert.IsFalse(l.CompletionReported);
                Assert.IsTrue(l.TryGo(out ChainRun r));
                Assert.AreEqual(ChainOutcome.NothingToPush, r.Outcome);
            }
        }

        [Test]
        public void RandomSequences_NeverBlock([Values(1, 2, 3, 4, 5, 6)] int seed, [Values(9, 80, 400)] int steps)
        {
            var rnd = new Random(seed);
            var l = new ChainReactionLogic();
            int completions = 0;
            for (int step = 0; step < steps; step++)
            {
                switch (rnd.Next(8))
                {
                    case 0:
                    case 1: l.TryPlace(Movable[rnd.Next(Movable.Length)], rnd.Next(-1, 5), out _); break;
                    case 2: l.TryRemove(Movable[rnd.Next(Movable.Length)]); break;
                    case 3: l.TryBalloonInSlot(); break;
                    case 4: l.TryGo(out _); break;
                    case 5: l.TryFinishRun(); break;
                    case 6: if (l.TryCompleteRing()) completions++; break;
                    default: if (rnd.Next(50) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.AreEqual(l.State == ChainState.Done, l.CompletionReported);
                // ogni oggetto al massimo in uno slot
                foreach (ChainItem it in Movable)
                {
                    int n = 0;
                    for (int i = 0; i < ChainReactionLogic.Slots; i++) if (l.SlotItem(i) == it) n++;
                    Assert.LessOrEqual(n, 1);
                }
            }
            // nessun blocco: da qui la disposizione giusta suona sempre
            if (l.State == ChainState.Running) l.TryFinishRun();
            if (l.State == ChainState.Building)
            {
                Assert.IsTrue(l.TryPlace(ChainItem.Ball, 0, out _));
                Assert.IsTrue(l.TryPlace(ChainItem.Book, 1, out _));
                Assert.IsTrue(l.TryPlace(ChainItem.Domino, 2, out _));
                Assert.IsTrue(l.TryPlace(ChainItem.Candle, 3, out _));
                Assert.IsTrue(l.TryGo(out ChainRun r));
                Assert.IsTrue(r.Success);
                Assert.IsTrue(l.TryFinishRun());
            }
            if (l.State == ChainState.Ringing) Assert.IsTrue(l.TryCompleteRing());
            Assert.AreEqual(ChainState.Done, l.State);
        }
    }
}
