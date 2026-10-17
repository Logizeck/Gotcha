using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Testo "scritto col dito" (per esempio sullo specchio appannato): ogni lettera è fatta di tratti spessi con le
    /// punte tonde, un po' storti e di spessore irregolare, come un polpastrello. Il testo sta SEMPRE dentro il
    /// riquadro "box" (coordinate locali, centrato sull'oggetto): se è lungo, diventa più piccolo.
    /// Caratteri: A-Z, 0-9, ! ? - . _ spazio e a capo.
    /// </summary>
    public class FingerText : MonoBehaviour
    {
        [SerializeField] string value = "";
        public Vector2 box = new Vector2(2f, 1f);
        [Tooltip("Spessore del dito rispetto all'altezza di una lettera.")]
        public float thickness = 0.16f;
        public Color color = Color.white;
        public int sortingOrder = 4;
        public Material material;
        public int seed = 7;

        readonly List<LineRenderer> strokes = new List<LineRenderer>();
        const float GlyphW = 1f, GlyphH = 1.4f, Advance = 1.3f, LineStep = 1.95f;

        public string Text
        {
            get => value;
            set { if (this.value == value) return; this.value = value ?? ""; Rebuild(); }
        }

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                foreach (LineRenderer lr in strokes) if (lr != null) lr.startColor = lr.endColor = color;
            }
        }

        void Awake() { Rebuild(); }

        public void Rebuild()
        {
            if (!Application.isPlaying) return;   // i tratti si creano solo in gioco (il prefab salva solo il testo)
            foreach (LineRenderer lr in strokes) if (lr != null) Destroy(lr.gameObject);
            strokes.Clear();
            string[] lines = (value ?? "").ToUpperInvariant().Split('\n');
            int cols = 1;
            foreach (string l in lines) cols = Mathf.Max(cols, l.Length);
            float w = cols * Advance - (Advance - GlyphW), h = lines.Length * LineStep - (LineStep - GlyphH);
            float scale = Mathf.Min(box.x / w, box.y / h);
            if (material == null) material = new Material(Shader.Find("Sprites/Default"));
            var rnd = new System.Random(seed + value.GetHashCode());
            for (int li = 0; li < lines.Length; li++)
            {
                string line = lines[li];
                float lineW = line.Length * Advance - (Advance - GlyphW);
                float x0 = -lineW * 0.5f, y0 = h * 0.5f - GlyphH - li * LineStep;
                for (int ci = 0; ci < line.Length; ci++)
                {
                    if (!Glyphs.TryGetValue(line[ci], out string g)) continue;
                    // ogni lettera un po' storta e fuori riga, come scritta in fretta
                    float tilt = (float)(rnd.NextDouble() - 0.5) * 0.25f, dy = (float)(rnd.NextDouble() - 0.5) * 0.15f;
                    Vector2 origin = new Vector2(x0 + ci * Advance, y0 + dy);
                    foreach (string stroke in g.Split('|'))
                    {
                        var pts = new List<Vector2>();
                        foreach (string pair in stroke.Trim().Split(' '))
                        {
                            string[] xy = pair.Split(',');
                            Vector2 p = new Vector2(float.Parse(xy[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(xy[1], System.Globalization.CultureInfo.InvariantCulture));
                            p += new Vector2((float)(rnd.NextDouble() - 0.5), (float)(rnd.NextDouble() - 0.5)) * 0.08f;
                            p = new Vector2(p.x - GlyphW * 0.5f, p.y);
                            p = new Vector2(p.x * Mathf.Cos(tilt) - p.y * Mathf.Sin(tilt), p.x * Mathf.Sin(tilt) + p.y * Mathf.Cos(tilt));
                            pts.Add((origin + new Vector2(GlyphW * 0.5f, 0f) + p) * scale);
                        }
                        AddStroke(Smooth(pts), scale, rnd);
                    }
                }
            }
        }

        void AddStroke(List<Vector2> pts, float scale, System.Random rnd)
        {
            var go = new GameObject("Stroke");
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.sharedMaterial = material;
            lr.sortingOrder = sortingOrder;
            lr.numCapVertices = 6;
            lr.numCornerVertices = 4;
            lr.alignment = LineAlignment.TransformZ;
            if (pts.Count == 1) pts.Add(pts[0] + new Vector2(0.01f, -0.01f));   // un punto (il puntino del "!")
            lr.positionCount = pts.Count;
            for (int i = 0; i < pts.Count; i++) lr.SetPosition(i, new Vector3(pts[i].x, pts[i].y, 0f));
            // il dito preme di più a metà tratto e si alza alla fine
            float a = 0.8f + (float)rnd.NextDouble() * 0.15f, b = 0.65f + (float)rnd.NextDouble() * 0.2f;
            lr.widthCurve = new AnimationCurve(new Keyframe(0f, a), new Keyframe(0.45f, 1.05f), new Keyframe(1f, b));
            lr.widthMultiplier = thickness * GlyphH * scale * Mathf.Abs(transform.lossyScale.x);
            lr.startColor = lr.endColor = color;
            strokes.Add(lr);
        }

        /// <summary>Curva morbida che passa per i punti (Catmull-Rom): le dita non fanno spigoli netti.</summary>
        static List<Vector2> Smooth(List<Vector2> p)
        {
            if (p.Count < 3) return p;
            var o = new List<Vector2>();
            for (int i = 0; i < p.Count - 1; i++)
            {
                Vector2 p0 = p[Mathf.Max(0, i - 1)], p1 = p[i], p2 = p[i + 1], p3 = p[Mathf.Min(p.Count - 1, i + 2)];
                for (int k = 0; k < 6; k++)
                {
                    float t = k / 6f, t2 = t * t, t3 = t2 * t;
                    o.Add(0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3));
                }
            }
            o.Add(p[p.Count - 1]);
            return o;
        }

        // Lettere a tratti (x 0..1, y 0..1.4): punti separati da spazi, tratti separati da "|".
        const string Oval = "0.5,1.4 0.12,1.15 0,0.7 0.12,0.25 0.5,0 0.88,0.25 1,0.7 0.88,1.15 0.5,1.4";
        static readonly Dictionary<char, string> Glyphs = new Dictionary<char, string>
        {
            { '0', Oval },
            { '1', "0.25,1.1 0.6,1.4 0.6,0" },
            { '2', "0.1,1.1 0.4,1.4 0.8,1.35 0.95,1.05 0.8,0.7 0.05,0 1,0" },
            { '3', "0.1,1.25 0.5,1.4 0.9,1.15 0.8,0.85 0.45,0.75 0.85,0.6 0.95,0.3 0.6,0 0.1,0.15" },
            { '4', "0.75,0 0.75,1.4 0.05,0.45 1,0.45" },
            { '5', "0.9,1.4 0.15,1.4 0.1,0.8 0.5,0.9 0.9,0.65 0.9,0.25 0.5,0 0.1,0.15" },
            { '6', "0.85,1.3 0.45,1.4 0.1,1 0.05,0.45 0.3,0.05 0.7,0.05 0.95,0.4 0.75,0.75 0.35,0.75 0.08,0.5" },
            { '7', "0.05,1.4 0.95,1.4 0.4,0" },
            { '8', "0.5,0.75 0.15,1 0.25,1.35 0.75,1.35 0.85,1 0.5,0.75 0.08,0.4 0.25,0.02 0.75,0.02 0.92,0.4 0.5,0.75" },
            { '9', "0.92,0.9 0.65,0.65 0.25,0.7 0.05,1 0.3,1.38 0.7,1.38 0.92,1 0.9,0.4 0.55,0 0.15,0.1" },
            { 'A', "0,0 0.5,1.4 1,0|0.2,0.5 0.8,0.5" },
            { 'B', "0.1,0 0.1,1.4 0.7,1.35 0.85,1.05 0.6,0.75 0.1,0.75|0.6,0.75 0.95,0.45 0.8,0.05 0.1,0" },
            { 'C', "0.95,1.15 0.6,1.4 0.2,1.2 0.02,0.7 0.2,0.15 0.6,0 0.95,0.25" },
            { 'D', "0.1,0 0.1,1.4 0.6,1.3 0.95,0.7 0.6,0.08 0.1,0" },
            { 'E', "0.9,1.4 0.1,1.4 0.1,0 0.9,0|0.1,0.72 0.7,0.72" },
            { 'F', "0.9,1.4 0.1,1.4 0.1,0|0.1,0.72 0.7,0.72" },
            { 'G', "0.95,1.15 0.6,1.4 0.2,1.2 0.02,0.7 0.2,0.15 0.6,0 0.95,0.25 0.95,0.6 0.55,0.6" },
            { 'H', "0.1,1.4 0.1,0|0.9,1.4 0.9,0|0.1,0.72 0.9,0.72" },
            { 'I', "0.5,1.4 0.5,0|0.2,1.4 0.8,1.4|0.2,0 0.8,0" },
            { 'J', "0.85,1.4 0.85,0.35 0.6,0 0.25,0.05 0.08,0.35" },
            { 'K', "0.1,1.4 0.1,0|0.9,1.4 0.1,0.6|0.35,0.8 0.95,0" },
            { 'L', "0.1,1.4 0.1,0 0.9,0" },
            { 'M', "0.05,0 0.15,1.4 0.5,0.5 0.85,1.4 0.95,0" },
            { 'N', "0.1,0 0.1,1.4 0.9,0 0.9,1.4" },
            { 'O', Oval },
            { 'P', "0.1,0 0.1,1.4 0.7,1.35 0.92,1.05 0.7,0.72 0.1,0.7" },
            { 'Q', Oval + "|0.6,0.35 1,0" },
            { 'R', "0.1,0 0.1,1.4 0.7,1.35 0.92,1.05 0.7,0.72 0.1,0.7|0.5,0.7 0.95,0" },
            { 'S', "0.9,1.2 0.55,1.4 0.15,1.25 0.15,0.9 0.5,0.72 0.85,0.5 0.85,0.15 0.45,0 0.08,0.2" },
            { 'T', "0.05,1.4 0.95,1.4|0.5,1.4 0.5,0" },
            { 'U', "0.1,1.4 0.1,0.35 0.35,0 0.65,0 0.9,0.35 0.9,1.4" },
            { 'V', "0,1.4 0.5,0 1,1.4" },
            { 'W', "0,1.4 0.25,0 0.5,0.9 0.75,0 1,1.4" },
            { 'X', "0.05,1.4 0.95,0|0.95,1.4 0.05,0" },
            { 'Y', "0.05,1.4 0.5,0.7 0.95,1.4|0.5,0.7 0.5,0" },
            { 'Z', "0.05,1.4 0.95,1.4 0.05,0 0.95,0" },
            { '!', "0.5,1.4 0.5,0.45|0.5,0.04" },
            { '?', "0.1,1.1 0.4,1.4 0.85,1.25 0.85,0.9 0.5,0.65 0.5,0.4|0.5,0.04" },
            { '-', "0.2,0.7 0.8,0.7" },
            { '.', "0.5,0.04" },
            { '_', "0,0 1,0" },
        };
    }
}
