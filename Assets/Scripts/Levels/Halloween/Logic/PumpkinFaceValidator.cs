using System;
using System.Collections.Generic;

namespace Gotcha.Levels.Halloween
{
    /// <summary>Punto 2D semplice (coordinate normalizzate della faccia della zucca: -1..1 su x e y).</summary>
    public struct HwPoint
    {
        public float X, Y;
        public HwPoint(float x, float y) { X = x; Y = y; }
    }

    public enum FacePart { None, LeftEye, RightEye, Nose, Mouth }

    /// <summary>Soglie (configurabili) delle regioni logiche invisibili della faccia. Coordinate normalizzate.</summary>
    public sealed class PumpkinFaceConfig
    {
        /// <summary>Tratti più vicini di così (fra i loro riquadri) formano un unico gruppo (es. occhio fatto a più tratti).</summary>
        public float MergeDistance = 0.1f;
        public float MinStrokeLength = 0.08f;
        public int MaxStrokes = 30;
        // occhi: metà alta, uno a sinistra e uno a destra
        public float EyeMinY = 0.02f;
        public float EyeMinAbsX = 0.12f;
        public float EyeMaxSize = 0.85f;
        // naso: al centro
        public float NoseMaxAbsX = 0.3f;
        public float NoseMinY = -0.4f;
        public float NoseMaxY = 0.3f;
        public float NoseMaxSize = 0.6f;
        // bocca: in basso, abbastanza larga
        public float MouthMaxY = -0.18f;
        public float MouthMinWidth = 0.5f;
        public float MouthMaxAbsX = 0.45f;
    }

    /// <summary>Esito della verifica: quali parti ci sono e a quale parte appartiene ogni tratto.</summary>
    public sealed class FaceReport
    {
        public bool LeftEye, RightEye, Nose, Mouth;
        public FacePart[] StrokeParts = new FacePart[0];
        public bool IsComplete => LeftEye && RightEye && Nose && Mouth;
        public int PartsFound => (LeftEye ? 1 : 0) + (RightEye ? 1 : 0) + (Nose ? 1 : 0) + (Mouth ? 1 : 0);
    }

    /// <summary>
    /// Riconosce una faccia intagliata a mano libera (livello 246): due occhi separati nella metà alta (uno a
    /// sinistra, uno a destra), un naso al centro e una bocca larga in basso. Tollerante: i tratti vicini si uniscono
    /// in gruppi, ogni gruppo vale per una sola parte, i tratti in più non danno fastidio.
    /// </summary>
    public sealed class PumpkinFaceValidator
    {
        struct Box
        {
            public float MinX, MinY, MaxX, MaxY;
            public float CX => (MinX + MaxX) * 0.5f;
            public float CY => (MinY + MaxY) * 0.5f;
            public float W => MaxX - MinX;
            public float H => MaxY - MinY;
            public Box Union(Box o) => new Box { MinX = Math.Min(MinX, o.MinX), MinY = Math.Min(MinY, o.MinY), MaxX = Math.Max(MaxX, o.MaxX), MaxY = Math.Max(MaxY, o.MaxY) };
            public bool Near(Box o, float d) => MinX - d <= o.MaxX && o.MinX - d <= MaxX && MinY - d <= o.MaxY && o.MinY - d <= MaxY;
        }

        readonly List<Box> strokes = new List<Box>();
        public PumpkinFaceConfig Config { get; }
        public int StrokeCount => strokes.Count;

        public PumpkinFaceValidator(PumpkinFaceConfig config = null) { Config = config ?? new PumpkinFaceConfig(); }

        public void Reset() { strokes.Clear(); }

        /// <summary>
        /// Aggiunge un tratto. False se è troppo corto (scartato). Oltre MaxStrokes il tratto più vecchio viene tolto
        /// (evicted = true): chi disegna deve togliere anche il suo segno.
        /// </summary>
        public bool AddStroke(IList<HwPoint> points, out bool evicted)
        {
            evicted = false;
            if (points == null || points.Count == 0) return false;
            float len = 0f;
            var b = new Box { MinX = points[0].X, MaxX = points[0].X, MinY = points[0].Y, MaxY = points[0].Y };
            for (int i = 1; i < points.Count; i++)
            {
                float dx = points[i].X - points[i - 1].X, dy = points[i].Y - points[i - 1].Y;
                len += (float)Math.Sqrt(dx * dx + dy * dy);
                b.MinX = Math.Min(b.MinX, points[i].X); b.MaxX = Math.Max(b.MaxX, points[i].X);
                b.MinY = Math.Min(b.MinY, points[i].Y); b.MaxY = Math.Max(b.MaxY, points[i].Y);
            }
            if (len < Config.MinStrokeLength) return false;
            strokes.Add(b);
            if (Config.MaxStrokes > 0 && strokes.Count > Config.MaxStrokes)
            {
                strokes.RemoveAt(0);
                evicted = true;
            }
            return true;
        }

