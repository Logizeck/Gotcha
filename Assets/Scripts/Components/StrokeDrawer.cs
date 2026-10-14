using System;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Components
{
    /// <summary>
    /// Lascia disegnare un tratto col dito dentro l'area del Collider2D (es. la bocca del pupazzo).
    /// Il tratto è figlio di questo oggetto, quindi segue i suoi movimenti. Un tratto troppo corto
    /// viene scartato; con singleStroke un nuovo tratto sostituisce il precedente.
    /// I tocchi che iniziano su un oggetto trascinabile non disegnano.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StrokeDrawer : MonoBehaviour
    {
        public float width = 0.07f;
        public Color color = new Color(0.17f, 0.13f, 0.2f);
        public float minLength = 0.3f;
        public bool singleStroke = true;
        public int sortingOrder = 30;
        [Tooltip("Facoltativo: se vuoto si usa lo shader degli sprite.")]
        public Material material;

        public bool HasStroke => strokes.Count > 0;
        public float LastStrokeLength { get; private set; }
        public event Action<StrokeDrawer> StrokeFinished;

        readonly List<LineRenderer> strokes = new List<LineRenderer>();
        readonly List<Vector3> points = new List<Vector3>();
        Collider2D area;
        Camera cam;
        LineRenderer drawing;
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
            foreach (LineRenderer l in strokes) if (l != null) Destroy(l.gameObject);
            strokes.Clear();
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
            var go = new GameObject("Stroke");
            go.transform.SetParent(transform, false);
            drawing = go.AddComponent<LineRenderer>();
            drawing.useWorldSpace = false;
            drawing.widthMultiplier = width;
            drawing.numCapVertices = 6;
            drawing.numCornerVertices = 4;
            drawing.sharedMaterial = material;
            drawing.startColor = drawing.endColor = color;
            drawing.sortingOrder = sortingOrder;
            drawing.positionCount = 0;
            points.Clear();
            AddPoint(world);
        }

        void AddPoint(Vector2 world)
        {
            Vector3 local = transform.InverseTransformPoint(new Vector3(world.x, world.y, transform.position.z));
            local.z = 0f;
            if (points.Count > 0 && (points[points.Count - 1] - local).sqrMagnitude < 0.0009f) return;
            points.Add(local);
            drawing.positionCount = points.Count;
            drawing.SetPositions(points.ToArray());
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
                foreach (LineRenderer l in strokes) if (l != null) Destroy(l.gameObject);
                strokes.Clear();
            }
            strokes.Add(drawing);
            drawing = null;
            LastStrokeLength = length;
            StrokeFinished?.Invoke(this);
        }

        void CancelStroke()
        {
            if (drawing != null) Destroy(drawing.gameObject);
            drawing = null;
            points.Clear();
        }
    }
}
