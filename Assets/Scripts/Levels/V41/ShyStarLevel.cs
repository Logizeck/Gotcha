using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The shy star" — una stellina deve brillare ma resta nascosta dietro una nuvola; se la tocchi scappa
    /// dietro un'altra nuvola (tre tocchi diretti = se ne va = sconfitta). Soluzione: trascina lo specchio davanti
    /// alla nuvola dove si nasconde: si vede riflessa, vanitosa si mette in posa e si accende.
    /// </summary>
    public class ShyStarLevel : LevelController
    {
        [SerializeField] Transform star, mirror, glow;
        [SerializeField] Transform[] clouds;
        [SerializeField] SpriteRenderer starArt, reflection;
        [SerializeField] Sprite happySprite;
        [SerializeField] Collider2D mirrorArea;
        [SerializeField] TextMesh triesText;
        [SerializeField] SmokeEmitter sparkles;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] Vector2 peek = new Vector2(0.7f, 0.62f);
        [SerializeField] float mirrorRadius = 1.35f;

        Camera cam;
        int spot, touches;
        bool dragging, busy;
        Vector2 grabOffset;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            spot = 0;
            star.position = (Vector2)clouds[spot].position + peek;
            S41KKit.Alpha(reflection, 0f);
            glow.localScale = Vector3.one * 0.4f;
            if (sparkles != null) sparkles.Stop();
            S41KKit.Confetti(confetti, false);
            UpdateTries();
            if (bubble != null) bubble.Show("Don't look at me! I'm not pretty enough to shine...");
        }

        void UpdateTries() { triesText.text = "STARTLED: " + touches + "/3"; }

        void Update()
        {
            if (IsFinished) return;
            if (!busy) star.position = (Vector2)clouds[spot].position + peek + new Vector2(0f, Mathf.Sin(Time.time * 2f) * 0.05f);
            if (busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (dragging) Drop(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (mirrorArea.OverlapPoint(w)) { dragging = true; grabOffset = (Vector2)mirror.position - w; Sfx.Play(this, "tick", 0.5f); }
                else if (Vector2.Distance(w, star.position) < 0.6f) { StartCoroutine(Startle()); return; }
            }
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                mirror.position = new Vector3(Mathf.Clamp(p.x, -2.4f, 2.4f), Mathf.Clamp(p.y, -3.9f, 2.0f), 0f);
                if (ended) Drop();
            }
        }

        void Drop()
        {
            dragging = false;
            Sfx.Play(this, "clunk", 0.4f);
            for (int i = 0; i < clouds.Length; i++)
            {
                Vector2 c = clouds[i].position;
                float d = Mathf.Min(Vector2.Distance(mirror.position, c), Vector2.Distance(mirror.position, c + new Vector2(0f, -1.0f)));
                if (d > mirrorRadius) continue;
                if (i == spot) { StartCoroutine(WinRoutine()); return; }
                if (bubble != null) bubble.Show("Nobody behind that cloud is looking.");
                return;
            }
        }

        IEnumerator Startle()
        {
            busy = true;
            touches++;
            UpdateTries();
            Sfx.Play(this, "whoosh", 0.7f);
            Haptics.Light();
            Vector3 hide = clouds[spot].position;
            yield return StartCoroutine(Fly(hide, 0.2f));
            if (touches >= 3)
            {
                if (bubble != null) bubble.Show("Too many eyes! I'm moving to another galaxy!");
                yield return StartCoroutine(Fly(new Vector3(3.6f, 4.8f, 0f), 0.9f));
                yield return new WaitForSeconds(0.6f);
                Fail();
                yield break;
            }
            spot = (spot + 1) % clouds.Length;
            yield return StartCoroutine(Fly((Vector2)clouds[spot].position + peek, 0.5f));
            if (bubble != null) bubble.Show(touches == 1 ? "Eek! Don't touch! I'll hide over here." : "Stop staring at me! So embarrassing!");
            busy = false;
        }

        IEnumerator Fly(Vector3 to, float seconds)
        {
            Vector3 from = star.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
            {
                float e = t * t * (3f - 2f * t);
                star.position = Vector3.Lerp(from, to, e) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.4f;
                yield return null;
            }
            star.position = to;
        }

        IEnumerator WinRoutine()
        {
            busy = true;
            Sfx.Play(this, "pop", 0.6f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f) { S41KKit.Alpha(reflection, t); yield return null; }
            S41KKit.Alpha(reflection, 1f);
            if (bubble != null) bubble.Show("Wait... who is THAT gorgeous star?!");
            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(Fly((Vector2)clouds[spot].position + new Vector2(0f, 1.0f), 0.7f));
            starArt.sprite = happySprite;
            if (bubble != null) bubble.Show("It's ME! I'm FABULOUS! Time to SHINE!");
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Medium();
            if (sparkles != null) sparkles.Play();
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.8f)
            {
                glow.localScale = Vector3.one * Mathf.Lerp(0.4f, 2.2f, t);
                star.localScale = Vector3.one * (1f + 0.3f * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            S41KKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41KKit.Confetti(confetti, false);
            if (sparkles != null) sparkles.Stop();
            Win();
        }
    }
}
