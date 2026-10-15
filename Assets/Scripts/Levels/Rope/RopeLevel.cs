using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 30 — taglia la corda. Il biscotto è appeso a due corde. Tagliandone una (swipe sopra la corda) il
    /// biscotto oscilla come un pendolo; tagliando l'altra al momento giusto vola fino al topolino. Se cade lontano:
    /// briciole, si riprova.
    /// </summary>
    public class RopeLevel : LevelController
    {
        [SerializeField] Rigidbody2D cookie;
        [SerializeField] Transform[] anchors;
        [SerializeField] LineRenderer[] ropes;
        [SerializeField] Transform mouse;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SmokeEmitter crumbs;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] LineRenderer trail;
        [SerializeField] float eatDistance = 0.7f;
        [SerializeField] float floorY = -1.85f;

        readonly List<DistanceJoint2D> joints = new List<DistanceJoint2D>();
        readonly List<Vector3> trailPts = new List<Vector3>();
        readonly List<float> trailTimes = new List<float>();
        Camera cam;
        Vector2 last;
        bool tracking, done;
        float restFor;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            joints.AddRange(cookie.GetComponents<DistanceJoint2D>());
            if (hearts != null) hearts.Stop();
            if (crumbs != null) crumbs.Stop();
            if (bubble != null) bubble.Hide();
        }

        void Update()
        {
            if (done || IsFinished) return;
            for (int i = 0; i < ropes.Length; i++)
            {
                if (ropes[i] == null || !ropes[i].enabled) continue;
                ropes[i].SetPosition(0, anchors[i].position);
                ropes[i].SetPosition(1, cookie.position);
            }

            // swipe: ogni tratto del dito che attraversa una corda la taglia
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (tracking && !began)
                    for (int i = 0; i < ropes.Length; i++)
                        if (ropes[i] != null && ropes[i].enabled && Cross(last, w, anchors[i].position, cookie.position)) CutRope(i);
                AddTrail(w);
                tracking = !ended;
                last = w;
            }
            else tracking = false;
            FadeTrail();

            if (Vector2.Distance(cookie.position, (Vector2)mouse.position + new Vector2(0f, 0.2f)) < eatDistance) { StartCoroutine(Eat()); return; }
            // caduto e fermo lontano dal topolino: briciole
            bool free = true;
            foreach (LineRenderer r in ropes) if (r != null && r.enabled) free = false;
            if (free && cookie.position.y < floorY + 0.35f) restFor += Time.deltaTime; else restFor = 0f;
            if (restFor > 0.6f || cookie.position.y < -6f) StartCoroutine(Crumbs());
        }

        void CutRope(int i)
        {
            ropes[i].enabled = false;
            foreach (DistanceJoint2D j in joints)
                if (j != null && j.enabled && Vector2.Distance(j.connectedAnchor, anchors[i].position) < 0.05f) j.enabled = false;
        }

        static bool Cross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float d1 = Side(c, d, a), d2 = Side(c, d, b), d3 = Side(a, b, c), d4 = Side(a, b, d);
            return d1 * d2 < 0f && d3 * d4 < 0f;
        }

        static float Side(Vector2 p, Vector2 q, Vector2 r) { return (q.x - p.x) * (r.y - p.y) - (q.y - p.y) * (r.x - p.x); }

        void AddTrail(Vector2 w)
        {
            if (trail == null) return;
            trailPts.Add(new Vector3(w.x, w.y, 0f));
            trailTimes.Add(Time.time);
        }

        void FadeTrail()
        {
            if (trail == null) return;
            while (trailTimes.Count > 0 && Time.time - trailTimes[0] > 0.15f) { trailTimes.RemoveAt(0); trailPts.RemoveAt(0); }
            trail.positionCount = trailPts.Count;
            trail.SetPositions(trailPts.ToArray());
        }

        IEnumerator Eat()
        {
            done = true;
            cookie.simulated = false;
            foreach (LineRenderer r in ropes) if (r != null) r.enabled = false;
            Transform t = cookie.transform;
            Vector3 from = t.position, scale = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                t.position = Vector3.Lerp(from, mouse.position + new Vector3(-0.1f, 0.25f, 0f), k);
                t.localScale = scale * (1f - k);
                yield return null;
            }
            t.gameObject.SetActive(false);
            if (hearts != null) hearts.Play();
            if (bubble != null) bubble.Show("Nice catch!");
            yield return new WaitForSeconds(1.3f);
            if (hearts != null) hearts.Stop();
            Win();
        }

        IEnumerator Crumbs()
        {
            done = true;
            if (crumbs != null) { crumbs.transform.position = cookie.position; crumbs.Play(); }
            cookie.gameObject.SetActive(false);
            if (bubble != null) bubble.Show("Nooo, crumbs!");
            yield return new WaitForSeconds(0.2f);
            if (crumbs != null) crumbs.Stop();
            yield return new WaitForSeconds(1f);
            Fail();
        }
    }
}
