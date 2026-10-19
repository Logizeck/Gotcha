using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Phone sandwich" — nella cucina di una stazione spaziale gli ingredienti galleggiano senza peso.
    /// Soluzione: inclinare piano il telefono (la "gravità" è quanto lo inclini rispetto a come lo tenevi all'inizio)
    /// per far scivolare formaggio, insalata e pomodoro sulla fetta di pane; poi capovolgere il telefono: il panino
    /// "cade" sulla fetta di sopra attaccata al soffitto. Tre ingredienti persi fuori dallo schermo → Fail().
    /// Senza accelerometro: si trascinano gli ingredienti e c'è il pulsante FLIP. (Editor: frecce; freccia su = capovolto.)
    /// </summary>
    public class PhoneSandwichLevel : LevelController
    {
        [SerializeField] Transform[] items;              // formaggio, insalata, pomodoro
        [SerializeField] float[] halfThick = { 0.08f, 0.11f, 0.08f };
        [SerializeField] Transform breadBottom, topSlice, mouse;
        [SerializeField] Transform flipHint;             // icona "gira il telefono"
        [SerializeField] Collider2D flipButton;          // solo senza sensore
        [SerializeField] TextMesh goal, dropsLabel;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float gain = 9f, deadZone = 0.05f, stickSpeed = 4.5f, breadHalf = 0.16f, catchHalfWidth = 0.95f;

        static readonly Vector2[] Spawns = { new Vector2(-1.6f, 2.2f), new Vector2(0.3f, 1.2f), new Vector2(1.7f, 2.6f), new Vector2(-0.9f, 0.4f), new Vector2(1.2f, 0.2f) };

        Camera cam;
        Vector2[] pos, vel;
        bool[] stacked;
        int stackCount, drops, spawnIndex, dragged = -1;
        float stackTop, clock, bubbleUntil, upsideFor, calibrate = 0.25f;
        Vector2 rest, dragOffset;
        bool touchMode, phase2, busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            touchMode = !SystemInfo.supportsAccelerometer;
            pos = new Vector2[items.Length];
            vel = new Vector2[items.Length];
            stacked = new bool[items.Length];
            for (int i = 0; i < items.Length; i++) { pos[i] = items[i].position; vel[i] = Random.insideUnitCircle * 0.15f; }
            stackTop = breadBottom.position.y + breadHalf;
            topSlice.gameObject.SetActive(false);
            if (flipHint != null) flipHint.gameObject.SetActive(false);
            if (flipButton != null) flipButton.gameObject.SetActive(false);
            UpdateDrops();
            Goal(touchMode ? "DRAG THE FILLINGS ONTO THE BREAD" : "TILT TO MAKE A SANDWICH");
            Say("I'm STARVING! Make me a sandwich!", 2.4f);
        }

        void Goal(string s) { if (goal != null) goal.text = s; }
        void UpdateDrops() { if (dropsLabel != null) dropsLabel.text = "DROPPED: " + drops + "/3"; }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;

            Vector2 tilt = Inputs.Tilt();
            if (calibrate > 0f) { calibrate -= dt; rest = tilt; return; }

            if (phase2) { Phase2(dt, tilt); return; }

            // gravità relativa (zona morta: a telefono fermo tutto galleggia)
            Vector2 g = Vector2.zero;
            if (!touchMode)
            {
                Vector2 d = tilt - rest;
                if (d.magnitude > deadZone) g = (d - d.normalized * deadZone) * gain;
                rest = Vector2.Lerp(rest, tilt, dt * 0.05f);   // ricalibrazione lentissima
            }
            else DragItems();

            for (int i = 0; i < items.Length; i++)
            {
                if (stacked[i] || i == dragged) continue;
                vel[i] += g * dt + new Vector2(Mathf.Sin(clock * 0.9f + i * 2f), Mathf.Cos(clock * 0.7f + i)) * 0.05f * dt;
                vel[i] *= 1f - 0.5f * dt;
                vel[i] = Vector2.ClampMagnitude(vel[i], 6f);
                Vector2 prev = pos[i];
                pos[i] += vel[i] * dt;
                // pareti: lati e soffitto rimbalzano, sotto si perde
                if (pos[i].x < -2.5f) { pos[i].x = -2.5f; vel[i].x = Mathf.Abs(vel[i].x) * 0.5f; }
                if (pos[i].x > 2.5f) { pos[i].x = 2.5f; vel[i].x = -Mathf.Abs(vel[i].x) * 0.5f; }
                if (pos[i].y > 3.3f) { pos[i].y = 3.3f; vel[i].y = -Mathf.Abs(vel[i].y) * 0.5f; }
                TryLand(i, prev);
                if (pos[i].y < -5.4f) { Lost(i); if (IsFinished || busy) return; }
                Place(i);
            }
            if (stackCount >= items.Length) StartPhase2();
        }

        void Place(int i)
        {
            items[i].position = new Vector3(pos[i].x, pos[i].y, 0f);
            if (!stacked[i]) items[i].localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 0.8f + i) * 8f);
        }

        void TryLand(int i, Vector2 prev)
        {
            float bottom = pos[i].y - halfThick[i], prevBottom = prev.y - halfThick[i];
            if (vel[i].y > 0f || Mathf.Abs(pos[i].x - breadBottom.position.x) > catchHalfWidth) return;
            if (!(prevBottom >= stackTop - 0.02f && bottom <= stackTop)) return;
            if (vel[i].magnitude > stickSpeed)
            {
                vel[i].y = Mathf.Abs(vel[i].y) * 0.5f;
                Sfx.Play(this, "bonk", 0.5f);
                Say("Too fast! Gently!", 1.2f);
                return;
            }
            Stack(i);
        }

        void Stack(int i)
        {
            stacked[i] = true;
            stackCount++;
            pos[i] = new Vector2(Mathf.Lerp(pos[i].x, breadBottom.position.x, 0.6f), stackTop + halfThick[i]);
            stackTop += halfThick[i] * 2f;
            vel[i] = Vector2.zero;
            items[i].localRotation = Quaternion.identity;
            items[i].position = pos[i];
            items[i].SetParent(breadBottom, true);
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            StartCoroutine(S41AKit.Bounce(breadBottom, 0.08f, 0.2f));
            if (stackCount < items.Length) Say(stackCount == 1 ? "Yes! Keep going!" : "One more!", 1.2f);
        }

        void Lost(int i)
        {
            drops++;
            UpdateDrops();
            Sfx.Play(this, "whoosh", 0.7f);
            Haptics.Medium();
            if (drops >= 3) { StartCoroutine(Starving()); return; }
            Say("Noo! It floated away! (here's another)", 1.6f);
            Vector2 s = Spawns[spawnIndex++ % Spawns.Length];
            pos[i] = s;
            vel[i] = Vector2.zero;
            Place(i);
            StartCoroutine(S41AKit.Bounce(items[i], 0.4f, 0.3f));
        }

        void DragItems()
        {
            if (!S41AKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { dragged = -1; return; }
            if (began)
            {
                dragged = -1;
                for (int i = 0; i < items.Length; i++)
                    if (!stacked[i] && Mathf.Abs(w.x - pos[i].x) < 0.95f && Mathf.Abs(w.y - pos[i].y) < 0.4f) { dragged = i; dragOffset = pos[i] - w; break; }
            }
            if (dragged < 0) return;
            int k = dragged;
            Vector2 prev = pos[k];
            pos[k] = w + dragOffset;
            pos[k].x = Mathf.Clamp(pos[k].x, -2.5f, 2.5f);
            pos[k].y = Mathf.Clamp(pos[k].y, -4.6f, 3.3f);
            vel[k] = Vector2.zero;
            Place(k);
            if (ended)
            {
                dragged = -1;
                // lasciato appena sopra il panino: si posa
                if (Mathf.Abs(pos[k].x - breadBottom.position.x) < catchHalfWidth && pos[k].y - halfThick[k] > stackTop - 0.1f && pos[k].y - halfThick[k] < stackTop + 0.9f) Stack(k);
                else vel[k] = (pos[k] - prev) / Mathf.Max(Time.deltaTime, 0.01f) * 0.2f;
            }
        }

        void StartPhase2()
        {
            phase2 = true;
            topSlice.gameObject.SetActive(true);
            StartCoroutine(S41AKit.Bounce(topSlice, 0.3f, 0.3f));
            Sfx.Play(this, "ding", 0.6f);
            Say("The top slice is stuck on the ceiling!", 2.2f);
            Goal(touchMode ? "NOW PRESS FLIP!" : "NOW FLIP THE PHONE!");
            if (flipHint != null) flipHint.gameObject.SetActive(!touchMode);
            if (flipButton != null) flipButton.gameObject.SetActive(touchMode);
        }

        void Phase2(float dt, Vector2 tilt)
        {
            topSlice.localRotation = Quaternion.Euler(0f, 0f, 180f + Mathf.Sin(clock * 6f) * 3f);
            if (flipHint != null) flipHint.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 2f) * 25f);
            bool flip = false;
            if (touchMode)
                flip = flipButton != null && S41AKit.Pointer(cam, out Vector2 w, out bool began, out _) && began && flipButton.OverlapPoint(w);
            else
            {
                upsideFor = tilt.y > 0.7f ? upsideFor + dt : 0f;
                flip = upsideFor > 0.35f;
            }
            if (flip) StartCoroutine(Assemble());
        }

        IEnumerator Assemble()
        {
            busy = true;
            if (flipHint != null) flipHint.gameObject.SetActive(false);
            if (flipButton != null) flipButton.gameObject.SetActive(false);
            Goal("");
            // capovolto, il panino "cade" verso il soffitto e si chiude sulla fetta di sopra
            Sfx.Play(this, "whoosh", 0.9f);
            breadBottom.SetParent(transform, true);
            float sliceHalf = 0.13f;
            Vector3 to = new Vector3(topSlice.position.x, topSlice.position.y - sliceHalf - (stackTop - breadBottom.position.y), 0f);
            Vector3 from = breadBottom.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                breadBottom.position = Vector3.Lerp(from, to, k * k);
                yield return null;
            }
            breadBottom.position = to;
            topSlice.localRotation = Quaternion.identity;
            Sfx.Play(this, "clunk", 1f);
            Haptics.Medium();
            yield return S41AKit.Bounce(breadBottom, 0.12f, 0.2f);
            // il panino completo torna giù al centro e il topolino lo addenta
            topSlice.SetParent(breadBottom, true);
            yield return S41AKit.Move(breadBottom, new Vector3(0f, 0.3f, 0f), 0.8f);
            Say("PERFECT SANDWICH!", 1.4f);
            yield return S41AKit.Move(mouse, new Vector3(1.25f, 0.35f, 0f), 0.6f, 0.2f);
            for (int i = 0; i < 3; i++)
            {
                Sfx.Play(this, "pop", 0.8f);
                yield return S41AKit.Bounce(breadBottom, -0.1f, 0.18f);
            }
            Sfx.Play(this, "ding", 0.8f);
            Say("Mmm! Delicious!", 1.2f);
            yield return new WaitForSeconds(1f);
            Win();
        }

        IEnumerator Starving()
        {
            busy = true;
            Say("Three in a row?! I'll starve in space!", 2f);
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Heavy();
            yield return S41AKit.Bounce(mouse, 0.25f, 0.4f);
            yield return new WaitForSeconds(1.4f);
            Fail();
        }
    }
}
