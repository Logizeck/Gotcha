using System;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Components
{
    /// <summary>
    /// Taglio col dito: scatta quando un unico gesto entra nel cerchio (centro = questo oggetto) e ne esce dalla
    /// parte opposta, attraversandolo per almeno minChord × raggio. Lascia una scia bianca che svanisce.
    /// Un gesto che inizia dentro il cerchio non conta (bisogna "affondare" il taglio da fuori).
    /// </summary>
    public class SwipeCutter : MonoBehaviour
    {
        public float radius = 0.75f;
        [Tooltip("Lunghezza minima del taglio dentro il cerchio, in raggi (2 = diametro).")]
        public float minChord = 1.1f;
        public Material trailMaterial;
        public Color trailColor = new Color(1f, 1f, 1f, 0.9f);
        public float trailWidth = 0.14f;
        public float trailLife = 0.16f;
        public int trailSortingOrder = 50;

        public event Action<Vector2, Vector2> Cut;   // punto di entrata e di uscita (mondo)

        readonly List<Vector3> trailPoints = new List<Vector3>();
        readonly List<float> trailTimes = new List<float>();
        Camera cam;
        LineRenderer trail;
        Vector2 last, entry;
        bool tracking, hasEntry, done;

        void Awake()
        {
            cam = Camera.main;
            var go = new GameObject("SwipeTrail");
            go.transform.SetParent(transform, false);
            trail = go.AddComponent<LineRenderer>();
            trail.useWorldSpace = true;
            trail.widthMultiplier = trailWidth;
            trail.widthCurve = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);
            trail.numCapVertices = 4;
            trail.sharedMaterial = trailMaterial != null ? trailMaterial : new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            trail.endColor = trailColor;
            trail.sortingOrder = trailSortingOrder;
            trail.positionCount = 0;
        }

        void Update()
        {
            UpdateTrail();
            if (done) return;

            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) || PointerInput.IsOverUi() && began)
            {
                tracking = false;
                return;
            }
            Vector2 world = PointerInput.ToWorld(cam, sp);

            if (began || !tracking)
            {
                tracking = true;
                hasEntry = false;
                last = world;
                AddTrail(world);
                return;
            }

            Process(last, world);
            last = world;
            AddTrail(world);
            if (ended) tracking = false;
        }

        /// <summary>Intersezione del segmento a→b col cerchio: ricorda l'entrata, al momento dell'uscita valuta il taglio.</summary>
        void Process(Vector2 a, Vector2 b)
        {
            Vector2 c = transform.position;
            Vector2 d = b - a, f = a - c;
            float A = Vector2.Dot(d, d);
            if (A < 1e-6f) return;
            float B = 2f * Vector2.Dot(f, d), C = Vector2.Dot(f, f) - radius * radius;
            float disc = B * B - 4f * A * C;
            if (disc < 0f) return;
            float sq = Mathf.Sqrt(disc);
            float t1 = (-B - sq) / (2f * A), t2 = (-B + sq) / (2f * A);

            if (t1 >= 0f && t1 <= 1f) { hasEntry = true; entry = a + d * t1; }
            if (t2 >= 0f && t2 <= 1f && hasEntry)
            {
                Vector2 exit = a + d * t2;
                hasEntry = false;
                if (Vector2.Distance(entry, exit) >= radius * minChord)
                {
                    done = true;
                    Cut?.Invoke(entry, exit);
                }
            }
        }

        void AddTrail(Vector2 p)
        {
            trailPoints.Add(new Vector3(p.x, p.y, 0f));
            trailTimes.Add(Time.time);
        }

        void UpdateTrail()
        {
            while (trailTimes.Count > 0 && Time.time - trailTimes[0] > trailLife)
            {
                trailTimes.RemoveAt(0);
                trailPoints.RemoveAt(0);
            }
            trail.positionCount = trailPoints.Count;
            if (trailPoints.Count > 0) trail.SetPositions(trailPoints.ToArray());
        }
    }
}
