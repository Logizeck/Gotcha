using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 80 — dov'è il topolino? Una folla enorme di animaletti minuscoli. Da qualche parte c'è IL topolino col
    /// cappellino ROSSO (ce ne sono altri col cappellino blu, o senza: non sono lui). Allarga due dita per ingrandire,
    /// trascina con un dito per spostarti, tocca il topolino giusto. (Editor: rotellina = zoom, trascina = sposta.)
    /// </summary>
    public class WhereMouseLevel : LevelController
    {
        [SerializeField] Transform crowd;             // contenitore che si ingrandisce e si sposta
        [SerializeField] Transform target;
        [SerializeField] Transform[] decoys;
        [SerializeField] Transform timeBar;
        [SerializeField] TextMesh info;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Vector2 area = new Vector2(5.2f, 7.0f);   // grandezza della folla a zoom 1
        [SerializeField] float maxZoom = 6f, seconds = 70f, hitRadius = 0.18f;
        [SerializeField] int maxWrong = 3;

        float zoom = 1f, clock, lastPinch;
        Vector2 lastDrag;
        bool dragging, pinching, done, moved;
        int wrong;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            // il topolino giusto finisce in un posto a caso, scambiandosi con un altro animaletto
            if (decoys.Length > 0)
            {
                Transform swap = decoys[Random.Range(0, decoys.Length)];
                (target.localPosition, swap.localPosition) = (swap.localPosition, target.localPosition);
            }
            Info();
            if (bubble != null) { bubble.Show("Find the mouse with the RED hat!"); StartCoroutine(HideLater(2.4f)); }
        }

        void Info() { if (info != null) info.text = "ZOOM x" + zoom.ToString("0.0") + "     X " + wrong + "/" + maxWrong; }

        void ZoomAt(Vector2 world, float factor)
        {
            float z = Mathf.Clamp(zoom * factor, 1f, maxZoom);
            factor = z / zoom;
            zoom = z;
            Vector3 p = crowd.position;
            crowd.position = (Vector3)(world + ((Vector2)p - world) * factor) + Vector3.forward * p.z;
            crowd.localScale = Vector3.one * zoom;
            Clamp();
            Info();
        }

        void Clamp()
        {
            // la folla resta sempre sullo schermo
            Vector2 half = area * 0.5f * (zoom - 1f);
            Vector3 p = crowd.position;
            p.x = Mathf.Clamp(p.x, -half.x, half.x);
            p.y = Mathf.Clamp(p.y, -0.3f - half.y, -0.3f + half.y);
            crowd.position = p;
        }

        void Update()
        {
            if (done || IsFinished) return;
            clock += Time.deltaTime;
            if (timeBar != null) timeBar.localScale = new Vector3(Mathf.Clamp01(1f - clock / seconds), 1f, 1f);
            if (clock >= seconds) { StartCoroutine(Lost("Time's up! He was here...")); return; }

            if (Input.touchCount >= 2)
            {
                Vector2 a = cam.ScreenToWorldPoint(Input.GetTouch(0).position), b = cam.ScreenToWorldPoint(Input.GetTouch(1).position);
                float d = Vector2.Distance(a, b);
                if (pinching && lastPinch > 0.05f) ZoomAt((a + b) * 0.5f, d / lastPinch);
                lastPinch = d;
                pinching = true;
                dragging = false;
                moved = true;
                return;
            }
            pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) ZoomAt(cam.ScreenToWorldPoint(Input.mousePosition), 1f + wheel * 0.15f);
#endif
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { dragging = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began) { dragging = true; moved = false; lastDrag = w; return; }
            if (dragging && !ended)
            {
                Vector2 delta = w - lastDrag;
                if (delta.sqrMagnitude > 0.0004f) moved = true;
                crowd.position += (Vector3)delta;
                Clamp();
                lastDrag = w;
            }
            if (ended && dragging && !moved) Tap(w);
            if (ended) dragging = false;
        }

        void Tap(Vector2 w)
        {
            float r = hitRadius * zoom;
            if (Vector2.Distance(w, target.position) < r) { StartCoroutine(Found()); return; }
            foreach (Transform d in decoys)
                if (Vector2.Distance(w, d.position) < r)
                {
                    wrong++;
                    Info();
                    Sfx.Play(this, "buzz", 0.6f);
                    if (bubble != null) { bubble.Show("Not him!"); StartCoroutine(HideLater(0.8f)); }
                    if (wrong >= maxWrong) StartCoroutine(Lost("Three wrong mice!"));
                    return;
                }
        }

        IEnumerator Found()
        {
            done = true;
            Sfx.Play(this, "ding", 0.8f);
            Vector3 s = target.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { target.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.8f); yield return null; }
            if (bubble != null) bubble.Show("You found me! Nice eyes.");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator Lost(string msg)
        {
            done = true;
            // mostra dov'era
            float from = zoom;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { ZoomAt(target.position, Mathf.Lerp(from, 3f, k) / zoom); crowd.position += (Vector3)(-(Vector2)target.position) * 0.1f; Clamp(); yield return null; }
            if (bubble != null) bubble.Show(msg);
            yield return new WaitForSeconds(1.8f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
