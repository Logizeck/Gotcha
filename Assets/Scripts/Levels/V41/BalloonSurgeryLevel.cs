using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Balloon surgery" — un palloncino tiene sospesa una chiave sopra un pavimento pieno di punte; il topolino
    /// aspetta sulla colonna sicura in mezzo. Soluzione: pizzicare con due dita per sgonfiarlo PIANO (la grandezza segue
    /// la distanza fra le dita): scende e la chiave si posa sulla colonna. Stringere troppo in fretta lo fa scoppiare:
    /// la chiave vola sulle punte → Fail(). (Editor: rotella del mouse.)
    /// </summary>
    public class BalloonSurgeryLevel : LevelController
    {
        [SerializeField] Transform balloon;              // perno sul nodo
        [SerializeField] SpriteRenderer balloonArt;
        [SerializeField] Transform key;                  // punta della chiave (la chiave pende in giù)
        [SerializeField] LineRenderer rope;
        [SerializeField] Transform mouse;
        [SerializeField] SpriteRenderer scrap;           // brandello del palloncino scoppiato
        [SerializeField] SpriteRenderer[] puffs;
        [SerializeField] TextMesh status;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float baseScale = 0.8f, minSize = 0.3f, topKnotY = 1.6f, ledgeY = -2.2f, spikesY = -3.75f;
        [SerializeField] float ropeLength = 0.5f, keyLength = 0.89f, warnRate = 0.45f, popRate = 0.95f;

        Camera cam;
        float size = 1f, rate, prevSize = 1f, pinchD0, pinchS0, clock, warnSound, bubbleUntil;
        bool pinching, busy;
        float knotY;

        float LandKnotY => ledgeY + keyLength + ropeLength;
        float KnotFor(float s) => Mathf.Lerp(LandKnotY, topKnotY, Mathf.InverseLerp(minSize + 0.05f, 1f, s));

        public override void OnLevelStart()
        {
            cam = Camera.main;
            knotY = topKnotY;
            if (scrap != null) scrap.enabled = false;
            foreach (SpriteRenderer p in puffs) p.enabled = false;
            Place();
            SetStatus("GENTLY...", new Color(0.3f, 0.3f, 0.45f));
            Say("My key! Bring it down... carefully!", 2.4f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void SetStatus(string text, Color c)
        {
            if (status == null) return;
            status.text = text;
            status.color = c;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }

            float target = size;
            if (PointerInput.TryGetPinch(out float d))
            {
                if (!pinching) { pinching = true; pinchD0 = Mathf.Max(d, 1f); pinchS0 = size; }
                target = pinchS0 * d / pinchD0;
            }
            else pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) target = size + wheel * 0.025f;
#endif
            target = Mathf.Clamp(target, minSize, 1f);
            size = target;

            // velocità di sgonfiaggio (media breve)
            float inst = (prevSize - size) / dt;
            prevSize = size;
            rate = Mathf.Lerp(rate, inst, 1f - Mathf.Exp(-dt / 0.12f));
            if (rate > popRate) { StartCoroutine(Pop()); return; }
            if (rate > warnRate)
            {
                SetStatus("TOO FAST!", new Color(0.9f, 0.2f, 0.2f));
                warnSound -= dt;
                if (warnSound <= 0f) { Sfx.Play(this, "tick", 0.8f); Haptics.Light(); warnSound = 0.12f; }
            }
            else SetStatus(size < 0.99f ? "SLOWLY... GOOD" : "GENTLY...", new Color(0.3f, 0.3f, 0.45f));

            // il palloncino scende piano verso la quota che corrisponde alla sua grandezza
            knotY = Mathf.MoveTowards(knotY, KnotFor(size), dt * 1.6f);
            Place();
            if (knotY <= LandKnotY + 0.01f && size <= minSize + 0.06f) StartCoroutine(Landed());
        }

        void Place()
        {
            float tremble = rate > warnRate ? Random.Range(-0.03f, 0.03f) : 0f;
            balloon.position = new Vector3(Mathf.Sin(clock * 1.1f) * 0.06f * size + tremble, knotY, 0f);
            balloon.localScale = Vector3.one * baseScale * size;
            Vector3 tip = new Vector3(balloon.position.x * 0.5f, knotY - ropeLength - keyLength, 0f);
            key.position = tip;
            key.localRotation = Quaternion.Euler(0f, 0f, -90f + Mathf.Sin(clock * 1.3f) * 4f);
            UpdateRope();
        }

        void UpdateRope()
        {
            if (rope == null) return;
            rope.SetPosition(0, balloon.position);
            rope.SetPosition(1, key.position + key.right * -keyLength * 1.0f);
        }

        IEnumerator Pop()
        {
            busy = true;
            balloonArt.enabled = false;
            if (rope != null) rope.enabled = false;
            if (scrap != null) { scrap.enabled = true; scrap.transform.position = balloon.position + Vector3.up * 0.4f; }
            foreach (SpriteRenderer p in puffs)
            {
                p.enabled = true;
                p.transform.position = balloon.position + Vector3.up * 0.6f + (Vector3)(Random.insideUnitCircle * 0.5f);
            }
            Sfx.Play(this, "pop", 1f);
            Sfx.Play(this, "bonk", 0.6f);
            Haptics.Medium();
            SetStatus("POP!", new Color(0.9f, 0.2f, 0.2f));
            Say("NOOOO!", 1.5f);
            // lo scoppio lancia la chiave di lato, oltre la colonna
            float dir = key.position.x > 0.02f ? 1f : (key.position.x < -0.02f ? -1f : (Random.value < 0.5f ? -1f : 1f));
            Vector2 v = new Vector2(dir * 1.9f, 4f);
            Vector3 p0 = key.position;
            float spin = 0f;
            while (key.position.y > spikesY || Mathf.Abs(key.position.x) < 0.8f)
            {
                float dt = Time.deltaTime;
                v.y -= 12f * dt;
                key.position += (Vector3)(v * dt);
                spin += dt * 540f * dir;
                key.localRotation = Quaternion.Euler(0f, 0f, -90f - spin);
                if (scrap != null) scrap.transform.position += Vector3.down * dt * 1.5f;
                foreach (SpriteRenderer p in puffs) S41AKit.Alpha(p, Mathf.Max(0f, p.color.a - dt * 1.5f));
                if (key.position.y < -6f) break;
                yield return null;
            }
            key.position = new Vector3(key.position.x, spikesY, 0f);
            Sfx.Play(this, "clunk", 1f);
            Haptics.Heavy();
            yield return S41AKit.Bounce(key, 0.2f, 0.2f);
            Say("It's on the spikes...", 1.6f);
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        IEnumerator Landed()
        {
            busy = true;
            SetStatus("PERFECT!", new Color(0.2f, 0.6f, 0.25f));
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Light();
            Say("Got it! Thanks, doc!", 1.8f);
            yield return S41AKit.Move(mouse, new Vector3(key.position.x + 0.45f, mouse.position.y, 0f), 0.5f, 0.15f);
            Sfx.Play(this, "ding", 0.8f);
            // il topolino prende la chiave e la alza in trionfo
            if (rope != null) rope.enabled = false;
            key.SetParent(mouse, true);
            Vector3 up = key.position + Vector3.up * 0.5f;
            yield return S41AKit.Move(key, up, 0.4f);
            yield return S41AKit.Bounce(mouse, 0.2f, 0.3f);
            yield return new WaitForSeconds(0.9f);
            Win();
        }
    }
}
