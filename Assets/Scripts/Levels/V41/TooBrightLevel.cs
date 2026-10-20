using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Too bright!" — un vampiro deve attraversare una stanza illuminata dal sole fino alla sua bara. Nella luce
    /// si muove a fatica e si scotta: 5 secondi di esposizione in totale = sconfitta. Il trucco: fai buio chiudendo le
    /// tende virtuali (trascinandole sulle finestre), poi trascina il vampiro fino all'uscita. Unity non offre un
    /// sensore di luce portabile, quindi c'è solo la versione con le tende.
    /// </summary>
    public class TooBrightLevel : S41FLevelBase
    {
        [SerializeField] Transform[] curtains;          // bordo sinistro in alto, scala x = quanto è chiusa
        [SerializeField] Collider2D[] windowAreas;
        [SerializeField] SpriteRenderer[] beams;
        [SerializeField] Vector2[] litZones = { new Vector2(-1.65f, 0.25f), new Vector2(0.8f, 2.7f) };           // impronta a terra di ogni raggio (x min, x max)
        [SerializeField] float[] windowLefts = { -2.07f, 0.38f };
        [SerializeField] Transform vampire;             // piedi del vampiro
        [SerializeField] Collider2D vampireArea;
        [SerializeField] SpriteRenderer vampireArt;
        [SerializeField] Sprite idleSprite, burntSprite;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] Transform sunMeter;
        [SerializeField] Transform coffin;
        [SerializeField] float curtainWidth = 1.64f, minClosed = 0.12f, maxExposure = 5f, fastSpeed = 1.6f, slowSpeed = 0.35f;

        float[] closed;
        float exposure, targetX;
        int grabCurtain = -1;
        bool dragVampire, smoking, saidCurtains;
        float grabOffset;

        protected override void Begin()
        {
            closed = new float[curtains.Length];
            for (int i = 0; i < closed.Length; i++) closed[i] = minClosed;
            targetX = vampire.localPosition.x;
            if (smoke != null) smoke.Stop();
            Apply();
            Say("Ugh, who opened the windows? I need to reach my coffin!", 3f);
        }

        float Open(int i) => 1f - Mathf.InverseLerp(minClosed, 1f, closed[i]);

        /// <summary>Quanta luce c'è sul pavimento in x (0..1).</summary>
        float LightAt(float x)
        {
            float l = 0f;
            for (int i = 0; i < litZones.Length && i < closed.Length; i++)
                if (x > litZones[i].x && x < litZones[i].y) l = Mathf.Max(l, Open(i));
            return l;
        }

        protected override void Tick()
        {
            if (busy) return;
            if (Pointer(out Vector2 w, out bool began, out bool ended))
            {
                if (began)
                {
                    grabCurtain = -1;
                    dragVampire = vampireArea != null && vampireArea.OverlapPoint(w);
                    if (dragVampire) { grabOffset = vampire.position.x - w.x; Sfx.Play(this, "pop", 0.4f); }
                    else for (int i = 0; i < windowAreas.Length; i++) if (windowAreas[i] != null && windowAreas[i].OverlapPoint(w)) { grabCurtain = i; Sfx.Play(this, "whoosh", 0.4f); }
                }
                if (grabCurtain >= 0) closed[grabCurtain] = Mathf.Clamp((w.x - windowLefts[grabCurtain]) / curtainWidth, minClosed, 1f);
                if (dragVampire) targetX = Mathf.Clamp(transform.InverseTransformPoint(new Vector3(w.x + grabOffset, 0f, 0f)).x, -2.4f, 2.4f);
                if (ended) { grabCurtain = -1; dragVampire = false; }
            }
            else { grabCurtain = -1; dragVampire = false; }

            // il vampiro avanza verso il dito: nella luce arranca
            Vector3 p = vampire.localPosition;
            float light = LightAt(p.x);
            float speed = Mathf.Lerp(fastSpeed, slowSpeed, light);
            float nx = Mathf.MoveTowards(p.x, targetX, speed * Time.deltaTime);
            bool walking = Mathf.Abs(nx - p.x) > 0.0001f;
            vampire.localPosition = new Vector3(nx, p.y, p.z);
            if (vampireArt != null)
            {
                vampireArt.transform.localPosition = new Vector3(vampireArt.transform.localPosition.x, walking ? Mathf.Abs(Mathf.Sin(clock * 12f)) * 0.05f + ArtBaseY : ArtBaseY, 0f);
                if (Mathf.Abs(targetX - nx) > 0.01f) vampireArt.flipX = targetX < nx;
            }

            bool burning = light > 0.15f;
            if (burning) exposure += Time.deltaTime * light;
            if (burning != smoking)
            {
                smoking = burning;
                if (smoke != null) { if (burning) smoke.Play(); else smoke.Stop(); }
                if (vampireArt != null) vampireArt.sprite = burning ? burntSprite : idleSprite;
                if (burning) { Say(saidCurtains ? "HSSSS! Too bright!" : "HSSSS! Make it DARKER!", 1.6f); saidCurtains = true; Sfx.Play(this, "buzz", 0.4f); Haptics.Light(); }
            }
            Apply();
            if (exposure >= maxExposure) { StartCoroutine(Toast()); return; }
            if (coffin != null && nx >= coffin.localPosition.x - 0.3f) StartCoroutine(Bedtime());
        }

        float artBaseY = float.NaN;
        float ArtBaseY
        {
            get
            {
                if (float.IsNaN(artBaseY)) artBaseY = vampireArt != null ? vampireArt.transform.localPosition.y : 0f;
                return artBaseY;
            }
        }

        void Apply()
        {
            for (int i = 0; i < curtains.Length; i++)
            {
                curtains[i].localScale = new Vector3(closed[i], 1f, 1f);
                if (i < beams.Length) Alpha(beams[i], Open(i) * 0.9f);
            }
            if (sunMeter != null) sunMeter.localScale = new Vector3(Mathf.Clamp01(1f - exposure / maxExposure), 1f, 1f);
        }

        IEnumerator Toast()
        {
            busy = true;
            Say("TOO... BRIGHT... *poof*", 2f);
            if (smoke != null) smoke.Play();
            Sfx.Play(this, "whoosh", 1f);
            Haptics.Heavy();
            Vector3 s = vampire.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                vampire.localScale = new Vector3(s.x * (1f - k * 0.7f), s.y * (1f - k), 1f);
                yield return null;
            }
            yield return new WaitForSeconds(0.8f);
            if (smoke != null) smoke.Stop();
            Fail();
        }

        IEnumerator Bedtime()
        {
            busy = true;
            if (smoke != null) smoke.Stop();
            if (vampireArt != null) { vampireArt.sprite = idleSprite; vampireArt.flipX = false; }
            yield return Walk(vampire, new Vector3(coffin.localPosition.x, vampire.localPosition.y, 0f), 1.2f);
            Say("Ahh, darkness. Goodnight!", 2f);
            Sfx.Play(this, "clunk", 0.6f);
            yield return new WaitForSeconds(0.4f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { vampire.localScale = Vector3.one * (1f - k); yield return null; }
            vampire.gameObject.SetActive(false);
            yield return Pop(coffin, 1.12f, 0.3f);
            yield return Celebrate();
        }
    }
}
