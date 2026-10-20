using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Mirror twins" — due gemelli divisi da una crepa nel pavimento: uno ha la chiave, l'altro è davanti alla
    /// porta chiusa. Soluzione: porta lo specchio tra di loro e fallo scorrere finché la chiave RIFLESSA non combacia con
    /// la serratura: lì il riflesso si materializza e apre la porta. Sconfitta: 40 secondi.
    /// </summary>
    public class MirrorTwinsLevel : LevelController
    {
        [SerializeField] Transform mirror, key, reflection, lockPoint, door, twinA, twinB, timerFill;
        [SerializeField] Collider2D mirrorArea;
        [SerializeField] SpriteRenderer reflectionArt, doorArt, glow;
        [SerializeField] Sprite openDoorSprite;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float seconds = 40f, mirrorHalfHeight = 1.0f, matchDistance = 0.28f, matchHold = 0.5f;

        Camera cam;
        Vector2 grab;
        bool dragging, busy, firstReflection;
        float left, aligned, clock;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            left = seconds;
            reflection.gameObject.SetActive(false);
            S41EKit.SetAlpha(glow, 0f);
            S41EKit.Confetti(confetti, false);
            S41EKit.Say(bubble, "Psst! Toss me the key! ...Oh right, the crack.");
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            clock += dt;
            left -= dt;
            timerFill.localScale = new Vector3(Mathf.Clamp01(left / seconds), 1f, 1f);
            if (left <= 0f) { StartCoroutine(TimeUp()); return; }

            if (S41EKit.Pointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (began && mirrorArea.OverlapPoint(w)) { dragging = true; grab = (Vector2)mirror.position - w; Sfx.Play(this, "tick", 0.5f); }
                if (dragging)
                {
                    Vector2 p = w + grab;
                    mirror.position = new Vector3(Mathf.Clamp(p.x, -2.45f, 2.45f), Mathf.Clamp(p.y, -3.4f, 2.0f), mirror.position.z);
                    if (ended) dragging = false;
                }
            }
            else dragging = false;

            // riflesso della chiave rispetto alla linea verticale dello specchio
            float m = mirror.position.x;
            Vector2 k = key.position;
            bool visible = k.x < m - 0.1f && Mathf.Abs(k.y - mirror.position.y) < mirrorHalfHeight;
            reflection.gameObject.SetActive(visible);
            if (!visible) { aligned = 0f; S41EKit.SetAlpha(glow, 0f); return; }
            if (!firstReflection) { firstReflection = true; S41EKit.Say(bubble, "Whoa! A second key... in the mirror?"); Sfx.Play(this, "ding", 0.4f); }
            reflection.position = new Vector3(2f * m - k.x, k.y, reflection.position.z);
            S41EKit.SetAlpha(reflectionArt, 0.55f + 0.15f * Mathf.Sin(clock * 5f));
            float d = Vector2.Distance(reflection.position, lockPoint.position);
            S41EKit.SetAlpha(glow, Mathf.Clamp01(1f - d / 1.2f) * 0.8f);
            if (d < matchDistance) aligned += dt; else aligned = 0f;
            if (aligned >= matchHold) StartCoroutine(Unlock());
        }

        IEnumerator Unlock()
        {
            busy = true;
            dragging = false;
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            reflection.position = new Vector3(lockPoint.position.x, lockPoint.position.y, reflection.position.z);
            // il riflesso diventa vero
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.5f) { S41EKit.SetAlpha(reflectionArt, Mathf.Lerp(0.6f, 1f, t)); reflectionArt.color = Color.Lerp(reflectionArt.color, Color.white, t); yield return null; }
            reflectionArt.color = Color.white;
            yield return StartCoroutine(S41EKit.Pop(reflection, 1.3f, 0.3f));
            Sfx.Play(this, "clunk", 0.8f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f) { reflection.localRotation = Quaternion.Euler(90f * t, 0f, 0f); yield return null; }
            reflection.gameObject.SetActive(false);
            if (openDoorSprite != null) doorArt.sprite = openDoorSprite;
            S41EKit.SetAlpha(glow, 0f);
            S41EKit.Say(bubble, "IT OPENED! Mirror magic!");
            yield return S41EKit.Move(twinB, twinB.localPosition + new Vector3(0f, 0f, 0f), 0.3f, 0.4f);
            yield return S41EKit.Move(twinB, new Vector3(door.localPosition.x, twinB.localPosition.y, 0f), 0.6f, 0.2f);
            twinB.gameObject.SetActive(false);
            yield return S41EKit.Move(twinA, twinA.localPosition, 0.3f, 0.4f);
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.6f);
            S41EKit.Confetti(confetti, false);
            Win();
        }

        IEnumerator TimeUp()
        {
            busy = true;
            Sfx.Play(this, "buzz", 0.8f);
            S41EKit.Say(bubble, "Too late... we'll never get in!");
            yield return StartCoroutine(S41EKit.Shake(twinB, 0.1f, 0.5f));
            yield return new WaitForSeconds(1f);
            Fail();
        }
    }
}
