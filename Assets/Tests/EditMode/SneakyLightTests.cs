using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Sneaky Mouse: la luce della torcia si ferma sugli ostacoli e il topo si scopre solo nella zona illuminata.</summary>
    public class SneakyLightTests
    {
        const float Tol = 1e-3f;

        // torcia in alto (0, 3) che punta in giù; una scatola 1 x 1 centrata in (0, 0)
        static readonly SneakyLightBox[] OneBox = { new SneakyLightBox(0f, 0f, 0.5f, 0.5f) };

        [Test]
        public void Direction_FollowsSignedAngleFromDown()
        {
            SneakyLightGeometry.Direction(0f, out float dx, out float dy);
            Assert.AreEqual(0f, dx, Tol); Assert.AreEqual(-1f, dy, Tol);
            SneakyLightGeometry.Direction(90f, out dx, out dy);
            Assert.AreEqual(1f, dx, Tol); Assert.AreEqual(0f, dy, Tol);
            SneakyLightGeometry.Direction(-90f, out dx, out dy);
            Assert.AreEqual(-1f, dx, Tol); Assert.AreEqual(0f, dy, Tol);
            Assert.AreEqual(90f, SneakyLightGeometry.AngleOf(1f, 0f), Tol);
            Assert.AreEqual(-45f, SneakyLightGeometry.AngleOf(-1f, -1f), Tol);
            Assert.AreEqual(20f, SneakyLightGeometry.DeltaAngle(170f, -170f), Tol);
        }

        [Test]
        public void RayBox_HitsFrontFace()
        {
            Assert.IsTrue(SneakyLightGeometry.RayBox(0f, 3f, 0f, -1f, OneBox[0], out float t));
            Assert.AreEqual(2.5f, t, Tol);
            // in diagonale verso lo spigolo
            Assert.IsTrue(SneakyLightGeometry.RayBox(-3f, 0.2f, 1f, 0f, OneBox[0], out t));
            Assert.AreEqual(2.5f, t, Tol);
        }

        [Test]
        public void RayBox_MissesSideBehindAndParallel()
        {
            Assert.IsFalse(SneakyLightGeometry.RayBox(1f, 3f, 0f, -1f, OneBox[0], out _));        // passa di fianco
            Assert.IsFalse(SneakyLightGeometry.RayBox(0f, 3f, 0f, 1f, OneBox[0], out _));         // va dall'altra parte
            Assert.IsFalse(SneakyLightGeometry.RayBox(-3f, 0.6f, 1f, 0f, OneBox[0], out _));      // parallelo, sopra
        }

        [Test]
        public void RayBox_OriginInsideIsZero()
        {
            Assert.IsTrue(SneakyLightGeometry.RayBox(0.1f, 0.1f, 0f, -1f, OneBox[0], out float t));
            Assert.AreEqual(0f, t, Tol);
        }

        [Test]
        public void CastRay_StopsAtNearestObstacle()
        {
            var boxes = new[] { new SneakyLightBox(0f, -2f, 0.5f, 0.5f), new SneakyLightBox(0f, 0f, 0.5f, 0.5f) };
            Assert.AreEqual(2.5f, SneakyLightGeometry.CastRay(0f, 3f, 0f, -1f, 10f, boxes, 2), Tol);
            Assert.AreEqual(10f, SneakyLightGeometry.CastRay(0f, 3f, 1f, 0f, 10f, boxes, 2), Tol);
            Assert.AreEqual(4f, SneakyLightGeometry.CastRay(0f, 3f, 0f, -1f, 4f, null, 0), Tol);
            // count limita gli ostacoli usati
            Assert.AreEqual(10f, SneakyLightGeometry.CastRay(0f, 3f, 0f, -1f, 10f, boxes, 0), Tol);
        }

        [Test]
        public void IsLitExact_ShadowBehindObstacle()
        {
            Assert.IsTrue(SneakyLightGeometry.IsLitExact(0f, 1f, 0f, 3f, 0f, 20f, 7f, OneBox, 1));     // davanti
            Assert.IsFalse(SneakyLightGeometry.IsLitExact(0f, -1.5f, 0f, 3f, 0f, 20f, 7f, OneBox, 1)); // dietro: ombra
            Assert.IsFalse(SneakyLightGeometry.IsLitExact(0f, 0f, 0f, 3f, 0f, 20f, 7f, OneBox, 1));    // dentro
            Assert.IsTrue(SneakyLightGeometry.IsLitExact(1.2f, -1.5f, 0f, 3f, 0f, 20f, 7f, OneBox, 1)); // a lato dell'ombra
            Assert.IsFalse(SneakyLightGeometry.IsLitExact(3f, 0f, 0f, 3f, 0f, 20f, 7f, OneBox, 1));    // fuori dal cono
            Assert.IsFalse(SneakyLightGeometry.IsLitExact(0f, -5f, 0f, 3f, 0f, 20f, 7f, null, 0));     // oltre la portata
            Assert.IsTrue(SneakyLightGeometry.IsLitExact(3f, 0f, 0f, 3f, 45f, 20f, 7f, null, 0));      // girata verso destra
        }

        [Test]
        public void Fan_HitPointsStopOnBox()
        {
            var fan = new SneakyLightFan(40);
            fan.Build(0f, 3f, 0f, 20f, 7f, OneBox, 1);
            Assert.AreEqual(41, fan.HitX.Length);
            Assert.AreEqual(2.5f, fan.HitDistance[20], Tol);                 // raggio centrale: fronte della scatola
            Assert.AreEqual(0.5f, fan.HitY[20], Tol);
            Assert.AreEqual(7f, fan.HitDistance[0], Tol);                    // raggi ai bordi: liberi
            Assert.AreEqual(7f, fan.HitDistance[40], Tol);
            Assert.AreEqual(-20f, fan.RayAngle(0), Tol);
            Assert.AreEqual(20f, fan.RayAngle(40), Tol);
        }

        [Test]
        public void Fan_ContainsMatchesTheRule()
        {
            var fan = new SneakyLightFan(96);
            fan.Build(0f, 3f, 0f, 20f, 7f, OneBox, 1);
            Assert.IsTrue(fan.Contains(0f, 1f));
            Assert.IsFalse(fan.Contains(0f, -1.5f));          // ombra della scatola
            Assert.IsFalse(fan.Contains(0f, 0f));             // dentro la scatola
            Assert.IsTrue(fan.Contains(1.2f, -1.5f));
            Assert.IsFalse(fan.Contains(3f, 0f));
            Assert.IsFalse(fan.Contains(0f, -4.5f));          // oltre i 7 di portata
        }

        [Test]
        public void Fan_AgreesWithExactTestAwayFromEdges()
        {
            var boxes = new[]
            {
                new SneakyLightBox(-0.6f, 0.2f, 0.5f, 0.45f), new SneakyLightBox(1.1f, -1.2f, 1.15f, 0.5f),
                new SneakyLightBox(0f, -4.75f, 3.2f, 0.5f),
            };
            var fan = new SneakyLightFan(96);
            for (int k = 0; k < 7; k++)
            {
                float look = -60f + k * 20f;
                fan.Build(0f, 2.72f, look, 20f, 7f, boxes, boxes.Length);
                int checkedPoints = 0;
                for (float x = -2.6f; x <= 2.6f; x += 0.13f)
                    for (float y = -4.6f; y <= 2.5f; y += 0.13f)
                    {
                        if (NearAnEdge(x, y, 0f, 2.72f, look, boxes)) continue;
                        Assert.AreEqual(SneakyLightGeometry.IsLitExact(x, y, 0f, 2.72f, look, 20f, 7f, boxes, boxes.Length),
                            fan.Contains(x, y), "look " + look + " at " + x + ", " + y);
                        checkedPoints++;
                    }
                Assert.Greater(checkedPoints, 500);
            }
        }

        // i bordi delle ombre e del cono sono approssimati dai raggi: lì il confronto non è significativo
        static bool NearAnEdge(float x, float y, float ox, float oy, float look, SneakyLightBox[] boxes)
        {
            const float m = 0.08f;
            float a = SneakyLightGeometry.DeltaAngle(look, SneakyLightGeometry.AngleOf(x - ox, y - oy));
            if (System.Math.Abs(System.Math.Abs(a) - 20f) < 1.5f) return true;
            float d = (float)System.Math.Sqrt((x - ox) * (x - ox) + (y - oy) * (y - oy));
            if (System.Math.Abs(d - 7f) < m) return true;
            foreach (SneakyLightBox b in boxes)
            {
                if (System.Math.Abs(System.Math.Abs(x - b.CenterX) - b.HalfW) < m || System.Math.Abs(System.Math.Abs(y - b.CenterY) - b.HalfH) < m) return true;
                // le rette d'ombra passano dagli spigoli: si scarta la fascia angolare attorno a ciascuno
                foreach (float sx in new[] { -1f, 1f })
                    foreach (float sy in new[] { -1f, 1f })
                    {
                        float c = SneakyLightGeometry.AngleOf(b.CenterX + sx * b.HalfW - ox, b.CenterY + sy * b.HalfH - oy);
                        if (System.Math.Abs(SneakyLightGeometry.DeltaAngle(c, SneakyLightGeometry.AngleOf(x - ox, y - oy))) < 1.5f) return true;
                    }
            }
            return false;
        }

        [Test]
        public void Fan_OriginInsideObstacleLightsNothing()
        {
            var fan = new SneakyLightFan(16);
            fan.Build(0f, 0f, 0f, 20f, 7f, OneBox, 1);
            Assert.IsFalse(fan.Contains(0f, -2f));
            Assert.IsFalse(new SneakyLightFan(8).Contains(0f, -1f));   // mai costruito: niente luce
        }

        [Test]
        public void Fan_HiddenInWallHoleIsDark()
        {
            // il muro in basso con la tana: il topo nella tana (dentro il muro) non si vede mai, appena fuori sì
            var wall = new[] { new SneakyLightBox(0f, -4.75f, 3.2f, 0.5f) };
            var fan = new SneakyLightFan(96);
            for (float look = -60f; look <= 60f; look += 5f)
            {
                fan.Build(0f, 2.72f, look, 20f, 7f, wall, 1);
                Assert.IsFalse(fan.Contains(-1.6f, -4.45f), "look " + look);
            }
            fan.Build(0f, 2.72f, -13f, 20f, 7f, wall, 1);
            Assert.IsTrue(fan.Contains(-1.6f, -3.6f));
        }
    }
}
