using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 98 — il cronometro cieco. Premi il pulsante per far partire il cronometro e premilo di nuovo esattamente
    /// al tempo richiesto. Ma dopo due secondi le cifre si spengono: devi contare a mente! Tre tentativi.
    /// Easter egg `speedrun`: fermarlo subito, in meno di un quarto di secondo.
    /// </summary>
    public class StopwatchLevel : LevelController
    {
        [SerializeField] TapButton button;
        [SerializeField] TextMesh display;
        [SerializeField] TextMesh goal;
        [SerializeField] TextMesh tries;
        [SerializeField] Transform hand;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float tolerance = 0.12f, visibleFor = 2f;
        [SerializeField] int attempts = 3;
        [SerializeField] string eggId = "speedrun";

        static readonly float[] targets = { 5f, 6f, 7f, 8f };
        float target, started = -1f;
        int left;
        bool done, busy;

        public override void OnLevelStart()
        {
            target = targets[Random.Range(0, targets.Length)];
            left = attempts;
            goal.text = "STOP AT " + target.ToString("0.00") + " s";
            display.text = "0.00";
            Tries();
            button.Pressed += _ => Press();
            if (bubble != null) { bubble.Show("Count in your head. The digits will vanish!"); StartCoroutine(HideLater(2.4f)); }
        }

        void Tries() { tries.text = "TRIES: " + left; }

        void Update()
        {
            if (started < 0f || done || busy) return;
            float t = Time.time - started;
            display.text = t < visibleFor ? t.ToString("0.00") : "?.??";
            hand.localRotation = Quaternion.Euler(0, 0, t < visibleFor ? -t * 36f : -visibleFor * 36f);   // anche la lancetta si blocca
        }

        void Press()
        {
            if (done || busy) return;
            if (started < 0f) { started = Time.time; Sfx.Play(this, "tick"); return; }
            float t = Time.time - started;
            started = -1f;
            StartCoroutine(Result(t));
        }

        IEnumerator Result(float t)
        {
            busy = true;
            display.text = t.ToString("0.00");
            hand.localRotation = Quaternion.Euler(0, 0, -t * 36f);
            Sfx.Play(this, "clunk");
            if (t < 0.25f)
            {
                done = true;
                if (bubble != null) bubble.Show("0." + Mathf.RoundToInt(t * 100f).ToString("00") + "?! WORLD RECORD! ...wrong game.");
                yield return new WaitForSeconds(2f);
                FailWithEgg(eggId);
                yield break;
            }
            float diff = t - target;
            if (Mathf.Abs(diff) <= tolerance)
            {
                done = true;
                Sfx.Play(this, "ding", 0.8f);
                if (bubble != null) bubble.Show("Swiss precision!");
                yield return new WaitForSeconds(1.5f);
                Win();
                yield break;
            }
            left--;
            Tries();
            if (bubble != null) bubble.Show(diff > 0f ? "Too late by " + diff.ToString("0.00") + " s" : "Too early by " + (-diff).ToString("0.00") + " s");
            if (left <= 0)
            {
                done = true;
                yield return new WaitForSeconds(1.6f);
                Fail();
                yield break;
            }
            yield return new WaitForSeconds(1.6f);
            display.text = "0.00";
            hand.localRotation = Quaternion.identity;
            busy = false;
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
