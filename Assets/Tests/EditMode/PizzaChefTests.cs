using System;
using System.Collections.Generic;
using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Pizza Chef (0.83): pezzi sparsi a caso, ricetta senza ananas, ananas sempre sconfitta, uovo piccante.</summary>
    public class PizzaChefTests
    {
        static readonly string[] Fixed = { PizzaChefLogic.Tomato, PizzaChefLogic.Mozzarella, PizzaChefLogic.Ham, PizzaChefLogic.Artichoke };

        static float Dist(PizzaChefPiece a, PizzaChefPiece b) => (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        static void AddAll(PizzaChefLogic l, IEnumerable<string> ids)
        {
            foreach (string id in ids) Assert.AreEqual(PizzaChefAdd.Added, l.TryAdd(id), id);
        }

        /// <summary>Ricetta -> forno -> taglio -> servizio; restituisce il verdetto calcolato.</summary>
        static PizzaChefVerdict BakeCutServe(PizzaChefLogic l)
        {
            Assert.IsTrue(l.StartBake());
            Assert.IsTrue(l.BakeDone());
            Assert.IsTrue(l.CutDone());
            return l.ServedAll();
        }

        // ---------- pezzi sparsi ----------

        [Test]
        public void Scatter_StaysInsideDisc_AndIsRotated()
        {
            for (int seed = 0; seed < 300; seed++)
            {
                var l = new PizzaChefLogic(seed, Fixed);
                var rots = new HashSet<int>();
                foreach (string id in PizzaChefLogic.Ingredients)
                    foreach (PizzaChefPiece p in l.PlacePieces(id))
                    {
                        Assert.LessOrEqual(Math.Sqrt(p.X * p.X + p.Y * p.Y), PizzaChefLogic.ScatterRadius + 1e-4, "seed " + seed);
                        Assert.IsFalse(float.IsNaN(p.X) || float.IsNaN(p.Y));
                        Assert.That(p.Rotation, Is.InRange(0f, 360f));
                        rots.Add((int)(p.Rotation / 30f));
                    }
                Assert.Greater(rots.Count, 4, "rotazioni davvero casuali");
            }
        }

        [Test]
        public void Scatter_SingleIngredient_RespectsMinSpacing()
        {
            foreach (string id in PizzaChefLogic.Ingredients)
            {
                int n = PizzaChefLogic.PiecesFor(id);
                for (int seed = 0; seed < 300; seed++)
                {
                    var l = new PizzaChefLogic(seed, Fixed);
                    List<PizzaChefPiece> ps = l.PlacePieces(id);
                    Assert.AreEqual(n, ps.Count);
                    for (int i = 0; i < ps.Count; i++)
                        for (int j = i + 1; j < ps.Count; j++)
                            Assert.GreaterOrEqual(Dist(ps[i], ps[j]), PizzaChefLogic.MinSpacing, id + " seed " + seed);
                }
            }
        }

        [Test]
        public void Scatter_FullRecipe_RespectsSpacingAcrossIngredients()
        {
            // la ricetta più carica: 4 condimenti a pezzi + peperoncino (l'uovo): tutti a distanza minima
            string[] heavy = { PizzaChefLogic.Mushroom, PizzaChefLogic.Olive, PizzaChefLogic.Ham, PizzaChefLogic.Salami, PizzaChefLogic.Chili };
            for (int seed = 0; seed < 200; seed++)
            {
                var l = new PizzaChefLogic(seed, Fixed);
                foreach (string id in heavy) l.PlacePieces(id);
                IReadOnlyList<PizzaChefPiece> all = l.Pieces;
                float min = float.MaxValue;
                for (int i = 0; i < all.Count; i++)
                    for (int j = i + 1; j < all.Count; j++) min = Math.Min(min, Dist(all[i], all[j]));
                Assert.GreaterOrEqual(min, PizzaChefLogic.MinSpacing * 0.6f, "seed " + seed);
            }
        }

        [Test]
        public void Scatter_IsDeterministicPerSeed_AndDiffersBetweenSeeds()
        {
            List<PizzaChefPiece> a = new PizzaChefLogic(42, Fixed).PlacePieces(PizzaChefLogic.Salami);
            List<PizzaChefPiece> b = new PizzaChefLogic(42, Fixed).PlacePieces(PizzaChefLogic.Salami);
            List<PizzaChefPiece> c = new PizzaChefLogic(43, Fixed).PlacePieces(PizzaChefLogic.Salami);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].X, b[i].X);
                Assert.AreEqual(a[i].Y, b[i].Y);
                Assert.AreEqual(a[i].Rotation, b[i].Rotation);
            }
            bool differs = false;
            for (int i = 0; i < a.Count; i++) if (Math.Abs(a[i].X - c[i].X) > 1e-4f) differs = true;
            Assert.IsTrue(differs);
        }

        [Test]
        public void Scatter_CoversTheWholeDisc()
        {
            // uniforme nel disco: in tante partite cadono pezzi in tutti e quattro i quadranti e anche vicino al bordo
            int[] quad = new int[4];
            int nearEdge = 0;
            for (int seed = 0; seed < 100; seed++)
                foreach (PizzaChefPiece p in new PizzaChefLogic(seed, Fixed).PlacePieces(PizzaChefLogic.Olive))
                {
                    quad[(p.X >= 0 ? 0 : 1) + (p.Y >= 0 ? 0 : 2)]++;
                    if (Math.Sqrt(p.X * p.X + p.Y * p.Y) > PizzaChefLogic.ScatterRadius * 0.75) nearEdge++;
                }
            foreach (int q in quad) Assert.Greater(q, 150);
            Assert.Greater(nearEdge, 200);
        }

        // ---------- ricetta ----------

        [Test]
        public void Recipe_NeverContainsPineappleOrChili()
        {
            var rng = new Random(7);
            var seen = new HashSet<string>();
            for (int i = 0; i < 3000; i++)
            {
                string[] r = PizzaChefLogic.GenerateRecipe(rng);
                CollectionAssert.DoesNotContain(r, PizzaChefLogic.Pineapple);
                CollectionAssert.DoesNotContain(r, PizzaChefLogic.Chili);
                Assert.That(r.Length, Is.InRange(PizzaChefLogic.MinRecipe, PizzaChefLogic.MaxRecipe));
                CollectionAssert.AllItemsAreUnique(r);
                Assert.IsTrue(Array.IndexOf(r, PizzaChefLogic.Tomato) >= 0 || Array.IndexOf(r, PizzaChefLogic.Mozzarella) >= 0);
                foreach (string s in r) seen.Add(s);
            }
            // i nuovi ingredienti escono davvero sulla lavagna
            CollectionAssert.IsSubsetOf(new[] { PizzaChefLogic.Ham, PizzaChefLogic.Salami, PizzaChefLogic.Artichoke }, seen);
            for (int seed = 0; seed < 500; seed++) CollectionAssert.DoesNotContain(new PizzaChefLogic(seed).Required, PizzaChefLogic.Pineapple);
        }

        [Test]
        public void Ingredients_IncludeTheNewOnes_AndHaveLabels()
        {
            CollectionAssert.IsSubsetOf(new[] { "ham", "salami", "artichoke" }, PizzaChefLogic.Ingredients);
            Assert.AreEqual(PizzaChefLogic.Ingredients.Length, PizzaChefLogic.Labels.Length);
            Assert.AreEqual("ARTICHOKES", PizzaChefLogic.LabelOf("artichoke"));
            foreach (string id in PizzaChefLogic.RecipePool) CollectionAssert.Contains(PizzaChefLogic.Ingredients, id);
        }

        // ---------- ananas ----------

        static void AssertPineappleFails(PizzaChefLogic l)
        {
            Assert.AreEqual(PizzaChefAdd.Pineapple, l.TryAdd(PizzaChefLogic.Pineapple));
            Assert.IsTrue(l.PineappleFailed);
            Assert.IsTrue(l.IsDone);
            // dopo: niente vittoria, niente uovo, niente di niente
            Assert.AreEqual(PizzaChefAdd.Ignored, l.TryAdd(PizzaChefLogic.Pineapple));
            Assert.AreEqual(PizzaChefAdd.Ignored, l.TryAdd(PizzaChefLogic.Chili));
            Assert.IsFalse(l.StartBake());
            Assert.IsFalse(l.BakeDone());
            Assert.IsFalse(l.CutDone());
            Assert.AreEqual(PizzaChefVerdict.None, l.ServedAll());
            Assert.AreEqual(PizzaChefVerdict.None, l.Commit());
            Assert.AreEqual(PizzaChefVerdict.None, l.Outcome);
        }

        [Test]
        public void Pineapple_First_Fails()
        {
            AssertPineappleFails(new PizzaChefLogic(1, Fixed));
        }

        [Test]
        public void Pineapple_AfterTheExactRecipe_Fails()
        {
            var l = new PizzaChefLogic(2, Fixed);
            AddAll(l, Fixed);
            Assert.AreEqual(PizzaChefVerdict.Win, l.Evaluate());
            AssertPineappleFails(l);
        }

        [Test]
        public void Pineapple_InEveryPhase_Fails()
        {
            for (int stop = 0; stop <= 4; stop++)
            {
                var l = new PizzaChefLogic(3 + stop, Fixed);
                AddAll(l, Fixed);
                if (stop >= 1) Assert.IsTrue(l.StartBake());   // in forno
                if (stop >= 2) Assert.IsTrue(l.BakeDone());    // cotta
                if (stop >= 3) Assert.IsTrue(l.CutDone());     // tagliata
                if (stop >= 4) Assert.AreEqual(PizzaChefVerdict.Win, l.ServedAll());   // anche durante la festa, prima dell'esito
                AssertPineappleFails(l);
            }
        }

        [Test]
        public void Pineapple_WithChili_FailsAndNoEgg()
        {
            var l = new PizzaChefLogic(9, Fixed);
            AddAll(l, Fixed);
            Assert.AreEqual(PizzaChefAdd.Added, l.TryAdd(PizzaChefLogic.Chili));
            AssertPineappleFails(l);
            Assert.AreNotEqual(PizzaChefVerdict.SpicyEgg, l.Outcome);
        }

        [Test]
        public void Pineapple_DuringEggAnimation_StopsTheEgg()
        {
            var l = new PizzaChefLogic(10, Fixed);
            AddAll(l, Fixed);
            l.TryAdd(PizzaChefLogic.Chili);
            Assert.AreEqual(PizzaChefVerdict.SpicyEgg, BakeCutServe(l));
            AssertPineappleFails(l);
        }

        [Test]
        public void Taunts_RotateAndAreNeverEmpty()
        {
            Assert.GreaterOrEqual(PizzaChefLogic.Taunts.Length, 3);
            var seen = new HashSet<string>();
            for (int t = -5; t < 20; t++) { Assert.IsNotEmpty(PizzaChefLogic.Taunt(t)); seen.Add(PizzaChefLogic.Taunt(t)); }
            Assert.AreEqual(PizzaChefLogic.Taunts.Length, seen.Count);
            Assert.AreNotEqual(PizzaChefLogic.Taunt(0), PizzaChefLogic.Taunt(1));
        }

        // ---------- percorso giusto, errori, uovo ----------

        [Test]
        public void CorrectRecipe_Wins_Once()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var l = new PizzaChefLogic(seed);
                foreach (string id in l.Required) { Assert.AreEqual(PizzaChefAdd.Added, l.TryAdd(id)); l.PlacePieces(id); }
                Assert.AreEqual(PizzaChefVerdict.Win, BakeCutServe(l));
                Assert.AreEqual(PizzaChefVerdict.Win, l.Commit());
                Assert.AreEqual(PizzaChefVerdict.Win, l.Outcome);
                Assert.AreEqual(PizzaChefVerdict.None, l.Commit(), "una volta sola");
                Assert.AreEqual(PizzaChefAdd.Ignored, l.TryAdd(PizzaChefLogic.Pineapple), "dopo la vittoria l'ananas non cambia l'esito");
                Assert.IsFalse(l.PineappleFailed);
            }
        }

        [Test]
        public void MissingOrExtra_IsANormalFail()
        {
            var l = new PizzaChefLogic(5, Fixed);
            l.TryAdd(PizzaChefLogic.Tomato);
            Assert.AreEqual(PizzaChefVerdict.Missing, BakeCutServe(l));
            Assert.AreEqual(PizzaChefVerdict.Missing, l.Commit());

            l = new PizzaChefLogic(6, Fixed);
            AddAll(l, Fixed);
            l.TryAdd(PizzaChefLogic.Salami);
            Assert.AreEqual(PizzaChefVerdict.Extra, BakeCutServe(l));
            Assert.AreEqual(PizzaChefVerdict.Extra, l.Commit());
        }

        [Test]
        public void Chili_IsTheSpicyEgg()
        {
            var l = new PizzaChefLogic(8, Fixed);
            AddAll(l, Fixed);
            Assert.AreEqual(PizzaChefAdd.Added, l.TryAdd(PizzaChefLogic.Chili));
            Assert.AreEqual(PizzaChefVerdict.SpicyEgg, BakeCutServe(l));
            Assert.AreEqual(PizzaChefVerdict.SpicyEgg, l.Commit());
            Assert.AreEqual(PizzaChefVerdict.None, l.Commit());
            Assert.IsFalse(l.PineappleFailed);

            // anche solo peperoncino
            l = new PizzaChefLogic(11, Fixed);
            l.TryAdd(PizzaChefLogic.Chili);
            Assert.AreEqual(PizzaChefVerdict.SpicyEgg, BakeCutServe(l));
        }

        [Test]
        public void NoToppings_CannotBake_AndLateToppingsAreRejected()
        {
            var l = new PizzaChefLogic(12, Fixed);
            Assert.IsFalse(l.StartBake());
            l.TryAdd(PizzaChefLogic.Tomato);
            Assert.AreEqual(PizzaChefAdd.Duplicate, l.TryAdd(PizzaChefLogic.Tomato));
            Assert.IsTrue(l.StartBake());
            Assert.AreEqual(PizzaChefAdd.TooLate, l.TryAdd(PizzaChefLogic.Chili));
            Assert.IsTrue(l.BakeDone());
            Assert.AreEqual(PizzaChefAdd.TooLate, l.TryAdd(PizzaChefLogic.Ham));
            Assert.IsFalse(l.Has(PizzaChefLogic.Chili));
        }

        [Test]
        public void PhasesCannotBeSkipped()
        {
            var l = new PizzaChefLogic(13, Fixed);
            Assert.IsFalse(l.BakeDone());
            Assert.IsFalse(l.CutDone());
            Assert.AreEqual(PizzaChefVerdict.None, l.ServedAll());
            Assert.AreEqual(PizzaChefVerdict.None, l.Commit());
            Assert.AreEqual(PizzaChefPhase.Toppings, l.Phase);
        }
    }
}
