using System.Collections.Generic;
using NUnit.Framework;
using Gotcha.UI;

namespace Gotcha.Tests
{
    /// <summary>Tempi e curve della schermata "SECRET FOUND!" degli Easter egg (logica pura, senza Unity).</summary>
    public class EggCelebrationTimelineTests
    {
        static EggCelebrationClock Run(EggCelebrationPlan plan, float seconds, float step = 1f / 60f)
        {
            var c = new EggCelebrationClock(plan);
            for (float t = 0f; t < seconds - 1e-4f; t += step) c.Advance(step);
            return c;
        }

        [Test]
        public void FullVersionLastsAsLongAsTheFanfare()
        {
            EggCelebrationPlan p = EggCelebrationPlan.Complete(true);
            Assert.That(p.Duration, Is.EqualTo(5f).Within(1e-4f));
            Assert.That(p.SkipAfter, Is.EqualTo(1.5f).Within(1e-4f));
            Assert.IsTrue(p.Full);
            Assert.IsTrue(p.PieceFlight);
        }

        [Test]
        public void ShortVersionIsAboutTwoSecondsWithoutFlight()
        {
            EggCelebrationPlan p = EggCelebrationPlan.Short();
            Assert.That(p.Duration, Is.InRange(1.8f, 2.5f));
            Assert.IsFalse(p.Full);
            Assert.IsFalse(p.PieceFlight);
            Assert.Less(p.SkipAfter, p.Duration - p.EndFade);
        }

        [Test]
        public void PhasesAreInOrderAndInsideTheDuration()
        {
            foreach (EggCelebrationPlan p in new[] { EggCelebrationPlan.Complete(true), EggCelebrationPlan.Complete(false), EggCelebrationPlan.Short() })
            {
                Assert.LessOrEqual(p.DropStart, p.DropEnd);
                Assert.LessOrEqual(p.DropEnd, p.SlamEnd + 1e-4f);
                Assert.Less(p.SlamStart, p.SlamEnd);
                Assert.Less(p.SlamEnd, p.ShakeEnd);
                Assert.LessOrEqual(p.SlamEnd, p.NameStart);
                Assert.Less(p.NameStart, p.NameEnd);
                Assert.Less(p.PreviewStart, p.PreviewEnd);
                Assert.Less(p.PreviewEnd, p.Duration - p.EndFade);
                if (p.PieceFlight)
                {
                    Assert.LessOrEqual(p.PreviewEnd, p.FlightStart);
                    Assert.Less(p.FlightStart, p.FlightEnd);
                    Assert.Less(p.FlightEnd, p.Duration - p.EndFade);
                }
            }
        }

