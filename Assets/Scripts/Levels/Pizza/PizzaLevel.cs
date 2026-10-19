using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 31 — pizza per quattro. 1) Taglia la pizza in 4 con due tagli che passano dal centro (un taglio
    /// storto non vale: "Not fair!"); 2) trascina una fetta a ognuno dei 4 topolini affamati, che la mangiano
    /// a morsi (la fetta si accorcia, briciole, guance piene) finché non sparisce.
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
        [SerializeField] Transform[] mice;           // stesso ordine dei piatti
        [SerializeField] SmokeEmitter crumbs;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] LineRenderer trail;

        Camera cam;
        Vector2 start;
        bool tracking, done;
        int cuts, served, eaten;
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
            if (crumbs != null) crumbs.Stop();
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
            Haptics.Light();
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
            Haptics.Light();
            slice.transform.rotation = Quaternion.Euler(0, 0, -45f);
            int i = System.Array.IndexOf(plates, plate);
            StartCoroutine(Eat(i >= 0 && mice != null && i < mice.Length ? mice[i] : null, slice.transform));
        }

        /// <summary>Il topolino si avvicina, addenta la fetta a piccoli morsi (con le briciole) e alla fine si pulisce i baffi.</summary>
        IEnumerator Eat(Transform mouse, Transform slice)
        {
            Vector3 s0 = slice.localScale, home = mouse != null ? mouse.localPosition : Vector3.zero, ms = mouse != null ? mouse.localScale : Vector3.one;
            yield return new WaitForSeconds(0.2f);
            for (int bite = 0; bite < 5; bite++)
            {
                // il topolino si sporge e morde
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.16f)
                {
                    if (mouse != null)
                    {
                        mouse.localPosition = home + Vector3.down * Mathf.Sin(k * Mathf.PI) * 0.12f;
                        mouse.localScale = new Vector3(ms.x * (1f + Mathf.Sin(k * Mathf.PI) * 0.06f), ms.y * (1f - Mathf.Sin(k * Mathf.PI) * 0.06f), ms.z);
                    }
                    yield return null;
                }
                Sfx.Play(this, "tick", 0.6f);
                if (crumbs != null) { crumbs.transform.position = slice.position; crumbs.Play(); }
                float left = 1f - (bite + 1) / 5f;
                slice.localScale = s0 * Mathf.Max(0.05f, left);
                slice.position += Vector3.up * 0.04f;   // la fetta "si avvicina" alla bocca
                yield return new WaitForSeconds(0.12f);
                if (crumbs != null) crumbs.Stop();
            }
            slice.gameObject.SetActive(false);
            // guance piene: gonfio e soddisfatto
            if (mouse != null)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
                {
                    mouse.localScale = ms * (1f + Mathf.Sin(k * Mathf.PI) * 0.12f);
                    mouse.localPosition = home + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.15f;
                    yield return null;
                }
                mouse.localScale = ms;
                mouse.localPosition = home;
            }
            eaten++;
            if (eaten >= plates.Length) StartCoroutine(Party());
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
