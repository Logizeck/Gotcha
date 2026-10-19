using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Snow globe" — fase 1: scuoti il telefono; mentre la neve si posa, i fiocchi formano per un attimo un
    /// codice di 3 cifre (sempre lo stesso: si può scuotere di nuovo per rivederlo). Fase 2: la base ha tre anelli con
    /// le cifre; tocca un anello per sceglierlo e inclina il telefono a destra/sinistra per farlo girare, finché sotto
    /// la freccia c'è il codice. Fase 3: la sfera si apre e dentro c'è una chiave minuscola: pizzica per ingrandirla e
    /// trascinala nella serratura del carillon. Troppo piccola o troppo grande = si inceppa (si perde un cuore, 3).
    /// </summary>
    public class SnowGlobeLevel : LevelController
    {
        [SerializeField] Transform globe;                  // centro della sfera
        [SerializeField] SpriteRenderer dome;
        [SerializeField] Transform[] flakes;
        [SerializeField] Transform[] rings;                // i tre anelli (centro)
        [SerializeField] TextMesh[] ringDigits;            // 6 cifre per anello
        [SerializeField] SpriteRenderer[] ringGlows;       // evidenzia l'anello scelto
        [SerializeField] Transform ringGroup;              // anelli + freccia (spariscono nella fase 3)
        [SerializeField] Transform musicBox;
        [SerializeField] SpriteRenderer musicBoxArt;
        [SerializeField] Sprite musicBoxOpen;
        [SerializeField] Transform keyhole;
        [SerializeField] Transform key;                    // punta della chiave
        [SerializeField] Transform keyArt;                 // figlio scalato
        [SerializeField] Transform[] notes;
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] TextMesh goal;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float radius = 1.25f, pixel = 0.19f, digitStep = 0.82f, ringStep = 0.62f;
        [SerializeField] float keyMin = 0.32f, keyMax = 0.62f;

        static readonly string[] Font =
        {
            "111101101101111", "010110010010111", "111001111100111", "111001111001111", "101101111001001",
            "111100111001111", "111100111101111", "111001010010010", "111101111101111", "111101111001111",
        };

        Camera cam;
        int stage, lives = 3, selected;
        int[] code = new int[3];
        float[] pos = new float[3];
        Vector2[] flakeVel;
        Vector2[] homes;
        Vector2[] targets;
        float sinceShake = 99f, settle = -1f, solvedFor, bubbleUntil, clock;
        bool revealedOnce, busy, grabbingKey, pinching;
        float keyScale = 0.12f, pinchStart, scaleStart;
        Vector2 keyOffset;
        Vector3 keyHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            for (int i = 0; i < 3; i++) code[i] = Random.Range(1, 10);
            for (int i = 0; i < 3; i++)
            {
                int start;
                do start = Random.Range(0, 10); while (start == code[i]);
                pos[i] = start;
            }
            flakeVel = new Vector2[flakes.Length];
            homes = new Vector2[flakes.Length];
            for (int i = 0; i < flakes.Length; i++)
            {
                float x = Random.Range(-1.05f, 1.05f);
                homes[i] = new Vector2(x, -radius * Mathf.Sqrt(1f - (x / radius) * (x / radius)) + Random.Range(0.12f, 0.3f));
                flakes[i].localPosition = homes[i];
            }
            BuildTargets();
            key.gameObject.SetActive(false);
            if (musicBox != null) musicBox.gameObject.SetActive(false);
            foreach (Transform n in notes) n.gameObject.SetActive(false);
            E40Kit.Hearts(hearts, lives);
            E40Kit.Dots(dots, 0);
            E40Kit.Goal(goal, "Shake the snow globe!");
            DrawRings();
            Say("So pretty... shake it!", 2f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        /// <summary>Posizioni dei fiocchi che disegnano le tre cifre (font 3x5); quelli in più restano sul fondo.</summary>
        void BuildTargets()
        {
            targets = new Vector2[flakes.Length];
            int k = 0;
            for (int d = 0; d < 3; d++)
            {
                string g = Font[code[d]];
                for (int i = 0; i < 15 && k < flakes.Length; i++)
                {
                    if (g[i] != '1') continue;
                    int col = i % 3, row = i / 3;
                    targets[k++] = new Vector2((d - 1) * digitStep + (col - 1) * pixel, 0.55f - row * pixel);
                }
            }
            for (; k < flakes.Length; k++) targets[k] = homes[k];
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            if (stage <= 1) Snow(dt);
            if (stage == 1) Rings(dt);
            if (stage == 2) KeyInput();
        }

        // ---------- Fase 1: la neve ----------

        void Snow(float dt)
        {
            if (Inputs.Shake() > 12f)
            {
                if (sinceShake > 0.3f) Sfx.Play(this, "whoosh", 0.4f);
                sinceShake = 0f;
                settle = -1f;
                for (int i = 0; i < flakes.Length; i++) flakeVel[i] += Random.insideUnitCircle * 9f * dt + Vector2.up * 6f * dt;
            }
            else sinceShake += dt;

            bool swirling = sinceShake < 0.6f;
            if (!swirling && settle < 0f && sinceShake < 1f) settle = 0f;   // appena finito di scuotere: inizia a posarsi
            if (settle >= 0f) settle += dt;

            for (int i = 0; i < flakes.Length; i++)
            {
                Vector2 p = flakes[i].localPosition;
                if (swirling)
                {
                    flakeVel[i] += new Vector2(-p.y, p.x) * 1.2f * dt;   // vortice
                    flakeVel[i] *= 1f - 1.2f * dt;
                    p += flakeVel[i] * dt;
                }
                else if (settle >= 0f && settle < 3.6f)
                {
                    // i fiocchi scendono verso le cifre, restano un attimo, poi ricadono
                    Vector2 goalPos = settle < 3.0f ? targets[i] : homes[i];
                    float rate = settle < 3.0f ? 2.6f : 4f;
                    p = Vector2.Lerp(p, goalPos + new Vector2(Mathf.Sin(clock * 2f + i) * 0.01f, 0f), 1f - Mathf.Exp(-rate * dt));
                    flakeVel[i] = Vector2.zero;
                }
                else
                {
                    p = Vector2.Lerp(p, homes[i], 1f - Mathf.Exp(-1.5f * dt));
                    flakeVel[i] = Vector2.zero;
                }
                if (p.magnitude > radius) p = p.normalized * radius;
                flakes[i].localPosition = p;
            }
            if (settle > 1.2f && !revealedOnce)
            {
                revealedOnce = true;
                Sfx.Play(this, "ding", 0.5f);
                StartCoroutine(ToRings());
            }
        }

        IEnumerator ToRings()
        {
            yield return new WaitForSeconds(2.0f);
            stage = 1;
            E40Kit.Dots(dots, 1);
            E40Kit.Goal(goal, "Tap a ring, tilt the phone to turn it");
            Say("Remember the code? Shake again to see it.", 2.6f);
            SelectRing(0);
        }

        // ---------- Fase 2: gli anelli ----------

        void SelectRing(int i)
        {
            selected = i;
            for (int k = 0; k < ringGlows.Length; k++) if (ringGlows[k] != null) ringGlows[k].enabled = k == i;
        }

        void Rings(float dt)
        {
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool _) && began && !PointerInput.IsOverUi())
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                for (int i = 0; i < rings.Length; i++)
                    if (Mathf.Abs(w.x - rings[i].position.x) < 2f && Mathf.Abs(w.y - rings[i].position.y) < 0.33f)
                    {
                        if (i != selected) Sfx.Play(this, "tick", 0.6f);
                        SelectRing(i);
                        StartCoroutine(E40Kit.Pop(rings[i], 0.05f));
                    }
            }
            float t = Inputs.Tilt().x;
            float a = Mathf.Abs(t);
            if (a > 0.12f)
            {
                float speed = Mathf.Lerp(1.2f, 6f, Mathf.Clamp01((a - 0.12f) / 0.45f));
                float before = pos[selected];
                pos[selected] -= Mathf.Sign(t) * speed * dt;   // le cifre scorrono dalla parte in cui inclini
                if (Mathf.FloorToInt(before + 0.5f) != Mathf.FloorToInt(pos[selected] + 0.5f)) Sfx.Play(this, "tick", 0.4f);
            }
            else
            {
                for (int i = 0; i < 3; i++) pos[i] = Mathf.MoveTowards(pos[i], Mathf.Round(pos[i]), 3f * dt);   // scatta sulla cifra
            }
            for (int i = 0; i < 3; i++) pos[i] = Mathf.Repeat(pos[i], 10f);
            DrawRings();

            bool ok = true;
            for (int i = 0; i < 3; i++)
            {
                int v = Mathf.RoundToInt(pos[i]) % 10;
                float off = Mathf.Abs(pos[i] - Mathf.Round(pos[i]));
                if (v != code[i] || off > 0.08f) ok = false;
            }
            solvedFor = ok ? solvedFor + dt : 0f;
            if (solvedFor > 0.5f) StartCoroutine(OpenGlobe());
        }

        void DrawRings()
        {
            for (int i = 0; i < 3; i++)
            {
                int whole = Mathf.FloorToInt(pos[i]);
                float frac = pos[i] - whole;
                for (int k = 0; k < 6; k++)
                {
                    int idx = i * 6 + k;
                    if (idx >= ringDigits.Length || ringDigits[idx] == null) continue;
                    int slot = k - 2;
                    float x = (slot - frac) * ringStep;
                    TextMesh d = ringDigits[idx];
                    d.text = (((whole + slot) % 10 + 10) % 10).ToString();
                    d.transform.localPosition = new Vector3(x, 0.02f, -0.01f);
                    Color c = Mathf.Abs(x) < 0.25f ? new Color(0.17f, 0.13f, 0.2f) : new Color(0.3f, 0.2f, 0.1f);
                    c.a = Mathf.Clamp01(1.6f - Mathf.Abs(x)) ;
                    d.color = c;
                }
            }
        }

        // ---------- Fase 3: chiave e carillon ----------

        IEnumerator OpenGlobe()
        {
            busy = true;
            stage = 2;
            Sfx.Play(this, "ding", 0.8f);
            E40Kit.Dots(dots, 2);
            E40Kit.Goal(goal, "Click! The globe opens...");
            Vector3 d0 = dome.transform.localPosition;
            Vector3 r0 = ringGroup.localPosition;
            musicBox.gameObject.SetActive(true);
            Vector3 mb = musicBox.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.0f)
            {
                dome.transform.localPosition = d0 + Vector3.up * k * 2.5f;
                dome.color = new Color(1f, 1f, 1f, 1f - k);
                ringGroup.localPosition = r0 + Vector3.down * k * 4f;
                musicBox.localPosition = mb + Vector3.down * (1f - k) * 3f;
                yield return null;
            }
            dome.gameObject.SetActive(false);
            ringGroup.gameObject.SetActive(false);
            musicBox.localPosition = mb;
            keyHome = globe.position + new Vector3(0.08f, -0.55f, 0f);
            key.position = keyHome;
            key.gameObject.SetActive(true);
            SetKeyScale(keyScale);
            Sfx.Play(this, "pop", 0.7f);
            E40Kit.Goal(goal, "Pinch the tiny key, then drag it to the lock");
            Say("A key! A teeny tiny one...", 2.4f);
            busy = false;
        }

        void SetKeyScale(float s)
        {
            keyScale = Mathf.Clamp(s, 0.08f, 1.3f);
            keyArt.localScale = Vector3.one * keyScale;
        }

        void KeyInput()
        {
            if (PointerInput.TryGetPinch(out float d))
            {
                if (!pinching) { pinching = true; pinchStart = Mathf.Max(d, 1f); scaleStart = keyScale; grabbingKey = false; }
                else SetKeyScale(scaleStart * d / pinchStart);
                return;
            }
            pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) SetKeyScale(keyScale * (1f + wheel * 0.08f));
