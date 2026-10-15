using System;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Components
{
    /// <summary>
    /// Lascia disegnare un tratto col dito dentro l'area del Collider2D (es. la bocca sulla faccia del pupazzo).
    /// Il tratto è figlio di questo oggetto, quindi segue i suoi movimenti. Può essere composto da più
    /// strati sovrapposti (es. un solco nella neve: luce sotto, ombra, fondo scuro). Un tratto troppo corto
    /// viene scartato; con singleStroke un nuovo tratto sostituisce il precedente.
    /// I tocchi che iniziano su un oggetto trascinabile non disegnano.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StrokeDrawer : MonoBehaviour
    {
        [Serializable]
        public struct Layer
        {
            public Color color;
            public float width;
            public Vector2 offset;
        }

        [Tooltip("Strati del tratto, dal più basso al più alto. Vuoto = un solo tratto col colore e lo spessore sotto.")]
        public Layer[] layers;
        public float width = 0.07f;
        public Color color = new Color(0.17f, 0.13f, 0.2f);
        public float minLength = 0.3f;
        public bool singleStroke = true;
        [Tooltip("Se singleStroke è falso: quanti tratti restano al massimo (0 = nessun limite); i più vecchi spariscono.")]
        public int maxStrokes = 0;
        [Tooltip("Ogni tratto diventa anche una barriera fisica (EdgeCollider2D con lo spessore indicato).")]
        public bool addCollider;
        public float colliderRadius = 0.06f;
        public int sortingOrder = 30;
        [Tooltip("Facoltativo: se vuoto si usa lo shader degli sprite.")]
        public Material material;

        public bool HasStroke => strokes.Count > 0;
        /// <summary>Punti dell'ultimo tratto accettato, in coordinate locali di questo oggetto.</summary>
        public IReadOnlyList<Vector3> LastStrokePoints => lastPoints;
        public float LastStrokeLength { get; private set; }
        public event Action<StrokeDrawer> StrokeFinished;

        readonly List<GameObject> strokes = new List<GameObject>();
        readonly List<Vector3> points = new List<Vector3>();
        readonly List<Vector3> lastPoints = new List<Vector3>();
        readonly List<LineRenderer> drawingLines = new List<LineRenderer>();
        Collider2D area;
        Camera cam;
        GameObject drawing;
        Material ownMaterial;

        void Awake()
        {
            area = GetComponent<Collider2D>();
            cam = Camera.main;
            if (material == null)
            {
                ownMaterial = new Material(Shader.Find("Sprites/Default"));
                material = ownMaterial;
            }
        }

        void OnDisable() { CancelStroke(); }

        void OnDestroy() { if (ownMaterial != null) Destroy(ownMaterial); }

        void Update()
        {
            if (Input.touchCount >= 2) { CancelStroke(); return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) return;
            Vector2 world = PointerInput.ToWorld(cam, sp);

            if (began && drawing == null)
            {
                if (PointerInput.IsOverUi() || !area.OverlapPoint(world) || ItemAt(world)) return;
                StartStroke(world);
                return;
            }
            if (drawing == null) return;

            AddPoint(world);
            if (ended) FinishStroke();
        }

        public void Clear()
        {
            CancelStroke();
            foreach (GameObject s in strokes) if (s != null) Destroy(s);
            strokes.Clear();
            lastPoints.Clear();
        }

        static bool ItemAt(Vector2 world)
        {
            foreach (Collider2D c in Physics2D.OverlapPointAll(world))
            {
                Draggable2D d = c.GetComponentInParent<Draggable2D>();
                if (d != null && d.enabled) return true;
            }
            return false;
        }

        void StartStroke(Vector2 world)
        {
            drawing = new GameObject("Stroke");
            drawing.transform.SetParent(transform, false);
            drawingLines.Clear();

            Layer[] stack = layers != null && layers.Length > 0
                ? layers
                : new[] { new Layer { color = color, width = width, offset = Vector2.zero } };
            for (int i = 0; i < stack.Length; i++)
            {
                var go = new GameObject("Layer" + i);
                go.transform.SetParent(drawing.transform, false);
                go.transform.localPosition = stack[i].offset;
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.widthMultiplier = stack[i].width;
                lr.numCapVertices = 6;
                lr.numCornerVertices = 4;
                lr.sharedMaterial = material;
                lr.startColor = lr.endColor = stack[i].color;
                lr.sortingOrder = sortingOrder + i;
                lr.positionCount = 0;
                drawingLines.Add(lr);
            }
            points.Clear();
            AddPoint(world);
        }

        void AddPoint(Vector2 world)
        {
            Vector3 local = transform.InverseTransformPoint(new Vector3(world.x, world.y, transform.position.z));
            local.z = 0f;
            if (points.Count > 0 && (points[points.Count - 1] - local).sqrMagnitude < 0.0009f) return;
            points.Add(local);
            Vector3[] arr = points.ToArray();
            foreach (LineRenderer lr in drawingLines)
            {
                lr.positionCount = arr.Length;
                lr.SetPositions(arr);
            }
        }

        void FinishStroke()
        {
            float length = 0f;
            for (int i = 1; i < points.Count; i++)
                length += Vector3.Distance(transform.TransformPoint(points[i - 1]), transform.TransformPoint(points[i]));

            if (length < minLength)
            {
                CancelStroke();
                return;
            }
            if (singleStroke)
            {
                foreach (GameObject s in strokes) if (s != null) Destroy(s);
                strokes.Clear();
            }
            else if (maxStrokes > 0)
            {
                while (strokes.Count >= maxStrokes)
                {
                    if (strokes[0] != null) Destroy(strokes[0]);
                    strokes.RemoveAt(0);
                }
            }
            if (addCollider)
            {
                EdgeCollider2D edge = drawing.AddComponent<EdgeCollider2D>();
                var pts = new Vector2[points.Count];
                for (int i = 0; i < pts.Length; i++) pts[i] = points[i];
                edge.points = pts;
                edge.edgeRadius = colliderRadius;
            }
            strokes.Add(drawing);
            drawing = null;
            drawingLines.Clear();
            lastPoints.Clear();
            lastPoints.AddRange(points);
            points.Clear();
            LastStrokeLength = length;
            StrokeFinished?.Invoke(this);
        }

        void CancelStroke()
        {
            if (drawing != null) Destroy(drawing);
            drawing = null;
            drawingLines.Clear();
            points.Clear();
        }
    }
}
