using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Why the chicken" — perché il pulcino attraversa la strada? Quattro corsie di traffico (con un'aiuola
    /// sicura in mezzo) e dall'altra parte c'è la mamma. Tocca = un saltello avanti, scorri giù = indietro, scorri a
    /// destra/sinistra = un passo di lato. Se una macchina lo prende: frittella di piume e si ricomincia; 3 vite.
    /// </summary>
    public class ChickenLevel : LevelController
    {
        [SerializeField] Transform chick;
        [SerializeField] SpriteRenderer chickArt;
        [SerializeField] Sprite chickSprite, flatSprite;
        [SerializeField] Transform mom;
        [SerializeField] Transform[] lanes;           // radici delle corsie (y = centro corsia), figli = veicoli
        [SerializeField] float[] laneSpeeds;          // unità/s, il segno è la direzione
        [SerializeField] float[] rows = { -3.6f, -2.5f, -1.4f, -0.3f, 0.8f, 1.9f, 3.0f };
        [SerializeField] Transform[] feathers;
        [SerializeField] SpriteRenderer[] lives;
        [SerializeField] TextMesh banner;
        [SerializeField] Transform hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float hopTime = 0.13f, sideStep = 0.8f, swipe = 0.45f, wrap = 5.3f, chickHalf = 0.18f;

        static readonly string[] Ouch = { "SPLAT!", "Look both ways!", "Honk honk!", "Not today, chick." };

        Camera cam;
        float[] lengths;
        int row, livesLeft = 3, queued, deaths;
        bool hopping, dying, done, tracking, consumed;
        Vector2 touchStart;
        float bubbleUntil, clock, safeFor;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            int total = 0;
            foreach (Transform l in lanes) total += l.childCount;
            lengths = new float[total];
            int k = 0;
            foreach (Transform l in lanes)
                foreach (Transform v in l)
                {
                    SpriteRenderer sr = v.GetComponentInChildren<SpriteRenderer>();
                    lengths[k++] = sr != null ? sr.bounds.size.x : 1.2f;
                }
            foreach (Transform f in feathers) f.gameObject.SetActive(false);
            if (banner != null) banner.gameObject.SetActive(false);
            if (hearts != null) hearts.gameObject.SetActive(false);
            ResetChick();
            Say("Mom's waiting! Tap to hop.", 2.2f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void ResetChick()
        {
            row = 0;
            chick.localPosition = new Vector3(0f, rows[0], 0f);
            chickArt.sprite = chickSprite;
            chickArt.transform.localScale = Vector3.one;
            chickArt.transform.localPosition = Vector3.zero;
            hopping = false;
            queued = 0;
            safeFor = 0.6f;
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil && !done) { bubble.Hide(); bubbleUntil = 0f; }

            // traffico continuo: i veicoli escono da un lato e rientrano dall'altro
            for (int i = 0; i < lanes.Length; i++)
            {
                float s = laneSpeeds[i];
                foreach (Transform v in lanes[i])
                {
                    Vector3 p = v.localPosition;
                    p.x += s * dt;
                    if (s > 0f && p.x > wrap) p.x -= wrap * 2f;
                    if (s < 0f && p.x < -wrap) p.x += wrap * 2f;
                    v.localPosition = p;
                }
            }
            if (done || dying) return;
            safeFor -= dt;
            Input2();
            if (safeFor <= 0f && Hit()) StartCoroutine(Squash());
        }

        void Input2()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { tracking = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began) { tracking = !PointerInput.IsOverUi(); consumed = false; touchStart = w; return; }
            if (!tracking) return;
            Vector2 d = w - touchStart;
            if (!consumed && d.magnitude > swipe)
            {
                consumed = true;
                if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) Command(d.x > 0f ? 2 : -2);
                else Command(d.y > 0f ? 1 : -1);
            }
            if (ended)
            {
                if (!consumed) Command(1);   // tocco = salto avanti
                tracking = false;
            }
        }

        /// <summary>1 avanti, -1 indietro, 2 destra, -2 sinistra.</summary>
        void Command(int c)
        {
            if (hopping) { queued = c; return; }
            StartCoroutine(Hop(c));
        }

        IEnumerator Hop(int c)
        {
            hopping = true;
            Vector3 from = chick.localPosition, to = from;
            if (c == 1 && row < rows.Length - 1) { row++; to.y = rows[row]; }
            else if (c == -1 && row > 0) { row--; to.y = rows[row]; }
            else if (c == 2 || c == -2) to.x = Mathf.Clamp(from.x + Mathf.Sign(c) * sideStep, -2.4f, 2.4f);
            if (to != from) Sfx.Play(this, "tick", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / hopTime)
            {
                if (dying) yield break;
                chick.localPosition = Vector3.Lerp(from, to, k);
                chickArt.transform.localPosition = Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.18f;
                chickArt.transform.localScale = new Vector3(1f - Mathf.Sin(k * Mathf.PI) * 0.1f, 1f + Mathf.Sin(k * Mathf.PI) * 0.12f, 1f);
                yield return null;
            }
            if (dying) yield break;
            chick.localPosition = to;
            chickArt.transform.localPosition = Vector3.zero;
            chickArt.transform.localScale = Vector3.one;
            hopping = false;
            if (row == rows.Length - 1) { StartCoroutine(Home()); yield break; }
            if (queued != 0) { int q = queued; queued = 0; StartCoroutine(Hop(q)); }
        }

        bool Hit()
        {
            Vector2 c = chick.position;
            int k = 0;
            for (int i = 0; i < lanes.Length; i++)
            {
                bool near = Mathf.Abs(c.y - lanes[i].position.y) < 0.42f;
                foreach (Transform v in lanes[i])
                {
                    float half = lengths[k++] * 0.42f;   // un po' più piccolo del disegno: niente urti ingiusti
                    if (near && Mathf.Abs(c.x - v.position.x) < half + chickHalf) return true;
                }
            }
            return false;
        }

        IEnumerator Squash()
        {
            dying = true;
            hopping = false;
            deaths++;
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.9f);
            chickArt.sprite = flatSprite;
            chickArt.transform.localPosition = Vector3.zero;
            chickArt.transform.localScale = Vector3.one;
            livesLeft--;
            for (int i = 0; i < lives.Length; i++) lives[i].color = i < livesLeft ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.35f);
            Say(Ouch[(deaths - 1) % Ouch.Length], 1.2f);
            // esplosione di piume
            Vector3 c = chick.position;
            var vel = new Vector2[feathers.Length];
            for (int i = 0; i < feathers.Length; i++)
            {
                feathers[i].gameObject.SetActive(true);
                feathers[i].position = c;
                float a = (i / (float)feathers.Length) * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
                vel[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a) + 0.6f) * Random.Range(1.5f, 2.6f);
            }
            for (float k = 0f; k < 1.1f; k += Time.deltaTime)
            {
                for (int i = 0; i < feathers.Length; i++)
                {
                    vel[i] = new Vector2(vel[i].x * 0.96f, Mathf.Max(vel[i].y - 6f * Time.deltaTime, -0.6f));
                    feathers[i].position += (Vector3)(vel[i] * Time.deltaTime) + Vector3.right * Mathf.Sin(k * 8f + i) * 0.01f;
                    feathers[i].Rotate(0, 0, 200f * Time.deltaTime * (i % 2 == 0 ? 1f : -1f));
                }
                yield return null;
            }
            foreach (Transform f in feathers) f.gameObject.SetActive(false);
            if (livesLeft <= 0)
            {
                Say("Game over, chick.", 2f);
                yield return new WaitForSeconds(1f);
                Fail();
                yield break;
            }
            ResetChick();
            dying = false;
        }

        IEnumerator Home()
        {
            done = true;
            Sfx.Play(this, "ding", 0.8f);
            // corre dalla mamma
            Vector3 from = chick.position, to = mom.position + new Vector3(mom.position.x > from.x ? -0.6f : 0.6f, -0.2f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                chick.position = Vector3.Lerp(from, to, k);
                chickArt.transform.localPosition = Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f)) * 0.12f;
                yield return null;
            }
            chickArt.transform.localPosition = Vector3.zero;
            if (hearts != null) { hearts.position = (chick.position + mom.position) * 0.5f + Vector3.up * 0.7f; hearts.gameObject.SetActive(true); }
            Vector3 ms = mom.localScale, cs = chick.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                float b = Mathf.Sin(k * Mathf.PI * 2f) * 0.08f;
                mom.localScale = ms * (1f + b);
                chick.localScale = cs * (1f - b);
                if (hearts != null) hearts.position += Vector3.up * Time.deltaTime * 0.4f;
                yield return null;
            }
            mom.localScale = ms;
            chick.localScale = cs;
            if (bubble != null) bubble.Hide();
            if (banner != null) banner.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.7f);
            yield return new WaitForSeconds(2.4f);
            Win();
        }
    }
}
