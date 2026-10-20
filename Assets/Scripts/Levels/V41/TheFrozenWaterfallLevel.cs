using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The frozen waterfall" — una cascata ghiacciata imprigiona la chiave del topo. Il trucco: trascina la
    /// lente davanti al sole (nella sua luce) e orientala con l'anello di mira finché il raggio concentrato colpisce
    /// il ghiaccio; il ghiaccio si scioglie solo se il raggio resta allineato per 4 secondi. Tre interruzioni lunghe
    /// a lavoro iniziato lo fanno ricongelare (sconfitta).
    /// </summary>
    public class TheFrozenWaterfallLevel : S41JLevelBase
    {
        [SerializeField] Transform sun;
        [SerializeField] Transform lens;              // locale +x = direzione del raggio
        [SerializeField] Collider2D lensArea;
        [SerializeField] Collider2D aimArea;
        [SerializeField] LineRenderer sunlight;
        [SerializeField] LineRenderer beam;
        [SerializeField] SpriteRenderer hotSpot;
        [SerializeField] Collider2D iceArea;
        [SerializeField] SpriteRenderer iceArt;
        [SerializeField] SpriteRenderer waterArt;
        [SerializeField] SmokeEmitter steam;
        [SerializeField] Transform meltGauge;         // scala x 0..1
        [SerializeField] Transform key;
        [SerializeField] Transform mouse;
        [SerializeField] TextMesh triesText;
        [SerializeField] float sunReach = 2.3f, beamLength = 7f, groundY = -3.3f;

        const int MaxTries = 3;
        const float MeltTime = 4f, LongBreak = 1.0f;
        int grab;                                     // 0 niente, 1 lente, 2 mira
        Vector2 grabOffset;
        float progress, offTime;
        int tries;
        bool saidLit, saidHit;

        protected override void Begin()
        {
            Alpha(waterArt, 0f);
            if (steam != null) steam.Stop();
            UpdateTries();
            UpdateBeam(out _);
            Say("My key is stuck in the ice! Brrr...", 3f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "REFREEZES " + tries + "/" + MaxTries; }

        /// <summary>Luce del sole sulla lente e raggio concentrato; restituisce se il raggio tocca il ghiaccio.</summary>
        bool UpdateBeam(out Vector2 hit)
        {
            Vector2 l = lens.position;
            bool lit = Vector2.Distance(l, sun.position) < sunReach;
            hit = l;
            sunlight.enabled = lit;
            beam.enabled = lit;
            if (!lit) { Alpha(hotSpot, 0f); return false; }
            sunlight.SetPosition(0, sun.position);
            sunlight.SetPosition(1, l);
            Vector2 dir = lens.right;
            bool onIce = false;
            Vector2 end = l + dir * beamLength;
            for (float d = 0.3f; d < beamLength; d += 0.05f)
            {
                Vector2 p = l + dir * d;
                if (iceArea != null && iceArea.OverlapPoint(p)) { end = p + dir * 0.15f; onIce = true; break; }
                if (Mathf.Abs(p.x) > 3.2f || p.y < groundY - 0.3f || p.y > 4f) { end = p; break; }
            }
            beam.SetPosition(0, l);
            beam.SetPosition(1, end);
            hit = end;
            hotSpot.transform.position = new Vector3(end.x, end.y, hotSpot.transform.position.z);
            Alpha(hotSpot, onIce ? 0.85f : 0.4f);
            return onIce;
        }

        protected override void Tick()
        {
            if (busy) return;
            if (Pointer(out Vector2 w, out bool began, out bool ended))
            {
                if (began)
                {
                    if (aimArea != null && aimArea.OverlapPoint(w)) { grab = 2; Sfx.Play(this, "tick", 0.4f); }
                    else if (lensArea != null && lensArea.OverlapPoint(w)) { grab = 1; grabOffset = (Vector2)lens.position - w; Sfx.Play(this, "tick", 0.4f); }
                }
                if (grab == 1)
                {
                    Vector2 p = w + grabOffset;
                    lens.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 0.8f), Mathf.Clamp(p.y, groundY + 0.4f, 2.8f), lens.position.z);
                }
                else if (grab == 2)
                {
                    Vector2 d = w - (Vector2)lens.position;
                    if (d.sqrMagnitude > 0.04f) lens.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                }
                if (ended) grab = 0;
            }
            else grab = 0;

            bool lit = Vector2.Distance(lens.position, sun.position) < sunReach;
            if (lit && !saidLit) { saidLit = true; Say("Ooh, sunlight through the lens!", 2f); }
            bool onIce = UpdateBeam(out _);
            if (onIce)
            {
                if (!saidHit) { saidHit = true; Say("It's melting! Keep it steady!", 2.2f); }
                progress += Time.deltaTime;
                offTime = 0f;
                if (steam != null) { steam.transform.position = hotSpot.transform.position; steam.Play(); }
                if (Random.value < Time.deltaTime * 4f) Haptics.Light();
                if (progress >= MeltTime) { StartCoroutine(Melted()); return; }
            }
            else
            {
                if (steam != null) steam.Stop();
                if (progress > 0.3f)
                {
                    offTime += Time.deltaTime;
                    if (offTime > LongBreak) { StartCoroutine(Refreeze()); return; }
                }
            }
            if (meltGauge != null) meltGauge.localScale = new Vector3(Mathf.Max(0.01f, progress / MeltTime), 1f, 1f);
            if (iceArt != null) iceArt.color = Color.Lerp(Color.white, new Color(0.75f, 0.9f, 1f, 0.75f), progress / MeltTime);
        }

        IEnumerator Refreeze()
        {
            busy = true;
            grab = 0;
            tries++;
            UpdateTries();
            progress = 0f;
            offTime = 0f;
            if (steam != null) steam.Stop();
            if (meltGauge != null) meltGauge.localScale = new Vector3(0.01f, 1f, 1f);
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Medium();
            Say(tries >= MaxTries ? "Frozen solid. Forever." : "Nooo! It froze again!", 2.2f);
            if (iceArt != null) iceArt.color = new Color(0.7f, 0.85f, 1f);
            yield return Shake(iceArt != null ? iceArt.transform : lens, 0.06f, 0.4f);
            if (iceArt != null) iceArt.color = Color.white;
            if (tries >= MaxTries)
            {
                yield return new WaitForSeconds(0.8f);
                Fail();
                yield break;
            }
            busy = false;
        }

        IEnumerator Melted()
        {
            busy = true;
            grab = 0;
            Sfx.Play(this, "whoosh", 0.9f);
            Haptics.Medium();
            beam.enabled = false;
            sunlight.enabled = false;
            Alpha(hotSpot, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                Alpha(iceArt, 1f - k);
                Alpha(waterArt, k);
                yield return null;
            }
            if (steam != null) steam.Stop();
            Alpha(iceArt, 0f);
            // la chiave cade nell'acqua bassa e il topo la prende
            Sfx.Play(this, "pop", 0.7f);
            yield return Move(key, new Vector3(key.localPosition.x - 0.6f, groundY + 0.2f, 0f), 0.5f, 0.4f);
            Haptics.Light();
            Say("My key! Thanks, sunshine!", 2.2f);
            yield return Walk(mouse, new Vector3(key.localPosition.x - 0.7f, mouse.localPosition.y, 0f), 2.2f, 0.06f);
            key.SetParent(mouse, true);
            yield return Move(key, new Vector3(0.2f, 1.0f, 0f), 0.25f);
            yield return Move(mouse, mouse.localPosition, 0.35f, 0.45f);
            yield return Celebrate();
        }
    }
}
