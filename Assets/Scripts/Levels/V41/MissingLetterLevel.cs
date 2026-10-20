using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The missing letter" — il cartello dice "OPEN THE G_TE" e il cancello resta chiuso. Le lettere sono
    /// nascoste nel paesaggio come decorazioni (una A nella nuvola, una O nel tronco, una E nella staccionata, una U
    /// appesa al pilastro): trascina la A nel buco del cartello. Tre lettere sbagliate = sconfitta.
    /// </summary>
    public class MissingLetterLevel : LevelController
    {
        [SerializeField] Transform[] letters;
        [SerializeField] TextMesh[] letterTexts;
        [SerializeField] int rightLetter;
        [SerializeField] Transform slot, gateL, gateR, mouse, sign;
        [SerializeField] SpriteRenderer slotMark;
        [SerializeField] TextMesh triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] Color inkColor = new Color(0.17f, 0.13f, 0.2f);

        Camera cam;
        Vector3[] homePos, homeScale;
        Quaternion[] homeRot;
        Color[] homeColor;
        int held = -1, wrong;
        Vector2 grabOffset;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            int n = letters.Length;
            homePos = new Vector3[n]; homeScale = new Vector3[n]; homeRot = new Quaternion[n]; homeColor = new Color[n];
            for (int i = 0; i < n; i++)
            {
                homePos[i] = letters[i].localPosition;
                homeScale[i] = letters[i].localScale;
                homeRot[i] = letters[i].localRotation;
                homeColor[i] = letterTexts[i].color;
            }
            UpdateTries();
            S41GKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("Open the... G-blank-TE? It won't open!");
        }

        void UpdateTries() { triesText.text = "WRONG LETTERS: " + wrong + "/3"; }

        void Update()
        {
            if (IsFinished || busy) return;
            slotMark.color = new Color(1f, 1f, 1f, 0.5f + 0.3f * Mathf.Sin(Time.time * 4f));
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (held >= 0) Drop(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began && held < 0)
            {
                for (int i = 0; i < letters.Length; i++)
                {
                    Collider2D c = letters[i].GetComponent<Collider2D>();
                    if (c != null && c.OverlapPoint(w))
                    {
                        held = i;
                        grabOffset = (Vector2)letters[i].position - w;
                        letterTexts[i].color = inkColor;
                        letters[i].localRotation = Quaternion.identity;
                        letters[i].localScale = Vector3.one;
                        Sfx.Play(this, "pop", 0.6f);
                        if (i == rightLetter && bubble != null) bubble.Show("Hey, that cloud had an A in it!");
                        break;
                    }
                }
            }
            if (held >= 0)
            {
                Vector2 p = w + grabOffset;
                letters[held].position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.6f), Mathf.Clamp(p.y, -4.4f, 2.9f), 0f);
                if (ended) Drop();
            }
        }

        void Drop()
        {
            int i = held;
            held = -1;
            if (Vector2.Distance(letters[i].position, slot.position) < 0.6f)
            {
                letters[i].position = slot.position;
                StartCoroutine(i == rightLetter ? Opened(i) : Wrong(i));
            }
            else StartCoroutine(GoHome(i));
        }

        IEnumerator GoHome(int i)
        {
            yield return StartCoroutine(S41GKit.Move(letters[i], homePos[i], 0.3f, 0.3f));
            letters[i].localRotation = homeRot[i];
            letters[i].localScale = homeScale[i];
            letterTexts[i].color = homeColor[i];
        }

        IEnumerator Wrong(int i)
        {
            busy = true;
            wrong++;
            UpdateTries();
            Sfx.Play(this, "clunk", 0.6f);
            string word = "G" + letterTexts[i].text + "TE";
            yield return new WaitForSeconds(0.25f);
            if (bubble != null) bubble.Show("OPEN THE " + word + "? What's a " + word.ToLower() + "?!");
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Medium();
            StartCoroutine(S41GKit.Shake(gateL, 0.06f, 0.4f));
            yield return StartCoroutine(S41GKit.Shake(gateR, 0.06f, 0.4f));
            yield return new WaitForSeconds(0.4f);
            if (wrong >= 3)
            {
                if (bubble != null) bubble.Show("I give up. I'll live on this side forever.");
                yield return new WaitForSeconds(1.5f);
                Fail();
                yield break;
            }
            yield return StartCoroutine(GoHome(i));
            busy = false;
        }

        IEnumerator Opened(int i)
        {
            busy = true;
            slotMark.enabled = false;
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            yield return StartCoroutine(S41GKit.Pop(sign, 1.1f, 0.35f));
            if (bubble != null) bubble.Show("OPEN THE GATE! It's opening!");
            Sfx.Play(this, "whoosh", 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                float s = Mathf.Lerp(1f, 0.15f, k * k * (3f - 2f * k));
                gateL.localScale = new Vector3(s, 1f, 1f);
                gateR.localScale = new Vector3(-s, 1f, 1f);
                yield return null;
            }
            Sfx.Play(this, "clunk", 0.6f);
            // il topolino entra
            Vector3 start = mouse.localPosition, ms = mouse.localScale;
            yield return StartCoroutine(S41GKit.Move(mouse, new Vector3(0f, start.y + 0.3f, 0f), 0.6f, 0.25f));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                mouse.localPosition = new Vector3(0f, start.y + 0.3f + k * 0.9f, 0f);
                mouse.localScale = ms * (1f - 0.5f * k);
                yield return null;
            }
            S41GKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41GKit.Confetti(confetti, false);
            Win();
        }
    }
}
