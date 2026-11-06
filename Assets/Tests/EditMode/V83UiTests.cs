using System;
using System.Collections.Generic;
using NUnit.Framework;
using Gotcha.UI;

namespace Gotcha.Tests
{
    /// <summary>0.83: cuori "vivi", nastro dorato dei livelli con un Easter egg, controllo delle condivisioni e premio "+2 vite".</summary>
    public class V83UiTests
    {
        // ---------- cuori ----------

        [Test]
        public void Hearts_EveryMovePoseStartsAndEndsAtRest()
        {
            foreach (HeartMove m in new[] { HeartMove.Hop, HeartMove.Pulse, HeartMove.Pirouette, HeartMove.Wiggle })
            {
                foreach (float k in new[] { 0f, 1f })
                {
                    HeartPose p = HeartIdle.Pose(m, k);
                    Assert.AreEqual(0f, p.OffsetY, 0.01f, m + " y @" + k);
                    Assert.AreEqual(1f, p.ScaleX, 0.01f, m + " sx @" + k);
                    Assert.AreEqual(1f, p.ScaleY, 0.01f, m + " sy @" + k);
                    Assert.AreEqual(0f, p.RotationZ, 0.01f, m + " rot @" + k);
                }
                Assert.Greater(HeartIdle.Duration(m), 0.3f);
                Assert.Less(HeartIdle.Duration(m), 1f);
            }
        }

        [Test]
        public void Hearts_MovesAreContinuous_AndLookDifferent()
        {
            foreach (HeartMove m in new[] { HeartMove.Hop, HeartMove.Pulse, HeartMove.Pirouette, HeartMove.Wiggle })
            {
                HeartPose prev = HeartIdle.Pose(m, 0f);
                for (int i = 1; i <= 400; i++)
                {
                    HeartPose p = HeartIdle.Pose(m, i / 400f);
                    Assert.Less(Math.Abs(p.OffsetY - prev.OffsetY), 1.5f, m + " salto in y a " + i);
                    // nella giravolta la larghezza passa per lo zero cambiando segno (da ±0.08 si ribalta): conta il modulo
                    float dx = m == HeartMove.Pirouette ? Math.Abs(p.ScaleX) - Math.Abs(prev.ScaleX) : p.ScaleX - prev.ScaleX;
                    Assert.Less(Math.Abs(dx), 0.06f, m + " salto in sx a " + i);
                    Assert.Less(Math.Abs(p.ScaleY - prev.ScaleY), 0.06f, m + " salto in sy a " + i);
                    Assert.Less(Math.Abs(p.RotationZ - prev.RotationZ), 2f, m + " salto in rot a " + i);
                    prev = p;
                }
            }
            // ogni mossa ha la sua firma
            Assert.Greater(HeartIdle.Pose(HeartMove.Hop, 0.48f).OffsetY, HeartIdle.HopHeight * 0.9f, "il saltello sale");
            Assert.Less(HeartIdle.Pose(HeartMove.Pirouette, 0.5f).ScaleX, 0f, "a metà giravolta il cuore è ribaltato");
            Assert.Greater(HeartIdle.Pose(HeartMove.Pulse, 0.185f).ScaleX, 1.2f, "primo colpo del battito");
            Assert.Greater(HeartIdle.Pose(HeartMove.Pulse, 0.54f).ScaleX, 1.1f, "secondo colpo del battito");
            Assert.AreEqual(1f, HeartIdle.Pose(HeartMove.Pulse, 0.35f).ScaleX, 0.03f, "pausa tra i due colpi");
            Assert.Greater(Math.Abs(HeartIdle.Pose(HeartMove.Wiggle, 1f / 12f).RotationZ), 10f, "il dondolio ruota");
            // la giravolta non diventa mai una riga invisibile
            for (int i = 0; i <= 100; i++) Assert.GreaterOrEqual(Math.Abs(HeartIdle.Pose(HeartMove.Pirouette, i / 100f).ScaleX), 0.079f);
        }

