using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 50 — "TUG OF WAR". Le chiavi di casa sono sparite e Toni ha un calzino che gli spunta dalla bocca,
    /// con l'aria più innocente del mondo. Tira il calzino... ed escono un sacco di cose legate insieme, una dopo
    /// l'altra, fino alla chiave. Ma va tirato PIANO: uno strattone e Toni lo prende per un gioco, tira anche lui e
    /// si riprende tutto il pezzo. Troppi strattoni... vince lui, si rimangia tutto e il livello è perso.
    /// </summary>
    public class ToniTugLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Transform[] items;          // in ordine; l'ultimo è la chiave
        [SerializeField] LineRenderer rope;
        [SerializeField] Vector2 pileCenter = new Vector2(-1.3f, -3.6f);
        [SerializeField] float pullLength = 1.7f;
        [SerializeField] float yankSpeed = 7f;
        [SerializeField] int yanksForEgg = 5;
        [SerializeField] string[] lines =
        {
            "...it's not what it looks like.", "I was hungry.", "Mine.", "Savings.", "Ding?", "Snack for later.", "...", "Oh. THAT key.",
        };

        Camera cam;
        int index, yanks;
        bool grabbing, busy;
        Vector2 lastHand, vel;
        Vector3[] itemScale;

        Vector3 HangPoint => toni.MouthWorld + new Vector3(-0.12f, -0.45f, 0f);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            toni.Stand(false);
            toni.WakeUp(1);   // occhi a mezz'asta: l'aria colpevole
            toni.Wag = 0.2f;
            itemScale = new Vector3[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                itemScale[i] = items[i].localScale;
                items[i].gameObject.SetActive(i == 0);
            }
            Hang();
        }

        Transform Current => index < items.Length ? items[index] : null;

        void Hang()
        {
            if (Current == null) return;
            Vector3 h = HangPoint;
            Current.position = new Vector3(h.x, h.y, Current.position.z);
            Current.localRotation = Quaternion.identity;
            toni.LookTarget = Current;
        }

        void Update()
        {
            if (IsFinished || busy || Current == null || cam == null) { DrawRope(); return; }
            bool has = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended);
            Vector2 w = has ? (Vector2)PointerInput.ToWorld(cam, sp) : Vector2.zero;
            if (has && began && !PointerInput.IsOverUi() && Vector2.Distance(w, Current.position) < 0.7f)
            {
                grabbing = true;
                lastHand = w;
                vel = Vector2.zero;
            }
            if (grabbing && has)
            {
                if (Time.deltaTime > 0f) vel = Vector2.Lerp(vel, (w - lastHand) / Time.deltaTime, 0.35f);
                lastHand = w;
                Current.position = new Vector3(w.x, w.y, Current.position.z);
                // Toni tiene duro: si piega verso la mano
                Vector2 pull = w - (Vector2)toni.MouthWorld;
                toni.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Clamp(-pull.x * 4f, -10f, 10f));
                if (vel.magnitude > yankSpeed) { StartCoroutine(Yank()); return; }
                if (pull.magnitude > pullLength) { StartCoroutine(PopOut()); return; }
            }
            if (grabbing && (!has || ended))
            {
                grabbing = false;
                toni.transform.localRotation = Quaternion.identity;
                StartCoroutine(SpringBack());
            }
            DrawRope();
        }

        void DrawRope()
        {
            if (rope == null) return;
            rope.enabled = Current != null && Current.gameObject.activeSelf;
            if (!rope.enabled) return;
            Vector3 a = toni.MouthWorld, b = Current.position;
            rope.positionCount = 2;
            rope.SetPosition(0, new Vector3(a.x, a.y, 0f));
            rope.SetPosition(1, new Vector3(b.x, b.y, 0f));
        }

        IEnumerator SpringBack()
        {
            busy = true;
            Vector3 from = Current.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)
            {
                Current.position = Vector3.Lerp(from, HangPoint, k);
                DrawRope();
                yield return null;
            }
            Hang();
            busy = false;
        }

        IEnumerator Yank()
        {
            busy = true;
            grabbing = false;
            yanks++;
            Haptics.Medium();
            toni.Eyes(2);
            toni.Say(yanks >= yanksForEgg ? "MY TURN!" : "GRRR! *tug*", 1f, "woof");
            // strattone di Toni: si tira indietro e il pezzo torna in bocca
            Vector3 home = toni.transform.localPosition;
            Vector3 from = Current.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                toni.transform.localPosition = home + new Vector3(0.25f * Mathf.Sin(k * Mathf.PI), 0f, 0f);
                toni.transform.localRotation = Quaternion.Euler(0, 0, -12f * Mathf.Sin(k * Mathf.PI));
                Current.position = Vector3.Lerp(from, HangPoint, k * k);
                DrawRope();
                yield return null;
            }
            toni.transform.localPosition = home;
            toni.transform.localRotation = Quaternion.identity;
            Hang();
            toni.Eyes(1);
            if (yanks >= yanksForEgg) { yield return SlurpEverything(); yield break; }
            busy = false;
        }

        IEnumerator PopOut()
        {
            busy = true;
            grabbing = false;
            toni.transform.localRotation = Quaternion.identity;
            Transform it = Current;
            Sfx.Play(this, "pop", 0.9f);
            Haptics.Light();
            // vola sul mucchio
            Vector3 from = it.position;
            Vector3 to = new Vector3(pileCenter.x + Random.Range(-0.5f, 0.5f), pileCenter.y + index * 0.12f, from.z);
            float spin = Random.Range(-540f, 540f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                it.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                it.Rotate(0, 0, spin * Time.deltaTime);
                yield return null;
            }
            it.position = to;
            Sfx.Play(this, "tick", 0.6f);
            bool key = index == items.Length - 1;
            toni.Say(lines[Mathf.Min(index, lines.Length - 1)], 1.6f);
            index++;
            if (key)
            {
                rope.enabled = false;
                Sfx.Play(this, "ding", 0.8f);
                yield return new WaitForSeconds(1.2f);
                toni.Eyes(2);
                toni.Stand(true);
                yield return toni.Happy(1f);
                toni.Bark("WALK? WALK!!");
                yield return new WaitForSeconds(1.2f);
                Win();
                yield break;
            }
            items[index].gameObject.SetActive(true);
            Hang();
            busy = false;
        }

        /// <summary>Troppi strattoni: Toni vince la gara di tiro alla fune... e si rimangia il mucchio, chiave compresa.</summary>
        IEnumerator SlurpEverything()
        {
            toni.MouthOpen(true);
            toni.Say("SLUUUURP!", 1.5f, "whoosh");
            for (int i = index - 1; i >= 0; i--)
            {
                Transform it = items[i];
                Vector3 from = it.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
                {
                    it.position = Vector3.Lerp(from, toni.MouthWorld, k);
                    it.localScale = itemScale[i] * (1f - k * 0.8f);
                    yield return null;
                }
                it.gameObject.SetActive(false);
                Sfx.Play(this, "pop", 0.5f);
            }
            if (Current != null) Current.gameObject.SetActive(false);
            if (rope != null) rope.enabled = false;
            toni.MouthOpen(false);
            yield return new WaitForSeconds(0.4f);
            toni.Say("*BURP*", 1.5f, "bonk");
            yield return new WaitForSeconds(1.6f);
            Fail();
        }
    }
}
