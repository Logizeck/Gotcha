using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The invisible ink" — una mappa sembra bianca; accanto c'è un asciugacapelli. Soluzione: trascinare il phon
    /// sul foglio tenendolo in movimento: il calore rivela pian piano un sentiero d'inchiostro simpatico fino alla X.
    /// Fermarsi troppo su un punto lo surriscalda (mappa delle temperature): la carta brucia e si perde.
    /// </summary>
    public class TheInvisibleInkLevel : S41ILevelBase
    {
        [SerializeField] Transform dryer, paper, token, treasure;
        [SerializeField] Collider2D dryerArea;
        [SerializeField] SpriteRenderer[] dots;     // l'ultimo è la X
        [SerializeField] SpriteRenderer[] scorches; // griglia cols x rows sulla carta
        [SerializeField] TextMesh hereText;
        [SerializeField] SmokeEmitter hotAir, fireSmoke;
        [SerializeField] int cols = 9, rows = 10;
        [SerializeField] Vector2 gridMin = new Vector2(-2f, -2.65f);
        [SerializeField] float cell = 0.5f, spotDrop = 0.35f;

        float[] heat, reveal;
        Vector2 grabOffset;
        bool dragging;
        float warnAt;

        protected override void Begin()
        {
            heat = new float[scorches.Length];
            reveal = new float[dots.Length];
            foreach (SpriteRenderer d in dots) Alpha(d, 0f);
            foreach (SpriteRenderer s in scorches) Alpha(s, 0f);
            hereText.color = new Color(hereText.color.r, hereText.color.g, hereText.color.b, 0f);
            token.gameObject.SetActive(false);
            treasure.gameObject.SetActive(false);
            if (hotAir != null) hotAir.Stop();
            if (fireSmoke != null) fireSmoke.Stop();
            Say("A blank map? Suspicious...", 2.2f);
        }

        Vector2 Spot() => (Vector2)dryer.position + Vector2.down * spotDrop;

        protected override void Tick()
        {
            if (busy) return;
            float dt = Time.deltaTime;
            bool has = Pointer(out Vector2 w, out bool began, out bool ended);
            if (has && began && dryerArea.OverlapPoint(w)) { dragging = true; grabOffset = (Vector2)dryer.position - w; Sfx.Play(this, "whoosh", 0.4f); }
            if (dragging && (!has || ended)) dragging = false;
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                dryer.position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.6f), Mathf.Clamp(p.y, -4.3f, 2.4f), dryer.position.z);
                dryer.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 40f) * 1.5f);
            }
            if (hotAir != null) { if (dragging && !hotAir.IsPlaying) hotAir.Play(); else if (!dragging && hotAir.IsPlaying) hotAir.Stop(); }

            Vector2 spot = Spot();
            float hottest = 0f;
            for (int i = 0; i < heat.Length; i++)
            {
                Vector2 c = new Vector2(gridMin.x + (i % cols) * cell, gridMin.y + (i / cols) * cell);
                float f = dragging ? Mathf.Clamp01(1f - Vector2.Distance(c, spot) / 0.55f) : 0f;
                heat[i] = Mathf.Max(0f, heat[i] + (f * 0.85f - 0.35f) * dt);
                hottest = Mathf.Max(hottest, heat[i]);
                Alpha(scorches[i], Mathf.Clamp01((heat[i] - 0.4f) / 0.6f) * 0.85f);
                if (heat[i] >= 1f) { StartCoroutine(Burn(c)); return; }
            }
            if (hottest > 0.7f && clock > warnAt) { Say("It's getting too hot! Keep moving!", 1.4f); warnAt = clock + 2.5f; Haptics.Light(); }

            if (!dragging) return;
            float sum = 0f;
            for (int i = 0; i < dots.Length; i++)
            {
                float f = Mathf.Clamp01(1f - Vector2.Distance(dots[i].transform.position, spot) / 0.65f);
                reveal[i] = Mathf.Min(1f, reveal[i] + f * 1.7f * dt);
                Alpha(dots[i], reveal[i]);
                sum += reveal[i];
            }
            if (sum / dots.Length > 0.85f && reveal[dots.Length - 1] > 0.6f) StartCoroutine(Revealed());
        }

        IEnumerator Burn(Vector2 at)
        {
            busy = true;
            dragging = false;
            if (hotAir != null) hotAir.Stop();
            if (fireSmoke != null) { fireSmoke.transform.position = new Vector3(at.x, at.y, fireSmoke.transform.position.z); fireSmoke.Play(); }
            Sfx.Play(this, "buzz", 0.9f);
            Haptics.Heavy();
            Say("The map is toast!", 0f);
            StartCoroutine(Shake(paper, 0.08f, 0.6f));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                for (int i = 0; i < scorches.Length; i++)
                {
                    Vector2 c = new Vector2(gridMin.x + (i % cols) * cell, gridMin.y + (i / cols) * cell);
                    if (Vector2.Distance(c, at) < k * 1.6f) Alpha(scorches[i], 0.9f);
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.8f);
            if (fireSmoke != null) fireSmoke.Stop();
            Fail();
        }

        IEnumerator Revealed()
        {
            busy = true;
            dragging = false;
            if (hotAir != null) hotAir.Stop();
            Sfx.Play(this, "ding", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                for (int i = 0; i < dots.Length; i++) Alpha(dots[i], Mathf.Max(reveal[i], k));
                foreach (SpriteRenderer s in scorches) Alpha(s, s.color.a * (1f - k));
                hereText.color = new Color(hereText.color.r, hereText.color.g, hereText.color.b, k);
                yield return null;
            }
            Say("A secret path!", 0f);
            yield return StartCoroutine(Move(dryer, dryer.localPosition + new Vector3(3f, -1f, 0f), 0.5f));
            // il topolino segue il sentiero fino alla X
            token.gameObject.SetActive(true);
            token.position = new Vector3(dots[0].transform.position.x, dots[0].transform.position.y, token.position.z);
            for (int i = 1; i < dots.Length; i++)
            {
                Vector3 to = new Vector3(dots[i].transform.position.x, dots[i].transform.position.y, token.position.z);
                while ((token.position - to).sqrMagnitude > 0.0004f)
                {
                    token.position = Vector3.MoveTowards(token.position, to, 3.5f * Time.deltaTime);
                    yield return null;
                }
                if (i % 3 == 0) Sfx.Play(this, "tick", 0.4f);
            }
            treasure.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.8f);
            yield return StartCoroutine(Pop(treasure, 1.5f, 0.35f));
            yield return StartCoroutine(Celebrate(1.6f));
        }
    }
}