        /// <summary>Simula un cuore per 10 minuti a 60 fps: mosse a intervalli 1,5-4 s, mai la stessa due volte di fila,
        /// tutte e quattro usate, ferme in mezzo.</summary>
        [Test]
        public void Hearts_RandomScheduleAlternatesMovesWithIdleGaps()
        {
            var h = new HeartIdle(12345);
            var starts = new List<float>();
            var moves = new List<HeartMove>();
            HeartMove prev = HeartMove.None;
            int restFrames = 0;
            for (int f = 0; f < 60 * 600; f++)
            {
                float t = 100f + f / 60f;
                HeartPose p = h.Sample(t);
                if (h.Current != prev && h.Current != HeartMove.None) { starts.Add(t); moves.Add(h.Current); }
                if (h.Current == HeartMove.None)
                {
                    restFrames++;
                    Assert.AreEqual(1f, p.ScaleX); Assert.AreEqual(0f, p.OffsetY);
                }
                prev = h.Current;
            }
            Assert.Greater(moves.Count, 600 / 5, "almeno una mossa ogni ~5 s");
            Assert.Less(moves.Count, 600 / 1.8f, "non in continuazione");
            for (int i = 1; i < moves.Count; i++)
            {
                Assert.AreNotEqual(moves[i - 1], moves[i], "mai la stessa mossa di fila");
                float gap = starts[i] - (starts[i - 1] + HeartIdle.Duration(moves[i - 1]));
                Assert.GreaterOrEqual(gap, HeartIdle.MinGap - 0.02f, "pausa minima");
                Assert.LessOrEqual(gap, HeartIdle.MaxGap + 0.02f, "pausa massima");
            }
            var used = new HashSet<HeartMove>(moves);
            Assert.AreEqual(4, used.Count, "tutte e quattro le mosse");
            Assert.Greater(restFrames, 60 * 600 / 2, "la maggior parte del tempo fermo");
        }

        [Test]
        public void Hearts_SameSeedSameShow_DifferentSeedsNotInSync()
        {
            var a = new HeartIdle(7); var b = new HeartIdle(7);
            var c = new HeartIdle(8);
            int differ = 0;
            for (int f = 0; f < 60 * 60; f++)
            {
                float t = f / 60f;
                HeartPose pa = a.Sample(t), pb = b.Sample(t), pc = c.Sample(t);
                Assert.AreEqual(pa.ScaleX, pb.ScaleX); Assert.AreEqual(pa.OffsetY, pb.OffsetY);
                if (a.Current != c.Current) differ++;
            }
            Assert.Greater(differ, 60 * 10, "due cuori con semi diversi non si muovono insieme");
        }

        [Test]
        public void Hearts_TimeJumpDoesNotReplayMissedMoves_AndRestDelaysNext()
        {
            var h = new HeartIdle(3);
            h.Sample(0f);
            HeartPose p = h.Sample(1000f);   // l'app era in background: una mossa sola, che parte da adesso
            Assert.AreNotEqual(HeartMove.None, h.Current);
            Assert.AreEqual(1f, p.ScaleX, 0.01f, "la mossa parte dall'inizio, non a metà");
            h.Sample(1000f + HeartIdle.Duration(h.Current) + 0.01f);
            Assert.AreEqual(HeartMove.None, h.Current, "finita la mossa, si ferma");
            Assert.GreaterOrEqual(h.NextStart, 1000f + HeartIdle.MinGap);
            h.Rest(2000f);
            Assert.AreEqual(HeartMove.None, h.Current);
            Assert.GreaterOrEqual(h.NextStart, 2000f + HeartIdle.MinGap);
            h.Sample(2000f + HeartIdle.MinGap - 0.01f);
            Assert.AreEqual(HeartMove.None, h.Current, "dopo Rest resta fermo almeno la pausa minima");
        }

        // ---------- nastro dorato ----------

