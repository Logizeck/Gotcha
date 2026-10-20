using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello di Toni "TONI'S BEAUTY FILTER" — Toni vuole la foto profilo perfetta; il pannello degli effetti ha tre
    /// ritocchi (occhi, orecchie, muso) ma ognuno peggiora un'altra parte del muso. La soluzione: spostare il pannello
    /// e premere il pulsantino "NO FILTER" nascosto dietro, che cancella tutti i ritocchi. Sei ritocchi = Toni diventa
    /// un improbabile alieno = sconfitta.
    /// </summary>
    public class ToniBeautyFilterLevel : LevelController
    {
        [SerializeField] Transform face, earL, earR, eyeL, eyeR, snout, panel;
        [SerializeField] SpriteRenderer[] furParts;     // tinta verde da alieno
        [SerializeField] GameObject[] antennae;
        [SerializeField] Collider2D panelArea, noFilterArea;
        [SerializeField] Collider2D[] filterButtons;    // 0 occhi, 1 orecchie, 2 muso
        [SerializeField] SpriteRenderer flash;
        [SerializeField] TextMesh retouchText, caption;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter hearts, sparkle;
        [SerializeField] int maxRetouches = 6;

        Camera cam;
        bool busy, pressing, draggingPanel;
        int retouches;
        Vector2 pressAt, panelOffset;
        // scale volute (x, y) delle parti
        Vector2 eyeLS = Vector2.one, eyeRS = Vector2.one, earS = Vector2.one, snoutS = Vector2.one, faceS = Vector2.one;
        Vector3 earLBase, earRBase, eyeLBase, eyeRBase, snoutBase, faceBase;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            earLBase = earL.localScale; earRBase = earR.localScale;
            eyeLBase = eyeL.localScale; eyeRBase = eyeR.localScale;
            snoutBase = snout.localScale; faceBase = face.localScale;
            foreach (GameObject a in antennae) a.SetActive(false);
            S41LKit.Alpha(flash, 0f);
            caption.text = "";
            hearts.Stop();
            sparkle.Stop();
            UpdateText();
            S41LKit.Say(bubble, "Make me PERFECT for my profile pic!");
        }

        void UpdateText() { retouchText.text = "RETOUCHES " + retouches + "/" + maxRetouches; }

        static Vector3 Mul(Vector3 b, Vector2 s) => new Vector3(b.x * s.x, b.y * s.y, b.z);

        void Update()
        {
            // le parti scivolano verso la deformazione voluta
            float k = 1f - Mathf.Exp(-8f * Time.deltaTime);
            eyeL.localScale = Vector3.Lerp(eyeL.localScale, Mul(eyeLBase, eyeLS), k);
            eyeR.localScale = Vector3.Lerp(eyeR.localScale, Mul(eyeRBase, eyeRS), k);
            earL.localScale = Vector3.Lerp(earL.localScale, Mul(earLBase, earS), k);
            earR.localScale = Vector3.Lerp(earR.localScale, Mul(earRBase, earS), k);
            snout.localScale = Vector3.Lerp(snout.localScale, Mul(snoutBase, snoutS), k);
            face.localScale = Vector3.Lerp(face.localScale, Mul(faceBase, faceS), k);
            if (IsFinished || busy) return;

            bool down = S41LKit.Pointer(cam, out Vector2 w, out bool began, out bool ended);
            if (began)
            {
                if (panelArea.OverlapPoint(w)) { pressing = true; pressAt = w; panelOffset = (Vector2)panel.position - w; }
                else if (noFilterArea.OverlapPoint(w)) { StartCoroutine(NoFilter()); return; }
            }
            if (pressing)
            {
                if (!draggingPanel && Vector2.Distance(w, pressAt) > 0.2f) draggingPanel = true;
                if (draggingPanel)
                {
                    Vector2 p = w + panelOffset;
                    panel.position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.6f), Mathf.Clamp(p.y, -4.6f, -0.6f), panel.position.z);
                }
                if (ended || !down)
                {
                    if (!draggingPanel)
                        for (int i = 0; i < filterButtons.Length; i++)
                            if (filterButtons[i].OverlapPoint(pressAt)) { Retouch(i); break; }
                    pressing = draggingPanel = false;
                }
            }
        }

        /// <summary>Ogni ritocco migliora una cosa e ne peggiora un'altra.</summary>
        void Retouch(int which)
        {
            retouches++;
            UpdateText();
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            StartCoroutine(S41LKit.Pop(filterButtons[which].transform, 0.12f, 0.2f));
            switch (which)
            {
                case 0: eyeLS *= 1.25f; eyeRS *= 1.25f; earS = new Vector2(earS.x * 0.9f, earS.y * 1.45f); S41LKit.Say(bubble, "Bigger eyes! ...why are my ears so long?"); break;
                case 1: earS = new Vector2(earS.x * 0.75f, earS.y * 0.85f); snoutS *= 1.35f; S41LKit.Say(bubble, "Cute ears! ...my SNOUT!"); break;
                default: snoutS *= 0.8f; eyeLS *= 1.4f; eyeRS *= 0.75f; faceS = new Vector2(faceS.x * 1.08f, faceS.y * 0.92f); S41LKit.Say(bubble, "Tiny snout! ...my eyes don't match!"); break;
            }
            float alien = retouches / (float)maxRetouches;
            foreach (SpriteRenderer sr in furParts) sr.color = Color.Lerp(Color.white, new Color(0.55f, 1f, 0.45f), alien * alien);
            if (retouches >= maxRetouches) StartCoroutine(Alien());
        }

        IEnumerator Alien()
        {
            busy = true;
            yield return new WaitForSeconds(0.4f);
            foreach (GameObject a in antennae) { a.SetActive(true); StartCoroutine(S41LKit.Pop(a.transform, 0.4f, 0.3f)); }
            eyeLS = eyeRS = Vector2.one * 1.9f;
            snoutS = Vector2.one * 0.4f;
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Medium();
            S41LKit.Say(bubble, "BEEP BOOP? This is NOT me!");
            caption.text = "PHOTO REJECTED";
            caption.color = new Color(1f, 0.35f, 0.35f);
            yield return new WaitForSeconds(2f);
            Fail();
        }

        IEnumerator NoFilter()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.8f);
            sparkle.Play();
            eyeLS = eyeRS = earS = snoutS = faceS = Vector2.one;
            foreach (SpriteRenderer sr in furParts) sr.color = Color.white;
            S41LKit.Say(bubble, "That's... the real me!");
            yield return new WaitForSeconds(1.0f);
            sparkle.Stop();
            // clic! flash della foto
            Sfx.Play(this, "clunk", 1f);
            Haptics.Medium();
            S41LKit.Alpha(flash, 1f);
            yield return StartCoroutine(S41LKit.Fade(flash, 0f, 0.6f));
            caption.text = "#NOFILTER";
            caption.color = new Color(0.95f, 0.3f, 0.55f);
            StartCoroutine(S41LKit.Pop(caption.transform, 0.3f, 0.3f));
            hearts.Play();
            S41LKit.Say(bubble, "1,000,000 likes! Woof!");
            Sfx.Play(this, "woof", 0.8f);
            for (float t = 0f; t < 2f; t += Time.deltaTime)
            {
                face.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 10f) * 4f);
                yield return null;
            }
            face.localRotation = Quaternion.identity;
            hearts.Stop();
            Win();
        }
    }
}
