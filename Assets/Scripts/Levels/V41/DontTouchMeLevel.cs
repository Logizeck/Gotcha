using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Don't touch me!" — un enorme pulsante rosso con due occhi arrabbiati e il cartello "DON'T TOUCH ME".
    /// Soluzione: tenerlo premuto per 5 secondi di fila: si schiaccia, si allunga, diventa viola, trema e alla fine si
    /// arrende sgonfiandosi. Tre tocchi brevi di fila lo fanno infuriare: salta fuori dallo schermo "rompendolo"
    /// (crepe da cartone) → Fail(). Lasciarlo prima dei 5 secondi azzera la pressione.
    /// </summary>
    public class DontTouchMeLevel : LevelController
    {
        [SerializeField] Transform dome;                 // perno alla base della cupola
        [SerializeField] SpriteRenderer domeRed, domePurple, domeFlat;
        [SerializeField] Collider2D buttonArea;
        [SerializeField] Transform anger;                // simbolo di rabbia sopra la cupola
        [SerializeField] SpriteRenderer[] cracks;        // crepe dello "schermo rotto"
        [SerializeField] SpriteRenderer flash;           // velo scuro dell'urto
        [SerializeField] SpriteRenderer[] stars;         // stelline della vittoria
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float needSeconds = 5f, tapSeconds = 0.35f, streakGap = 1.1f;

        static readonly string[] HoldLines = { "HEY! LET GO!", "I SAID DON'T TOUCH ME!", "Ngh... stop... it...", "ok... ok... OK..." };
        static readonly float[] HoldAt = { 0.25f, 1.4f, 2.8f, 4.1f };

        Camera cam;
        bool holding, busy;
        float holdT, vis, clock, lastTap = -10f, bubbleUntil, nextBuzz;
        int streak, line;
        Vector3 domeBase;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            domeBase = dome.localPosition;
            domeFlat.enabled = false;
            S41AKit.Alpha(domePurple, 0f);
            if (anger != null) anger.gameObject.SetActive(false);
            foreach (SpriteRenderer c in cracks) c.enabled = false;
            foreach (SpriteRenderer s in stars) s.enabled = false;
            if (flash != null) flash.enabled = false;
            Say("Don't even think about it.", 2.2f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }

            bool down = S41AKit.Pointer(cam, out Vector2 w, out bool began, out _) && began;
            if (!holding && down && buttonArea.OverlapPoint(w))
            {
                holding = true;
                holdT = 0f;
                line = 0;
                nextBuzz = 0.5f;
                Sfx.Play(this, "tick", 0.7f);
            }

            if (holding)
            {
                if (PointerInput.IsHeld(cam, buttonArea))
                {
                    holdT += dt;
                    if (line < HoldLines.Length && holdT >= HoldAt[line]) { Say(HoldLines[line], 1.6f); line++; }
                    if (holdT >= nextBuzz) { Haptics.Light(); Sfx.Play(this, "buzz", 0.15f + holdT * 0.05f); nextBuzz += 0.5f; }
                    if (holdT >= needSeconds) { StartCoroutine(GiveUp()); return; }
                }
                else
                {
                    holding = false;
                    Released(holdT);
                }
            }

            // aspetto: più lo tieni, più è viola, schiacciato e tremante
            float target = holding ? holdT / needSeconds : 0f;
            vis = Mathf.MoveTowards(vis, target, dt * (holding ? 3f : 0.5f));
            S41AKit.Alpha(domePurple, vis);
            float wob = Mathf.Sin(clock * (8f + vis * 10f));
            float sx = 1f + vis * 0.16f + wob * 0.06f * vis;
            float sy = 1f - vis * 0.3f - wob * 0.08f * vis;
            if (holding) sy -= 0.08f;   // il dito lo schiaccia
            dome.localScale = new Vector3(sx, sy, 1f);
            float shake = vis * 0.05f;
            dome.localPosition = domeBase + new Vector3(Random.Range(-shake, shake), 0f, 0f);
        }

        void Released(float held)
        {
            if (held < tapSeconds)
            {
                streak = (clock - lastTap < streakGap) ? streak + 1 : 1;
                lastTap = clock;
                Sfx.Play(this, "bonk", 0.7f);
                StartCoroutine(S41AKit.Bounce(dome, 0.18f, 0.2f));
                if (anger != null) { anger.gameObject.SetActive(true); StartCoroutine(S41AKit.Bounce(anger, 0.4f, 0.25f)); }
                if (streak >= 3) { StartCoroutine(Furious()); return; }
                Say(streak == 1 ? "HEY! Don't poke me!" : "STOP IT! I'm warning you!", 1.8f);
                Haptics.Light();
                return;
            }
            streak = 0;
            if (anger != null) anger.gameObject.SetActive(false);
            Say(held > 2.5f ? "Phew! I almost... NO. Go away." : "And STAY away!", 1.8f);
        }

        IEnumerator Furious()
        {
            busy = true;
            Say("THAT'S IT!!!", 1.5f);
            domeRed.color = new Color(1f, 0.6f, 0.6f);
            Vector3 p0 = dome.localPosition;
            for (float t = 0f; t < 0.7f; t += Time.deltaTime)
            {
                dome.localPosition = p0 + (Vector3)(Random.insideUnitCircle * 0.12f);
                yield return null;
            }
            if (bubble != null) bubble.Hide();
            // salta verso di noi... e sfonda lo schermo
            Sfx.Play(this, "whoosh", 0.9f);
            Vector3 s0 = dome.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                dome.localScale = s0 * Mathf.Lerp(1f, 4.5f, k * k);
                dome.localPosition = p0 + new Vector3(0f, Mathf.Lerp(0f, 2.5f, k), 0f);
                yield return null;
            }
            Sfx.Play(this, "clunk", 1f);
            Sfx.Play(this, "bonk", 1f);
            Haptics.Heavy();
            if (flash != null) flash.enabled = true;
            dome.gameObject.SetActive(false);
            foreach (SpriteRenderer c in cracks)
            {
                c.enabled = true;
                StartCoroutine(S41AKit.Bounce(c.transform, 0.3f, 0.15f));
                Sfx.Play(this, "tick", 0.9f);
                yield return new WaitForSeconds(0.08f);
            }
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                if (flash != null) S41AKit.Alpha(flash, Mathf.Lerp(0.6f, 0.25f, t / 0.4f));
                yield return null;
            }
            yield return new WaitForSeconds(1.1f);
            Fail();
        }

        IEnumerator GiveUp()
        {
            busy = true;
            holding = false;
            Say("FINE! FINE! I GIVE UP!", 1.6f);
            Haptics.Medium();
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                dome.localPosition = domeBase + (Vector3)(Random.insideUnitCircle * 0.06f);
                yield return null;
            }
            dome.localPosition = domeBase;
            // si sgonfia: pffffff
            Sfx.Play(this, "whoosh", 1f);
            Vector3 s0 = dome.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                dome.localScale = new Vector3(s0.x * (1f + k * 0.15f) + Mathf.Sin(k * 40f) * 0.04f, Mathf.Lerp(s0.y, 0.3f, k), 1f);
                yield return null;
            }
            domeRed.enabled = false;
            domePurple.enabled = false;
            domeFlat.enabled = true;
            dome.localScale = Vector3.one;
            if (anger != null) anger.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.6f);
            Say("...you win. Happy now?", 2f);
            Sfx.Play(this, "ding", 0.7f);
            foreach (SpriteRenderer s in stars) { s.enabled = true; StartCoroutine(S41AKit.Bounce(s.transform, 0.6f, 0.4f)); }
            yield return new WaitForSeconds(1.8f);
            Win();
        }
    }
}
