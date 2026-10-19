using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 37 — il codice in rosso e blu. Sul muro ci sono 4 cifre, ognuna scritta in rosso o in blu (a caso).
    /// Al buio non si vede niente; con la luce rossa si leggono solo le cifre blu, con la luce blu solo le rosse.
    /// I due interruttori non si possono accendere insieme: bisogna memorizzare metà codice con una luce e metà
    /// con l'altra, poi digitarlo sulla cassaforte (toccala per aprire la tastiera).
    /// </summary>
    public class LampSafeLevel : LevelController
    {
        [SerializeField] TapButton redSwitch;
        [SerializeField] TapButton blueSwitch;
        [SerializeField] GameObject redLight;
        [SerializeField] GameObject blueLight;
        [SerializeField] SpriteRenderer wall;
        [SerializeField] TextMesh[] digits;
        [SerializeField] Collider2D safeArea;
        [SerializeField] GameObject safeClosed;
        [SerializeField] GameObject safeOpen;
        [SerializeField] GameObject keypad;
        [SerializeField] Collider2D keypadPanel;
        [SerializeField] TapButton[] keys;
        [SerializeField] TextMesh display;
        [SerializeField] SmokeEmitter confetti;
        [SerializeField] SpeechBubble bubble;

        static readonly Color WallOff = new Color(0.13f, 0.13f, 0.17f);
        static readonly Color WallRed = new Color(0.86f, 0.32f, 0.32f);
        static readonly Color WallBlue = new Color(0.32f, 0.42f, 0.9f);
        static readonly Color InkDark = new Color(0.1f, 0.05f, 0.1f);

        string code;
        bool[] isRed;
        bool red, blue, busy, done;
        string typed = "";
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            // codice casuale; colori casuali ma almeno una cifra per colore
            isRed = new bool[digits.Length];
            var sb = new System.Text.StringBuilder();
            do { for (int i = 0; i < isRed.Length; i++) isRed[i] = Random.value < 0.5f; }
            while (System.Array.TrueForAll(isRed, r => r) || System.Array.TrueForAll(isRed, r => !r));
            for (int i = 0; i < digits.Length; i++) { char c = (char)('0' + Random.Range(0, 10)); sb.Append(c); digits[i].text = c.ToString(); }
            code = sb.ToString();
            redSwitch.Pressed += b => Toggle(true);
            blueSwitch.Pressed += b => Toggle(false);
            foreach (TapButton k in keys) k.Pressed += OnKey;
            if (keypad != null) keypad.SetActive(false);
            if (safeOpen != null) safeOpen.SetActive(false);
            if (confetti != null) confetti.Stop();
            Apply();
            if (bubble != null) { bubble.Show("Only one light at a time!"); StartCoroutine(HideLater(2f)); }
        }

        void Toggle(bool isRedSwitch)
        {
            if (done || keypad.activeSelf) return;
            if (isRedSwitch) { red = !red; if (red) blue = false; }
            else { blue = !blue; if (blue) red = false; }
            Apply();
        }

        void Apply()
        {
            if (redLight != null) redLight.SetActive(red);
            if (blueLight != null) blueLight.SetActive(blue);
            Color w = red ? WallRed : blue ? WallBlue : WallOff;
            if (wall != null) wall.color = w;
            for (int i = 0; i < digits.Length; i++)
            {
                // inchiostro dello stesso colore della luce = invisibile; l'altro colore si legge scuro
                bool visible = (red && !isRed[i]) || (blue && isRed[i]);
                digits[i].color = visible ? InkDark : w;
            }
        }

        void Update()
        {
            if (done || busy || IsFinished) return;
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (!keypad.activeSelf) { if (safeArea != null && safeArea.OverlapPoint(w)) { keypad.SetActive(true); typed = ""; Show(); } }
            else if (keypadPanel != null && !keypadPanel.OverlapPoint(w)) keypad.SetActive(false);
        }

        void OnKey(TapButton k)
        {
            if (done || busy || !keypad.activeSelf) return;
            typed += k.id;
            Show();
            if (typed.Length >= code.Length) StartCoroutine(Check());
        }

        void Show()
        {
            string s = typed.PadRight(code.Length, '_');
            if (display != null) { display.text = string.Join(" ", s.ToCharArray()); display.color = new Color(0.36f, 1f, 0.6f); }
        }

        IEnumerator Check()
        {
            busy = true;
            yield return new WaitForSeconds(0.25f);
            if (typed == code)
            {
                if (display != null) display.text = "OPEN";
                yield return new WaitForSeconds(0.5f);
                done = true;
                keypad.SetActive(false);
                if (safeClosed != null) safeClosed.SetActive(false);
                if (safeOpen != null) safeOpen.SetActive(true);
                Haptics.Light();
                if (confetti != null) confetti.Play();
                if (bubble != null) bubble.Show("Cracked it!");
                yield return new WaitForSeconds(1.6f);
                if (confetti != null) confetti.Stop();
                Win();
                yield break;
            }
            if (display != null) { display.text = "ERR"; display.color = new Color(1f, 0.3f, 0.3f); }
            yield return new WaitForSeconds(0.7f);
            typed = "";
            Show();
            busy = false;
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
