using System;
using NUnit.Framework;
using Gotcha.Levels.V63;

namespace Gotcha.Tests
{
    /// <summary>Livello 258: le ombre cinesi per spaventare il cane (zone, distanza, sagome, errori).</summary>
    public class V63ShadowPuppetTests
    {
        static readonly PuppetItem[] Items = { PuppetItem.Yarn, PuppetItem.PizzaA, PuppetItem.PizzaB, PuppetItem.Feather, PuppetItem.Apple, PuppetItem.Boot, PuppetItem.Paw };

        static void Cat(ShadowPuppetLogic l)
        {
            l.TryPlace(PuppetItem.Yarn, PuppetZone.Head, out _);
            l.TryPlace(PuppetItem.PizzaA, PuppetZone.EarLeft, out _);
            l.TryPlace(PuppetItem.PizzaB, PuppetZone.EarRight, out _);
            l.TryPlace(PuppetItem.Feather, PuppetZone.Tail, out _);
        }

        [Test]
        public void BigCatShadow_ScaresTheDog()
        {
            var l = new ShadowPuppetLogic();
            Cat(l);
            Assert.IsTrue(l.TrySetDistance(PuppetDistance.Near));
            Assert.AreEqual(Silhouette.BigCat, l.TryShow());
            Assert.AreEqual(PuppetState.DogFlees, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(0, l.Strikes);
            Assert.IsTrue(l.TryCompleteFlee());
            Assert.IsFalse(l.TryCompleteFlee());
            Assert.AreEqual(PuppetState.Done, l.State);
        }

        [Test]
        public void SmallCat_IsAKitten_AndCountsAsWrong()
        {
            var l = new ShadowPuppetLogic();
            Cat(l);
            Assert.AreEqual(Silhouette.Kitten, l.TryShow());   // lontano dalla lampada
            Assert.AreEqual(1, l.Strikes);
            Assert.IsTrue(l.TryFinishShow());
            Assert.AreEqual(PuppetState.Composing, l.State);
            l.TrySetDistance(PuppetDistance.Mid);
            Assert.AreEqual(Silhouette.Kitten, l.TryShow());   // a metà: ancora piccolo
            l.TryFinishShow();
            l.TrySetDistance(PuppetDistance.Near);
            Assert.AreEqual(Silhouette.BigCat, l.TryShow());
            Assert.AreEqual(2, l.Strikes);
        }

        [Test]
        public void OtherShapes_GetTheirOwnReaction()
        {
            const PuppetDistance N = PuppetDistance.Near;
            Assert.AreEqual(Silhouette.Empty, ShadowPuppetLogic.Classify(PuppetItem.None, PuppetItem.None, PuppetItem.None, PuppetItem.None, N));
            Assert.AreEqual(Silhouette.Rabbit, ShadowPuppetLogic.Classify(PuppetItem.Yarn, PuppetItem.Feather, PuppetItem.Paw, PuppetItem.None, N));
            Assert.AreEqual(Silhouette.Rabbit, ShadowPuppetLogic.Classify(PuppetItem.Apple, PuppetItem.Paw, PuppetItem.Feather, PuppetItem.PizzaA, N));
            Assert.AreEqual(Silhouette.Pizza, ShadowPuppetLogic.Classify(PuppetItem.PizzaA, PuppetItem.None, PuppetItem.PizzaB, PuppetItem.None, N));
            Assert.AreEqual(Silhouette.Pizza, ShadowPuppetLogic.Classify(PuppetItem.None, PuppetItem.PizzaA, PuppetItem.PizzaB, PuppetItem.None, N));
            Assert.AreEqual(Silhouette.AppleCat, ShadowPuppetLogic.Classify(PuppetItem.Apple, PuppetItem.PizzaA, PuppetItem.PizzaB, PuppetItem.Feather, N));
            Assert.AreEqual(Silhouette.TaillessCat, ShadowPuppetLogic.Classify(PuppetItem.Yarn, PuppetItem.PizzaA, PuppetItem.PizzaB, PuppetItem.None, N));
            Assert.AreEqual(Silhouette.WeirdTail, ShadowPuppetLogic.Classify(PuppetItem.Yarn, PuppetItem.PizzaB, PuppetItem.PizzaA, PuppetItem.Paw, N));
            Assert.AreEqual(Silhouette.Ball, ShadowPuppetLogic.Classify(PuppetItem.Yarn, PuppetItem.None, PuppetItem.None, PuppetItem.Feather, N));
            Assert.AreEqual(Silhouette.Boot, ShadowPuppetLogic.Classify(PuppetItem.Yarn, PuppetItem.PizzaA, PuppetItem.PizzaB, PuppetItem.Boot, N));
            Assert.AreEqual(Silhouette.ModernArt, ShadowPuppetLogic.Classify(PuppetItem.Feather, PuppetItem.Yarn, PuppetItem.None, PuppetItem.Apple, N));
            foreach (Silhouette s in Enum.GetValues(typeof(Silhouette)))
                Assert.IsFalse(string.IsNullOrEmpty(ShadowPuppetLogic.Reaction(s)));
            Assert.AreEqual("Aww, a kitten! Wanna play?", ShadowPuppetLogic.Reaction(Silhouette.Kitten));
            Assert.AreEqual("A rabbit? I EAT rabbits.", ShadowPuppetLogic.Reaction(Silhouette.Rabbit));
            Assert.AreEqual("That's a pizza. Now I'm hungry.", ShadowPuppetLogic.Reaction(Silhouette.Pizza));
        }

        [Test]
        public void OnlyTheBigCat_Wins()
        {
            // tutte le disposizioni (ogni zona vuota o con un oggetto diverso) a tutte le distanze: vince solo il gatto vicino
            var opts = new PuppetItem[Items.Length + 1];
            Array.Copy(Items, 0, opts, 1, Items.Length);
            int wins = 0;
            foreach (PuppetDistance d in Enum.GetValues(typeof(PuppetDistance)))
            foreach (PuppetItem a in opts)
            foreach (PuppetItem b in opts)
            foreach (PuppetItem c in opts)
            foreach (PuppetItem e in opts)
            {
                if (Dup(a, b, c, e)) continue;
                if (ShadowPuppetLogic.Classify(a, b, c, e, d) != Silhouette.BigCat) continue;
                wins++;
                Assert.AreEqual(PuppetDistance.Near, d);
                Assert.AreEqual(PuppetItem.Yarn, a);
                Assert.AreEqual(PuppetItem.Feather, e);
            }
            Assert.AreEqual(2, wins);   // le due fette di pizza si possono scambiare
        }

        static bool Dup(params PuppetItem[] s)
        {
            for (int i = 0; i < s.Length; i++)
                for (int j = i + 1; j < s.Length; j++)
                    if (s[i] != PuppetItem.None && s[i] == s[j]) return true;
            return false;
        }

        [Test]
        public void EmptyWall_DoesNotCount()
        {
            var l = new ShadowPuppetLogic();
            Assert.AreEqual(Silhouette.Empty, l.TryShow());
            Assert.AreEqual(0, l.Strikes);
            Assert.IsTrue(l.TryFinishShow());
            Assert.AreEqual(PuppetState.Composing, l.State);
        }

        [Test]
        public void ThreeWrongShadows_Fail()
        {
            var l = new ShadowPuppetLogic();
            l.TryPlace(PuppetItem.Boot, PuppetZone.Head, out _);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(PuppetState.Composing, l.State);
                Assert.AreEqual(Silhouette.Boot, l.TryShow());
                Assert.IsTrue(l.TryFinishShow());
            }
            Assert.AreEqual(3, l.Strikes);
            Assert.AreEqual(PuppetState.Failed, l.State);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(Silhouette.Empty, l.TryShow());
            Assert.IsFalse(l.TryPlace(PuppetItem.Yarn, PuppetZone.Head, out _));
            Assert.IsFalse(l.TrySetDistance(PuppetDistance.Near));
        }