#endif
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (grabbingKey) ReleaseKey(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return;
                // presa generosa: lungo tutto il gambo, anche quando la chiave è minuscola
                Vector2 tip = key.position, bow = tip + Vector2.left * 1.1f * keyScale;
                if (DistToSegment(w, tip, bow) < 0.6f) { grabbingKey = true; keyOffset = tip - w; }
                return;
            }
            if (!grabbingKey) return;
            Vector2 p = w + keyOffset;
            key.position = new Vector3(Mathf.Clamp(p.x, -2.4f, 2.8f), Mathf.Clamp(p.y, -4.2f, 2.4f), 0f);
            if (ended) ReleaseKey();
        }

        static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector2.Distance(p, a + ab * t);
        }

        void ReleaseKey()
        {
            grabbingKey = false;
            if (Vector2.Distance(key.position, keyhole.position) > 0.5f) return;   // posata altrove: resta lì
            if (keyScale >= keyMin && keyScale <= keyMax) { StartCoroutine(Unlock()); return; }
            StartCoroutine(Jam(keyScale < keyMin));
        }

        IEnumerator Jam(bool tooSmall)
        {
            busy = true;
            lives--;
            E40Kit.Hearts(hearts, lives);
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.8f);
            Say(tooSmall ? "Too tiny! It rattles around." : "Way too big for that lock!", 2f);
            yield return E40Kit.Shake(musicBox, 0.1f);
            if (lives <= 0)
            {
                yield return new WaitForSeconds(0.8f);
                Fail();
                yield break;
            }
            Vector3 from = key.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { key.position = Vector3.Lerp(from, keyHome, k); yield return null; }
            key.position = keyHome;
            busy = false;
        }

        IEnumerator Unlock()
        {
            busy = true;
            Vector3 from = key.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { key.position = Vector3.Lerp(from, keyhole.position, k); yield return null; }
            key.position = keyhole.position;
            Sfx.Play(this, "clunk", 0.9f);
            // giro della chiave
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { keyArt.localScale = new Vector3(keyScale, keyScale * Mathf.Cos(k * Mathf.PI), 1f); yield return null; }
            keyArt.localScale = Vector3.one * keyScale;
            key.gameObject.SetActive(false);
            musicBoxArt.sprite = musicBoxOpen;
            E40Kit.Dots(dots, 3);
            E40Kit.Goal(goal, "Ding-a-ling!");
            StartCoroutine(E40Kit.Pop(musicBox, 0.1f, 0.3f));
            yield return Melody();
            Win();
        }

        IEnumerator Melody()
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            AudioClip ding = Sfx.Clip("ding");
            float[] tune = { 0f, 4f, 7f, 12f, 7f, 4f, 5f, 9f, 12f, 16f };
            for (int i = 0; i < tune.Length; i++)
            {
                src.pitch = Mathf.Pow(2f, (tune[i] - 12f) / 12f);
                src.PlayOneShot(ding, 0.5f);
                Transform n = notes[i % notes.Length];
                StartCoroutine(Float(n, musicBox.position + new Vector3(Random.Range(-0.8f, 0.8f), 1.6f, 0f)));
                yield return new WaitForSeconds(0.28f);
            }
            yield return new WaitForSeconds(0.9f);
        }

        IEnumerator Float(Transform n, Vector3 from)
        {
            n.gameObject.SetActive(true);
            SpriteRenderer sr = n.GetComponentInChildren<SpriteRenderer>();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                n.position = from + new Vector3(Mathf.Sin(k * 9f) * 0.15f, k * 1.4f, 0f);
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f - k);
                yield return null;
            }
            n.gameObject.SetActive(false);
        }
    }
}
