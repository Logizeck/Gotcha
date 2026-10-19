using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Paint mixer" — il gatto pittore vuole un colore preciso (la macchia sul cavalletto). Tocca i tubetti
    /// (rosso, giallo, blu, bianco, nero) per far cadere una goccia nel secchio: il colore si mescola come le tempere
    /// (rosso+giallo = arancio, giallo+blu = verde, rosso+blu = viola, il bianco schiarisce). Il misuratore dice quanto
    /// sei vicino; EMPTY svuota il secchio. Tre giri: un secondario, una tinta chiara e infine il trucco: il "colore
    /// della cosa preferita del gatto", SALMONE (niente campione: 2 rosso + 1 giallo + 2 bianco).
    /// </summary>
    public class PaintMixerLevel : LevelController
    {
        [SerializeField] TapButton[] tubes;               // rosso, giallo, blu, bianco, nero
        [SerializeField] TapButton emptyButton;
        [SerializeField] SpriteRenderer surface;          // la vernice nel secchio
        [SerializeField] SpriteRenderer dropTemplate;     // goccia (spenta) da clonare
        [SerializeField] SpriteRenderer rippleTemplate;   // anello d'onda (spento) da clonare
        [SerializeField] SpriteRenderer swatch;           // il campione sulla tela
        [SerializeField] SpriteRenderer painting;         // la pennellata del gatto (si tinge col colore fatto)
        [SerializeField] SpriteRenderer paintedFish;      // il salmone dipinto alla fine
        [SerializeField] TextMesh canvasLabel;
        [SerializeField] TextMesh matchText;
        [SerializeField] TextMesh roundText;
        [SerializeField] TextMesh dropsText;
        [SerializeField] Transform meterFill;             // GaugeBar: scala x da 0 a 1
        [SerializeField] SpriteRenderer meterColor;       // il riempimento della barra (cambia colore)
        [SerializeField] Transform brushArm;              // perno alla spalla del gatto
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int capacity = 10;
        [SerializeField] float matchPercent = 90f;

        static readonly Color[] pigments =
        {
            new Color(0.91f, 0.26f, 0.25f), new Color(1f, 0.82f, 0.24f), new Color(0.24f, 0.48f, 0.88f),
            Color.white, new Color(0.18f, 0.16f, 0.2f),
        };

        // ricette (gocce di R, Y, B, W, K) e nomi dei bersagli
        static readonly int[][] round1 = { new[] { 1, 1, 0, 0, 0 }, new[] { 0, 1, 1, 0, 0 }, new[] { 1, 0, 1, 0, 0 } };
        static readonly string[] round1Names = { "ORANGE", "GREEN", "PURPLE" };
        static readonly int[][] round2 = { new[] { 0, 1, 1, 1, 0 }, new[] { 1, 0, 1, 1, 0 }, new[] { 1, 1, 0, 1, 0 } };
        static readonly string[] round2Names = { "MINT", "LILAC", "PEACH" };
        static readonly int[] salmon = { 2, 1, 0, 2, 0 };

        readonly int[] counts = new int[5];
        int inFlight, round;
        Color target;
        bool busy, done;
        Vector3 surfaceBase, surfaceScale;

        public override void OnLevelStart()
        {
            for (int i = 0; i < tubes.Length; i++)
            {
                int k = i;
                tubes[i].Pressed += _ => Squeeze(k);
            }
            emptyButton.Pressed += _ => { if (!busy && !done && inFlight == 0 && Total() > 0) StartCoroutine(Pour()); };
            surfaceBase = surface.transform.localPosition;
            surfaceScale = surface.transform.localScale;
            dropTemplate.gameObject.SetActive(false);
            rippleTemplate.gameObject.SetActive(false);
            painting.gameObject.SetActive(false);
            paintedFish.gameObject.SetActive(false);
            NextRound();
            Say("Mix me THIS colour, please!", 2.4f);
        }

        int Total() { int t = 0; foreach (int c in counts) t += c; return t; }

        void NextRound()
        {
            round++;
            roundText.text = "ROUND " + round + "/3";
            int[] recipe;
            string name;
            if (round == 1) { int r = Random.Range(0, round1.Length); recipe = round1[r]; name = round1Names[r]; }
            else if (round == 2) { int r = Random.Range(0, round2.Length); recipe = round2[r]; name = round2Names[r]; }
            else { recipe = salmon; name = "SALMON"; }
            target = Mix(recipe);
            swatch.color = target;
            swatch.gameObject.SetActive(round < 3);   // il trucco: l'ultimo è solo una parola
            canvasLabel.text = round < 3 ? name : "SALMON\n?";
            canvasLabel.transform.localPosition = new Vector3(canvasLabel.transform.localPosition.x, round < 3 ? -0.5f : 0f, canvasLabel.transform.localPosition.z);
            canvasLabel.characterSize = round < 3 ? 0.035f : 0.06f;
            Refresh();
        }

        /// <summary>Mescola come le tempere: RYB (cubo di Gossett-Chen) → RGB, poi bianco e nero in proporzione.</summary>
        static Color Mix(int[] c)
        {
            int total = c[0] + c[1] + c[2] + c[3] + c[4];
            if (total == 0) return Color.white;
            int m = Mathf.Max(c[0], Mathf.Max(c[1], c[2]));
            Color col;
            float w, k;
            if (m > 0)
            {
                col = Ryb(c[0] / (float)m, c[1] / (float)m, c[2] / (float)m);
                w = c[3] / (float)total;
                k = c[4] / (float)total;
            }
            else { col = Color.white; w = 0f; k = c[4] / (float)(c[3] + c[4]); }
            col = Color.Lerp(col, Color.white, w);
            return Color.Lerp(col, new Color(0.12f, 0.1f, 0.14f), k);
        }

        static Color Ryb(float r, float y, float b)
        {
            // gli otto vertici del cubo RYB
            Color c000 = Color.white, c100 = new Color(1f, 0f, 0f), c010 = new Color(1f, 1f, 0f), c001 = new Color(0.163f, 0.373f, 0.6f);
            Color c110 = new Color(1f, 0.5f, 0f), c101 = new Color(0.5f, 0f, 0.5f), c011 = new Color(0f, 0.66f, 0.2f), c111 = new Color(0.2f, 0.094f, 0f);
            Color x00 = Color.Lerp(c000, c100, r), x10 = Color.Lerp(c010, c110, r), x01 = Color.Lerp(c001, c101, r), x11 = Color.Lerp(c011, c111, r);
            return Color.Lerp(Color.Lerp(x00, x10, y), Color.Lerp(x01, x11, y), b);
        }

        float Percent()
        {
            if (Total() == 0) return 0f;
            Color c = Mix(counts);
            float d = Mathf.Sqrt((c.r - target.r) * (c.r - target.r) + (c.g - target.g) * (c.g - target.g) + (c.b - target.b) * (c.b - target.b));
            return Mathf.Clamp01(1f - d / 0.65f) * 100f;
        }

        void Refresh()
        {
            int n = Total();
            dropsText.text = "DROPS " + n + "/" + capacity;
            float pct = Percent();
            matchText.text = n == 0 ? "--" : Mathf.RoundToInt(pct) + "%";
            meterFill.localScale = new Vector3(Mathf.Max(0.001f, pct / 100f), 1f, 1f);
            if (meterColor != null) meterColor.color = Color.Lerp(new Color(1f, 0.45f, 0.3f), new Color(0.3f, 0.85f, 0.4f), Mathf.InverseLerp(50f, matchPercent, pct));
        }

        void Squeeze(int i)
        {
            if (busy || done) return;
            if (Total() + inFlight >= capacity)
            {
                Sfx.Play(this, "buzz", 0.4f);
                Say("The bucket is full! Tap EMPTY.", 1.8f);
                return;
            }
            Sfx.Play(this, "tick");
            StartCoroutine(Drop(i));
        }

        IEnumerator Drop(int i)
        {
            inFlight++;
            SpriteRenderer d = Instantiate(dropTemplate, dropTemplate.transform.parent);
            d.gameObject.SetActive(true);
            d.color = pigments[i];
            Vector3 from = tubes[i].transform.position + new Vector3(0f, 0.7f, 0f);
            Vector3 to = surface.transform.parent.TransformPoint(surfaceBase) + new Vector3(0f, 0.05f, 0f);
            float peak = Mathf.Max(from.y, to.y) + 1.1f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f)
            {
                float x = Mathf.Lerp(from.x, to.x, t);
                // parabola che passa per from, peak e to
                float y = (1 - t) * (1 - t) * from.y + 2 * (1 - t) * t * (2 * peak - (from.y + to.y) * 0.5f) + t * t * to.y;
                d.transform.position = new Vector3(x, y, 0f);
                yield return null;
            }
            Destroy(d.gameObject);
            inFlight--;
            counts[i]++;
            Sfx.Play(this, "pop", 0.7f);
            Color now = Mix(counts);
            if (!surface.gameObject.activeSelf) { surface.gameObject.SetActive(true); surface.color = now; }
            StartCoroutine(Ripple(now));
            StartCoroutine(Tint(surface.color, now));
            int n = Mathf.Min(Total(), capacity);
            surface.transform.localPosition = surfaceBase + new Vector3(0f, 0.012f * n, 0f);
            surface.transform.localScale = new Vector3(surfaceScale.x * (0.86f + 0.014f * n), surfaceScale.y, 1f);
            Refresh();
            if (inFlight == 0 && !busy && !done && Percent() >= matchPercent) StartCoroutine(Matched());
            else if (inFlight == 0 && counts[4] > 0 && counts[4] * 2 >= Total() && Total() >= 3 && !busy)
                Say("That's not paint. That's MUD.", 1.8f);
        }

        IEnumerator Tint(Color a, Color b)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f) { surface.color = Color.Lerp(a, b, t); yield return null; }
            surface.color = b;
        }

        IEnumerator Ripple(Color c)
        {
            SpriteRenderer r = Instantiate(rippleTemplate, rippleTemplate.transform.parent);
            r.gameObject.SetActive(true);
            r.transform.position = surface.transform.position + new Vector3(0f, 0f, -0.01f);
            Color rc = Color.Lerp(c, Color.white, 0.5f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.5f)
            {
                r.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.25f, t);
                r.color = new Color(rc.r, rc.g, rc.b, 1f - t);
                yield return null;
            }
            Destroy(r.gameObject);
        }

        IEnumerator Pour()
        {
            busy = true;
            Sfx.Play(this, "whoosh", 0.7f);
            Vector3 s = surface.transform.localScale;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.4f)
            {
                surface.transform.localScale = new Vector3(s.x * (1f - t), s.y * (1f - t), 1f);
                yield return null;
            }
            for (int i = 0; i < counts.Length; i++) counts[i] = 0;
            surface.gameObject.SetActive(false);
            surface.transform.localScale = surfaceScale;
            surface.transform.localPosition = surfaceBase;
            Refresh();
            busy = false;
        }

        IEnumerator Matched()
        {
            busy = true;
            Color made = Mix(counts);
            Sfx.Play(this, "ding", 0.7f);
            Say(round < 3 ? "Purr-fect! Hold still..." : "SALMON! My favourite!", 0f);
            // il gatto dà tre pennellate
            SpriteRenderer art = round < 3 ? painting : paintedFish;
            art.gameObject.SetActive(true);
            art.color = made;
            Vector3 full = art.transform.localScale;
            for (int stroke = 0; stroke < 3; stroke++)
            {
                for (float t = 0f; t < 1f; t += Time.deltaTime / 0.28f)
                {
                    brushArm.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI) * 28f - 6f);
                    float k = (stroke + t) / 3f;
                    art.transform.localScale = new Vector3(full.x * k, full.y * Mathf.Min(1f, k * 1.6f), 1f);
                    yield return null;
                }
                Sfx.Play(this, "whoosh", 0.3f);
            }
            brushArm.localRotation = Quaternion.identity;
            art.transform.localScale = full;
            if (round >= 3)
            {
                done = true;
                Sfx.Play(this, "ding", 0.9f);
                Say("A masterpiece. A DELICIOUS masterpiece.", 0f);
                yield return new WaitForSeconds(2.2f);
                Win();
                yield break;
            }
            yield return new WaitForSeconds(1.4f);
            Say(round == 1 ? "Now something lighter..." : "Last one: my FAVOURITE thing's colour!", 2.8f);
            art.gameObject.SetActive(false);
            for (int i = 0; i < counts.Length; i++) counts[i] = 0;
            surface.gameObject.SetActive(false);
            surface.transform.localPosition = surfaceBase;
            surface.transform.localScale = surfaceScale;
            NextRound();
            busy = false;
        }

        int said;

        /// <summary>Fumetto del gatto; si chiude da solo dopo "seconds" (0 = resta) se nel frattempo non ha detto altro.</summary>
        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            int id = ++said;
            if (seconds > 0f) StartCoroutine(HideLater(seconds, id));
        }

        IEnumerator HideLater(float seconds, int id)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && id == said) bubble.Hide();
        }
    }
}
