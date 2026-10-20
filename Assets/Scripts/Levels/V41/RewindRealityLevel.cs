using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Rewind reality" — il vaso è già rotto: la telecamera di sicurezza mostra il gatto che lo butta giù dal
    /// tavolo. Trascina la timeline indietro a prima della caduta, premi PAUSE e sposta il vaso al sicuro (mensola o
    /// cuscino); poi il nastro riparte e il gatto colpisce l'aria. Lasciar cadere il vaso tre volte = sconfitta.
    /// </summary>
    public class RewindRealityLevel : S41FLevelBase
    {
        [SerializeField] Transform cat;                 // piedi del gatto
        [SerializeField] SpriteRenderer catArt;
        [SerializeField] Sprite catWalk, catSwat;
        [SerializeField] Transform vase;                // base del vaso
        [SerializeField] Collider2D vaseArea;
        [SerializeField] GameObject shards;
        [SerializeField] Transform handle;
        [SerializeField] Collider2D trackArea, playArea;
        [SerializeField] TextMesh playLabel, clockText, fallsText;
        [SerializeField] Transform playButton;
        [SerializeField] Collider2D[] safeZones;
        [SerializeField] Transform[] safeSpots;
        [SerializeField] SpriteRenderer[] safeMarks;
        [SerializeField] float trackLeft = -2.2f, trackRight = 2.2f, seconds = 5f;

        const float SwatT = 0.42f, PushT = 0.55f, CrashT = 0.68f;
        const int MaxFalls = 3;
        float t = 1f;
        bool playing, scrubbing, draggingVase, saved;
        int falls;
        Vector2 vaseOffset;
        Vector3 tableSpot;

        protected override void Begin()
        {
            tableSpot = vase.localPosition;
            Marks(false);
            UpdateUi();
            SetTime(t);
            Say("Security footage: the vase is history. Or is it?", 3f);
        }

        void Marks(bool on)
        {
            if (safeMarks == null) return;
            foreach (SpriteRenderer m in safeMarks) if (m != null) m.gameObject.SetActive(on);
        }

        void UpdateUi()
        {
            if (playLabel != null) playLabel.text = playing ? "PAUSE" : "PLAY";
            if (fallsText != null) fallsText.text = "FALLS " + falls + "/" + MaxFalls;
            if (clockText != null) clockText.text = "00:0" + Mathf.Clamp(Mathf.FloorToInt(t * seconds), 0, 9);
            if (handle != null) handle.localPosition = new Vector3(Mathf.Lerp(trackLeft, trackRight, t), handle.localPosition.y, 0f);
        }

        protected override void Tick()
        {
            if (busy) return;
            if (Pointer(out Vector2 w, out bool began, out bool ended))
            {
                if (began) Press(w);
                if (scrubbing) t = Mathf.Clamp01(Mathf.InverseLerp(trackLeft, trackRight, w.x));
                if (draggingVase) vase.position = new Vector3(w.x + vaseOffset.x, w.y + vaseOffset.y, vase.position.z);
                if (ended) Release(w);
            }
            else if (scrubbing || draggingVase) Release(Vector2.one * 99f);

            float before = t;
            if (playing && !scrubbing && t < 1f) t = Mathf.Min(1f, t + Time.deltaTime / seconds);
            if (!draggingVase) SetTime(t);
            UpdateUi();
            if (!saved && playing && !scrubbing && before < CrashT && t >= CrashT) StartCoroutine(Crash());
            else if (saved && playing && t >= 0.78f) StartCoroutine(Saved());
        }

        void Press(Vector2 w)
        {
            if (playArea != null && playArea.OverlapPoint(w))
            {
                if (!playing && t >= 1f) t = 0f;    // a fine nastro PLAY riparte da capo
                playing = !playing;
                Sfx.Play(this, "tick", 0.7f);
                StartCoroutine(Pop(playButton, 1.12f, 0.15f));
                return;
            }
            if (vaseArea != null && vaseArea.OverlapPoint(w) && !saved)
            {
                if (t >= PushT) { Say("Too late! Rewind first.", 1.8f); return; }
                if (playing) { Say("It's moving! Pause the tape first.", 1.8f); return; }
                draggingVase = true;
                vaseOffset = (Vector2)vase.position - w;
                Marks(true);
                Sfx.Play(this, "pop", 0.5f);
                return;
            }
            if (trackArea != null && trackArea.OverlapPoint(w)) { scrubbing = true; Sfx.Play(this, "whoosh", 0.3f); }
        }

        void Release(Vector2 w)
        {
            if (scrubbing) playing = true;     // lasciato il cursore, il nastro riparte
            scrubbing = false;
            if (!draggingVase) return;
            draggingVase = false;
            Marks(false);
            for (int i = 0; safeZones != null && i < safeZones.Length; i++)
            {
                if (safeZones[i] == null || !safeZones[i].OverlapPoint(w)) continue;
                saved = true;
                vase.localRotation = Quaternion.identity;
                StartCoroutine(Move(vase, safeSpots[i].localPosition, 0.25f, 0.2f));
                Sfx.Play(this, "clunk", 0.5f);
                Haptics.Light();
                StartCoroutine(Resume());
                return;
            }
            Say("That's not a safe place...", 1.5f);
            vase.localPosition = tableSpot;
        }

        IEnumerator Resume()
        {
            busy = true;
            Say("Safe and sound. Roll the tape!", 2f);
            yield return new WaitForSeconds(1.0f);
            playing = true;
            busy = false;
        }

        /// <summary>Ricostruisce la scena registrata all'istante t (0..1).</summary>
        void SetTime(float time)
        {
            cat.localPosition = new Vector3(Mathf.Lerp(-0.6f, 1.15f, Mathf.Clamp01(time / SwatT)), cat.localPosition.y, 0f) + Vector3.up * (time < SwatT ? Mathf.Abs(Mathf.Sin(time * 60f)) * 0.04f : 0f);
            if (catArt != null) catArt.sprite = time > SwatT - 0.02f && time < PushT + 0.06f ? catSwat : catWalk;
            if (saved)
            {
                if (shards != null) shards.SetActive(false);
                return;
            }
            bool broken = time >= CrashT;
            vase.gameObject.SetActive(!broken);
            if (shards != null) shards.SetActive(broken);
            if (time < SwatT + 0.03f) { vase.localPosition = tableSpot; vase.localRotation = Quaternion.identity; }
            else if (time < PushT)
            {
                float k = (time - SwatT - 0.03f) / (PushT - SwatT - 0.03f);
                vase.localPosition = tableSpot + new Vector3(0.55f * k, 0f, 0f);
                vase.localRotation = Quaternion.Euler(0, 0, -15f * k);
            }
            else if (!broken)
            {
                float k = (time - PushT) / (CrashT - PushT);
                vase.localPosition = tableSpot + new Vector3(0.55f + 0.12f * k, -2.35f * k * k, 0f);
                vase.localRotation = Quaternion.Euler(0, 0, -15f - 120f * k);
            }
        }

        IEnumerator Crash()
        {
            busy = true;
            falls++;
            UpdateUi();
            Sfx.Play(this, "clunk", 1f);
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Heavy();
            yield return Shake(transform, 0.1f, 0.35f);
            if (falls >= MaxFalls)
            {
                Say("CRASH! Again! Some things can't be undone...", 2.5f);
                yield return new WaitForSeconds(1.6f);
                Fail();
                yield break;
            }
            Say(falls == 1 ? "Aaand it broke again." : "History repeats itself...", 2f);
            busy = false;
        }

        IEnumerator Saved()
        {
            busy = true;
            playing = false;
            UpdateUi();
            Say("...? Where did the vase go?", 2f);
            if (catArt != null) catArt.sprite = catWalk;
            Vector3 cs = cat.localScale;
            cat.localScale = new Vector3(-cs.x, cs.y, cs.z);
            yield return new WaitForSeconds(0.5f);
            cat.localScale = cs;
            yield return new WaitForSeconds(0.5f);
            yield return Pop(vase, 1.2f, 0.3f);
            yield return Celebrate();
        }
    }
}
