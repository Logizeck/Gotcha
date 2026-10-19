using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Screen time" — un bambino a letto fissa lo schermo luminoso (cioè NOI!) e non vuole dormire: "Just one
    /// more video!". Spegnere la luce o toccarlo non serve ("Still watching!"). Soluzione: appoggiare il telefono a
    /// faccia IN GIÙ (o tenerlo capovolto in aria) per 3 secondi: si sente una ninna nanna e poi il russare. Girandolo
    /// di nuovo, il bambino dorme. Se lo giri troppo presto: "I'm awake!". (Editor: tieni premuto F = faccia in giù.)
    /// </summary>
    public class FaceDownLevel : LevelController
    {
        [SerializeField] SpriteRenderer kid;
        [SerializeField] Sprite awakeSprite, drowsySprite, asleepSprite;
        [SerializeField] SpriteRenderer screenGlow;   // alone blu dello schermo sulla faccia
        [SerializeField] SpriteRenderer darkness;     // la luce della stanza spenta
        [SerializeField] Collider2D kidArea;
        [SerializeField] Collider2D switchArea;
        [SerializeField] SpriteRenderer switchArt;
        [SerializeField] Sprite switchOn, switchOff;
        [SerializeField] Transform[] zzz;             // Z che salgono quando dorme
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float downZ = 0.75f, upZ = 0.4f, needSeconds = 3f, hintAfter = 12f;

        static readonly string[] Whines = { "Just one more video!", "Still watching!", "I'm NOT tired!", "Five more minutes!", "Shh, it's the best part!" };
        // ninna nanna di Brahms (Hz): mi mi sol, mi mi sol, mi sol do
        static readonly float[] Lullaby = { 329.6f, 329.6f, 392f, 329.6f, 329.6f, 392f, 329.6f, 392f, 523.3f };
        static readonly Dictionary<int, AudioClip> tones = new Dictionary<int, AudioClip>();

        Camera cam;
        AudioSource src;
        bool faceDown, asleep, busy, lightsOff, hinted;
        float downFor, clock, nextNote, nextSnore, bubbleUntil;
        int noteIndex, whine;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            src = GetComponent<AudioSource>();
            if (src == null) { src = gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            foreach (Transform z in zzz) z.gameObject.SetActive(false);
            Say("Just one more video!", 2.2f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }

            // lo schermo luccica sulla faccia
            if (screenGlow != null && !asleep)
            {
                Color c = screenGlow.color;
                c.a = (lightsOff ? 0.85f : 0.6f) + Mathf.Sin(clock * 7f) * 0.08f + (Random.value < 0.04f ? 0.15f : 0f);
                screenGlow.color = c;
            }

            float z = Input.acceleration.z;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.F)) z = 1f;
#endif
            if (!faceDown && z > downZ) { faceDown = true; downFor = 0f; noteIndex = 0; nextNote = 0.15f; }
            else if (faceDown && z < upZ) { faceDown = false; TurnedUp(); return; }

            if (faceDown)
            {
                downFor += dt;
                if (!asleep)
                {
                    if (downFor >= nextNote && noteIndex < Lullaby.Length)
                    {
                        src.PlayOneShot(Tone(Lullaby[noteIndex]), 0.5f);
                        noteIndex++;
                        nextNote += needSeconds / Lullaby.Length;
                    }
                    if (downFor > needSeconds * 0.5f) kid.sprite = drowsySprite;
                    if (downFor >= needSeconds)
                    {
                        asleep = true;
                        kid.sprite = asleepSprite;
                        if (screenGlow != null) screenGlow.enabled = false;
                        Sfx.Play(this, "snore", 0.9f);
                        Haptics.Medium();   // si sente in mano: ora puoi girarlo
                        nextSnore = downFor + 1.6f;
                    }
                }
                else if (downFor >= nextSnore) { Sfx.Play(this, "snore", 0.8f); nextSnore = downFor + 1.6f; }
                return;
            }

            if (!hinted && clock > hintAfter) { hinted = true; Say("I'll sleep when the SCREEN sleeps!", 2.6f); }

            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) || !began || PointerInput.IsOverUi()) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (switchArea != null && switchArea.OverlapPoint(w))
            {
                lightsOff = !lightsOff;
                Sfx.Play(this, "clunk", 0.6f);
                if (switchArt != null) switchArt.sprite = lightsOff ? switchOff : switchOn;
                if (darkness != null) darkness.enabled = lightsOff;
                Say(lightsOff ? "Still watching! Even better in the dark." : "Still watching!", 1.8f);
                return;
            }
            Sfx.Play(this, "tick", 0.6f);
            if (kidArea != null && kidArea.OverlapPoint(w)) StartCoroutine(Bounce());
            Say(Whines[whine++ % Whines.Length], 1.6f);
        }

        void TurnedUp()
        {
            if (asleep) { StartCoroutine(Sleeping()); return; }
            if (downFor < 0.25f) return;
            kid.sprite = awakeSprite;
            StartCoroutine(Bounce());
            Sfx.Play(this, "pop", 0.7f);
            Say(downFor > needSeconds * 0.5f ? "I'm awake! I'm AWAKE!" : "I'm awake!", 1.8f);
        }

        IEnumerator Bounce()
        {
            Transform t = kid.transform;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.12f); yield return null; }
            t.localScale = s;
        }

        IEnumerator Sleeping()
        {
            busy = true;
            if (bubble != null) bubble.Show("Zzz... (shh!)");
            if (darkness != null) darkness.enabled = true;
            Sfx.Play(this, "snore", 0.9f);
            for (int i = 0; i < zzz.Length; i++) StartCoroutine(Float(zzz[i], i * 0.45f));
            yield return new WaitForSeconds(2.4f);
            Win();
        }

        IEnumerator Float(Transform z, float delay)
        {
            yield return new WaitForSeconds(delay);
            Vector3 start = z.localPosition;
            z.gameObject.SetActive(true);
            while (true)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 1.4f)
                {
                    z.localPosition = start + new Vector3(Mathf.Sin(k * 6f) * 0.12f, k * 0.9f, 0f);
                    z.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.2f, k);
                    yield return null;
                }
            }
        }

        static AudioClip Tone(float hz)
        {
            int key = Mathf.RoundToInt(hz);
            if (tones.TryGetValue(key, out AudioClip c) && c != null) return c;
            const int rate = 22050;
            int n = (int)(0.45f * rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 5f);
                data[i] = (Mathf.Sin(t * 2f * Mathf.PI * hz) + Mathf.Sin(t * 4f * Mathf.PI * hz) * 0.25f) * env * 0.5f;
            }
            c = AudioClip.Create("lullaby_" + key, n, 1, rate, false);
            c.SetData(data, 0);
            tones[key] = c;
            return c;
        }
    }
}
