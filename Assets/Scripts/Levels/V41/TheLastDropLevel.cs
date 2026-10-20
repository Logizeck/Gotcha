using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The last drop" — una pianta vuole un'ultima goccia, ma la bottiglia appesa sopra è vuota. Una goccia è
    /// rimasta intrappolata nel tappo: inclinando il telefono scivola fino al foro (e la bottiglia dondola); a telefono
    /// dritto basta toccare lo schermo per farla cadere sulla pianta. Se la bottiglia oscilla, la goccia manca il vaso:
    /// tre mancati = sconfitta. Senza accelerometro compaiono due pulsanti da tenere premuti per inclinare.
    /// </summary>
    public class TheLastDropLevel : S41ILevelBase
    {
        [SerializeField] Transform pivot, drop, fallingDrop, pot;
        [SerializeField] SpriteRenderer plantArt, readyGlint;
        [SerializeField] Sprite bloomSprite;
        [SerializeField] TextMesh triesText;
        [SerializeField] GameObject tiltButtons;
        [SerializeField] Collider2D leftArea, rightArea;
        [SerializeField] float capY = -2.58f, holeY = -2.74f, potHalfWidth = 0.6f, potTopY = -3.1f, groundY = -4.35f;

        float angle, omega, u = -0.26f, v;
        bool ready, fallback;
        int misses;

        protected override void Begin()
        {
            fallback = !SystemInfo.supportsAccelerometer;
#if UNITY_EDITOR
            fallback = false;
#endif
            if (tiltButtons != null) tiltButtons.SetActive(fallback);
            fallingDrop.gameObject.SetActive(false);
            triesText.text = "MISSES: " + Marks(0, 3);
            PlaceDrop();
            Say("Water... just one drop...", 2.4f);
        }

        float TiltX()
        {
            float t = fallback ? 0f : Inputs.Tilt().x;
            if (fallback)
            {
                if (PointerInput.IsHeld(cam, leftArea)) t -= 0.6f;
                if (PointerInput.IsHeld(cam, rightArea)) t += 0.6f;
            }
            return Mathf.Clamp(t, -1f, 1f);
        }

        void PlaceDrop()
        {
            drop.localPosition = new Vector3(u, capY, drop.localPosition.z);
            if (readyGlint != null) Alpha(readyGlint, ready ? 0.6f + 0.4f * Mathf.Sin(clock * 8f) : 0f);
        }

        protected override void Tick()
        {
            if (busy) return;
            float dt = Time.deltaTime, tilt = TiltX();

            // la bottiglia appesa dondola verso il lato inclinato
            omega += ((tilt * 40f) - angle) * 30f * dt - omega * 4f * dt;
            angle += omega * dt;
            pivot.localRotation = Quaternion.Euler(0, 0, angle);

            // la goccia nel tappo: resta attaccata se il telefono è quasi dritto
            if (ready)
            {
                u = 0f;
                if (Mathf.Abs(tilt) > 0.45f) { ready = false; v = Mathf.Sign(tilt) * 0.3f; }
            }
            else if (Mathf.Abs(tilt) > 0.12f)
            {
                v += tilt * 2.5f * dt;
                v *= 1f - 2.5f * dt;
                float nu = u + v * dt;
                // lo scalino a sinistra trattiene la goccia se l'inclinazione è debole
                if (u <= -0.2f && nu > -0.2f && tilt < 0.25f) { nu = -0.2f; v = 0f; }
                if (nu < -0.27f || nu > 0.27f) { nu = Mathf.Clamp(nu, -0.27f, 0.27f); v = 0f; }
                // passando sul foro con un'inclinazione moderata la goccia ci si ferma
                bool crossed = (u < 0f) != (nu < 0f) || Mathf.Abs(nu) < 0.05f;
                u = nu;
                if (crossed && Mathf.Abs(tilt) < 0.4f) Capture();
            }
            else
            {
                v = 0f;
                if (Mathf.Abs(u) < 0.07f) Capture();
            }
            PlaceDrop();

            if (!Pointer(out Vector2 w, out bool began, out _) || !began) return;
            if (fallback && (leftArea.OverlapPoint(w) || rightArea.OverlapPoint(w))) return;
            if (ready) StartCoroutine(Squeeze());
            else { Sfx.Play(this, "clunk", 0.5f); StartCoroutine(Shake(pivot, 0.05f, 0.25f)); Say("Nothing comes out... check the cap!", 1.8f); }
        }

        void Capture()
        {
            ready = true;
            u = 0f;
            v = 0f;
            Sfx.Play(this, "tick", 0.6f);
            Haptics.Light();
            Say("It's at the hole! Hold still and tap!", 1.8f);
        }

        IEnumerator Squeeze()
        {
            busy = true;
            ready = false;
            Sfx.Play(this, "pop", 0.7f);
            Vector3 start = pivot.TransformPoint(new Vector3(0f, holeY, 0f));
            drop.gameObject.SetActive(false);
            fallingDrop.gameObject.SetActive(true);
            fallingDrop.position = new Vector3(start.x, start.y, fallingDrop.position.z);
            // velocità iniziale data dal dondolio
            float L = -holeY, a = angle * Mathf.Deg2Rad, w = omega * Mathf.Deg2Rad;
            Vector2 vel = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * w * L;
            Vector2 p = start;
            bool hit = false;
            while (true)
            {
                vel.y -= 9f * Time.deltaTime;
                p += vel * Time.deltaTime;
                fallingDrop.position = new Vector3(p.x, p.y, fallingDrop.position.z);
                if (p.y <= potTopY && p.y > potTopY - 0.4f && Mathf.Abs(p.x - pot.position.x) < potHalfWidth) { hit = true; break; }
                if (p.y <= groundY) break;
                yield return null;
            }
            fallingDrop.gameObject.SetActive(false);
            Haptics.Light();
            if (hit)
            {
                Sfx.Play(this, "ding", 0.7f);
                if (plantArt != null && bloomSprite != null) plantArt.sprite = bloomSprite;
                yield return StartCoroutine(Pop(plantArt.transform, 1.3f, 0.4f));
                Say("Ahh... the LAST drop. Perfect.", 0f);
                yield return StartCoroutine(Celebrate(1.6f));
                yield break;
            }
            misses++;
            triesText.text = "MISSES: " + Marks(misses, 3);
            Sfx.Play(this, "bonk", 0.6f);
            if (misses >= 3)
            {
                Say("Missed again... *wilts*", 0f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { plantArt.transform.localRotation = Quaternion.Euler(0, 0, -25f * k); yield return null; }
                yield return new WaitForSeconds(0.8f);
                Fail();
                yield break;
            }
            Say("Missed! Keep the bottle still... wait, one more drop in the cap!", 2.6f);
            yield return new WaitForSeconds(0.6f);
            u = -0.26f;
            v = 0f;
            drop.gameObject.SetActive(true);
            PlaceDrop();
            busy = false;
        }
    }
}
