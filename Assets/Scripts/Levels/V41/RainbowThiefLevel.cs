using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Rainbow thief" — un ladro ha rubato i colori dell'arcobaleno; sono rimaste solo sei luci primarie (due
    /// rosse, due verdi, due blu). Mettile a coppie nelle tre casse trasparenti per ricreare per somma di luce i colori
    /// mancanti: rosso+verde = GIALLO, verde+blu = CIANO, rosso+blu = MAGENTA (c'è un esempio luminoso). Rosso, verde e blu
    /// tornano appena li usi; alla fine le tre casse insieme danno il bianco. Quattro combinazioni sbagliate = tempo finito.
    /// </summary>
    public class RainbowThiefLevel : LevelController
    {
        [SerializeField] SpriteRenderer[] bands;       // 0 rosso, 1 giallo, 2 verde, 3 ciano, 4 blu, 5 magenta, 6 bianco
        [SerializeField] Transform[] orbs;             // 0-1 rosse, 2-3 verdi, 4-5 blu
        [SerializeField] Transform[] crates;           // con Collider2D
        [SerializeField] SpriteRenderer[] crateGlows;
        [SerializeField] SpriteRenderer[] timeSegments;
        [SerializeField] Transform thief;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float dropRadius = 0.75f;

        static readonly Color[] bandColors =
        {
            new Color(1f, 0.25f, 0.25f), new Color(1f, 0.92f, 0.2f), new Color(0.3f, 0.92f, 0.35f), new Color(0.3f, 0.92f, 1f),
            new Color(0.3f, 0.45f, 1f), new Color(1f, 0.35f, 0.92f), Color.white,
        };
        static readonly Color Grey = new Color(0.62f, 0.62f, 0.68f, 0.55f);
        static readonly int[] crateTarget = { 3, 6, 5 };          // giallo (R|G), ciano (G|B), magenta (R|B)
        static readonly int[] crateBand = { 1, 3, 5 };

        Camera cam;
        Vector3[] homes;
        int[] orbCrate;                 // -1 = sul vassoio
        bool[] sealed_;
        readonly bool[] lit = new bool[7];
        int grabbed = -1, wrong;
        Vector2 grabOffset;
        bool busy;
        Vector3 thiefHome;

        static int Mask(int orb) => orb < 2 ? 1 : orb < 4 ? 2 : 4;
        static int MaskBand(int m) => m == 1 ? 0 : m == 2 ? 2 : 4;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            homes = new Vector3[orbs.Length];
            orbCrate = new int[orbs.Length];
            for (int i = 0; i < orbs.Length; i++) { homes[i] = orbs[i].position; orbCrate[i] = -1; }
            sealed_ = new bool[crates.Length];
            foreach (SpriteRenderer b in bands) b.color = Grey;
            for (int i = 0; i < crateGlows.Length; i++) crateGlows[i].color = new Color(1f, 1f, 1f, 0f);
            thiefHome = thief.localPosition;
            S41DKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("Hehe! I stole every color! Good luck!");
        }

        void Update()
        {
            if (IsFinished) return;
            if (busy) return;
            thief.localPosition = thiefHome + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 0.08f;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (grabbed >= 0) Drop(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                grabbed = -1;
                float best = 0.5f;
                for (int i = 0; i < orbs.Length; i++)
                {
                    if (orbCrate[i] >= 0 && sealed_[orbCrate[i]]) continue;
                    float d = Vector2.Distance(w, orbs[i].position);
                    if (d < best) { best = d; grabbed = i; }
                }
                if (grabbed >= 0)
                {
                    grabOffset = (Vector2)orbs[grabbed].position - w;
                    int from = orbCrate[grabbed];
                    orbCrate[grabbed] = -1;
                    if (from >= 0) RefreshGlow(from);
                    Sfx.Play(this, "tick", 0.5f);
                }
            }
            if (grabbed >= 0)
            {
                Vector2 p = w + grabOffset;
                orbs[grabbed].position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.6f), Mathf.Clamp(p.y, -4.4f, 3.4f), orbs[grabbed].position.z);
                if (ended) Drop();
            }
        }

        int CountIn(int c) { int n = 0; foreach (int x in orbCrate) if (x == c) n++; return n; }

        Color Mix(int c)
        {
            Color sum = Color.black;
            for (int i = 0; i < orbs.Length; i++) if (orbCrate[i] == c) sum += bandColors[MaskBand(Mask(i))];
            sum.r = Mathf.Min(1f, sum.r); sum.g = Mathf.Min(1f, sum.g); sum.b = Mathf.Min(1f, sum.b);
            return sum;
        }

        void RefreshGlow(int c)
        {
            int n = CountIn(c);
            Color m = Mix(c);
            m.a = n == 0 ? 0f : 0.75f;
            crateGlows[c].color = m;
            for (int i = 0; i < orbs.Length; i++)
                if (orbCrate[i] == c)
                {
                    int k = 0;
                    for (int j = 0; j < i; j++) if (orbCrate[j] == c) k++;
                    orbs[i].position = crates[c].position + new Vector3(n == 1 ? 0f : (k == 0 ? -0.22f : 0.22f), 0f, 0f);
                }
        }

        void Drop()
        {
            int o = grabbed;
            grabbed = -1;
            int target = -1;
            for (int c = 0; c < crates.Length; c++)
                if (!sealed_[c] && CountIn(c) < 2 && Vector2.Distance(orbs[o].position, crates[c].position) < dropRadius) target = c;
            if (target < 0) { StartCoroutine(Home(o)); return; }
            orbCrate[o] = target;
            Sfx.Play(this, "pop", 0.6f);
            RefreshGlow(target);
            int band = MaskBand(Mask(o));
            if (!lit[band]) StartCoroutine(Light(band, null));
            if (CountIn(target) == 2) StartCoroutine(Evaluate(target));
        }

        IEnumerator Home(int o)
        {
            Transform t = orbs[o];
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { t.position = Vector3.Lerp(from, homes[o], k); yield return null; }
            t.position = homes[o];
        }

        IEnumerator Light(int band, string line)
        {
            lit[band] = true;
            Sfx.Play(this, "ding", 0.5f);
            Color to = bandColors[band];
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { bands[band].color = Color.Lerp(Grey, to, k); yield return null; }
            bands[band].color = to;
            if (line != null && bubble != null) bubble.Show(line);
        }

        IEnumerator Evaluate(int c)
        {
            busy = true;
            yield return new WaitForSeconds(0.35f);
            int mask = 0, count = 0;
            for (int i = 0; i < orbs.Length; i++) if (orbCrate[i] == c) { mask |= Mask(i); count++; }
            bool ok = count == 2 && mask == crateTarget[c];
            if (ok)
            {
                sealed_[c] = true;
                Haptics.Light();
                StartCoroutine(S41DKit.Pop(crates[c], 1.15f, 0.3f));
                for (int i = 0; i < orbs.Length; i++) if (orbCrate[i] == c) orbs[i].gameObject.SetActive(false);
                Color g = bandColors[crateBand[c]];
                g.a = 0.9f;
                crateGlows[c].color = g;
                yield return StartCoroutine(Light(crateBand[c], c == 0 ? "What?! You made YELLOW?" : c == 1 ? "Not my cyan!" : "Magenta too?! Unfair!"));
                bool all = true;
                foreach (bool s in sealed_) all &= s;
                if (all) { yield return StartCoroutine(Finale()); yield break; }
            }
            else
            {
                wrong++;
                Haptics.Medium();
                Sfx.Play(this, "buzz", 0.7f);
                crateGlows[c].color = new Color(1f, 0.2f, 0.2f, 0.9f);
                yield return StartCoroutine(S41DKit.Shake(crates[c], 0.1f, 0.35f));
                if (wrong - 1 < timeSegments.Length && timeSegments[wrong - 1] != null) timeSegments[wrong - 1].color = new Color(0.3f, 0.3f, 0.35f, 0.6f);
                for (int i = 0; i < orbs.Length; i++) if (orbCrate[i] == c) { orbCrate[i] = -1; StartCoroutine(Home(i)); }
                RefreshGlow(c);
                if (wrong >= 4)
                {
                    if (bubble != null) bubble.Show("Time's up! The colors are MINE! Hehehe!");
                    Sfx.Play(this, "whoosh", 0.8f);
                    yield return StartCoroutine(S41DKit.Move(thief, thiefHome + new Vector3(4f, 0f, 0f), 1.0f, 0.4f));
                    yield return new WaitForSeconds(0.6f);
                    Fail();
                    yield break;
                }
                if (bubble != null) bubble.Show(crateTarget[c] == 3 ? "Hehe! That's not yellow!" : crateTarget[c] == 6 ? "Hehe! That's not cyan!" : "Hehe! That's not magenta!");
            }
            busy = false;
        }

        IEnumerator Finale()
        {
            // le tre casse insieme fanno il bianco
            Sfx.Play(this, "whoosh", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                foreach (SpriteRenderer g in crateGlows) g.color = Color.Lerp(g.color, Color.white, k);
                yield return null;
            }
            yield return StartCoroutine(Light(6, "Red + green + blue = WHITE?! I give up!"));
            Haptics.Medium();
            S41DKit.Confetti(confetti, true);
            Sfx.Play(this, "ding", 0.9f);
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                for (int b = 0; b < bands.Length; b++) bands[b].color = Color.Lerp(bandColors[b], Color.white, 0.25f + 0.25f * Mathf.Sin(k * 8f + b));
                yield return null;
            }
            for (int b = 0; b < bands.Length; b++) bands[b].color = bandColors[b];
            yield return StartCoroutine(S41DKit.Move(thief, thiefHome + new Vector3(-4.5f, 0f, 0f), 0.9f, 0.5f));
            S41DKit.Confetti(confetti, false);
            Win();
        }
    }
}