        [Test]
        public void Banner_SlidesInHoldsAndLeaves()
        {
            float outStart = SecretBannerLogic.DefaultOutStart;
            Assert.AreEqual(0f, SecretBannerLogic.Visible(0f, outStart));
            Assert.AreEqual(0f, SecretBannerLogic.Visible(SecretBannerLogic.Delay, outStart));
            float hold = SecretBannerLogic.Delay + SecretBannerLogic.SlideSeconds + 0.1f;
            Assert.AreEqual(1f, SecretBannerLogic.Visible(hold, outStart), 1e-4);
            Assert.AreEqual(1f, SecretBannerLogic.Visible(outStart - 0.01f, outStart), 1e-4);
            Assert.AreEqual(2.5f, SecretBannerLogic.HoldSeconds, 1e-4, "resta circa 2,5 s");
            float mid = SecretBannerLogic.Visible(outStart + SecretBannerLogic.SlideSeconds * 0.5f, outStart);
            Assert.That(mid, Is.GreaterThan(0f).And.LessThan(1f));
            Assert.IsFalse(SecretBannerLogic.Finished(outStart + SecretBannerLogic.SlideSeconds - 0.01f, outStart));
            Assert.IsTrue(SecretBannerLogic.Finished(outStart + SecretBannerLogic.SlideSeconds, outStart));
            Assert.Less(SecretBannerLogic.DefaultOutStart + SecretBannerLogic.SlideSeconds, 4.5f, "copre la parte alta per poco");
            // piccolo rimbalzo in entrata, mai oltre il 115%
            float max = 0f;
            for (int i = 0; i <= 100; i++) max = Math.Max(max, SecretBannerLogic.Visible(SecretBannerLogic.Delay + i / 100f * SecretBannerLogic.SlideSeconds, outStart));
            Assert.That(max, Is.GreaterThan(1f).And.LessThan(1.15f));
        }

        [Test]
        public void Banner_TapDismissesNow_OnlyTappableWhileShown()
        {
            float outStart = SecretBannerLogic.DefaultOutStart;
            Assert.IsFalse(SecretBannerLogic.Tappable(0.1f, outStart), "prima di entrare non ruba tocchi");
            Assert.IsTrue(SecretBannerLogic.Tappable(1.5f, outStart));
            float tapped = SecretBannerLogic.OutStartAfterTap(1.5f, outStart);
            Assert.AreEqual(1.5f, tapped, 1e-4);
            Assert.IsFalse(SecretBannerLogic.Tappable(1.6f, tapped), "mentre esce non riceve tocchi");
            Assert.IsTrue(SecretBannerLogic.Finished(1.5f + SecretBannerLogic.SlideSeconds, tapped));
            // un secondo tocco non lo fa restare di più
            Assert.AreEqual(tapped, SecretBannerLogic.OutStartAfterTap(1.7f, tapped), 1e-4);
            // tocco a metà entrata: esce partendo da dove si trova, senza scatti
            float early = SecretBannerLogic.Delay + 0.1f;
            float o2 = SecretBannerLogic.OutStartAfterTap(early, outStart);
            float before = SecretBannerLogic.Visible(early, outStart), after = SecretBannerLogic.Visible(early + 0.001f, o2);
            Assert.AreEqual(before, after, 0.02f);
        }

        [Test]
        public void Banner_CaptionsArePlainAscii_UpperCase_Short()
        {
            foreach (bool found in new[] { false, true })
                foreach (bool stars in new[] { false, true })
                {
                    string c = SecretBannerLogic.Caption(found, stars);
                    foreach (char ch in c) Assert.Less((int)ch, 128, "carattere non ASCII (i font non hanno stelle/spunte): " + c);
                    Assert.AreEqual(c.ToUpperInvariant(), c);
                    Assert.LessOrEqual(c.Length, 36);
                    StringAssert.Contains("EGG", c);
                    Assert.AreEqual(!stars, c.StartsWith("* "));
                }
            Assert.AreNotEqual(SecretBannerLogic.Caption(false, true), SecretBannerLogic.Caption(true, true));
            StringAssert.Contains("FOUND", SecretBannerLogic.Caption(true, true));
        }