        public FaceReport Evaluate()
        {
            var report = new FaceReport { StrokeParts = new FacePart[strokes.Count] };
            int n = strokes.Count;
            if (n == 0) return report;

            // gruppi di tratti vicini (union-find)
            var parent = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;
            int Find(int i) { while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; } return i; }
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (strokes[i].Near(strokes[j], Config.MergeDistance)) parent[Find(i)] = Find(j);
            // i riquadri dei gruppi possono essersi allargati: si uniscono finché serve
            bool merged = true;
            while (merged)
            {
                merged = false;
                var boxes = GroupBoxes(parent, Find);
                foreach (var a in boxes)
                    foreach (var c in boxes)
                        if (a.Key != c.Key && Find(a.Key) != Find(c.Key) && a.Value.Near(c.Value, Config.MergeDistance))
                        {
                            parent[Find(a.Key)] = Find(c.Key);
                            merged = true;
                        }
            }

            Dictionary<int, Box> groups = GroupBoxes(parent, Find);
            var partOf = new Dictionary<int, FacePart>();
            // prima la bocca, poi gli occhi, poi il naso; ogni parte una volta sola (il gruppo più adatto vince)
            AssignBest(groups, partOf, FacePart.Mouth, g => g.CY <= Config.MouthMaxY && g.W >= Config.MouthMinWidth && Math.Abs(g.CX) <= Config.MouthMaxAbsX, g => g.W);
            AssignBest(groups, partOf, FacePart.LeftEye, g => IsEye(g) && g.CX < 0f, g => g.CY);
            AssignBest(groups, partOf, FacePart.RightEye, g => IsEye(g) && g.CX > 0f, g => g.CY);
            AssignBest(groups, partOf, FacePart.Nose, g => Math.Abs(g.CX) <= Config.NoseMaxAbsX && g.CY >= Config.NoseMinY && g.CY <= Config.NoseMaxY
                && Math.Max(g.W, g.H) <= Config.NoseMaxSize, g => -Math.Abs(g.CX));

            foreach (FacePart p in partOf.Values)
            {
                if (p == FacePart.LeftEye) report.LeftEye = true;
                else if (p == FacePart.RightEye) report.RightEye = true;
                else if (p == FacePart.Nose) report.Nose = true;
                else if (p == FacePart.Mouth) report.Mouth = true;
            }
            for (int i = 0; i < n; i++)
                report.StrokeParts[i] = partOf.TryGetValue(Find(i), out FacePart part) ? part : FacePart.None;
            return report;
        }

        bool IsEye(Box g) => g.CY >= Config.EyeMinY && Math.Abs(g.CX) >= Config.EyeMinAbsX && Math.Max(g.W, g.H) <= Config.EyeMaxSize;

        Dictionary<int, Box> GroupBoxes(int[] parent, Func<int, int> find)
        {
            var boxes = new Dictionary<int, Box>();
            for (int i = 0; i < strokes.Count; i++)
            {
                int r = find(i);
                boxes[r] = boxes.TryGetValue(r, out Box b) ? b.Union(strokes[i]) : strokes[i];
            }
            return boxes;
        }

        static void AssignBest(Dictionary<int, Box> groups, Dictionary<int, FacePart> partOf, FacePart part, Func<Box, bool> fits, Func<Box, float> score)
        {
            int best = -1;
            float bestScore = float.MinValue;
            foreach (var g in groups)
            {
                if (partOf.ContainsKey(g.Key) || !fits(g.Value)) continue;
                float s = score(g.Value);
                if (s > bestScore) { bestScore = s; best = g.Key; }
            }
            if (best >= 0) partOf[best] = part;
        }
    }
}