        [Test]
        public void Placing_MovesAndSwaps_AndLocksWhileShowing()
        {
            var l = new ShadowPuppetLogic();
            Assert.IsTrue(l.TryPlace(PuppetItem.Apple, PuppetZone.Head, out PuppetItem d));
            Assert.AreEqual(PuppetItem.None, d);
            Assert.IsTrue(l.TryPlace(PuppetItem.Yarn, PuppetZone.Head, out d));
            Assert.AreEqual(PuppetItem.Apple, d);
            Assert.AreEqual(-1, l.ZoneOf(PuppetItem.Apple));
            Assert.IsTrue(l.TryPlace(PuppetItem.Yarn, PuppetZone.Tail, out d));
            Assert.AreEqual(PuppetItem.None, l.ZoneItem(PuppetZone.Head));
            Assert.IsTrue(l.TryRemove(PuppetItem.Yarn));
            Assert.IsFalse(l.TryRemove(PuppetItem.Yarn));
            Assert.IsFalse(l.TryPlace(PuppetItem.None, PuppetZone.Head, out _));
            l.TryPlace(PuppetItem.Paw, PuppetZone.Head, out _);
            l.TryShow();
            Assert.IsFalse(l.TryPlace(PuppetItem.Yarn, PuppetZone.EarLeft, out _));
            Assert.IsFalse(l.TryRemove(PuppetItem.Paw));
            Assert.IsFalse(l.TrySetDistance(PuppetDistance.Mid));
            Assert.AreEqual(Silhouette.Empty, l.TryShow());
        }