        // ---------- controllo della condivisione ----------

        [Test]
        public void Share_VerifiedOnlyWithPickAndFourSecondsAway()
        {
            Assert.IsTrue(ShareVerifier.IsVerified(true, 1000, 500, 5000));
            Assert.IsFalse(ShareVerifier.IsVerified(true, 1000, 500, 4999), "tornato dopo meno di 4 s");
            Assert.IsFalse(ShareVerifier.IsVerified(false, 1000, 500, 9000), "nessuna app scelta (menu chiuso)");
            Assert.IsFalse(ShareVerifier.IsVerified(true, 400, 500, 9000), "scelta vecchia, di una condivisione precedente");
            Assert.IsFalse(ShareVerifier.IsVerified(true, 1000, 500, -1), "non è ancora tornato");
            Assert.AreEqual(4000, ShareVerifier.MinAwayMs);
        }

        [Test]
        public void Share_FullFlow_PickedAndStayedAway_Verified()
        {
            var v = new ShareVerifier();
            v.Begin(10_000);
            Assert.AreEqual(ShareOutcome.Pending, v.Poll(10_100, false, -1));
            v.OnLeft(10_200);                      // si apre il menu
            Assert.AreEqual(ShareOutcome.Pending, v.Poll(11_000, false, -1));
            // sceglie l'app a 12 s, torna a 20 s
            Assert.AreEqual(ShareOutcome.Pending, v.Poll(15_000, true, 12_000));
            v.OnReturned(20_000);
            Assert.AreEqual(ShareOutcome.Verified, v.Poll(20_016, true, 12_000));
            Assert.IsFalse(v.Active);
            Assert.AreEqual(ShareOutcome.Verified, v.Poll(30_000, false, -1), "l'esito non cambia più");
        }

        [Test]
        public void Share_ChooserCancelled_Rejected_AfterGrace()
        {
            var v = new ShareVerifier();
            v.Begin(0);
            v.OnLeft(100);
            v.OnReturned(9000);   // menu chiuso con Indietro dopo 9 s: nessuna scelta
            Assert.AreEqual(ShareOutcome.Pending, v.Poll(9000 + ShareVerifier.PickGraceMs - 1, false, -1), "l'avviso della scelta può arrivare in ritardo");
            Assert.AreEqual(ShareOutcome.Rejected, v.Poll(9000 + ShareVerifier.PickGraceMs + 1, false, -1));
        }

        [Test]
        public void Share_PickedButBackTooSoon_Rejected_UnlessLeavesAgain()
        {
            var v = new ShareVerifier();
            v.Begin(0);
            v.OnLeft(50);
            v.OnReturned(3000);   // scelta a 1 s, ritorno a 3 s: troppo presto
            Assert.AreEqual(ShareOutcome.Pending, v.Poll(3100, true, 1000));
            Assert.AreEqual(ShareOutcome.Rejected, v.Poll(3000 + ShareVerifier.PickGraceMs + 1, true, 1000));

            // un ritorno "di passaggio" (tra menu e app scelta) seguito da una nuova uscita non boccia
            var w = new ShareVerifier();
            w.Begin(0);
            w.OnLeft(50);
            w.OnReturned(1200);
            Assert.AreEqual(ShareOutcome.Pending, w.Poll(1300, true, 1000));
            w.OnLeft(1400);
            Assert.AreEqual(ShareOutcome.Pending, w.Poll(5000, true, 1000), "di nuovo fuori: si aspetta");
            w.OnReturned(8000);
            Assert.AreEqual(ShareOutcome.Verified, w.Poll(8001, true, 1000));
        }

