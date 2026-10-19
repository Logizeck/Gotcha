using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Ghost selfie" — un fantasmino vuole una foto ma sparisce appena tocchi il pulsante (odia le foto a
    /// sorpresa). Il trucco: TIENI premuto il pulsante: parte il conto alla rovescia 3-2-1, il fantasma si avvicina e si
    /// mette in posa; rilascia alla fine dei 3 secondi per scattare. Tre foto senza fantasma = sconfitta. Nessuna
    /// fotocamera vera.
    /// </summary>
    public class GhostSelfieLevel : LevelController
    {
        [SerializeField] Transform ghost, frameCenter, shutter, polaroid, polaroidGhost;
        [SerializeField] SpriteRenderer ghostArt, flash;
        [SerializeField] Sprite idleSprite, poseSprite;
        [SerializeField] Collider2D shutterArea;
        [SerializeField] TextMesh countdown, photoCaption, triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int triesAllowed = 3;
        [SerializeField] float tapLimit = 0.35f, readyAt = 2.85f, boredAt = 4.8f;

        Camera cam;
        bool holding, busy, hidden;
        float holdT, clock;
        int misses;
        Vector3 approachFrom, polaroidHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            polaroidHome = polaroid.localPosition;
            polaroid.gameObject.SetActive(false);
            SetAlpha(flash, 0f);
            countdown.text = "";
            S41DKit.Confetti(confetti, false);
            UpdateTries();
            if (bubble != null) bubble.Show("Boo! Take my picture! ...but gently.");
        }

        static void SetAlpha(SpriteRenderer s, float a) { if (s != null) { Color c = s.color; c.a = a; s.color = c; } }

        void UpdateTries() { if (triesText != null) triesText.text = "EMPTY PHOTOS " + misses + "/" + triesAllowed; }

        Vector3 WanderSpot(float t) => new Vector3(1.55f * Mathf.Sin(t * 0.7f) + 0.25f * Mathf.Sin(t * 2.3f), 2.3f + 0.2f * Mathf.Sin(t * 1.9f), 0f);

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (busy) return;
            if (!holding)
            {
                ghost.localPosition = Vector3.Lerp(ghost.localPosition, WanderSpot(clock), Time.deltaTime * 2.5f);
                if (PointerInput.AnyTouchBegan() && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && shutterArea.OverlapPoint(PointerInput.ToWorld(cam, sp)))
                {
                    holding = true;
                    holdT = 0f;
                    approachFrom = ghost.localPosition;
                    shutter.localScale = Vector3.one * 0.9f;
                    Sfx.Play(this, "tick", 0.6f);
                }
                return;
            }
            holdT += Time.deltaTime;
            bool pressed = PointerInput.IsHeld(cam, shutterArea);
            // conto alla rovescia
            int n = 3 - Mathf.FloorToInt(holdT);
            string txt = holdT < tapLimit ? "" : n > 0 ? n.ToString() : "SMILE!";
            if (txt != countdown.text) { countdown.text = txt; if (txt.Length > 0) { Sfx.Play(this, n > 0 ? "tick" : "ding", 0.6f); StartCoroutine(S41DKit.Pop(countdown.transform, 1.3f, 0.2f)); } }
            // il fantasma si avvicina e si mette in posa
            if (holdT > tapLimit && holdT < boredAt)
            {
                float k = Mathf.Clamp01((holdT - tapLimit) / (readyAt - 0.3f - tapLimit));
                ghost.localPosition = Vector3.Lerp(approachFrom, frameCenter.localPosition, k * k * (3f - 2f * k)) + Vector3.up * Mathf.Sin(clock * 3f) * 0.05f;
                ghostArt.sprite = k >= 1f ? poseSprite : idleSprite;
                if (k >= 1f && bubble != null && bubble.gameObject.activeSelf) bubble.Hide();
            }
            if (holdT >= boredAt)
            {
                holding = false;
                shutter.localScale = Vector3.one;
                StartCoroutine(Bored());
                return;
            }
            if (!pressed)
            {
                holding = false;
                shutter.localScale = Vector3.one;
                bool ok = holdT >= readyAt;
                StartCoroutine(Snap(ok, holdT < tapLimit ? "EEK! Too sudden!" : "Not yet! I wasn't ready!"));
            }
        }

        IEnumerator Vanish()
        {
            hidden = true;
            Sfx.Play(this, "whoosh", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { SetAlpha(ghostArt, 1f - k); yield return null; }
            SetAlpha(ghostArt, 0f);
        }

        IEnumerator Bored()
        {
            busy = true;
            countdown.text = "";
            if (bubble != null) bubble.Show("Booored... I'm leaving.");
            ghostArt.sprite = idleSprite;
            yield return StartCoroutine(S41DKit.Move(ghost, WanderSpot(clock + 1f), 0.6f));
            yield return StartCoroutine(Snap(false, null));
        }

        IEnumerator Snap(bool success, string complaint)
        {
            busy = true;
            countdown.text = "";
            if (!success)
            {
                ghostArt.sprite = idleSprite;
                if (complaint != null && bubble != null) bubble.Show(complaint);
                if (!hidden) yield return StartCoroutine(Vanish());
            }
            // flash e foto
            Sfx.Play(this, "pop", 0.9f);
            Haptics.Light();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { SetAlpha(flash, 1f - k); yield return null; }
            SetAlpha(flash, 0f);
            polaroid.gameObject.SetActive(true);
            polaroidGhost.gameObject.SetActive(success);
            photoCaption.text = success ? "BEST GHOST EVER" : "...NO GHOST";
            if (success) SetAlpha(ghostArt, 0f);
            polaroid.localPosition = polaroidHome + Vector3.down * 4f;
            yield return StartCoroutine(S41DKit.Move(polaroid, polaroidHome, 0.4f));
            if (success)
            {
                Sfx.Play(this, "ding", 0.8f);
                if (bubble != null) bubble.Show("I LOOK SPOOKTACULAR!");
                S41DKit.Confetti(confetti, true);
                yield return new WaitForSeconds(2.2f);
                S41DKit.Confetti(confetti, false);
                Win();
                yield break;
            }
            misses++;
            UpdateTries();
            Sfx.Play(this, "buzz", 0.5f);
            yield return new WaitForSeconds(1.3f);
            if (misses >= triesAllowed)
            {
                if (bubble != null) bubble.Show("Hmph. No more photos for you!");
                yield return new WaitForSeconds(1.2f);
                Fail();
                yield break;
            }
            yield return StartCoroutine(S41DKit.Move(polaroid, polaroidHome + Vector3.down * 4f, 0.3f));
            polaroid.gameObject.SetActive(false);
            // il fantasma torna
            ghost.localPosition = WanderSpot(clock);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { SetAlpha(ghostArt, k); yield return null; }
            SetAlpha(ghostArt, 1f);
            hidden = false;
            if (bubble != null) bubble.Show(misses == 1 ? "Don't startle me! Slooowly..." : "Hold it... then click!");
            busy = false;
        }
    }
}
