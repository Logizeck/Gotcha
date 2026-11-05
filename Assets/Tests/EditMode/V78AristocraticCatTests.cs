using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 13 della roadmap: The Aristocratic Cat (gli oggetti non lo convincono, il telefono capovolto sì).</summary>
    public class V78AristocraticCatTests
    {
        // stessa geometria del gioco: zona "verso il gatto" e soglia della scala
        const float ZoneMinX = -0.45f, ZoneMaxX = 2.45f, ZoneMinY = 0.0f, LadderMinX = -1.2f;
        const float Upright = -1f, Upside = 1f, Frame = 1f / 60f;

        static AristocraticCatLogic New() => new AristocraticCatLogic(ZoneMinX, ZoneMaxX, ZoneMinY, LadderMinX);

        /// <summary>Lettura costante dell'accelerometro per <paramref name="seconds"/>, a fotogrammi di 1/60 s.</summary>
        static AristocraticCatEvent Feed(AristocraticCatLogic l, float y, float seconds, float x = 0f)
        {
            AristocraticCatEvent last = AristocraticCatEvent.None;
            float t = 0f;
            while (t < seconds - 1e-4f)
            {
                float dt = System.Math.Min(Frame, seconds - t);
                AristocraticCatEvent e = l.Tick(dt, x, y);
                if (e != AristocraticCatEvent.None) last = e;
                t += dt;
                if (e == AristocraticCatEvent.Slid) break;
            }
            return last;
        }

        /// <summary>Come Feed ma conta quante volte arriva un certo evento.</summary>
        static int Count(AristocraticCatLogic l, float y, float seconds, AristocraticCatEvent what)
        {
            int n = 0;
            float t = 0f;
            while (t < seconds - 1e-4f)
            {
                if (l.Tick(Frame, 0f, y) == what) n++;
                t += Frame;
            }
            return n;
        }

        static AristocraticCatLogic Armed()
        {
            var l = New();
            Feed(l, Upright, 0.2f);
            Assert.IsTrue(l.Armed);
            return l;
        }

        [Test]
        public void StartsPerchedNotArmed()
        {
            var l = New();
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.IsFalse(l.Armed);
            Assert.IsFalse(l.LadderLeaned);
            Assert.AreEqual(0f, l.Lean, 1e-5f);
            Assert.AreEqual(0f, l.HoldProgress, 1e-5f);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void OfferingObjectsNeverWins()
        {
            var l = Armed();
            Assert.AreEqual(AristocraticCatEvent.LadderLeaned, l.Drop(AristocraticCatItem.Ladder, -0.6f, -1.2f));
            Assert.IsTrue(l.LadderLeaned);
            Assert.AreEqual(AristocraticCatEvent.BowlRefused, l.Drop(AristocraticCatItem.Bowl, 1.0f, 1.2f));
            Assert.AreEqual(AristocraticCatEvent.ToyRefused, l.Drop(AristocraticCatItem.Toy, 0.2f, 0.4f));
            Assert.AreEqual(AristocraticCatEvent.Poked, l.Poke());
            for (int i = 0; i < 30; i++)
            {
                l.Poke();
                l.Drop(AristocraticCatItem.Bowl, 1f, 1f);
                l.Drop(AristocraticCatItem.Toy, 1f, 1f);
                l.Pick(AristocraticCatItem.Ladder);
                l.Drop(AristocraticCatItem.Ladder, 0f, 0f);
                Feed(l, Upright, 0.1f);
            }
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(31, l.Pokes);
            Assert.AreEqual(31, l.BowlOffers);
            Assert.AreEqual(31, l.ToyOffers);
            Assert.AreEqual(31, l.LadderOffers);
        }

        [Test]
        public void ObjectsDroppedAwayFromTheCatAreJustDropped()
        {
            var l = Armed();
            Assert.AreEqual(AristocraticCatEvent.Dropped, l.Drop(AristocraticCatItem.Bowl, 1.0f, -1.0f));   // troppo in basso
            Assert.AreEqual(AristocraticCatEvent.Dropped, l.Drop(AristocraticCatItem.Toy, -1.8f, 1.0f));    // troppo a sinistra
            Assert.AreEqual(AristocraticCatEvent.Dropped, l.Drop(AristocraticCatItem.Ladder, -1.8f, -1.3f)); // di nuovo al muro
            Assert.IsFalse(l.LadderLeaned);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void LadderIsReversible()
        {
            var l = Armed();
            l.Drop(AristocraticCatItem.Ladder, -0.5f, -1.2f);
            Assert.IsTrue(l.LadderLeaned);
            Assert.IsTrue(l.Pick(AristocraticCatItem.Ladder));
            Assert.IsFalse(l.LadderLeaned, "presa in mano si stacca dall'armadio");
            l.Drop(AristocraticCatItem.Ladder, -1.75f, -1.3f);
            Assert.IsFalse(l.LadderLeaned);
            // la scala appoggiata non impedisce la soluzione
            l.Drop(AristocraticCatItem.Ladder, -0.5f, -1.2f);
            Assert.AreEqual(AristocraticCatEvent.Slid, Feed(l, Upside, 2f));
        }

        [Test]
        public void FullInversionHeldWinsOnce()
        {
            var l = Armed();
            int clinging = 0, slid = 0;
            for (int i = 0; i < 180; i++)
            {
                AristocraticCatEvent e = l.Tick(Frame, 0f, Upside);
                if (e == AristocraticCatEvent.Clinging) clinging++;
                if (e == AristocraticCatEvent.Slid) slid++;
            }
            Assert.AreEqual(1, clinging);
            Assert.AreEqual(1, slid);
            Assert.AreEqual(AristocraticCatState.Fallen, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(1f, l.HoldProgress, 1e-5f);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "Win una volta sola");
            // dopo la caduta tutto è ignorato
            Assert.AreEqual(AristocraticCatEvent.None, l.Tick(Frame, 0f, Upright));
            Assert.AreEqual(AristocraticCatEvent.Ignored, l.Poke());
            Assert.AreEqual(AristocraticCatEvent.Ignored, l.Drop(AristocraticCatItem.Bowl, 1f, 1f));
            Assert.IsFalse(l.Pick(AristocraticCatItem.Ladder));
            l.Cancel();
            Assert.AreEqual(AristocraticCatState.Fallen, l.State, "la pausa non annulla la caduta già partita");
        }

        [Test]
        public void HoldTimeIsMeasuredFromTheInversion()
        {
            var l = Armed();
            // dritto -> capovolto di colpo: il filtro supera InvertOn dopo ~0.27 s, poi servono HoldTime s
            float t = 0f;
            AristocraticCatEvent e = AristocraticCatEvent.None;
            float clingAt = -1f;
            while (e != AristocraticCatEvent.Slid && t < 5f)
            {
                e = l.Tick(Frame, 0f, Upside);
                t += Frame;
                if (e == AristocraticCatEvent.Clinging) clingAt = t;
            }
            Assert.That(clingAt, Is.InRange(0.2f, 0.4f));
            Assert.That(t - clingAt, Is.InRange(AristocraticCatLogic.HoldTime - 2 * Frame, AristocraticCatLogic.HoldTime + 2 * Frame));
            Assert.That(t, Is.LessThan(1.2f), "dal gesto alla caduta: circa un secondo");
        }

        [Test]
        public void PartialInversionReleasedEarlyCancels()
        {
            var l = Armed();
            Assert.AreEqual(AristocraticCatEvent.Clinging, Feed(l, Upside, 0.4f));
            Assert.AreEqual(AristocraticCatState.Clinging, l.State);
            Assert.That(l.HoldProgress, Is.GreaterThan(0f).And.LessThan(1f));
            Assert.AreEqual(AristocraticCatEvent.ClimbedBack, Feed(l, Upright, 0.5f));
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.AreEqual(0f, l.HoldProgress, 1e-5f);
            Assert.AreEqual(1, l.ClimbBacks);
            // il tempo riparte da zero: un'altra tenuta breve non basta
            Feed(l, Upside, 0.6f);
            Assert.AreEqual(AristocraticCatState.Clinging, l.State);
            Feed(l, Upright, 0.5f);
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.IsFalse(l.TryComplete());
            // e poi una tenuta piena vince
            Assert.AreEqual(AristocraticCatEvent.Slid, Feed(l, Upside, 2f));
        }

        [Test]
        public void SidewaysOrHalfTurnedNeverWins()
        {
            var l = Armed();
            // telefono di lato (90°): y ≈ 0
            Assert.AreEqual(0, Count(l, 0f, 5f, AristocraticCatEvent.Slid));
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            // girato di ~120°: y ≈ 0.5, sotto InvertOn
            Assert.AreEqual(0, Count(l, 0.5f, 5f, AristocraticCatEvent.Slid));
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.That(l.Lean, Is.GreaterThan(AristocraticCatLogic.UneasyAt), "ma il gatto lo sente (aggrappo visivo)");
            // piatto sul tavolo (gravità tutta su z)
            Assert.AreEqual(0, Count(l, 0f, 3f, AristocraticCatEvent.Clinging));
        }

        [Test]
        public void UneasyIsSaidOncePerTilt()
        {
            var l = Armed();
            Assert.AreEqual(1, Count(l, 0.45f, 2f, AristocraticCatEvent.Uneasy));
            Assert.AreEqual(0, Count(l, 0.45f, 2f, AristocraticCatEvent.Uneasy), "non si ripete finché resta inclinato");
            Feed(l, Upright, 1f);
            Assert.AreEqual(1, Count(l, 0.45f, 2f, AristocraticCatEvent.Uneasy), "si riarma dopo essere tornato dritto");
        }

        [Test]
        public void AlreadyInvertedAtStartDoesNotWinUntilUpright()
        {
            var l = New();
            Assert.AreEqual(0, Count(l, Upside, 5f, AristocraticCatEvent.Slid));
            Assert.AreEqual(0, Count(l, Upside, 1f, AristocraticCatEvent.Clinging));
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.IsFalse(l.Armed);
            Assert.IsTrue(l.Inverted);
            Assert.AreEqual(0f, l.Lean, 1e-5f, "niente reazioni finché non è armato");
            Assert.AreEqual(0, Count(l, Upside, 1f, AristocraticCatEvent.Uneasy));
            // girato poco (resta sopra ArmBelow): ancora niente
            Feed(l, 0.3f, 2f);
            Assert.IsFalse(l.Armed);
            Feed(l, Upside, 2f);
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            // dritto, poi di nuovo capovolto: ora vale
            Feed(l, Upright, 1f);
            Assert.IsTrue(l.Armed);
            Assert.AreEqual(AristocraticCatEvent.Slid, Feed(l, Upside, 2f));
        }

        [Test]
        public void FlatOnTableArmsToo()
        {
            var l = New();
            Feed(l, 0f, 0.5f);   // piatto: y ≈ 0
            Assert.IsTrue(l.Armed);
            Assert.AreEqual(AristocraticCatEvent.Slid, Feed(l, Upside, 2f));
        }

        [Test]
        public void ThresholdBoundaries()
        {
            // appena sotto InvertOn per sempre: mai capovolto
            var l = Armed();
            Assert.AreEqual(0, Count(l, AristocraticCatLogic.InvertOn - 0.02f, 4f, AristocraticCatEvent.Clinging));
            Assert.IsFalse(l.Inverted);
            // appena sopra: si aggrappa e vince
            Assert.AreEqual(AristocraticCatEvent.Slid, Feed(l, AristocraticCatLogic.InvertOn + 0.02f, 5f));

            // isteresi: una volta capovolto, scendere fra InvertOff e InvertOn non annulla
            l = Armed();
            Feed(l, Upside, 0.4f);
            Assert.AreEqual(AristocraticCatState.Clinging, l.State);
            Assert.AreEqual(AristocraticCatEvent.Slid, Feed(l, 0.45f, 2f));

            // sotto InvertOff annulla
            l = Armed();
            Feed(l, Upside, 0.4f);
            Assert.AreEqual(AristocraticCatEvent.ClimbedBack, Feed(l, AristocraticCatLogic.InvertOff - 0.1f, 2f));
            Assert.AreEqual(AristocraticCatState.Perched, l.State);

            // ArmBelow: una lettura appena sopra non arma, appena sotto sì
            l = New();
            Feed(l, AristocraticCatLogic.ArmBelow + 0.02f, 1f);
            Assert.IsFalse(l.Armed);
            Feed(l, AristocraticCatLogic.ArmBelow - 0.02f, 1f);
            Assert.IsTrue(l.Armed);
        }

        [Test]
        public void FilterIgnoresSingleSpikes()
        {
            var l = Armed();
            // un picco di un fotogramma (scossone) non capovolge
            for (int i = 0; i < 20; i++)
            {
                Assert.AreNotEqual(AristocraticCatEvent.Clinging, l.Tick(Frame, 0f, 3f));
                Feed(l, Upright, 0.3f);
            }
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.IsFalse(l.Inverted);
        }

        [Test]
        public void RapidWobbleNearThresholdNeverTriggers()
        {
            var l = Armed();
            // oscillazione rapida attorno alla soglia, fotogramma per fotogramma
            for (int i = 0; i < 600; i++)
                Assert.AreNotEqual(AristocraticCatEvent.Slid, l.Tick(Frame, 0f, i % 2 == 0 ? 0.15f : 0.95f));
            Assert.AreEqual(AristocraticCatState.Perched, l.State, "media 0.55 < InvertOn");
            // dondolio dritto/capovolto ogni 0.35 s: si aggrappa e risale, ma la tenuta non si accumula
            l = Armed();
            int slid = 0;
            for (int i = 0; i < 20; i++)
            {
                slid += Count(l, Upside, 0.35f, AristocraticCatEvent.Slid);
                slid += Count(l, Upright, 0.35f, AristocraticCatEvent.Slid);
            }
            Assert.AreEqual(0, slid);
            Assert.AreNotEqual(AristocraticCatState.Fallen, l.State);
            Assert.That(l.ClimbBacks, Is.GreaterThan(0));
        }

        [Test]
        public void HugeFramesStillNeedTheHold()
        {
            var l = Armed();
            // un fotogramma lunghissimo (scatto): il filtro si riempie, ma la tenuta parte da zero
            Assert.AreEqual(AristocraticCatEvent.Clinging, l.Tick(2f, 0f, Upside));
            Assert.AreEqual(AristocraticCatState.Clinging, l.State);
            Assert.AreEqual(AristocraticCatEvent.None, l.Tick(0.3f, 0f, Upside));
            Assert.AreEqual(AristocraticCatEvent.Slid, l.Tick(0.45f, 0f, Upside));
        }

        [Test]
        public void BadInputIsHarmless()
        {
            var l = Armed();
            l.Tick(float.NaN, 0f, Upside);
            l.Tick(-1f, 0f, Upside);
            l.Tick(Frame, float.NaN, float.NaN);
            l.Tick(float.PositiveInfinity, 0f, Upside);
            Assert.AreNotEqual(AristocraticCatState.Fallen, l.State);
            Assert.IsFalse(float.IsNaN(l.GravityY));
        }

        [Test]
        public void CancelInPauseIsValid()
        {
            var l = Armed();
            Feed(l, Upside, 0.6f);
            Assert.AreEqual(AristocraticCatState.Clinging, l.State);
            l.Cancel();
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.AreEqual(0f, l.HoldProgress, 1e-5f);
            Assert.IsFalse(l.Inverted);
            Assert.IsTrue(l.Armed, "chi ha già girato il telefono resta armato");
            Assert.IsFalse(l.TryComplete());
            // al ritorno, una tenuta piena serve di nuovo tutta
            Assert.AreEqual(AristocraticCatEvent.Clinging, l.Tick(Frame, 0f, Upside), "il filtro riparte dalla lettura vera");
            Assert.AreEqual(AristocraticCatEvent.None, Feed(l, Upside, AristocraticCatLogic.HoldTime * 0.8f));
            Assert.AreEqual(AristocraticCatEvent.Slid, Feed(l, Upside, 0.5f));
            // Cancel più volte di fila, anche da fermo
            l = Armed();
            l.Cancel(); l.Cancel();
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
        }

        [Test]
        public void ResetRestoresEverything()
        {
            var l = Armed();
            l.Drop(AristocraticCatItem.Ladder, 0f, 0f);
            l.Poke(); l.Poke();
            Feed(l, Upside, 2f);
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
            Assert.IsFalse(l.Armed);
            Assert.IsFalse(l.LadderLeaned);
            Assert.AreEqual(0, l.Pokes);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
            // dopo il reset si rivince (una volta)
            Feed(l, Upright, 0.3f);
            Assert.AreEqual(AristocraticCatEvent.Slid, Feed(l, Upside, 2f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void HintAfterEnoughWrongMovesOnce()
        {
            var l = Armed();
            for (int i = 0; i < AristocraticCatLogic.HintAfter - 1; i++) { l.Poke(); Assert.IsFalse(l.TakeHint()); }
            l.Drop(AristocraticCatItem.Toy, 1f, 1f);
            Assert.IsTrue(l.TakeHint());
            l.Poke();
            Assert.IsFalse(l.TakeHint());
        }

        [Test]
        public void RapidPokesAreAllCounted()
        {
            var l = Armed();
            for (int i = 0; i < 50; i++) Assert.AreEqual(AristocraticCatEvent.Poked, l.Poke());
            Assert.AreEqual(50, l.Pokes);
            Assert.AreEqual(AristocraticCatState.Perched, l.State);
        }

        [Test]
        public void ZoneEdges()
        {
            var l = New();
            Assert.IsTrue(l.InCatZone(ZoneMinX, ZoneMinY));
            Assert.IsTrue(l.InCatZone(ZoneMaxX, 3f));
            Assert.IsFalse(l.InCatZone(ZoneMinX - 0.01f, 1f));
            Assert.IsFalse(l.InCatZone(1f, ZoneMinY - 0.01f));
            // la scala si appoggia anche lasciata nella zona alta
            Assert.AreEqual(AristocraticCatEvent.LadderLeaned, l.Drop(AristocraticCatItem.Ladder, LadderMinX, -2f));
            Assert.AreEqual(AristocraticCatEvent.Dropped, l.Drop(AristocraticCatItem.Ladder, LadderMinX - 0.01f, -2f));
        }
    }
}
