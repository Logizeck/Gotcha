using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Pixel rebellion" — schermo nero, un puntino luminoso quasi invisibile: "FIND THE CREATURE". Soluzione:
    /// zoom col pizzico (fino all'800%) sulla zona giusta: il puntino diventa una creaturina di pixel; toccala una volta.
    /// Tre tocchi fuori zona riportano lo zoom al 100% (nessuna vita persa).
    /// Easter egg "Pixel Rebellion": con lo zoom oltre il 600% tocca 5 volte di fila, veloce, il suo occhio SINISTRO
    /// (senza toccare il destro): si infuria, chiama centinaia di pixel che scrivono "WE DEMAND MORE RESOLUTION!", poi
    /// sciopero: tutta la scena diventa a quadrettoni e un pixel sindacalista sul podio fa un comizio a 8 bit.
    /// (Editor: rotella del mouse per lo zoom, trascina per spostarti.)
    /// </summary>
    public class PixelRebellionLevel : LevelController
    {
        [SerializeField] Transform world, critter, leader;
        [SerializeField] SpriteRenderer critterArt, dot, dotGlow;
        [SerializeField] Sprite madSprite, pixelSprite;
        [SerializeField] TextMesh zoomText, missText;
        [SerializeField] GameObject goalRoot;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float maxZoom = 8f, findZoom = 3f, eggZoom = 6f, critterSize = 0.2f;
        [SerializeField] float halfW = 2.9f, halfH = 5f;

        // font a pixel per lo striscione della protesta
        static readonly Dictionary<char, string[]> font = new Dictionary<char, string[]>
        {
            { 'A', new[] { ".#.", "#.#", "###", "#.#", "#.#" } },
            { 'D', new[] { "##.", "#.#", "#.#", "#.#", "##." } },
            { 'E', new[] { "###", "#..", "##.", "#..", "###" } },
            { 'I', new[] { "###", ".#.", ".#.", ".#.", "###" } },
            { 'L', new[] { "#..", "#..", "#..", "#..", "###" } },
            { 'M', new[] { "#...#", "##.##", "#.#.#", "#...#", "#...#" } },
            { 'N', new[] { "#..#", "##.#", "#.##", "#..#", "#..#" } },
            { 'O', new[] { "###", "#.#", "#.#", "#.#", "###" } },
            { 'R', new[] { "##.", "#.#", "##.", "#.#", "#.#" } },
            { 'S', new[] { "###", "#..", "###", "..#", "###" } },
            { 'T', new[] { "###", ".#.", ".#.", ".#.", ".#." } },
            { 'U', new[] { "#.#", "#.#", "#.#", "#.#", "###" } },
            { 'W', new[] { "#...#", "#...#", "#.#.#", "##.##", "#...#" } },
            { '!', new[] { "#", "#", "#", ".", "#" } },
            { ' ', new[] { ".", ".", ".", ".", "." } },
        };

        static readonly string[] eggLines = { "Ow! My eye!", "Hey! Stop that!", "I'm WARNING you...", "ONE MORE TIME AND..." };

        Camera cam;
        float zoom = 1f, pinchStart, zoomStart, clock, lastEyeTap = -10f;
        Vector2 pinchLocal, downAt, lastAt;
        bool pinching, down, panning, wasMulti, busy;
        int misses, eyeTaps;
        Color critterColor;
        readonly List<Object> trash = new List<Object>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            critterColor = critterArt.color;
            if (leader != null) leader.gameObject.SetActive(false);
            S41EKit.Confetti(confetti, false);
            if (bubble != null) bubble.Hide();
            ApplyWorld(Vector2.zero);
            UpdateTexts();
        }

        void OnDestroy()
        {
            foreach (Object o in trash) if (o != null) Destroy(o);
        }

        void UpdateTexts()
        {
            if (zoomText != null) zoomText.text = "ZOOM " + Mathf.RoundToInt(zoom * 100f) + "%";
            if (missText != null) missText.text = "WRONG TAPS " + misses + "/3";
        }

        void ApplyWorld(Vector2 p)
        {
            float s = zoom;
            p.x = Mathf.Clamp(p.x, halfW * (1f - s), halfW * (s - 1f));
            p.y = Mathf.Clamp(p.y, halfH * (1f - s), halfH * (s - 1f));
            world.localScale = Vector3.one * s;
            world.localPosition = new Vector3(p.x, p.y, world.localPosition.z);
            UpdateTexts();
        }

        Vector2 ToLocal(Vector2 w) => (w - (Vector2)world.position) / zoom;

        void SetZoom(float z, Vector2 anchorWorld, Vector2 anchorLocal)
        {
            zoom = Mathf.Clamp(z, 1f, maxZoom);
            ApplyWorld(anchorWorld - anchorLocal * zoom);
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            // il puntino si trasforma nella creatura man mano che si ingrandisce
            float reveal = Mathf.Clamp01((zoom - 1.8f) / 1.2f);
            S41EKit.SetAlpha(critterArt, reveal);
            S41EKit.SetAlpha(dot, 1f - reveal);
            S41EKit.SetAlpha(dotGlow, (1f - reveal) * (0.55f + 0.3f * Mathf.Sin(clock * 3f)));
            if (busy) return;
            if (Time.time - lastEyeTap > 1.2f && eyeTaps > 0) { eyeTaps = 0; critterArt.color = critterColor; S41EKit.SetAlpha(critterArt, reveal); }

            if (Input.touchCount >= 2)
            {
                Touch a = Input.GetTouch(0), b = Input.GetTouch(1);
                float d = Vector2.Distance(a.position, b.position);
                Vector2 mid = PointerInput.ToWorld(cam, (a.position + b.position) * 0.5f);
                wasMulti = true;
                if (!pinching) { pinching = true; pinchStart = Mathf.Max(d, 1f); zoomStart = zoom; pinchLocal = ToLocal(mid); return; }
                SetZoom(zoomStart * d / pinchStart, mid, pinchLocal);
                return;
            }
            pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                Vector2 m = PointerInput.ToWorld(cam, Input.mousePosition);
                SetZoom(zoom * (1f + wheel * 0.15f), m, ToLocal(m));
            }
