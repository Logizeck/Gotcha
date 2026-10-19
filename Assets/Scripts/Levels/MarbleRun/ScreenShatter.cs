using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// "Schermo rotto" (easter egg di Marble Run 2): la biglia colpisce il vetro del telefono. Schianto di vetro
    /// sintetizzato, punto d'impatto, crepe che si allargano fino a coprire tutto lo schermo e schegge che piovono giù.
    /// </summary>
    public class ScreenShatter : MonoBehaviour
    {
        [SerializeField] Sprite[] shards;
        [SerializeField] Sprite impact;
        [SerializeField] Material lineMaterial;
        [SerializeField] int sortingOrder = 50;   // sopra tutto tranne il fumetto (60)

        AudioClip crash;
        Material ownMaterial;
        Camera cam;
        float minX, maxX, minY, maxY;

        void OnDestroy()
        {
            if (crash != null) Destroy(crash);
            if (ownMaterial != null) Destroy(ownMaterial);
        }

        /// <summary>Rompe lo schermo partendo dal punto at (mondo). Dura circa 2.6 secondi.</summary>
        public IEnumerator Play(Vector2 at)
        {
            cam = Camera.main;
            float h = cam != null ? cam.orthographicSize : 5f, w = cam != null ? h * cam.aspect : 2.9f;
            Vector2 c = cam != null ? (Vector2)cam.transform.position : Vector2.zero;
            minX = c.x - w - 0.3f; maxX = c.x + w + 0.3f; minY = c.y - h - 0.3f; maxY = c.y + h + 0.3f;
            if (lineMaterial == null) { ownMaterial = new Material(Shader.Find("Sprites/Default")); lineMaterial = ownMaterial; }

            // schianto: suono forte, vibrazione lunga, lampo bianco
            AudioSource src = GetComponent<AudioSource>();
            if (src == null) { src = gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            if (crash == null) crash = CrashClip();
            src.PlayOneShot(crash, 1f);
            Haptics.Vibrate(600);
            LineRenderer flash = Line("Flash", 0, new Color(1f, 1f, 1f, 0.85f), Mathf.Max(maxX - minX, maxY - minY) * 1.2f);
            flash.positionCount = 2;
            flash.SetPosition(0, new Vector3(minX - 1f, c.y, 0));
            flash.SetPosition(1, new Vector3(maxX + 1f, c.y, 0));

            if (impact != null)
            {
                SpriteRenderer sr = MakeSprite("Impact", impact, at, sortingOrder + 4);
                StartCoroutine(Pop(sr.transform, 1.6f));
            }

            // crepe principali a raggiera, più rami: crescono tutte insieme
            var growing = new List<IEnumerator>();
            int rays = 13;
            for (int i = 0; i < rays; i++)
            {
                float a = (i + Random.Range(-0.3f, 0.3f)) / rays * Mathf.PI * 2f;
                growing.Add(Crack(at, a, 99f, Random.Range(9f, 13f), 2));
            }
            StartCoroutine(Flash(flash));
            StartCoroutine(Rain());
            foreach (IEnumerator g in growing) StartCoroutine(g);
            yield return new WaitForSeconds(0.35f);
            Haptics.Heavy();
            yield return new WaitForSeconds(0.5f);
            // altre crepe da punti a caso: lo schermo si riempie
            for (int i = 0; i < 16; i++)
            {
                Vector2 p = new Vector2(Random.Range(minX + 0.4f, maxX - 0.4f), Random.Range(minY + 0.4f, maxY - 0.4f));
                StartCoroutine(Crack(p, Random.Range(0f, Mathf.PI * 2f), Random.Range(1.2f, 3f), 9f, 1));
                if (i % 4 == 0) Haptics.Light();
                yield return new WaitForSeconds(0.06f);
            }
            yield return new WaitForSeconds(0.9f);
        }

        IEnumerator Flash(LineRenderer flash)
        {
            // il bianco si spegne ma lo schermo resta un po' lattiginoso
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                Color col = new Color(1f, 1f, 1f, Mathf.Lerp(0.85f, 0.12f, k));
                flash.startColor = flash.endColor = col;
                yield return null;
            }
        }

        IEnumerator Pop(Transform t, float size)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
            {
                t.localScale = Vector3.one * size * Mathf.Lerp(0.3f, 1.15f, k);
                yield return null;
            }
            t.localScale = Vector3.one * size;
        }

        /// <summary>Una crepa che avanza a zig-zag (con un'ombra scura sotto) e ogni tanto si ramifica.</summary>
        IEnumerator Crack(Vector2 from, float angle, float maxLen, float speed, int depth)
        {
            LineRenderer shadow = Line("CrackShadow", 1, new Color(0.1f, 0.1f, 0.15f, 0.45f), 0.075f);
            LineRenderer line = Line("Crack", 2, new Color(1f, 1f, 1f, 0.95f), depth >= 2 ? 0.035f : 0.025f);
            var pts = new List<Vector3> { from };
            Vector2 p = from;
            float len = 0f, nextStep = Random.Range(0.12f, 0.3f), walked = 0f;
            Add(line, shadow, pts, p);
            while (len < maxLen)
            {
                float d = speed * Time.deltaTime;
                walked += d;
                len += d;
                if (walked >= nextStep)
                {
                    angle += Random.Range(-0.45f, 0.45f);
                    p += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * walked;
                    walked = 0f;
                    nextStep = Random.Range(0.12f, 0.3f);
                    Add(line, shadow, pts, p);
                    if (depth > 0 && Random.value < 0.12f)
                        StartCoroutine(Crack(p, angle + (Random.value < 0.5f ? -1f : 1f) * Random.Range(0.5f, 1.1f), Random.Range(0.6f, 2.2f), speed * 0.8f, depth - 1));
                    if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY) yield break;
                }
                yield return null;
            }
        }

        static void Add(LineRenderer a, LineRenderer b, List<Vector3> pts, Vector2 p)
        {
            pts.Add(p);
            a.positionCount = pts.Count; a.SetPosition(pts.Count - 1, p);
            b.positionCount = pts.Count; b.SetPosition(pts.Count - 1, p);
        }

        /// <summary>Pioggia di schegge: si staccano un po' ovunque e cadono girando.</summary>
        IEnumerator Rain()
        {
            if (shards == null || shards.Length == 0) yield break;
            for (int i = 0; i < 70; i++)
            {
                bool top = i % 3 == 0;   // alcune arrivano da sopra lo schermo
                Vector2 p = new Vector2(Random.Range(minX, maxX), top ? maxY + 0.3f : Random.Range(minY + 2f, maxY));
                StartCoroutine(Fall(MakeSprite("Shard", shards[Random.Range(0, shards.Length)], p, sortingOrder + 3).transform));
                yield return new WaitForSeconds(0.03f);
            }
        }

        IEnumerator Fall(Transform t)
        {
            t.localScale = Vector3.one * Random.Range(0.8f, 1.8f);
            t.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            Vector2 v = new Vector2(Random.Range(-1.2f, 1.2f), Random.Range(-0.5f, 1.5f));
            float spin = Random.Range(-400f, 400f);
            while (t != null && t.position.y > minY - 1f)
            {
                v.y -= 9f * Time.deltaTime;
                t.position += (Vector3)(v * Time.deltaTime);
                t.Rotate(0, 0, spin * Time.deltaTime);
                yield return null;
            }
        }

        SpriteRenderer MakeSprite(string name, Sprite s, Vector2 at, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(at.x, at.y, 0);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.sortingOrder = order;
            return sr;
        }

        LineRenderer Line(string name, int order, Color color, float width)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.sharedMaterial = lineMaterial;
            lr.widthMultiplier = width;
            lr.numCapVertices = 2;
            lr.startColor = lr.endColor = color;
            lr.sortingOrder = sortingOrder + order;
            lr.positionCount = 0;
            return lr;
        }

        /// <summary>Schianto di vetro: botta di rumore secco e poi tanti tintinnii acuti che si spengono.</summary>
        static AudioClip CrashClip()
        {
            const int rate = 22050;
            int n = (int)(rate * 1.8f);
            var data = new float[n];
            var rnd = new System.Random(55);
            float prev = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float noise = (float)rnd.NextDouble() * 2f - 1f;
                float hp = noise - prev;   // rumore "brillante" (passa-alto)
                prev = noise;
                data[i] = hp * 0.9f * Mathf.Exp(-t * 7f) + noise * 0.5f * Mathf.Exp(-t * 18f)
                        + Mathf.Sin(t * 2f * Mathf.PI * 70f) * 0.6f * Mathf.Exp(-t * 14f);   // colpo sordo
            }
            // tintinnii: piccole note acute sparse
            for (int k = 0; k < 60; k++)
            {
                int start = (int)(rate * (0.02f + 1.3f * (float)System.Math.Pow(rnd.NextDouble(), 1.6)));
                float f = 2200f + (float)rnd.NextDouble() * 4800f, amp = 0.25f + (float)rnd.NextDouble() * 0.3f;
                int len = (int)(rate * 0.12f);
                for (int i = 0; i < len && start + i < n; i++)
                {
                    float t = i / (float)rate;
                    data[start + i] += Mathf.Sin(t * 2f * Mathf.PI * f) * amp * Mathf.Exp(-t * 45f);
                }
            }
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
            AudioClip clip = AudioClip.Create("glass_crash", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
