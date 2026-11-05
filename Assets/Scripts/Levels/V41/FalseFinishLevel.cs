using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "False finish" (rifatto nella 0.78 su indicazione del proprietario). Obiettivo "WIN THE RACE": il corridore
    /// taglia il traguardo e al centro, sopra di lui, compare un enorme "YOU WIN!" col pulsante NEXT. Ma TU sei il
    /// topolino lentissimo (ha il cartellino "YOU"). Soluzione: trascinare la scritta YOU WIN! sopra il topo. NEXT dà
    /// la vittoria al corridore: tre NEXT fanno ripartire la gara (nessuna vita persa). La scritta lasciata altrove torna
    /// al suo posto.
    /// </summary>
    public class FalseFinishLevel : LevelController
    {
        [SerializeField] Transform hero, mouse, banner, nextButton, trophy;
        [SerializeField] Collider2D nextArea, mouseArea, bannerArea;
        [SerializeField] TextMesh countdown, falseText, realWin;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float finishX = 1.9f, heroSeconds = 1.5f, mouseSpeed = 0.42f, mouseStop = 2.3f, dropRadius = 1.5f;

        static readonly string[] MissLines =
        {
            "That's HIS win. Whose is it supposed to be?",
            "The banner went back to the runner.",
            "Not there. Who are YOU in this race?",
        };

        Camera cam;
        Vector3 heroStart, mouseStart, bannerHome, bannerScale, grabOffset;
        bool racing, busy, mouseDone, heroDone, dragging;
        int premature, misses;
        float clock;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            heroStart = hero.localPosition;
            mouseStart = mouse.localPosition;
            bannerHome = banner.localPosition;
            bannerScale = banner.localScale;
            trophy.gameObject.SetActive(false);
            realWin.gameObject.SetActive(false);
            S41EKit.Confetti(confetti, false);
            UpdateFalse();
            StartCoroutine(Race());
        }

        void UpdateFalse() { if (falseText != null) falseText.text = "NEXT PRESSED " + premature + "/3"; }

        IEnumerator Race()
        {
            busy = true;
            racing = false;
            dragging = false;
            heroDone = mouseDone = false;
            banner.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            banner.localPosition = bannerHome;
            banner.localRotation = Quaternion.identity;
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
            // la vittoria del corridore: la scritta al centro, sopra di lui
            banner.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
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
            if (busy || !racing) { dragging = false; return; }
            if (!S41EKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { if (dragging) DropBanner(); return; }

            if (dragging)
            {
                // la scritta segue il dito (dentro lo schermo), un po' più piccola per vedere dove la si lascia
                Vector3 target = new Vector3(w.x, w.y, banner.position.z) + grabOffset;
                target.x = Mathf.Clamp(target.x, -2.3f, 2.3f);
                target.y = Mathf.Clamp(target.y, -4.2f, 3.0f);
                banner.position = target;
                banner.localScale = Vector3.Lerp(banner.localScale, bannerScale * 0.6f, 12f * Time.deltaTime);
                if (ended) DropBanner();
                return;
            }
            if (!began) return;
            if (heroDone && banner.gameObject.activeSelf && bannerArea != null && bannerArea.OverlapPoint(w))
            {
                dragging = true;
                grabOffset = banner.position - new Vector3(w.x, w.y, banner.position.z);
                grabOffset *= 0.6f;
                Sfx.Play(this, "tick", 0.5f);
                return;
            }
            if (mouseArea.OverlapPoint(w))
            {
                Sfx.Play(this, "tick", 0.5f);
                S41EKit.Say(bubble, heroDone ? "Squeak! I'm still racing... and I'm YOU!" : "Squeak! I'm doing my best!");
                return;
            }
            if (heroDone && nextButton.gameObject.activeSelf && nextArea.OverlapPoint(w)) StartCoroutine(Next());
        }

        void DropBanner()
        {
            dragging = false;
            Vector3 target = mouse.position + Vector3.up * 0.4f;
            if (Vector2.Distance(banner.position, target) <= dropRadius) { StartCoroutine(RealWin()); return; }
            misses++;
            Sfx.Play(this, "buzz", 0.5f);
            S41EKit.Say(bubble, MissLines[(misses - 1) % MissLines.Length]);
            StartCoroutine(BannerHome());
        }

        IEnumerator BannerHome()
        {
            busy = true;
            yield return S41EKit.Move(banner, bannerHome, 0.25f);
            banner.localScale = bannerScale;
            busy = false;
        }

        void OnApplicationPause(bool paused) { if (paused) CancelDrag(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelDrag(); }

        void CancelDrag()
        {
            if (!dragging) return;
            dragging = false;
            banner.localPosition = bannerHome;
            banner.localScale = bannerScale;
        }

        IEnumerator Next()
        {
            busy = true;
            Sfx.Play(this, "pop", 0.7f);
            yield return StartCoroutine(S41EKit.Pop(nextButton, 0.85f, 0.15f));
            premature++;
            UpdateFalse();
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Light();
            yield return StartCoroutine(S41EKit.Shake(banner, 0.15f, 0.4f));
            if (premature >= 3)
            {
                S41EKit.Say(bubble, "Race restarted! Weren't YOU racing too?");
                premature = 0;
                UpdateFalse();
                yield return new WaitForSeconds(0.8f);
                if (bubble != null) bubble.Hide();
                yield return Race();
                yield break;
            }
            S41EKit.Say(bubble, premature == 1 ? "NEXT just gives HIM the win." : "Squeak! And what about ME?");
            busy = false;
        }

        IEnumerator RealWin()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            nextButton.gameObject.SetActive(false);
            // la scritta si appoggia sopra il topo, il corridore protesta
            Vector3 above = mouse.position + Vector3.up * 1.35f;
            above.x = Mathf.Clamp(above.x, -1.6f, 1.6f);
            StartCoroutine(S41EKit.Move(banner, banner.parent.InverseTransformPoint(above), 0.3f));
            S41EKit.Say(bubble, "Hey! That was MINE!");
            yield return new WaitForSeconds(0.4f);
            trophy.gameObject.SetActive(true);
            Vector3 t = trophy.parent.InverseTransformPoint(mouse.position + Vector3.up * 0.2f);
            trophy.localPosition = t + Vector3.up * 3f;
            yield return S41EKit.Move(trophy, t, 0.4f);
            Sfx.Play(this, "clunk", 0.7f);
            realWin.gameObject.SetActive(true);
            StartCoroutine(S41EKit.Pop(realWin.transform, 1.25f, 0.3f));
            yield return S41EKit.Move(mouse, mouse.localPosition, 0.35f, 0.4f);
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41EKit.Confetti(confetti, false);
            Win("YOU WROTE YOUR OWN ENDING!");
        }
    }
}
