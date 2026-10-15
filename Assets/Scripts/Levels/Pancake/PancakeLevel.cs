using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 35 — la frittella. Un colpo secco verso l'alto col telefono la lancia: deve fare mezzo giro e
    /// ricadere nella padella, che si sposta inclinando il telefono. Tre giri riusciti e la frittella è perfetta.
    /// Troppo piano non si gira; troppo forte... resta appiccicata al soffitto! Se cade fuori: splat per terra.
    /// (Editor: tasti 1 = piano, 2 = giusto, 3 = troppo forte; frecce per la padella.)
    /// </summary>
    public class PancakeLevel : LevelController
    {
        [SerializeField] Transform pan;
        [SerializeField] Transform pancake;
        [SerializeField] GameObject rawUp;            // lato crudo in alto
        [SerializeField] GameObject cookedUp;         // lato cotto in alto
        [SerializeField] SmokeEmitter steam;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int flipsNeeded = 3;
        [SerializeField] float panY = -1.9f;
        [SerializeField] float ceilingY = 3.3f;
        [SerializeField] float floorY = -3.4f;
        [SerializeField] float gravity = 14f;
        [SerializeField] float idealSpeed = 7f;       // lancio che fa esattamente mezzo giro
        [SerializeField] float speedPerShake = 0.4f;
        [SerializeField] float shakeTrigger = 8f;
        [SerializeField] float panRange = 1.8f;
        [SerializeField] float catchRadius = 0.75f;

        enum State { Resting, Charging, Flying, Over }
        State state = State.Resting;
        int flips;
        bool rawIsUp = true;
        float vy, vx, spin, peak, chargeTime, rest, baselineX;
        bool primed;

        public override void OnLevelStart()
        {
            Show();
            if (steam != null) steam.Play();
            if (bubble != null) { bubble.Show("Flip it! (a quick flick)"); StartCoroutine(HideLater(2f)); }
        }

        void Update()
        {
            if (state == State.Over || IsFinished) return;
            // la padella segue l'inclinazione (rispetto a come tieni il telefono all'inizio)
            Vector3 t = SensorHub.Instance != null ? SensorHub.Instance.Tilt : Vector3.zero;
            if (!primed) { primed = true; baselineX = t.x; }
            float target = Mathf.Clamp((t.x - baselineX) * 4f, -panRange, panRange);
            pan.position = new Vector3(Mathf.Lerp(pan.position.x, target, 1f - Mathf.Exp(-12f * Time.deltaTime)), pan.position.y, pan.position.z);

            float shake = SensorHub.Instance != null ? SensorHub.Instance.ShakeStrength : 0f;
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1)) shake = 10f;
            if (Input.GetKeyDown(KeyCode.Alpha2)) shake = 17.5f;
            if (Input.GetKeyDown(KeyCode.Alpha3)) shake = 32f;
#endif
            switch (state)
            {
                case State.Resting:
                    pancake.position = new Vector3(pan.position.x, panY, pancake.position.z);
                    rest -= Time.deltaTime;
                    if (rest <= 0f && shake > shakeTrigger) { state = State.Charging; peak = shake; chargeTime = 0f; }
                    break;
                case State.Charging:
                    // si misura il picco del colpo per un istante, poi parte il lancio
                    peak = Mathf.Max(peak, shake);
                    chargeTime += Time.deltaTime;
                    pancake.position = new Vector3(pan.position.x, panY, pancake.position.z);
                    if (chargeTime > 0.1f) Toss(peak);
                    break;
                case State.Flying:
                    Fly();
                    break;
            }
        }

        void Toss(float strength)
        {
            state = State.Flying;
            vy = Mathf.Clamp(strength * speedPerShake, 2.5f, 16f);
            float flight = 2f * vy / gravity;
            spin = 180f * (vy / idealSpeed) / flight;              // gradi al secondo: mezzo giro col lancio ideale
            vx = Random.Range(-0.9f, 0.9f) / flight;                // un po' di deriva: va ripresa con la padella
            if (steam != null) steam.Stop();
        }

        void Fly()
        {
            vy -= gravity * Time.deltaTime;
            pancake.position += new Vector3(vx, vy, 0f) * Time.deltaTime;
            pancake.Rotate(0, 0, spin * Time.deltaTime);
            if (pancake.position.y > ceilingY) { StartCoroutine(Ceiling()); return; }
            if (vy < 0f && pancake.position.y <= panY)
            {
                if (Mathf.Abs(pancake.position.x - pan.position.x) < catchRadius) Land();
                else StartCoroutine(Floor());
            }
        }

        void Land()
        {
            float a = Mathf.Repeat(pancake.eulerAngles.z, 360f);
            bool flipped = Mathf.Abs(Mathf.DeltaAngle(a, 180f)) < 50f;
            bool flat = flipped || Mathf.Abs(Mathf.DeltaAngle(a, 0f)) < 50f;
            pancake.rotation = Quaternion.identity;
            pancake.position = new Vector3(pancake.position.x, panY, pancake.position.z);
            state = State.Resting;
            rest = 0.5f;
            if (steam != null) steam.Play();
            StartCoroutine(Squash());
            if (flipped)
            {
                rawIsUp = !rawIsUp;
                flips++;
                Show();
                if (flips >= flipsNeeded) { StartCoroutine(Perfect()); return; }
                Say(flips == 1 ? "Flip!" : "Again!");
            }
            else Say(flat ? "Didn't flip... harder!" : "Wobbly! Try again.");
        }

        IEnumerator Squash()
        {
            Vector3 s = Vector3.one;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { pancake.localScale = new Vector3(1f + Mathf.Sin(k * Mathf.PI) * 0.15f, 1f - Mathf.Sin(k * Mathf.PI) * 0.3f, 1f); yield return null; }
            pancake.localScale = s;
        }

        void Show()
        {
            if (rawUp != null) rawUp.SetActive(rawIsUp);
            if (cookedUp != null) cookedUp.SetActive(!rawIsUp);
            if (counter != null) counter.text = flips + " / " + flipsNeeded;
        }

        IEnumerator Ceiling()
        {
            state = State.Over;
            // SPLAT: resta appiccicata al soffitto, schiacciata
            pancake.position = new Vector3(pancake.position.x, ceilingY, pancake.position.z);
            pancake.rotation = Quaternion.Euler(0, 0, 180f);
            pancake.localScale = new Vector3(1.25f, 0.7f, 1f);
            Say("SPLAT! ...too strong.");
            yield return new WaitForSeconds(1.6f);
            Fail();
        }

        IEnumerator Floor()
        {
            state = State.Over;
            while (pancake.position.y > floorY)
            {
                vy -= gravity * Time.deltaTime;
                pancake.position += new Vector3(vx, vy, 0f) * Time.deltaTime;
                yield return null;
            }
            pancake.rotation = Quaternion.identity;
            pancake.localScale = new Vector3(1.3f, 0.6f, 1f);
            Say("Splat! On the floor...");
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        IEnumerator Perfect()
        {
            state = State.Over;
            Say("Perfect pancake!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        void Say(string s)
        {
            if (bubble == null) return;
            bubble.Show(s);
            StopCoroutine("HideLater");
            StartCoroutine(HideLater(1.3f));
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && state != State.Over) bubble.Hide();
        }
    }
}
