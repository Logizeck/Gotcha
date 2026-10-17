using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 67 — lo specchio appannato. L'armadietto del bagno ha un lucchetto a codice. Il codice c'è, ma è
    /// scritto col dito sullo specchio: si vede solo quando è appannato. Soffia sul telefono (microfono) per
    /// appannarlo, leggi le cifre prima che la condensa sparisca, poi tocca l'armadietto e digita il codice.
    /// Easter egg `bad_breath`: soffiare senza sosta per tanti secondi... lo specchio protesta.
    /// </summary>
    public class MirrorLevel : LevelController
    {
        [SerializeField] SpriteRenderer fog;
        [SerializeField] FingerText code;   // scritto col dito sullo specchio
        [SerializeField] Collider2D cabinetArea;
        [SerializeField] GameObject cabinetClosed, cabinetOpen;
        [SerializeField] GameObject keypad;
        [SerializeField] Collider2D keypadPanel;
        [SerializeField] TapButton[] keys;
        [SerializeField] TextMesh display;
        [SerializeField] SmokeEmitter steam;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float fogRise = 1.1f, fogFall = 0.22f, blowLevel = 0.2f;
        [SerializeField] float eggSeconds = 5f;
        [SerializeField] string eggId = "bad_breath";

        string secret, typed = "";
        float misty, blowingFor;
        bool done, busy;
        Camera cam;
        Color ink = new Color(0.45f, 0.6f, 0.7f);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            secret = "";
            for (int i = 0; i < 4; i++) secret += (char)('0' + Random.Range(0, 10));
            code.Text = secret;
            keypad.SetActive(false);
            cabinetOpen.SetActive(false);
            foreach (TapButton k in keys) k.Pressed += OnKey;
            if (steam != null) steam.Stop();
            Apply();
            if (bubble != null) { bubble.Show("The code? It's on the mirror... somewhere."); StartCoroutine(HideLater(2.4f)); }
        }

        void OnDestroy() { Inputs.ReleaseMic(this); }

        void Apply()
        {
            fog.color = new Color(1f, 1f, 1f, misty * 0.85f);
            // le cifre sono "pulite" dal dito: si vedono come segni più scuri nella condensa
            code.Color = new Color(ink.r, ink.g, ink.b, misty * 0.95f);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float mic = Inputs.Mic(this);
            bool blowing = mic > blowLevel;
            misty = Mathf.Clamp01(misty + (blowing ? fogRise : -fogFall) * Time.deltaTime);
            blowingFor = blowing ? blowingFor + Time.deltaTime : Mathf.Max(0f, blowingFor - Time.deltaTime * 0.3f);   // il fiato va a scatti: le pausette non azzerano
            if (steam != null) { if (blowing && !steam.IsPlaying) steam.Play(); else if (!blowing && steam.IsPlaying) steam.Stop(); }
            Apply();
            if (blowingFor > eggSeconds) { StartCoroutine(BadBreath()); return; }

            if (busy || !PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (!keypad.activeSelf) { if (cabinetArea.OverlapPoint(w)) { keypad.SetActive(true); typed = ""; Show(); } }
            else if (!keypadPanel.OverlapPoint(w)) keypad.SetActive(false);
        }

        void OnKey(TapButton k)
        {
            if (done || busy || !keypad.activeSelf) return;
            typed += k.id;
            Show();
            if (typed.Length >= secret.Length) StartCoroutine(Check());
        }

        void Show()
        {
            display.text = string.Join(" ", typed.PadRight(secret.Length, '_').ToCharArray());
            display.color = new Color(0.36f, 1f, 0.6f);
        }

        IEnumerator Check()
        {
            busy = true;
            yield return new WaitForSeconds(0.25f);
            if (typed == secret)
            {
                done = true;
                keypad.SetActive(false);
                cabinetClosed.SetActive(false);
                cabinetOpen.SetActive(true);
                if (bubble != null) bubble.Show("Toothpaste! Finally.");
                yield return new WaitForSeconds(1.5f);
                Win();
                yield break;
            }
            display.text = "ERR";
            display.color = new Color(1f, 0.3f, 0.3f);
            yield return new WaitForSeconds(0.7f);
            typed = "";
            Show();
            busy = false;
        }

        /// <summary>Easter egg: hai soffiato così tanto che lo specchio ti risponde.</summary>
        IEnumerator BadBreath()
        {
            done = true;
            if (steam != null) steam.Stop();
            misty = 1f;
            code.Text = "BRUSH\nYOUR\nTEETH!";   // si rimpicciolisce da sola per stare nel vetro
            Apply();
            if (bubble != null) bubble.Show("...the mirror has opinions.");
            yield return new WaitForSeconds(2.2f);
            FailWithEgg(eggId);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
