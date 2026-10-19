using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Bomb squad" — bomba a tre moduli con 90 secondi, stile "Keep talking": a sinistra il manuale (3 pagine,
    /// si sfogliano con uno swipe o con le frecce). Modulo 1: fili (4-5, colori a caso) — le regole della pagina 1
    /// dicono quale tagliare. Modulo 2: il pulsante (colore ed etichetta a caso) — la pagina 2 dice se toccarlo o tenerlo
    /// premuto; tenendolo si accende una striscia colorata che dice quando lasciarlo (blu: quando il timer mostra un 4,
    /// giallo: un 5, bianco: un 1). Modulo 3: tastierino a 4 simboli — sono tutti in una sola colonna della pagina 3,
    /// vanno premuti nell'ordine della colonna. Ogni errore è uno strike: 3 strike o tempo scaduto = BOOM. Soluzione:
    /// seguire il manuale; a bomba disinnescata ne esce una bandierina "BANG!" (era un giocattolo).
    /// </summary>
    public class BombSquadLevel : LevelController
    {
        [SerializeField] TextMesh goalText;
        [SerializeField] SpriteRenderer[] dots;           // 3 pallini dei moduli in alto
        [SerializeField] SpriteRenderer[] leds;           // 3 led sui moduli
        [SerializeField] TextMesh timerText;
        [SerializeField] TextMesh strikesText;
        [SerializeField] Transform page;                  // pagina del manuale (si gira)
        [SerializeField] TextMesh pageText;
        [SerializeField] TextMesh pageNumber;
        [SerializeField] Collider2D manualArea;
        [SerializeField] Collider2D prevArrow;
        [SerializeField] Collider2D nextArrow;
        [SerializeField] SpriteRenderer[] manualSymbols;  // 18 simboli della pagina 3 (colonna per colonna)
        [SerializeField] Transform[] wires;               // 5 fili (con Collider2D)
        [SerializeField] SpriteRenderer[] wireArt;
        [SerializeField] Sprite wireSprite;
        [SerializeField] Sprite wireCutSprite;
        [SerializeField] Transform button;
        [SerializeField] SpriteRenderer buttonArt;
        [SerializeField] TextMesh buttonLabel;
        [SerializeField] Collider2D buttonArea;
        [SerializeField] SpriteRenderer strip;
        [SerializeField] Transform[] keys;                // 4 tasti (con Collider2D)
        [SerializeField] SpriteRenderer[] keyArt;
        [SerializeField] SpriteRenderer[] keySymbols;
        [SerializeField] Sprite[] symbolSprites;          // 11 simboli
        [SerializeField] Transform boom;
        [SerializeField] Transform flag;
        [SerializeField] SpriteRenderer flash;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float seconds = 90f;

        // colonne del manuale (indici dei simboli dall'alto in basso): ogni gruppo di 4 sta in una sola colonna
        static readonly int[][] Columns = { new[] { 0, 1, 2, 3, 4, 5 }, new[] { 6, 2, 7, 0, 8, 9 }, new[] { 10, 5, 8, 3, 7, 1 } };
        static readonly string[] WireNames = { "red", "blue", "yellow", "white", "black" };
        static readonly Color[] WireColors =
        {
            new Color(0.9f, 0.27f, 0.24f), new Color(0.24f, 0.52f, 0.95f), new Color(1f, 0.84f, 0.2f), Color.white, new Color(0.25f, 0.24f, 0.28f),
        };
        static readonly string[] ButtonColorNames = { "blue", "red", "yellow", "white" };
        static readonly Color[] ButtonColors = { new Color(0.24f, 0.52f, 0.95f), new Color(0.9f, 0.27f, 0.24f), new Color(1f, 0.84f, 0.2f), Color.white };
        static readonly string[] Labels = { "ABORT", "DETONATE", "HOLD", "PRESS" };
        static readonly Color[] StripColors = { new Color(0.24f, 0.52f, 0.95f), new Color(1f, 0.84f, 0.2f), Color.white };
        static readonly char[] StripDigits = { '4', '5', '1' };

        const string Page1 =
            "WIRES\n(count them left to right,\nuse the FIRST rule that fits)\n\n4 WIRES:\n- No red wire?\n   cut the 2nd.\n- Last wire is white?\n   cut the last.\n- More than 1 blue?\n   cut the last blue.\n- Otherwise\n   cut the last.\n\n5 WIRES:\n- More than 2 red?\n   cut the last red.\n- Last wire is black?\n   cut the 4th.\n- No yellow wire?\n   cut the 2nd.\n- Otherwise\n   cut the 1st.";
        const string Page2 =
            "THE BUTTON\n(FIRST rule that fits)\n\n- Blue + \"ABORT\":\n   HOLD it.\n- Says \"DETONATE\":\n   TAP it.\n- Red + \"HOLD\":\n   TAP it.\n- Otherwise:\n   HOLD it.\n\nHOLDING IT?\nA strip lights up.\nLet go when the timer\nshows this digit\n(any position):\n\nBLUE strip: 4\nYELLOW strip: 5\nWHITE strip: 1";
        const string Page3 =
            "KEYPAD\nOne column has all\n4 symbols. Press them\nin that column's order,\ntop to bottom.";

        int wireCount, wireToCut, buttonColor, labelIndex, stripIndex, pageIndex, strikes, keyProgress;
        int[] wireColor;
        bool[] wireIsCut, keyDone, solved;
        int[] keySymbol, keyOrder;
        bool shouldHold, holding, stripLit, busy, swiping;
        float timeLeft, holdTime, bubbleUntil, clock;
        Vector2 swipeStart;
        Vector3 buttonHome;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            timeLeft = seconds;
            solved = new bool[3];
            SetupWires();
            SetupButton();
            SetupKeypad();
            ShowPage(0);
            strip.color = new Color(0.1f, 0.1f, 0.12f);
            boom.gameObject.SetActive(false);
            flag.gameObject.SetActive(false);
            flash.color = new Color(1f, 1f, 1f, 0f);
            foreach (SpriteRenderer d in dots) d.color = new Color(1f, 1f, 1f, 0.5f);
            foreach (SpriteRenderer l in leds) l.color = new Color(0.9f, 0.27f, 0.24f);
            goalText.text = "Defuse all 3 modules!";
            UpdateStrikes();
            UpdateTimer();
            Say("Read the manual!", 2.5f);
        }

        void Say(string text, float secs)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + secs;
        }

        // ---------- Preparazione dei moduli ----------

        void SetupWires()
        {
            wireCount = Random.Range(4, 6);
            wireColor = new int[wireCount];
            wireIsCut = new bool[wireCount];
            for (int i = 0; i < wireCount; i++) wireColor[i] = Random.Range(0, WireNames.Length);
            wireToCut = WireRule();
            for (int i = 0; i < wires.Length; i++)
            {
                bool on = i < wireCount;
                wires[i].gameObject.SetActive(on);
                if (!on) continue;
                wires[i].localPosition = new Vector3(1.4f + (i - (wireCount - 1) * 0.5f) * 0.52f, 1.33f, 0f);
                wireArt[i].sprite = wireSprite;
                wireArt[i].color = WireColors[wireColor[i]];
            }
        }

        int Count(int color) { int n = 0; foreach (int c in wireColor) if (c == color) n++; return n; }
        int Last(int color) { for (int i = wireCount - 1; i >= 0; i--) if (wireColor[i] == color) return i; return -1; }

        /// <summary>Le regole della pagina 1 (vale la prima che si applica).</summary>
        int WireRule()
        {
            const int red = 0, blue = 1, yellow = 2, white = 3, black = 4;
            if (wireCount == 4)
            {
                if (Count(red) == 0) return 1;
                if (wireColor[3] == white) return 3;
                if (Count(blue) > 1) return Last(blue);
                return 3;
            }
            if (Count(red) > 2) return Last(red);
            if (wireColor[4] == black) return 3;
            if (Count(yellow) == 0) return 1;
            return 0;
        }

        void SetupButton()
        {
            buttonColor = Random.Range(0, ButtonColors.Length);
            labelIndex = Random.Range(0, Labels.Length);
            string label = Labels[labelIndex];
            if (buttonColor == 0 && label == "ABORT") shouldHold = true;
            else if (label == "DETONATE") shouldHold = false;
            else if (buttonColor == 1 && label == "HOLD") shouldHold = false;
            else shouldHold = true;
            buttonArt.color = ButtonColors[buttonColor];
            buttonLabel.text = label;
            buttonLabel.color = buttonColor <= 1 ? Color.white : new Color(0.17f, 0.13f, 0.2f);
            buttonHome = button.localPosition;
        }

        void SetupKeypad()
        {
            int[] col = Columns[Random.Range(0, Columns.Length)];
            var picks = new List<int>();
            for (int i = 0; i < col.Length; i++) picks.Add(i);
            while (picks.Count > 4) picks.RemoveAt(Random.Range(0, picks.Count));
            keyOrder = new int[4];
            for (int i = 0; i < 4; i++) keyOrder[i] = col[picks[i]];   // ordine giusto (dall'alto in basso)
            var shuffled = new List<int>(keyOrder);
            for (int i = shuffled.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]); }
            keySymbol = shuffled.ToArray();
            keyDone = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                keySymbols[i].sprite = symbolSprites[keySymbol[i]];
                keyArt[i].color = Color.white;
            }
            int k = 0;
            for (int c = 0; c < Columns.Length; c++)
                for (int r = 0; r < Columns[c].Length; r++)
                    if (k < manualSymbols.Length) manualSymbols[k++].sprite = symbolSprites[Columns[c][r]];
        }

        // ---------- Manuale ----------

        void ShowPage(int index)
        {
            pageIndex = index;
            pageText.text = index == 0 ? Page1 : index == 1 ? Page2 : Page3;
            pageText.alignment = index == 2 ? TextAlignment.Center : TextAlignment.Left;
            var fit = pageText.GetComponent<TextMeshFit>();
            if (index == 2)
            {
                pageText.transform.localPosition = new Vector3(0f, 1.95f, -0.01f);
                if (fit != null) { fit.maxWidth = 2.3f; fit.maxHeight = 1.4f; }
            }
            else
            {
                pageText.transform.localPosition = new Vector3(0f, -0.55f, -0.01f);
                if (fit != null) { fit.maxWidth = 2.3f; fit.maxHeight = 6.4f; }
            }
            foreach (SpriteRenderer s in manualSymbols) s.gameObject.SetActive(index == 2);
            pageNumber.text = (index + 1) + " / 3";
        }

        IEnumerator Flip(int to)
        {
            Sfx.Play(this, "whoosh", 0.4f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.12f) { page.localScale = new Vector3(1f - t, 1f, 1f); yield return null; }
            ShowPage(to);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.12f) { page.localScale = new Vector3(t, 1f, 1f); yield return null; }
            page.localScale = Vector3.one;
        }

        void TurnPage(int dir)
        {
            int to = Mathf.Clamp(pageIndex + dir, 0, 2);
            if (to == pageIndex) { Sfx.Play(this, "tick", 0.4f); return; }
            StartCoroutine(Flip(to));
        }

        // ---------- Ciclo ----------

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;

            timeLeft -= Time.deltaTime;
            UpdateTimer();
            if (timeLeft <= 0f) { timeLeft = 0f; UpdateTimer(); StartCoroutine(Explode("Time's up!")); return; }

            bool has = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended);
            Vector2 w = has ? PointerInput.ToWorld(cam, sp) : Vector2.zero;

            if (holding)
            {
                holdTime += Time.deltaTime;
                if (!stripLit && holdTime > 0.5f)
                {
                    stripLit = true;
                    stripIndex = Random.Range(0, StripColors.Length);
                    strip.color = StripColors[stripIndex];
                    Sfx.Play(this, "tick", 0.6f);
                }
                if (!has || ended) ReleaseButton();
                return;
            }

            if (swiping)
            {
                if (!has || ended)
                {
                    swiping = false;
                    float dx = (has ? w.x : swipeStart.x) - swipeStart.x;
                    if (Mathf.Abs(dx) > 0.5f) TurnPage(dx < 0f ? 1 : -1);
                }
                return;
            }

            if (!has || !began || PointerInput.IsOverUi()) return;
            if (prevArrow.OverlapPoint(w)) { TurnPage(-1); return; }
            if (nextArrow.OverlapPoint(w)) { TurnPage(1); return; }
            if (manualArea.OverlapPoint(w)) { swiping = true; swipeStart = w; return; }
            if (!solved[1] && buttonArea.OverlapPoint(w))
            {
                holding = true;
                holdTime = 0f;
                stripLit = false;
                button.localPosition = buttonHome + new Vector3(0f, -0.05f, 0f);
                button.localScale = Vector3.one * 0.94f;
                Sfx.Play(this, "clunk", 0.5f);
                Haptics.Light();
                return;
            }
            for (int i = 0; i < wireCount; i++)
            {
                if (wireIsCut[i]) continue;
                Collider2D c = wires[i].GetComponent<Collider2D>();
                if (c != null && c.OverlapPoint(w)) { CutWire(i); return; }
            }
            for (int i = 0; i < keys.Length; i++)
            {
                Collider2D c = keys[i].GetComponent<Collider2D>();
                if (c != null && c.OverlapPoint(w)) { PressKey(i); return; }
            }
        }

        void UpdateTimer()
        {
            int s = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            timerText.text = (s / 60) + ":" + (s % 60).ToString("00");
        }

        void UpdateStrikes()
        {
            string t = "";
            for (int i = 0; i < 3; i++) t += i < strikes ? "X" : "-";
            strikesText.text = t;
        }

        // ---------- Moduli ----------

        void CutWire(int i)
        {
            if (solved[0]) return;
            wireIsCut[i] = true;
            wireArt[i].sprite = wireCutSprite;
            Sfx.Play(this, "tick", 0.9f);
            Haptics.Light();
            if (i == wireToCut) Solve(0);
            else Strike("Wrong wire!");
        }

        void ReleaseButton()
        {
            holding = false;
            button.localPosition = buttonHome;
            button.localScale = Vector3.one;
            bool quick = holdTime < 0.5f;
            bool ok;
            string why;
            if (!shouldHold) { ok = quick; why = "That one was a TAP!"; }
            else if (quick) { ok = false; why = "You had to HOLD it!"; }
            else
            {
                ok = timerText.text.IndexOf(StripDigits[stripIndex]) >= 0;
                why = "Bad timing!";
            }
            strip.color = new Color(0.1f, 0.1f, 0.12f);
            stripLit = false;
            if (ok) Solve(1);
            else Strike(why);
        }

        void PressKey(int i)
        {
            if (solved[2] || keyDone[i]) return;
            Sfx.Play(this, "tick", 0.7f);
            StartCoroutine(Bump(keys[i]));
            if (keySymbol[i] == keyOrder[keyProgress])
            {
                keyDone[i] = true;
                keyArt[i].color = new Color(0.62f, 0.95f, 0.6f);
                keyProgress++;
                if (keyProgress >= 4) Solve(2);
            }
            else Strike("Wrong symbol!");
        }

        IEnumerator Bump(Transform t)
        {
            t.localScale = Vector3.one * 0.9f;
            yield return new WaitForSeconds(0.08f);
            t.localScale = Vector3.one;
        }

        void Solve(int module)
        {
            solved[module] = true;
            leds[module].color = new Color(0.42f, 0.88f, 0.48f);
            dots[module].color = new Color(0.42f, 0.88f, 0.48f);
            Sfx.Play(this, "ding", 0.5f);
            if (solved[0] && solved[1] && solved[2]) StartCoroutine(Defused());
            else Say(module == 0 ? "Wires: done!" : module == 1 ? "Button: done!" : "Keypad: done!", 1.5f);
        }

        void Strike(string why)
        {
            strikes++;
            UpdateStrikes();
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Medium();
            StartCoroutine(Flash(new Color(1f, 0.2f, 0.15f), 0.35f));
            if (strikes >= 3) { StartCoroutine(Explode("Three strikes...")); return; }
            Say(why + " Strike " + strikes + "!", 1.8f);
        }

        IEnumerator Flash(Color c, float secs)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / secs)
            {
                flash.color = new Color(c.r, c.g, c.b, (1f - t) * 0.45f);
                yield return null;
            }
            flash.color = new Color(c.r, c.g, c.b, 0f);
        }

        // ---------- Esiti ----------

        IEnumerator Explode(string why)
        {
            busy = true;
            holding = false;
            Say(why, 2f);
            for (int i = 0; i < 6; i++)
            {
                timerText.color = i % 2 == 0 ? Color.white : new Color(1f, 0.3f, 0.25f);
                Sfx.Play(this, "tick", 0.8f);
                yield return new WaitForSeconds(0.12f);
            }
            boom.gameObject.SetActive(true);
            Sfx.Play(this, "bonk", 1f);
            Sfx.Play(this, "clunk", 1f);
            Haptics.Heavy();
            Vector3 home = transform.localPosition;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.8f)
            {
                boom.localScale = Vector3.one * Mathf.Lerp(0.3f, 3.2f, Mathf.Sqrt(t));
                boom.localRotation = Quaternion.Euler(0f, 0f, t * 40f);
                flash.color = new Color(1f, 1f, 1f, (1f - t) * 0.9f);
                transform.localPosition = home + (Vector3)(Random.insideUnitCircle * 0.15f * (1f - t));
                yield return null;
            }
            transform.localPosition = home;
            yield return new WaitForSeconds(0.4f);
            Fail();
        }

        IEnumerator Defused()
        {
            busy = true;
            goalText.text = "Bomb defused!";
            Say("Phew... defused!", 2f);
            for (int i = 0; i < 4; i++)
            {
                timerText.color = i % 2 == 0 ? new Color(0.42f, 0.88f, 0.48f) : Color.white;
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(0.4f);
            // il colpo di scena: la bomba era un giocattolo
            flag.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.9f);
            Vector3 to = flag.localPosition, from = to + new Vector3(0f, -1.2f, 0f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                float k = 1f - (1f - t) * (1f - t);
                flag.localPosition = Vector3.LerpUnclamped(from, to, k * 1.1f - 0.1f * k * k);
                yield return null;
            }
            flag.localPosition = to;
            Say("...it was a toy all along!", 2.2f);
            yield return new WaitForSeconds(2.2f);
            Win();
        }
    }
}
