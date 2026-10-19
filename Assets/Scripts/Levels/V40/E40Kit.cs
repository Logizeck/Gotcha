using System.Collections;
using UnityEngine;

namespace Gotcha.Levels
{
    /// <summary>Piccoli aiuti comuni ai livelli compositi del gruppo E (pallini delle fasi, cuori, dissolvenze, rimbalzi).</summary>
    public static class E40Kit
    {
        static readonly Color Done = new Color(0.36f, 0.85f, 0.42f), Now = new Color(1f, 0.85f, 0.3f), Todo = new Color(1f, 1f, 1f, 0.35f);

        /// <summary>Pallini delle fasi: verdi quelle finite, gialla quella in corso, spente le altre.</summary>
        public static void Dots(SpriteRenderer[] dots, int stage)
        {
            if (dots == null) return;
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] == null) continue;
                dots[i].color = i < stage ? Done : i == stage ? Now : Todo;
                dots[i].transform.localScale = Vector3.one * (i == stage ? 1.25f : 1f);
            }
        }

        public static void Hearts(SpriteRenderer[] hearts, int left)
        {
            if (hearts == null) return;
            for (int i = 0; i < hearts.Length; i++)
                if (hearts[i] != null) hearts[i].color = i < left ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.35f);
        }

        public static void Goal(TextMesh goal, string text) { if (goal != null) goal.text = text; }

        /// <summary>Porta l'opacità del velo a "to" in "seconds".</summary>
        public static IEnumerator Fade(SpriteRenderer veil, float to, float seconds)
        {
            if (veil == null) yield break;
            veil.enabled = true;
            float from = veil.color.a;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                Color c = veil.color;
                c.a = Mathf.Lerp(from, to, k);
                veil.color = c;
                yield return null;
            }
            Color e = veil.color;
            e.a = to;
            veil.color = e;
            if (to <= 0f) veil.enabled = false;
        }

        /// <summary>Rimbalzo di scala (feedback di un tocco o di un successo).</summary>
        public static IEnumerator Pop(Transform t, float amount = 0.18f, float seconds = 0.25f)
        {
            if (t == null) yield break;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * amount);
                yield return null;
            }
            t.localScale = s;
        }

        /// <summary>Scossone orizzontale (errore).</summary>
        public static IEnumerator Shake(Transform t, float amount = 0.12f, float seconds = 0.35f)
        {
            if (t == null) yield break;
            Vector3 p = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localPosition = p + Vector3.right * Mathf.Sin(k * 40f) * amount * (1f - k);
                yield return null;
            }
            t.localPosition = p;
        }

        /// <summary>Fuochi d'artificio: ogni scoppio si allarga e svanisce, con colori a giro.</summary>
        public static IEnumerator Fireworks(MonoBehaviour owner, SpriteRenderer[] bursts, float seconds)
        {
            if (bursts == null || bursts.Length == 0) yield break;
            Color[] tints = { new Color(1f, 0.4f, 0.45f), new Color(0.45f, 0.85f, 1f), new Color(1f, 0.9f, 0.35f), new Color(0.6f, 1f, 0.5f), new Color(0.95f, 0.55f, 1f) };
            float t0 = Time.time;
            int i = 0;
            while (Time.time - t0 < seconds)
            {
                SpriteRenderer b = bursts[i % bursts.Length];
                b.transform.localPosition = new Vector3(Random.Range(-2f, 2f), Random.Range(-1.5f, 2.6f), 0f);
                owner.StartCoroutine(Burst(b, tints[i % tints.Length]));
                Gotcha.Components.Sfx.Play(owner, i % 2 == 0 ? "pop" : "clunk", 0.6f);
                i++;
                yield return new WaitForSeconds(0.28f);
            }
        }

        static IEnumerator Burst(SpriteRenderer b, Color tint)
        {
            b.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                b.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.2f, Mathf.Sqrt(k));
                tint.a = 1f - k * k;
                b.color = tint;
                yield return null;
            }
            b.gameObject.SetActive(false);
        }
    }
}
