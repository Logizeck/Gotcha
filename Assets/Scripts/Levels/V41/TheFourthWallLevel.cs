using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The fourth wall" — un attore è chiuso dentro un quadro con quattro pareti e chiede aiuto. Trascinarlo
    /// contro le pareti è inutile (tre urti = sconfitta). Il trucco: trascina la CORNICE fuori dallo schermo finché un
    /// lato esce dal display; quella parete sparisce e l'attore esce dal quadro sul palco e si inchina.
    /// </summary>
    public class TheFourthWallLevel : LevelController
    {
        [SerializeField] Transform frame;            // radice della cornice (centro del quadro)
        [SerializeField] Transform actor;            // piedi dell'attore, figlio della cornice
        [SerializeField] Collider2D actorArea, frameArea;
        [SerializeField] Transform wallLeft, wallRight;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float innerHalfX = 1.3f, floorY = -1.2f, ceilY = 0.2f, bumpMargin = 0.3f, frameHalfW = 1.7f;
        [SerializeField] float stageY = -2.75f;

        Camera cam;
        Vector3 frameHome;
        Vector2 grabOffset;
        int grab;              // 0 niente, 1 attore, 2 cornice
        int bumps;
        bool busy;
        float sayUntil, clock;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            frameHome = frame.localPosition;
            S41DKit.Confetti(confetti, false);
            Say("Help! I'm stuck inside this painting!", 3f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            sayUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && sayUntil > 0f && clock > sayUntil && !busy) { bubble.Hide(); sayUntil = 0f; }
            if (busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { grab = 0; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (actorArea != null && actorArea.OverlapPoint(w)) { grab = 1; grabOffset = (Vector2)actor.position - w; }
                else if (frameArea != null && frameArea.OverlapPoint(w)) { grab = 2; grabOffset = (Vector2)frame.position - w; }
                else grab = 0;
            }
            if (grab == 1) DragActor(w + grabOffset);
            else if (grab == 2) DragFrame(w + grabOffset);
            if (ended) grab = 0;
        }

        void DragActor(Vector2 world)
        {
            Vector3 local = frame.InverseTransformPoint(world);
            bool hitSide = Mathf.Abs(local.x) > innerHalfX + bumpMargin;
            bool hitTop = local.y > ceilY + bumpMargin;
            local.x = Mathf.Clamp(local.x, -innerHalfX, innerHalfX);
            local.y = Mathf.Clamp(local.y, floorY, ceilY);
            actor.localPosition = new Vector3(local.x, local.y, 0f);
            if (hitSide || hitTop) { grab = 0; StartCoroutine(Bump(hitSide ? Mathf.Sign(local.x) : 0f)); }
        }

        void DragFrame(Vector2 world)
        {
            float hw = S41DKit.HalfWidth(cam);
            Vector3 p = frame.position;
            p.x = Mathf.Clamp(world.x, -hw - 0.6f, hw + 0.6f);
            p.y = Mathf.Clamp(world.y, frameHome.y - 0.8f, frameHome.y + 0.6f);
            frame.position = p;
            // un lato della cornice è uscito dallo schermo: quella parete non c'è più
            if (p.x + frameHalfW > hw + 0.05f) { grab = 0; StartCoroutine(Escape(1f)); }
            else if (p.x - frameHalfW < -hw - 0.05f) { grab = 0; StartCoroutine(Escape(-1f)); }
        }

        IEnumerator Bump(float side)
        {
            busy = true;
            bumps++;
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.9f);
            Vector3 home = actor.localPosition;
            for (float k = 0f; k < 0.35f; k += Time.deltaTime)
            {
                actor.localScale = new Vector3(1f - 0.25f * Mathf.Sin(k / 0.35f * Mathf.PI), 1f + 0.12f * Mathf.Sin(k / 0.35f * Mathf.PI), 1f);
                actor.localPosition = home - new Vector3(side * 0.25f * Mathf.Sin(k / 0.35f * Mathf.PI), 0f, 0f);
                yield return null;
            }
            actor.localScale = Vector3.one;
            actor.localPosition = home;
            if (bumps >= 3)
            {
                Say("Ow... I see stars... the walls win.", 3f);
                for (float k = 0f; k < 1.2f; k += Time.deltaTime) { actor.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 14f) * 12f); yield return null; }
                actor.localRotation = Quaternion.Euler(0, 0, 80f);
                Sfx.Play(this, "clunk", 0.8f);
                Haptics.Heavy();
                yield return new WaitForSeconds(1.0f);
                Fail();
                yield break;
            }
            Say(bumps == 1 ? "OUCH! These walls are solid!" : "OOF! Stop pushing ME around!", 2.2f);
            busy = false;
        }

        IEnumerator Escape(float side)
        {
            busy = true;
            Transform wall = side > 0f ? wallRight : wallLeft;
            Sfx.Play(this, "whoosh", 0.8f);
            Haptics.Light();
            // la parete uscita dallo schermo resta là fuori
            Vector3 wallWorld = wall.position;
            wall.SetParent(transform, true);
            Vector3 frameFrom = frame.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                frame.localPosition = Vector3.Lerp(frameFrom, frameHome, k * k * (3f - 2f * k));
                wall.position = wallWorld + new Vector3(side * k * 2f, -k * k * 3f, 0f);
                wall.localRotation = Quaternion.Euler(0, 0, -side * 60f * k);
                yield return null;
            }
            frame.localPosition = frameHome;
            wall.gameObject.SetActive(false);
            Sfx.Play(this, "clunk", 0.5f);
            Say("Wait... the wall is GONE!", 1.6f);
            yield return new WaitForSeconds(1.0f);
            // cammina fuori dal quadro
            Vector3 exitLocal = new Vector3(side * (frameHalfW + 0.35f), floorY, 0f);
            Vector3 a = actor.localPosition;
            actor.localScale = new Vector3(side, 1f, 1f);
            float walk = Mathf.Abs(exitLocal.x - a.x) / 1.6f + 0.2f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / walk)
            {
                actor.localPosition = Vector3.Lerp(new Vector3(a.x, floorY, 0f), exitLocal, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * walk * 12f)) * 0.06f;
                yield return null;
            }
            // salta giù sul palco
            actor.SetParent(transform, true);
            Vector3 from = actor.position;
            Vector3 to = new Vector3(Mathf.Clamp(from.x + side * 0.3f, -2.2f, 2.2f), stageY, 0f);
            Sfx.Play(this, "whoosh", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                actor.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.6f;
                yield return null;
            }
            actor.position = to;
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Light();
            // inchino verso il pubblico (cioè te)
            actor.localScale = Vector3.one;
            Say("FREE! Thank you, thank you!", 3f);
            for (int i = 0; i < 2; i++)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { actor.localRotation = Quaternion.Euler(0, 0, -side * 35f * Mathf.Sin(k * Mathf.PI)); yield return null; }
            }
            actor.localRotation = Quaternion.identity;
            S41DKit.Confetti(confetti, true);
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(1.6f);
            S41DKit.Confetti(confetti, false);
            Win();
        }
    }
}
