using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Dot to dot — la porta è chiusa a chiave e la chiave non c'è... ma c'è un foglio con 12 puntini numerati.
    /// Trascina il dito da 1 a 12 e di nuovo su 1: il disegno diventa una CHIAVE vera che salta fuori dal foglio;
    /// trascinala nella serratura e la porta si apre. Puntino sbagliato: bzz, il tratto riparte dall'ultimo puntino
    /// giusto. Sorpresa: sul puntino 9 è posata una mosca che blocca il tratto: toccala per farla volare via.
    /// </summary>
    public class DotToDotLevel : LevelController
    {
        [SerializeField] Transform[] dots;             // 12 puntini in ordine
        [SerializeField] LineRenderer line;
        [SerializeField] Transform fly;
        [SerializeField] int flyDot = 8;               // il puntino 9
        [SerializeField] Transform paper;
        [SerializeField] Transform key;                // perno = punta della chiave
        [SerializeField] float keyPopScale = 2.9f, keyScale = 0.9f;
        [SerializeField] Transform keyhole;
        [SerializeField] Transform doorPanel;
        [SerializeField] Transform mouse;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float hitRadius = 0.27f, flyRadius = 0.55f;
        [SerializeField] Color inkColor = new Color(0.17f, 0.13f, 0.2f), goldColor = new Color(1f, 0.78f, 0.2f);

        int connected;         // puntini già uniti (0..12)
        bool drawing, flyOn = true, closed, keyReady, keyDragging, busy, done;
        Vector2 keyGrab;
        Camera cam;
        readonly List<Vector3> pts = new List<Vector3>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            key.gameObject.SetActive(false);
            line.startColor = line.endColor = inkColor;
            UpdateLine(null);
            StartCoroutine(FlyFidget());
            if (bubble != null) { bubble.Show("Locked! Maybe I can DRAW a key... 1, 2, 3...", true); StartCoroutine(HideLater(2.6f)); }
        }

        void UpdateLine(Vector3? finger)
        {
            pts.Clear();
            for (int i = 0; i < connected; i++) pts.Add(dots[i].position);
            if (closed) pts.Add(dots[0].position);
            if (finger.HasValue && connected > 0) pts.Add(finger.Value);
            line.positionCount = pts.Count;
            line.SetPositions(pts.ToArray());
        }

        void Update()
        {
            if (busy || done || IsFinished) return;
            if (keyReady) { KeyInput(); return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                if (drawing) { drawing = false; UpdateLine(null); }
                return;
            }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began && flyOn && Vector2.Distance(w, fly.position) < flyRadius)
            {
                StartCoroutine(Shoo());
                return;
            }
            int last = connected == 0 ? 0 : connected - 1;
            if (!drawing && Vector2.Distance(w, dots[last].position) < hitRadius + 0.15f)
            {
                drawing = true;
                if (connected == 0) { connected = 1; Sfx.Play(this, "tick", 0.7f); }
            }
            if (!drawing) return;
            Track(w);
            if (drawing) UpdateLine(w);
            if (ended) { drawing = false; UpdateLine(null); }
        }

        void Track(Vector2 w)
        {
            int next = connected == dots.Length ? 0 : connected;
            if (Vector2.Distance(w, dots[next].position) < hitRadius)
            {
                if (next == flyDot && flyOn)
                {
                    Wrong("Bzzz! Busy here!");
                    return;
                }
                if (next == 0) { StartCoroutine(Complete()); return; }
                connected++;
                Sfx.Play(this, "tick", 0.8f);
                Haptics.Light();
                return;
            }
            for (int i = connected; i < dots.Length; i++)
            {
                if (i == next || Vector2.Distance(w, dots[i].position) >= hitRadius) continue;
                Wrong(null);
                return;
            }
        }

        void Wrong(string say)
        {
            drawing = false;
            UpdateLine(null);
            Sfx.Play(this, "buzz", 0.5f);
            Sfx.Buzz();
            if (bubble != null && say != null) bubble.Show(say, true);
        }

        IEnumerator FlyFidget()
        {
            while (flyOn)
            {
                yield return new WaitForSeconds(Random.Range(0.6f, 1.6f));
                if (!flyOn) yield break;
                fly.localRotation = Quaternion.Euler(0, 0, Random.Range(-40f, 40f));   // si gira e si pulisce le zampe
            }
        }

        IEnumerator Shoo()
        {
            flyOn = false;
            drawing = false;
            Sfx.Play(this, "buzz", 0.6f);
            if (bubble != null) bubble.Show("Bzz! Fine, fine, I'm going!", true);
            Vector3 a = fly.position;
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                Vector3 p = a + new Vector3(Mathf.Sin(k * 9f) * 0.5f + k * 2.5f, k * k * 5f, 0f);
                Vector3 d = p - fly.position;
                if (d.sqrMagnitude > 1e-6f) fly.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                fly.position = p;
                yield return null;
            }
            fly.gameObject.SetActive(false);
            if (bubble != null && !done) bubble.Hide();
        }

        IEnumerator Complete()
        {
            busy = true;
            drawing = false;
            closed = true;
            UpdateLine(null);
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Medium();
            for (float k = 0f; k < 0.8f; k += Time.deltaTime)
            {
                line.startColor = line.endColor = Color.Lerp(inkColor, goldColor, Mathf.PingPong(k * 5f, 1f));
                yield return null;
            }
            line.startColor = line.endColor = goldColor;
            // la chiave salta fuori dal foglio
            line.enabled = false;
            key.gameObject.SetActive(true);
            Vector3 center = paper.position + new Vector3(0.6f * keyPopScale + 0.1f, 0f, 0f);
            key.position = center;
            Sfx.Play(this, "pop", 0.9f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                key.localScale = Vector3.one * keyPopScale * (1f + Mathf.Sin(k * Mathf.PI) * 0.15f);
                yield return null;
            }
            Vector3 rest = paper.position + new Vector3(0.6f * keyScale, 0.2f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                float e = 1f - (1f - k) * (1f - k);
                key.localScale = Vector3.one * Mathf.Lerp(keyPopScale, keyScale, e);
                key.position = Vector3.Lerp(center, rest, e) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.8f;
                yield return null;
            }
            key.localScale = Vector3.one * keyScale;
            key.position = rest;
            if (bubble != null) bubble.Show("A real key! Into the lock!", true);
            keyReady = true;
            busy = false;
        }

        void KeyInput()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { keyDragging = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            Vector2 keyMid = key.position + new Vector3(-0.6f * keyScale, 0f, 0f);
            if (began && Vector2.Distance(w, keyMid) < 0.9f) { keyDragging = true; keyGrab = (Vector2)key.position - w; }
            if (!keyDragging) return;
            Vector2 p = w + keyGrab;
            p.x = Mathf.Clamp(p.x, -1.5f, 2.7f);
            p.y = Mathf.Clamp(p.y, -4.2f, 3.4f);
            key.position = p;
            if (Vector2.Distance(p, keyhole.position) < 0.6f) { keyDragging = false; StartCoroutine(Unlock()); return; }
            if (ended) keyDragging = false;
        }

        IEnumerator Unlock()
        {
            done = true;
            key.position = keyhole.position + new Vector3(0.05f, 0f, 0f);
            key.SetParent(doorPanel, true);
            Vector3 ks = key.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                key.localScale = new Vector3(ks.x, ks.y * Mathf.Cos(k * Mathf.PI * 2f), ks.z);   // la chiave gira
                yield return null;
            }
            key.localScale = ks;
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Light();
            Vector3 ps = doorPanel.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                float e = 1f - (1f - k) * (1f - k);
                doorPanel.localScale = new Vector3(ps.x * Mathf.Lerp(1f, 0.08f, e), ps.y, ps.z);
                yield return null;
            }
            if (bubble != null) bubble.Show("Art saves the day!", true);
            Vector3 m0 = mouse.position, m1 = keyhole.position + new Vector3(-0.9f, -0.6f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                mouse.position = Vector3.Lerp(m0, m1, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f)) * 0.2f;
                yield return null;
            }
            yield return new WaitForSeconds(0.6f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && flyOn) bubble.Hide();
        }
    }
}
