using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The gauntlet" — i trucchi classici del gioco in fila, con un'unica barra di 3 cuori:
    /// 1) la chiave è gigantesca: pizzicala per rimpicciolirla e trascinala nel lucchetto (troppo grande = cuore perso);
    /// 2) inclina il telefono per portare la biglia nella buca col traguardo, evitando le altre buche;
    /// 3) soffia nel microfono per spegnere 3 candeline (toccarle scotta: cuore perso);
    /// 4) premi VOLUME GIÙ per zittire la radio che urla (3 tacche; toccare la manopola la alza: cuore perso);
    /// 5) il grande bottone rosso dice "PRESS ME": non toccare niente per 5 secondi (toccarlo = cuore perso).
    /// Vittoria: fuochi d'artificio. Cuori finiti = sconfitta.
    /// </summary>
    public class GauntletLevel : LevelController
    {
        [SerializeField] GameObject[] stages;
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpriteRenderer veil;
        [SerializeField] TextMesh goal;
        [SerializeField] SpeechBubble bubble;
        [Header("1: chiave")]
        [SerializeField] Transform key;                 // punta della chiave
        [SerializeField] Transform keyArt;              // figlio scalato
        [SerializeField] Transform padlock;
        [SerializeField] Transform keyhole;
        [SerializeField] float keyStartScale = 2.6f, keyMin = 0.3f, keyMax = 0.65f;
        [Header("2: labirinto")]
        [SerializeField] Rigidbody2D ball;
        [SerializeField] Transform[] holes;             // l'ultima è il traguardo
        [SerializeField] TiltGravity tilt;
        [SerializeField] float holeRadius = 0.15f;
        [Header("3: candeline")]
        [SerializeField] Transform[] flames;
        [SerializeField] Collider2D cakeArea;
        [SerializeField] Transform[] smokes;
        [Header("4: radio")]
        [SerializeField] Transform radio;
        [SerializeField] Collider2D knobArea;
        [SerializeField] SpriteRenderer[] volumeBars;
        [SerializeField] AudioSource screamer;
        [Header("5: bottone")]
        [SerializeField] Transform bigButton;
        [SerializeField] TextMesh buttonText;
        [SerializeField] TextMesh countdown;
        [SerializeField] SpriteRenderer[] bursts;

        static readonly string[] Goals =
        {
            "Unlock the padlock", "Tilt to roll the marble into the star hole", "Blow out the candles",
            "Make the radio stop screaming!", "Don't touch anything for 5 seconds",
        };
        static readonly string[] Tempt = { "PRESS ME", "PLEASE?", "JUST ONCE!", "I'M SO\nLONELY", "PRESS ME!!" };

        Camera cam;
        int stage = -1, lives = 3;
        bool busy, stageDone, grabbingKey, pinching;
        float keyScale, pinchStart, scaleStart, bubbleUntil, clock, hurtCooldown;
        Vector2 keyOffset;
        Vector3 keyHome, ballStart;
        float[] blown;
        int bars, pressesStart;
        float lastVolume;
        float still;
        int temptIndex;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            E40Kit.Hearts(hearts, lives);
            foreach (GameObject s in stages) s.SetActive(false);
            foreach (SpriteRenderer b in bursts) b.gameObject.SetActive(false);
            if (tilt != null) tilt.enabled = false;
            keyHome = key.localPosition;
            ballStart = ball.transform.localPosition;
            blown = new float[flames.Length];
            StartCoroutine(Begin(0));
        }

        void OnDestroy()
        {
            Inputs.ReleaseMic(this);
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        // ---------- Passaggio tra le prove ----------

        IEnumerator Begin(int s)
        {
            busy = true;
            if (stage >= 0)
            {
                yield return E40Kit.Fade(veil, 1f, 0.25f);
                stages[stage].SetActive(false);
                EndStage(stage);
            }
            stage = s;
            stageDone = false;
            stages[s].SetActive(true);
            SetupStage(s);
            E40Kit.Dots(dots, s);
            E40Kit.Goal(goal, (s + 1) + "/5  " + Goals[s]);
            yield return E40Kit.Fade(veil, 0f, 0.3f);
            busy = false;
        }

        void SetupStage(int s)
        {
            switch (s)
            {
                case 0:
                    keyScale = keyStartScale;
                    keyArt.localScale = Vector3.one * keyScale;
                    key.localPosition = keyHome;
                    Say("That key is... ambitious.", 2f);
                    break;
                case 1:
                    ResetBall();
                    if (tilt != null) tilt.enabled = true;
                    break;
                case 2:
                    for (int i = 0; i < flames.Length; i++) { blown[i] = 0f; flames[i].gameObject.SetActive(true); }
                    foreach (Transform sm in smokes) sm.gameObject.SetActive(false);
                    Say("Make a wish!", 1.6f);
                    break;
                case 3:
                    bars = volumeBars.Length;
                    pressesStart = SensorHub.Instance != null ? SensorHub.Instance.VolumeDownPresses : 0;
                    lastVolume = SensorHub.Instance != null ? SensorHub.Instance.SystemVolume01 : 1f;
                    if (MusicPlayer.Instance != null) MusicPlayer.Instance.Duck(this, 0f);
                    if (screamer != null)
                    {
                        if (screamer.clip == null) screamer.clip = ScreamSynth.Create(1.6f);
                        screamer.loop = true;
                        screamer.Play();
                    }
                    DrawBars();
                    break;
                case 4:
                    still = -0.6f;   // un attimo di respiro prima che il conto parta
                    temptIndex = 0;
                    if (buttonText != null) buttonText.text = Tempt[0];
                    if (countdown != null) countdown.text = "5";
                    break;
            }
        }

        void EndStage(int s)
        {
            if (s == 1 && tilt != null) tilt.enabled = false;
            if (s == 2) Inputs.ReleaseMic(this);
            if (s == 3)
            {
                if (screamer != null) screamer.Stop();
                if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
            }
        }

        IEnumerator Passed(string text)
        {
            stageDone = true;
            busy = true;
            Sfx.Play(this, "ding", 0.8f);
            Say(text, 1.4f);
            yield return new WaitForSeconds(1.1f);
            if (stage < stages.Length - 1) yield return Begin(stage + 1);
            else yield return Finale();
        }

        /// <summary>Cuore perso; a zero si perde il livello. Restituisce false se è finita.</summary>
        bool Hurt(string text)
        {
            if (hurtCooldown > 0f || IsFinished) return true;
            hurtCooldown = 1.0f;
            lives--;
            E40Kit.Hearts(hearts, lives);
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.7f);
            Say(text, 1.8f);
            if (lives <= 0) { StartCoroutine(Lose()); return false; }
            return true;
        }

        IEnumerator Lose()
        {
            busy = true;
            if (stage >= 0) EndStage(stage);
            yield return new WaitForSeconds(1.2f);
            Fail();
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            hurtCooldown -= dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy || stageDone || stage < 0) return;
            switch (stage)
            {
                case 0: KeyStage(); break;
                case 1: MazeStage(); break;
                case 2: CandleStage(dt); break;
                case 3: RadioStage(); break;
                case 4: ButtonStage(dt); break;
            }
        }

        // ---------- 1: chiave gigante ----------

        void KeyStage()
        {
            if (PointerInput.TryGetPinch(out float d))
            {
                if (!pinching) { pinching = true; pinchStart = Mathf.Max(d, 1f); scaleStart = keyScale; grabbingKey = false; }
                else SetKey(scaleStart * d / pinchStart);
                return;
            }
            pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) SetKey(keyScale * (1f + wheel * 0.08f));
