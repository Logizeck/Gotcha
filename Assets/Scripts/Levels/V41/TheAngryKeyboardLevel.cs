using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The angry keyboard" — bisogna scrivere OPEN, ma la tastiera è arrabbiata: a ogni tasto premuto perde
    /// una lettera (che cade via) e sullo schermo esce solo spazzatura; al quinto tasto premuto esplode in una nuvola
    /// di lettere (sconfitta). Il trucco: non si preme niente, si trascinano le lettere stampate sui tasti (O, P, E, N,
    /// in ordine) direttamente dentro la casella della risposta.
    /// </summary>
    public class TheAngryKeyboardLevel : S41JLevelBase
    {
        [SerializeField] Transform[] keys;            // tasti (radice con collider)
        [SerializeField] TextMesh[] keyLetters;       // lettere stampate sui tasti
        [SerializeField] Transform keyboard;          // tastiera intera (per i tremori)
        [SerializeField] SpriteRenderer faceArt;
        [SerializeField] Sprite defeatedFace;
        [SerializeField] Collider2D answerArea;
        [SerializeField] Transform[] slots;           // quattro caselle della risposta
        [SerializeField] TextMesh screenText;
        [SerializeField] TextMesh pressText;
        [SerializeField] SpriteRenderer screenGlow;
        [SerializeField] SmokeEmitter boomSmoke;

        const string Word = "OPEN";
        const int MaxPresses = 5;
        const float DragStart = 0.22f;

        int presses, filled;
        int pending = -1, dragging = -1;
        Vector2 downAt, grabOffset;
        Vector3[] letterHome;
        bool[] lost;
        string typed = "";

        protected override void Begin()
        {
            letterHome = new Vector3[keyLetters.Length];
            lost = new bool[keyLetters.Length];
            for (int i = 0; i < keyLetters.Length; i++) letterHome[i] = keyLetters[i].transform.localPosition;
            UpdateTexts();
            Say("Type OPEN. If you dare.", 2.5f);
        }

        void UpdateTexts()
        {
            if (pressText != null) pressText.text = "KEY PRESSES " + presses + "/" + MaxPresses;
            if (screenText != null) screenText.text = typed.Length == 0 ? "PASSWORD?" : typed;
        }

        int KeyAt(Vector2 w)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                Collider2D c = keys[i].GetComponent<Collider2D>();
                if (c != null && c.OverlapPoint(w)) return i;
            }
            return -1;
        }

        protected override void Tick()
        {
            if (busy) return;
            if (!Pointer(out Vector2 w, out bool began, out bool ended)) { CancelDrag(); return; }
            if (began)
            {
                pending = KeyAt(w);
                downAt = w;
            }
            if (pending >= 0 && dragging < 0 && (w - downAt).magnitude > DragStart)
            {
                if (!lost[pending])
                {
                    dragging = pending;
                    Transform l = keyLetters[dragging].transform;
                    grabOffset = (Vector2)l.position - w;
                    keyLetters[dragging].GetComponent<MeshRenderer>().sortingOrder = 40;
                    l.localScale = Vector3.one * 1.25f;
                    Sfx.Play(this, "tick", 0.5f);
                }
                pending = -1;
            }
            if (dragging >= 0)
            {
                Transform l = keyLetters[dragging].transform;
                Vector2 p = w + grabOffset;
                l.position = new Vector3(Mathf.Clamp(p.x, -2.7f, 2.7f), Mathf.Clamp(p.y, -4.7f, 2.9f), l.position.z);
            }
            if (ended)
            {
                if (dragging >= 0) Drop(w);
                else if (pending >= 0 && KeyAt(w) == pending) StartCoroutine(Press(pending));
                pending = -1;
            }
        }

        void CancelDrag()
        {
            if (dragging >= 0) ReturnLetter(dragging);
            dragging = -1;
            pending = -1;
        }

        void ReturnLetter(int i)
        {
            keyLetters[i].transform.localPosition = letterHome[i];
            keyLetters[i].transform.localScale = Vector3.one;
            keyLetters[i].GetComponent<MeshRenderer>().sortingOrder = 22;
        }

        void Drop(Vector2 w)
        {
            int i = dragging;
            dragging = -1;
            Vector2 letterPos = keyLetters[i].transform.position;
            bool inBox = answerArea != null && (answerArea.OverlapPoint(w) || answerArea.OverlapPoint(letterPos));
            if (!inBox) { ReturnLetter(i); return; }
            string letter = keyLetters[i].text;
            if (filled < Word.Length && letter == Word[filled].ToString())
            {
                StartCoroutine(Place(i));
                return;
            }
            ReturnLetter(i);
            Sfx.Play(this, "buzz", 0.4f);
            Haptics.Light();
            if (Word.Contains(letter)) Say("Right letter, wrong order! " + Word[filled] + " comes next.", 2.4f);
            else Say("'" + letter + "'? That's not in OPEN.", 2f);
        }

        IEnumerator Place(int i)
        {
            busy = true;
            Transform l = keyLetters[i].transform;
            Transform slot = slots[filled];
            filled++;
            lost[i] = true;   // la lettera ora sta nella risposta
            l.SetParent(slot, true);
            l.localScale = Vector3.one;
            yield return Move(l, Vector3.zero + new Vector3(0f, 0f, -0.01f), 0.18f);
            keyLetters[i].GetComponent<MeshRenderer>().sortingOrder = 32;
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            yield return Pop(slot, 1.25f, 0.2f);
            if (filled == 1) Say("Hey! Those are MY letters!", 2f);
            if (filled < Word.Length) { busy = false; yield break; }

            // vittoria: lo schermo diventa verde, la tastiera si arrende
            Say("Fine. FINE. It's open.", 2.5f);
            if (screenText != null) screenText.text = "ACCESS GRANTED";
            if (screenGlow != null) screenGlow.color = new Color(0.45f, 1f, 0.55f, 0.55f);
            if (faceArt != null && defeatedFace != null) faceArt.sprite = defeatedFace;
            for (int k = 0; k < slots.Length; k++) StartCoroutine(Pop(slots[k], 1.3f, 0.3f));
            yield return Shake(keyboard, 0.05f, 0.4f);
            yield return Celebrate();
        }

        IEnumerator Press(int i)
        {
            busy = true;
            presses++;
            Transform k = keys[i];
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Light();
            Vector3 s = k.localScale;
            k.localScale = new Vector3(s.x * 1.05f, s.y * 0.8f, 1f);
            yield return new WaitForSeconds(0.08f);
            k.localScale = s;
            // sullo schermo esce spazzatura
            const string junk = "#@%&!?*$";
            typed += junk[Random.Range(0, junk.Length)];
            if (typed.Length > 8) typed = typed.Substring(typed.Length - 8);
            UpdateTexts();

            if (presses >= MaxPresses) { yield return Explode(); yield break; }

            // la tastiera perde una lettera (mai una di OPEN ancora da mettere, così resta risolvibile)
            int victim = -1;
            for (int tries = 0; tries < 60 && victim < 0; tries++)
            {
                int r = Random.Range(0, keyLetters.Length);
                if (!lost[r] && !Word.Contains(keyLetters[r].text)) victim = r;
            }
            if (faceArt != null) StartCoroutine(Shake(faceArt.transform, 0.08f, 0.4f));
            StartCoroutine(Shake(keyboard, 0.06f, 0.3f));
            string[] lines = { "STOP POKING ME!", "Ouch! I lost a letter!", "Another one?! Grr!", "ONE MORE AND I BLOW!" };
            Say(lines[Mathf.Min(presses - 1, lines.Length - 1)], 2f);
            if (victim >= 0)
            {
                lost[victim] = true;
                yield return FallAway(keyLetters[victim].transform, Random.Range(-1f, 1f));
            }
            busy = false;
        }

        IEnumerator FallAway(Transform t, float side)
        {
            Vector3 v = new Vector3(side * 1.5f, 2.5f, 0f);
            float spin = Random.Range(-500f, 500f);
            for (float k = 0f; k < 1.0f; k += Time.deltaTime)
            {
                v.y -= 12f * Time.deltaTime;
                t.position += v * Time.deltaTime;
                t.Rotate(0f, 0f, spin * Time.deltaTime);
                yield return null;
            }
            t.gameObject.SetActive(false);
        }

        IEnumerator Explode()
        {
            Say("THAT'S IT!!!", 1.5f);
            if (faceArt != null) faceArt.color = new Color(1f, 0.5f, 0.5f);
            yield return Shake(keyboard, 0.15f, 0.8f);
            Sfx.Play(this, "bonk", 1f);
            Sfx.Play(this, "whoosh", 0.8f);
            Haptics.Heavy();
            if (boomSmoke != null) boomSmoke.Play();
            // nuvola di lettere: tutte le lettere volano via
            for (int i = 0; i < keyLetters.Length; i++)
            {
                if (keyLetters[i] == null || !keyLetters[i].gameObject.activeSelf) continue;
                keyLetters[i].transform.SetParent(transform, true);
                StartCoroutine(Fly(keyLetters[i].transform));
            }
            for (int i = 0; i < keys.Length; i++) keys[i].gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            if (boomSmoke != null) boomSmoke.Stop();
            yield return new WaitForSeconds(1.0f);
            Fail();
        }

        IEnumerator Fly(Transform t)
        {
            Vector3 v = (Vector3)(Random.insideUnitCircle.normalized * Random.Range(3f, 7f)) + Vector3.up * 2f;
            float spin = Random.Range(-720f, 720f);
            t.localScale = Vector3.one * Random.Range(1.2f, 2f);
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                v.y -= 6f * Time.deltaTime;
                t.position += v * Time.deltaTime;
                t.Rotate(0f, 0f, spin * Time.deltaTime);
                yield return null;
            }
        }
    }
}
