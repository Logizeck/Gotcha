using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Gotcha.Progress;

namespace Gotcha.Tests
{
    /// <summary>Progressi per identificativo e conversione dei salvataggi fino alla 0.75 (riordino della roadmap 1-300).</summary>
    public class ProgressLogicTests
    {
        [Test]
        public void LegacyOrderHas276UniqueIds()
        {
            Assert.AreEqual(276, LegacyOrder.V075.Length);
            Assert.AreEqual(276, LegacyOrder.V075.Distinct().Count());
            Assert.AreEqual("key_door", LegacyOrder.V075[0]);
            Assert.AreEqual("v75_locked_bike", LegacyOrder.V075[275]);
        }

        [Test]
        public void MigrationKeepsEverySolvedLevel()
        {
            Assert.IsEmpty(ProgressLogic.MigrateFromIndex(0, LegacyOrder.V075));          // nuovo giocatore
            CollectionAssert.AreEqual(new[] { "key_door" }, ProgressLogic.MigrateFromIndex(1, LegacyOrder.V075));
            List<string> mid = ProgressLogic.MigrateFromIndex(30, LegacyOrder.V075);       // era al livello 31
            Assert.AreEqual(30, mid.Count);
            CollectionAssert.AreEqual(LegacyOrder.V075.Take(30), mid);
            Assert.AreEqual(276, ProgressLogic.MigrateFromIndex(999, LegacyOrder.V075).Count);   // tutto finito
            Assert.IsEmpty(ProgressLogic.MigrateFromIndex(-3, LegacyOrder.V075));
        }

        [Test]
        public void NewEarlyLevelBecomesTheFrontierForVeterans()
        {
            var order = new[] { "a", "NEW", "b", "c", "d" };
            var done = new HashSet<string> { "a", "b", "c" };
            Assert.AreEqual(1, ProgressLogic.Frontier(order, done.Contains));
            Assert.IsTrue(ProgressLogic.IsOpen(2, order, done.Contains));    // già superati: restano aperti
            Assert.IsTrue(ProgressLogic.IsOpen(3, order, done.Contains));
            Assert.IsFalse(ProgressLogic.IsOpen(4, order, done.Contains));   // non superato e oltre la frontiera
            done.Add("NEW");
            Assert.AreEqual(4, ProgressLogic.Frontier(order, done.Contains));
            Assert.IsTrue(ProgressLogic.IsOpen(4, order, done.Contains));
        }

        [Test]
        public void NewPlayerSeesOnlyTheFirstLevel()
        {
            var order = new[] { "a", "b", "c" };
            var done = new HashSet<string>();
            Assert.AreEqual(0, ProgressLogic.Frontier(order, done.Contains));
            Assert.IsTrue(ProgressLogic.IsOpen(0, order, done.Contains));
            Assert.IsFalse(ProgressLogic.IsOpen(1, order, done.Contains));
            Assert.IsFalse(ProgressLogic.IsOpen(-1, order, done.Contains));
            Assert.IsFalse(ProgressLogic.IsOpen(3, order, done.Contains));
        }

        [Test]
        public void AllSolvedFrontierIsTheLastLevel()
        {
            var order = new[] { "a", "b" };
            var done = new HashSet<string> { "a", "b" };
            Assert.AreEqual(1, ProgressLogic.Frontier(order, done.Contains));
            Assert.AreEqual(0, ProgressLogic.Frontier(new string[0], done.Contains));
        }

        [Test]
        public void SavedListRoundTrips()
        {
            HashSet<string> set = ProgressLogic.Parse("a|b||c");
            Assert.AreEqual(3, set.Count);
            Assert.AreEqual(set, ProgressLogic.Parse(ProgressLogic.Join(set)));
            Assert.IsEmpty(ProgressLogic.Parse(null));
        }
    }
}
