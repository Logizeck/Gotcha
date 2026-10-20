using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Invisible button" — stanza buia con un pulsante invisibile. Strisciando il dito sullo schermo il
    /// telefono vibra sempre più forte vicino al pulsante (tre intensità, più un'onda luminosa e un suono opzionale);
    /// poi si alza il dito e si tocca il punto giusto. Tre tocchi di conferma nel posto sbagliato = sconfitta.
    /// </summary>
    public class InvisibleButtonLevel : LevelController
    {
        [SerializeField] SpriteRenderer dark, probe, buttonArt;
        [SerializeField] Sprite pressedSprite;
        [SerializeField] Transform hiddenButton;
        [SerializeField] Collider2D soundToggle;
        [SerializeField] TextMesh soundText, missText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int missesAllowed = 3;
        [SerializeField] float hitRadius = 0.55f;
        [SerializeField] Vector2 areaMin = new Vector2(-2.1f, -3.4f), areaMax = new Vector2(2.1f, 1.6f);

        static readonly float[] ZoneRadius = { 0.5f, 1.0f, 1.7f };          // forte, medio, debole
        static readonly float[] PulseEvery = { 0.12f, 0.28f, 0.55f };
        static readonly long[] PulseMs = { 70, 35, 12 };
        static readonly float[] PulseHz = { 880f, 587f, 392f };
        static readonly Color[] ProbeTint = { new Color(1f, 0.45f, 0.4f, 0.55f), new Color(1f, 0.75f, 0.4f, 0.35f), new Color(0.6f, 0.8f, 1f, 0.2f) };

        Camera cam;
        bool touching, sliding, busy, sound;
        int finger, misses;
        float touchT, pulseT;
        Vector2 touchStart;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            hiddenButton.localPosition = new Vector3(Random.Range(areaMin.x, areaMax.x), Random.Range(areaMin.y, areaMax.y), 0f);
            sound = !Haptics.Enabled;   // senza vibrazione si parte col suono acceso
            S41HKit.Alpha(probe, 0f);
            S41HKit.Confetti(confetti, false);
            UpdateTexts();
            S41HKit.Say(bubble, "Who turned off the lights? The switch is here... somewhere.");
        }

        void UpdateTexts()
        {
            if (soundText != null) soundText.text = sound ? "SOUND: ON" : "SOUND: OFF";
            if (missText != null) missText.text = "WRONG PRESSES " + misses + "/" + missesAllowed;
        }

        int Zone(Vector2 p)
        {
            float d = Vector2.Distance(p, hiddenButton.position);
            for (int z = 0; z < ZoneRadius.Length; z++) if (d < ZoneRadius[z]) return z;
            return -1;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
            {
                if (f.began && !touching)
                {
                    if (soundToggle.OverlapPoint(f.world))
                    {
                        sound = !sound;
                        UpdateTexts();
                        Sfx.Play(this, "tick", 0.6f);
                        StartCoroutine(S41HKit.Pop(soundToggle.transform, 0.15f, 0.2f));
                        continue;
                    }
                    touching = true;
                    sliding = false;
                    finger = f.id;
                    touchT = 0f;
                    pulseT = 0f;
                    touchStart = f.world;
                }
                if (!touching || f.id != finger) continue;
                touchT += Time.deltaTime;
                if (!sliding && (touchT > 0.35f || Vector2.Distance(f.world, touchStart) > 0.3f)) sliding = true;
                if (sliding) Probe(f.world);
                if (f.ended)
                {
                    touching = false;
                    S41HKit.Alpha(probe, 0f);
                    if (!sliding) StartCoroutine(Press(f.world));
                }
            }
            if (touching && S41HKit.Fingers(cam).Count == 0) { touching = false; S41HKit.Alpha(probe, 0f); }
        }

        void Probe(Vector2 at)
        {
            int z = Zone(at);
            probe.transform.position = new Vector3(at.x, at.y, 0f);
            pulseT -= Time.deltaTime;
            if (z < 0) { S41HKit.Alpha(probe, Mathf.MoveTowards(probe.color.a, 0.06f, Time.deltaTime)); return; }
            if (pulseT <= 0f)
            {
                pulseT = PulseEvery[z];
                Haptics.Vibrate(PulseMs[z]);
                if (sound) S41HKit.Note(this, PulseHz[z], 0.35f, 0.08f);
                probe.color = ProbeTint[z];
                probe.transform.localScale = Vector3.one * (1.2f - z * 0.25f);
            }
            else
            {
                probe.transform.localScale *= 1f - Time.deltaTime * 1.5f;
                S41HKit.Alpha(probe, probe.color.a * (1f - Time.deltaTime * 2f));
            }
        }

        IEnumerator Press(Vector2 at)
        {
            busy = true;
            if (Vector2.Distance(at, hiddenButton.position) <= hitRadius)
            {
                Sfx.Play(this, "clunk", 0.9f);
                Haptics.Heavy();
                buttonArt.sprite = pressedSprite;
                yield return StartCoroutine(S41HKit.Fade(dark, 0f, 0.6f));
                Sfx.Play(this, "ding", 0.8f);
                S41HKit.Say(bubble, "LIGHTS ON! You've got sensitive fingers!");
                S41HKit.Confetti(confetti, true);
                yield return new WaitForSeconds(2.2f);
                S41HKit.Confetti(confetti, false);
                Win();
                yield break;
            }
            misses++;
            UpdateTexts();
            Sfx.Play(this, "bonk", 0.6f);
            Haptics.Light();
            S41HKit.Say(bubble, misses == 1 ? "*tap* ...just a wall." : "*tap* Nope. Wall again.");
            yield return new WaitForSeconds(0.9f);
            if (misses >= missesAllowed)
            {
                S41HKit.Say(bubble, "Fine. I'll sleep in the dark.");
                yield return new WaitForSeconds(1.2f);
                Fail();
                yield break;
            }
            S41HKit.Say(bubble, "Slide your finger around and feel it buzz...");
            busy = false;
        }
    }
}
