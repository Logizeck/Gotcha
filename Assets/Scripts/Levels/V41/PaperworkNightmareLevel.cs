using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Paperwork nightmare" — un impiegato deve trovare il contratto in una montagna di fogli (40 secondi).
    /// Toccare i fogli non serve ("not it"); si trascinano e quelli lasciati sul cestino "decorativo" a fiori spariscono.
    /// Trucco: il contratto non è un foglio, è stampato sul piano della scrivania sotto la pila: si sgombera la
    /// scrivania e si tocca il contratto stampato.
    /// </summary>
    public class PaperworkNightmareLevel : LevelController
    {
        [SerializeField] Transform[] sheets;
        [SerializeField] SpriteRenderer[] sheetArts;
        [SerializeField] Transform binMouth;
        [SerializeField] Transform bin;
        [SerializeField] Collider2D docArea;          // il contratto stampato sulla scrivania
        [SerializeField] SpriteRenderer docGlow;
        [SerializeField] TextMesh timerText;
        [SerializeField] TextMesh goal;
        [SerializeField] SpeechBubble bubble;

        const float TimeLimit = 40f;

        Camera cam;
        bool[] gone;
        int grabbed = -1, topOrder = 40;
        bool moved, busy;
        Vector2 grabStart, grabOffset;
        float left = TimeLimit, bubbleUntil;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            gone = new bool[sheets.Length];
            if (docGlow != null) docGlow.enabled = false;
            Kit41C.Goal(goal, "FIND THE CONTRACT");
            Say("The contract is somewhere in this mess!", 2.4f);
            ShowTime();
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void ShowTime()
        {
            if (timerText == null) return;
            int s = Mathf.CeilToInt(Mathf.Max(0f, left));
            timerText.text = "0:" + s.ToString("00");
            timerText.color = s <= 10 ? new Color(1f, 0.35f, 0.35f) : Color.white;
        }

        void Update()
        {
            if (IsFinished) return;
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) { grabbed = -1; return; }
            left -= Time.deltaTime;
            ShowTime();
            if (left <= 0f) { StartCoroutine(TimeUp()); return; }
            if (!Kit41C.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { if (grabbed >= 0) Drop(); return; }
            if (began) { Grab(w); return; }
            if (grabbed < 0) return;
            if ((w - grabStart).magnitude > 0.15f) moved = true;
            Vector2 p = w + grabOffset;
            sheets[grabbed].position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -4.4f, 2.3f), 0f);
            if (ended) Drop();
        }

        void Grab(Vector2 w)
        {
            grabbed = -1;
            moved = false;
            grabStart = w;
            int best = int.MinValue;
            for (int i = 0; i < sheets.Length; i++)
            {
                if (gone[i]) continue;
                Vector2 l = sheets[i].InverseTransformPoint(w);
                if (Mathf.Abs(l.x) > 0.5f || Mathf.Abs(l.y) > 0.62f) continue;
                if (sheetArts[i].sortingOrder > best) { best = sheetArts[i].sortingOrder; grabbed = i; }
            }
            if (grabbed >= 0)
            {
                sheetArts[grabbed].sortingOrder = ++topOrder;
                grabOffset = (Vector2)sheets[grabbed].position - w;
                return;
            }
            if (docArea != null && docArea.OverlapPoint(w)) TapDesk();
        }

        void Drop()
        {
            int i = grabbed;
            grabbed = -1;
            if (i < 0) return;
            if (!moved)
            {
                string[] nope = { "Not it.", "Nope, lunch menu.", "That's a doodle.", "Old memo. Not it." };
                Say(nope[i % nope.Length], 1.2f);
                StartCoroutine(Kit41C.Pop(sheets[i], 0.1f));
                Sfx.Play(this, "tick", 0.4f);
                return;
            }
            if (Vector2.Distance(sheets[i].position, binMouth.position) < 0.85f) StartCoroutine(Toss(i));
            else Sfx.Play(this, "whoosh", 0.25f);
        }

        IEnumerator Toss(int i)
        {
            gone[i] = true;
            Transform s = sheets[i];
            Vector3 from = s.position, scale = s.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)
            {
                s.position = Vector3.Lerp(from, binMouth.position, k);
                s.localScale = scale * (1f - k * 0.8f);
                yield return null;
            }
            s.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.5f);
            if (bin != null) StartCoroutine(Kit41C.Pop(bin, 0.12f, 0.2f));
            if (!Covered()) Say("Hey... something's printed on my desk!", 2f);
        }

        bool Covered()
        {
            if (docArea == null) return false;
            Bounds d = docArea.bounds;
            d.Expand(-0.15f);
            for (int i = 0; i < sheets.Length; i++)
                if (!gone[i] && sheetArts[i].bounds.Intersects(d)) return true;
            return false;
        }

        void TapDesk()
        {
            if (Covered()) { Say("There's still paper on it.", 1.4f); return; }
            StartCoroutine(Found());
        }

        IEnumerator Found()
        {
            busy = true;
            if (timerText != null) timerText.text = "";
            if (docGlow != null) docGlow.enabled = true;
            Kit41C.Goal(goal, "FOUND IT!");
            Sfx.Play(this, "ding", 0.8f);
            Say("It was ON the desk all along!", 2.2f);
            for (int k = 0; k < 3; k++)
            {
                if (docGlow != null) yield return Kit41C.Pop(docGlow.transform, 0.15f, 0.3f);
                else yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(0.8f);
            Win();
        }

        IEnumerator TimeUp()
        {
            busy = true;
            Kit41C.Goal(goal, "TIME'S UP!");
            ShowTime();
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Medium();
            Say("The boss is coming! I'M FIRED!", 2f);
            yield return new WaitForSeconds(1.5f);
            Fail();
        }
    }
}