        [Test]
        public void Reset_FromEveryState()
        {
            foreach (PuppetState target in Enum.GetValues(typeof(PuppetState)))
            {
                var l = new ShadowPuppetLogic();
                Cat(l);
                switch (target)
                {
                    case PuppetState.Showing: l.TryShow(); break;
                    case PuppetState.DogFlees: l.TrySetDistance(PuppetDistance.Near); l.TryShow(); break;
                    case PuppetState.Done: l.TrySetDistance(PuppetDistance.Near); l.TryShow(); l.TryCompleteFlee(); break;
                    case PuppetState.Failed: for (int i = 0; i < 3; i++) { l.TryShow(); l.TryFinishShow(); } break;
                }
                Assert.AreEqual(target, l.State);
                l.Reset();
                Assert.AreEqual(PuppetState.Composing, l.State);
                Assert.AreEqual(PuppetDistance.Far, l.Distance);
                Assert.AreEqual(0, l.Strikes);
                Assert.AreEqual(0, l.Shows);
                Assert.IsFalse(l.CompletionReported);
                for (int z = 0; z < ShadowPuppetLogic.Zones; z++) Assert.AreEqual(PuppetItem.None, l.ZoneItem((PuppetZone)z));
            }
        }

        [Test]
        public void RandomSequences_NeverBlock([Values(1, 2, 3, 4, 5, 6)] int seed, [Values(8, 90, 400)] int steps)
        {
            var rnd = new Random(seed);
            var l = new ShadowPuppetLogic();
            int completions = 0;
            for (int step = 0; step < steps; step++)
            {
                switch (rnd.Next(8))
                {
                    case 0:
                    case 1: l.TryPlace(Items[rnd.Next(Items.Length)], (PuppetZone)rnd.Next(4), out _); break;
                    case 2: l.TryRemove(Items[rnd.Next(Items.Length)]); break;
                    case 3: l.TrySetDistance((PuppetDistance)rnd.Next(3)); break;
                    case 4: if (rnd.Next(4) == 0) l.TryShow(); break;
                    case 5: l.TryFinishShow(); break;
                    case 6: if (l.TryCompleteFlee()) completions++; break;
                    default: if (rnd.Next(40) == 0) { l.Reset(); completions = 0; } break;
                }
                Assert.LessOrEqual(completions, 1);
                Assert.LessOrEqual(l.Strikes, ShadowPuppetLogic.MaxStrikes);
                Assert.AreEqual(l.State == PuppetState.Done, l.CompletionReported);
                if (l.State == PuppetState.Failed) Assert.AreEqual(ShadowPuppetLogic.MaxStrikes, l.Strikes);
                foreach (PuppetItem it in Items)
                {
                    int n = 0;
                    for (int z = 0; z < ShadowPuppetLogic.Zones; z++) if (l.ZoneItem((PuppetZone)z) == it) n++;
                    Assert.LessOrEqual(n, 1);
                }
            }
            // nessun blocco: dal punto in cui si è, il gatto gigante vince (dopo aver ricominciato, se si era perso)
            if (l.State == PuppetState.Showing) l.TryFinishShow();
            if (l.State == PuppetState.Failed) l.Reset();
            if (l.State == PuppetState.Composing)
            {
                foreach (PuppetItem it in Items) l.TryRemove(it);
                Cat(l);
                Assert.IsTrue(l.TrySetDistance(PuppetDistance.Near));
                Assert.AreEqual(Silhouette.BigCat, l.TryShow());
            }
            if (l.State == PuppetState.DogFlees) Assert.IsTrue(l.TryCompleteFlee());
            Assert.AreEqual(PuppetState.Done, l.State);
        }
    }
}