#endif
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (grabbingKey) DropKey(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return;
                Vector2 tip = key.position, bow = tip + Vector2.left * 1.25f * keyScale;
                Vector2 ab = bow - tip;
                float t = Mathf.Clamp01(Vector2.Dot(w - tip, ab) / ab.sqrMagnitude);
                if (Vector2.Distance(w, tip + ab * t) < Mathf.Max(0.55f, 0.4f * keyScale)) { grabbingKey = true; keyOffset = tip - w; }
                return;
            }
            if (!grabbingKey) return;
            Vector2 p = w + keyOffset;
            key.position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.8f), Mathf.Clamp(p.y, -4.2f, 2.4f), 0f);
            if (ended) DropKey();
        }

        void SetKey(float s)
        {
            keyScale = Mathf.Clamp(s, 0.15f, 3f);
            keyArt.localScale = Vector3.one * keyScale;
        }

        void DropKey()
        {
            grabbingKey = false;
            if (Vector2.Distance(key.position, keyhole.position) > 0.45f) return;
            if (keyScale >= keyMin && keyScale <= keyMax) { StartCoroutine(Unlock()); return; }
            if (Hurt(keyScale > keyMax ? "Way too big for that lock!" : "Too tiny, it falls out!")) StartCoroutine(KeyBack());
        }

        IEnumerator KeyBack()
        {
            busy = true;
            StartCoroutine(E40Kit.Shake(padlock, 0.1f));
            Vector3 from = key.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { key.localPosition = Vector3.Lerp(from, keyHome, k); yield return null; }
            key.localPosition = keyHome;
            if (lives > 0) busy = false;
        }

        IEnumerator Unlock()
        {
            busy = true;
            key.position = keyhole.position;
            Sfx.Play(this, "clunk", 0.9f);
            yield return new WaitForSeconds(0.2f);
            Vector3 p = padlock.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { padlock.localPosition = p + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.25f; yield return null; }
            padlock.localPosition = p;
            busy = false;
            yield return Passed("Click! Open!");
        }

        // ---------- 2: labirinto ----------

        void ResetBall()
        {
            ball.transform.localPosition = ballStart;
            ball.transform.localScale = Vector3.one;
#if UNITY_6000_0_OR_NEWER
            ball.linearVelocity = Vector2.zero;
#else
            ball.SetVelocity(Vector2.zero);
#endif
            ball.position = ball.transform.position;
            ball.simulated = true;
        }

        void MazeStage()
        {
            for (int i = 0; i < holes.Length; i++)
            {
                if (Vector2.Distance(ball.position, holes[i].position) > holeRadius) continue;
                StartCoroutine(BallFalls(i, i == holes.Length - 1));
                return;
            }
        }

        IEnumerator BallFalls(int hole, bool goalHole)
        {
            busy = true;
            ball.simulated = false;
            Vector3 from = ball.transform.position, to = holes[hole].position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                ball.transform.position = Vector3.Lerp(from, to, k);
                ball.transform.localScale = Vector3.one * (1f - k * 0.7f);
                yield return null;
            }
            Haptics.Light();
            Sfx.Play(this, "clunk", 0.7f);
            if (goalHole)
            {
                busy = false;
                yield return Passed("Hole in one!");
                yield break;
            }
            if (!Hurt("Plop! Wrong hole.")) yield break;
            yield return new WaitForSeconds(0.5f);
            ResetBall();
            busy = false;
        }

        // ---------- 3: candeline ----------

        void CandleStage(float dt)
        {
            float mic = Inputs.Mic(this);
            bool blowing = mic > 0.3f;
            int lit = 0;
            for (int i = 0; i < flames.Length; i++)
            {
                if (!flames[i].gameObject.activeSelf) continue;
                lit++;
                float lean = blowing ? -25f - mic * 20f : Mathf.Sin(clock * 9f + i) * 5f;
                flames[i].localRotation = Quaternion.Euler(0, 0, lean);
                flames[i].localScale = new Vector3(1f, blowing ? 0.7f : 1f + Mathf.Sin(clock * 13f + i) * 0.08f, 1f);
            }
            if (blowing)
            {
                // si spegne la prima candelina ancora accesa
                for (int i = 0; i < flames.Length; i++)
                {
                    if (!flames[i].gameObject.activeSelf) continue;
                    blown[i] += dt;
                    if (blown[i] > 0.45f)
                    {
                        flames[i].gameObject.SetActive(false);
                        if (i < smokes.Length) StartCoroutine(Smoke(smokes[i]));
                        Sfx.Play(this, "whoosh", 0.5f);
                        lit--;
                    }
                    break;
                }
            }
            if (lit == 0) { StartCoroutine(Passed("Wish granted!")); return; }

            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool _) && began && !PointerInput.IsOverUi())
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (cakeArea != null && cakeArea.OverlapPoint(w)) Hurt("OUCH! Hot! Use your breath.");
            }
        }

        IEnumerator Smoke(Transform s)
        {
            s.gameObject.SetActive(true);
            Vector3 p = s.localPosition;
            SpriteRenderer sr = s.GetComponentInChildren<SpriteRenderer>();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.0f)
            {
                s.localPosition = p + new Vector3(Mathf.Sin(k * 8f) * 0.06f, k * 0.6f, 0f);
                if (sr != null) sr.color = new Color(0.8f, 0.8f, 0.85f, 0.8f * (1f - k));
                yield return null;
            }
            s.localPosition = p;
            s.gameObject.SetActive(false);
        }

        // ---------- 4: radio ----------

        void RadioStage()
        {
            SensorHub hub = SensorHub.Instance;
            if (hub != null)
            {
                int presses = hub.VolumeDownPresses - pressesStart;
                float v = hub.SystemVolume01;
                bool dropped = v < lastVolume - 0.001f;
                lastVolume = v;
                if (presses > 0 || dropped)
                {
                    pressesStart = hub.VolumeDownPresses;
                    bars = Mathf.Max(0, bars - Mathf.Max(1, presses));
                    Sfx.Play(this, "tick", 0.8f);
                    DrawBars();
                    StartCoroutine(E40Kit.Pop(radio, 0.05f, 0.15f));
                }
            }
            if (radio != null) radio.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 50f) * 2f * bars / Mathf.Max(1, volumeBars.Length));
            if (bars <= 0) { StartCoroutine(Passed("Ahhh... silence.")); return; }

            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool _) && began && !PointerInput.IsOverUi())
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (knobArea != null && knobArea.OverlapPoint(w))
                {
                    bars = volumeBars.Length;
                    DrawBars();
                    Hurt("LOUDER?! That knob turns it UP!");
                }
                else Say("Not with your finger... the phone's buttons!", 1.6f);
            }
        }

        void DrawBars()
        {
            for (int i = 0; i < volumeBars.Length; i++)
                volumeBars[i].color = i < bars ? Color.Lerp(new Color(0.4f, 0.9f, 0.4f), new Color(1f, 0.3f, 0.3f), i / (float)Mathf.Max(1, volumeBars.Length - 1)) : new Color(0.2f, 0.2f, 0.2f, 0.4f);
            if (screamer != null) screamer.volume = bars / (float)Mathf.Max(1, volumeBars.Length) * 0.7f;
        }

        // ---------- 5: non premere ----------

        void ButtonStage(float dt)
        {
            if (PointerInput.AnyTouchBegan())
            {
                StartCoroutine(Pressed());
                return;
            }
            still += dt;
            int left = Mathf.CeilToInt(5f - Mathf.Max(0f, still));
            if (countdown != null) countdown.text = left.ToString();
            int t = Mathf.Clamp(Mathf.FloorToInt(Mathf.Max(0f, still) / 1.1f), 0, Tempt.Length - 1);
            if (t != temptIndex) { temptIndex = t; if (buttonText != null) buttonText.text = Tempt[t]; StartCoroutine(E40Kit.Pop(bigButton, 0.12f, 0.3f)); Sfx.Play(this, "pop", 0.4f); }
            bigButton.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 6f) * (1f + temptIndex));
            if (still >= 5f) StartCoroutine(Passed("You resisted!"));
        }

        IEnumerator Pressed()
        {
            busy = true;
            Vector3 s = bigButton.localScale;
            bigButton.localScale = new Vector3(s.x * 1.05f, s.y * 0.8f, s.z);
            Sfx.Play(this, "clunk", 0.9f);
            bool alive = Hurt("GOTCHA! You pressed it!");
            yield return new WaitForSeconds(0.4f);
            bigButton.localScale = s;
            if (!alive) yield break;
            still = -0.6f;
            temptIndex = 0;
            if (buttonText != null) buttonText.text = Tempt[0];
            if (countdown != null) countdown.text = "5";
            busy = false;
        }

        IEnumerator Finale()
        {
            busy = true;
            E40Kit.Dots(dots, stages.Length);
            E40Kit.Goal(goal, "GAUNTLET CLEARED!");
            if (buttonText != null) buttonText.text = "YOU WIN";
            Say("Champion!", 3f);
            yield return E40Kit.Fireworks(this, bursts, 3f);
            yield return new WaitForSeconds(0.8f);
            Win();
        }
    }
}