        [Test]
        public void Share_MenuNeverOpened_OrCancelled_Rejected()
        {
            var v = new ShareVerifier();
            v.Begin(0);
            Assert.AreEqual(ShareOutcome.Pending, v.Poll(ShareVerifier.OpenTimeoutMs - 1, false, -1));
            Assert.AreEqual(ShareOutcome.Rejected, v.Poll(ShareVerifier.OpenTimeoutMs + 1, false, -1));

            var w = new ShareVerifier();
            Assert.IsFalse(w.Active, "senza Begin non c'è niente in attesa");
            w.Begin(0);
            w.Cancel();
            Assert.AreEqual(ShareOutcome.Rejected, w.Outcome);
            // una pressione del pulsante da sola non basta mai
            w.Begin(100);
            Assert.AreNotEqual(ShareOutcome.Verified, w.Poll(100_000, true, 50), "scelta precedente al Begin");
        }

        [Test]
        public void Share_ReturnWithoutLeaving_IsIgnored()
        {
            var v = new ShareVerifier();
            v.Begin(0);
            v.OnReturned(5000);   // fuoco "ritrovato" senza averlo perso: non conta
            Assert.AreEqual(ShareOutcome.Pending, v.Poll(5100, true, 10));
        }

        // ---------- "+2 vite" ----------

        [Test]
        public void Viral_DailyCapOfThree_AndClockRollback()
        {
            Assert.AreEqual(3, ViralShareRules.DailyCap);
            Assert.AreEqual(2, ViralShareRules.RewardLives);
            Assert.AreEqual(2, ViralShareRules.UsedToday(100, 2, 100));
            Assert.AreEqual(0, ViralShareRules.UsedToday(100, 3, 101), "giorno nuovo: si riparte");
            Assert.AreEqual(ViralShareRules.DailyCap, ViralShareRules.UsedToday(100, 1, 99), "data spostata indietro: niente premi");
            Assert.AreEqual(0, ViralShareRules.UsedToday(100, -5, 100));
            Assert.IsFalse(ViralShareRules.CapReached(2));
            Assert.IsTrue(ViralShareRules.CapReached(3));
        }

        [Test]
        public void Viral_OfferedOnlyOnMemorableOrEveryFifthSolve_WhenLivesCanGrow()
        {
            const int max = 5;
            Assert.IsTrue(ViralShareRules.ShouldOffer(true, true, 1, 3, max, false, 0, false), "livello memorabile");
            Assert.IsTrue(ViralShareRules.ShouldOffer(true, false, 10, 3, max, false, 0, false), "decima vittoria");
            Assert.IsFalse(ViralShareRules.ShouldOffer(true, false, 7, 3, max, false, 0, false), "vittoria qualsiasi");
            Assert.IsFalse(ViralShareRules.ShouldOffer(false, true, 5, 3, max, false, 0, false), "solo sui livelli superati");
            Assert.IsFalse(ViralShareRules.ShouldOffer(true, true, 5, max, max, false, 0, false), "vite piene: nascosto");
            Assert.IsFalse(ViralShareRules.ShouldOffer(true, true, 5, 2, max, true, 0, false), "vite infinite: nascosto");
            Assert.IsFalse(ViralShareRules.ShouldOffer(true, true, 5, 2, max, false, 3, false), "3 premi oggi: nascosto");
            Assert.IsFalse(ViralShareRules.ShouldOffer(true, true, 5, 2, max, false, 0, true), "c'è già la condivisione del segreto");
            Assert.IsFalse(ViralShareRules.IsMoment(false, 0));
            Assert.AreEqual(5, ViralShareRules.LivesAfterReward(4, max), "fino al massimo, mai oltre");
            Assert.AreEqual(3, ViralShareRules.LivesAfterReward(1, max));
            int offers = 0;
            for (int n = 1; n <= 50; n++) if (ViralShareRules.IsMoment(false, n)) offers++;
            Assert.AreEqual(10, offers, "una proposta ogni 5 vittorie sui livelli normali");
        }
    }
}
