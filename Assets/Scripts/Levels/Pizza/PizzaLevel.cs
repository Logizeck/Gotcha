using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 31 — pizza per quattro. 1) Taglia la pizza in 4 con due tagli che passano dal centro (un taglio
    /// storto non vale: "Not fair!"); 2) trascina una fetta a ognuno dei 4 topolini affamati.
    /// </summary>
    public class PizzaLevel : LevelController
    {
        [SerializeField] Transform pizza;            // centro della pizza
        [SerializeField] GameObject whole;
        [SerializeField] float radius = 1.1f;
        [SerializeField] float centerTolerance = 0.32f;
        [SerializeField] LineRenderer cutMark;
        [SerializeField] Transform[] slices;         // 4 spicchi (apice al centro), attivati dopo i tagli
        [SerializeField] DropSlot[] plates;          // davanti ai 4 topolini
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] LineRenderer trail;

        Camera cam;
        Vector2 start;
        bool tracking, done;
        int cuts, served;
        float firstAngle;
        readonly System.Collections.Generic.List<Vector3> trailPts = new System.Collections.Generic.List<Vector3>();
        readonly System.Collections.Generic.List<float> trailTimes = new System.Collections.Generic.List<float>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (Transform s in slices) s.gameObject.SetActive(false);
            foreach (DropSlot p in plates) p.Filled += OnServed;
            if (cutMark != null) cutMark.enabled = false;
            if (hearts != null) hearts.Stop();
            if (bubble != null) { bubble.Show("Pizza for four, please!"); StartCoroutine(HideLater(2f)); }
        }

        void OnDestroy() { if (plates != null) foreach (DropSlot p in plates) if (p != null) p.Filled -= OnServed; }

        void Update()
        {
            if (done || IsFinished || cuts >= 2) { FadeTrail(); return; }
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (began) { start = w; tracking = true; }
                if (tracking) { trailPts.Add(new Vector3(w.x, w.y, 0f)); trailTimes.Add(Time.time); }
                if (ended && tracking) { tracking = false; TryCut(start, w); }
            }
            FadeTrail();
        }

        void TryCut(Vector2 a, Vector2 b)
        {
            Vector2 c = pizza.position;
            // deve entrare e uscire dalla pizza (estremi fuori dal bordo)
            if (Vector2.Distance(a, c) < radius * 0.9f || Vector2.Distance(b, c) < radius * 0.9f || (b - a).magnitude < radius * 1.6f) return;
            Vector2 dir = (b - a).normalized;
            float off = Mathf.Abs((c.x - a.x) * dir.y - (c.y - a.y) * dir.x);   // distanza del centro dalla linea
            float along = Vector2.Dot(c - a, dir);
            if (along < 0f || along > (b - a).magnitude) return;
            if (off > centerTolerance) { if (bubble != null) { bubble.Show("Not fair! Through the middle!"); StartCoroutine(HideLater(1.5f)); } return; }
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (cuts == 0)
            {
                firstAngle = angle;
                cuts = 1;
                if (cutMark != null)
                {
                    cutMark.enabled = true;
                    cutMark.SetPosition(0, (Vector3)(c - dir * radius) + Vector3.back * 0.01f);
                    cutMark.SetPosition(1, (Vector3)(c + dir * radius) + Vector3.back * 0.01f);
                }
                return;
            }
            float diff = Mathf.Abs(Mathf.DeltaAngle(angle, firstAngle));
            if (diff > 90f) diff = 180f - diff;
            if (diff < 45f) { if (bubble != null) { bubble.Show("Same cut again? Cross it!"); StartCoroutine(HideLater(1.5f)); } return; }
            cuts = 2;
            StartCoroutine(Split());
        }

        IEnumerator Split()
        {
            if (cutMark != null) cutMark.enabled = false;
            whole.SetActive(false);
            Vector3 c = pizza.position;
            // ogni spicchio ha il suo pivot nel baricentro (a 4r/3π dal centro, sulla bisettrice): lo si afferra lì
            float centroid = 4f * radius / (3f * Mathf.PI);
            for (float k = 0f; k <= 1f; k += Time.deltaTime / 0.3f)
            {
                for (int i = 0; i < slices.Length; i++)
                {
                    float a = (firstAngle + i * 90f + 45f) * Mathf.Deg2Rad;
                    slices[i].position = c + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * (centroid + 0.18f * k);
                    slices[i].rotation = Quaternion.Euler(0, 0, firstAngle + i * 90f);
                    if (!slices[i].gameObject.activeSelf) slices[i].gameObject.SetActive(true);
                }
                yield return null;
            }
            if (bubble != null) { bubble.Show("Now serve it!"); StartCoroutine(HideLater(1.5f)); }
        }

        void OnServed(DropSlot plate, Draggable2D slice)
        {
            served++;
            slice.transform.rotation = Quaternion.Euler(0, 0, -45f);
            if (served >= plates.Length) StartCoroutine(Party());
        }

        IEnumerator Party()
        {
            done = true;
            if (hearts != null) hearts.Play();
            if (bubble != null) bubble.Show("Yum! Thanks!");
            yield return new WaitForSeconds(1.5f);
            if (hearts != null) hearts.Stop();
            Win();
        }

        void FadeTrail()
        {
            if (trail == null) return;
            while (trailTimes.Count > 0 && Time.time - trailTimes[0] > 0.15f) { trailTimes.RemoveAt(0); trailPts.RemoveAt(0); }
            trail.positionCount = trailPts.Count;
            trail.SetPositions(trailPts.ToArray());
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