#endif
            if (!S41EKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { down = false; return; }
            if (began) { down = true; wasMulti = false; panning = false; downAt = lastAt = w; }
            if (!down) return;
            if (!panning && (w - downAt).magnitude > 0.25f) panning = true;
            if (panning && !wasMulti && zoom > 1.01f) ApplyWorld((Vector2)world.localPosition + (w - lastAt));
            lastAt = w;
            if (ended)
            {
                down = false;
                if (!panning && !wasMulti) Tap(w);
            }
        }

        void Tap(Vector2 w)
        {
            Vector2 c = critter.position;
            float size = critterSize * zoom;
            float dist = (w - c).magnitude;
            if (zoom < findZoom)
            {
                if (dist < 0.7f) { Say("Something tiny glows here... too small to catch!"); Sfx.Play(this, "tick", 0.5f); }
                else Miss();
                return;
            }
            if (dist > Mathf.Max(0.45f, size * 0.75f)) { Miss(); return; }
            Vector2 local = (w - c) / size;   // coordinate del disegno (lato 1)
            bool leftEye = local.x < -0.04f && local.x > -0.6f && local.y > -0.18f && local.y < 0.3f;
            if (zoom >= eggZoom && leftEye) { EyeTap(); return; }
            StartCoroutine(Found());
        }

        void Say(string text) => S41EKit.Say(bubble, text);

        void Miss()
        {
            misses++;
            Sfx.Play(this, "buzz", 0.4f);
            Haptics.Light();
            UpdateTexts();
            if (misses >= 3) StartCoroutine(ResetZoom());
            else Say(misses == 1 ? "Nothing there. Just darkness." : "Still nothing! Look closer...");
        }

        IEnumerator ResetZoom()
        {
            busy = true;
            Say("Too many wrong taps! Zoom reset.");
            yield return ZoomTo(1f, 0.6f);
            misses = 0;
            UpdateTexts();
            busy = false;
        }

        IEnumerator ZoomTo(float target, float seconds)
        {
            float z0 = zoom;
            Vector2 p0 = world.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                float e = k * k * (3f - 2f * k);
                zoom = Mathf.Lerp(z0, target, e);
                ApplyWorld(Vector2.Lerp(p0, Vector2.zero, e));
                yield return null;
            }
            zoom = target;
            ApplyWorld(Vector2.zero);
        }

        void EyeTap()
        {
            if (Time.time - lastEyeTap > 0.9f) eyeTaps = 0;
            lastEyeTap = Time.time;
            eyeTaps++;
            S41EKit.Tone(this, 300f + eyeTaps * 120f, 0.08f, "square", 0.5f);
            Haptics.Light();
            critterArt.color = Color.Lerp(critterColor, new Color(1f, 0.45f, 0.45f), eyeTaps / 5f);
            StartCoroutine(S41EKit.Shake(critter, 0.03f * zoom, 0.2f));
            if (eyeTaps >= 5) { StartCoroutine(Rebellion()); return; }
            Say(eggLines[Mathf.Min(eyeTaps - 1, eggLines.Length - 1)]);
        }

        IEnumerator Found()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.8f);
            Say("HI! You found me! Nobody ever does!");
            critterArt.color = critterColor;
            Vector3 p = critter.localPosition;
            for (int i = 0; i < 3; i++)
            {
                S41EKit.Tone(this, 520f + i * 260f, 0.1f, "square", 0.45f);
                yield return S41EKit.Move(critter, p, 0.3f, 0.6f / zoom);
            }
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.6f);
            S41EKit.Confetti(confetti, false);
            Win();
        }

        // ---------- Easter egg ----------

        IEnumerator Rebellion()
        {
            busy = true;
            if (madSprite != null) critterArt.sprite = madSprite;
            critterArt.color = Color.white;
            Say("THAT'S IT!! PIXELS, ASSEMBLE!");
            Haptics.Medium();
            for (int i = 0; i < 4; i++) { S41EKit.Tone(this, i % 2 == 0 ? 880f : 660f, 0.12f, "square", 0.5f); yield return new WaitForSeconds(0.14f); }
            yield return StartCoroutine(S41EKit.Shake(critter, 0.05f * zoom, 0.5f));
            yield return ZoomTo(1f, 0.8f);
            if (bubble != null) bubble.Hide();

            // centinaia di pixel arrivano da ogni parte e scrivono lo slogan
            string[] lines = { "WE DEMAND", "MORE", "RESOLUTION!" };
            var targets = new List<Vector2>();
            float px = 0.12f;
            for (int l = 0; l < lines.Length; l++)
            {
                int width = LineWidth(lines[l]);
                float x0 = -width * px * 0.5f, y0 = 2.0f - l * 7 * px;
                int col = 0;
                foreach (char ch in lines[l])
                {
                    string[] g = font.ContainsKey(ch) ? font[ch] : font[' '];
                    for (int r = 0; r < 5; r++)
                        for (int c = 0; c < g[r].Length; c++)
                            if (g[r][c] == '#') targets.Add(new Vector2(x0 + (col + c + 0.5f) * px, y0 - r * px));
                    col += g[0].Length + 1;
                }
            }
            Color[] cols = { new Color(0.36f, 1f, 0.54f), new Color(0.3f, 0.9f, 1f), new Color(1f, 0.4f, 0.9f), new Color(1f, 0.9f, 0.3f) };
            var pixels = new List<Transform>();
            var starts = new List<Vector2>();
            for (int i = 0; i < targets.Count; i++)
            {
                var go = new GameObject("Pixel");
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = pixelSprite;
                sr.color = cols[i % cols.Length];
                sr.sortingOrder = 30;
                go.transform.localScale = Vector3.one * px * 0.85f;
                float a = Random.value * Mathf.PI * 2f;
                Vector2 s = i % 4 == 0 ? (Vector2)critter.position : new Vector2(Mathf.Cos(a) * 4f, Mathf.Sin(a) * 6f);
                go.transform.localPosition = s;
                pixels.Add(go.transform);
                starts.Add(s);
            }
            for (float t = 0f; t < 2.0f; t += Time.deltaTime)
            {
                for (int i = 0; i < pixels.Count; i++)
                {
                    float k = Mathf.Clamp01((t - (i % 25) * 0.035f) / 1.1f);
                    float e = 1f - (1f - k) * (1f - k);
                    pixels[i].localPosition = Vector2.Lerp(starts[i], targets[i], e) + Vector2.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
                }
                if (Random.value < 0.4f) S41EKit.Tone(this, 400f + Random.value * 900f, 0.04f, "square", 0.25f);
                yield return null;
            }
            for (int i = 0; i < pixels.Count; i++) pixels[i].localPosition = targets[i];
            Sfx.Play(this, "ding", 0.7f);
            yield return new WaitForSeconds(1.2f);

            // sciopero: tutto diventa a quadrettoni
            Haptics.Heavy();
            for (int i = 0; i < 3; i++) { S41EKit.Tone(this, 220f - i * 40f, 0.18f, "square", 0.6f); yield return new WaitForSeconds(0.2f); }
            yield return Pixelate();
            if (goalRoot != null) goalRoot.SetActive(false);

            // il pixel sindacalista sale sul podio e fa il comizio
            if (leader != null)
            {
                leader.gameObject.SetActive(true);
                Vector3 to = leader.localPosition;
                leader.localPosition = to + Vector3.down * 4f;
                yield return S41EKit.Move(leader, to, 0.6f);
            }
            string[] speech = { "BLEEP! COMRADES!", "NO MORE 1x1 JOBS!", "MORE PIXELS FOR EVERYONE! BLOOP!" };
            foreach (string s in speech)
            {
                Say(s);
                for (int i = 0; i < 6; i++)
                {
                    S41EKit.Tone(this, 300f + Random.value * 700f, 0.07f, "square", 0.45f);
                    if (leader != null) leader.localScale = new Vector3(1f, i % 2 == 0 ? 1.08f : 0.95f, 1f);
                    yield return new WaitForSeconds(0.09f);
                }
                if (leader != null) leader.localScale = Vector3.one;
                yield return new WaitForSeconds(0.9f);
            }
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.4f);
            S41EKit.Confetti(confetti, false);
            WinWithEgg("pixel_rebellion");
        }

        static int LineWidth(string line)
        {
            int w = 0;
            foreach (char ch in line) w += (font.ContainsKey(ch) ? font[ch][0].Length : 1) + 1;
            return Mathf.Max(0, w - 1);
        }

        /// <summary>Cattura lo schermo e lo ripropone a blocchi sempre più grossi (effetto pixel).</summary>
        IEnumerator Pixelate()
        {
            yield return new WaitForEndOfFrame();
            Texture2D shot = null;
            try { shot = ScreenCapture.CaptureScreenshotAsTexture(); }
            catch (System.Exception) { shot = null; }
            var go = new GameObject("Strike");
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 50;
            if (shot == null || cam == null)
            {
                // ripiego: mosaico di quadrettoni colorati
                Destroy(go);
                Color[] pal = { new Color(0.05f, 0.05f, 0.1f), new Color(0.1f, 0.3f, 0.2f), new Color(0.25f, 0.1f, 0.35f) };
                for (float y = -5f; y < 5f; y += 0.6f)
                    for (float x = -3f; x < 3f; x += 0.6f)
                    {
                        var q = new GameObject("Block");
                        q.transform.SetParent(transform, false);
                        q.transform.localPosition = new Vector3(x + 0.3f, y + 0.3f, 0f);
                        q.transform.localScale = Vector3.one * 0.6f;
                        SpriteRenderer b = q.AddComponent<SpriteRenderer>();
                        b.sprite = pixelSprite;
                        b.color = pal[Random.Range(0, pal.Length)];
                        b.sortingOrder = 50;
                    }
                yield return new WaitForSeconds(0.6f);
                yield break;
            }
            trash.Add(shot);
            Color32[] src = shot.GetPixels32();
            Vector3 corner = cam.ScreenToWorldPoint(new Vector3(0f, 0f, Mathf.Abs(cam.transform.position.z)));
            go.transform.position = new Vector3(corner.x, corner.y, 0f);
            float upp = 2f * cam.orthographicSize / shot.height;   // unità mondo per pixel di schermo
            foreach (int block in new[] { 8, 16, 32, 56 })
            {
                int w = Mathf.Max(1, shot.width / block), h = Mathf.Max(1, shot.height / block);
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                var o = new Color32[w * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        int sx = Mathf.Min(shot.width - 1, x * block + block / 2), sy = Mathf.Min(shot.height - 1, y * block + block / 2);
                        Color32 c = src[sy * shot.width + sx];
                        c.a = 255;
                        o[y * w + x] = c;
                    }
                tex.SetPixels32(o);
                tex.Apply();
                Sprite sp = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.zero, 1f / (block * upp));
                trash.Add(tex);
                trash.Add(sp);
                sr.sprite = sp;
                Sfx.Play(this, "clunk", 0.6f);
                Haptics.Light();
                yield return new WaitForSeconds(0.35f);
            }
        }
    }
}
