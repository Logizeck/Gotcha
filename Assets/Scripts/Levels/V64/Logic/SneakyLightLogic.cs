using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Ostacolo per la luce del livello "Sneaky Mouse": un rettangolo allineato agli assi (centro e mezze misure).</summary>
    public struct SneakyLightBox
    {
        public float CenterX, CenterY, HalfW, HalfH;

        public SneakyLightBox(float centerX, float centerY, float halfW, float halfH)
        {
            CenterX = centerX;
            CenterY = centerY;
            HalfW = Math.Abs(halfW);
            HalfH = Math.Abs(halfH);
        }

        public bool Contains(float x, float y) => Math.Abs(x - CenterX) <= HalfW && Math.Abs(y - CenterY) <= HalfH;
    }

    /// <summary>
    /// Geometria pura della torcia di "Sneaky Mouse" (niente Unity): raggio contro rettangolo, lancio di un raggio
    /// contro tutti gli ostacoli, test esatto "il punto è illuminato?". Convenzione degli angoli: gradi misurati da
    /// "giù" (0, -1), positivi in senso antiorario, come Vector2.SignedAngle(Vector2.down, d) e come la rotazione z
    /// del perno della torcia.
    /// </summary>
    public static class SneakyLightGeometry
    {
        const float Deg2Rad = (float)(Math.PI / 180.0);
        const float Eps = 1e-6f;

        /// <summary>Direzione (unitaria) di un raggio con l'angolo dato.</summary>
        public static void Direction(float degrees, out float dx, out float dy)
        {
            float a = degrees * Deg2Rad;
            dx = (float)Math.Sin(a);
            dy = -(float)Math.Cos(a);
        }

        /// <summary>Angolo (da "giù", antiorario) del vettore (dx, dy), in [-180, 180].</summary>
        public static float AngleOf(float dx, float dy) => (float)(Math.Atan2(dx, -dy) / Deg2Rad);

        /// <summary>Differenza più corta fra due angoli, in [-180, 180].</summary>
        public static float DeltaAngle(float from, float to)
        {
            float d = (to - from) % 360f;
            if (d > 180f) d -= 360f;
            if (d < -180f) d += 360f;
            return d;
        }

        /// <summary>
        /// Distanza lungo il raggio (origine o, direzione unitaria d) a cui entra nel rettangolo. Se l'origine è già
        /// dentro il rettangolo la distanza è 0. False se il raggio non lo tocca mai (in avanti).
        /// </summary>
        public static bool RayBox(float ox, float oy, float dx, float dy, SneakyLightBox box, out float distance)
        {
            distance = 0f;
            float tMin = float.NegativeInfinity, tMax = float.PositiveInfinity;
            if (!Slab(ox, dx, box.CenterX - box.HalfW, box.CenterX + box.HalfW, ref tMin, ref tMax)) return false;
            if (!Slab(oy, dy, box.CenterY - box.HalfH, box.CenterY + box.HalfH, ref tMin, ref tMax)) return false;
            if (tMax < 0f || tMin > tMax) return false;
            distance = Math.Max(tMin, 0f);
            return true;
        }

        static bool Slab(float o, float d, float min, float max, ref float tMin, ref float tMax)
        {
            if (Math.Abs(d) < Eps) return o >= min && o <= max;   // parallelo: tocca solo se è già dentro la fascia
            float t1 = (min - o) / d, t2 = (max - o) / d;
            if (t1 > t2) { float s = t1; t1 = t2; t2 = s; }
            if (t1 > tMin) tMin = t1;
            if (t2 < tMax) tMax = t2;
            return tMin <= tMax;
        }

        /// <summary>Fin dove arriva un raggio: la distanza del primo ostacolo, o maxLength se non ne tocca nessuno.</summary>
        public static float CastRay(float ox, float oy, float dx, float dy, float maxLength, SneakyLightBox[] boxes, int count)
        {
            float best = maxLength;
            if (boxes == null) return best;
            int n = Math.Min(count, boxes.Length);
            for (int i = 0; i < n; i++)
                if (RayBox(ox, oy, dx, dy, boxes[i], out float t) && t < best) best = t;
            return Math.Max(0f, best);
        }

        /// <summary>
        /// Test esatto (un raggio dritto verso il punto): dentro il cono, entro la portata e senza ostacoli prima.
        /// Un punto DENTRO un ostacolo è al buio (il raggio si ferma sul bordo dell'ostacolo).
        /// </summary>
        public static bool IsLitExact(float px, float py, float ox, float oy, float look, float halfAngle, float length,
            SneakyLightBox[] boxes, int count)
        {
            float vx = px - ox, vy = py - oy;
            float dist = (float)Math.Sqrt(vx * vx + vy * vy);
            if (dist > length) return false;
            if (dist < Eps) return CastRay(ox, oy, 0f, -1f, length, boxes, count) > 0f;
            if (Math.Abs(DeltaAngle(look, AngleOf(vx, vy))) > halfAngle) return false;
            return CastRay(ox, oy, vx / dist, vy / dist, length, boxes, count) >= dist;
        }
    }

    /// <summary>
    /// Il ventaglio di luce della torcia: Rays + 1 raggi equidistanti da look - halfAngle a look + halfAngle, ognuno
    /// fermato dal primo ostacolo. Lo stesso poligono si disegna (mesh a ventaglio: origine + punti d'arrivo) e si usa
    /// per scoprire il topo (<see cref="Contains"/>), così quello che si vede e la regola coincidono esattamente.
    /// Gli array si allocano una sola volta nel costruttore.
    /// </summary>
    public sealed class SneakyLightFan
    {
        public readonly int Rays;
        /// <summary>Punti d'arrivo dei raggi (Rays + 1 elementi) e loro distanza dall'origine.</summary>
        public readonly float[] HitX, HitY, HitDistance;
        public float OriginX { get; private set; }
        public float OriginY { get; private set; }
        public float Look { get; private set; }
        public float HalfAngle { get; private set; }
        public float Length { get; private set; }
        public bool Built { get; private set; }

        public SneakyLightFan(int rays)
        {
            Rays = Math.Max(2, rays);
            HitX = new float[Rays + 1];
            HitY = new float[Rays + 1];
            HitDistance = new float[Rays + 1];
        }

        /// <summary>Angolo del raggio i (0..Rays).</summary>
        public float RayAngle(int i) => Look - HalfAngle + 2f * HalfAngle * i / Rays;

        public void Build(float ox, float oy, float look, float halfAngle, float length, SneakyLightBox[] boxes, int count)
        {
            OriginX = ox;
            OriginY = oy;
            Look = look;
            HalfAngle = Math.Max(0.01f, halfAngle);
            Length = Math.Max(0f, length);
            for (int i = 0; i <= Rays; i++)
            {
                SneakyLightGeometry.Direction(RayAngle(i), out float dx, out float dy);
                float t = SneakyLightGeometry.CastRay(ox, oy, dx, dy, Length, boxes, count);
                HitDistance[i] = t;
                HitX[i] = ox + dx * t;
                HitY[i] = oy + dy * t;
            }
            Built = true;
        }

        /// <summary>Il punto è nella zona illuminata (dentro uno dei triangoli origine - arrivo i - arrivo i+1)?</summary>
        public bool Contains(float px, float py)
        {
            if (!Built) return false;
            float vx = px - OriginX, vy = py - OriginY;
            float dist2 = vx * vx + vy * vy;
            if (dist2 > Length * Length) return false;
            if (dist2 < 1e-10f) return HitDistance[0] > 0f || HitDistance[Rays] > 0f;
            float rel = SneakyLightGeometry.DeltaAngle(Look, SneakyLightGeometry.AngleOf(vx, vy));
            if (Math.Abs(rel) > HalfAngle) return false;
            float f = (rel + HalfAngle) / (2f * HalfAngle) * Rays;
            int i = (int)Math.Floor(f);
            if (i < 0) i = 0;
            if (i > Rays - 1) i = Rays - 1;
            // dentro il triangolo (O, A, B) sapendo già di essere nello spicchio fra i due raggi: basta il lato AB
            float ax = HitX[i], ay = HitY[i], bx = HitX[i + 1], by = HitY[i + 1];
            float ex = bx - ax, ey = by - ay;
            float sideP = ex * (py - ay) - ey * (px - ax);
            float sideO = ex * (OriginY - ay) - ey * (OriginX - ax);
            if (Math.Abs(sideO) < 1e-9f) return false;   // triangolo schiacciato (luce nulla in questo spicchio)
            return sideP * sideO >= 0f;
        }
    }
}
