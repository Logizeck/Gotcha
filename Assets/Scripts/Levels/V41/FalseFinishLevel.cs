using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "False finish" — il corridore taglia il traguardo e appare un enorme "YOU WIN!" col pulsante NEXT. Ma la
    /// gara non è finita: sotto la scritta corre ancora un topolino lentissimo. Soluzione: NON premere NEXT, aspetta che
    /// il topolino passi il traguardo e toccalo. Tre NEXT prematuri fanno ripartire la gara (nessuna vita persa).
    /// </summary>
    public class FalseFinishLevel : LevelController
    {
        [SerializeField] Transform hero, mouse, banner, nextButton, trophy;
        [SerializeField] Collider2D nextArea, mouseArea;
        [SerializeField] TextMesh countdown, falseText, realWin;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float finishX = 1.9f, heroSeconds = 1.5f, mouseSpeed = 0.42f, mouseStop = 2.3f;

        Camera cam;
        Vector3 heroStart, mouseStart, bannerScale;
        bool racing, busy, mouseDone, heroDone;
        int premature;
        float clock;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            heroStart = hero.localPosition;
            mouseStart = mouse.localPosition;
            bannerScale = banner.localScale;
            trophy.gameObject.SetActive(false);
            realWin.gameObject.SetActive(false);
            S41EKit.Confetti(confetti, false);
            UpdateFalse();
            StartCoroutine(Race());
        }

        void UpdateFalse() { if (falseText != null) falseText.text = "EARLY NEXT " + premature + "/3"; }

        IEnumerator Race()
        {
            busy = true;
            racing = false;
            heroDone = mouseDone = false;
            banner.gameObject.SetActive(false);
            hero.localPosition = heroStart;
            mouse.localPosition = mouseStart;
            foreach (string s in new[] { "3", "2", "1", "GO!" })
            {
                countdown.text = s;
                Sfx.Play(this, s == "GO!" ? "ding" : "tick", 0.7f);
                StartCoroutine(S41EKit.Pop(countdown.transform, 1.3f, 0.2f));
                yield return new WaitForSeconds(0.6f);
            }
            countdown.text = "";
            racing = true;
            busy = false;
            // il corridore sfreccia
            Vector3 end = new Vector3(finishX + 0.6f, heroStart.y, heroStart.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / heroSeconds)
            {
                hero.localPosition = Vector3.Lerp(heroStart, end, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 30f)) * 0.12f;
                yield return null;
            }
            hero.localPosition = end;
            heroDone = true;
            // la falsa vittoria
            banner.gameObject.SetActive(true);
            banner.localScale = Vector3.zero;
            Sfx.Play(this, "ding", 0.9f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { banner.localScale = bannerScale * (k + Mathf.Sin(k * Mathf.PI) * 0.2f); yield return null; }
            banner.localScale = bannerScale;
            S41EKit.Say(bubble, "I WON! I'm the fastest! Press NEXT!");
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (racing && !mouseDone)
            {
                Vector3 p = mouse.localPosition;
                p.x += mouseSpeed * Time.deltaTime;
                mouse.localPosition = new Vector3(p.x, mouseStart.y + Mathf.Abs(Mathf.Sin(clock * 9f)) * 0.08f, p.z);
                mouse.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 9f) * 6f);
                if (p.x >= mouseStop) { mouseDone = true; mouse.localRotation = Quaternion.identity; Sfx.Play(this, "pop", 0.6f); }
            }
            if (nextButton.gameObject.activeInHierarchy) nextButton.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(clock * 6f));
            if (busy || !racing) return;
            if (!PointerInput.AnyTouchBegan() || !S41EKit.Pointer(cam, out Vector2 w, out _, out _)) return;
            if (mouseArea.OverlapPoint(w))
            {
                if (mouse.position.x >= finishX + 0.05f) { StartCoroutine(RealWin()); return; }
                Sfx.Play(this, "tick", 0.5f);
                S41EKit.Say(bubble, "Squeak! Not yet... still running!");
                return;
            }
            if (heroDone && banner.gameObject.activeSelf && nextArea.OverlapPoint(w)) StartCoroutine(Next());
        }

        IEnumerator Next()
        {
            busy = true;
            Sfx.Play(this, "pop", 0.7f);
            yield return StartCoroutine(S41EKit.Pop(nextButton, 0.85f, 0.15f));
            if (mouseDone)
            {
                S41EKit.Say(bubble, "NEXT is broken... someone else finished too!");
                busy = false;
                yield break;
            }
            premature++;
            UpdateFalse();
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Light();
            yield return StartCoroutine(S41EKit.Shake(banner, 0.15f, 0.4f));
            if (premature >= 3)
            {
                S41EKit.Say(bubble, "Race restarted! Watch more carefully.");
                premature = 0;
                UpdateFalse();
                yield return new WaitForSeconds(0.8f);
                if (bubble != null) bubble.Hide();
                yield return Race();
                yield break;
            }
            S41EKit.Say(bubble, premature == 1 ? "Squeak! Hey! The race isn't over!" : "Wait for me! I'm still running!");
            busy = false;
        }

        IEnumerator RealWin()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.9f);
            // la falsa scritta cade giù
            Vector3 b = banner.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                banner.localPosition = b + Vector3.down * 7f * k * k;
                banner.localRotation = Quaternion.Euler(0f, 0f, -25f * k);
                yield return null;
            }
            banner.gameObject.SetActive(false);
            Haptics.Light();
            trophy.gameObject.SetActive(true);
            Vector3 t = trophy.localPosition;
            trophy.localPosition = t + Vector3.up * 3f;
            yield return S41EKit.Move(trophy, t, 0.4f);
            Sfx.Play(this, "clunk", 0.7f);
            realWin.gameObject.SetActive(true);
            StartCoroutine(S41EKit.Pop(realWin.transform, 1.25f, 0.3f));
            S41EKit.Say(bubble, "The real champion: patience!");
            yield return S41EKit.Move(mouse, mouse.localPosition, 0.35f, 0.4f);
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41EKit.Confetti(confetti, false);
            Win();
        }
    }
}
