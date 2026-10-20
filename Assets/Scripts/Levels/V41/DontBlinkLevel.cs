using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Don't blink!" — una statua si avvicina ogni volta che nessuno la guarda; accanto a lei c'è la chiave.
    /// Versione touch (niente fotocamera): si tiene premuto con un dito il grande occhio WATCH (attenzione) e con un
    /// secondo dito si trascina la chiave fino alla serratura. Se si lascia l'occhio la statua avanza (e si riprende la
    /// chiave); dopo tre avanzate arriva addosso = sconfitta. Nell'Editor la barra spaziatrice fa da occhio.
    /// </summary>
    public class DontBlinkLevel : LevelController
    {
        [SerializeField] Transform statue, key, eyeButton, lockSpot;
        [SerializeField] SpriteRenderer statueArt, eyeArt, blackout, doorway, doorPanel;
        [SerializeField] Sprite shySprite, peekSprite, eyeOpen, eyeClosed;
        [SerializeField] Collider2D eyeArea, keyArea;
        [SerializeField] TextMesh stepText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float idleLimit = 2.0f, grabLimit = 0.35f;

        // x, y, scala della statua a ogni avanzata (l'ultima è "addosso")
        static readonly Vector3[] steps = { new Vector3(-0.2f, -0.3f, 0.55f), new Vector3(-0.15f, -1.1f, 0.8f), new Vector3(-0.1f, -2.0f, 1.05f), new Vector3(0f, -3.2f, 1.4f) };

        Camera cam;
        int stepIndex, keyFinger = -99;
        bool started, busy, dragging;
        float unwatched;
        Vector2 dragOffset;
        Vector3 keyHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            keyHome = key.position;
            PlaceStatue();
            S41HKit.Alpha(blackout, 0f);
            doorway.gameObject.SetActive(false);
            S41HKit.Confetti(confetti, false);
            UpdateText();
            S41HKit.Say(bubble, "Take the key. But never, EVER stop watching her.");
        }

        void PlaceStatue()
        {
            Vector3 s = steps[stepIndex];
            statue.localPosition = new Vector3(s.x, s.y, 0f);
            statue.localScale = Vector3.one * s.z;
        }

        void UpdateText() { if (stepText != null) stepText.text = "SHE MOVED " + stepIndex + "/" + (steps.Length - 1); }

        bool Watching()
        {
            bool held = false;
            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
                if (!f.ended && f.id != keyFinger && eyeArea.OverlapPoint(f.world)) held = true;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Space)) held = true;
#endif
            return held;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            bool watching = Watching();
            eyeArt.sprite = watching ? eyeOpen : eyeClosed;
            statueArt.sprite = watching ? shySprite : peekSprite;
            eyeButton.localScale = Vector3.one * (watching ? 1.08f : 1f);

            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
            {
                if (f.began) started = true;
                if (f.began && !dragging && keyArea.OverlapPoint(f.world))
                {
                    dragging = true;
                    keyFinger = f.id;
                    dragOffset = (Vector2)key.position - f.world;
                    Sfx.Play(this, "tick", 0.5f);
                }
                if (dragging && f.id == keyFinger)
                {
                    Vector2 p = f.world + dragOffset;
                    key.position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.6f), Mathf.Clamp(p.y, -4.4f, 2.6f), 0f);
                    if (Vector2.Distance(key.position, lockSpot.position) < 0.6f) { StartCoroutine(Escape()); return; }
                    if (f.ended) { dragging = false; keyFinger = -99; }
                }
            }
            if (dragging && S41HKit.Fingers(cam).Count == 0) { dragging = false; keyFinger = -99; }

            if (watching || !started) { unwatched = 0f; return; }
            unwatched += Time.deltaTime;
            if (unwatched >= (dragging ? grabLimit : idleLimit)) StartCoroutine(Advance());
        }

        IEnumerator Advance()
        {
            busy = true;
            unwatched = 0f;
            bool hadKey = dragging;
            dragging = false;
            keyFinger = -99;
            // buio di un attimo: quando torna la luce lei è più vicina
            S41HKit.Alpha(blackout, 1f);
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Medium();
            stepIndex++;
            PlaceStatue();
            if (hadKey || Vector2.Distance(key.position, keyHome) > 0.1f) key.position = keyHome;
            yield return new WaitForSeconds(0.25f);
            yield return StartCoroutine(S41HKit.Fade(blackout, 0f, 0.2f));
            UpdateText();
            if (stepIndex >= steps.Length - 1)
            {
                S41HKit.Say(bubble, "BOO!");
                Haptics.Heavy();
                Sfx.Play(this, "bonk", 1f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { statue.localScale = Vector3.one * Mathf.Lerp(steps[stepIndex].z, 3f, k); yield return null; }
                S41HKit.Alpha(blackout, 1f);
                yield return new WaitForSeconds(0.6f);
                Fail();
                yield break;
            }
            S41HKit.Say(bubble, hadKey ? "She took the key back! Keep watching her!" : stepIndex == 1 ? "Did she just... move?!" : "She's getting closer! Hold the eye!");
            busy = false;
        }

        IEnumerator Escape()
        {
            busy = true;
            dragging = false;
            key.position = lockSpot.position;
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Light();
            yield return new WaitForSeconds(0.2f);
            key.gameObject.SetActive(false);
            doorPanel.gameObject.SetActive(false);
            doorway.gameObject.SetActive(true);
            Sfx.Play(this, "ding", 0.8f);
            statueArt.sprite = shySprite;
            S41HKit.Say(bubble, "You never blinked! She's sulking now.");
            S41HKit.Confetti(confetti, true);
            yield return new WaitForSeconds(2.2f);
            S41HKit.Confetti(confetti, false);
            Win();
        }
    }
}
