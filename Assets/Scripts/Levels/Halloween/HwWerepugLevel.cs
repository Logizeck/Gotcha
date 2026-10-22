using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.Halloween;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 245 "Toni the were-pug" — la luna piena ha trasformato Toni in un lupo-carlino. Toccarlo lo fa solo
    /// ringhiare, la luna non si sposta. Soluzione: toccare la scatola dei biscotti (ne esce UNO solo) e trascinare il
    /// biscotto tondo davanti alla luna finché la copre: la luce si spegne e Toni torna un carlino normale.
    /// Il biscotto, alzato verso il cielo, "cresce" in prospettiva fino alla misura della luna.
    /// </summary>
    public class HwWerepugLevel : LevelController
    {
        [SerializeField] Transform moon;
        [SerializeField] float moonRadius = 0.8f;
        [SerializeField] SpriteRenderer moonGlow;
        [SerializeField] SpriteRenderer nightTint;
        [SerializeField] Transform werepug;
        [SerializeField] Collider2D werepugArea;
        [SerializeField] GameObject toniNormal;
        [SerializeField] Transform toniTail;
        [SerializeField] Transform box;
        [SerializeField] SpriteRenderer boxArt;
        [SerializeField] Sprite boxOpenSprite;
        [SerializeField] Collider2D boxArea;
        [SerializeField] Collider2D moonArea;
        [SerializeField] Transform biscuit;
        [SerializeField] Collider2D biscuitArea;
        [SerializeField] float biscuitRadius = 0.3f;
        [SerializeField] Transform biscuitSpot;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SmokeEmitter[] confetti;

        readonly WerepugLogic logic = new WerepugLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item biscuitItem;
        HwSay say;
        Vector3 biscuitBase;
        int growls;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            say = new HwSay(bubble);
            drag = new HwDragger(this);
            biscuitBase = biscuit.localScale;
            biscuit.position = box.position;
            biscuit.gameObject.SetActive(false);
            biscuitItem = drag.Add(biscuit, biscuitArea, 5);
            biscuitItem.Home = biscuitSpot.position;
            biscuitItem.Enabled = false;
            toniNormal.SetActive(false);
            werepug.gameObject.SetActive(true);
            HalloweenKit.Confetti(confetti, false);
            if (hearts != null) hearts.Stop();
            if (smoke != null) smoke.Stop();
            say.Show("GRRRR... AWOOOO!", 2f);
        }

        void Update()
        {
            say.Tick();
            if (IsFinished || busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) DropBiscuit();
                return;
            }
            if (began && drag.TryPick(w) != null) logic.TryPickBiscuit();
            else if (began) Tap(w);
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            biscuit.localScale = biscuitBase * PerspectiveScale(biscuit.position.y);
            if (ended) DropBiscuit();
        }

        /// <summary>Più il biscotto sale verso il cielo, più "copre" (prospettiva): in alto è grande come la luna.</summary>
        float PerspectiveScale(float y)
        {
            float k = Mathf.InverseLerp(biscuitSpot.position.y + 0.3f, moon.position.y - 0.4f, y);
            float full = moonRadius * 1.08f / Mathf.Max(0.05f, biscuitRadius);
            return Mathf.Lerp(1.12f, full, k * k * (3f - 2f * k));
        }

        void Tap(Vector2 w)
        {
            if (werepugArea.OverlapPoint(w)) { StartCoroutine(Growl()); return; }
            if (moonArea.OverlapPoint(w))
            {
                StartCoroutine(HalloweenKit.Pop(moon, 1.06f, 0.25f));
                Sfx.Play(this, "bonk", 0.4f);
                say.Show(Random.value < 0.5f ? "AWOOOOOO!" : "The MOON! GRRR!", 1.6f);
                return;
            }
            if (boxArea.OverlapPoint(w))
            {
                if (logic.TryOpenBox()) StartCoroutine(OpenBox());
                else
                {
                    StartCoroutine(HalloweenKit.Shake(box, 0.06f, 0.3f));
                    Sfx.Play(this, "tick", 0.5f);
                    say.Show("*sniff* Only ONE biscuit left!", 1.8f);
                }
            }
        }

        IEnumerator Growl()
        {
            busy = true;
            growls++;
            Haptics.Light();
            Sfx.Play(this, "buzz", 0.45f);
            Sfx.Play(this, "woof", 0.6f);
            string[] lines = { "GRRRRR!", "GRRR... *wag*... GRRR!", "Rrrr... WOOF?!", "I'm a MONSTER! GRRR!" };
            say.Show(lines[growls % lines.Length], 1.5f);
            yield return HalloweenKit.Shake(werepug, 0.07f, 0.4f);
            busy = false;
        }

        IEnumerator OpenBox()
        {
            busy = true;
            boxArt.sprite = boxOpenSprite;
            Sfx.Play(this, "pop", 0.6f);
            Haptics.Light();
            yield return HalloweenKit.Pop(box, 1.12f, 0.2f);
            biscuit.gameObject.SetActive(true);
            biscuit.position = box.position + Vector3.up * 0.3f;
            biscuit.localScale = biscuitBase * 1.12f;
            yield return HalloweenKit.Move(biscuit, biscuitSpot.position, 0.4f, 0.8f);
            drag.SetBaseScale(biscuitItem, biscuitBase * 1.12f);
            biscuitItem.Enabled = true;
            busy = false;
        }

        void DropBiscuit()
        {
            float r = biscuitRadius * biscuit.localScale.x / Mathf.Max(0.01f, biscuitBase.x);
            bool covers = WerepugLogic.Covers(biscuit.position.x, biscuit.position.y, r, moon.position.x, moon.position.y, moonRadius);
            drag.Release();
            if (logic.TryDropBiscuit(covers))
            {
                biscuitItem.Enabled = false;
                StartCoroutine(TurnBack());
                return;
            }
            // non copre: torna al suo posto (e alla sua misura)
            StartCoroutine(HalloweenKit.ScaleTo(biscuit, biscuitBase * 1.12f, 0.3f));
            drag.SendHome(biscuitItem, 0.35f);
            if (biscuit.position.y > biscuitSpot.position.y + 1.5f) say.Show("GRRR? *drool*", 1.4f);
        }

        IEnumerator TurnBack()
        {
            busy = true;
            // il biscotto si incastra davanti alla luna
            float full = moonRadius * 1.08f / Mathf.Max(0.05f, biscuitRadius);
            StartCoroutine(HalloweenKit.ScaleTo(biscuit, biscuitBase * full, 0.25f));
            yield return HalloweenKit.Move(biscuit, new Vector3(moon.position.x, moon.position.y, biscuit.position.z), 0.25f);
            Sfx.Play(this, "clunk", 0.7f);
            Haptics.Medium();
            // la luce della luna si spegne
            StartCoroutine(HalloweenKit.Fade(moonGlow, 0f, 0.8f));
            yield return HalloweenKit.Fade(nightTint, 0.35f, 0.8f);
            say.Show("Grr... grr...? ...woof?", 1.4f);
            // la trasformazione (una sola volta, input bloccato)
            if (smoke != null) smoke.Play();
            Haptics.Heavy();
            Sfx.Play(this, "whoosh", 0.8f);
            yield return HalloweenKit.Shake(werepug, 0.12f, 0.9f);
            werepug.gameObject.SetActive(false);
            toniNormal.SetActive(true);
            yield return HalloweenKit.Pop(toniNormal.transform, 1.2f, 0.3f);
            if (smoke != null) smoke.Stop();
            Sfx.Play(this, "woof", 0.8f);
            yield return new WaitForSeconds(0.5f);
            say.Show("Woof! A COOKIE MOON! Can I eat it?", 2.4f);
            if (hearts != null) hearts.Play();
            HalloweenKit.Confetti(confetti, true);
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Success();
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                if (toniTail != null) toniTail.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 22f) * 25f);
                yield return null;
            }
            if (hearts != null) hearts.Stop();
            HalloweenKit.Confetti(confetti, false);
            if (logic.TryCompleteTransform()) Win();
        }
    }
}
