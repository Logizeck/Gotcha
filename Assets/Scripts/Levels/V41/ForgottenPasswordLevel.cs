using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The forgotten password" — un computer chiede una password di 4 caratteri; un post-it attaccato al
    /// monitor dice "I FORGOT IT". Il tastierino sulla scrivania è una trappola: tre password sbagliate fanno scattare
    /// il blocco di sicurezza (sconfitta). Trucco: si trascina il post-it nel campo della password: "I FORGOT IT" è
    /// proprio la password.
    /// </summary>
    public class ForgottenPasswordLevel : LevelController
    {
        [SerializeField] Transform note;
        [SerializeField] Collider2D passwordBox;
        [SerializeField] TextMesh field;
        [SerializeField] TextMesh screenMsg;
        [SerializeField] SpriteRenderer screenTint;
        [SerializeField] Collider2D[] keys;          // 0..9, 10 = DEL, 11 = OK
        [SerializeField] TextMesh goal;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;

        Camera cam;
        Vector3 noteHome;
        Quaternion noteRot;
        string typed = "";
        int wrong;
        bool busy, dragging, moved;
        Vector2 grabStart, grabOffset;
        float bubbleUntil;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            noteHome = note.position;
            noteRot = note.rotation;
            if (screenTint != null) screenTint.color = Color.clear;
            ShowTyped();
            Msg("ENTER PASSWORD (4 CHARACTERS)", Color.white);
            Kit41C.Hearts(hearts, 3);
            Kit41C.Goal(goal, "LOG IN");
            Say("What was my password again?", 2.2f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void Msg(string text, Color c)
        {
            if (screenMsg == null) return;
            screenMsg.text = text;
            screenMsg.color = c;
        }

        void ShowTyped()
        {
            if (field == null) return;
            string s = "";
            for (int i = 0; i < 4; i++) s += (i > 0 ? " " : "") + (i < typed.Length ? typed[i].ToString() : "_");
            field.text = s;
        }

        void Update()
        {
            if (IsFinished) return;
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) { dragging = false; return; }
            if (!Kit41C.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { if (dragging) DropNote(); return; }
            if (began)
            {
                Vector2 l = note.InverseTransformPoint(w);
                if (Mathf.Abs(l.x) < 1.05f && Mathf.Abs(l.y) < 0.75f)
                {
                    dragging = true;
                    moved = false;
                    grabStart = w;
                    grabOffset = (Vector2)note.position - w;
                    Sfx.Play(this, "tick", 0.4f);
                    return;
                }
                for (int i = 0; i < keys.Length; i++)
                    if (keys[i] != null && keys[i].OverlapPoint(w)) { Press(i); return; }
                return;
            }
            if (!dragging) return;
            if ((w - grabStart).magnitude > 0.15f) moved = true;
            Vector2 p = w + grabOffset;
            note.position = new Vector3(Mathf.Clamp(p.x, -2.3f, 2.3f), Mathf.Clamp(p.y, -4.2f, 2.3f), 0f);
            note.rotation = Quaternion.Slerp(note.rotation, Quaternion.identity, Time.deltaTime * 8f);
            if (ended) DropNote();
        }

        void DropNote()
        {
            dragging = false;
            if (moved && passwordBox != null && passwordBox.OverlapPoint(note.position)) { StartCoroutine(Granted()); return; }
            if (!moved) Say("\"I FORGOT IT\". Very helpful.", 1.8f);
            StartCoroutine(Home());
        }

        IEnumerator Home()
        {
            busy = true;
            Vector3 from = note.position;
            Quaternion r = note.rotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                note.position = Vector3.Lerp(from, noteHome, Mathf.SmoothStep(0f, 1f, k));
                note.rotation = Quaternion.Slerp(r, noteRot, k);
                yield return null;
            }
            note.position = noteHome;
            note.rotation = noteRot;
            busy = false;
        }

        void Press(int i)
        {
            if (keys[i] != null) StartCoroutine(Kit41C.Pop(keys[i].transform, -0.12f, 0.15f));
            Sfx.Play(this, "tick", 0.6f);
            Haptics.Light();
            if (i == 10) { if (typed.Length > 0) typed = typed.Substring(0, typed.Length - 1); ShowTyped(); return; }
            if (i == 11)
            {
                if (typed.Length < 4) { Msg("4 CHARACTERS, PLEASE", new Color(1f, 0.9f, 0.4f)); return; }
                StartCoroutine(Wrong());
                return;
            }
            if (typed.Length >= 4) return;
            typed += i.ToString();
            ShowTyped();
        }

        IEnumerator Wrong()
        {
            busy = true;
            wrong++;
            Kit41C.Hearts(hearts, 3 - wrong);
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Medium();
            Msg("WRONG PASSWORD!", new Color(1f, 0.45f, 0.45f));
            if (field != null) yield return Kit41C.Shake(field.transform, 0.15f);
            typed = "";
            ShowTyped();
            if (wrong >= 3)
            {
                Kit41C.Goal(goal, "SECURITY LOCK!");
                Msg("SECURITY LOCK!  INTRUDER ALERT!", Color.white);
                for (int b = 0; b < 6; b++)
                {
                    if (screenTint != null) screenTint.color = b % 2 == 0 ? new Color(1f, 0.15f, 0.15f, 0.75f) : new Color(0.4f, 0f, 0f, 0.5f);
                    Sfx.Play(this, "buzz", 0.5f);
                    yield return new WaitForSeconds(0.22f);
                }
                Say("Locked out of my own computer!", 2f);
                yield return new WaitForSeconds(0.8f);
                Fail();
                yield break;
            }
            Say(wrong == 1 ? "Nope. I really forgot it." : "One more try before the lock!", 1.8f);
            yield return new WaitForSeconds(0.5f);
            Msg("ENTER PASSWORD (4 CHARACTERS)", Color.white);
            busy = false;
        }

        IEnumerator Granted()
        {
            busy = true;
            // il post-it entra nel campo e diventa la password
            Vector3 from = note.position, to = passwordBox.bounds.center;
            Vector3 s0 = note.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                note.position = Vector3.Lerp(from, to, k);
                note.localScale = s0 * (1f - k * 0.9f);
                yield return null;
            }
            note.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.7f);
            if (field != null) field.text = "I FORGOT IT";
            yield return new WaitForSeconds(0.5f);
            Msg("ACCESS GRANTED!", new Color(0.55f, 1f, 0.55f));
            if (screenTint != null) screenTint.color = new Color(0.3f, 0.9f, 0.4f, 0.35f);
            Kit41C.Goal(goal, "LOGGED IN!");
            Sfx.Play(this, "ding", 0.8f);
            Say("Ha! THAT was my password!", 2f);
            if (field != null) yield return Kit41C.Pop(field.transform, 0.3f, 0.35f);
            yield return new WaitForSeconds(1.2f);
            Win();
        }
    }
}
