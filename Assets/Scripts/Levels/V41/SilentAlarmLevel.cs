using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The silent alarm" — un sistema di sicurezza con 4 pulsanti colorati e il cartello "DO NOT MAKE A
    /// SOUND". Il monitor mostra in loop la sequenza giusta (4 colori). Il trucco: ogni pulsante fa "bip", e il bip fa
    /// scattare l'allarme. Soluzione: abbassare a zero il volume multimediale del telefono (o, in alternativa, la
    /// manopola virtuale del volume a destra), poi premere i pulsanti nell'ordine mostrato. Sconfitta: tre allarmi
    /// (pulsanti premuti col volume alto). L'ordine sbagliato col volume a zero azzera solo l'inserimento.
    /// (Editor: tasti - e + per il volume simulato.)
    /// </summary>
    public class SilentAlarmLevel : LevelController
    {
        [SerializeField] SpriteRenderer[] buttons;        // rosso, verde, blu, giallo
        [SerializeField] SpriteRenderer[] screenLamps;    // le 4 lucine del monitor
        [SerializeField] SpriteRenderer[] progressDots;
        [SerializeField] TextMesh screenText;
        [SerializeField] Transform knob;                  // manopola del volume virtuale
        [SerializeField] float knobMinY = -3.5f, knobMaxY = -0.6f;
        [SerializeField] SpriteRenderer[] volumeBars;
        [SerializeField] SpriteRenderer speakerCross;     // la X sull'altoparlante quando è muto
        [SerializeField] SpriteRenderer alarmFlash;
        [SerializeField] Transform siren;
        [SerializeField] Transform vaultDoor;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;

        static readonly Color[] Colors = { new Color(1f, 0.3f, 0.3f), new Color(0.35f, 0.9f, 0.4f), new Color(0.35f, 0.6f, 1f), new Color(1f, 0.85f, 0.25f) };
        static readonly float[] Beeps = { 523f, 659f, 784f, 988f };

        Camera cam;
        readonly int[] seq = new int[4];
        int entered, strikes, pressed = -1;
        float virtualVolume = 1f, showClock, pressTimer;
        bool draggingKnob, busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            for (int i = 0; i < seq.Length; i++)
            {
                int c;
                do c = Random.Range(0, 4); while (i > 0 && c == seq[i - 1]);
                seq[i] = c;
            }
            if (alarmFlash != null) alarmFlash.color = new Color(1f, 0f, 0f, 0f);
            if (bubble != null) bubble.Hide();
            Kit41B.Hearts(hearts, 3);
            SetKnob();
            ShowProgress();
            if (screenText != null) screenText.text = "ENTER CODE";
        }

        float SystemVolume => SensorHub.Instance != null ? SensorHub.Instance.SystemVolume01 : AudioListener.volume;
        float EffectiveVolume => Mathf.Min(SystemVolume, virtualVolume);
        bool Muted => SystemVolume <= 0.01f || virtualVolume <= 0.03f;

        void SetKnob()
        {
            if (knob != null) knob.localPosition = new Vector3(knob.localPosition.x, Mathf.Lerp(knobMinY, knobMaxY, virtualVolume), knob.localPosition.z);
        }

        void ShowProgress()
        {
            for (int i = 0; i < progressDots.Length; i++) progressDots[i].color = i < entered ? Colors[seq[i]] : new Color(1f, 1f, 1f, 0.25f);
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;

            // indicatore del volume
            float v = Muted ? 0f : EffectiveVolume;
            for (int i = 0; i < volumeBars.Length; i++) volumeBars[i].color = v > (i + 0.5f) / volumeBars.Length * 0.999f || (i == 0 && v > 0.01f) ? new Color(0.4f, 0.95f, 0.5f) : new Color(1f, 1f, 1f, 0.18f);
            if (speakerCross != null) speakerCross.enabled = Muted;

            // il monitor ripete la sequenza
            showClock += dt;
            const float step = 0.7f, cycle = 0.7f * 4f + 1.3f;
            float t = Mathf.Repeat(showClock, cycle);
            int lit = t < step * 4f && Mathf.Repeat(t, step) < 0.5f ? (int)(t / step) : -1;
            for (int i = 0; i < screenLamps.Length; i++) screenLamps[i].color = i == lit ? Colors[seq[i]] : new Color(0.3f, 0.35f, 0.4f);

            if (pressTimer > 0f) { pressTimer -= dt; if (pressTimer <= 0f && pressed >= 0) { buttons[pressed].transform.localScale = Vector3.one * baseScale; pressed = -1; } }
            if (busy) return;

            if (!Kit41B.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { draggingKnob = false; return; }
            if (began)
            {
                if (knob != null && Vector2.Distance(w, knob.position) < 0.6f) { draggingKnob = true; return; }
                for (int i = 0; i < buttons.Length; i++)
                {
                    Collider2D col = buttons[i].GetComponent<Collider2D>();
                    if (col != null && col.OverlapPoint(w)) { Press(i); return; }
                }
                return;
            }
            if (draggingKnob)
            {
                Vector3 local = knob.parent != null ? knob.parent.InverseTransformPoint(w) : (Vector3)w;
                virtualVolume = Mathf.Clamp01(Mathf.InverseLerp(knobMinY, knobMaxY, local.y));
                if (virtualVolume < 0.04f) virtualVolume = 0f;
                SetKnob();
                if (ended) draggingKnob = false;
            }
        }

        float baseScale = 1f;

        void Press(int i)
        {
            if (pressed >= 0) buttons[pressed].transform.localScale = Vector3.one * baseScale;
            baseScale = buttons[i].transform.localScale.x;
            pressed = i;
            pressTimer = 0.15f;
            buttons[i].transform.localScale = Vector3.one * baseScale * 0.9f;
            if (!Muted)
            {
                Kit41B.Tone(this, Beeps[i], 0.25f, "sine", 0.5f * EffectiveVolume + 0.2f);
                StartCoroutine(Alarm());
                return;
            }
            Haptics.Light();
            if (seq[entered] == i)
            {
                entered++;
                ShowProgress();
                if (entered >= seq.Length) StartCoroutine(Granted());
                return;
            }
            entered = 0;
            ShowProgress();
            StartCoroutine(WrongOrder());
        }

        IEnumerator WrongOrder()
        {
            busy = true;
            if (screenText != null) { screenText.text = "WRONG ORDER"; screenText.color = new Color(1f, 0.5f, 0.4f); }
            yield return new WaitForSeconds(0.9f);
            if (screenText != null) { screenText.text = "ENTER CODE"; screenText.color = Color.white; }
            busy = false;
        }

        IEnumerator Alarm()
        {
            busy = true;
            strikes++;
            entered = 0;
            ShowProgress();
            Kit41B.Hearts(hearts, 3 - strikes);
            Haptics.Heavy();
            if (screenText != null) { screenText.text = "INTRUDER!"; screenText.color = new Color(1f, 0.4f, 0.35f); }
            Kit41B.Say(bubble, strikes >= 3 ? "BUSTED!" : "BEEP = NOISE! ALARM!");
            for (int k = 0; k < 6; k++)
            {
                Kit41B.Tone(this, k % 2 == 0 ? 880f : 660f, 0.24f, "square", 0.5f);
                if (alarmFlash != null) alarmFlash.color = new Color(1f, 0f, 0f, k % 2 == 0 ? 0.35f : 0.1f);
                if (siren != null) siren.localRotation = Quaternion.Euler(0f, 0f, k * 60f);
                yield return new WaitForSeconds(0.25f);
            }
            if (alarmFlash != null) alarmFlash.color = new Color(1f, 0f, 0f, 0f);
            if (strikes >= 3) { yield return new WaitForSeconds(0.3f); Fail(); yield break; }
            if (screenText != null) { screenText.text = "ENTER CODE"; screenText.color = Color.white; }
            if (bubble != null) bubble.Hide();
            busy = false;
        }

        IEnumerator Granted()
        {
            busy = true;
            if (screenText != null) { screenText.text = "ACCESS GRANTED"; screenText.color = new Color(0.5f, 1f, 0.55f); }
            Haptics.Medium();
            yield return new WaitForSeconds(0.6f);
            Kit41B.Say(bubble, "...not a peep. Respect.");
            if (vaultDoor != null)
            {
                Vector3 p = vaultDoor.localPosition;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
                {
                    vaultDoor.localPosition = p + Vector3.right * (k * k * 3.2f);
                    yield return null;
                }
            }
            Sfx.Play(this, "ding", 0.3f);
            yield return new WaitForSeconds(1f);
            Win();
        }
    }
}