        [Test]
        public void ClockEndsNaturallyAndShowsResultAtFadeStart()
        {
            var c = new EggCelebrationClock(EggCelebrationPlan.Complete(true));
            Assert.IsFalse(c.ResultDue);
            c = Run(EggCelebrationPlan.Complete(true), 4.3f);
            Assert.IsFalse(c.ResultDue);
            c = Run(EggCelebrationPlan.Complete(true), 4.5f);
            Assert.IsTrue(c.ResultDue);
            Assert.IsFalse(c.Done);
            c = Run(EggCelebrationPlan.Complete(true), 5.2f);
            Assert.IsTrue(c.Done);
            Assert.That(c.T, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void TapIsIgnoredBeforeSkipAfter()
        {
            EggCelebrationClock c = Run(EggCelebrationPlan.Complete(true), 1.2f);
            Assert.IsFalse(c.TrySkip());
            Assert.IsFalse(c.Skipped);
            Assert.That(c.EndAt, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void TapAfterSkipAfterFadesQuickly()
        {
            EggCelebrationClock c = Run(EggCelebrationPlan.Complete(true), 2f);
            Assert.IsTrue(c.TrySkip());
            Assert.IsTrue(c.ResultDue);
            Assert.That(c.EndAt, Is.EqualTo(c.T + EggCelebrationTimeline.SkipFade).Within(1e-4f));
            Assert.IsFalse(c.TrySkip(), "un secondo tocco non cambia nulla");
            for (int i = 0; i < 30; i++) c.Advance(1f / 60f);
            Assert.IsTrue(c.Done);
        }

        [Test]
        public void TapDuringNaturalFadeDoesNothing()
        {
            EggCelebrationClock c = Run(EggCelebrationPlan.Complete(true), 4.6f);
            Assert.IsFalse(c.TrySkip());
        }

        [Test]
        public void LongFrameAfterResumeAdvancesOnlyOneStep()
        {
            var c = new EggCelebrationClock(EggCelebrationPlan.Complete(true));
            c.Advance(30f);   // l'app è stata in background 30 s
            Assert.That(c.T, Is.EqualTo(EggCelebrationTimeline.MaxStep).Within(1e-5f));
            c.Advance(-1f);
            Assert.That(c.T, Is.EqualTo(EggCelebrationTimeline.MaxStep).Within(1e-5f));
        }

        [Test]
        public void SceneFadesInAndOut()
        {
            Assert.That(new EggCelebrationClock(EggCelebrationPlan.Complete(true)).SceneAlpha, Is.EqualTo(0f));
            Assert.That(Run(EggCelebrationPlan.Complete(true), 1f).SceneAlpha, Is.EqualTo(1f).Within(1e-4f));
            Assert.That(Run(EggCelebrationPlan.Complete(true), 4.7f).SceneAlpha, Is.InRange(0.3f, 0.7f));
            Assert.That(Run(EggCelebrationPlan.Complete(true), 5f).SceneAlpha, Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void FanfareKeepsFullVolumeInTheFullVersion()
        {
            Assert.That(Run(EggCelebrationPlan.Complete(true), 4.9f).FanfareVolume, Is.EqualTo(1f));
            Assert.That(Run(EggCelebrationPlan.Short(), 2.05f).FanfareVolume, Is.LessThan(0.5f));
            EggCelebrationClock c = Run(EggCelebrationPlan.Complete(true), 2f);
            c.TrySkip();
            for (int i = 0; i < 9; i++) c.Advance(1f / 60f);   // metà della dissolvenza rapida
            Assert.That(c.FanfareVolume, Is.InRange(0.3f, 0.7f));
        }

        [Test]
        public void EggDropsOvershootsAndSettles()
        {
            EggCelebrationPlan p = EggCelebrationPlan.Complete(true);
            Assert.That(EggCelebrationTimeline.DropOffset(0f, p), Is.EqualTo(1f));
            float min = 1f;
            for (float t = p.DropStart; t <= p.DropEnd; t += 0.005f) min = System.Math.Min(min, EggCelebrationTimeline.DropOffset(t, p));
            Assert.Less(min, -0.05f, "l'uovo deve andare oltre la posizione finale (rimbalzo)");
            Assert.That(EggCelebrationTimeline.DropOffset(p.DropEnd + 0.1f, p), Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void TitleSlamsFromBigToNormal()
        {
            EggCelebrationPlan p = EggCelebrationPlan.Complete(true);
            Assert.That(EggCelebrationTimeline.SlamScale(0f, p), Is.EqualTo(2.6f).Within(1e-4f));
            Assert.That(EggCelebrationTimeline.SlamAlpha(0f, p), Is.EqualTo(0f));
            Assert.That(EggCelebrationTimeline.SlamScale(p.SlamEnd + 2f, p), Is.EqualTo(1f).Within(1e-3f));
            Assert.That(EggCelebrationTimeline.SlamAlpha(p.SlamEnd, p), Is.EqualTo(1f));
        }

        [Test]
        public void ShakeOnlyAfterImpactAndDecays()
        {
            EggCelebrationPlan p = EggCelebrationPlan.Complete(true);
            EggCelebrationTimeline.Shake(p.SlamEnd - 0.01f, p, 30f, out float x, out float y);
            Assert.AreEqual(0f, x); Assert.AreEqual(0f, y);
            EggCelebrationTimeline.Shake(p.ShakeEnd, p, 30f, out x, out y);
            Assert.AreEqual(0f, x); Assert.AreEqual(0f, y);
            float early = 0f, late = 0f;
            for (int i = 0; i < 20; i++)
            {
                EggCelebrationTimeline.Shake(p.SlamEnd + 0.01f + i * 0.003f, p, 30f, out x, out y);
                early = System.Math.Max(early, System.Math.Abs(x) + System.Math.Abs(y));
                EggCelebrationTimeline.Shake(p.ShakeEnd - 0.08f + i * 0.003f, p, 30f, out x, out y);
                late = System.Math.Max(late, System.Math.Abs(x) + System.Math.Abs(y));
            }
            Assert.Greater(early, late);
            Assert.LessOrEqual(early, 60f);
        }

        [Test]
        public void CounterCountsUpAndLandsOnTheTotal()
        {
            EggCelebrationPlan p = EggCelebrationPlan.Complete(true);
            Assert.AreEqual(0, EggCelebrationTimeline.CounterValue(0f, p, 7));
            int last = 0;
            for (float t = 0f; t < p.FlightEnd; t += 0.01f)
            {
                int v = EggCelebrationTimeline.CounterValue(t, p, 7);
                Assert.GreaterOrEqual(v, last, "il contatore non torna mai indietro");
                Assert.LessOrEqual(v, 6, "prima dell'atterraggio non arriva al totale");
                last = v;
            }
            Assert.AreEqual(6, last);
            Assert.AreEqual(7, EggCelebrationTimeline.CounterValue(p.FlightEnd, p, 7));
            Assert.AreEqual(1, EggCelebrationTimeline.CounterValue(p.FlightEnd, p, 1));
            Assert.AreEqual(0, EggCelebrationTimeline.CounterValue(p.FlightEnd - 0.2f, p, 1));
        }

        [Test]
        public void CounterIsStaticWithoutFlight()
        {
            Assert.AreEqual(12, EggCelebrationTimeline.CounterValue(0f, EggCelebrationPlan.Short(), 12));
            Assert.AreEqual(50, EggCelebrationTimeline.CounterValue(0.5f, EggCelebrationPlan.Complete(false), 50));
        }

        [Test]
        public void FlightGoesFromZeroToOneWithAnArc()
        {
            EggCelebrationPlan p = EggCelebrationPlan.Complete(true);
            Assert.AreEqual(0f, EggCelebrationTimeline.Flight(p.FlightStart, p, out float arc));
            Assert.AreEqual(0f, arc);
            Assert.That(EggCelebrationTimeline.Flight((p.FlightStart + p.FlightEnd) * 0.5f, p, out arc), Is.EqualTo(0.5f).Within(1e-4f));
            Assert.That(arc, Is.EqualTo(1f).Within(1e-4f));
            Assert.AreEqual(1f, EggCelebrationTimeline.Flight(p.FlightEnd, p, out arc));
        }

        [Test]
        public void PieceOrderIsAPermutation()
        {
            int[] order = EggCelebrationTimeline.PieceOrder();
            Assert.AreEqual(50, order.Length);
            var seen = new HashSet<int>(order);
            Assert.AreEqual(50, seen.Count);
            foreach (int i in order) Assert.That(i, Is.InRange(0, 49));
        }

        /// <summary>Stessa costruzione di EggPuzzle.Rank: il pezzo nuovo è quello con rank = sbloccati - 1.</summary>
        [Test]
        public void NewestPieceMatchesEggPuzzleRank()
        {
            var order = new int[50];
            for (int i = 0; i < 50; i++) order[i] = i;
            var rnd = new System.Random(2026);
            for (int i = 49; i > 0; i--) { int j = rnd.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
            var rank = new int[50];
            for (int i = 0; i < 50; i++) rank[order[i]] = i;
            for (int unlocked = 1; unlocked <= 50; unlocked++)
            {
                int piece = EggCelebrationTimeline.NewestPiece(unlocked);
                Assert.AreEqual(unlocked - 1, rank[piece]);
            }
            Assert.AreEqual(-1, EggCelebrationTimeline.NewestPiece(0));
            Assert.AreEqual(-1, EggCelebrationTimeline.NewestPiece(51));
        }

        [Test]
        public void MusicComesBackSmoothly()
        {
            Assert.AreEqual(0f, EggCelebrationTimeline.MusicReturn(0f));
            Assert.That(EggCelebrationTimeline.MusicReturn(EggCelebrationTimeline.MusicReturnSeconds * 0.5f), Is.InRange(0.3f, 0.7f));
            Assert.AreEqual(1f, EggCelebrationTimeline.MusicReturn(5f));
        }
    }
}
