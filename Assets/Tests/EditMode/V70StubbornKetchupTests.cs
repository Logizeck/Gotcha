using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 268: il ketchup testardo (era ancora sigillato: tappo via, sigillo via, poi esce).</summary>
    public class V70StubbornKetchupTests
    {
        static void AssertUntouched(StubbornKetchupLogic l)
        {
            Assert.AreEqual(KetchupStage.Playing, l.Stage);
            Assert.IsTrue(l.CapOn);
            Assert.IsTrue(l.Sealed);
            Assert.IsFalse(l.Open);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        static void AllWrongMoves(StubbornKetchupLogic l)
        {
            Assert.AreEqual(KetchupResult.Nothing, l.TipOver());
            Assert.IsTrue(l.Tipped);
            Assert.AreEqual(KetchupResult.Nothing, l.Shake());
            Assert.AreEqual(KetchupResult.Nothing, l.Squeeze());
            Assert.AreEqual(KetchupResult.Nothing, l.Slap());
            Assert.AreEqual(KetchupResult.Nothing, l.Slap());
            l.Upright();
            Assert.AreEqual(KetchupResult.Nothing, l.Shake());
            Assert.AreEqual(KetchupResult.Nothing, l.Squeeze());
            Assert.AreEqual(KnifeResult.Clean, l.Knife());
        }

        [Test]
        public void StartsClosedAndSealed()
        {
            var l = new StubbornKetchupLogic();
            AssertUntouched(l);
            Assert.IsFalse(l.Tipped);
            Assert.AreEqual(0f, l.CapProgress);
            Assert.AreEqual(0, l.WrongMoves);
        }

        [Test]
        public void EachWrongMove_KeepsStateAndCounts()
        {
            var l = new StubbornKetchupLogic();
            AllWrongMoves(l);
            AssertUntouched(l);
            Assert.AreEqual(1, l.Tips);
            Assert.AreEqual(2, l.Shakes);
            Assert.AreEqual(2, l.Squeezes);
            Assert.AreEqual(2, l.Slaps);
            Assert.AreEqual(1, l.Knifes);
            Assert.AreEqual(8, l.WrongMoves);
        }

        [Test]
        public void WrongMoves_DoNotLosePartialProgress()
        {
            var l = new StubbornKetchupLogic();
            Assert.IsFalse(l.TapCap());
            Assert.AreEqual(0.5f, l.CapProgress, 1e-4f);
            AllWrongMoves(l);
            l.Upright();
            Assert.AreEqual(0.5f, l.CapProgress, 1e-4f);
            Assert.IsTrue(l.TapCap());
            Assert.IsFalse(l.CapOn);
            // tappo tolto: le false piste non rimettono niente a posto
            Assert.AreEqual(KetchupResult.Nothing, l.TipOver());
            Assert.AreEqual(KetchupResult.Nothing, l.Shake());
            Assert.AreEqual(KnifeResult.Foil, l.Knife());
            Assert.IsFalse(l.CapOn);
            Assert.IsTrue(l.Sealed);
        }

        [Test]
        public void TwoTapsOrOneFullTurn_RemoveTheCap()
        {
            var l = new StubbornKetchupLogic();
            Assert.IsFalse(l.TapCap());
            Assert.IsTrue(l.TapCap());
            Assert.IsFalse(l.CapOn);

            var c = new StubbornKetchupLogic();
            Assert.IsFalse(c.TurnCap(100f));
            Assert.IsFalse(c.TurnCap(-150f));       // qualunque verso
            Assert.IsTrue(c.CapOn);
            Assert.IsTrue(c.TurnCap(120f));
            Assert.IsFalse(c.CapOn);
            Assert.AreEqual(1f, c.CapProgress);
            Assert.IsFalse(c.TurnCap(500f));        // già tolto
        }

        [Test]
        public void CapCannotBeTurnedWhileTipped()
        {
            var l = new StubbornKetchupLogic();
            l.TipOver();
            Assert.IsFalse(l.TapCap());
            Assert.IsFalse(l.TapCap());
            Assert.IsTrue(l.CapOn);
            l.Upright();
            Assert.IsFalse(l.TapCap());
            Assert.IsTrue(l.TapCap());
        }

        [Test]
        public void CapOffAndOn_AnyOrder_NoSoftlock()
        {
            var l = new StubbornKetchupLogic();
            Assert.IsFalse(l.PutCapBack());          // è già su
            for (int i = 0; i < 3; i++)
            {
                l.TapCap();
                Assert.IsTrue(l.TapCap());
                Assert.IsFalse(l.CapOn);
                Assert.IsTrue(l.PutCapBack());
                Assert.IsTrue(l.CapOn);
                Assert.AreEqual(0f, l.CapProgress);
                Assert.IsTrue(l.Sealed);
            }
            Assert.AreEqual(3, l.CapsRemoved);
            Assert.AreEqual(3, l.CapsReplaced);
            // sigillo tolto, tappo rimesso, ritolto: tutto sempre possibile
            l.TapCap(); l.TapCap();
            Assert.AreEqual(PeelResult.Peeled, l.Peel(1f));
            Assert.IsTrue(l.PutCapBack());
            Assert.IsTrue(l.Open);
            l.TapCap();
            Assert.IsTrue(l.TapCap());
            Assert.IsTrue(l.Open);
        }

        [Test]
        public void Peel_OnlyWithCapOff()
        {
            var l = new StubbornKetchupLogic();
            Assert.AreEqual(PeelResult.CapInTheWay, l.Peel(2f));
            Assert.AreEqual(PeelResult.CapInTheWay, l.Peel(2f));
            Assert.IsTrue(l.Sealed);
            Assert.AreEqual(2, l.TabTugs);
            l.TapCap(); l.TapCap();
            Assert.AreEqual(PeelResult.Partial, l.Peel(StubbornKetchupLogic.PeelDistance * 0.5f));
            Assert.IsTrue(l.Sealed);
            Assert.AreEqual(PeelResult.Peeled, l.Peel(StubbornKetchupLogic.PeelDistance));
            Assert.IsFalse(l.Sealed);
            Assert.AreEqual(PeelResult.None, l.Peel(5f));
            Assert.AreEqual(KetchupStage.Playing, l.Stage);
        }

        [Test]
        public void Ketchup_OnlyAfterTheSealIsGone()
        {
            var l = new StubbornKetchupLogic();
            l.TapCap(); l.TapCap();
            // tappo tolto ma sigillo ancora lì: niente
            Assert.AreEqual(KetchupResult.Nothing, l.TipOver());
            Assert.AreEqual(KetchupResult.Nothing, l.Squeeze());
            Assert.AreEqual(KetchupResult.Nothing, l.Shake());
            Assert.AreEqual(KetchupResult.Nothing, l.Slap());
            Assert.AreEqual(KetchupStage.Playing, l.Stage);
            l.Upright();
            Assert.AreEqual(PeelResult.Peeled, l.Peel(1f));
            Assert.AreEqual(KetchupResult.Pour, l.TipOver());
            Assert.AreEqual(KetchupStage.Pouring, l.Stage);
            Assert.IsTrue(l.InputLocked);
        }

        [Test]
        public void OpenBottle_PoursWithAnyReasonableMove()
        {
            foreach (int way in new[] { 0, 1, 2, 3 })
            {
                var l = new StubbornKetchupLogic();
                l.TapCap(); l.TapCap();
                l.Peel(1f);
                if (way == 0) Assert.AreEqual(KetchupResult.Pour, l.Squeeze());         // anche dritto: va sulle patatine
                else
                {
                    // dritto, scuotere o sbattere non basta (deve essere sopra le patatine)
                    if (way == 1) Assert.AreEqual(KetchupResult.Nothing, l.Shake());
                    if (way == 2) Assert.AreEqual(KetchupResult.Nothing, l.Slap());
                    Assert.AreEqual(KetchupStage.Playing, l.Stage);
                    Assert.AreEqual(KetchupResult.Pour, l.TipOver());
                }
                Assert.AreEqual(KetchupStage.Pouring, l.Stage);
            }
            var s = new StubbornKetchupLogic();
            s.TapCap(); s.TapCap(); s.Peel(1f); s.PutCapBack();
            s.Upright();
            // capovolto (sigillo già tolto, il tappo ha il beccuccio): scuotere fa uscire il ketchup
            var t = new StubbornKetchupLogic();
            t.TapCap(); t.TapCap();
            t.TipOver();
            Assert.AreEqual(KetchupResult.Nothing, t.Shake());
            t.Upright();
            t.Peel(1f);
            t.TipOver();
            Assert.AreEqual(KetchupStage.Pouring, t.Stage);
            Assert.AreEqual(KetchupResult.Pour, s.TipOver());   // anche con il tappo rimesso
        }

        [Test]
        public void ShakeAndSlap_PourWhenTippedAndOpen()
        {
            var a = new StubbornKetchupLogic();
            a.TapCap(); a.TapCap(); a.Peel(1f);
            a.TipOver();
            Assert.AreEqual(KetchupStage.Pouring, a.Stage);
            Assert.AreEqual(KetchupResult.None, a.Shake());      // già in corso: niente doppioni
            Assert.AreEqual(KetchupResult.None, a.Slap());
            Assert.AreEqual(KetchupResult.None, a.Squeeze());
            Assert.AreEqual(KetchupResult.None, a.TipOver());
        }

        [Test]
        public void WinsOnlyOnce()
        {
            var l = new StubbornKetchupLogic();
            Assert.IsFalse(l.TryComplete());
            l.TapCap(); l.TapCap(); l.Peel(1f);
            l.Squeeze();
            Assert.IsTrue(l.TryComplete());
            Assert.AreEqual(KetchupStage.Done, l.Stage);
            Assert.IsFalse(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
            Assert.AreEqual(KetchupResult.None, l.Squeeze());
            Assert.IsFalse(l.TapCap());
            Assert.IsFalse(l.PutCapBack());
            Assert.AreEqual(KnifeResult.None, l.Knife());
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = new StubbornKetchupLogic();
            AllWrongMoves(l);
            l.Upright();
            l.TapCap(); l.TapCap(); l.Peel(1f);
            l.Squeeze();
            l.TryComplete();
            l.Reset();
            AssertUntouched(l);
            Assert.IsFalse(l.Tipped);
            Assert.AreEqual(0f, l.CapProgress);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.CapsRemoved);
            Assert.IsFalse(l.CompletionReported);
            // dopo il reset si vince di nuovo, una volta
            l.TapCap(); l.TapCap(); l.Peel(1f);
            Assert.AreEqual(KetchupResult.Pour, l.TipOver());
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }
    }
}
