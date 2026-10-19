using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The fake crash" — una finestra d'errore da cartone animato dice "GOTCHA HAS STOPPED WORKING" coi pulsanti
    /// CLOSE e WAIT. Non è un errore vero: trascina la parola WORKING fuori dalla finestra; la frase diventa "GOTCHA HAS
    /// STOPPED THE ERROR" e l'errore sparisce. CLOSE tre volte fa solo ripartire la finta schermata (nessuna vita persa).
    /// </summary>
    public class TheFakeCrashLevel : LevelController
    {
        [SerializeField] Transform window, working, closeButton, waitButton, frozenCat;
        [SerializeField] Collider2D workingArea, closeArea, waitArea;
        [SerializeField] TextMesh errorWord, statusText;
        [SerializeField] SpriteRenderer dim, glitch;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float windowHalfX = 2.4f, windowHalfY = 1.7f;

        static readonly string[] waitLines = { "Waiting...", "Still not working.", "Waiting harder...", "Have you tried NOT waiting?", "Zzz... still broken." };

        Camera cam;
        Vector3 workingHome, windowScale;
        Vector2 grabOffset;
        bool dragging, busy;
        int closes, waits;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            workingHome = working.localPosition;
            windowScale = window.localScale;
            errorWord.text = "";
            statusText.text = "";
            SetAlpha(glitch, 0f);
            S41DKit.Confetti(confetti, false);
            StartCoroutine(S41DKit.Pop(window, 1.1f, 0.3f));
            Sfx.Play(this, "buzz", 0.6f);
        }

        static void SetAlpha(SpriteRenderer s, float a) { if (s != null) { Color c = s.color; c.a = a; s.color = c; } }

        void Update()
        {
            if (IsFinished || busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (dragging) Release(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (workingArea.OverlapPoint(w)) { dragging = true; grabOffset = (Vector2)working.position - w; Sfx.Play(this, "tick", 0.6f); }
                else if (closeArea.OverlapPoint(w)) { StartCoroutine(Close()); return; }
                else if (waitArea.OverlapPoint(w)) { Wait(); return; }
            }
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                working.position = new Vector3(Mathf.Clamp(p.x, -2.7f, 2.7f), Mathf.Clamp(p.y, -4.6f, 3.6f), working.position.z);
                working.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 10f) * 4f);
            }
            if (ended && dragging) Release();
        }

        void Release()
        {
            dragging = false;
            working.localRotation = Quaternion.identity;
            Vector3 local = window.InverseTransformPoint(working.position);
            if (Mathf.Abs(local.x) > windowHalfX + 0.1f || Mathf.Abs(local.y) > windowHalfY + 0.1f) StartCoroutine(Fixed());
            else StartCoroutine(S41DKit.Move(working, workingHome, 0.25f));
        }

        void Wait()
        {
            Sfx.Play(this, "tick", 0.6f);
            statusText.text = waitLines[waits % waitLines.Length];
            waits++;
            StartCoroutine(S41DKit.Pop(waitButton, 1.15f, 0.2f));
        }

        IEnumerator Close()
        {
            busy = true;
            closes++;
            Sfx.Play(this, "pop", 0.7f);
            StartCoroutine(S41DKit.Pop(closeButton, 1.15f, 0.2f));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { window.localScale = windowScale * (1f - k); yield return null; }
            window.localScale = Vector3.zero;
            yield return new WaitForSeconds(0.45f);
            if (closes >= 3)
            {
                // la finta schermata "si riavvia"
                closes = 0;
                Haptics.Medium();
                Sfx.Play(this, "buzz", 0.8f);
                for (float k = 0f; k < 1.2f; k += Time.deltaTime)
                {
                    SetAlpha(glitch, Random.value * 0.8f);
                    yield return null;
                }
                SetAlpha(glitch, 0f);
                working.localPosition = workingHome;
                statusText.text = "Rebooted! Still broken. :)";
            }
            else statusText.text = closes == 1 ? "Nice try! I'm back." : "Closing won't fix me!";
            Sfx.Play(this, "buzz", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { window.localScale = windowScale * (1f + 0.15f * Mathf.Sin(k * Mathf.PI)) * k; yield return null; }
            window.localScale = windowScale;
            busy = false;
        }

        IEnumerator Fixed()
        {
            busy = true;
            // WORKING cade giù dallo schermo
            Sfx.Play(this, "whoosh", 0.7f);
            Vector3 p = working.position;
            float vy = 1.5f;
            while (p.y > -6f)
            {
                vy -= 14f * Time.deltaTime;
                p.y += vy * Time.deltaTime;
                working.position = p;
                working.localRotation = Quaternion.Euler(0, 0, working.localRotation.eulerAngles.z + 300f * Time.deltaTime);
                yield return null;
            }
            working.gameObject.SetActive(false);
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            yield return new WaitForSeconds(0.3f);
            // "GOTCHA HAS STOPPED ... THE ERROR"
            const string fix = "THE ERROR";
            for (int i = 1; i <= fix.Length; i++)
            {
                errorWord.text = fix.Substring(0, i);
                Sfx.Play(this, "tick", 0.6f);
                yield return new WaitForSeconds(0.07f);
            }
            statusText.text = "Error stopped. You're welcome!";
            Sfx.Play(this, "ding", 0.8f);
            yield return StartCoroutine(S41DKit.Pop(window, 1.12f, 0.35f));
            yield return new WaitForSeconds(0.6f);
            // la finestra si accartoccia e il gioco si "scongela"
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                window.localScale = windowScale * (1f - k);
                window.localRotation = Quaternion.Euler(0, 0, 360f * k);
                if (dim != null) SetAlpha(dim, 0.55f * (1f - k));
                yield return null;
            }
            window.gameObject.SetActive(false);
            if (frozenCat != null)
            {
                Vector3 c = frozenCat.localPosition;
                frozenCat.localRotation = Quaternion.identity;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { frozenCat.localPosition = c + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.8f; yield return null; }
                frozenCat.localPosition = c;
            }
            if (bubble != null) bubble.Show("Meow! Did I miss something?");
            S41DKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41DKit.Confetti(confetti, false);
            Win();
        }
    }
}
