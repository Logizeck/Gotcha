using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Almost there" — il contatore dice 99/100 e non c'è niente da raccogliere. Il trucco: trascinare giù la
    /// barra "/" tra 99 e 100: sotto c'è un puntino nascosto; toccarlo dà l'ultimo punto. Tre tocchi sul contatore
    /// (usato come pulsante) = sconfitta; ogni volta il punteggio scende per un attimo a 98.
    /// </summary>
    public class AlmostThereLevel : LevelController
    {
        [SerializeField] TextMesh scoreText, maxText, strikeText;
        [SerializeField] Collider2D scoreArea, maxArea, slashArea, dotArea;
        [SerializeField] Transform slash, dot, board;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int strikesAllowed = 3;

        Camera cam;
        bool busy, dragging;
        int strikes;
        Vector3 slashHome;
        Vector2 dragOffset;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            slashHome = slash.position;
            S41LKit.Confetti(confetti, false);
            UpdateStrikes();
            S41LKit.Say(bubble, "Just ONE more point!");
        }

        void UpdateStrikes() { strikeText.text = "PENALTIES " + strikes + "/" + strikesAllowed; }

        bool SlashMoved => Vector2.Distance(slash.position, slashHome) > 0.5f;

        void Update()
        {
            if (IsFinished || busy) return;
            // il puntino luccica (si vede solo se la barra si è spostata)
            dot.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 5f) * 0.12f);
            bool down = S41LKit.Pointer(cam, out Vector2 w, out bool began, out bool ended);
            if (began)
            {
                if (slashArea.OverlapPoint(w)) { dragging = true; dragOffset = (Vector2)slash.position - w; Sfx.Play(this, "tick", 0.5f); return; }
                if (SlashMoved && dotArea.OverlapPoint(w)) { StartCoroutine(Collect()); return; }
                if (scoreArea.OverlapPoint(w) || maxArea.OverlapPoint(w)) { StartCoroutine(Penalty(scoreArea.OverlapPoint(w) ? scoreText.transform : maxText.transform)); return; }
            }
            if (dragging)
            {
                Vector2 p = w + dragOffset;
                slash.position = new Vector3(Mathf.Clamp(p.x, -2.3f, 2.3f), Mathf.Clamp(p.y, -4.0f, 2.8f), slash.position.z);
                if (ended || !down)
                {
                    dragging = false;
                    Sfx.Play(this, "clunk", 0.4f);
                    if (SlashMoved) S41LKit.Say(bubble, "Hey... what's that little thing?");
                }
            }
        }

        IEnumerator Penalty(Transform what)
        {
            busy = true;
            strikes++;
            UpdateStrikes();
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Medium();
            scoreText.text = "98";
            scoreText.color = new Color(1f, 0.35f, 0.35f);
            S41LKit.Say(bubble, strikes >= strikesAllowed ? "GAME OVER, button masher!" : "It's a counter, not a button! -1");
            yield return StartCoroutine(S41LKit.Shake(what, 0.15f, 0.4f));
            yield return new WaitForSeconds(0.8f);
            if (strikes >= strikesAllowed) { Fail(); yield break; }
            scoreText.text = "99";
            scoreText.color = Color.white;
            busy = false;
        }

        IEnumerator Collect()
        {
            busy = true;
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Light();
            Vector3 from = dot.position, to = scoreText.transform.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                dot.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                yield return null;
            }
            dot.gameObject.SetActive(false);
            scoreText.text = "100";
            scoreText.color = new Color(1f, 0.9f, 0.3f);
            Sfx.Play(this, "ding", 1f);
            Haptics.Medium();
            // la barra torna al suo posto: 100/100
            Vector3 sf = slash.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                slash.position = Vector3.Lerp(sf, slashHome, Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            StartCoroutine(S41LKit.Pop(board, 0.12f, 0.4f));
            S41LKit.Say(bubble, "100/100! PERFECT SCORE!");
            S41LKit.Confetti(confetti, true);
            for (float t = 0f; t < 2f; t += Time.deltaTime)
            {
                Color c = Color.HSVToRGB(Mathf.Repeat(t * 0.8f, 1f), 0.6f, 1f);
                scoreText.color = c;
                maxText.color = c;
                yield return null;
            }
            S41LKit.Confetti(confetti, false);
            Win();
        }
    }
}
