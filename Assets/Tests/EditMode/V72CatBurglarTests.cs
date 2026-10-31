using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 269: il gatto scassinatore (le false piste non costano niente, il gatto guidato dal laser butta giù il barattolo).</summary>
    public class V72CatBurglarTests
    {
        const float Eps = 1e-4f;

        /// <summary>Fa passare il tempo a piccoli passi (come i fotogrammi), fermandosi al primo evento uguale a "until".</summary>
        static bool RunUntil(CatBurglarLogic l, CatEvent until, float seconds, float step = 0.05f)
        {
            for (float t = 0f; t < seconds - 1e-5f; t += step)
                if (l.Tick(step) == until) return true;
            return false;
        }

        static void Run(CatBurglarLogic l, float seconds, float step = 0.05f)
        {
            for (float t = 0f; t < seconds - 1e-5f; t += step) l.Tick(step);
        }

        /// <summary>Il puntino sopra il ripiano p (un po' più in alto della superficie, come lo metterebbe un dito).</summary>
        static DotResult DotOn(CatBurglarLogic l, CatPerch p, float x = float.NaN)
        {
            if (float.IsNaN(x)) x = (CatBurglarLogic.MinX(p) + CatBurglarLogic.MaxX(p)) * 0.5f;
            return l.PointDot(x, CatBurglarLogic.SurfaceY(p) + 0.2f);
        }

        /// <summary>Guida il gatto un ripiano alla volta fino alla mensola.</summary>
        static void LeadToShelf(CatBurglarLogic l)
        {
            if (!l.LaserOn) Assert.AreEqual(LaserResult.On, l.ToggleLaser());
            for (int i = (int)l.Perch + 1; i <= (int)CatPerch.Shelf; i++)
            {
                Assert.AreEqual(DotResult.Chase, DotOn(l, (CatPerch)i), "ripiano " + (CatPerch)i);
                Assert.IsTrue(RunUntil(l, CatEvent.Landed, 2f), "il gatto atterra su " + (CatPerch)i);
                Assert.AreEqual((CatPerch)i, l.Perch);
            }
        }

        static void AssertJarStillThere(CatBurglarLogic l)
        {
            Assert.AreEqual(CookieJarStage.OnShelf, l.JarStage);
            Assert.AreEqual(CatBurglarLogic.JarStartX, l.JarX, Eps);
            Assert.IsFalse(l.Solved);
            Assert.IsFalse(l.InputLocked);
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void Starts_CatOnTheFloor_JarOnTheShelf_LaserOff()
        {
            var l = new CatBurglarLogic();
            AssertJarStillThere(l);
            Assert.AreEqual(CatPerch.Floor, l.Perch);
            Assert.AreEqual(CatBurglarLogic.CatStartX, l.CatX, Eps);
            Assert.IsFalse(l.LaserOn);
            Assert.IsFalse(l.CushionUnder);
            Assert.AreEqual(CatPerch.None, l.DotPerch);
            Assert.AreEqual(0, l.WrongMoves);
            Run(l, 10f);
            AssertJarStillThere(l);
            Assert.AreEqual(CatPerch.Floor, l.Perch);
        }

        [Test]
        public void WrongMoves_NeverChangeAnything()
        {
            var l = new CatBurglarLogic();
            Assert.AreEqual(1, l.LadderUnderShelf());
            Assert.AreEqual(2, l.LadderUnderShelf());
            Assert.AreEqual(1, l.StackBooks());
            Assert.AreEqual(1, l.TapJar());
            Assert.AreEqual(1, l.Shake());
            Assert.AreEqual(2, l.Shake());
            Assert.AreEqual(1, l.DragCatUp());
            Run(l, 5f);
            AssertJarStillThere(l);
            Assert.AreEqual(CatPerch.Floor, l.Perch);
            Assert.AreEqual(7, l.WrongMoves);
        }

        [Test]
        public void WrongMoves_KeepPartialProgress()
        {
            var l = new CatBurglarLogic();
            Assert.IsTrue(l.SetCushion(true));
            LeadToShelf(l);
            // il gatto spinge un po'...
            Assert.IsTrue(RunUntil(l, CatEvent.Nudge, 3f));
            Run(l, 0.3f);
            float jar = l.JarX;
            Assert.Greater(jar, CatBurglarLogic.JarStartX);
            // ...e le false piste non toccano niente: né il gatto, né il barattolo, né il cuscino
            l.LadderUnderShelf();
            l.StackBooks();
            l.TapJar();
            l.Shake();
            l.DragCatUp();
            Assert.AreEqual(CatPerch.Shelf, l.Perch);
            Assert.AreEqual(jar, l.JarX, Eps);
            Assert.IsTrue(l.CushionUnder);
            Assert.IsTrue(l.LaserOn);
            Assert.AreEqual(CookieJarStage.OnShelf, l.JarStage);
        }

        [Test]
        public void Laser_TogglesAndThePerkCountsAsTheClue()
        {
            var l = new CatBurglarLogic();
            Assert.AreEqual(DotResult.None, DotOn(l, CatPerch.Chair), "senza laser il puntino non c'è");
            Assert.AreEqual(LaserResult.On, l.ToggleLaser());
            Assert.AreEqual(1, l.Perks);
            Assert.AreEqual(CatPerch.Floor, l.DotPerch, "il puntino compare accanto al gatto");
            Assert.AreNotEqual(l.CatX, l.DotX);
            Assert.AreEqual(LaserResult.Off, l.ToggleLaser());
            Assert.AreEqual(CatPerch.None, l.DotPerch);
            Run(l, 3f);
            Assert.AreEqual(CatPerch.Floor, l.Perch);
            Assert.AreEqual(LaserResult.On, l.ToggleLaser());
            Assert.AreEqual(2, l.Perks);
        }

        [Test]
        public void Dot_SnapsGenerouslyToTheNearestPerch()
        {
            for (int i = 0; i < CatBurglarLogic.PerchCount; i++)
            {
                var p = (CatPerch)i;
                float mid = (CatBurglarLogic.MinX(p) + CatBurglarLogic.MaxX(p)) * 0.5f;
                Assert.AreEqual(p, CatBurglarLogic.NearestPerch(mid, CatBurglarLogic.SurfaceY(p) + 0.45f, out float x), "sopra " + p);
                Assert.AreEqual(mid, x, Eps);
                Assert.AreEqual(p, CatBurglarLogic.NearestPerch(mid, CatBurglarLogic.SurfaceY(p) - 0.15f, out _), "appena sotto il bordo di " + p);
            }
            // fuori dal ripiano: la x viene riportata dentro
            Assert.AreEqual(CatPerch.Shelf, CatBurglarLogic.NearestPerch(2.4f, 1.2f, out float sx));
            Assert.AreEqual(CatBurglarLogic.MaxX(CatPerch.Shelf), sx, Eps);
            // tutto quello che sta davanti, in basso, è pavimento
            Assert.AreEqual(CatPerch.Floor, CatBurglarLogic.NearestPerch(0.3f, -4.6f, out _));
            // in alto a sinistra, sul muro, lontano da tutto: niente
            Assert.AreEqual(CatPerch.None, CatBurglarLogic.NearestPerch(-1.9f, 2.6f, out _));
            Assert.AreEqual(CatPerch.None, CatBurglarLogic.NearestPerch(float.NaN, 0f, out _));
        }

        [Test]
        public void Reachable_OneStepUp_AnyStepDown()
        {
            Assert.IsTrue(CatBurglarLogic.Reachable(CatPerch.Floor, CatPerch.Chair));
            Assert.IsFalse(CatBurglarLogic.Reachable(CatPerch.Floor, CatPerch.Table));
            Assert.IsFalse(CatBurglarLogic.Reachable(CatPerch.Floor, CatPerch.Shelf));
            Assert.IsTrue(CatBurglarLogic.Reachable(CatPerch.Cupboard, CatPerch.Shelf));
            Assert.IsTrue(CatBurglarLogic.Reachable(CatPerch.Shelf, CatPerch.Floor));
            Assert.IsTrue(CatBurglarLogic.Reachable(CatPerch.Table, CatPerch.Chair));
            Assert.IsFalse(CatBurglarLogic.Reachable(CatPerch.Table, CatPerch.Table));
            Assert.IsFalse(CatBurglarLogic.Reachable(CatPerch.None, CatPerch.Floor));
        }

        [Test]
        public void Dot_TooHigh_TheCatStaysPut()
        {
            var l = new CatBurglarLogic();
            l.ToggleLaser();
            Assert.AreEqual(DotResult.TooHigh, DotOn(l, CatPerch.Shelf));
            Assert.AreEqual(1, l.TooHighs);
            Assert.AreEqual(DotResult.TooHigh, DotOn(l, CatPerch.Shelf, 1.2f), "sullo stesso ripiano non si conta di nuovo");
            Assert.AreEqual(1, l.TooHighs);
            Run(l, 3f);
            Assert.AreEqual(CatPerch.Floor, l.Perch);
            Assert.IsFalse(l.Jumping);
            Assert.AreEqual(DotResult.TooHigh, DotOn(l, CatPerch.Cupboard));
            Run(l, 3f);
            Assert.AreEqual(CatPerch.Floor, l.Perch);
            AssertJarStillThere(l);
        }

        [Test]
        public void Dot_OnTheWall_TheCatOnlyWatches()
        {
            var l = new CatBurglarLogic();
            l.ToggleLaser();
            Assert.AreEqual(DotResult.Watching, l.PointDot(-1.9f, 2.6f));
            Run(l, 3f);
            Assert.AreEqual(CatPerch.Floor, l.Perch);
            Assert.AreEqual(CatBurglarLogic.CatStartX, l.CatX, Eps);
        }

        [Test]
        public void Dot_SamePerch_TheCatWalksToIt_InsideThePerch()
        {
            var l = new CatBurglarLogic();
            l.ToggleLaser();
            Assert.AreEqual(DotResult.SamePerch, l.PointDot(1.5f, -3.4f));
            Run(l, 2f);
            Assert.AreEqual(1.5f, l.CatX, Eps);
            Assert.AreEqual(DotResult.SamePerch, l.PointDot(2.6f, -4.4f));
            Run(l, 2f);
            Assert.AreEqual(CatBurglarLogic.MaxX(CatPerch.Floor), l.CatX, Eps);
        }

        [Test]
        public void Dot_LeadsTheCatThroughThePerches_AndBackDown()
        {
            var l = new CatBurglarLogic();
            l.ToggleLaser();
            Assert.AreEqual(DotResult.Chase, DotOn(l, CatPerch.Chair));
            Assert.IsTrue(RunUntil(l, CatEvent.Pounce, 1f));
            Assert.IsTrue(l.Jumping);
            Assert.AreEqual(CatPerch.Floor, l.JumpFrom);
            Assert.AreEqual(CatPerch.Chair, l.JumpTo);
            Assert.IsTrue(RunUntil(l, CatEvent.Landed, 1f));
            Assert.AreEqual(CatPerch.Chair, l.Perch);
            Assert.IsFalse(l.Jumping);
            Assert.That(l.CatX, Is.InRange(CatBurglarLogic.MinX(CatPerch.Chair), CatBurglarLogic.MaxX(CatPerch.Chair)));
            // il puntino passato velocemente di lì non basta: serve un attimo di sosta
            DotOn(l, CatPerch.Table);
            l.Tick(0.05f);
            DotOn(l, CatPerch.Shelf);
            Run(l, 1f);
            Assert.AreEqual(CatPerch.Chair, l.Perch);
            Assert.IsFalse(l.Jumping);
            LeadToShelf(l);
            Assert.AreEqual(CatPerch.Shelf, l.Perch);
            Assert.LessOrEqual(l.CatX, l.JarX - CatBurglarLogic.PushGap + Eps, "sulla mensola resta a sinistra del barattolo");
            // giù dove vuole, in un salto solo
            Assert.AreEqual(DotResult.Chase, DotOn(l, CatPerch.Floor));
            Assert.IsTrue(RunUntil(l, CatEvent.Landed, 2f));
            Assert.AreEqual(CatPerch.Floor, l.Perch);
        }

        [Test]
        public void OnTheShelf_TheCatPushesTheJarSlowly_ToTheEdge()
        {
            var l = new CatBurglarLogic();
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.StartedPushing, 2f));
            Assert.IsTrue(l.Pushing);
            float t = 0f;
            int nudges = 0;
            float last = l.JarX;
            CatEvent ev = CatEvent.None;
            while (t < 10f && ev != CatEvent.JarTipped)
            {
                ev = l.Tick(0.05f);
                t += 0.05f;
                if (ev == CatEvent.Nudge) nudges++;
                Assert.GreaterOrEqual(l.JarX, last - Eps, "il barattolo va solo verso il bordo");
                last = l.JarX;
            }
            Assert.AreEqual(CatEvent.JarTipped, ev);
            Assert.AreEqual(CookieJarStage.Falling, l.JarStage);
            Assert.Greater(t, 2f, "lentamente");
            Assert.Less(t, 6f);
            Assert.GreaterOrEqual(nudges, 4);
        }

        [Test]
        public void JarWithoutCushion_Shatters_ThenANewJar_NoSoftlock()
        {
            var l = new CatBurglarLogic();
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.JarTipped, 10f));
            Assert.IsTrue(RunUntil(l, CatEvent.JarShattered, 2f));
            Assert.AreEqual(CookieJarStage.Broken, l.JarStage);
            Assert.AreEqual(1, l.Shatters);
            Assert.IsFalse(l.Solved);
            Assert.IsFalse(l.TryComplete());
            // il gatto salta giù sulla credenza, il puntino lo segue
            Assert.IsTrue(l.Jumping);
            Assert.AreEqual(CatPerch.Cupboard, l.JumpTo);
            Assert.AreEqual(CatPerch.Cupboard, l.DotPerch);
            Assert.IsTrue(RunUntil(l, CatEvent.NewJar, 4f));
            AssertJarStillThere(l);
            Assert.AreEqual(CatPerch.Cupboard, l.Perch);
            Assert.IsTrue(l.LaserOn, "il laser resta acceso");
            // si riprova: cuscino sotto, di nuovo sulla mensola, e stavolta si vince
            Assert.IsTrue(l.SetCushion(true));
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.JarOnCushion, 12f));
            Assert.IsTrue(l.TryComplete());
        }

        [Test]
        public void WhileBroken_TheCatIgnoresTheDot()
        {
            var l = new CatBurglarLogic();
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.JarShattered, 12f));
            Assert.IsTrue(RunUntil(l, CatEvent.Landed, 2f));
            Assert.AreEqual(DotResult.Chase, DotOn(l, CatPerch.Shelf));
            Run(l, 1f);
            Assert.AreEqual(CatPerch.Cupboard, l.Perch, "aspetta il barattolo nuovo");
            Assert.IsTrue(RunUntil(l, CatEvent.NewJar, 3f));
            Assert.IsTrue(RunUntil(l, CatEvent.Landed, 2f), "poi il puntino sulla mensola lo richiama");
            Assert.AreEqual(CatPerch.Shelf, l.Perch);
        }

        [Test]
        public void WithCushion_SoftLanding_WinsOnce()
        {
            var l = new CatBurglarLogic();
            Assert.IsTrue(CatBurglarLogic.InCushionZone(CatBurglarLogic.LandX - 0.6f, -3.4f));
            Assert.IsFalse(CatBurglarLogic.InCushionZone(-0.5f, -4.3f));
            Assert.IsFalse(CatBurglarLogic.InCushionZone(CatBurglarLogic.LandX, 0.5f), "non sulla mensola");
            Assert.IsTrue(l.SetCushion(true));
            Assert.IsFalse(l.SetCushion(true), "già lì");
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.JarTipped, 10f));
            Assert.IsFalse(l.TryComplete(), "ancora in volo");
            Assert.IsTrue(RunUntil(l, CatEvent.JarOnCushion, 2f));
            Assert.AreEqual(CookieJarStage.Landed, l.JarStage);
            Assert.IsTrue(l.Solved);
            Assert.IsTrue(l.InputLocked);
            Assert.AreEqual(0, l.Shatters);
            // tutto bloccato
            Assert.AreEqual(LaserResult.None, l.ToggleLaser());
            Assert.AreEqual(DotResult.None, DotOn(l, CatPerch.Floor));
            Assert.IsFalse(l.SetCushion(false));
            Assert.AreEqual(0, l.LadderUnderShelf());
            Assert.AreEqual(0, l.DragCatUp());
            Assert.AreEqual(CatEvent.None, l.Tick(1f));
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete(), "una volta sola");
            Assert.AreEqual(CookieJarStage.Done, l.JarStage);
        }

        [Test]
        public void CushionMovedAwayMidPush_Shatters_PutBackMidFall_Lands()
        {
            var l = new CatBurglarLogic();
            l.SetCushion(true);
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.Nudge, 3f));
            Assert.IsTrue(l.SetCushion(false));
            Assert.IsTrue(RunUntil(l, CatEvent.JarShattered, 10f));
            Assert.IsFalse(l.Solved);

            // il cuscino conta nel momento in cui il barattolo arriva giù: rimesso a metà caduta, lo salva
            var m = new CatBurglarLogic();
            LeadToShelf(m);
            Assert.IsTrue(RunUntil(m, CatEvent.JarTipped, 10f));
            m.Tick(0.1f);
            Assert.IsTrue(m.SetCushion(true));
            Assert.IsTrue(RunUntil(m, CatEvent.JarOnCushion, 2f));
        }

        [Test]
        public void CatLeavesMidPush_JarKeepsItsPlace_AndCatResumes()
        {
            var l = new CatBurglarLogic();
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.Nudge, 3f));
            Run(l, 0.3f);
            float jar = l.JarX;
            Assert.AreEqual(DotResult.Chase, DotOn(l, CatPerch.Cupboard));
            Assert.IsTrue(RunUntil(l, CatEvent.Landed, 2f));
            Assert.AreEqual(CatPerch.Cupboard, l.Perch);
            Assert.AreEqual(jar, l.JarX, Eps, "il barattolo resta dov'era");
            Assert.IsFalse(l.Pushing);
            Assert.AreEqual(DotResult.Chase, DotOn(l, CatPerch.Shelf));
            Assert.IsTrue(RunUntil(l, CatEvent.Landed, 2f));
            Assert.IsTrue(RunUntil(l, CatEvent.JarTipped, 10f), "riprende a spingere da dove era rimasto");
        }

        [Test]
        public void LaserOffOnTheShelf_TheCatKeepsPushing()
        {
            var l = new CatBurglarLogic();
            LeadToShelf(l);
            Assert.AreEqual(LaserResult.Off, l.ToggleLaser());
            Assert.IsTrue(RunUntil(l, CatEvent.JarTipped, 10f));
        }

        [Test]
        public void LongPause_TickIsClamped_NoTeleport()
        {
            var l = new CatBurglarLogic();
            l.ToggleLaser();
            DotOn(l, CatPerch.Chair);
            Assert.AreEqual(CatEvent.Pounce, l.Tick(100f));
            Assert.AreEqual(CatEvent.None, l.Tick(0.1f));
            Assert.IsTrue(l.Jumping, "un passo lungo non finisce il salto in un colpo");
            Assert.AreEqual(CatEvent.None, l.Tick(float.NaN));
            Assert.AreEqual(CatEvent.None, l.Tick(-1f));
            Assert.IsTrue(RunUntil(l, CatEvent.Landed, 1f));
        }

        [Test]
        public void Reset_RestoresTheStart()
        {
            var l = new CatBurglarLogic();
            l.SetCushion(true);
            l.LadderUnderShelf();
            l.Shake();
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.JarOnCushion, 12f));
            Assert.IsTrue(l.TryComplete());
            l.Reset();
            AssertJarStillThere(l);
            Assert.AreEqual(CatPerch.Floor, l.Perch);
            Assert.AreEqual(CatBurglarLogic.CatStartX, l.CatX, Eps);
            Assert.IsFalse(l.LaserOn);
            Assert.IsFalse(l.CushionUnder);
            Assert.IsFalse(l.Jumping);
            Assert.IsFalse(l.Pushing);
            Assert.AreEqual(0, l.WrongMoves);
            Assert.AreEqual(0, l.Perks);
            Assert.AreEqual(0, l.Shatters);
            Assert.IsFalse(l.CompletionReported);
            LeadToShelf(l);
            Assert.IsTrue(RunUntil(l, CatEvent.JarShattered, 12f), "dopo il reset il cuscino non c'è più");
        }
    }
}
