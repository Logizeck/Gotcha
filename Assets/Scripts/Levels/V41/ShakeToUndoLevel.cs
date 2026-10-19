using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Shake to undo" — il pittore ha disegnato per sbaglio delle mutande giganti sul capolavoro e il tasto
    /// UNDO è rotto. Scuoti il telefono: ogni scossa (moderata) cancella un tratto. Dopo il terzo bisogna FERMARSI: una
    /// quarta scossa troppo forte cancella anche il quadro e si ricomincia (senza perdere vite). Se il sensore manca,
    /// dopo qualche secondo compare un pulsante "SHAKE" accessibile.
    /// </summary>
    public class ShakeToUndoLevel : LevelController
    {
        [SerializeField] SpriteRenderer[] strokes;     // tratti nell'ordine in cui sono stati disegnati
        [SerializeField] SpriteRenderer blank;         // tela bianca che copre il capolavoro quando si cancella tutto
        [SerializeField] Transform painter, undoButton;
        [SerializeField] Collider2D undoArea, shakeButtonArea;
        [SerializeField] GameObject shakeButton;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float shakeLevel = 9f, strongLevel = 32f, stopSeconds = 1.8f, buttonDelay = 7f, repeatEvery = 0.6f;

        Camera cam;
        int erased;
        bool busy, inShake;
        float peak, shakeFor, cooldown, calm = -1f, elapsed;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (SpriteRenderer s in strokes) Alpha(s, 1f);
            Alpha(blank, 0f);
            if (shakeButton != null) shakeButton.SetActive(false);
            S41DKit.Confetti(confetti, false);
            Say("My masterpiece! I slipped... and UNDO is broken!");
        }

        static void Alpha(SpriteRenderer s, float a) { if (s != null) { Color c = s.color; c.a = a; s.color = c; } }

        void Say(string t) { if (bubble != null) bubble.Show(t); }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            elapsed += dt;
            cooldown -= dt;
            if (shakeButton != null && !shakeButton.activeSelf && elapsed > buttonDelay && !busy)
            {
                shakeButton.SetActive(true);
                StartCoroutine(S41DKit.Pop(shakeButton.transform, 1.25f, 0.3f));
            }

            // tocchi: UNDO rotto e pulsante di riserva
            if (!busy && PointerInput.AnyTouchBegan() && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (undoArea != null && undoArea.OverlapPoint(w))
                {
                    Sfx.Play(this, "buzz", 0.6f);
                    Haptics.Light();
                    StartCoroutine(S41DKit.Shake(undoButton, 0.08f, 0.35f));
                    Say("Pfff! UNDO is broken. Try something more... shaky.");
                }
                else if (shakeButton != null && shakeButton.activeSelf && shakeButtonArea != null && shakeButtonArea.OverlapPoint(w))
                {
                    StartCoroutine(S41DKit.Pop(shakeButton.transform, 1.15f, 0.15f));
                    OnShake(shakeLevel + 4f);
                }
            }

            // scosse vere
            float s = Inputs.Shake();
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.D)) s = 60f;   // D = scossone violento
#endif
            if (!inShake && s > shakeLevel && cooldown <= 0f) { inShake = true; peak = s; shakeFor = 0f; }
            if (inShake)
            {
                peak = Mathf.Max(peak, s);
                shakeFor += dt;
                if (s < shakeLevel * 0.5f) { inShake = false; OnShake(peak); cooldown = 0.3f; }
                else if (shakeFor > repeatEvery) { OnShake(peak); peak = s; shakeFor = 0f; }
            }

            // dopo il terzo tratto: bisogna stare fermi un attimo
            if (calm >= 0f && !busy)
            {
                calm += dt;
                if (calm >= stopSeconds) { calm = -1f; StartCoroutine(Done()); }
            }
        }

        void OnShake(float strength)
        {
            if (busy || IsFinished) return;
            Haptics.Light();
            if (erased < strokes.Length)
            {
                SpriteRenderer s = strokes[strokes.Length - 1 - erased];
                erased++;
                StartCoroutine(Erase(s));
                if (erased < strokes.Length) Say(erased == 1 ? "Yes! One stroke gone!" : "Again! Almost there!");
                else { Say("PERFECT! Now STOP shaking!"); calm = 0f; }
                return;
            }
            if (strength >= strongLevel) StartCoroutine(WipeAll());
            else { calm = 0f; Say("Easy! EASY! Stop shaking!"); StartCoroutine(S41DKit.Shake(painter, 0.08f, 0.3f)); }
        }

        IEnumerator Erase(SpriteRenderer s)
        {
            Sfx.Play(this, "whoosh", 0.6f);
            Transform t = s.transform;
            Vector3 p = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                Alpha(s, 1f - k);
                t.localPosition = p + new Vector3(Mathf.Sin(k * 40f) * 0.05f, 0f, 0f);
                yield return null;
            }
            Alpha(s, 0f);
            t.localPosition = p;
        }

        IEnumerator WipeAll()
        {
            busy = true;
            calm = -1f;
            Haptics.Heavy();
            Sfx.Play(this, "whoosh", 1f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { Alpha(blank, k); yield return null; }
            Alpha(blank, 1f);
            Say("NOOO! You erased my masterpiece too!");
            Sfx.Play(this, "buzz", 0.6f);
            yield return StartCoroutine(S41DKit.Shake(painter, 0.12f, 0.8f));
            yield return new WaitForSeconds(0.8f);
            // ridipinge tutto... e gli scappa di nuovo la mano
            Say("Fine, I'll paint it again...");
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f) { Alpha(blank, 1f - k); painter.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 30f) * 4f); yield return null; }
            Alpha(blank, 0f);
            painter.localRotation = Quaternion.identity;
            foreach (SpriteRenderer s in strokes)
            {
                Sfx.Play(this, "tick", 0.7f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { Alpha(s, k); yield return null; }
                Alpha(s, 1f);
            }
            erased = 0;
            Say("Oops. My hand slipped AGAIN. Shake gently!");
            busy = false;
        }

        IEnumerator Done()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.8f);
            Say("Magnifique! My masterpiece is back!");
            S41DKit.Confetti(confetti, true);
            Vector3 p = painter.localPosition;
            for (float k = 0f; k < 1.8f; k += Time.deltaTime)
            {
                painter.localPosition = p + Vector3.up * Mathf.Abs(Mathf.Sin(k * 8f)) * 0.25f;
                painter.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 8f) * 6f);
                yield return null;
            }
            painter.localPosition = p;
            painter.localRotation = Quaternion.identity;
            S41DKit.Confetti(confetti, false);
            Win();
        }
    }
}
