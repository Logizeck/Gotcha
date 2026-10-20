using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Adpocalypse" — un topo è intrappolato dietro un'enorme pubblicità FINTA e la X di chiusura scappa.
    /// Soluzione: trascinare tutto il banner oltre il bordo dello schermo. Cinque tocchi falliti sulla X fanno solo
    /// tornare il banner al suo posto. Easter egg "Adpocalypse": toccare la X esattamente sette volte entro 15 secondi
    /// prima di spostare il banner: arrivano pubblicità assurde, un dirigente minuscolo urla, le X scioperano e se lo
    /// portano via, una X gigante cancella tutto. Tutte le pubblicità sono oggetti interni finti, nessun link.
    /// </summary>
    public class AdpocalypseLevel : S41ILevelBase
    {
        [SerializeField] Transform banner, closeX, mouse, cheese, executive, giantX;
        [SerializeField] Collider2D bannerArea;
        [SerializeField] Transform[] ads, adXs, strikers;
        [SerializeField] TextMesh pressText;

        const float EggWindow = 15f;
        static readonly Vector2[] corners = { new Vector2(2.05f, 1.5f), new Vector2(-2.05f, 1.5f), new Vector2(-2.05f, -1.5f), new Vector2(2.05f, -1.5f), new Vector2(0.9f, 1.5f), new Vector2(-0.9f, -1.5f) };

        Vector3 bannerHome;
        Vector2 grabOffset;
        bool dragging, bannerMoved;
        int corner, presses, eggPresses;
        float eggStart;

        protected override void Begin()
        {
            bannerHome = banner.localPosition;
            foreach (Transform t in ads) t.gameObject.SetActive(false);
            foreach (Transform t in strikers) t.gameObject.SetActive(false);
            executive.gameObject.SetActive(false);
            giantX.gameObject.SetActive(false);
            pressText.text = "";
            closeX.localPosition = corners[0];
            Say("Squeak! Get this ad off me!", 2.4f);
        }

        protected override void Tick()
        {
            if (busy) return;
            closeX.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 5f) * 8f);
            bool has = Pointer(out Vector2 w, out bool began, out bool ended);
            if (!has) { if (dragging) Release(); return; }
            if (began)
            {
                if (Vector2.Distance(w, closeX.position) < 0.6f) { PressX(); return; }
                if (bannerArea.OverlapPoint(w)) { dragging = true; grabOffset = (Vector2)banner.position - w; Sfx.Play(this, "tick", 0.5f); }
            }
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                banner.position = new Vector3(p.x, p.y, banner.position.z);
                if (Vector2.Distance(banner.localPosition, bannerHome) > 0.3f) bannerMoved = true;
                if (OffScreen()) { dragging = false; StartCoroutine(Freed()); return; }
            }
            if (ended && dragging) Release();
        }

        bool OffScreen()
        {
            Vector3 p = banner.localPosition;
            return Mathf.Abs(p.x) > 2.5f || p.y < -3.6f || p.y > 3.2f;
        }

        void Release()
        {
            dragging = false;
            if (OffScreen()) StartCoroutine(Freed());
        }

        void PressX()
        {
            presses++;
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            // conteggio per l'Easter egg: sette tocchi entro 15 s, banner mai spostato
            if (!bannerMoved)
            {
                if (eggPresses == 0 || clock - eggStart > EggWindow) { eggPresses = 0; eggStart = clock; }
                eggPresses++;
                if (eggPresses >= 7) { StartCoroutine(Adpocalypse()); return; }
            }
            int next;
            do next = Random.Range(0, corners.Length); while (next == corner);
            corner = next;
            closeX.localPosition = corners[corner];
            StartCoroutine(Pop(closeX, 1.3f, 0.2f));
            string[] lines = { "Nope!", "Missed me!", "Too slow!", "Watch a 30s video first!", "Ha!" };
            pressText.text = "X PRESSES: " + presses;
            Say(lines[(presses - 1) % lines.Length], 1.2f);
            if (presses % 5 == 0) StartCoroutine(Refresh());
        }

        IEnumerator Refresh()
        {
            busy = true;
            dragging = false;
            Say("AD REFRESHED!", 1.4f);
            Sfx.Play(this, "whoosh", 0.7f);
            yield return StartCoroutine(Move(banner, bannerHome, 0.3f));
            yield return StartCoroutine(Pop(banner, 1.06f, 0.25f));
            busy = false;
        }

        IEnumerator Freed()
        {
            busy = true;
            Sfx.Play(this, "whoosh", 0.9f);
            Vector3 dir = banner.localPosition - bannerHome;
            if (dir.sqrMagnitude < 0.01f) dir = Vector3.right;
            dir.Normalize();
            for (float k = 0f; k < 0.5f; k += Time.deltaTime)
            {
                banner.localPosition += dir * 14f * Time.deltaTime;
                yield return null;
            }
            banner.gameObject.SetActive(false);
            yield return StartCoroutine(MouseRuns("Freedom! No ads, just cheese!"));
            yield return StartCoroutine(Celebrate(1.6f));
        }

        IEnumerator MouseRuns(string line)
        {
            Say(line, 0f);
            yield return StartCoroutine(Pop(mouse, 1.2f, 0.25f));
            yield return StartCoroutine(Walk(mouse, cheese.localPosition + new Vector3(-0.6f, 0.15f, 0f), 3f, 0.12f));
            Sfx.Play(this, "pop", 0.7f);
            StartCoroutine(Pop(cheese, 1.3f, 0.3f));
        }

        IEnumerator Adpocalypse()
        {
            busy = true;
            dragging = false;
            pressText.text = "";
            // le pubblicità parodia invadono lo schermo, sempre più in fretta
            float gap = 0.45f;
            foreach (Transform ad in ads)
            {
                ad.gameObject.SetActive(true);
                Sfx.Play(this, "pop", 0.8f);
                Haptics.Light();
                StartCoroutine(Pop(ad, 1.2f, 0.2f));
                yield return new WaitForSeconds(gap);
                gap = Mathf.Max(0.12f, gap * 0.82f);
            }
            yield return new WaitForSeconds(0.4f);
            // il dirigente minuscolo, disperato
            executive.gameObject.SetActive(true);
            Vector3 execAt = executive.localPosition;
            executive.localPosition = execAt + Vector3.left * 3.5f;
            yield return StartCoroutine(Walk(executive, execAt, 4f, 0.1f));
            if (bubble != null) bubble.transform.localPosition = execAt + new Vector3(0.6f, 1.9f, 0f);
            Say("STOP CLICKING! WE CAN'T AFFORD THIS!", 0f);
            Sfx.Play(this, "buzz", 0.9f);
            Haptics.Medium();
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                executive.localPosition = execAt + new Vector3(Mathf.Sin(k * 40f) * 0.04f, Mathf.Abs(Mathf.Sin(k * 9f)) * 0.12f, 0f);
                yield return null;
            }
            executive.localPosition = execAt;
            // le X prendono vita e scioperano
            for (int i = 0; i < strikers.Length; i++)
            {
                if (i < adXs.Length && adXs[i] != null) adXs[i].gameObject.SetActive(false);
                Transform s = strikers[i];
                Vector3 dest = s.localPosition;
                s.localPosition = i < adXs.Length && adXs[i] != null ? s.parent.InverseTransformPoint(adXs[i].position) : dest + Vector3.up * 3f;
                s.gameObject.SetActive(true);
                StartCoroutine(Move(s, dest, 0.45f, 0.8f));
                Sfx.Play(this, "pop", 0.7f);
                yield return new WaitForSeconds(0.15f);
            }
            yield return new WaitForSeconds(0.5f);
            Say("STRIKE! STRIKE! WE WON'T BE CLOSED!", 0f);
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                for (int i = 0; i < strikers.Length; i++)
                    strikers[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 10f + i) * 12f);
                yield return null;
            }
            // trascinano via il dirigente
            if (bubble != null) bubble.Hide();
            executive.localRotation = Quaternion.Euler(0, 0, 70f);
            Sfx.Play(this, "whoosh", 0.8f);
            for (float k = 0f; k < 1.4f; k += Time.deltaTime)
            {
                Vector3 d = Vector3.right * 5f * Time.deltaTime;
                executive.localPosition += d;
                foreach (Transform s in strikers) { s.localPosition += d; s.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 20f) * 10f); }
                yield return null;
            }
            executive.gameObject.SetActive(false);
            foreach (Transform s in strikers) s.gameObject.SetActive(false);
            // una X gigante spazza via tutto
            giantX.gameObject.SetActive(true);
            Vector3 big = giantX.localScale;
            Sfx.Play(this, "whoosh", 1f);
            Haptics.Heavy();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                giantX.localScale = big * Mathf.SmoothStep(0f, 1f, k);
                giantX.localRotation = Quaternion.Euler(0, 0, 360f * (1f - k));
                yield return null;
            }
            Sfx.Play(this, "bonk", 1f);
            foreach (Transform ad in ads) ad.gameObject.SetActive(false);
            banner.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { giantX.localScale = big * (1f - k); yield return null; }
            giantX.gameObject.SetActive(false);
            if (bubble != null) bubble.transform.localPosition = mouse.localPosition + new Vector3(0.9f, 1.4f, 0f);
            yield return StartCoroutine(MouseRuns("Ad-free at last!"));
            yield return StartCoroutine(Celebrate(1.8f, "adpocalypse"));
        }
    }
}
