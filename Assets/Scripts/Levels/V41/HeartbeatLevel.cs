using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Heartbeat" — un mostriciattolo dorme attaccato al monitor del battito, ma lo schermo mostra una linea
    /// piatta. Il battito c'è: il telefono vibra uno schema corto-corto-lungo (con l'altoparlante attivo si sente anche).
    /// Soluzione: riprodurlo premendo il cuore: due tocchi brevi e uno lungo. Tre ritmi sbagliati = sconfitta.
    /// </summary>
    public class HeartbeatLevel : LevelController
    {
        [SerializeField] Transform heart, soundButton, monster;
        [SerializeField] Collider2D heartArea, soundArea;
        [SerializeField] SpriteRenderer monsterArt, soundIcon;
        [SerializeField] Sprite awakeSprite, soundOnSprite, soundOffSprite;
        [SerializeField] LineRenderer trace;
        [SerializeField] TextMesh wrongText, monitorText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float traceLeft = -1.85f, traceRight = 1.85f, traceY = 1.6f, traceHeight = 0.5f;
        [SerializeField] float longPress = 0.36f, sequenceTimeout = 1.8f;

        const int Points = 90;
        static readonly bool[] pattern = { false, false, true };   // corto, corto, lungo

        Camera cam;
        float[] wave = new float[Points];
        bool soundOn, busy, pressing, alive;
        float pressStart, lastRelease, scroll;
        int wrong;
        readonly bool[] got = new bool[3];
        int count;
        Coroutine player;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            soundOn = !Haptics.Enabled || Application.isEditor;
            trace.positionCount = Points;
            trace.useWorldSpace = false;
            DrawTrace();
            S41EKit.Confetti(confetti, false);
            UpdateSound();
            UpdateWrong();
            monitorText.text = "NO SIGNAL";
            S41EKit.Say(bubble, "Zzz... the screen can't see my heartbeat... can you FEEL it?");
            player = StartCoroutine(PlayPattern(1.5f));
        }

        void UpdateSound() { soundIcon.sprite = soundOn ? soundOnSprite : soundOffSprite; }
        void UpdateWrong() { if (wrongText != null) wrongText.text = "WRONG RHYTHMS " + wrong + "/3"; }

        void DrawTrace()
        {
            for (int i = 0; i < Points; i++)
            {
                float x = Mathf.Lerp(traceLeft, traceRight, i / (Points - 1f));
                trace.SetPosition(i, new Vector3(x, traceY + wave[i] * traceHeight, 0f));
            }
        }

        /// <summary>Lo schema: vibrazione (e suono se attivo). Lo schermo resta piatto.</summary>
        IEnumerator PlayPattern(float delay)
        {
            yield return new WaitForSeconds(delay);
            while (true)
            {
                foreach (bool isLong in pattern)
                {
                    float len = isLong ? 0.6f : 0.14f;
                    Haptics.Vibrate((long)(len * 1000f));
                    if (soundOn) S41EKit.Tone(this, 70f, len, "thump", 0.9f);
                    yield return new WaitForSeconds(len + 0.4f);
                }
                yield return new WaitForSeconds(3.0f);
            }
        }

        void StopPattern() { if (player != null) { StopCoroutine(player); player = null; } }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            // scorrimento della traccia del monitor
            scroll += dt * 40f;
            while (scroll >= 1f)
            {
                scroll -= 1f;
                for (int i = 0; i < Points - 1; i++) wave[i] = wave[i + 1];
                float v = 0f;
                if (pressing) v = Mathf.Sin(Time.time * 30f) * 0.8f;
                if (alive) { float ph = Mathf.Repeat(Time.time, 0.8f); v = ph < 0.05f ? 1f : ph < 0.1f ? -0.6f : 0f; }
                wave[Points - 1] = v;
            }
            DrawTrace();
            if (busy) return;

            bool held = PointerInput.IsHeld(cam, heartArea);
            if (PointerInput.AnyTouchBegan() && S41EKit.Pointer(cam, out Vector2 w, out _, out _))
            {
                if (soundArea.OverlapPoint(w))
                {
                    soundOn = !soundOn;
                    UpdateSound();
                    Sfx.Play(this, "tick", 0.6f);
                    StartCoroutine(S41EKit.Pop(soundButton, 1.2f, 0.15f));
                    return;
                }
                if (heartArea.OverlapPoint(w) && !pressing)
                {
                    StopPattern();
                    pressing = true;
                    pressStart = Time.time;
                    Haptics.Light();
                    if (soundOn) S41EKit.Tone(this, 90f, 0.1f, "thump", 0.6f);
                }
            }
            heart.localScale = Vector3.one * (pressing ? 1.15f : 1f);
            if (pressing && !held)
            {
                pressing = false;
                lastRelease = Time.time;
                if (count < 3) got[count] = Time.time - pressStart >= longPress;
                count++;
                monitorText.text = "BEAT " + Mathf.Min(count, 3);
                if (count >= 3) { StartCoroutine(Check()); return; }
            }
            if (!pressing && count > 0 && Time.time - lastRelease > sequenceTimeout) StartCoroutine(Check());
        }

        IEnumerator Check()
        {
            busy = true;
            bool ok = count == 3;
            for (int i = 0; i < 3 && ok; i++) if (got[i] != pattern[i]) ok = false;
            count = 0;
            yield return new WaitForSeconds(0.3f);
            if (ok) { yield return Alive(); yield break; }
            wrong++;
            UpdateWrong();
            monitorText.text = "WRONG RHYTHM";
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Medium();
            if (wrong >= 3)
            {
                S41EKit.Say(bubble, "Zzzzz... (he needs a real doctor)");
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            S41EKit.Say(bubble, wrong == 1 ? "Mmh... that's not my rhythm." : "Feel it again... short, short...?");
            yield return new WaitForSeconds(1.0f);
            monitorText.text = "NO SIGNAL";
            busy = false;
            player = StartCoroutine(PlayPattern(0.8f));
        }

        IEnumerator Alive()
        {
            alive = true;
            monitorText.text = "BEEP! BEEP!";
            if (awakeSprite != null) monsterArt.sprite = awakeSprite;
            S41EKit.Say(bubble, "BA-DUM BA-DUM! I'M ALIVE!");
            for (int i = 0; i < 4; i++)
            {
                S41EKit.Tone(this, 880f, 0.12f, "beep", 0.6f);
                StartCoroutine(S41EKit.Pop(heart, 1.2f, 0.2f));
                yield return S41EKit.Move(monster, monster.localPosition, 0.4f, 0.35f);
            }
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.6f);
            S41EKit.Confetti(confetti, false);
            Win();
        }
    }
}
