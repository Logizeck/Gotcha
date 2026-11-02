using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 10 della roadmap: Half Full (EMPTY fuori dal fumetto, FULL dal cartello nel buco).</summary>
    public class V77HalfFullTests
    {
        static void AssertStillPlaying(HalfFullLogic l)
        {
            Assert.AreEqual(HalfFullStage.Playing, l.Stage);
            Assert.IsFalse(l.InputLocked);
            Assert.AreNotEqual(HalfFullSlot.Full, l.Slot);
            Assert.IsFalse(l.TryComplete());
        }

        static HalfFullEmptyDrop MoveEmpty(HalfFullLogic l, bool insideBubble)
        {
            Assert.IsTrue(l.GrabEmpty());
            Assert.AreEqual(HalfFullSlot.Gap, l.Slot, "mentre si tiene EMPTY il fumetto mostra il buco");
            return l.ReleaseEmpty(insideBubble);
        }

        static HalfFullFullDrop MoveFull(HalfFullLogic l, bool inSlot)
        {
            Assert.IsTrue(l.GrabFull());
            return l.ReleaseFull(inSlot);
        }

        [Test]
        public void StartsAsHalfEmpty()
        {
            var l = new HalfFullLogic();
            AssertStillPlaying(l);
            Assert.AreEqual(HalfFullSlot.Empty, l.Slot);
            Assert.AreEqual(HalfFullEmptyAt.Bubble, l.EmptyAt);
            Assert.AreEqual(HalfFullFullAt.Sign, l.FullAt);
            Assert.AreEqual(0.5f, l.WaterFraction, 1e-5f);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void Solution_EmptyOut_ThenFullIn_WinsOnce()
        {
            var l = new HalfFullLogic();
            Assert.AreEqual(HalfFullEmptyDrop.Floor, MoveEmpty(l, false));
            Assert.AreEqual(HalfFullSlot.Gap, l.Slot);
            Assert.AreEqual(HalfFullEmptyAt.Floor, l.EmptyAt);
            AssertStillPlaying(l);
            Assert.AreEqual(HalfFullFullDrop.Placed, MoveFull(l, true));
            Assert.AreEqual(HalfFullSlot.Full, l.Slot);
            Assert.AreEqual(HalfFullStage.Celebrating, l.Stage);
            Assert.IsTrue(l.InputLocked);
            // a festa iniziata niente si muove più
            Assert.IsFalse(l.GrabEmpty());
            Assert.IsFalse(l.GrabFull());
            Assert.AreEqual(HalfFullPour.None, l.Pour());
            Assert.IsFalse(l.TapMan());
            Assert.IsFalse(l.OfferGlass());
            Assert.IsFalse(l.Cancel());
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(HalfFullStage.Done, l.Stage);
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void FullOntoEmpty_IsAccepted_EmptyFallsOff()
        {
            var l = new HalfFullLogic();
            Assert.AreEqual(HalfFullFullDrop.Swapped, MoveFull(l, true));
            Assert.AreEqual(HalfFullEmptyAt.Floor, l.EmptyAt);
            Assert.AreEqual(HalfFullSlot.Full, l.Slot);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void Filling_NeverWins_AndSpillGoesBackToHalf()
        {
            var l = new HalfFullLogic();
            for (int round = 0; round < 3; round++)
            {
                for (int i = 1; i < HalfFullLogic.PourSteps; i++) Assert.AreEqual(HalfFullPour.Higher, l.Pour());
                Assert.AreEqual(HalfFullPour.Brim, l.Pour());
                Assert.AreEqual(1f, l.WaterFraction, 1e-5f);
                Assert.IsTrue(l.Spilling);
                Assert.AreEqual(HalfFullPour.Spilling, l.Pour());
                AssertStillPlaying(l);
                Assert.IsFalse(l.Tick(HalfFullLogic.SpillSeconds * 0.5f));
                Assert.IsTrue(l.Tick(HalfFullLogic.SpillSeconds));
                Assert.IsFalse(l.Spilling);
                Assert.AreEqual(0.5f, l.WaterFraction, 1e-5f);
                Assert.IsFalse(l.Tick(10f));
            }
            Assert.AreEqual(3 * HalfFullLogic.PourSteps, l.Pours);
            Assert.IsTrue(l.OfferGlass());
            Assert.IsTrue(l.OfferGlass());
            AssertStillPlaying(l);
            Assert.AreEqual(HalfFullSlot.Empty, l.Slot);
        }

        [Test]
        public void FullDroppedElsewhere_ReturnsToSign()
        {
            var l = new HalfFullLogic();
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(HalfFullFullDrop.Returned, MoveFull(l, false));
                Assert.AreEqual(HalfFullFullAt.Sign, l.FullAt);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(4, l.FullReturns);
            // anche col buco già aperto
            MoveEmpty(l, false);
            Assert.AreEqual(HalfFullFullDrop.Returned, MoveFull(l, false));
            Assert.AreEqual(HalfFullSlot.Gap, l.Slot);
            Assert.AreEqual(HalfFullFullDrop.Placed, MoveFull(l, true));
        }

        [Test]
        public void EmptyDroppedInBubble_ReturnsToItsSlot()
        {
            var l = new HalfFullLogic();
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(HalfFullEmptyDrop.Returned, MoveEmpty(l, true));
                Assert.AreEqual(HalfFullEmptyAt.Bubble, l.EmptyAt);
                Assert.AreEqual(HalfFullSlot.Empty, l.Slot);
                AssertStillPlaying(l);
            }
            Assert.AreEqual(3, l.EmptyReturns);
        }

        [Test]
        public void EmptyOnFloor_CanBePutBack_Reversible()
        {
            var l = new HalfFullLogic();
            Assert.AreEqual(HalfFullEmptyDrop.Floor, MoveEmpty(l, false));
            Assert.AreEqual(HalfFullEmptyDrop.Floor, MoveEmpty(l, false));   // ripresa da terra e lasciata di nuovo fuori
            Assert.AreEqual(HalfFullEmptyDrop.Returned, MoveEmpty(l, true));  // rimessa nel fumetto
            Assert.AreEqual(HalfFullSlot.Empty, l.Slot);
            AssertStillPlaying(l);
            Assert.AreEqual(0, l.EmptyReturns, "rimetterla a posto da terra non è un errore");
        }

        [Test]
        public void ThreeOrMoreWrongAttempts_ThenSolution()
        {
            var l = new HalfFullLogic();
            l.Pour();
            l.OfferGlass();
            l.TapMan();
            l.TapSign();
            l.TapStuckWord();
            MoveEmpty(l, true);
            MoveFull(l, false);
            Assert.AreEqual(7, l.WrongMoves);
            AssertStillPlaying(l);
            MoveEmpty(l, false);
            Assert.AreEqual(HalfFullFullDrop.Placed, MoveFull(l, true));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void OnlyOneWordAtATime()
        {
            var l = new HalfFullLogic();
            Assert.IsTrue(l.GrabEmpty());
            Assert.IsFalse(l.GrabFull());
            Assert.IsFalse(l.GrabEmpty());
            Assert.AreEqual(HalfFullFullDrop.None, l.ReleaseFull(true));
            l.ReleaseEmpty(true);
            Assert.IsTrue(l.GrabFull());
            Assert.IsFalse(l.GrabEmpty());
            Assert.AreEqual(HalfFullEmptyDrop.None, l.ReleaseEmpty(false));
            Assert.AreEqual(HalfFullFullDrop.Returned, l.ReleaseFull(false));
        }

        [Test]
        public void ReleaseWithoutGrab_DoesNothing()
        {
            var l = new HalfFullLogic();
            Assert.AreEqual(HalfFullEmptyDrop.None, l.ReleaseEmpty(false));
            Assert.AreEqual(HalfFullFullDrop.None, l.ReleaseFull(true));
            Assert.AreEqual(HalfFullSlot.Empty, l.Slot);
            AssertStillPlaying(l);
        }

        [Test]
        public void RapidTouches_ManyGrabsAndReleases_StayValid()
        {
            var l = new HalfFullLogic();
            for (int i = 0; i < 50; i++)
            {
                l.GrabEmpty(); l.ReleaseEmpty(i % 2 == 0);
                l.GrabFull(); l.ReleaseFull(false);
                l.Pour(); l.TapMan(); l.TapSign();
                l.Tick(0.05f);
                AssertStillPlaying(l);
                Assert.IsFalse(l.Holding);
            }
        }

        [Test]
        public void Cancel_InEveryState_LeavesAValidState()
        {
            var l = new HalfFullLogic();
            Assert.IsFalse(l.Cancel());                       // niente in mano
            l.GrabEmpty();
            Assert.IsTrue(l.Cancel());
            Assert.AreEqual(HalfFullEmptyAt.Bubble, l.EmptyAt);   // torna nel fumetto
            l.GrabEmpty(); l.ReleaseEmpty(false);
            l.GrabEmpty();
            Assert.IsTrue(l.Cancel());
            Assert.AreEqual(HalfFullEmptyAt.Floor, l.EmptyAt);    // presa da terra: torna a terra
            l.GrabFull();
            Assert.IsTrue(l.Cancel());
            Assert.AreEqual(HalfFullFullAt.Sign, l.FullAt);
            Assert.IsFalse(l.Holding);
            l.Pour(); l.Pour(); l.Pour();                     // pausa mentre trabocca
            Assert.IsFalse(l.Cancel());
            Assert.IsTrue(l.Spilling);
            AssertStillPlaying(l);
            Assert.IsTrue(l.GrabFull());                      // si ricomincia normalmente
            Assert.AreEqual(HalfFullFullDrop.Placed, l.ReleaseFull(true));
            Assert.IsFalse(l.Cancel());                       // a festa iniziata la pausa non toglie la vittoria
            Assert.AreEqual(HalfFullSlot.Full, l.Slot);
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void Reset_FromAnyState_StartsOver()
        {
            var l = new HalfFullLogic();
            l.Pour(); l.Pour(); l.Pour();
            l.GrabEmpty(); l.ReleaseEmpty(false);
            l.GrabFull();
            l.Reset();
            AssertStillPlaying(l);
            Assert.AreEqual(HalfFullSlot.Empty, l.Slot);
            Assert.AreEqual(HalfFullFullAt.Sign, l.FullAt);
            Assert.IsFalse(l.Spilling);
            Assert.AreEqual(0, l.WrongMoves);

            MoveEmpty(l, false);
            MoveFull(l, true);
            Assert.IsTrue(l.TryComplete());
            l.Reset();                                         // uscita e rientro nel livello
            AssertStillPlaying(l);
            MoveEmpty(l, false);
            Assert.AreEqual(HalfFullFullDrop.Placed, MoveFull(l, true));
            Assert.IsTrue(l.TryComplete(), "dopo il reset si può vincere di nuovo (una volta)");
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void SpillTimer_IsNotAdvancedByZeroOrNegativeTime()
        {
            var l = new HalfFullLogic();
            l.Pour(); l.Pour(); l.Pour();
            Assert.IsFalse(l.Tick(0f));
            Assert.IsFalse(l.Tick(-5f));
            Assert.IsTrue(l.Spilling);
        }

        // ---------- Impaginazione dalle larghezze ----------

        [Test]
        public void Row_IsCentered_AndFollowsWidths()
        {
            float[] c = HalfFullLogic.Row(new[] { 1.0f, 1.5f, 0.2f }, new[] { 0f, 0.25f, 0.02f }, out float total);
            Assert.AreEqual(1.0f + 0.25f + 1.5f + 0.02f + 0.2f, total, 1e-5f);
            Assert.AreEqual(-total / 2f + 0.5f, c[0], 1e-5f);
            Assert.AreEqual(-total / 2f + 1.0f + 0.25f + 0.75f, c[1], 1e-5f);
            Assert.AreEqual(total / 2f - 0.1f, c[2], 1e-5f);
            // le parole non si sovrappongono mai
            Assert.LessOrEqual(c[0] + 0.5f, c[1] - 0.75f + 1e-5f);
            Assert.LessOrEqual(c[1] + 0.75f, c[2] - 0.1f + 1e-5f);
        }

        [Test]
        public void Row_LongAndShortTranslations_KeepTheSlotBetweenTheWords()
        {
            // "HALF EMPTY!" vs una traduzione lunga ("HALB LEER!" / "MEZZO VUOTO!" / parola lunghissima) vs una corta
            foreach (var (half, slot) in new[] { (0.9f, 1.4f), (1.8f, 3.1f), (0.3f, 0.2f) })
            {
                const float space = 0.2f, bangW = 0.15f;
                float[] c = HalfFullLogic.Row(new[] { half, slot, bangW }, new[] { 0f, space, 0.02f }, out float total);
                Assert.AreEqual(c[0] + half / 2f + space + slot / 2f, c[1], 1e-5f, "il posto segue la larghezza di HALF");
                Assert.AreEqual(c[1] + slot / 2f + 0.02f + bangW / 2f, c[2], 1e-5f, "il ! segue la larghezza del posto");
                Assert.AreEqual(0f, (c[0] - half / 2f) + (c[2] + bangW / 2f), 1e-4f, "riga centrata");
            }
        }

        [Test]
        public void SlotWidth_FollowsTheWordInIt_WithAMinimum()
        {
            Assert.AreEqual(1.4f, HalfFullLogic.SlotWidth(HalfFullSlot.Empty, 1.4f, 1.0f, 0.8f), 1e-5f);
            Assert.AreEqual(1.0f, HalfFullLogic.SlotWidth(HalfFullSlot.Full, 1.4f, 1.0f, 0.8f), 1e-5f);
            Assert.AreEqual(0.8f, HalfFullLogic.SlotWidth(HalfFullSlot.Gap, 1.4f, 1.0f, 0.8f), 1e-5f);
            Assert.AreEqual(HalfFullLogic.MinSlotWidth, HalfFullLogic.SlotWidth(HalfFullSlot.Gap, 1.4f, 1.0f, 0.1f), 1e-5f);
            Assert.AreEqual(3.5f, HalfFullLogic.SlotWidth(HalfFullSlot.Empty, 3.5f, 1.0f, 0.8f), 1e-5f);
        }

        [Test]
        public void DropZone_GrowsWithTheWordWidths()
        {
            const float line = 0.4f, mx = HalfFullLogic.SlotMarginX;
            // parola corta in un buco corto: la zona finisce poco oltre il margine
            Assert.IsTrue(HalfFullLogic.InSlot(0.5f / 2f + mx - 0.01f, 0f, 0f, 0f, 0.5f, 0.4f, line));
            Assert.IsFalse(HalfFullLogic.InSlot(0.5f / 2f + mx + 0.05f, 0f, 0f, 0f, 0.5f, 0.4f, line));
            // la stessa distanza dentro una zona per una parola lunga (traduzione) è dentro
            Assert.IsTrue(HalfFullLogic.InSlot(0.5f / 2f + mx + 0.05f, 0f, 0f, 0f, 0.5f, 2.4f, line));
            Assert.IsTrue(HalfFullLogic.InSlot(2.4f / 2f + mx - 0.01f, 0f, 0f, 0f, 0.5f, 2.4f, line));
            Assert.IsFalse(HalfFullLogic.InSlot(2.4f / 2f + mx + 0.01f, 0f, 0f, 0f, 0.5f, 2.4f, line));
            // ...e anche quando è largo il buco (EMPTY lunga ancora al suo posto)
            Assert.IsTrue(HalfFullLogic.InSlot(-(3f / 2f + mx - 0.01f), 0f, 0f, 0f, 3f, 0.4f, line));
            // in verticale: riga più margine
            Assert.IsTrue(HalfFullLogic.InSlot(0f, line / 2f + HalfFullLogic.SlotMarginY - 0.01f, 0f, 0f, 1f, 1f, line));
            Assert.IsFalse(HalfFullLogic.InSlot(0f, line / 2f + HalfFullLogic.SlotMarginY + 0.01f, 0f, 0f, 1f, 1f, line));
            // zona centrata sul posto, non sull'origine
            Assert.IsTrue(HalfFullLogic.InSlot(5.1f, 3.0f, 5f, 3f, 0.8f, 0.8f, line));
            Assert.IsFalse(HalfFullLogic.InSlot(0f, 0f, 5f, 3f, 0.8f, 0.8f, line));
        }

        [Test]
        public void InRect_Basics()
        {
            Assert.IsTrue(HalfFullLogic.InRect(0.9f, -0.4f, 0f, 0f, 1f, 0.5f));
            Assert.IsFalse(HalfFullLogic.InRect(1.1f, 0f, 0f, 0f, 1f, 0.5f));
            Assert.IsFalse(HalfFullLogic.InRect(0f, 0.6f, 0f, 0f, 1f, 0.5f));
        }
    }
}
