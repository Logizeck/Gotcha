using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Tall Guest — una giraffa è alla porta della festa ma il collo esce dallo schermo: "Non ci sto nel tuo
    /// schermo!". Toccarla fa male alle ginocchia, lo zoom non funziona. Soluzione: girare il telefono in
    /// orizzontale (gravità |x| > 0.7 per 0.8 s): il mondo ruota di 90° per restare dritto, lo schermo ora è largo,
    /// la giraffa piega il collo ed entra dalla porta. Dopo 10 s compare l'icona "gira il telefono".
    /// </summary>
    public class GiraffeLevel : LevelController
    {
        [SerializeField] Transform world;          // ruota col telefono
        [SerializeField] GameObject portrait;
        [SerializeField] GameObject landscape;
        [SerializeField] Collider2D giraffeHit;
        [SerializeField] Transform giraffeWide;    // giraffa della scena orizzontale
        [SerializeField] Transform neckWide;
        [SerializeField] Transform headWide;
        [SerializeField] Transform rotateHint;
        [SerializeField] Transform confettiTemplate;
        [SerializeField] Transform doorPoint;
        [SerializeField] SpeechBubble bubble;      // scena verticale
        [SerializeField] SpeechBubble bubbleWide;  // scena orizzontale
        [SerializeField] float tiltNeeded = 0.7f, holdSeconds = 0.8f;

        float hold, talkCooldown, pinchStart;
        bool turning, pinching, pinchTold;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            landscape.SetActive(false);
            rotateHint.gameObject.SetActive(false);
            confettiTemplate.gameObject.SetActive(false);
            if (bubbleWide != null) bubbleWide.Hide();
            Say("I don't fit in your screen!");
        }

        void Say(string s)
        {
            if (bubble == null) return;
            bubble.Show(s);
            talkCooldown = 1.6f;
        }

        void Update()
        {
            if (turning || IsFinished) return;
            float dt = Time.deltaTime;
            talkCooldown -= dt;
            if (talkCooldown < -1.5f && bubble != null && bubble.gameObject.activeSelf) bubble.Hide();
            // niente icona "ruota il telefono": la soluzione si scopre da soli (o con gli hint)
            if (rotateHint.gameObject.activeSelf)
                rotateHint.localRotation = Quaternion.Euler(0, 0, -Mathf.SmoothStep(0f, 90f, Mathf.PingPong(Time.time * 0.8f, 1f)));

            // telefono di lato?
            Vector2 tilt = Inputs.Tilt();
            if (Mathf.Abs(tilt.x) > tiltNeeded) hold += dt; else hold = 0f;
            if (hold >= holdSeconds) { StartCoroutine(Turn(Mathf.Sign(tilt.x))); return; }

            // pizzico: niente zoom
            if (PointerInput.TryGetPinch(out float dist))
            {
                if (!pinching) { pinching = true; pinchStart = dist; pinchTold = false; }
                else if (!pinchTold && Mathf.Abs(dist - pinchStart) > 40f) { pinchTold = true; Say("No zooming at this party!"); Sfx.Play(this, "buzz", 0.4f); }
                return;
            }
            pinching = false;

            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
            {
                if (giraffeHit.OverlapPoint(PointerInput.ToWorld(cam, sp)))
                {
                    Haptics.Light();
                    Sfx.Play(this, "bonk", 0.6f);
                    Say(Random.value < 0.5f ? "Ouch, my knees!" : "Hey! Still not fitting!");
                }
            }
        }

        static List<SpriteRenderer> Renderers(GameObject g) => new List<SpriteRenderer>(g.GetComponentsInChildren<SpriteRenderer>(true));

        static void Alpha(List<SpriteRenderer> list, float a)
        {
            foreach (SpriteRenderer r in list) { Color c = r.color; c.a = a; r.color = c; }
        }

        IEnumerator Turn(float side)
        {
            turning = true;
            if (bubble != null) bubble.Hide();
            rotateHint.gameObject.SetActive(false);
            Sfx.Play(this, "whoosh", 0.7f);
            var oldR = Renderers(portrait);
            landscape.SetActive(true);
            var newR = Renderers(landscape);
            newR.RemoveAll(r => r.transform == confettiTemplate || r.transform.IsChildOf(confettiTemplate));
            Alpha(newR, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                float e = Mathf.SmoothStep(0f, 1f, k);
                world.localRotation = Quaternion.Euler(0, 0, 90f * side * e);
                Alpha(oldR, 1f - e);
                Alpha(newR, e);
                yield return null;
            }
            world.localRotation = Quaternion.Euler(0, 0, 90f * side);
            Alpha(newR, 1f);
            portrait.SetActive(false);
            Haptics.Light();
            if (bubbleWide != null) bubbleWide.Show("Ahh, a WIDE screen!");
            yield return new WaitForSeconds(0.6f);
            // piega il collo verso la porta
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.1f)
            {
                float a = -80f * Mathf.SmoothStep(0f, 1f, k);
                neckWide.localRotation = Quaternion.Euler(0, 0, a);
                headWide.localRotation = Quaternion.Euler(0, 0, -a);
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);
            if (bubbleWide != null) bubbleWide.Hide();
            // entra alla festa
            Vector3 from = giraffeWide.localPosition;
            for (float k = 0f; k < 2.4f; k += Time.deltaTime)
            {
                giraffeWide.localPosition = from + new Vector3(k * 3.4f, Mathf.Abs(Mathf.Sin(k * 9f)) * 0.05f, 0f);
                yield return null;
            }
            Sfx.Play(this, "pop", 0.8f);
            Sfx.Play(this, "ding", 0.6f);
            Haptics.Medium();
            for (int i = 0; i < 26; i++) StartCoroutine(Confetti(i));
            if (bubbleWide != null) bubbleWide.Show("PARTY TIME!");
            yield return new WaitForSeconds(1.8f);
            Win();
        }

        IEnumerator Confetti(int i)
        {
            Transform c = Instantiate(confettiTemplate, confettiTemplate.parent);
            c.gameObject.SetActive(true);
            SpriteRenderer sr = c.GetComponentInChildren<SpriteRenderer>();
            Color[] colors = { new Color(1f, 0.36f, 0.54f), new Color(1f, 0.82f, 0.35f), new Color(0.43f, 0.78f, 1f), new Color(0.5f, 0.83f, 0.42f), new Color(0.73f, 0.64f, 1f) };
            if (sr != null) sr.color = colors[i % colors.Length];
            Vector3 p = doorPoint.localPosition;
            Vector3 v = new Vector3(Random.Range(-3.5f, 1.5f), Random.Range(2.5f, 5.5f), 0f);
            float spin = Random.Range(-600f, 600f);
            for (float k = 0f; k < 1.8f; k += Time.deltaTime)
            {
                v.y -= 6f * Time.deltaTime;
                p += v * Time.deltaTime;
                c.localPosition = p;
                c.Rotate(0, 0, spin * Time.deltaTime);
                yield return null;
            }
            Destroy(c.gameObject);
        }
    }
}
